using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Hjson;

internal abstract class BaseReader
{
    readonly string buffer;
    int pos;
    readonly StringBuilder sb = new();
    readonly StringBuilder white = new();
    bool prevLf;

    public int Line { get; private set; }
    public int Column { get; private set; }

    protected IJsonReader Reader { get; private set; }
    protected bool HasReader => Reader != null;

    public bool ReadWsc { get; set; }

    public BaseReader(TextReader reader, IJsonReader jsonReader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        Reader = jsonReader;
        buffer = reader.ReadToEnd();
        Reset();
    }

    public void Reset()
    {
        Line = 1;
        pos = 0;
        white.Length = sb.Length = 0;
        prevLf = false;
    }

    public int PeekChar(int idx = 0)
    {
        if (idx < 0) throw new ArgumentOutOfRangeException(nameof(idx));
        int p = pos + idx;
        return (uint)p < (uint)buffer.Length ? buffer[p] : -1;
    }

    public virtual int SkipPeekChar()
    {
        SkipWhite();
        return PeekChar();
    }

    public int ReadChar()
    {
        if ((uint)pos >= (uint)buffer.Length) return -1;

        char v = buffer[pos++];

        if (ReadWsc && v != '\r') white.Append(v);

        if (prevLf)
        {
            Line++;
            Column = 0;
            prevLf = false;
        }

        if (v == '\n') prevLf = true;
        Column++;

        return v;
    }

    protected void ResetWhite()
    {
        if (ReadWsc) white.Length = 0;
    }

    protected virtual string GetWhite()
    {
        if (!ReadWsc) throw new InvalidOperationException();
        return white.ToString();
    }

    public static bool IsWhite(char c)
    {
        return c == ' ' || c == '\t' || c == '\r' || c == '\n';
    }

    public void SkipWhite()
    {
        while (IsWhite((char)PeekChar())) ReadChar();
    }

    // Returns either long or double, depending on the parsed value.
    public JsonValue ReadNumericLiteral()
    {
        int c, leadingZeros = 0;
        bool testLeading = true;
        Span<char> numBuf = stackalloc char[64];
        int numLen = 0;

        if (PeekChar() == '-')
        {
            numBuf[numLen++] = '-';
            ReadChar();
            if (PeekChar() < 0) throw ParseError("Invalid JSON numeric literal; extra negation");
        }

        for (; ; )
        {
            c = PeekChar();
            if (c < '0' || c > '9') break;
            if (testLeading)
            {
                if (c == '0') leadingZeros++;
                else testLeading = false;
            }
            numBuf[numLen++] = (char)c;
            ReadChar();
        }
        if (testLeading) leadingZeros--; // single 0 is allowed
        if (leadingZeros > 0) throw ParseError("leading multiple zeros are not allowed");

        bool hasFracOrExp = false;

        // fraction
        if (PeekChar() == '.')
        {
            hasFracOrExp = true;
            int fdigits = 0;
            numBuf[numLen++] = '.';
            ReadChar();
            if (PeekChar() < 0) throw ParseError("Invalid JSON numeric literal; extra dot");
            for (; ; )
            {
                c = PeekChar();
                if (c < '0' || '9' < c) break;
                numBuf[numLen++] = (char)c;
                ReadChar();
                fdigits++;
            }
            if (fdigits == 0) throw ParseError("Invalid JSON numeric literal; extra dot");
        }

        c = PeekChar();
        if (c == 'e' || c == 'E')
        {
            hasFracOrExp = true;
            numBuf[numLen++] = (char)c;
            ReadChar();
            if (PeekChar() < 0) throw new ArgumentException("Invalid JSON numeric literal; incomplete exponent");

            c = PeekChar();
            if (c == '-')
            {
                numBuf[numLen++] = '-';
                ReadChar();
            }
            else if (c == '+')
            {
                numBuf[numLen++] = '+';
                ReadChar();
            }

            if (PeekChar() < 0) throw ParseError("Invalid JSON numeric literal; incomplete exponent");

            for (; ; )
            {
                c = PeekChar();
                if (c < '0' || c > '9') break;
                numBuf[numLen++] = (char)c;
                ReadChar();
            }
        }

        var numSpan = numBuf[..numLen];

        // Try parsing as long first to preserve precision for large integers
        if (!hasFracOrExp && long.TryParse(numSpan, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out long lval))
        {
            // Preserve negative zero as double (-0 as long is just 0)
            if (lval == 0 && numSpan[0] == '-') return -0.0;
            return lval;
        }

        double val = double.Parse(numSpan, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
        if (val == 0.0 && double.IsNegative(val)) return -0.0;

        return val;
    }

    public string ReadStringLiteral(Func<string> allowML)
    {
        // callers make sure that (exitCh == '"' || exitCh == "'")

        int exitCh = ReadChar();
        sb.Length = 0;
        for (; ; )
        {
            int c = ReadChar();
            if (c < 0) throw ParseError("JSON string is not closed");
            if (c == exitCh)
            {
                if (allowML != null && exitCh == '\'' && PeekChar() == '\'' && sb.Length == 0)
                {
                    // ''' indicates a multiline string
                    ReadChar();
                    return allowML();
                }
                else return sb.ToString();
            }
            else if (c == '\n' || c == '\r')
            {
                throw ParseError("Bad string containing newline");
            }
            else if (c != '\\')
            {
                sb.Append((char)c);
                continue;
            }

            // escaped expression
            c = ReadChar();
            if (c < 0)
                throw ParseError("Invalid JSON string literal; incomplete escape sequence");
            switch (c)
            {
                case '"':
                case '\'':
                case '\\':
                case '/': sb.Append((char)c); break;
                case 'b': sb.Append('\x8'); break;
                case 'f': sb.Append('\f'); break;
                case 'n': sb.Append('\n'); break;
                case 'r': sb.Append('\r'); break;
                case 't': sb.Append('\t'); break;
                case 'u':
                    ushort cp = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        cp <<= 4;
                        if ((c = ReadChar()) < 0)
                            throw ParseError("Incomplete unicode character escape literal");
                        if (c >= '0' && c <= '9') cp += (ushort)(c - '0');
                        else if (c >= 'A' && c <= 'F') cp += (ushort)(c - 'A' + 10);
                        else if (c >= 'a' && c <= 'f') cp += (ushort)(c - 'a' + 10);
                        else throw ParseError("Bad \\u char " + (char)c);
                    }
                    sb.Append((char)cp);
                    break;
                default:
                    throw ParseError("Invalid JSON string literal; unexpected escape character");
            }
        }
    }

    public void Expect(char expected)
    {
        int c;
        if ((c = ReadChar()) != expected)
            throw ParseError($"Expected '{expected}', got '{(char)c}'");
    }

    public void Expect(string expected)
    {
        for (int i = 0; i < expected.Length; i++)
            if (ReadChar() != expected[i])
                throw ParseError($"Expected '{expected}', differed at {i}");
    }

    public Exception ParseError(string msg)
    {
        return new ArgumentException($"{msg}. At line {Line}, column {Column}");
    }
}