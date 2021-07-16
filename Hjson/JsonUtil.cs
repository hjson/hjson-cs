using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Hjson
{
	/// <summary>Provides Json extension methods.</summary>
	public static class JsonUtil
	{
		private static Exception failQ(JsonValue forObject, string op)
		{
			var type = forObject != null ? forObject.JsonType.ToString().ToLower() : "null";
			return new Exception("JsonUtil." + op + " not supported for type " + type + "!");
		}

		private static Exception failM(Exception e, string key)
		{
			var msg = e.Message;
			if (msg.EndsWith("!")) msg = msg.Substring(0, msg.Length - 1);
			return new Exception(msg + " [key:" + key + "]!");
		}

		/// <summary>For JsonValues with type boolean, this method will return its
		/// value as bool, otherwise it will throw.</summary>
		public static bool Qb(this JsonValue json)
		{
			if (json != null && json.JsonType == JsonType.Boolean) return (bool)json.ToValue();
			else throw failQ(json, "Qb");
		}

		/// <summary>Gets the value of the member specified by key, then calls <see cref="Qb(Hjson.JsonValue)"/>.
		/// If the object does not contain the key, the defaultValue is returned.</summary>
		public static bool Qb(this JsonObject json, string key, bool defaultValue = false)
		{
			try
			{
				if (json.ContainsKey(key)) return json[key].Qb();
				else return defaultValue;
			}
			catch (Exception e) { throw failM(e, key); }
		}

		/// <summary>For JsonValues with type number, this method will return its
		/// value as int, otherwise it will throw.</summary>
		public static int Qi(this JsonValue json)
		{
			if (json != null && json.JsonType == JsonType.Number) return Convert.ToInt32(json.ToValue());
			else throw failQ(json, "Qi");
		}

		/// <summary>For JsonValues with type number, this method will return its
		/// value as int, otherwise it will throw.</summary>
		public static uint Qui(this JsonValue json)
		{
			if (json != null && json.JsonType == JsonType.Number) return Convert.ToUInt32(json.ToValue());
			else throw failQ(json, "Qi");
		}

		/// <summary>Gets the value of the member specified by key, then calls <see cref="Qi(Hjson.JsonValue)"/>.
		/// If the object does not contain the key, the defaultValue is returned.</summary>
		public static int Qi(this JsonObject json, string key, int defaultValue = 0)
		{
			try
			{
				if (json.ContainsKey(key)) return json[key].Qi();
				else return defaultValue;
			}
			catch (Exception e) { throw failM(e, key); }
		}

		/// <summary>For JsonValues with type number, this method will return its
		/// value as long, otherwise it will throw.</summary>
		public static long Ql(this JsonValue json)
		{
			if (json != null && json.JsonType == JsonType.Number) return Convert.ToInt64(json.ToValue());
			else throw failQ(json, "Ql");
		}

		/// <summary>For JsonValues with type number, this method will return its
		/// value as long, otherwise it will throw.</summary>
		public static ulong Qul(this JsonValue json)
		{
			unchecked
			{
				if (json != null && json.JsonType == JsonType.Number) return (ulong)Convert.ToUInt64(json.ToValue());
				else throw failQ(json, "Ql");
			}
		}

		/// <summary>Gets the value of the member specified by key, then calls <see cref="Ql(Hjson.JsonValue)"/>.
		/// If the object does not contain the key, the defaultValue is returned.</summary>
		public static long Ql(this JsonObject json, string key, long defaultValue = 0)
		{
			try
			{
				if (json.ContainsKey(key)) return json[key].Ql();
				else return defaultValue;
			}
			catch (Exception e) { throw failM(e, key); }
		}

		/// <summary>For JsonValues with type number, this method will return its
		/// value as double, otherwise it will throw.</summary>
		public static double Qd(this JsonValue json)
		{
			if (json != null && json.JsonType == JsonType.Number) return Convert.ToDouble(json.ToValue());
			else throw failQ(json, "Qd");
		}

		/// <summary>Gets the value of the member specified by key, then calls <see cref="Qd(Hjson.JsonValue)"/>.
		/// If the object does not contain the key, the defaultValue is returned.</summary>
		public static double Qd(this JsonObject json, string key, double defaultValue = 0)
		{
			try
			{
				if (json.ContainsKey(key)) return json[key].Qd();
				else return defaultValue;
			}
			catch (Exception e) { throw failM(e, key); }
		}

		/// <summary>For JsonValues with type string, this method will return its
		/// value as string, otherwise it will throw. Use <see cref="Qstr(Hjson.JsonValue)"/>
		/// to get a string value from number or boolean types as well.</summary>
		public static string Qs(this JsonValue json)
		{
			if (json == null) return null;
			else if (json.JsonType == JsonType.String) return json;
			else throw failQ(json, "Qs");
		}

		/// <summary>Gets the value of the member specified by key, then calls <see cref="Qs(Hjson.JsonValue)"/>.
		/// If the object does not contain the key, the defaultValue is returned.</summary>
		public static string Qs(this JsonObject json, string key, string defaultValue = "")
		{
			try
			{
				if (json.ContainsKey(key)) return json[key].Qs();
				else return defaultValue;
			}
			catch (Exception e) { throw failM(e, key); }
		}

		/// <summary>For JsonValues with type string, number or boolean, this method will return
		/// its value as a string (converted if necessary). For arrays or objects it will throw.</summary>
		public static string Qstr(this JsonValue json)
		{
			if (json == null) return null;
			else if (json.JsonType == JsonType.String) return json;
			else if (json.JsonType == JsonType.Boolean || json.JsonType == JsonType.Number) return json.ToString();
			else throw failQ(json, "Qstr");
		}

		/// <summary>Gets the value of the member specified by key, then,
		/// for string, number or boolean JsonValues, this method will return
		/// its value as a string (converted if necessary).</summary>
		public static string Qstr(this JsonObject json, string key, string defaultValue = "")
		{
			try
			{
				if (json.ContainsKey(key)) return json[key].Qstr();
				else return defaultValue;
			}
			catch (Exception e) { throw failM(e, key); }
		}

		/// <summary>Gets the JsonValue of the member specified by key.</summary>
		public static JsonValue Qv(this JsonObject json, string key)
		{
			if (json.ContainsKey(key)) return json[key];
			else return null;
		}

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
			catch { throw failQ(json, "Qo"); }
		}

		/// <summary>Enumerates JsonObjects from a JsonObject.</summary>
		public static IEnumerable<KeyValuePair<string, JsonObject>> Qqo(this JsonObject json) => json.Select(x => new KeyValuePair<string, JsonObject>(x.Key, x.Value.Qo()));

		private static readonly DateTime UnixEpochUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>Convert the date to json (unix epoch date offset).</summary>
		public static long ToJsonDate(this DateTime dt)
		{
			if (dt == DateTime.MinValue) return 0;
			else return (long)(dt.ToUniversalTime() - UnixEpochUtc).TotalMilliseconds;
		}

		/// <summary>Convert the json date (unix epoch date offset) to a DateTime.</summary>
		public static DateTime ToDateTime(long unixEpochDateOffset)
		{
			if (unixEpochDateOffset > 0) return UnixEpochUtc.AddMilliseconds(unixEpochDateOffset);
			else return DateTime.MinValue;
		}

		/// <summary>Convert the date to JSON/ISO 8601, compatible with ES5 Date.toJSON().</summary>
		/// <remarks>Use DateTime.Parse() to convert back (will be of local kind).</remarks>
		public static string ToJson(this DateTime dt)
		{
			if (dt == DateTime.MinValue) return "";
			else if (dt.Kind == DateTimeKind.Unspecified) return dt.ToString("yyyy-MM-ddTHH:mm:ss.fff");
			else return dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
		}

		/// <summary>Convert the date to a precise string representations (ten millionths of a second).</summary>
		/// <remarks>Use DateTime.Parse() to convert back (will be of local kind).</remarks>
		public static string ToPrecise(this DateTime dt)
		{
			if (dt == DateTime.MinValue) return "";
			else if (dt.Kind == DateTimeKind.Unspecified) return dt.ToString("yyyy-MM-ddTHH:mm:ss.fffffff");
			else return dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
		}

		/// <summary>Convert the timespan to JSON/ISO 8601.</summary>
		public static string ToJson(this TimeSpan ts)
		{
			StringBuilder rc = new StringBuilder(), rct = new StringBuilder();
			if (ts < TimeSpan.Zero) { rc.Append('-'); ts = ts.Negate(); }
			rc.Append('P');
			if (ts.Days > 0) rc.Append(ts.Days.ToString(CultureInfo.InvariantCulture) + 'D');
			if (ts.Hours > 0) rct.Append(ts.Hours.ToString(CultureInfo.InvariantCulture) + 'H');
			if (ts.Minutes > 0) rct.Append(ts.Minutes.ToString(CultureInfo.InvariantCulture) + 'M');
			if (ts.Seconds > 0 || ts.Milliseconds > 0)
			{
				rct.Append(ts.Seconds.ToString(CultureInfo.InvariantCulture));
				if (ts.Milliseconds > 0) rct.Append("." + ts.Milliseconds.ToString(CultureInfo.InvariantCulture));
				rct.Append('S');
			}
			if (rct.Length > 0) { rc.Append('T'); rc.Append(rct.ToString()); }
			return rc.ToString();
		}
	}
}
