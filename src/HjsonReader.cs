using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Hjson
{
  using JsonPair=KeyValuePair<string, JsonValue>;

  internal class HjsonReader : BaseReader
  {
    StringBuilder sb=new StringBuilder();

    public HjsonReader(TextReader reader, IJsonReader jsonReader)
      : base(reader, jsonReader)
    {
    }

    public JsonValue Read()
    {
      JsonValue v;
      // Braces for the root object are optional

      int c=SkipPeekChar();
      //if (c<0) throw ParseError("Incomplete input");
      switch (c)
      {
        case '[':
        case '{':
          v=ReadCore();
          break;
        default:
          // look if we are dealing with a single JSON value (true/false/null/#/"")
          // if it is multiline we assume it's a Hjson object without root braces.
          int i=0, line=Line;
          while (line==1)
          {
            c=PeekChar(i++);
            if (c=='\n') line++;
            else if (c<0) break;
          }
          // if we have multiple lines, assume optional {} (but ignore \n suffix)
          v=ReadCore(line>1 && (c!='\n' || PeekChar(i)>=0));
          break;
      }

      skipWhite2();
      if (ReadChar()>=0) throw ParseError("Extra characters in input");
      return v;
    }

    void skipWhite2()
    {
      while (PeekChar()>=0)
      {
        while (IsWhite((char)PeekChar())) ReadChar();
        int p=PeekChar();
        if (p=='#' || p=='/' && PeekChar(1)=='/')
        {
          for (; ; )
          {
            var ch=PeekChar();
            if (ch<0 || ch=='\n') break;
            ReadChar();
          }
        }
        else if (p=='/' && PeekChar(1)=='*')
        {
          ReadChar(); ReadChar();
          for (; ; )
          {
            var ch=PeekChar();
            if (ch<0 || ch=='*' && PeekChar(1)=='/') break;
            ReadChar();
          }
          if (PeekChar()>=0) { ReadChar(); ReadChar(); }
        }
        else break;
      }
    }

    protected override string GetWhite()
    {
      var res=base.GetWhite();
      int to=res.Length-1;
      if (to>=0)
      {
        // remove trailing whitespace
        for (; to>0 && res[to]<=' ' && res[to]!='\n'; to--) ;
        // but only up to EOL
        if (res[to]=='\n') to--;
        if (to>=0 && res[to]=='\r') to--;
        res=res.Substring(0, to+1);
        foreach (char c in res) if (c>' ') return res;
      }
      return "";
    }

    public override int SkipPeekChar()
    {
      skipWhite2();
      return PeekChar();
    }

    JsonValue ReadCore(bool objectWithoutBraces=false)
    {
      int c=objectWithoutBraces?'{':SkipPeekChar();
      if (c<0) throw ParseError("Incomplete input");
      switch (c)
      {
        case '[':
          JsonArray list;
          WscJsonArray wscL=null;
          ReadChar();
          ResetWhite();
          if (ReadWsc) list=wscL=new WscJsonArray();
          else list=new JsonArray();
          SkipPeekChar();
          if (ReadWsc) wscL.Comments.Add(GetWhite());
          for (int i=0; ; i++)
          {
            if (SkipPeekChar()==']') { ReadChar(); break; }
            if (HasReader) Reader.Index(i);
            var value=ReadCore();
            if (HasReader) Reader.Value(value);
            list.Add(value);
            ResetWhite();
            if (SkipPeekChar()==',') { ReadChar(); ResetWhite(); SkipPeekChar(); }
            if (ReadWsc) wscL.Comments.Add(GetWhite());
          }
          return list;
        case '{':
          JsonObject obj;
          WscJsonObject wsc=null;
          if (!objectWithoutBraces)
          {
            ReadChar();
            ResetWhite();
          }
          if (ReadWsc) obj=wsc=new WscJsonObject() { RootBraces=!objectWithoutBraces };
          else obj=new JsonObject();
          SkipPeekChar();
          if (ReadWsc) wsc.Comments[""]=GetWhite();
          for (; ; )
          {
            if (objectWithoutBraces) { if (SkipPeekChar()<0) break; }
            else if (SkipPeekChar()=='}') { ReadChar(); break; }
            string name=readKeyName();
            skipWhite2();
            Expect(':');
            skipWhite2();
            if (HasReader) Reader.Key(name);
            var value=ReadCore();
            if (HasReader) Reader.Value(value);
            obj.Add(new JsonPair(name, value));
            ResetWhite();
            if (SkipPeekChar()==',') { ReadChar(); ResetWhite(); SkipPeekChar(); }
            if (ReadWsc) { wsc.Comments[name]=GetWhite(); wsc.Order.Add(name); }
          }
          return obj;
        case '"': return ReadStringLiteral();
        default: return readTfnns(c);
      }
    }

    string readKeyName()
    {
      // quotes for keys are optional in Hjson
      // unless they include {}[],: or whitespace.

      if (PeekChar()=='"') return ReadStringLiteral();

      sb.Length=0;
      for (; ; )
      {
        int c=PeekChar();
        if (c<0) throw ParseError("Name is not closed");
        char ch=(char)c;
        if (ch==':')
        {
          if (sb.Length==0) throw ParseError("Empty key name requires quotes");
          return sb.ToString();
        }
        else if (IsWhite(ch) || ch=='{' || ch=='}' || ch=='[' || ch==']' || ch==',')
          throw ParseError("Key names that include {}[],: or whitespace require quotes");
        ReadChar();
        sb.Append(ch);
      }
    }

    void skipIndent(int indent)
    {
      while (indent-->0)
      {
        char c=(char)PeekChar();
        if (IsWhite(c) && c!='\n') ReadChar();
        else break;
      }
    }

    JsonValue readMlString()
    {
      // Parse a multiline string value.
      int triple=0;
      sb.Length=0;

      // we are at '''
      var indent=Column-3;

      // skip white/to (newline)
      for (; ; )
      {
        char c=(char)PeekChar();
        if (IsWhite(c) && c!='\n') ReadChar();
        else break;
      }
      if (PeekChar()=='\n') { ReadChar(); skipIndent(indent); }

      // When parsing for string values, we must look for " and \ characters.
      while (true)
      {
        int ch=PeekChar();
        if (ch<0) throw ParseError("Bad multiline string");
        else if (ch=='\'')
        {
          triple++;
          ReadChar();
          if (triple==3)
          {
            if (sb[sb.Length-1]=='\n') sb.Length--;
            return sb.ToString();
          }
          else continue;
        }
        else
        {
          while (triple>0)
          {
            sb.Append('\'');
            triple--;
          }
        }
        if (ch=='\n')
        {
          sb.Append('\n');
          ReadChar();
          skipIndent(indent);
        }
        else
        {
          if (ch!='\r') sb.Append((char)ch);
          ReadChar();
        }
      }
    }

    internal static bool TryParseNumericLiteral(string text, bool stopAtNext, out JsonValue value)
    {
      int c, leadingZeros=0, p=0;
      double val=0;
      bool negative=false, testLeading=true;
      text+='\0';
      value=null;

      if (text[p]=='-')
      {
        negative=true;
        p++;
        if (text[p]==0) return false;
      }

      for (int x=0; ; x++)
      {
        c=text[p];
        if (c<'0' || c>'9') break;
        if (testLeading)
        {
          if (c=='0') leadingZeros++;
          else testLeading=false;
        }
        val=val*10+(c-'0');
        p++;
      }
      if (testLeading) leadingZeros--; // single 0 is allowed
      if (leadingZeros>0) return false;

      // fraction
      if (text[p]=='.')
      {
        if (leadingZeros<0) return false;
        int fdigits=0;
        double frac=0;
        p++;
        if (text[p]==0) return false;
        double d=10;
        for (; ; )
        {
          c=text[p];
          if (c<'0' || '9'<c) break;
          p++;
          frac+=(c-'0')/d;
          d*=10;
          fdigits++;
        }
        if (fdigits==0) return false;
        val+=frac;
      }

      c=text[p];
      if (c=='e' || c=='E')
      {
        // exponent
        int exp=0, expSign=1;

        p++;
        if (text[p]==0) return false;

        c=text[p];
        if (c=='-')
        {
          p++;
          expSign=-1;
        }
        else if (c=='+') p++;

        if (text[p]==0) return false;

        for (; ; )
        {
          c=text[p];
          if (c<'0' || c>'9') break;
          exp=exp*10+(c-'0');
          p++;
        }

        if (exp!=0)
          val*=Math.Pow(10, exp*expSign);
      }

      while (p<text.Length && IsWhite(text[p])) p++;

      bool foundStop=false;
      if (p<text.Length && stopAtNext)
      {
        // end scan if we find a control character like ,}] or a comment
        char ch=text[p];
        if (ch==',' || ch=='}' || ch==']' || ch=='#' || ch=='/' && (text.Length>p+1 && (text[p+1]=='/' || text[p+1]=='*')))
          foundStop=true;
      }

      if (p+1!=text.Length && !foundStop) return false;

      if (negative) val*=-1;
      long lval=(long)val;
      if (lval==val) value=lval;
      else value=val;
      return true;
    }

    JsonValue readTfnns(int c)
    {
      sb.Length=0;
      for (; ; )
      {
        if (c<0) throw ParseError("String did not end");
        if (c=='\n' || c==',' ||
          c=='}' || c==']' ||
          c=='#' ||
          c=='/' && (PeekChar(1)=='/' || PeekChar(1)=='*'))
        {
          if (sb.Length>0)
          {
            char ch=sb[0];
            switch (ch)
            {
              case 'f': if (sb.ToString().Trim()=="false") return false; break;
              case 'n': if (sb.ToString().Trim()=="null") return null; break;
              case 't': if (sb.ToString().Trim()=="true") return true; break;
              default:
                if (ch=='-' || ch>='0' && ch<='9')
                {
                  JsonValue res;
                  if (TryParseNumericLiteral(sb.ToString(), false, out res)) return res;
                }
                break;
            }
          }
          if (c=='\n') return sb.ToString();
        }
        ReadChar();
        if (c!='\r')
        {
          sb.Append((char)c);
          if (sb.Length==3 && sb[0]=='\'' && sb[1]=='\'' && sb[2]=='\'') return readMlString();
        }
        c=PeekChar();
      }
    }
  }
}
