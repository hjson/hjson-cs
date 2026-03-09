#nullable enable

using System;
using System.IO;
using System.Text;

using Hjson;

using Xunit;

namespace Hjson.Tests;

[Collection("EolTests")]
public class ParserTests
{
    static readonly string AssetsDir = Path.Combine(AppContext.BaseDirectory, "assets");

    static string Load(string file, bool cr)
    {
        string text = File.ReadAllText(file, Encoding.UTF8);
        return text.ReplaceLineEndings(cr ? "\r\n" : "\n");
    }

    [Theory]
    [InlineData("charset_test.hjson")]
    [InlineData("comments_test.hjson")]
    [InlineData("empty_test.hjson")]
    [InlineData("failCharset1_test.hjson")]
    [InlineData("failJSON02_test.json")]
    [InlineData("failJSON05_test.json")]
    [InlineData("failJSON06_test.json")]
    [InlineData("failJSON07_test.json")]
    [InlineData("failJSON08_test.json")]
    [InlineData("failJSON10_test.json")]
    [InlineData("failJSON11_test.json")]
    [InlineData("failJSON12_test.json")]
    [InlineData("failJSON13_test.json")]
    [InlineData("failJSON14_test.json")]
    [InlineData("failJSON15_test.json")]
    [InlineData("failJSON16_test.json")]
    [InlineData("failJSON17_test.json")]
    [InlineData("failJSON19_test.json")]
    [InlineData("failJSON20_test.json")]
    [InlineData("failJSON21_test.json")]
    [InlineData("failJSON22_test.json")]
    [InlineData("failJSON23_test.json")]
    [InlineData("failJSON26_test.json")]
    [InlineData("failJSON28_test.json")]
    [InlineData("failJSON29_test.json")]
    [InlineData("failJSON30_test.json")]
    [InlineData("failJSON31_test.json")]
    [InlineData("failJSON32_test.json")]
    [InlineData("failJSON33_test.json")]
    [InlineData("failJSON34_test.json")]
    [InlineData("failKey1_test.hjson")]
    [InlineData("failKey2_test.hjson")]
    [InlineData("failKey3_test.hjson")]
    [InlineData("failKey4_test.hjson")]
    [InlineData("failKey5_test.hjson")]
    [InlineData("failMLStr1_test.hjson")]
    [InlineData("failObj1_test.hjson")]
    [InlineData("failObj2_test.hjson")]
    [InlineData("failObj3_test.hjson")]
    [InlineData("failStr1a_test.hjson")]
    [InlineData("failStr1b_test.hjson")]
    [InlineData("failStr1c_test.hjson")]
    [InlineData("failStr1d_test.hjson")]
    [InlineData("failStr2a_test.hjson")]
    [InlineData("failStr2b_test.hjson")]
    [InlineData("failStr2c_test.hjson")]
    [InlineData("failStr2d_test.hjson")]
    [InlineData("failStr3a_test.hjson")]
    [InlineData("failStr3b_test.hjson")]
    [InlineData("failStr3c_test.hjson")]
    [InlineData("failStr3d_test.hjson")]
    [InlineData("failStr4a_test.hjson")]
    [InlineData("failStr4b_test.hjson")]
    [InlineData("failStr4c_test.hjson")]
    [InlineData("failStr4d_test.hjson")]
    [InlineData("failStr5a_test.hjson")]
    [InlineData("failStr5b_test.hjson")]
    [InlineData("failStr5c_test.hjson")]
    [InlineData("failStr5d_test.hjson")]
    [InlineData("failStr6a_test.hjson")]
    [InlineData("failStr6b_test.hjson")]
    [InlineData("failStr6c_test.hjson")]
    [InlineData("failStr6d_test.hjson")]
    [InlineData("failStr7a_test.hjson")]
    [InlineData("failStr8a_test.hjson")]
    [InlineData("kan_test.hjson")]
    [InlineData("keys_test.hjson")]
    [InlineData("mltabs_test.json")]
    [InlineData("oa_test.hjson")]
    [InlineData("pass1_test.json")]
    [InlineData("pass2_test.json")]
    [InlineData("pass3_test.json")]
    [InlineData("pass4_test.json")]
    [InlineData("passSingle_test.hjson")]
    [InlineData("stringify1_test.hjson")]
    [InlineData("strings2_test.hjson")]
    [InlineData("strings_test.hjson")]
    [InlineData("trail_test.hjson")]
    public void ParseAndStringify(string file)
    {
        // Test all four CR/LF combinations
        RunTest(file, inputCr: false, outputCr: false);
        RunTest(file, inputCr: true, outputCr: false);
        RunTest(file, inputCr: false, outputCr: true);
        RunTest(file, inputCr: true, outputCr: true);
    }

    void RunTest(string file, bool inputCr, bool outputCr)
    {
        string name = Path.GetFileNameWithoutExtension(file)[..^5];
        bool isJson = Path.GetExtension(file) is ".json";
        bool shouldFail = name.StartsWith("fail");

        JsonValue.Eol = outputCr ? "\r\n" : "\n";
        string text = Load(Path.Combine(AssetsDir, file), inputCr);

        if (shouldFail)
        {
            Assert.ThrowsAny<Exception>(() => HjsonValue.Parse(text));
            return;
        }

        var data = HjsonValue.Parse(text);

        string data1 = data.ToString(Stringify.Formatted);
        string hjson1 = data.ToString(Stringify.Hjson);

        var result = JsonValue.Parse(Load(Path.Combine(AssetsDir, $"{name}_result.json"), inputCr));
        string data2 = result.ToString(Stringify.Formatted);
        string hjson2 = Load(Path.Combine(AssetsDir, $"{name}_result.hjson"), outputCr);

        Assert.Equal(data2, data1);
        Assert.Equal(hjson2, hjson1);

        if (isJson)
        {
            string json1 = data.ToString();
            string json2 = JsonValue.Parse(text).ToString();
            Assert.Equal(json2, json1);
        }
    }
}