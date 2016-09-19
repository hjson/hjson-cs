using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Hjson
{
  /// <summary>Implements a primitive value.</summary>
  internal class JsonPrimitive : JsonValue
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

    JsonPrimitive() { }
    public static new JsonPrimitive FromObject(object value) { return new JsonPrimitive { value=value }; }

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
          case TypeCode.Byte:
          case TypeCode.SByte:
          case TypeCode.Int16:
          case TypeCode.UInt16:
          case TypeCode.Int32:
          case TypeCode.UInt32:
          case TypeCode.Int64:
          case TypeCode.UInt64:
          case TypeCode.Single:
          case TypeCode.Double:
          case TypeCode.Decimal: return JsonType.Number;
          default: return JsonType.Unknown;
        }
      }
    }

    internal string GetRawString()
    {
      switch (JsonType)
      {
        case JsonType.String:
          return ((string)value)??"";
        case JsonType.Number:
#if __MonoCS__ // mono bug ca 2014
          if (value is decimal)
          {
            var res=((IFormattable)value).ToString("G", NumberFormatInfo.InvariantInfo);
            while (res.EndsWith("0")) res=res.Substring(0, res.Length-1);
            if (res.EndsWith(".") || res.EndsWith("e", StringComparison.OrdinalIgnoreCase)) res=res.Substring(0, res.Length-1);
            return res.ToLowerInvariant();
          }
#endif
          // use ToLowerInvariant() to convert E to e
          return ((IFormattable)value).ToString("G", NumberFormatInfo.InvariantInfo).ToLowerInvariant();
        default:
          throw new InvalidOperationException();
      }
    }
  }
}
