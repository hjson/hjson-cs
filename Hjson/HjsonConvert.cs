using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Hjson;

/// <summary>Provides methods for serializing and deserializing objects to/from Hjson.</summary>
/// <remarks>
/// Supports Hjson-specific attributes (<see cref="HjsonPropertyNameAttribute"/>,
/// <see cref="HjsonIgnoreAttribute"/>, <see cref="HjsonIncludeAttribute"/>,
/// <see cref="HjsonCommentAttribute"/>) with fallback to System.Text.Json attributes
/// (<see cref="JsonPropertyNameAttribute"/>, <see cref="JsonIgnoreAttribute"/>,
/// <see cref="JsonIncludeAttribute"/>).
/// </remarks>
public static class HjsonConvert
{
    /// <summary>Serializes an object to an Hjson string.</summary>
    public static string Serialize(object obj, HjsonOptions options = null)
    {
        var jsonValue = ToJsonValue(obj);
        if (jsonValue == null) return "null";

        using var sw = new StringWriter();
        bool hasComments = HasAnyComments(jsonValue);
        var opts = options ?? new HjsonOptions();
        if (hasComments) opts.KeepWsc = true;
        HjsonValue.Save(jsonValue, sw, opts);
        return sw.ToString();
    }

    /// <summary>Deserializes an Hjson string to an object of type <typeparamref name="T"/>.</summary>
    public static T Deserialize<T>(string hjson, HjsonOptions options = null)
    {
        var jsonValue = options != null
            ? HjsonValue.Parse(hjson, options)
            : HjsonValue.Parse(hjson);
        return (T)FromJsonValue(jsonValue, typeof(T));
    }

    // ── Serialization (Object → JsonValue) ──────────────────────────

    static JsonValue ToJsonValue(object obj)
    {
        if (obj == null) return null;

        var type = obj.GetType();

        // Primitives handled by implicit operators
        if (obj is bool b) return b;
        if (obj is string s) return s;
        if (obj is char c) return new string(c, 1);
        if (obj is int i) return i;
        if (obj is long l) return l;
        if (obj is double d) return d;
        if (obj is float f) return f;
        if (obj is decimal dec) return dec;
        if (obj is byte by) return by;
        if (obj is short sh) return sh;

        // Enum → string
        if (type.IsEnum) return obj.ToString();

        // Note: Nullable<T> is already unboxed to T by the CLR when boxed

        // Dictionary<string, T>
        if (obj is IDictionary dict && type.IsGenericType)
        {
            var keyType = type.GetGenericArguments()[0];
            if (keyType == typeof(string))
            {
                var jsonObj = new JsonObject();
                foreach (DictionaryEntry entry in dict)
                    jsonObj.Add((string)entry.Key, ToJsonValue(entry.Value));
                return jsonObj;
            }
        }

        // Array / List / IEnumerable (but not string)
        if (obj is IEnumerable enumerable)
        {
            var jsonArr = new JsonArray();
            foreach (var item in enumerable)
                jsonArr.Add(ToJsonValue(item));
            return jsonArr;
        }

        // Complex object → reflect
        return ObjectToJsonValue(obj, type);
    }

    static JsonValue ObjectToJsonValue(object obj, Type type)
    {
        var members = GetMembers(type);
        bool hasComments = members.Any(m => m.Comment != null);

        WscJsonObject wscObj = null;
        JsonObject jsonObj;

        if (hasComments)
        {
            wscObj = new WscJsonObject { RootBraces = true };
            jsonObj = wscObj;
            wscObj.Comments[""] = "";
        }
        else
        {
            jsonObj = new JsonObject();
        }

        foreach (var member in members)
        {
            var value = member.GetValue(obj);
            var jsonValue = ToJsonValue(value);
            jsonObj.Add(member.HjsonName, jsonValue);

            if (wscObj != null)
            {
                wscObj.Order.Add(member.HjsonName);
                wscObj.Comments[member.HjsonName] = member.Comment != null
                    ? "\n" + FormatComment(member.Comment)
                    : "";
            }
        }

        return jsonObj;
    }

    static string FormatComment(string comment)
    {
        var lines = comment.Replace("\r\n", "\n").Split('\n');
        return string.Join("\n", lines.Select(line => "# " + line));
    }

    static bool HasAnyComments(JsonValue value) => value is WscJsonObject;

    // ── Deserialization (JsonValue → Object) ─────────────────────────

    static object FromJsonValue(JsonValue value, Type targetType)
    {
        if (value == null)
        {
            if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                return Activator.CreateInstance(targetType);
            return null;
        }

        // Unwrap Nullable<T>
        var underlying = Nullable.GetUnderlyingType(targetType);
        if (underlying != null) targetType = underlying;

        // Primitives
        if (targetType == typeof(bool)) return (bool)value;
        if (targetType == typeof(string)) return (string)value;
        if (targetType == typeof(char))
        {
            var str = (string)value;
            return str.Length > 0 ? str[0] : default;
        }
        if (targetType == typeof(int)) return (int)value;
        if (targetType == typeof(long)) return (long)value;
        if (targetType == typeof(double)) return (double)value;
        if (targetType == typeof(float)) return (float)value;
        if (targetType == typeof(decimal)) return (decimal)value;
        if (targetType == typeof(byte)) return (byte)value;
        if (targetType == typeof(short)) return (short)value;

        // Enum
        if (targetType.IsEnum)
        {
            if (value.JsonType == JsonType.String)
                return Enum.Parse(targetType, (string)value, ignoreCase: true);
            if (value.JsonType == JsonType.Number)
                return Enum.ToObject(targetType, (int)value);
        }

        // JsonValue passthrough — if someone wants the raw value
        if (targetType == typeof(JsonValue) || targetType == typeof(JsonObject) ||
            targetType == typeof(JsonArray) || targetType == typeof(JsonPrimitive))
            return value;

        // Array
        if (targetType.IsArray)
        {
            var elementType = targetType.GetElementType()!;
            var arr = value.Count > 0 ? new object[value.Count] : [];
            for (int i = 0; i < value.Count; i++)
                arr[i] = FromJsonValue(value[i], elementType);
            var typed = Array.CreateInstance(elementType, arr.Length);
            Array.Copy(arr, typed, arr.Length);
            return typed;
        }

        // List<T> / IList<T> / IEnumerable<T> / ICollection<T>
        if (targetType.IsGenericType)
        {
            var genDef = targetType.GetGenericTypeDefinition();
            var genArgs = targetType.GetGenericArguments();

            // Dictionary<string, T>
            if ((genDef == typeof(Dictionary<,>) || genDef == typeof(IDictionary<,>)) &&
                genArgs[0] == typeof(string))
            {
                var dictType = genDef == typeof(IDictionary<,>)
                    ? typeof(Dictionary<,>).MakeGenericType(genArgs)
                    : targetType;
                var dict = (IDictionary)Activator.CreateInstance(dictType)!;
                if (value is JsonObject obj)
                {
                    foreach (var key in obj.Keys)
                        dict[key] = FromJsonValue(obj[key], genArgs[1]);
                }
                return dict;
            }

            // List<T>, IList<T>, ICollection<T>, IEnumerable<T>
            if (genDef == typeof(List<>) || genDef == typeof(IList<>) ||
                genDef == typeof(ICollection<>) || genDef == typeof(IEnumerable<>))
            {
                var listType = (genDef == typeof(List<>))
                    ? targetType
                    : typeof(List<>).MakeGenericType(genArgs);
                var list = (IList)Activator.CreateInstance(listType)!;
                for (int i = 0; i < value.Count; i++)
                    list.Add(FromJsonValue(value[i], genArgs[0]));
                return list;
            }
        }

        // Complex object
        if (value is JsonObject jsonObj)
            return ObjectFromJsonValue(jsonObj, targetType);

        throw new InvalidOperationException($"Cannot convert {value.JsonType} to {targetType.Name}");
    }

    static object ObjectFromJsonValue(JsonObject jsonObj, Type type)
    {
        var instance = Activator.CreateInstance(type)!;
        var members = GetMembers(type);

        foreach (var member in members)
        {
            if (!jsonObj.ContainsKey(member.HjsonName)) continue;
            var jsonValue = jsonObj[member.HjsonName];
            var converted = FromJsonValue(jsonValue, member.MemberType);
            member.SetValue(instance, converted);
        }

        return instance;
    }

    // ── Member reflection ────────────────────────────────────────────

    sealed class MemberDescriptor
    {
        public string HjsonName;
        public string Comment;
        public Type MemberType;
        public Func<object, object> GetValue;
        public Action<object, object> SetValue;
    }

    static List<MemberDescriptor> GetMembers(Type type)
    {
        var result = new List<MemberDescriptor>();
        const BindingFlags allInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // Properties
        foreach (var prop in type.GetProperties(allInstance))
        {
            if (!ShouldInclude(prop, prop.GetMethod?.IsPublic == true || prop.SetMethod?.IsPublic == true))
                continue;
            result.Add(new MemberDescriptor
            {
                HjsonName = GetName(prop),
                Comment = GetComment(prop),
                MemberType = prop.PropertyType,
                GetValue = prop.GetValue,
                SetValue = prop.SetValue,
            });
        }

        // Fields
        foreach (var field in type.GetFields(allInstance))
        {
            if (!ShouldInclude(field, field.IsPublic))
                continue;
            result.Add(new MemberDescriptor
            {
                HjsonName = GetName(field),
                Comment = GetComment(field),
                MemberType = field.FieldType,
                GetValue = field.GetValue,
                SetValue = field.SetValue,
            });
        }

        return result;
    }

    static bool ShouldInclude(MemberInfo member, bool isPublic)
    {
        // Explicit ignore
        if (member.GetCustomAttribute<HjsonIgnoreAttribute>() != null) return false;
        if (member.GetCustomAttribute<JsonIgnoreAttribute>() != null) return false;

        // Explicit include (for non-public)
        if (member.GetCustomAttribute<HjsonIncludeAttribute>() != null) return true;
        if (member.GetCustomAttribute<JsonIncludeAttribute>() != null) return true;

        // Otherwise, only public
        return isPublic;
    }

    static string GetName(MemberInfo member) =>
        member.GetCustomAttribute<HjsonPropertyNameAttribute>()?.Name ??
        member.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ??
        member.Name;

    static string GetComment(MemberInfo member) => member.GetCustomAttribute<HjsonCommentAttribute>()?.Comment;
}
