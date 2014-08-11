using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Hjson
{
  using JsonPair=KeyValuePair<string, JsonValue>;

  public abstract class JsonValue : IEnumerable
  {
    public virtual int Count
    {
      get { throw new InvalidOperationException(); }
    }

    public abstract JsonType JsonType { get; }

    public virtual JsonValue this[int index]
    {
      get { throw new InvalidOperationException(); }
      set { throw new InvalidOperationException(); }
    }

    public virtual JsonValue this[string key]
    {
      get { throw new InvalidOperationException(); }
      set { throw new InvalidOperationException(); }
    }

    public virtual bool ContainsKey(string key)
    {
      throw new InvalidOperationException();
    }

    public void Save(string path, bool formatted=false)
    {
      using (var s=File.CreateText(path))
        Save(s, formatted);
    }

    public void Save(Stream stream, bool formatted=false)
    {
      if (stream==null) throw new ArgumentNullException("stream");
      Save(new StreamWriter(stream), formatted);
    }

    public void Save(TextWriter textWriter, bool formatted=false)
    {
      if (textWriter==null) throw new ArgumentNullException("textWriter");
      new JsonWriter(formatted).Save(this, textWriter, 0);
      textWriter.Flush();
    }

    public string SaveAsString(bool formatted=false)
    {
      var sw=new StringWriter();
      Save(sw, formatted);
      return sw.ToString();
    }

    public override string ToString()
    {
      StringWriter sw=new StringWriter();
      new JsonWriter(true).Save(this, sw, 0);
      return sw.ToString();
    }

    public object ToValue()
    {
      return ((JsonPrimitive)this).Value;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      throw new InvalidOperationException();
    }

    public static JsonValue Load(string path)
    {
      using (var s=File.OpenRead(path))
        return Load(s);
    }

    public static JsonValue Load(Stream stream)
    {
      if (stream==null) throw new ArgumentNullException("stream");
      return Load(new StreamReader(stream, true));
    }

    public static JsonValue Load(TextReader textReader, IJsonReader jsonReader=null)
    {
      if (textReader==null) throw new ArgumentNullException("textReader");
      var ret=new JsonReader(textReader, jsonReader).Read();
      return ToJsonValue(ret);
    }

    static IEnumerable<JsonPair> ToJsonPairEnumerable(IEnumerable<KeyValuePair<string, object>> kvpc)
    {
      foreach (var kvp in kvpc)
        yield return new JsonPair(kvp.Key, ToJsonValue(kvp.Value));
    }

    static IEnumerable<JsonValue> ToJsonValueEnumerable(IEnumerable<object> arr)
    {
      foreach (var obj in arr)
        yield return ToJsonValue(obj);
    }

    internal static JsonValue ToJsonValue(object ret)
    {
      if (ret==null) return null;
      var kvpc=ret as IEnumerable<KeyValuePair<string, object>>;
      if (kvpc!=null) return new JsonObject(ToJsonPairEnumerable(kvpc));
      var arr=ret as IEnumerable<object>;
      if (arr!=null) return new JsonArray(ToJsonValueEnumerable(arr));
      if (ret is bool) return new JsonPrimitive((bool)ret);
      if (ret is byte) return new JsonPrimitive((byte)ret);
      if (ret is char) return new JsonPrimitive((char)ret);
      if (ret is decimal) return new JsonPrimitive((decimal)ret);
      if (ret is double) return new JsonPrimitive((double)ret);
      if (ret is float) return new JsonPrimitive((float)ret);
      if (ret is int) return new JsonPrimitive((int)ret);
      if (ret is long) return new JsonPrimitive((long)ret);
      if (ret is sbyte) return new JsonPrimitive((sbyte)ret);
      if (ret is short) return new JsonPrimitive((short)ret);
      if (ret is string) return new JsonPrimitive((string)ret);
      if (ret is uint) return new JsonPrimitive((uint)ret);
      if (ret is ulong) return new JsonPrimitive((ulong)ret);
      if (ret is ushort) return new JsonPrimitive((ushort)ret);
      throw new NotSupportedException(String.Format("Unexpected parser return type: {0}", ret.GetType()));
    }

    public static JsonValue Parse(string jsonString)
    {
      if (jsonString==null)
        throw new ArgumentNullException("jsonString");
      return Load(new StringReader(jsonString));
    }

    // CLI -> JsonValue

    public static implicit operator JsonValue(bool value) { return new JsonPrimitive(value); }
    public static implicit operator JsonValue(byte value) { return new JsonPrimitive(value); }
    public static implicit operator JsonValue(char value) { return new JsonPrimitive(value); }
    public static implicit operator JsonValue(decimal value) { return new JsonPrimitive(value); }
    public static implicit operator JsonValue(double value) { return new JsonPrimitive(value); }
    public static implicit operator JsonValue(float value) { return new JsonPrimitive(value); }
    public static implicit operator JsonValue(int value) { return new JsonPrimitive(value); }
    public static implicit operator JsonValue(long value) { return new JsonPrimitive(value); }
    public static implicit operator JsonValue(sbyte value) { return new JsonPrimitive(value); }
    public static implicit operator JsonValue(short value) { return new JsonPrimitive(value); }
    public static implicit operator JsonValue(string value) { return new JsonPrimitive(value); }
    public static implicit operator JsonValue(uint value) { return new JsonPrimitive(value); }
    public static implicit operator JsonValue(ulong value) { return new JsonPrimitive(value); }
    public static implicit operator JsonValue(ushort value) { return new JsonPrimitive(value); }

    // JsonValue -> CLI

    public static implicit operator bool(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToBoolean(((JsonPrimitive)value).Value);
    }

    public static implicit operator byte(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToByte(((JsonPrimitive)value).Value);
    }

    public static implicit operator char(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToChar(((JsonPrimitive)value).Value);
    }

    public static implicit operator decimal(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToDecimal(((JsonPrimitive)value).Value);
    }

    public static implicit operator double(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToDouble(((JsonPrimitive)value).Value);
    }

    public static implicit operator float(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToSingle(((JsonPrimitive)value).Value);
    }

    public static implicit operator int(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToInt32(((JsonPrimitive)value).Value);
    }

    public static implicit operator long(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToInt64(((JsonPrimitive)value).Value);
    }

    public static implicit operator sbyte(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToSByte(((JsonPrimitive)value).Value);
    }

    public static implicit operator short(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToInt16(((JsonPrimitive)value).Value);
    }

    public static implicit operator string(JsonValue value)
    {
      if (value==null) return null;
      return (string)((JsonPrimitive)value).Value;
    }

    public static implicit operator uint(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToUInt16(((JsonPrimitive)value).Value);
    }

    public static implicit operator ulong(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToUInt64(((JsonPrimitive)value).Value);
    }

    public static implicit operator ushort(JsonValue value)
    {
      if (value==null) throw new ArgumentNullException("value");
      return Convert.ToUInt16(((JsonPrimitive)value).Value);
    }
  }
}
