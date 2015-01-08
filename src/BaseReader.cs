using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Hjson
{
  internal abstract class BaseReader
  {
    TextReader r;
    StringBuilder sb=new StringBuilder();
    StringBuilder white=new StringBuilder();
    int[] peek=new int[2];
    int hasPeek;
    bool prevLf;

    public int Line { get; private set; }
    public int Column { get; private set; }

    protected IJsonReader Reader { get; private set; }
    protected bool HasReader { get { return Reader!=null; } }

    public bool ReadWsc { get; set; }

    public BaseReader(TextReader reader, IJsonReader jsonReader)
    {
      if (reader==null) throw new ArgumentNullException("reader");
      this.r=reader;
      this.Reader=jsonReader;
      Line=1;
    }

    public int PeekChar(int idx=0)
    {
      if (idx<0 || idx>1) throw new ArgumentOutOfRangeException();
      if (idx>=hasPeek)
        peek[hasPeek++]=r.Read();
      return peek[idx];
    }

    public virtual int SkipPeekChar()
    {
      SkipWhite();
      return PeekChar();
    }

    public int ReadChar()
    {
      int v=hasPeek>0?peek[--hasPeek]:r.Read();

      if (ReadWsc && v!='\r') white.Append((char)v);

      if (prevLf)
      {
        Line++;
        Column=0;
        prevLf=false;
      }

      if (v=='\n') prevLf=true;
      Column++;

      return v;
    }

    protected void ResetWhite()
    {
      if (ReadWsc) white.Length=0;
    }

    protected virtual string GetWhite()
    {
      if (!ReadWsc) throw new InvalidOperationException();
      return white.ToString();
    }

    public static bool IsWhite(char c)
    {
      return c==' ' || c=='\t' || c=='\r' || c=='\n';
    }

    public void SkipWhite()
    {
      for (; ; )
      {
        if (IsWhite((char)PeekChar())) ReadChar();
        else break;
      }
    }

    // It could return either long or decimal, depending on the parsed value.
    public JsonValue ReadNumericLiteral()
    {
      int c;
      decimal val=0;
      bool negative=false;

      if (PeekChar()=='-')
      {
        negative=true;
        ReadChar();
        if (PeekChar()<0) throw ParseError("Invalid JSON numeric literal; extra negation");
      }

      bool zeroStart=PeekChar()=='0';
      for (int x=0; ; x++)
      {
        c=PeekChar();
        if (c<'0' || c>'9') break;
        val=val*10+(c-'0');
        ReadChar();
        if (zeroStart && x==1 && c=='0') throw ParseError("leading multiple zeros are not allowed");
      }

      // fraction
      if (PeekChar()=='.')
      {
        int fdigits=0;
        decimal frac=0;
        ReadChar();
        if (PeekChar()<0) throw ParseError("Invalid JSON numeric literal; extra dot");
        decimal d=10;
        for (; ; )
        {
          c=PeekChar();
          if (c<'0' || '9'<c) break;
          ReadChar();
          frac+=(c-'0')/d;
          d*=10;
          fdigits++;
        }
        if (fdigits==0) throw ParseError("Invalid JSON numeric literal; extra dot");
        val+=frac;
      }

      c=PeekChar();
      if (c=='e' || c=='E')
      {
        // exponent
        int exp=0, expSign=1;

        ReadChar();
        if (PeekChar()<0) throw new ArgumentException("Invalid JSON numeric literal; incomplete exponent");

        c=PeekChar();
        if (c=='-')
        {
          ReadChar();
          expSign=-1;
        }
        else if (c=='+') ReadChar();

        if (PeekChar()<0) throw ParseError("Invalid JSON numeric literal; incomplete exponent");

        for (; ; )
        {
          c=PeekChar();
          if (c<'0' || c>'9') break;
          exp=exp*10+(c-'0');
          ReadChar();
        }

        if (exp!=0)
          val*=(decimal)Math.Pow(10, exp*expSign);
      }

      if (negative) val*=-1;
      long lval=(long)val;
      if (lval==val) return lval;
      else return val;
    }

    public string ReadStringLiteral()
    {
      if (PeekChar()!='"') throw ParseError("Invalid JSON string literal format");

      ReadChar();
      sb.Length=0;
      for (; ; )
      {
        int c=ReadChar();
        if (c<0) throw ParseError("JSON string is not closed");
        if (c=='"') return sb.ToString();
        else if (c!='\\')
        {
          sb.Append((char)c);
          continue;
        }

        // escaped expression
        c=ReadChar();
        if (c<0)
          throw ParseError("Invalid JSON string literal; incomplete escape sequence");
        switch (c)
        {
          case '"':
          case '\\':
          case '/': sb.Append((char)c); break;
          case 'b': sb.Append('\x8'); break;
          case 'f': sb.Append('\f'); break;
          case 'n': sb.Append('\n'); break;
          case 'r': sb.Append('\r'); break;
          case 't': sb.Append('\t'); break;
          case 'u':
            ushort cp=0;
            for (int i=0; i<4; i++)
            {
              cp <<= 4;
              if ((c=ReadChar())<0)
                throw ParseError("Incomplete unicode character escape literal");
              if (c>='0' && c<='9') cp+=(ushort)(c-'0');
              if (c>='A' && c<='F') cp+=(ushort)(c-'A'+10);
              if (c>='a' && c<='f') cp+=(ushort)(c-'a'+10);
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
      if ((c=ReadChar())!=expected)
        throw ParseError(String.Format("Expected '{0}', got '{1}'", expected, (char)c));
    }

    public void Expect(string expected)
    {
      for (int i=0; i<expected.Length; i++)
        if (ReadChar()!=expected[i])
          throw ParseError(String.Format("Expected '{0}', differed at {1}", expected, i));
    }

    public Exception ParseError(string msg)
    {
      return new ArgumentException(String.Format("{0}. At line {1}, column {2}", msg, Line, Column));
    }
  }
}
