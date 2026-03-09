#nullable enable
using System;

using Hjson;

string? file = null;
var todo = Stringify.Hjson;
bool err = false, roundtrip = false, rootBraces = true;

foreach (var arg in args)
{
    switch (arg)
    {
        case "-j": todo = Stringify.Formatted; break;
        case "-c": todo = Stringify.Plain; break;
        case "-h": todo = Stringify.Hjson; break;
        case "-r": roundtrip = true; todo = Stringify.Hjson; break;
        case "-n": rootBraces = false; break;
        default:
            if (!arg.StartsWith('-') && file is null)
                file = arg;
            else
                err = true;
            break;
    }
}

if (err || file is null)
{
    Console.WriteLine("""
        hjsonc [OPTION] FILE
        Options:
          -h  Hjson output (default)
          -r  Hjson output, round trip with comments
          -j  JSON output (formatted)
          -c  JSON output (compact)
          -n  omit braces for the root object (Hjson).
        """);
    return 1;
}

var data = HjsonValue.Load(file, new HjsonOptions { KeepWsc = roundtrip });

if (todo == Stringify.Hjson)
    Console.WriteLine(data.ToString(new HjsonOptions { KeepWsc = roundtrip, EmitRootBraces = rootBraces }));
else
    Console.WriteLine(data.ToString(todo));

return 0;