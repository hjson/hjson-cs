using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Hjson
{
  internal class JsonReader : BaseReader
  {
    public JsonReader(TextReader reader)
      : base(reader)
    {
    }

    public object Read()
    {
      object v=ReadCore();
      SkipWhite();
      if (ReadChar()>=0) throw ParseError("Extra characters in JSON input");
      return v;
    }

    object ReadCore()
    {
      SkipWhite();
      int c=PeekChar();
      if (c<0) throw ParseError("Incomplete JSON input");
      switch (c)
      {
        case '[':
          ReadChar();
          var list=new List<object>();
          SkipWhite();
          if (PeekChar()==']')
          {
            ReadChar();
            return list;
          }
          for (; ; )
          {
            list.Add(ReadCore());
            SkipWhite();
            c=PeekChar();
            if (c!=',') break;
            ReadChar();
          }
          if (ReadChar()!=']')
            throw ParseError("Array must end with ']'");
          return list.ToArray();
        case '{':
          ReadChar();
          var obj=new Dictionary<string, object>();
          SkipWhite();
          if (PeekChar()=='}')
          {
            ReadChar();
            return obj;
          }
          for (; ; )
          {
            SkipWhite();
            if (PeekChar()=='}') { ReadChar(); break; }
            string name=ReadStringLiteral();
            SkipWhite();
            Expect(':');
            SkipWhite();
            obj[name]=ReadCore(); // it does not reject duplicate names.
            SkipWhite();
            c=ReadChar();
            if (c=='}') break;
            //if (c==',') continue;
          }
          return obj;
        case 't':
          Expect("true");
          return true;
        case 'f':
          Expect("false");
          return false;
        case 'n':
          Expect("null");
          return (string)null;
        case '"':
          return ReadStringLiteral();
        default:
          if (c>='0' && c<='9' || c=='-')
            return ReadNumericLiteral();
          else
            throw ParseError(String.Format("Unexpected character '{0}'", (char)c));
      }
    }
  }
}
