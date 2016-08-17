# hjson-cs

[![Build Status](https://img.shields.io/travis/hjson/hjson-cs.svg?style=flat-square)](http://travis-ci.org/hjson/hjson-cs)
[![nuget version](https://img.shields.io/nuget/v/Hjson.svg?style=flat-square)](https://www.nuget.org/packages/Hjson/)
[![License](https://img.shields.io/github/license/hjson/hjson-cs.svg?style=flat-square)](https://github.com/hjson/hjson-cs/blob/master/LICENSE)

[Hjson](http://hjson.org), the Human JSON. A configuration file format for humans. Relaxed syntax, fewer mistakes, more comments.

![Hjson Intro](http://hjson.org/hjson1.gif)

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

This library includes two readers/writers that fully conform to the respective specification:

- JSON
- Hjson

The C# implementation of Hjson is based on [System.Json](https://github.com/mono/mono). For other platforms see [hjson.org](http://hjson.org).

# Install from nuget

```
Install-Package Hjson
```

# Usage

You can either

- use this libary directly
- or just convert Hjson to JSON and use it with your *favorite JSON library*.

### Convert

```c#
// convert Hjson to JSON
var jsonString = HjsonValue.Load(filePath).ToString();

// convert JSON to Hjson
var hjsonString = JsonValue.Load("test.json").ToString(Stringify.Hjson);
```

### Read

```c#
var jsonObject = HjsonValue.Load(filePath).Qo();
```

`HjsonValue.Load()` will accept both Hjson and JSON. You can use `JsonValue.Load()` to accept JSON input only.

### Object sample

```c#
var jsonObject = HjsonValue.Parse("{\"name\":\"hugo\",\"age\":5}").Qo();
string name = jsonObject.Qs("name");
int age = jsonObject.Qi("age");
// you may prefer to get any value as string
string age2 = jsonObject.Qstr("age");

// or iterate over the members
foreach (var item in jsonObject)
{
  Console.WriteLine("{0}: {1}", item.Key, item.Value);
}
```

### Array sample

```c#
var jsonArray = HjsonValue.Parse("[\"hugo\",5]").Qa();
string first = jsonArray[0];

// or iterate over the members
foreach (var item in jsonArray)
  Console.WriteLine(item.ToValue());
```

### Nested sample

```c#
var nested = HjsonValue.Parse("{\"partner\":{\"name\":\"John\",\"age\":23}}").Qo();
string name = nested.Qo("partner").Qs("name", "default");
int age = nested.Qo("partner").Qi("age", 77);
string gender = nested.Qo("partner").Qs("gender", "unknown");
```

### Create

```c#
var jsonObject = new JsonObject
{
  { "name", "John" },
  { "age", 23 },
};
// -> { "name": "John", "age", 23 }

JsonArray jsonArray = new JsonArray()
{
  "John",
  23,
};
// -> [ "John", 23 ]
```

### Modify

```c#
jsonObject["name"] = "Hugo";
jsonObject.Remove("age");
```

### Write

```c#
HjsonValue.Save(jsonObject, "file.hjson"); // as Hjson
HjsonValue.Save(jsonObject, "file.json"); // as JSON
```

### ToString()

```c#
jsonObject.ToString(Stringify.Hjson); // Hjson output
jsonObject.ToString(Stringify.Formatted); // formatted JSON output
jsonObject.ToString(Stringify.Plain); // plain JSON output, default
jsonObject.ToString(); // plain
```

Also see the [sample](sample).

# API

See [api.md](api.md).

## From the Commandline

A commandline tool to convert from/to Hjson is available in the cli folder.

You can also install it from [chocolatey](https://chocolatey.org/packages/hjson).

For other tools see [hjson.org](http://hjson.org).
