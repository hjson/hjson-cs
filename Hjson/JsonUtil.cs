using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Hjson;

/// <summary>Provides Json extension methods.</summary>
public static class JsonUtil
{
    static Exception failQ(JsonValue forObject, string op) =>
        new($"JsonUtil.{op} not supported for type {(forObject != null ? forObject.JsonType.ToString().ToLower() : "null")}!");

    static Exception failM(Exception e, string key) =>
        new($"{(e.Message.EndsWith('!') ? e.Message[..^1] : e.Message)} [key:{key}]!");

    /// <summary>For JsonValues with type boolean, this method will return its
    /// value as bool, otherwise it will throw.</summary>
    public static bool Qb(this JsonValue json) =>
        json != null && json.JsonType == JsonType.Boolean ? (bool)json.ToValue() : throw failQ(json, "Qb");

    /// <summary>Gets the value of the member specified by key, then calls <see cref="Qb(Hjson.JsonValue)"/>.
    /// If the object does not contain the key, the defaultValue is returned.</summary>
    public static bool Qb(this JsonObject json, string key, bool defaultValue = false)
    {
        try { return json.ContainsKey(key) ? json[key].Qb() : defaultValue; }
        catch (Exception e) { throw failM(e, key); }
    }

    /// <summary>For JsonValues with type number, this method will return its
    /// value as int, otherwise it will throw.</summary>
    public static int Qi(this JsonValue json) =>
        json != null && json.JsonType == JsonType.Number ? Convert.ToInt32(json.ToValue()) : throw failQ(json, "Qi");

    /// <summary>Gets the value of the member specified by key, then calls <see cref="Qi(Hjson.JsonValue)"/>.
    /// If the object does not contain the key, the defaultValue is returned.</summary>
    public static int Qi(this JsonObject json, string key, int defaultValue = 0)
    {
        try { return json.ContainsKey(key) ? json[key].Qi() : defaultValue; }
        catch (Exception e) { throw failM(e, key); }
    }

    /// <summary>For JsonValues with type number, this method will return its
    /// value as long, otherwise it will throw.</summary>
    public static long Ql(this JsonValue json) =>
        json != null && json.JsonType == JsonType.Number ? Convert.ToInt64(json.ToValue()) : throw failQ(json, "Ql");

    /// <summary>Gets the value of the member specified by key, then calls <see cref="Ql(Hjson.JsonValue)"/>.
    /// If the object does not contain the key, the defaultValue is returned.</summary>
    public static long Ql(this JsonObject json, string key, long defaultValue = 0)
    {
        try { return json.ContainsKey(key) ? json[key].Ql() : defaultValue; }
        catch (Exception e) { throw failM(e, key); }
    }

    /// <summary>For JsonValues with type number, this method will return its
    /// value as double, otherwise it will throw.</summary>
    public static double Qd(this JsonValue json) =>
        json != null && json.JsonType == JsonType.Number ? Convert.ToDouble(json.ToValue()) : throw failQ(json, "Qd");

    /// <summary>Gets the value of the member specified by key, then calls <see cref="Qd(Hjson.JsonValue)"/>.
    /// If the object does not contain the key, the defaultValue is returned.</summary>
    public static double Qd(this JsonObject json, string key, double defaultValue = 0)
    {
        try { return json.ContainsKey(key) ? json[key].Qd() : defaultValue; }
        catch (Exception e) { throw failM(e, key); }
    }

    /// <summary>For JsonValues with type string, this method will return its
    /// value as string, otherwise it will throw. Use <see cref="Qstr(Hjson.JsonValue)"/>
    /// to get a string value from number or boolean types as well.</summary>
    public static string Qs(this JsonValue json) =>
        json?.JsonType == JsonType.String ? (string)json : json == null ? null : throw failQ(json, "Qs");

    /// <summary>Gets the value of the member specified by key, then calls <see cref="Qs(Hjson.JsonValue)"/>.
    /// If the object does not contain the key, the defaultValue is returned.</summary>
    public static string Qs(this JsonObject json, string key, string defaultValue = "")
    {
        try { return json.ContainsKey(key) ? json[key].Qs() : defaultValue; }
        catch (Exception e) { throw failM(e, key); }
    }

    /// <summary>For JsonValues with type string, number or boolean, this method will return
    /// its value as a string (converted if necessary). For arrays or objects it will throw.</summary>
    public static string Qstr(this JsonValue json) => json?.JsonType switch
    {
        null => null,
        JsonType.String => (string)json,
        JsonType.Boolean or JsonType.Number => json.ToString(),
        _ => throw failQ(json, "Qstr")
    };

    /// <summary>Gets the value of the member specified by key, then,
    /// for string, number or boolean JsonValues, this method will return
    /// its value as a string (converted if necessary).</summary>
    public static string Qstr(this JsonObject json, string key, string defaultValue = "")
    {
        try { return json.ContainsKey(key) ? json[key].Qstr() : defaultValue; }
        catch (Exception e) { throw failM(e, key); }
    }

    /// <summary>Gets the JsonValue of the member specified by key.</summary>
    public static JsonValue Qv(this JsonObject json, string key) =>
        json.TryGetValue(key, out var val) ? val : null;

    /// <summary>Gets a JsonObject from a JsonObject.</summary>
    public static JsonObject Qo(this JsonObject json, string key)
    {
        try { return (JsonObject)json.Qv(key); }
        catch (Exception e) { throw failM(e, key); }
    }

    /// <summary>Gets the JsonObject from a JsonValue.</summary>
    public static JsonObject Qo(this JsonValue json)
    {
        try { return (JsonObject)json; }
        catch { throw failQ(json, "Qo"); }
    }

    /// <summary>Gets a JsonArray from a JsonObject.</summary>
    public static JsonArray Qa(this JsonObject json, string key)
    {
        try { return (JsonArray)json.Qv(key); }
        catch (Exception e) { throw failM(e, key); }
    }

    /// <summary>Gets the JsonArray from a JsonValue.</summary>
    public static JsonArray Qa(this JsonValue json)
    {
        try { return (JsonArray)json; }
        catch { throw failQ(json, "Qa"); }
    }

    /// <summary>Enumerates JsonObjects from a JsonObject.</summary>
    public static IEnumerable<KeyValuePair<string, JsonObject>> Qqo(this JsonObject json) =>
        json.Select(x => new KeyValuePair<string, JsonObject>(x.Key, x.Value.Qo()));

    static readonly DateTime UnixEpochUtc = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>Convert the date to json (unix epoch date offset).</summary>
    public static long ToJsonDate(this DateTime dt) =>
        dt == DateTime.MinValue ? 0 : (long)(dt.ToUniversalTime() - UnixEpochUtc).TotalMilliseconds;

    /// <summary>Convert the json date (unix epoch date offset) to a DateTime.</summary>
    public static DateTime ToDateTime(long unixEpochDateOffset) =>
        unixEpochDateOffset > 0 ? UnixEpochUtc.AddMilliseconds(unixEpochDateOffset) : DateTime.MinValue;

    /// <summary>Convert the date to JSON/ISO 8601, compatible with ES5 Date.toJSON().</summary>
    /// <remarks>Use DateTime.Parse() to convert back (will be of local kind).</remarks>
    public static string ToJson(this DateTime dt) => dt == DateTime.MinValue ? "" :
        dt.Kind == DateTimeKind.Unspecified ? dt.ToString("yyyy-MM-ddTHH:mm:ss.fff") :
        dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

    /// <summary>Convert the date to a precise string representations (ten millionths of a second).</summary>
    /// <remarks>Use DateTime.Parse() to convert back (will be of local kind).</remarks>
    public static string ToPrecise(this DateTime dt) => dt == DateTime.MinValue ? "" :
        dt.Kind == DateTimeKind.Unspecified ? dt.ToString("yyyy-MM-ddTHH:mm:ss.fffffff") :
        dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");

    /// <summary>Convert the timespan to JSON/ISO 8601.</summary>
    public static string ToJson(this TimeSpan ts)
    {
        StringBuilder rc = new(), rct = new();
        if (ts < TimeSpan.Zero) { rc.Append('-'); ts = ts.Negate(); }
        rc.Append('P');
        if (ts.Days > 0) rc.Append($"{ts.Days.ToString(CultureInfo.InvariantCulture)}D");
        if (ts.Hours > 0) rct.Append($"{ts.Hours.ToString(CultureInfo.InvariantCulture)}H");
        if (ts.Minutes > 0) rct.Append($"{ts.Minutes.ToString(CultureInfo.InvariantCulture)}M");
        if (ts.Seconds > 0 || ts.Milliseconds > 0)
        {
            rct.Append(ts.Seconds.ToString(CultureInfo.InvariantCulture));
            if (ts.Milliseconds > 0) rct.Append($".{ts.Milliseconds.ToString(CultureInfo.InvariantCulture)}");
            rct.Append('S');
        }
        if (rct.Length > 0) { rc.Append('T'); rc.Append(rct.ToString()); }
        return rc.ToString();
    }
}