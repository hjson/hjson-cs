# hjson-cs

[![Build Status](https://img.shields.io/travis/laktak/hjson-cs.svg?style=flat-square)](http://travis-ci.org/laktak/hjson-cs)
[![nuget version](https://img.shields.io/nuget/v/Hjson.svg?style=flat-square)](https://www.nuget.org/packages/Hjson/)

[Hjson](http://hjson.org), the Human JSON. A configuration file format that caters to humans and helps reduce the errors they make.

```
{
  # This is Hjson

  /*

  Why?

  JSON is a great tool that does its job very well. Maybe too well. JSON is a
  great hammer but not everything is a nail.

  Configuration files are edited by end-users, not developers. Users should not
  have to worry about putting commas in the correct place. Software should
  empower the user not hinder him.

  */

  "JSON": "is Hjson",

  but: commas and quotes are optional!
  and: those are allowed: // /**/ #
  so:  less mistakes, more comments ;-)
}
```

For details see [hjson.org](http://hjson.org).

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

Also see the [sample](sample).

# API

See [api.md](api.md).

## From the Commandline

A commandline tool to convert from/to Hjson is available in the cli folder.

For other tools see [hjson.org](http://hjson.org).
