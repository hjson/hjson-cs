# hjson-cs

[![Build Status](https://img.shields.io/travis/laktak/hjson-cs.svg?style=flat-square)](http://travis-ci.org/laktak/hjson-cs)
[![nuget version](https://img.shields.io/nuget/v/Hjson.svg?style=flat-square)](https://www.nuget.org/packages/Hjson/)

[Hjson](http://hjson.org), the Human JSON. A configuration file format that caters to humans and helps reduce the errors they make.

```
{
  # specify rate in requests/second (because comments are helpful!)
  rate: 1000

  // prefer c-style comments?
  /* feeling old fashioned? */

  # did you notice that rate doesn't need quotes?
  hey: look ma, no quotes for strings either!

  # best of all
  notice: []
  anything: ?

  # yes, commas are optional!
}
```

Tested on .net & Mono.

The C# implementation of Hjson is based on [System.Json](https://github.com/mono/mono). For other platforms see [hjson.org](http://hjson.org).

# Install from nuget

```
Install-Package Hjson
```

# Usage

```
var data=(JsonObject)HjsonValue.Load("test.hjson");
Console.WriteLine((string)data["hello"]);

// or
var data=HjsonValue.Load("test.hjson").Qo();
Console.WriteLine(data.Qs("hello"));
```

Also see the [sample](sample).

# API

See [api.md](api.md).

## From the Commandline

A commandline tool to convert from/to Hjson is available in the cli folder.

For other tools see [hjson.org](http://hjson.org).
