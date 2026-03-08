using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hjson;

/// <summary>Implements an object value, including whitespace and comments.</summary>
public class WscJsonObject : JsonObject
{
    /// <summary>Defines the order of the keys.</summary>
    public List<string> Order { get; private set; } = [];
    /// <summary>Defines a comment for each key. The "" entry is emitted before any element.</summary>
    public Dictionary<string, string> Comments { get; private set; } = [];

    /// <summary>Defines if braces are shown (root object only).</summary>
    public bool RootBraces { get; set; }
}

/// <summary>Implements an array value, including whitespace and comments.</summary>
public class WscJsonArray : JsonArray
{
    /// <summary>Defines a comment for each item. The [0] entry is emitted before any element.</summary>
    public List<string> Comments { get; private set; } = [];
}
