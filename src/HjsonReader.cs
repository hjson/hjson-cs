using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Hjson
{
  internal class HjsonReader : BaseReader
  {
    StringBuilder sb=new StringBuilder();

    public HjsonReader(TextReader reader)
      : base(reader)
    {
    }

    public object Read()
    {
      object v=ReadCore();
      skipWhite2();
      if (ReadChar()>=0) throw ParseError("Extra characters in input");
      return v;
    }

    void skipWhite2()
    {
      while (PeekChar()>=0)
      {
        while (IsWhite((char)PeekChar())) ReadChar();
        if (PeekChar()=='#')
        {
          for (; ; )
          {
            var ch=PeekChar();
            if (ch<0 || ch=='\n') break;
            ReadChar();
          }
        }
        else break;
      }
    }

    object ReadCore()
    {
      skipWhite2();
      int c=PeekChar();
      if (c<0) throw ParseError("Incomplete input");
      switch (c)
      {
        case '[':
          ReadChar();
          var list=new List<object>();
          skipWhite2();
          if (PeekChar()==']')
          {
            ReadChar();
            return list;
          }
          for (; ; )
          {
            list.Add(ReadCore());
            skipWhite2();
            c=PeekChar();
            if (c==',') ReadChar();
            else if (c==']') { ReadChar(); break; }
          }
          return list.ToArray();
        case '{':
          ReadChar();
          var obj=new Dictionary<string, object>();
          skipWhite2();
          if (PeekChar()=='}') { ReadChar(); return obj; }
          for (; ; )
          {
            skipWhite2();
            if (PeekChar()=='}') { ReadChar(); break; }
            string name=readName();
            skipWhite2();
            Expect(':');
            skipWhite2();
            obj[name]=ReadCore(); // it does not reject duplicate names.
            skipWhite2();
            c=PeekChar();
            if (c==',') ReadChar();
            else if (c=='}') { ReadChar(); break; }
          }
          return obj;
        case '-': return ReadNumericLiteral();
        default: return readPrimitive(c);
      }
    }

    string readName()
    {
      if (PeekChar()=='"') return ReadStringLiteral();

      sb.Length=0;
      for (; ; )
      {
        int c=PeekChar();
        if (c<0) throw ParseError("Name is not closed");
        char ch=(char)c;
        if (ch==':') return sb.ToString();
        if (!char.IsLetterOrDigit(ch)) throw ParseError("Unquoted keyname may only contain letters and digits");
        ReadChar();
        sb.Append(ch);
      }
    }

    object readPrimitive(int c)
    {
      if (c=='"') return ReadStringLiteral();
      else if (c>='0' && c<='9') return ReadNumericLiteral();
      else return readMore();
    }

    object readMore()
    {
      sb.Length=0;
      for (; ; )
      {
        int c=ReadChar();
        if (c<0) throw ParseError("String did not end");
        if (c=='\r' || c=='\n') return sb.ToString();
        sb.Append((char)c);
        if (sb.Length==4)
        {
          string v=sb.ToString();
          if (v=="true") return true;
          else if (v=="null") return (string)null;
        }
        else if (sb.Length==5 && sb.ToString()=="false") return false;
      }
    }
  }
}
