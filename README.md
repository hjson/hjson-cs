# hjson-cs

Hjson, the Human JSON. A data format that caters to humans and helps reduce the errors they make.

For details and syntax see http://laktak.github.io/hjson.

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
