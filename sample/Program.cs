using System;
using System.Collections.Generic;
using System.IO;
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
      var data=HjsonValue.Load("test.hjson").Qo();
      Console.WriteLine(data.Qs("hello"));

      Console.WriteLine("Saving as json...");
      HjsonValue.Save(data, "test.json");

      Console.WriteLine("Saving as hjson...");
      HjsonValue.Save(data, "test2.hjson");

      // edit (preserve whitespace and comments)
      var wdata=(WscJsonObject)HjsonValue.LoadWsc(new StreamReader("test.hjson")).Qo();

      // edit like you normally would
      wdata["hugo"]="value";
      // optionally set order and comments:
      wdata.Order.Insert(2, "hugo");
      wdata.Comments["hugo"]="just another test";

      var sw=new StringWriter();
      HjsonValue.SaveWsc(wdata, sw);
      Console.WriteLine(sw.ToString());
    }
  }
}
