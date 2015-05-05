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
      bool err=false, roundtrip=false, rootBraces=false;
      foreach (string arg in args)
      {
        if (arg=="-j") todo=Stringify.Formatted;
        else if (arg=="-c") todo=Stringify.Plain;
        else if (arg=="-h") todo=Stringify.Hjson;
        else if (arg=="-r") { roundtrip=true; todo=Stringify.Hjson; }
        else if (arg=="-b") rootBraces=true;
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
        Console.WriteLine("  -b  output braces for the root object (Hjson).");
        Console.WriteLine("  -j  JSON output (formatted)");
        Console.WriteLine("  -c  JSON output (compact)");
        return 1;
      }

      JsonValue data;
      if (roundtrip)
      {
        using (var sr=new StreamReader(file))
          data=HjsonValue.LoadWsc(sr);
      }
      else data=HjsonValue.Load(file);

      if (todo==Stringify.Hjson)
        Console.WriteLine(data.ToString(new HjsonOptions { KeepWsc=roundtrip, EmitRootBraces=rootBraces }));
      else
        Console.WriteLine(data.ToString(todo));
      return 0;
    }
  }
}
