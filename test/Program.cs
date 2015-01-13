using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hjson;

namespace Test
{
  class Program
  {
    static string assetsDir=Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "../../assets");

    static string load(string file, bool cr)
    {
      var text=File.ReadAllText(file, Encoding.UTF8);
      var std=text.Replace("\r", ""); // make sure we have unix style text regardless of the input
      return cr?std.Replace("\n", "\r\n"):std;
    }

    static bool test(string name, string file, bool inputCr, bool outputCr)
    {
      bool isJson=Path.GetExtension(file)==".json";
      bool shouldFail=name.StartsWith("fail");

      JsonValue.Eol=outputCr?"\r\n":"\n";
      var text=load(file, inputCr);

      try
      {
        var data=HjsonValue.Parse(text);
        var data1=data.ToString(Stringify.Formatted);
        var hjson1=data.ToString(Stringify.Hjson);

        if (!shouldFail)
        {
          var result=JsonValue.Parse(load(Path.Combine(assetsDir, name+"_result.json"), inputCr));
          var data2=result.ToString(Stringify.Formatted);
          var hjson2=load(Path.Combine(assetsDir, name+"_result.hjson"), outputCr);
          if (data1!=data2) return failErr(name, "parse", data1, data2);
          if (hjson1!=hjson2) return failErr(name, "stringify", hjson1, hjson2);

          if (isJson)
          {
            string json1=data.ToString(), json2=JsonValue.Parse(text).ToString();
            if (json1!=json2) return failErr(name, "json chk", json1, json2);
          }
        }
        else return failErr(name, "should fail");
      }
      catch (Exception e)
      {
        if (!shouldFail) return failErr(name, "exception", e.ToString(), "");
      }
      return true;
    }

    static int Main(string[] args)
    {
      string filter=args.Length==1?args[0]:null;

      Console.WriteLine("running tests...");

      foreach (var file in Directory.GetFiles(assetsDir, "*_test.*"))
      {
        string name=Path.GetFileNameWithoutExtension(file);
        name=name.Substring(0, name.Length-5);
        if (filter!=null && !name.Contains(filter)) continue;

        if (!test(name, file, false, false)
          || !test(name, file, true, false)
          || !test(name, file, false, true)
          || !test(name, file, true, true)) return 1;
        Console.WriteLine("- "+name+" OK");
      }
      Console.WriteLine("ALL OK!");
      return 0;
    }

    static bool failErr(string name, string type, string s1=null, string s2=null)
    {
      Console.WriteLine(name+" "+type+" FAILED!");
      if (s1!=null || s2!=null)
      {
        Console.WriteLine("--- actual ({0}):", s1.Length);
        Console.WriteLine(s1+"---");
        Console.WriteLine("--- expected ({0}):", s2.Length);
        Console.WriteLine(s2+"---");
        if (s1.Length==s2.Length)
          for (int i=0; i<s1.Length; i++)
          {
            if (s1[i]!=s2[i])
            {
              Console.WriteLine("Diff at offs {0}: {1}/{2}-{3}/{4}", i, s1[i], s1[i], s2[i], s2[i]);
              break;
            }
          }
      }
      return false;
    }
  }
}
