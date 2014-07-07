using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Hjson
{
  public static class HjsonValue
  {
    public static JsonValue Load(string path)
    {
      if (Path.GetExtension(path).ToLower()==".json") return JsonValue.Load(path);
      try
      {
        using (var s=File.OpenRead(path))
          return Load(s);
      }
      catch (Exception e) { throw new Exception(e.Message+" (in "+path+")", e); }
    }

    public static JsonValue Load(Stream stream)
    {
      if (stream==null) throw new ArgumentNullException("stream");
      return Load(new StreamReader(stream, true));
    }

    public static JsonValue Load(TextReader textReader)
    {
      if (textReader==null) throw new ArgumentNullException("textReader");
      var ret=new HjsonReader(textReader).Read();
      return JsonValue.ToJsonValue(ret);
    }

    public static JsonValue Parse(string hjsonString)
    {
      if (hjsonString==null) throw new ArgumentNullException("hjsonString");
      return Load(new StringReader(hjsonString));
    }

    public static void Save(JsonValue json, string path)
    {
      if (Path.GetExtension(path).ToLower()==".json") { json.Save(path, true); return; }
      using (var s=File.CreateText(path))
        Save(json, s);
    }

    public static void Save(JsonValue json, Stream stream)
    {
      if (stream==null) throw new ArgumentNullException("stream");
      Save(json, new StreamWriter(stream));
    }

    public static void Save(JsonValue json, TextWriter textWriter)
    {
      if (textWriter==null) throw new ArgumentNullException("textWriter");
      new HjsonWriter().Save(json, textWriter, 0);
      textWriter.Flush();
    }

    public static string SaveAsString(JsonValue json)
    {
      var sw=new StringWriter();
      Save(json, sw);
      return sw.ToString();
    }
  }
}
