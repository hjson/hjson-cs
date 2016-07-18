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
      return load(path, null, false);
    }

    /// <summary>Loads Hjson/JSON from a file, optionally preserving whitespace and comments.</summary>
    public static JsonValue Load(string path, bool preserveComments)
    {
      return load(path, null, preserveComments);
    }

    /// <summary>Loads Hjson/JSON from a stream.</summary>
    public static JsonValue Load(Stream stream)
    {
      return load(stream, null, false);
    }

    /// <summary>Loads Hjson/JSON from a stream, optionally preserving whitespace and comments.</summary>
    public static JsonValue Load(Stream stream, bool preserveComments)
    {
      return load(stream, null, preserveComments);
    }

    /// <summary>Loads Hjson/JSON from a TextReader.</summary>
    public static JsonValue Load(TextReader textReader, IJsonReader jsonReader=null)
    {
      return load(textReader, jsonReader, false);
    }

    /// <summary>Loads Hjson/JSON from a TextReader, optionally preserving whitespace and comments.</summary>
    public static JsonValue Load(TextReader textReader, bool preserveComments, IJsonReader jsonReader=null)
    {
      return load(textReader, jsonReader, preserveComments);
    }

    /// <summary>Loads Hjson/JSON from a TextReader, preserving whitespace and comments.</summary>
    [Obsolete("Use Load")]
    public static JsonValue LoadWsc(TextReader textReader)
    {
      return load(textReader, null, true);
    }

    static JsonValue load(string path, IJsonReader jsonReader, bool wsc)
    {
      if (Path.GetExtension(path).ToLower()==".json") return JsonValue.Load(path);
      try
      {
        using (var s=File.OpenRead(path))
          return load(s, jsonReader, wsc);
      }
      catch (Exception e) { throw new Exception(e.Message+" (in "+path+")", e); }
    }

    static JsonValue load(Stream stream, IJsonReader jsonReader, bool wsc)
    {
      if (stream==null) throw new ArgumentNullException("stream");
      return load(new StreamReader(stream, true), jsonReader, wsc);
    }

    static JsonValue load(TextReader textReader, IJsonReader jsonReader, bool wsc)
    {
      if (textReader==null) throw new ArgumentNullException("textReader");
      return new HjsonReader(textReader, jsonReader) { ReadWsc=wsc }.Read();
    }

    /// <summary>Parses the specified Hjson/JSON string.</summary>
    public static JsonValue Parse(string hjsonString)
    {
      if (hjsonString==null) throw new ArgumentNullException("hjsonString");
      return Load(new StringReader(hjsonString));
    }

        /// <summary>Parses the specified Hjson/JSON string, optionally preserving whitespace and comments.</summary>
    public static JsonValue Parse(string hjsonString, bool preserveComments)
    {
      if (hjsonString==null) throw new ArgumentNullException("hjsonString");
      return Load(new StringReader(hjsonString), preserveComments);
    }

    /// <summary>Saves Hjson to a file.</summary>
    public static void Save(JsonValue json, string path, HjsonOptions options=null)
    {
      if (Path.GetExtension(path).ToLower()==".json") { json.Save(path, Stringify.Formatted); return; }
      using (var s=File.CreateText(path))
        Save(json, s, options);
    }

    /// <summary>Saves Hjson to a stream.</summary>
    public static void Save(JsonValue json, Stream stream, HjsonOptions options=null)
    {
      if (stream==null) throw new ArgumentNullException("stream");
      Save(json, new StreamWriter(stream), options);
    }

    /// <summary>Saves Hjson to a TextWriter.</summary>
    public static void Save(JsonValue json, TextWriter textWriter, HjsonOptions options=null)
    {
      if (textWriter==null) throw new ArgumentNullException("textWriter");
      new HjsonWriter(options).Save(json, textWriter, 0, false, "", true, true);
      textWriter.Flush();
    }
  }
}
