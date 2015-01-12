using System;
using System.Collections.Generic;
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
      bool err=false;
      foreach (string arg in args)
      {
        if (arg=="-j") todo=Stringify.Formatted;
        else if (arg=="-c") todo=Stringify.Plain;
        else if (arg=="-h") todo=Stringify.Hjson;
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
        Console.WriteLine("  -j  JSON output (formatted)");
        Console.WriteLine("  -c  JSON output (compact)");
        return 1;
      }

      var data=HjsonValue.Load(file);
      Console.WriteLine(data.ToString(todo));
      return 0;
    }
  }
}
