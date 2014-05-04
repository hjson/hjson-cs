using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Hjson
{
  public class JsonPrimitive : JsonValue
  {
    object value;

    public JsonPrimitive(string value) { this.value=value; }
    public JsonPrimitive(char value) { this.value=value.ToString(); }
    public JsonPrimitive(bool value) { this.value=value; }
    public JsonPrimitive(decimal value) { this.value=value; }
    public JsonPrimitive(double value) { this.value=value; }
    public JsonPrimitive(float value) { this.value=(double)value; }
    public JsonPrimitive(long value) { this.value=value; }
    public JsonPrimitive(int value) { this.value=(long)value; }
    public JsonPrimitive(byte value) { this.value=(long)value; }
    public JsonPrimitive(sbyte value) { this.value=(long)value; }
    public JsonPrimitive(short value) { this.value=(long)value; }
    public JsonPrimitive(uint value) { this.value=(long)value; }
    public JsonPrimitive(ulong value) { this.value=(long)value; }
    public JsonPrimitive(ushort value) { this.value=(long)value; }

    internal object Value
    {
      get { return value; }
    }

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
