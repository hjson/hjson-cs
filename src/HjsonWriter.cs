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
      tw.Write(System.Environment.NewLine);
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

    public void Save(JsonValue value, TextWriter tw, int level, bool hasComment, string separator)
    {
      if (value==null)
      {
        tw.Write(separator);
        tw.Write("null");
        return;
      }
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
              tw.Write(":");
              var nextType=val!=null?(JsonType?)val.JsonType:null;
              Save(val, tw, level+1, testWsc(kwl), separator);
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
              tw.Write(":");
              var nextType=pair.Value!=null?(JsonType?)pair.Value.JsonType:null;
              Save(pair.Value, tw, level+1, false, " ");
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
            if (v==null || v.JsonType!=JsonType.Array && v.JsonType!=JsonType.Object) nl(tw, level+1);
            Save(v, tw, level+1, wsl!=null && testWsc(wsl), "");
          }
          if (whiteL!=null) tw.Write(wsl);
          nl(tw, level);
          tw.Write(']');
          break;
        case JsonType.Boolean:
          tw.Write(separator);
          tw.Write((bool)value?"true":"false");
          break;
        case JsonType.String:
          writeString(((JsonPrimitive)value).GetRawString(), tw, level, hasComment, separator);
          break;
        default:
          tw.Write(separator);
          tw.Write(((JsonPrimitive)value).GetRawString());
          break;
      }
    }

    static string escapeName(string name)
    {
      if (name.Any(c => !char.IsLetterOrDigit(c))) return "\""+JsonWriter.EscapeString(name)+"\"";
      else return name;
    }

    void writeString(string value, TextWriter tw, int level, bool hasComment, string separator)
    {
      if (value=="") { tw.Write(separator+"\"\""); return; }

      char first=value[0], last=value[value.Length-1];
      bool doEscape=hasComment || value.Any(c => needsQuotes(c));
      JsonValue dummy;

      if (doEscape ||
        BaseReader.IsWhite(first) ||
        first=='"' ||
        first=='#' ||
        first=='{' ||
        first=='[' ||
        BaseReader.IsWhite(last) ||
        HjsonReader.TryParseNumericLiteral(value, out dummy) ||
        isKeyword(value))
      {
        // If the string contains no control characters, no quote characters, and no
        // backslash characters, then we can safely slap some quotes around it.
        // Otherwise we first check if the string can be expressed in multiline
        // format or we must replace the offending characters with safe escape
        // sequences.

        if (!value.Any(c => needsEscape(c))) tw.Write(separator+"\""+value+"\"");
        else if (!value.Any(c => needsEscapeML(c)) && !value.Contains("'''")) writeMLString(value, tw, level, separator);
        else tw.Write(separator+"\""+JsonWriter.EscapeString(value)+"\"");
      }
      else tw.Write(separator+value);
    }

    void writeMLString(string value, TextWriter tw, int level, string separator)
    {
      var lines=value.Replace("\r", "").Split('\n');

      if (lines.Length==1)
      {
        tw.Write(separator+"'''");
        tw.Write(lines[0]);
        tw.Write("'''");
      }
      else
      {
        level++;
        nl(tw, level);
        tw.Write("'''");

        foreach (var line in lines)
        {
          nl(tw, level);
          tw.Write(line);
        }
        tw.Write("'''");
      }
    }

    static bool isKeyword(string value)
    {
      return value=="true" || value=="false" || value=="null";
    }

    static bool needsQuotes(char c)
    {
      switch (c)
      {
        case '\t':
        case '\f':
        case '\b':
        case '\n':
        case '\r':
          return true;
        default:
          return false;
      }
    }

    static bool needsEscape(char c)
    {
      switch (c)
      {
        case '\"':
        case '\\':
          return true;
        default:
          return needsQuotes(c);
      }
    }

    static bool needsEscapeML(char c)
    {
      switch (c)
      {
        case '\n':
        case '\r':
          return false;
        default:
          return needsQuotes(c);
      }
    }
  }
}
