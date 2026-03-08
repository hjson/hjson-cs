#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using Hjson;

using Xunit;

namespace Hjson.Tests;

#region Test models

public class SimpleConfig
{
    public string Name { get; set; } = "";
    public int Port { get; set; }
    public bool Enabled { get; set; }
}

public class RenamedProps
{
    [HjsonPropertyName("server_name")]
    public string ServerName { get; set; } = "";

    [HjsonPropertyName("listen_port")]
    public int Port { get; set; }
}

public class JsonRenamedFallback
{
    [JsonPropertyName("server_name")]
    public string ServerName { get; set; } = "";

    [JsonPropertyName("listen_port")]
    public int Port { get; set; }
}

public class HjsonOverridesJson
{
    [HjsonPropertyName("hjson_name")]
    [JsonPropertyName("json_name")]
    public string Name { get; set; } = "";
}

public class WithIgnored
{
    public string Visible { get; set; } = "";

    [HjsonIgnore]
    public string Secret { get; set; } = "";
}

public class WithJsonIgnored
{
    public string Visible { get; set; } = "";

    [JsonIgnore]
    public string Secret { get; set; } = "";
}

public class WithIncluded
{
    public string Public { get; set; } = "";

    [HjsonInclude]
    internal string Internal { get; set; } = "";
}

public class WithJsonIncluded
{
    public string Public { get; set; } = "";

    [JsonInclude]
    internal string Internal { get; set; } = "";
}

public class WithComments
{
    [HjsonComment("The server hostname")]
    public string Host { get; set; } = "";

    [HjsonComment("Port number to listen on")]
    public int Port { get; set; }

    public bool Debug { get; set; }
}

public class WithMultilineComment
{
    [HjsonComment("First line\nSecond line")]
    public string Value { get; set; } = "";
}

public class WithAllAttributes
{
    [HjsonComment("The display name")]
    [HjsonPropertyName("display_name")]
    public string Name { get; set; } = "";

    [HjsonIgnore]
    public string Hidden { get; set; } = "";

    [HjsonComment("Is active?")]
    public bool Active { get; set; }
}

public enum Color { Red, Green, Blue }

public class WithEnum
{
    public Color Favorite { get; set; }
}

public class WithNullable
{
    public int? MaybeInt { get; set; }
    public string? MaybeString { get; set; }
}

public class WithCollections
{
    public List<int> Numbers { get; set; } = [];
    public string[] Tags { get; set; } = [];
}

public class WithDictionary
{
    public Dictionary<string, int> Scores { get; set; } = new();
}

public class Nested
{
    public string Name { get; set; } = "";
    public InnerObj Inner { get; set; } = new();
}

public class InnerObj
{
    public int Value { get; set; }
    public string Label { get; set; } = "";
}

public class WithNumericTypes
{
    public byte ByteVal { get; set; }
    public short ShortVal { get; set; }
    public long LongVal { get; set; }
    public float FloatVal { get; set; }
    public double DoubleVal { get; set; }
    public decimal DecimalVal { get; set; }
}

#endregion

public class HjsonConvertTests
{
    // ── Basic Serialization ──────────────────────────────────────────

    [Fact]
    public void Serialize_SimpleObject()
    {
        var config = new SimpleConfig { Name = "test", Port = 8080, Enabled = true };
        string hjson = HjsonConvert.Serialize(config);

        Assert.Contains("Name", hjson);
        Assert.Contains("test", hjson);
        Assert.Contains("Port", hjson);
        Assert.Contains("8080", hjson);
        Assert.Contains("Enabled", hjson);
        Assert.Contains("true", hjson);
    }

    [Fact]
    public void Serialize_Null_ReturnsNullString()
    {
        Assert.Equal("null", HjsonConvert.Serialize(null!));
    }

    // ── Basic Deserialization ────────────────────────────────────────

    [Fact]
    public void Deserialize_SimpleObject()
    {
        string hjson = @"
        {
            Name: test
            Port: 8080
            Enabled: true
        }";

        var config = HjsonConvert.Deserialize<SimpleConfig>(hjson);

        Assert.Equal("test", config.Name);
        Assert.Equal(8080, config.Port);
        Assert.True(config.Enabled);
    }

    // ── Round-trip ───────────────────────────────────────────────────

    [Fact]
    public void Roundtrip_SimpleObject()
    {
        var original = new SimpleConfig { Name = "hello", Port = 3000, Enabled = false };
        string hjson = HjsonConvert.Serialize(original);
        var restored = HjsonConvert.Deserialize<SimpleConfig>(hjson);

        Assert.Equal(original.Name, restored.Name);
        Assert.Equal(original.Port, restored.Port);
        Assert.Equal(original.Enabled, restored.Enabled);
    }

    // ── HjsonPropertyName ───────────────────────────────────────────

    [Fact]
    public void Serialize_HjsonPropertyName_UsesCustomKey()
    {
        var obj = new RenamedProps { ServerName = "myhost", Port = 443 };
        string hjson = HjsonConvert.Serialize(obj);

        Assert.Contains("server_name", hjson);
        Assert.Contains("listen_port", hjson);
        Assert.DoesNotContain("ServerName", hjson);
        Assert.DoesNotContain("Port", hjson);
    }

    [Fact]
    public void Deserialize_HjsonPropertyName_ReadsCustomKey()
    {
        string hjson = @"
        {
            server_name: myhost
            listen_port: 443
        }";

        var obj = HjsonConvert.Deserialize<RenamedProps>(hjson);

        Assert.Equal("myhost", obj.ServerName);
        Assert.Equal(443, obj.Port);
    }

    // ── JsonPropertyName fallback ───────────────────────────────────

    [Fact]
    public void Serialize_JsonPropertyName_Fallback()
    {
        var obj = new JsonRenamedFallback { ServerName = "host1", Port = 80 };
        string hjson = HjsonConvert.Serialize(obj);

        Assert.Contains("server_name", hjson);
        Assert.Contains("listen_port", hjson);
    }

    [Fact]
    public void Deserialize_JsonPropertyName_Fallback()
    {
        string hjson = @"
        {
            server_name: host1
            listen_port: 80
        }";

        var obj = HjsonConvert.Deserialize<JsonRenamedFallback>(hjson);

        Assert.Equal("host1", obj.ServerName);
        Assert.Equal(80, obj.Port);
    }

    // ── Hjson attribute takes priority over Json attribute ───────────

    [Fact]
    public void Serialize_HjsonPropertyName_OverridesJsonPropertyName()
    {
        var obj = new HjsonOverridesJson { Name = "test" };
        string hjson = HjsonConvert.Serialize(obj);

        Assert.Contains("hjson_name", hjson);
        // "hjson_name" contains "json_name" as a substring, so check the key isn't standalone
        Assert.DoesNotContain("json_name:", hjson.Replace("hjson_name", ""));
    }

    // ── HjsonIgnore ─────────────────────────────────────────────────

    [Fact]
    public void Serialize_HjsonIgnore_ExcludesProperty()
    {
        var obj = new WithIgnored { Visible = "yes", Secret = "s3cr3t" };
        string hjson = HjsonConvert.Serialize(obj);

        Assert.Contains("Visible", hjson);
        Assert.DoesNotContain("Secret", hjson);
        Assert.DoesNotContain("s3cr3t", hjson);
    }

    [Fact]
    public void Deserialize_HjsonIgnore_SkipsProperty()
    {
        string hjson = @"
        {
            Visible: yes
            Secret: should_be_ignored
        }";

        var obj = HjsonConvert.Deserialize<WithIgnored>(hjson);

        Assert.Equal("yes", obj.Visible);
        Assert.Equal("", obj.Secret); // default, not deserialized
    }

    // ── JsonIgnore fallback ─────────────────────────────────────────

    [Fact]
    public void Serialize_JsonIgnore_Fallback()
    {
        var obj = new WithJsonIgnored { Visible = "yes", Secret = "s3cr3t" };
        string hjson = HjsonConvert.Serialize(obj);

        Assert.Contains("Visible", hjson);
        Assert.DoesNotContain("Secret", hjson);
    }

    // ── HjsonInclude ────────────────────────────────────────────────

    [Fact]
    public void Serialize_HjsonInclude_IncludesNonPublic()
    {
        var obj = new WithIncluded { Public = "pub" };
        // Use reflection to set internal property
        typeof(WithIncluded).GetProperty("Internal",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(obj, "internal_val");

        string hjson = HjsonConvert.Serialize(obj);

        Assert.Contains("Public", hjson);
        Assert.Contains("Internal", hjson);
        Assert.Contains("internal_val", hjson);
    }

    [Fact]
    public void Deserialize_HjsonInclude_SetsNonPublic()
    {
        string hjson = @"
        {
            Public: pub
            Internal: internal_val
        }";

        var obj = HjsonConvert.Deserialize<WithIncluded>(hjson);

        Assert.Equal("pub", obj.Public);
        Assert.Equal("internal_val", obj.Internal);
    }

    // ── JsonInclude fallback ────────────────────────────────────────

    [Fact]
    public void Serialize_JsonInclude_Fallback()
    {
        var obj = new WithJsonIncluded { Public = "pub" };
        typeof(WithJsonIncluded).GetProperty("Internal",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(obj, "internal_val");

        string hjson = HjsonConvert.Serialize(obj);

        Assert.Contains("Internal", hjson);
    }

    // ── HjsonComment ────────────────────────────────────────────────

    [Fact]
    public void Serialize_HjsonComment_EmitsComments()
    {
        var obj = new WithComments { Host = "localhost", Port = 8080, Debug = true };
        string hjson = HjsonConvert.Serialize(obj);

        Assert.Contains("# The server hostname", hjson);
        Assert.Contains("# Port number to listen on", hjson);
        Assert.Contains("Host", hjson);
        Assert.Contains("localhost", hjson);
    }

    [Fact]
    public void Serialize_MultilineComment_EmitsAllLines()
    {
        var obj = new WithMultilineComment { Value = "test" };
        string hjson = HjsonConvert.Serialize(obj);

        Assert.Contains("# First line", hjson);
        Assert.Contains("# Second line", hjson);
    }

    [Fact]
    public void Deserialize_IgnoresComments()
    {
        string hjson = @"
        {
            # This is a comment
            Host: myhost
            # Another comment
            Port: 9090
            Debug: false
        }";

        var obj = HjsonConvert.Deserialize<WithComments>(hjson);

        Assert.Equal("myhost", obj.Host);
        Assert.Equal(9090, obj.Port);
        Assert.False(obj.Debug);
    }

    // ── Combined attributes ─────────────────────────────────────────

    [Fact]
    public void Serialize_AllAttributes_Combined()
    {
        var obj = new WithAllAttributes { Name = "Test", Hidden = "secret", Active = true };
        string hjson = HjsonConvert.Serialize(obj);

        Assert.Contains("# The display name", hjson);
        Assert.Contains("display_name", hjson);
        Assert.Contains("# Is active?", hjson);
        Assert.DoesNotContain("Hidden", hjson);
        Assert.DoesNotContain("secret", hjson);
    }

    [Fact]
    public void Roundtrip_AllAttributes()
    {
        var original = new WithAllAttributes { Name = "Test", Hidden = "secret", Active = true };
        string hjson = HjsonConvert.Serialize(original);
        var restored = HjsonConvert.Deserialize<WithAllAttributes>(hjson);

        Assert.Equal("Test", restored.Name);
        Assert.Equal("", restored.Hidden); // ignored, uses default
        Assert.True(restored.Active);
    }

    // ── Enum support ────────────────────────────────────────────────

    [Fact]
    public void Roundtrip_Enum()
    {
        var obj = new WithEnum { Favorite = Color.Blue };
        string hjson = HjsonConvert.Serialize(obj);

        Assert.Contains("Blue", hjson);

        var restored = HjsonConvert.Deserialize<WithEnum>(hjson);
        Assert.Equal(Color.Blue, restored.Favorite);
    }

    // ── Nullable support ────────────────────────────────────────────

    [Fact]
    public void Roundtrip_Nullable_WithValues()
    {
        var obj = new WithNullable { MaybeInt = 42, MaybeString = "hello" };
        string hjson = HjsonConvert.Serialize(obj);
        var restored = HjsonConvert.Deserialize<WithNullable>(hjson);

        Assert.Equal(42, restored.MaybeInt);
        Assert.Equal("hello", restored.MaybeString);
    }

    [Fact]
    public void Roundtrip_Nullable_WithNulls()
    {
        var obj = new WithNullable { MaybeInt = null, MaybeString = null };
        string hjson = HjsonConvert.Serialize(obj);
        var restored = HjsonConvert.Deserialize<WithNullable>(hjson);

        Assert.Null(restored.MaybeInt);
        Assert.Null(restored.MaybeString);
    }

    // ── Collections ─────────────────────────────────────────────────

    [Fact]
    public void Roundtrip_List_And_Array()
    {
        var obj = new WithCollections
        {
            Numbers = [1, 2, 3],
            Tags = ["a", "b", "c"],
        };

        string hjson = HjsonConvert.Serialize(obj);
        var restored = HjsonConvert.Deserialize<WithCollections>(hjson);

        Assert.Equal([1, 2, 3], restored.Numbers);
        Assert.Equal(["a", "b", "c"], restored.Tags);
    }

    [Fact]
    public void Roundtrip_EmptyCollections()
    {
        var obj = new WithCollections { Numbers = [], Tags = [] };
        string hjson = HjsonConvert.Serialize(obj);
        var restored = HjsonConvert.Deserialize<WithCollections>(hjson);

        Assert.Empty(restored.Numbers);
        Assert.Empty(restored.Tags);
    }

    // ── Dictionary ──────────────────────────────────────────────────

    [Fact]
    public void Roundtrip_Dictionary()
    {
        var obj = new WithDictionary
        {
            Scores = new Dictionary<string, int>
            {
                ["alice"] = 100,
                ["bob"] = 85,
            },
        };

        string hjson = HjsonConvert.Serialize(obj);
        var restored = HjsonConvert.Deserialize<WithDictionary>(hjson);

        Assert.Equal(100, restored.Scores["alice"]);
        Assert.Equal(85, restored.Scores["bob"]);
    }

    // ── Nested objects ──────────────────────────────────────────────

    [Fact]
    public void Roundtrip_NestedObject()
    {
        var obj = new Nested
        {
            Name = "parent",
            Inner = new InnerObj { Value = 42, Label = "nested" },
        };

        string hjson = HjsonConvert.Serialize(obj);
        var restored = HjsonConvert.Deserialize<Nested>(hjson);

        Assert.Equal("parent", restored.Name);
        Assert.Equal(42, restored.Inner.Value);
        Assert.Equal("nested", restored.Inner.Label);
    }

    // ── Numeric types ───────────────────────────────────────────────

    [Fact]
    public void Roundtrip_NumericTypes()
    {
        var obj = new WithNumericTypes
        {
            ByteVal = 255,
            ShortVal = 1000,
            LongVal = 9999999999L,
            FloatVal = 3.14f,
            DoubleVal = 2.718281828,
            DecimalVal = 123.456m,
        };

        string hjson = HjsonConvert.Serialize(obj);
        var restored = HjsonConvert.Deserialize<WithNumericTypes>(hjson);

        Assert.Equal(255, restored.ByteVal);
        Assert.Equal(1000, restored.ShortVal);
        Assert.Equal(9999999999L, restored.LongVal);
        Assert.Equal(3.14f, restored.FloatVal);
        Assert.Equal(2.718281828, restored.DoubleVal);
        Assert.Equal(123.456m, restored.DecimalVal);
    }

    // ── Deserialize from real Hjson features ─────────────────────────

    [Fact]
    public void Deserialize_UnquotedStrings()
    {
        string hjson = @"
        {
            Name: hello world
            Port: 3000
            Enabled: true
        }";

        var config = HjsonConvert.Deserialize<SimpleConfig>(hjson);

        Assert.Equal("hello world", config.Name);
        Assert.Equal(3000, config.Port);
    }

    [Fact]
    public void Deserialize_WithHjsonComments()
    {
        string hjson = @"
        {
            // C-style comment
            Name: test
            # Hash comment
            Port: 80
            /* Block comment */
            Enabled: true
        }";

        var config = HjsonConvert.Deserialize<SimpleConfig>(hjson);

        Assert.Equal("test", config.Name);
        Assert.Equal(80, config.Port);
        Assert.True(config.Enabled);
    }

    [Fact]
    public void Deserialize_MissingProperties_UseDefaults()
    {
        string hjson = @"
        {
            Name: partial
        }";

        var config = HjsonConvert.Deserialize<SimpleConfig>(hjson);

        Assert.Equal("partial", config.Name);
        Assert.Equal(0, config.Port);       // default int
        Assert.False(config.Enabled);       // default bool
    }
}