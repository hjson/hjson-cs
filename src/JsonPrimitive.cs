using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Hjson
{
  /// <summary>Implements an primitive value.</summary>
  public class JsonPrimitive : JsonValue
  {
    object value;

    /// <summary>Initializes a new string.</summary>
    public JsonPrimitive(string value) { this.value=value; }
    /// <summary>Initializes a new char.</summary>
    public JsonPrimitive(char value) { this.value=value.ToString(); }
    /// <summary>Initializes a new bool.</summary>
    public JsonPrimitive(bool value) { this.value=value; }
    /// <summary>Initializes a new decimal.</summary>
    public JsonPrimitive(decimal value) { this.value=value; }
    /// <summary>Initializes a new double.</summary>
    public JsonPrimitive(double value) { this.value=value; }
    /// <summary>Initializes a new float.</summary>
    public JsonPrimitive(float value) { this.value=(double)value; }
    /// <summary>Initializes a new long.</summary>
    public JsonPrimitive(long value) { this.value=value; }
    /// <summary>Initializes a new int.</summary>
    public JsonPrimitive(int value) { this.value=(long)value; }
    /// <summary>Initializes a new byte.</summary>
    public JsonPrimitive(byte value) { this.value=(long)value; }
    /// <summary>Initializes a new short.</summary>
    public JsonPrimitive(short value) { this.value=(long)value; }

    internal object Value
    {
      get { return value; }
    }

    /// <summary>The type of this value.</summary>
    public override JsonType JsonType
    {
      get
      {
        if (value==null) return JsonType.String;

        switch (Type.GetTypeCode(value.GetType()))
        {
          case TypeCode.Boolean: return JsonType.Boolean;
          case TypeCode.String: return JsonType.String;
          default: return JsonType.Number;
        }
      }
    }

    internal string GetFormattedString()
    {
      switch (JsonType)
      {
        case JsonType.String:
          return (string)value;
        case JsonType.Number:
          return ((IFormattable)value).ToString("G", NumberFormatInfo.InvariantInfo);
        default:
          throw new InvalidOperationException();
      }
    }
  }
}
