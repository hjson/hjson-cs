#nullable enable

using System;
using System.IO;
using Hjson;

// note: this sample uses the Hjson library directly.
// Normally you would use nuget.

var data = HjsonValue.Load("test.hjson").Qo();
Console.WriteLine(data.Qs("hello"));

Console.WriteLine("Saving as test-out.json...");
HjsonValue.Save(data, "test-out.json");

Console.WriteLine("Saving as test-out.hjson...");
HjsonValue.Save(data, "test-out.hjson");

// edit (preserve whitespace and comments)
var wdata = (WscJsonObject)HjsonValue.Load("test.hjson", new HjsonOptions { KeepWsc = true }).Qo();

// edit like you normally would
wdata["hugo"] = "value";
// optionally set order and comments:
wdata.Order.Insert(2, "hugo");
wdata.Comments["hugo"] = "just another test";

using var sw = new StringWriter();
HjsonValue.Save(wdata, sw, new HjsonOptions { KeepWsc = true });
Console.WriteLine(sw.ToString());


