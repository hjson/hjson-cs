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
    public bool WriteWsc { get; set; }

    public HjsonWriter()
    {
    }

    void nl(TextWriter tw, int level)
    {
      tw.Write("\n");
      tw.Write(new string(' ', level*2));
    }

    string getWsc(string str)
    {
      if (string.IsNullOrEmpty(str)) return "";
      foreach (char c in str)
      {
        if (c=='\n' || c=='#') break;
        if (c>' ') return " # "+str;
      }
      return str;
    }

    string getWsc(Dictionary<string, string> white, string key) { return white.ContainsKey(key)?getWsc(white[key]):""; }
    string getWsc(List<string> white, int index) { return white.Count>index?getWsc(white[index]):""; }
    bool testWsc(string str) { return str.Length>0 && str[str[0]=='\r' && str.Length>1?1:0]!='\n'; }

    public void Save(JsonValue value, TextWriter tw, int level, bool hasComment)
    {
      switch (value.JsonType)
      {
        case JsonType.Object:
          var obj=value.Qo();
          WscJsonObject kw=WriteWsc?obj as WscJsonObject:null;
          if (level>0) nl(tw, level);
          tw.Write('{');
          if (kw!=null)
          {
            var kwl=getWsc(kw.Comments, "");
            foreach (string key in kw.Order.Concat(kw.Keys).Distinct())
            {
              if (!obj.ContainsKey(key)) continue;
              var val=obj[key];
              tw.Write(kwl);
              nl(tw, level+1);
              kwl=getWsc(kw.Comments, key);

              tw.Write(escapeName(key));
              tw.Write(": ");
              if (val==null) tw.Write("null");
              else Save(val, tw, level+1, testWsc(kwl));
            }
            tw.Write(kwl);
            nl(tw, level);
          }
          else
          {
            foreach (JsonPair pair in ((JsonObject)value))
            {
              nl(tw, level+1);
              tw.Write(escapeName(pair.Key));
              tw.Write(": ");
              if (pair.Value==null) tw.Write("null");
              else Save(pair.Value, tw, level+1, false);
            }
            nl(tw, level);
          }
          tw.Write('}');
          break;
        case JsonType.Array:
          if (level>0) nl(tw, level);
          tw.Write('[');
          int i=0, n=value.Count;
          WscJsonArray whiteL=null;
          string wsl=null;
          if (WriteWsc)
          {
            whiteL=value as WscJsonArray;
            if (whiteL!=null) wsl=getWsc(whiteL.Comments, 0);
          }
          for (; i<n; i++)
          {
            var v=value[i];
            if (whiteL!=null)
            {
              tw.Write(wsl);
              wsl=getWsc(whiteL.Comments, i+1);
            }
            if (v.JsonType!=JsonType.Array && v.JsonType!=JsonType.Object) nl(tw, level+1);
            if (v!=null) Save(v, tw, level+1, wsl!=null && testWsc(wsl));
            else tw.Write("null");
          }
          if (whiteL!=null) tw.Write(wsl);
          nl(tw, level);
          tw.Write(']');
          break;
        case JsonType.Boolean:
          tw.Write((bool)value?"true":"false");
          break;
        case JsonType.String:
          writeString(((JsonPrimitive)value).GetRawString(), tw, level, hasComment);
          break;
        default:
          tw.Write(((JsonPrimitive)value).GetRawString());
          break;
      }
    }

    static string escapeName(string name)
    {
      if (name.Any(c => !char.IsLetterOrDigit(c))) return "\""+JsonWriter.EscapeString(name)+"\"";
      else return name;
    }

    void writeString(string value, TextWriter tw, int level, bool hasComment)
    {
      if (value=="") { tw.Write("\"\""); return; }

      char first=value[0], last=value[value.Length-1];
      bool doEscape=value.Any(c => shouldEscapeChar(c));

      if (hasComment ||
        BaseReader.IsWhite(first) ||
        char.IsDigit(first) ||
        first=='#' ||
        first=='-' ||
        first=='{' ||
        first=='[' ||
        BaseReader.IsWhite(last) ||
        doEscape ||
        isKeyword(value))
      {
        if (doEscape && !value.Any(c => shouldEscapeCharExceptLF(c))) writeMLString(value, tw, level);
        else tw.Write("\""+JsonWriter.EscapeString(value)+"\"");
      }
      else tw.Write(value);
    }

    void writeMLString(string value, TextWriter tw, int level)
    {
      level++;
      nl(tw, level);
      tw.Write("'''");

      foreach (var line in value.Replace("\r", "").Split('\n'))
      {
        nl(tw, level);
        tw.Write(line);
      }
      tw.Write("'''");
    }

    static bool isKeyword(string value)
    {
      return value=="true" || value=="false" || value=="null";
    }

    static bool shouldEscapeCharExceptLF(char c)
    {
      switch (c)
      {
        case '\"':
        case '\t':
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

    static bool shouldEscapeChar(char c)
    {
      switch (c)
      {
        case '\n':
        case '\r':
          return true;
        default:
          return shouldEscapeCharExceptLF(c);
      }
    }
  }
}
