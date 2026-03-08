using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Hjson;

/// <summary>Implements a primitive value.</summary>
internal class JsonPrimitive : JsonValue
{
    object value;

    /// <summary>Initializes a new string.</summary>
    public JsonPrimitive(string value) { this.value = value; }
    /// <summary>Initializes a new char.</summary>
    public JsonPrimitive(char value) { this.value = value.ToString(); }
    /// <summary>Initializes a new bool.</summary>
    public JsonPrimitive(bool value) { this.value = value; }
    /// <summary>Initializes a new decimal.</summary>
    public JsonPrimitive(decimal value) { this.value = value; }
    /// <summary>Initializes a new double.</summary>
    public JsonPrimitive(double value) { this.value = value; }
    /// <summary>Initializes a new float.</summary>
    public JsonPrimitive(float value) { this.value = (double)value; }
    /// <summary>Initializes a new long.</summary>
    public JsonPrimitive(long value) { this.value = value; }
    /// <summary>Initializes a new int.</summary>
    public JsonPrimitive(int value) { this.value = (long)value; }
    /// <summary>Initializes a new byte.</summary>
    public JsonPrimitive(byte value) { this.value = (long)value; }
    /// <summary>Initializes a new short.</summary>
    public JsonPrimitive(short value) { this.value = (long)value; }

    JsonPrimitive() { }
    public static new JsonPrimitive FromObject(object value) { return new JsonPrimitive { value = value }; }

    // Using property
    internal object Value => value;

    /// <summary>The type of this value.</summary>
    public override JsonType JsonType
    {
        get
        {
            if (value == null) return JsonType.String;

            var type = value.GetType();
            if (type == typeof(bool)) return JsonType.Boolean;
            if (type == typeof(string)) return JsonType.String;
            if (type == typeof(byte) ||
              type == typeof(sbyte) ||
              type == typeof(short) ||
              type == typeof(ushort) ||
              type == typeof(int) ||
              type == typeof(uint) ||
              type == typeof(long) ||
              type == typeof(ulong) ||
              type == typeof(float) ||
              type == typeof(double) ||
              type == typeof(decimal)) return JsonType.Number;
            return JsonType.Unknown;
        }
    }

    internal string GetRawString()
    {
        return JsonType switch
        {
            JsonType.String => ((string)value) ?? "",
            JsonType.Number => ((IFormattable)value).ToString("G", NumberFormatInfo.InvariantInfo).ToLowerInvariant(),
            _ => throw new InvalidOperationException(),
        };
    }
}
