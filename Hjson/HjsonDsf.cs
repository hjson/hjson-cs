using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Hjson;

/// <summary>
/// A interface to support Domain Specific Formats for Hjson.
/// </summary>
public interface IHjsonDsfProvider
{
    /// <summary>Gets the name of this DSF.</summary>
    string Name { get; }
    /// <summary>Gets the description of this DSF.</summary>
    string Description { get; }
    /// <summary>Tries to parse the text as a DSF value.</summary>
    JsonValue Parse(string text);
    /// <summary>Stringifies DSF values.</summary>
    string Stringify(JsonValue value);
}

/// <summary>
/// Provides standard DSF providers.
/// </summary>
public static class HjsonDsf
{
    /// <summary>Returns a math DSF provider.</summary>
    public static IHjsonDsfProvider Math() { return new DsfMath(); }
    /// <summary>Returns a hex DSF provider.</summary>
    public static IHjsonDsfProvider Hex(bool stringify) { return new DsfHex(stringify); }
    /// <summary>Returns a date DSF provider.</summary>
    public static IHjsonDsfProvider Date() { return new DsfDate(); }

    static bool isInvalidDsfChar(char c)
    {
        return c == '{' || c == '}' || c == '[' || c == ']' || c == ',';
    }

    internal static JsonValue Parse(IEnumerable<IHjsonDsfProvider> dsfProviders, string value)
    {
        foreach (var dsf in dsfProviders)
        {
            try
            {
                var res = dsf.Parse(value);
                if (res != null) return res;
            }
            catch (Exception e)
            {
                throw new Exception("DSF-" + dsf.Name + " failed; " + e.Message, e);
            }
        }
        return value;
    }

    internal static string Stringify(IEnumerable<IHjsonDsfProvider> dsfProviders, JsonValue value)
    {
        foreach (var dsf in dsfProviders)
        {
            try
            {
                var text = dsf.Stringify(value);
                if (text != null)
                {
                    if (text.Length == 0 || text[0] == '"')
                        throw new Exception("value may not be empty or start with a quote: " + text);
                    foreach (char c in text)
                    {
                        if (isInvalidDsfChar(c))
                            throw new Exception("value may not contain a punctuator character except colon: " + text);
                    }
                    return text;
                }
            }
            catch (Exception e)
            {
                throw new Exception("DSF-" + dsf.Name + " failed; " + e.Message, e);
            }
        }
        return null;
    }
}


class DsfMath : IHjsonDsfProvider
{
    public string Name => "math";
    public string Description => "support for Inf/inf, -Inf/-inf, Nan/naN and -0";

    static readonly long NegativeZeroBits = BitConverter.DoubleToInt64Bits(-0.0);

    static bool isNegativeZero(double x)
    {
        return BitConverter.DoubleToInt64Bits(x) == NegativeZeroBits;
    }

    public JsonValue Parse(string text)
    {
        switch (text)
        {
            case "+inf":
            case "inf":
            case "+Inf":
            case "Inf":
                return double.PositiveInfinity;
            case "-inf":
            case "-Inf":
                return double.NegativeInfinity;
            case "nan":
            case "NaN":
                return double.NaN;
            default:
                return null;
        }
    }

    public string Stringify(JsonValue value)
    {
        if (value.JsonType != JsonType.Number) return null;
        var val = value.Qd();
        if (double.IsPositiveInfinity(val)) return "Inf";
        else if (double.IsNegativeInfinity(val)) return "-Inf";
        else if (double.IsNaN(val)) return "NaN";
        else if (isNegativeZero(val)) return "-0";
        else return null;
    }
}

class DsfHex(bool stringify) : IHjsonDsfProvider
{
    static readonly Regex isHex = new(@"^0x[0-9A-Fa-f]+$");

    public string Name => "hex";
    public string Description => "parse hexadecimal numbers prefixed with 0x";

    public JsonValue Parse(string text)
    {
        if (isHex.IsMatch(text))
            return long.Parse(text.AsSpan(2), NumberStyles.HexNumber);
        else
            return null;
    }

    public string Stringify(JsonValue value)
    {
        if (stringify &&
          value.JsonType == JsonType.Number &&
          value.Ql() == value.Qd())
        {
            return "0x" + value.Ql().ToString("x");
        }
        else
        {
            return null;
        }
    }
}

class DsfDate : IHjsonDsfProvider
{
    static readonly Regex isDate = new(@"^\d{4}-\d{2}-\d{2}$");
    static readonly Regex isDateTime = new(@"^\d{4}-\d{2}-\d{2}T\d{2}\:\d{2}\:\d{2}(?:.\d+)(?:Z|[+-]\d{2}:\d{2})$");

    public string Name => "date";
    public string Description => "support ISO dates";

    public JsonValue Parse(string text)
    {
        if (isDate.IsMatch(text) || isDateTime.IsMatch(text))
            return JsonValue.FromObject(DateTime.Parse(text));
        else
            return null;
    }

    public string Stringify(JsonValue value)
    {
        if (value.JsonType == JsonType.Unknown && value.ToValue().GetType() == typeof(DateTime))
        {
            var dt = (DateTime)value.ToValue();
            return dt.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
        else
        {
            return null;
        }
    }
}