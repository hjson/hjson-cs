using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hjson;

namespace HjsonSample
{
  class Program
  {
    static void Main(string[] args)
    {
      var data=(JsonObject)HjsonValue.Load("readme.hjson");
      Console.WriteLine((string)data["hello"]);

      Console.WriteLine("Saving as json...");
      HjsonValue.Save(data, "readme.json");

      Console.WriteLine("Saving as hjson...");
      HjsonValue.Save(data, "readme2.hjson");
    }
  }
}
