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

  internal class JsonWriter
  {
    bool format;

    public JsonWriter(bool format)
    {
      this.format=format;
    }

    void nl(TextWriter tw, int level)
    {
      if (format)
      {
        tw.Write(System.Environment.NewLine);
        tw.Write(new string(' ', level*2));
      }
    }

    public void Save(JsonValue value, TextWriter tw, int level)
    {
      bool following=false;
      switch (value.JsonType)
      {
        case JsonType.Object:
          if (level>0) nl(tw, level);
          tw.Write('{');
          foreach (JsonPair pair in ((JsonObject)value))
          {
            if (following) tw.Write(",");
            nl(tw, level+1);
            tw.Write('\"');
            tw.Write(EscapeString(pair.Key));
            tw.Write("\":");
            var nextType=pair.Value!=null?(JsonType?)pair.Value.JsonType:null;
            if (format && nextType!=JsonType.Array && nextType!=JsonType.Object) tw.Write(" ");
            if (pair.Value==null) tw.Write("null");
            else Save(pair.Value, tw, level+1);
            following=true;
          }
          nl(tw, level);
          tw.Write('}');
          break;
        case JsonType.Array:
          if (level>0) nl(tw, level);
          tw.Write('[');
          foreach (JsonValue v in ((JsonArray)value))
          {
            if (following) tw.Write(",");
            if (v!=null)
            {
              if (v.JsonType!=JsonType.Array && v.JsonType!=JsonType.Object) nl(tw, level+1);
              Save(v, tw, level+1);
            }
            else
            {
              nl(tw, level+1);
              tw.Write("null");
            }
            following=true;
          }
          nl(tw, level);
          tw.Write(']');
          break;
        case JsonType.Boolean:
          tw.Write((bool)value?"true":"false");
          break;
        case JsonType.String:
          tw.Write('"');
          tw.Write(EscapeString(((JsonPrimitive)value).GetRawString()));
          tw.Write('"');
          break;
        default:
          tw.Write(((JsonPrimitive)value).GetRawString());
          break;
      }
    }

    internal static string EscapeString(string src)
    {
      if (src==null) return null;

      for (int i=0; i<src.Length; i++)
      {
        if (getEscapedChar(src[i])!=null)
        {
          var sb=new StringBuilder();
          if (i>0) sb.Append(src, 0, i);
          return doEscapeString(sb, src, i);
        }
      }
      return src;
    }

    static string doEscapeString(StringBuilder sb, string src, int cur)
    {
      int start=cur;
      for (int i=cur; i<src.Length; i++)
      {
        string escaped=getEscapedChar(src[i]);
        if (escaped!=null)
        {
          sb.Append(src, start, i-start);
          sb.Append(escaped);
          start=i+1;
        }
      }
      sb.Append(src, start, src.Length-start);
      return sb.ToString();
    }

    static string getEscapedChar(char c)
    {
      switch (c)
      {
        case '\"': return "\\\"";
        case '\t': return "\\t";
        case '\n': return "\\n";
        case '\r': return "\\r";
        case '\f': return "\\f";
        case '\b': return "\\b";
        case '\\': return "\\\\";
        default: return null;
      }
    }
  }
}
