using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Hjson
{
  /// <summary>Contains functions to load and save in the Hjson format.</summary>
  public static class HjsonValue
  {
    /// <summary>Loads Hjson/JSON from a file.</summary>
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

    /// <summary>Loads Hjson/JSON from a stream.</summary>
    public static JsonValue Load(Stream stream)
    {
      if (stream==null) throw new ArgumentNullException("stream");
      return Load(new StreamReader(stream, true));
    }

    /// <summary>Loads Hjson/JSON from a TextReader.</summary>
    public static JsonValue Load(TextReader textReader, IJsonReader jsonReader=null)
    {
      if (textReader==null) throw new ArgumentNullException("textReader");
      var ret=new HjsonReader(textReader, jsonReader).Read();
      return ret;
    }

    /// <summary>Loads Hjson/JSON from a TextReader, preserving whitespace and comments.</summary>
    public static JsonValue LoadWsc(TextReader textReader)
    {
      if (textReader==null) throw new ArgumentNullException("textReader");
      var ret=new HjsonReader(textReader, null) { ReadWsc=true }.Read();
      return ret;
    }

    /// <summary>Parses the specified Hjson/JSON string.</summary>
    public static JsonValue Parse(string hjsonString)
    {
      if (hjsonString==null) throw new ArgumentNullException("hjsonString");
      return Load(new StringReader(hjsonString));
    }

    /// <summary>Saves Hjson to a file.</summary>
    public static void Save(JsonValue json, string path)
    {
      if (Path.GetExtension(path).ToLower()==".json") { json.Save(path, Stringify.Formatted); return; }
      using (var s=File.CreateText(path))
        Save(json, s);
    }

    /// <summary>Saves Hjson to a stream.</summary>
    public static void Save(JsonValue json, Stream stream)
    {
      if (stream==null) throw new ArgumentNullException("stream");
      Save(json, new StreamWriter(stream));
    }

    /// <summary>Saves Hjson to a TextWriter.</summary>
    public static void Save(JsonValue json, TextWriter textWriter)
    {
      if (textWriter==null) throw new ArgumentNullException("textWriter");
      new HjsonWriter().Save(json, textWriter, 0, false, "");
      textWriter.Flush();
    }

    /// <summary>Saves Hjson to a string, adding whitespace and comments.</summary>
    public static void SaveWsc(JsonValue json, TextWriter textWriter)
    {
      if (textWriter==null) throw new ArgumentNullException("textWriter");
      new HjsonWriter() { WriteWsc=true }.Save(json, textWriter, 0, false, "");
      textWriter.Flush();
    }

    /// <summary>Saves Hjson to a string.</summary>
    [Obsolete("Use json.ToString(Stringify.Hjson);")]
    public static string SaveAsString(JsonValue json)
    {
      return json.ToString(Stringify.Hjson);
    }
  }
}
