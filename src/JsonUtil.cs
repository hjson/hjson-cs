using System;
using System.Collections.Generic;
using System.Linq;

namespace Hjson
{
  /// <summary>Provides Json extension methods.</summary>
  public static class JsonUtil
  {
    /// <summary>Gets the bool from a JsonValue.</summary>
    public static bool Qb(this JsonValue json)
    {
      if (json!=null && json.JsonType==JsonType.Boolean) return (bool)json.ToValue();
      else throw new Exception("Missing JsonType.Boolean!");
    }

    /// <summary>Gets the bool value of a key in a JsonObject.</summary>
    public static bool Qb(this JsonObject json, string key, bool defaultValue=false)
    {
      if (json.ContainsKey(key)) return json[key].Qb();
      else return defaultValue;
    }

    /// <summary>Gets the long from a JsonValue.</summary>
    public static long Ql(this JsonValue json)
    {
      if (json!=null && json.JsonType==JsonType.Number) return Convert.ToInt64(json.ToValue());
      else throw new Exception("Missing JsonType.Number!");
    }

    /// <summary>Gets the long value of a key in a JsonObject.</summary>
    public static long Ql(this JsonObject json, string key, long defaultValue=0)
    {
      if (json.ContainsKey(key)) return json[key].Ql();
      else return defaultValue;
    }

    /// <summary>Gets the double from a JsonValue.</summary>
    public static double Qd(this JsonValue json)
    {
      if (json!=null && json.JsonType==JsonType.Number) return Convert.ToDouble(json.ToValue());
      else throw new Exception("Missing JsonType.Number!");
    }

    /// <summary>Gets the double value of a key in a JsonObject.</summary>
    public static double Qd(this JsonObject json, string key, double defaultValue=0)
    {
      if (json.ContainsKey(key)) return json[key].Qd();
      else return defaultValue;
    }

    /// <summary>Gets the string from a JsonValue.</summary>
    public static string Qs(this JsonValue json)
    {
      if (json==null) return null;
      else if (json.JsonType==JsonType.String) return (string)json;
      else throw new Exception("Missing JsonType.String!");
    }

    /// <summary>Gets the string value of a key in a JsonObject.</summary>
    public static string Qs(this JsonObject json, string key, string defaultValue="")
    {
      if (json.ContainsKey(key)) return json[key].Qs();
      else return defaultValue;
    }

    /// <summary>Gets the JsonValue of a key in a JsonObject.</summary>
    public static JsonValue Qv(this JsonObject json, string key)
    {
      if (json.ContainsKey(key)) return json[key];
      else return null;
    }

    /// <summary>Gets a JsonObject from a JsonObject.</summary>
    public static JsonObject Qo(this JsonObject json, string key)
    {
      return (JsonObject)json.Qv(key);
    }

    /// <summary>Gets the JsonObject from a JsonValue.</summary>
    public static JsonObject Qo(this JsonValue json)
    {
      return (JsonObject)json;
    }

    /// <summary>Gets a JsonArray from a JsonObject.</summary>
    public static JsonArray Qa(this JsonObject json, string key)
    {
      return (JsonArray)json.Qv(key);
    }

    /// <summary>Gets the JsonArray from a JsonValue.</summary>
    public static JsonArray Qa(this JsonValue json)
    {
      return (JsonArray)json;
    }

    /// <summary>Enumerates JsonObjects from a JsonObject.</summary>
    public static IEnumerable<KeyValuePair<string, JsonObject>> Qqo(this JsonObject json)
    {
      return json.Select(x => new KeyValuePair<string, JsonObject>(x.Key, (JsonObject)x.Value));
    }
  }
}
