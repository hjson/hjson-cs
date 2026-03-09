using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Hjson;

using JsonPair = KeyValuePair<string, JsonValue>;

internal class HjsonReader : BaseReader
{
    readonly StringBuilder sb = new();
    readonly IEnumerable<IHjsonDsfProvider> dsfProviders = [];
    readonly bool hasDsfProviders;

    public HjsonReader(string input, IJsonReader jsonReader, HjsonOptions options)
      : base(input, jsonReader)
    {
        if (options != null)
        {
            ReadWsc = options.KeepWsc;
            dsfProviders = options.DsfProviders;
        }
        hasDsfProviders = dsfProviders.Any();
    }

    public HjsonReader(TextReader reader, IJsonReader jsonReader, HjsonOptions options)
      : base(reader, jsonReader)
    {
        if (options != null)
        {
            ReadWsc = options.KeepWsc;
            dsfProviders = options.DsfProviders;
        }
        hasDsfProviders = dsfProviders.Any();
    }

    public JsonValue Read()
    {
        // Braces for the root object are optional

        int c = SkipPeekChar();
        switch (c)
        {
            case '[':
            case '{':
                return checkTrailing(ReadCore());
            default:
                if (RemainingContains(':'))
                {
                    try
                    {
                        // assume we have a root object without braces
                        return checkTrailing(ReadCore(true));
                    }
                    catch (Exception)
                    {
                        // test if we are dealing with a single JSON value instead (true/false/null/num/"")
                        Reset();
                        try { return checkTrailing(ReadCore()); }
                        catch (Exception) { }
                        throw; // throw original error
                    }
                }
                else
                {
                    return checkTrailing(ReadCore());
                }
        }
    }

    JsonValue checkTrailing(JsonValue v)
    {
        skipWhite2();
        if (ReadChar() >= 0) throw ParseError("Extra characters in input");
        return v;
    }

    void skipWhite2()
    {
        while (PeekChar() >= 0)
        {
            while (IsWhite((char)PeekChar())) ReadChar();
            int p = PeekChar();
            if (p == '#' || p == '/' && PeekChar(1) == '/')
            {
                for (; ; )
                {
                    var ch = PeekChar();
                    if (ch < 0 || ch == '\n') break;
                    ReadChar();
                }
            }
            else if (p == '/' && PeekChar(1) == '*')
            {
                ReadChar(); ReadChar();
                for (; ; )
                {
                    var ch = PeekChar();
                    if (ch < 0 || ch == '*' && PeekChar(1) == '/') break;
                    ReadChar();
                }
                if (PeekChar() >= 0) { ReadChar(); ReadChar(); }
            }
            else break;
        }
    }

    protected override string GetWhite()
    {
        var res = base.GetWhite();
        int to = res.Length - 1;
        if (to >= 0)
        {
            // remove trailing whitespace
            for (; to > 0 && res[to] <= ' ' && res[to] != '\n'; to--) ;
            // but only up to EOL
            if (res[to] == '\n') to--;
            if (to >= 0 && res[to] == '\r') to--;
            res = res.Substring(0, to + 1);
            foreach (char c in res) if (c > ' ') return res;
        }
        return "";
    }

    public override int SkipPeekChar()
    {
        skipWhite2();
        return PeekChar();
    }

    JsonValue ReadCore(bool objectWithoutBraces = false)
    {
        int c = objectWithoutBraces ? '{' : SkipPeekChar();
        if (c < 0) throw ParseError("Incomplete input");
        switch (c)
        {
            case '[':
                JsonArray list;
                WscJsonArray wscL = null;
                ReadChar();
                ResetWhite();
                if (ReadWsc) list = wscL = new WscJsonArray();
                else list = new JsonArray();
                SkipPeekChar();
                if (ReadWsc) wscL.Comments.Add(GetWhite());
                for (int i = 0; ; i++)
                {
                    if (SkipPeekChar() == ']') { ReadChar(); break; }
                    if (HasReader) Reader.Index(i);
                    var value = ReadCore();
                    if (HasReader) Reader.Value(value);
                    list.Add(value);
                    ResetWhite();
                    if (SkipPeekChar() == ',') { ReadChar(); ResetWhite(); SkipPeekChar(); }
                    if (ReadWsc) wscL.Comments.Add(GetWhite());
                }
                return list;
            case '{':
                JsonObject obj;
                WscJsonObject wsc = null;
                if (!objectWithoutBraces)
                {
                    ReadChar();
                    ResetWhite();
                }
                if (ReadWsc) obj = wsc = new WscJsonObject() { RootBraces = !objectWithoutBraces };
                else obj = new JsonObject();
                SkipPeekChar();
                if (ReadWsc) wsc.Comments[""] = GetWhite();
                for (; ; )
                {
                    if (objectWithoutBraces) { if (SkipPeekChar() < 0) break; }
                    else if (SkipPeekChar() == '}') { ReadChar(); break; }
                    string name = readKeyName();
                    skipWhite2();
                    Expect(':');
                    skipWhite2();
                    if (HasReader) Reader.Key(name);
                    var value = ReadCore();
                    if (HasReader) Reader.Value(value);
                    obj.Add(new JsonPair(name, value));
                    ResetWhite();
                    if (SkipPeekChar() == ',') { ReadChar(); ResetWhite(); SkipPeekChar(); }
                    if (ReadWsc) { wsc.Comments[name] = GetWhite(); wsc.Order.Add(name); }
                }
                return obj;
            case '\'':
            case '"': return ReadStringLiteral(readMlString);
            default: return readTfnns(c);
        }
    }

    string readKeyName()
    {
        // quotes for keys are optional in Hjson
        // unless they include {}[],: or whitespace.

        int c = PeekChar();
        if (c == '"' || c == '\'') return ReadStringLiteral(null);

        sb.Length = 0;
        int space = -1;
        for (; ; )
        {
            c = PeekChar();
            if (c < 0) throw ParseError("Name is not closed");
            char ch = (char)c;
            if (ch == ':')
            {
                if (sb.Length == 0) throw ParseError("Found ':' but no key name (for an empty key name use quotes)");
                else if (space >= 0 && space != sb.Length) throw ParseError("Found whitespace in your key name (use quotes to include)");
                return sb.ToString();
            }
            else if (IsWhite(ch))
            {
                if (space < 0) space = sb.Length;
                ReadChar();
            }
            else if (HjsonValue.IsPunctuatorChar(ch))
                throw ParseError($"Found '{ch}' where a key name was expected (check your syntax or use quotes if the key name includes {{}}[],: or whitespace)");
            else
            {
                ReadChar();
                sb.Append(ch);
            }
        }
    }

    void skipIndent(int indent)
    {
        while (indent-- > 0)
        {
            char c = (char)PeekChar();
            if (IsWhite(c) && c != '\n') ReadChar();
            else break;
        }
    }

    string readMlString()
    {
        // Parse a multiline string value.
        int triple = 0;
        sb.Length = 0;

        // we are at '''
        var indent = Column - 3;

        // skip white/to (newline)
        for (; ; )
        {
            char c = (char)PeekChar();
            if (IsWhite(c) && c != '\n') ReadChar();
            else break;
        }
        if (PeekChar() == '\n') { ReadChar(); skipIndent(indent); }

        // When parsing for string values, we must look for " and \ characters.
        while (true)
        {
            int ch = PeekChar();
            if (ch < 0) throw ParseError("Bad multiline string");
            else if (ch == '\'')
            {
                triple++;
                ReadChar();
                if (triple == 3)
                {
                    if (sb.Length > 0 && sb[sb.Length - 1] == '\n') sb.Length--;
                    return sb.ToString();
                }
                else continue;
            }
            else
            {
                while (triple > 0)
                {
                    sb.Append('\'');
                    triple--;
                }
            }
            if (ch == '\n')
            {
                sb.Append('\n');
                ReadChar();
                skipIndent(indent);
            }
            else
            {
                if (ch != '\r') sb.Append((char)ch);
                ReadChar();
            }
        }
    }

    internal static bool TryParseNumericLiteral(string text, bool stopAtNext, out JsonValue value)
        => TryParseNumericLiteral(text.AsSpan(), stopAtNext, out value);

    internal static bool TryParseNumericLiteral(StringBuilder sb, bool stopAtNext, out JsonValue value)
    {
        int len = sb.Length;
        if (len == 0) { value = null; return false; }
        Span<char> buf = len <= 64 ? stackalloc char[len] : new char[len];
        for (int i = 0; i < len; i++) buf[i] = sb[i];
        return TryParseNumericLiteral(buf, stopAtNext, out value);
    }

    internal static bool TryParseNumericLiteral(ReadOnlySpan<char> text, bool stopAtNext, out JsonValue value)
    {
        int c, leadingZeros = 0, p = 0;
        bool testLeading = true;
        int len = text.Length;
        value = null;

        if (len == 0) return false;

        if (text[p] == '-')
        {
            p++;
            if (p >= len) return false;
        }

        for (; ; )
        {
            if (p >= len) break;
            c = text[p];
            if (c < '0' || c > '9') break;
            if (testLeading)
            {
                if (c == '0') leadingZeros++;
                else testLeading = false;
            }
            p++;
        }
        if (testLeading) leadingZeros--; // single 0 is allowed
        if (leadingZeros > 0) return false;

        bool hasFracOrExp = false;

        if (p < len && text[p] == '.')
        {
            hasFracOrExp = true;
            if (leadingZeros < 0) return false;
            int fdigits = 0;
            p++;
            if (p >= len) return false;
            for (; ; )
            {
                if (p >= len) break;
                c = text[p];
                if (c < '0' || '9' < c) break;
                p++;
                fdigits++;
            }
            if (fdigits == 0) return false;
        }

        if (p < len)
        {
            c = text[p];
            if (c == 'e' || c == 'E')
            {
                hasFracOrExp = true;
                p++;
                if (p >= len) return false;

                c = text[p];
                if (c == '-') p++;
                else if (c == '+') p++;

                if (p >= len) return false;

                for (; ; )
                {
                    if (p >= len) break;
                    c = text[p];
                    if (c < '0' || c > '9') break;
                    p++;
                }
            }
        }

        int numEnd = p;

        while (p < len && IsWhite(text[p])) p++;

        bool foundStop = false;
        if (p < len && stopAtNext)
        {
            // end scan if we find a control character like ,}] or a comment
            char ch = text[p];
            if (ch == ',' || ch == '}' || ch == ']' || ch == '#' || ch == '/' && (len > p + 1 && (text[p + 1] == '/' || text[p + 1] == '*')))
                foundStop = true;
        }

        if (p != len && !foundStop) return false;

        var numSpan = text[..numEnd];

        if (!hasFracOrExp && long.TryParse(numSpan, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out long lval))
        {
            // Preserve negative zero as double (-0 as long is just 0)
            if (lval == 0 && numSpan[0] == '-') { value = -0.0; return true; }
            value = lval;
            return true;
        }

        double val = double.Parse(numSpan, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
        value = val;
        return true;
    }

    JsonValue readTfnns(int c)
    {
        if (HjsonValue.IsPunctuatorChar((char)c))
            throw ParseError($"Found a punctuator character '{c}' when expecting a quoteless string (check your syntax)");

        sb.Length = 0;
        for (; ; )
        {
            bool isEol = c < 0 || c == '\n';
            if (isEol || c == ',' ||
              c == '}' || c == ']' ||
              c == '#' ||
              c == '/' && (PeekChar(1) == '/' || PeekChar(1) == '*'))
            {
                if (sb.Length > 0)
                {
                    char ch = sb[0];
                    switch (ch)
                    {
                        case 'f':
                            if (SbTrimEquals(sb, "false")) return false;
                            break;
                        case 'n':
                            if (SbTrimEquals(sb, "null")) return null;
                            break;
                        case 't':
                            if (SbTrimEquals(sb, "true")) return true;
                            break;
                        default:
                            if (ch is '-' || (ch is >= '0' and <= '9'))
                            {
                                if (TryParseNumericLiteral(sb, false, out var res)) return res;
                            }
                            break;
                    }
                }
                if (isEol)
                {
                    // remove any whitespace at the end (ignored in quoteless strings)
                    var str = SbTrimEnd(sb);
                    if (!hasDsfProviders) return str;
                    return HjsonDsf.Parse(dsfProviders, str);
                }
            }
            ReadChar();
            if (c != '\r') sb.Append((char)c);
            c = PeekChar();
        }
    }

    /// <summary>Returns the content of the StringBuilder with trailing whitespace removed, without allocating extra.</summary>
    static string SbTrimEnd(StringBuilder sb)
    {
        int end = sb.Length - 1;
        while (end >= 0 && sb[end] <= ' ') end--;
        if (end < 0) return "";
        if (end == sb.Length - 1) return sb.ToString();
        return sb.ToString(0, end + 1);
    }

    /// <summary>Checks if the trimmed content of a StringBuilder equals a target string, without allocating.</summary>
    static bool SbTrimEquals(StringBuilder sb, string target)
    {
        int start = 0, end = sb.Length - 1;
        while (start <= end && sb[start] <= ' ') start++;
        while (end >= start && sb[end] <= ' ') end--;
        int len = end - start + 1;
        if (len != target.Length) return false;
        for (int i = 0; i < len; i++)
            if (sb[start + i] != target[i]) return false;
        return true;
    }
}