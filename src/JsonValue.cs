using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Hjson
{
  using JsonPair=KeyValuePair<string, JsonValue>;

  /// <summary>The ToString format.</summary>
  public enum Stringify
  {
    /// <summary>JSON (no whitespace).</summary>
    Plain,
    /// <summary>Formatted JSON.</summary>
    Formatted,
    /// <summary>Hjson.</summary>
    Hjson,
  }

  /// <summary>
  /// JsonValue is the abstract base class for all values (string, number, true, false, null, object or array).
  /// </summary>
  public abstract class JsonValue : IEnumerable
  {
    internal static string eol=Environment.NewLine;

    /// <summary>Gets or sets the newline charater(s).</summary>
    /// <remarks>Defaults to Environment.NewLine.</remarks>
    public static string Eol
    {
      get { return eol; }
      set { if (value=="\r\n" || value=="\n") eol=value; }
    }

    /// <summary>Gets the count of the contained items for arrays/objects.</summary>
    public virtual int Count
    {
      get { throw new InvalidOperationException(); }
    }

    /// <summary>The type of this value.</summary>
    public abstract JsonType JsonType { get; }

    /// <summary>Gets or sets the value for the specified index.</summary>
    public virtual JsonValue this[int index]
    {
      get { throw new InvalidOperationException(); }
      set { throw new InvalidOperationException(); }
    }

    /// <summary>Gets or sets the value for the specified key.</summary>
    public virtual JsonValue this[string key]
    {
      get { throw new InvalidOperationException(); }
      set { throw new InvalidOperationException(); }
    }

    /// <summary>Returns true if the object contains the specified key.</summary>
    public virtual bool ContainsKey(string key)
    {
      throw new InvalidOperationException();
    }

    /// <summary>Saves the JSON to a file.</summary>
    public void Save(string path, Stringify format=Stringify.Plain)
    {
      using (var s=File.CreateText(path))
        Save(s, format);
    }

    /// <summary>Saves the JSON to a stream.</summary>
    public void Save(Stream stream, Stringify format=Stringify.Plain)
    {
      if (stream==null) throw new ArgumentNullException("stream");
      Save(new StreamWriter(stream), format);
    }

    /// <summary>Saves the JSON to a TextWriter.</summary>
    public void Save(TextWriter textWriter, Stringify format=Stringify.Plain)
    {
      if (textWriter==null) throw new ArgumentNullException("textWriter");
      if (format==Stringify.Hjson) HjsonValue.Save(this, textWriter);
      else new JsonWriter(format==Stringify.Formatted).Save(this, textWriter, 0);
      textWriter.Flush();
    }

    /// <summary>Saves as Hjson to a string.</summary>
    public string ToString(HjsonOptions options)
    {
      if (options==null) throw new ArgumentNullException("options");
      var sw=new StringWriter();
      HjsonValue.Save(this, sw, options);
      return sw.ToString();
    }

    /// <summary>Saves the JSON to a string.</summary>
    public string ToString(Stringify format)
    {
      var sw=new StringWriter();
      Save(sw, format);
      return sw.ToString();
    }

    /// <summary>Saves the JSON to a string.</summary>
    public override string ToString()
    {
      return ToString(Stringify.Plain);
    }

    /// <summary>Returns the contained primitive value.</summary>
    public object ToValue()
    {
      return ((JsonPrimitive)this).Value;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      throw new InvalidOperationException();
    }

    /// <summary>Loads JSON from a file.</summary>
    public static JsonValue Load(string path)
    {
      using (var s=File.OpenRead(path))
        return Load(s);
    }

    /// <summary>Loads JSON from a stream.</summary>
    public static JsonValue Load(Stream stream)
    {
      if (stream==null) throw new ArgumentNullException("stream");
      return Load(new StreamReader(stream, true));
    }

    /// <summary>Loads JSON from a TextReader.</summary>
    public static JsonValue Load(TextReader textReader, IJsonReader jsonReader=null)
    {
      if (textReader==null) throw new ArgumentNullException("textReader");
      var ret=new JsonReader(textReader, jsonReader).Read();
      return ret;
    }

    /// <summary>Parses the specified JSON string.</summary>
    public static JsonValue Parse(string jsonString)
    {
      if (jsonString==null)
        throw new ArgumentNullException("jsonString");
      return Load(new StringReader(jsonString));
    }

    // CLI -> JsonValue

    /// <summary>Converts from bool.</summary>
    public static implicit operator JsonValue(bool value) { return new JsonPrimitive(value); }
    /// <summary>Converts from byte.</summary>
    public static implicit operator JsonValue(byte value) { return new JsonPrimitive(value); }
    /// <summary>Converts from char.</summary>
    public static implicit operator JsonValue(char value) { return new JsonPrimitive(value); }
    /// <summary>Converts from decimal.</summary>
    public static implicit operator JsonValue(decimal value) { return new JsonPrimitive(value); }
    /// <summary>Converts from double.</summary>
    public static implicit operator JsonValue(double value) { return new JsonPrimitive(value); }
    /// <summary>Converts from float.</summary>
    public static implicit operator JsonValue(float value) { return new JsonPrimitive(value); }
    /// <summary>Converts from int.</summary>
    public static implicit operator JsonValue(int value) { return new JsonPrimitive(value); }
    /// <summary>Converts from long.</summary>
    public static implicit operator JsonValue(long value) { return new JsonPrimitive(value); }
    /// <summary>Converts from short.</summary>
    public static implicit operator JsonValue(short value) { return new JsonPrimitive(value); }
    /// <summary>Converts from string.</summary>
    public static implicit operator JsonValue(string value) { return new JsonPrimitive(value); }

    // JsonValue -> CLI

    /// <summary>Converts to bool. Also see <see cref="JsonUtil"/>.</summary>
    public static implicit operator bool(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToBoolean(((JsonPrimitive)value).Value);
    }

    /// <summary>Converts to byte. Also see <see cref="JsonUtil"/>.</summary>
    public static implicit operator byte(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToByte(((JsonPrimitive)value).Value);
    }

    /// <summary>Converts to char. Also see <see cref="JsonUtil"/>.</summary>
    public static implicit operator char(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToChar(((JsonPrimitive)value).Value);
    }

    /// <summary>Converts to decimal. Also see <see cref="JsonUtil"/>.</summary>
    public static implicit operator decimal(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToDecimal(((JsonPrimitive)value).Value);
    }

    /// <summary>Converts to double. Also see <see cref="JsonUtil"/>.</summary>
    public static implicit operator double(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToDouble(((JsonPrimitive)value).Value);
    }

    /// <summary>Converts to float. Also see <see cref="JsonUtil"/>.</summary>
    public static implicit operator float(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToSingle(((JsonPrimitive)value).Value);
    }

    /// <summary>Converts to int. Also see <see cref="JsonUtil"/>.</summary>
    public static implicit operator int(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToInt32(((JsonPrimitive)value).Value);
    }

    /// <summary>Converts to long. Also see <see cref="JsonUtil"/>.</summary>
    public static implicit operator long(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToInt64(((JsonPrimitive)value).Value);
    }

    /// <summary>Converts to short. Also see <see cref="JsonUtil"/>.</summary>
    public static implicit operator short(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToInt16(((JsonPrimitive)value).Value);
    }

    /// <summary>Converts to string. Also see <see cref="JsonUtil"/>.</summary>
    public static implicit operator string(JsonValue value)
    {
      if (value==null) return null;
      return (string)((JsonPrimitive)value).Value;
    }
  }
}
