#nullable enable

using System;
using System.IO;

using Hjson;

using Xunit;

namespace Hjson.Tests;

[Collection("EolTests")]
public class EdgeCaseTests
{
    // ── Issue: Mixed line endings when saving with KeepWsc ───────────

    [Fact]
    public void KeepWsc_Save_ConsistentLineEndings_LF()
    {
        JsonValue.Eol = "\n";
        string input = "# comment\n\na: 0\nb: 0\n";

        var options = new HjsonOptions { EmitRootBraces = false, KeepWsc = true };
        var config = HjsonValue.Parse(input, options);
        config["a"] = 1;

        var sw = new StringWriter();
        HjsonValue.Save(config, sw, options);
        string output = sw.ToString();

        // No \r should be present when eol is LF
        Assert.DoesNotContain("\r", output);
        // Values should be updated
        Assert.Contains("a: 1", output);
        Assert.Contains("b: 0", output);
    }

    [Fact]
    public void KeepWsc_Save_ConsistentLineEndings_CRLF()
    {
        JsonValue.Eol = "\r\n";
        string input = "# comment\n\na: 0\nb: 0\n";

        var options = new HjsonOptions { EmitRootBraces = false, KeepWsc = true };
        var config = HjsonValue.Parse(input, options);
        config["a"] = 1;

        var sw = new StringWriter();
        HjsonValue.Save(config, sw, options);
        string output = sw.ToString();

        // All newlines should be CRLF, not bare LF
        string withoutCr = output.Replace("\r\n", "\n");
        Assert.DoesNotContain("\r", withoutCr); // no stray \r left
        // And there should be no bare LF (each \n should be preceded by \r)
        for (int i = 0; i < output.Length; i++)
        {
            if (output[i] == '\n')
                Assert.True(i > 0 && output[i - 1] == '\r',
                    $"Found bare LF at position {i} in output: {output[..Math.Min(i + 20, output.Length)]}");
        }

        Assert.Contains("a: 1", output);
        Assert.Contains("b: 0", output);
    }

    [Fact]
    public void KeepWsc_Save_CommentAndValueLines_SameLineEnding()
    {
        JsonValue.Eol = "\r\n";
        // Input with LF-only endings (as might come from a Unix-style file)
        string input = "# this is a comment\n\na: 0\nb: 0\n";

        var options = new HjsonOptions { EmitRootBraces = false, KeepWsc = true };
        var config = HjsonValue.Parse(input, options);

        var sw = new StringWriter();
        HjsonValue.Save(config, sw, options);
        string output = sw.ToString();

        // Split on CRLF; there should be no elements containing bare LF
        string[] lines = output.Split("\r\n");
        foreach (string line in lines)
        {
            Assert.DoesNotContain("\n", line);
            Assert.DoesNotContain("\r", line);
        }
    }

    [Fact]
    public void KeepWsc_Roundtrip_PreservesComments()
    {
        JsonValue.Eol = "\n";
        string input = "# header comment\n\na: 0\n# inline comment\nb: 0\n";

        var options = new HjsonOptions { EmitRootBraces = false, KeepWsc = true };
        var config = HjsonValue.Parse(input, options);

        var sw = new StringWriter();
        HjsonValue.Save(config, sw, options);
        string output = sw.ToString();

        Assert.Contains("# header comment", output);
        Assert.Contains("# inline comment", output);
    }

    [Fact]
    public void KeepWsc_Save_WithBraces_ConsistentLineEndings()
    {
        JsonValue.Eol = "\r\n";
        string input = "{\n  # comment\n  a: 0\n  b: 0\n}\n";

        var options = new HjsonOptions { KeepWsc = true };
        var config = HjsonValue.Parse(input, options);
        config["a"] = 42;

        var sw = new StringWriter();
        HjsonValue.Save(config, sw, options);
        string output = sw.ToString();

        // Every newline should be CRLF
        for (int i = 0; i < output.Length; i++)
        {
            if (output[i] == '\n')
                Assert.True(i > 0 && output[i - 1] == '\r',
                    $"Found bare LF at position {i}");
        }
    }

    [Fact]
    public void KeepWsc_Save_Array_ConsistentLineEndings()
    {
        JsonValue.Eol = "\r\n";
        string input = "{\n  items:\n  [\n    # first item\n    1\n    2\n  ]\n}\n";

        var options = new HjsonOptions { KeepWsc = true };
        var config = HjsonValue.Parse(input, options);

        var sw = new StringWriter();
        HjsonValue.Save(config, sw, options);
        string output = sw.ToString();

        for (int i = 0; i < output.Length; i++)
        {
            if (output[i] == '\n')
                Assert.True(i > 0 && output[i - 1] == '\r',
                    $"Found bare LF at position {i}");
        }
    }

    // ── Issue: ulong/long precision loss ────────────────────────────

    [Fact]
    public void Parse_LargeNumber_PreservesPrecision()
    {
        string hjson = @"
        {
            ChannelId: 943453428129071119
        }";

        var config = HjsonValue.Parse(hjson);

        long channelId = config["ChannelId"];
        Assert.Equal(943453428129071119L, channelId);
    }

    [Fact]
    public void Parse_LargeNumber_QuotelessValue_PreservesPrecision()
    {
        // Hjson allows quoteless values; this tests the readTfnns path
        string hjson = "ChannelId: 943453428129071119";

        var options = new HjsonOptions { EmitRootBraces = false };
        var config = HjsonValue.Parse(hjson, options);

        long channelId = config["ChannelId"];
        Assert.Equal(943453428129071119L, channelId);
    }

    [Fact]
    public void Parse_LargeNegativeNumber_PreservesPrecision()
    {
        string hjson = @"
        {
            Value: -943453428129071119
        }";

        var config = HjsonValue.Parse(hjson);

        long value = config["Value"];
        Assert.Equal(-943453428129071119L, value);
    }

    [Fact]
    public void Parse_LongMaxValue_PreservesPrecision()
    {
        string hjson = $@"
        {{
            Max: {long.MaxValue}
        }}";

        var config = HjsonValue.Parse(hjson);

        long value = config["Max"];
        Assert.Equal(long.MaxValue, value);
    }

    [Fact]
    public void Parse_LongMinValue_PreservesPrecision()
    {
        string hjson = $@"
        {{
            Min: {long.MinValue}
        }}";

        var config = HjsonValue.Parse(hjson);

        long value = config["Min"];
        Assert.Equal(long.MinValue, value);
    }

    [Fact]
    public void Parse_SmallNumber_StillWorksAsLong()
    {
        string hjson = @"
        {
            Value: 42
        }";

        var config = HjsonValue.Parse(hjson);

        int value = config["Value"];
        Assert.Equal(42, value);
    }

    [Fact]
    public void Parse_FloatingPoint_StillWorksAsDouble()
    {
        string hjson = @"
        {
            Value: 3.14159
        }";

        var config = HjsonValue.Parse(hjson);

        double value = config["Value"];
        Assert.Equal(3.14159, value);
    }

    [Fact]
    public void Parse_ScientificNotation_StillWorksAsDouble()
    {
        string hjson = @"
        {
            Value: 1.5e10
        }";

        var config = HjsonValue.Parse(hjson);

        double value = config["Value"];
        Assert.Equal(1.5e10, value);
    }

    [Fact]
    public void Parse_Zero_StillWorks()
    {
        string hjson = @"
        {
            Value: 0
        }";

        var config = HjsonValue.Parse(hjson);

        int value = config["Value"];
        Assert.Equal(0, value);
    }

    [Fact]
    public void Parse_NegativeZero_ReturnsMinus0()
    {
        string hjson = @"
        {
            Value: -0.0
        }";

        var config = HjsonValue.Parse(hjson);

        double value = config["Value"];
        Assert.True(double.IsNegative(value));
        Assert.Equal(0.0, Math.Abs(value));
    }

    [Fact]
    public void Parse_LargeNumber_JsonPath_PreservesPrecision()
    {
        // Test the JSON (strict) parser path as well
        string json = "{\"ChannelId\": 943453428129071119}";

        var config = JsonValue.Parse(json);

        long channelId = config["ChannelId"];
        Assert.Equal(943453428129071119L, channelId);
    }

    [Fact]
    public void Roundtrip_LargeNumber_PreservesPrecision()
    {
        string hjson = @"
        {
            ChannelId: 943453428129071119
        }";

        var config = HjsonValue.Parse(hjson);

        // Serialize back and re-parse
        string output = config.ToString(Stringify.Hjson);
        var config2 = HjsonValue.Parse(output);

        long channelId = config2["ChannelId"];
        Assert.Equal(943453428129071119L, channelId);
    }

    [Theory]
    [InlineData("999999999999999999")]
    [InlineData("100000000000000000")]
    [InlineData("123456789012345678")]
    public void Parse_Various18DigitNumbers_PreservesPrecision(string numStr)
    {
        string hjson = $@"
        {{
            Value: {numStr}
        }}";

        var config = HjsonValue.Parse(hjson);

        long value = config["Value"];
        Assert.Equal(long.Parse(numStr), value);
    }
}