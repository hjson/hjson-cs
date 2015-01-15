# hjson-cs

[![Build Status](https://img.shields.io/travis/laktak/hjson-cs.svg?style=flat-square)](http://travis-ci.org/laktak/hjson-cs)
[![nuget version](https://img.shields.io/nuget/v/Hjson.svg?style=flat-square)](https://www.nuget.org/packages/Hjson/)

[Hjson](http://hjson.org), the Human JSON. A data format that caters to humans and helps reduce the errors they make.

It supports `#`, `//` and `/**/` style comments as well as avoiding trailing/missing comma and other mistakes. For details and syntax see [hjson.org](http://hjson.org).

Tested on .net & Mono.

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

Also see the [sample](sample/HjsonSample).

# API

See [api.md](api.md).

## From the Commandline

A commandline tool to convert from/to Hjson is available at https://github.com/laktak/hjson-js.

