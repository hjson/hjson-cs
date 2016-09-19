using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hjson;

namespace HjsonCli
{
  class Program
  {
    static int Main(string[] args)
    {
      string file=null;
      Stringify todo=Stringify.Hjson;
      bool err=false, roundtrip=false, rootBraces=true;
      foreach (string arg in args)
      {
        if (arg=="-j") todo=Stringify.Formatted;
        else if (arg=="-c") todo=Stringify.Plain;
        else if (arg=="-h") todo=Stringify.Hjson;
        else if (arg=="-r") { roundtrip=true; todo=Stringify.Hjson; }
        else if (arg=="-n") rootBraces=false;
        else if (!arg.StartsWith("-"))
        {
          if (file==null) file=arg;
          else err=true;
        }
        else err=true;
      }

      if (err || file==null)
      {
        Console.WriteLine("hjsonc [OPTION] FILE");
        Console.WriteLine("Options:");
        Console.WriteLine("  -h  Hjson output (default)");
        Console.WriteLine("  -r  Hjson output, round trip with comments");
        Console.WriteLine("  -j  JSON output (formatted)");
        Console.WriteLine("  -c  JSON output (compact)");
        Console.WriteLine("  -n  omit braces for the root object (Hjson).");
        return 1;
      }

      JsonValue data=HjsonValue.Load(file, new HjsonOptions { KeepWsc=roundtrip });
      if (todo==Stringify.Hjson)
        Console.WriteLine(data.ToString(new HjsonOptions { KeepWsc=roundtrip, EmitRootBraces=rootBraces }));
      else
        Console.WriteLine(data.ToString(todo));
      return 0;
    }
  }
}
