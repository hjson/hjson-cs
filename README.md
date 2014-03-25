# hjson-cs

Hjson reference implementation.

Hjson is JSON - commas + comments for Humans.

It should be used for configuration files, for debug output or where it is likely that JSON data is read or will be edited by a human.

That means that you can write:
```
{
  # look, no quotes or commas!
  foo: Hello World!
  bar: Hello Hjson!
}
```

instead of:
```
{
  "foo": "Hello World!",
  "bar": "Hello Hjson!"
}
```

For details see http://laktak.github.io/hjson.


# Install from nuget

```
Install-Package Hjson
```

# Usage

```
var data=(JsonObject)HjsonValue.Load("readme.hjson");
Console.WriteLine((string)data["hello"]);
```

Also see the [sample](sample/HjsonSample).

## From the Commandline

A commandline tool to convert from/to Hjson is available at https://github.com/laktak/hjson-js.
