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

  # don't bother with escapes
  html: <div class="hello">world</div>

  # Hjson is a superset so the normal JSON syntax can be used
  "array": [ 1, "two" ]
}
```

instead of:
```
{
  "foo": "Hello World!",
  "bar": "Hello Hjson!",
  "html": "<div class=\"hello\">world</div>",
  "array": [ 1, "two" ]
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

// or
var data=HjsonValue.Load("readme.hjson").Qo();
Console.WriteLine(data.Qs("hello"));
```

Also see the [sample](sample/HjsonSample).

# API

See [api.md](api.md).

## From the Commandline

A commandline tool to convert from/to Hjson is available at https://github.com/laktak/hjson-js.
