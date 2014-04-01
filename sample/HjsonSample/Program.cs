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
      data.Save("readme.json", true);
    }
  }
}
