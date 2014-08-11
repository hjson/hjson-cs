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
    public JsonReader(TextReader reader, IJsonReader jsonReader)
      : base(reader, jsonReader)
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
      int c=SkipPeekChar();
      if (c<0) throw ParseError("Incomplete JSON input");
      switch (c)
      {
        case '[':
          ReadChar();
          var list=new List<object>();
          if (SkipPeekChar()==']')
          {
            ReadChar();
            return list;
          }
          for (int i=0; ; i++)
          {
            if (HasReader) Reader.Index(i);
            var value=ReadCore();
            if (HasReader) Reader.Value(JsonValue.ToJsonValue(value));
            list.Add(value);
            c=SkipPeekChar();
            if (c!=',') break;
            ReadChar();
          }
          if (ReadChar()!=']')
            throw ParseError("Array must end with ']'");
          return list.ToArray();
        case '{':
          ReadChar();
          var obj=new Dictionary<string, object>();
          if (SkipPeekChar()=='}')
          {
            ReadChar();
            return obj;
          }
          for (; ; )
          {
            if (SkipPeekChar()=='}') { ReadChar(); break; }
            string name=ReadStringLiteral();
            SkipWhite();
            Expect(':');
            SkipWhite();
            if (HasReader) Reader.Key(name);
            var value=ReadCore();
            if (HasReader) Reader.Value(JsonValue.ToJsonValue(value));
            obj[name]=value; // it does not reject duplicate names.
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
