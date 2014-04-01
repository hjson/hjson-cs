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

  internal class HjsonWriter
  {
    public HjsonWriter()
    {
    }

    void nl(TextWriter tw, int level)
    {
      tw.Write("\n");
      tw.Write(new string(' ', level*2));
    }

    public void Save(JsonValue value, TextWriter tw, int level)
    {
      switch (value.JsonType)
      {
        case JsonType.Object:
          if (level>0) nl(tw, level);
          tw.Write('{');
          foreach (JsonPair pair in ((JsonObject)value))
          {
            nl(tw, level+1);
            tw.Write(escapeName(pair.Key));
            tw.Write(": ");
            if (pair.Value==null) tw.Write("null");
            else Save(pair.Value, tw, level+1);
          }
          nl(tw, level);
          tw.Write('}');
          break;
        case JsonType.Array:
          if (level>0) nl(tw, level);
          tw.Write('[');
          foreach (JsonValue v in ((JsonArray)value))
          {
            nl(tw, level+1);
            if (v!=null) Save(v, tw, level+1);
            else tw.Write("null");
          }
          nl(tw, level);
          tw.Write(']');
          break;
        case JsonType.Boolean:
          tw.Write((bool)value?"true":"false");
          break;
        case JsonType.String:
          tw.Write(escapeString(((JsonPrimitive)value).GetFormattedString()));
          break;
        default:
          tw.Write(((JsonPrimitive)value).GetFormattedString());
          break;
      }
    }

    static string escapeName(string name)
    {
      if (name.Any(c => !char.IsLetterOrDigit(c))) return "\""+JsonWriter.EscapeString(name)+"\"";
      else return name;
    }

    static string escapeString(string value)
    {
      if (value=="") return "\"\"";
      char first=value[0], last=value[value.Length-1];
      if (BaseReader.IsWhite(first) ||
        char.IsDigit(first) ||
        first=='#' ||
        first=='-' ||
        first=='{' ||
        first=='[' ||
        BaseReader.IsWhite(last) ||
        value.Any(c => shouldEscapeChar(c)) ||
        isKeyword(value)) return "\""+JsonWriter.EscapeString(value)+"\"";
      else return value;
    }

    static bool isKeyword(string value)
    {
      return value=="true" || value=="false" || value=="null";
    }

    static bool shouldEscapeChar(char c)
    {
      switch (c)
      {
        case '\"':
        case '\t':
        case '\n':
        case '\r':
        case '\f':
        case '\b':
        case '\u0085':
        case '\u2028':
        case '\u2029':
          return true;
        default:
          return false;
      }
    }
  }
}
