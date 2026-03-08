using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Hjson;

/// <summary>Implements an array value.</summary>
public class JsonArray : JsonValue, IList<JsonValue>
{
    readonly List<JsonValue> list;

    /// <summary>Initializes a new instance of this class.</summary>
    public JsonArray(params JsonValue[] items)
    {
        list = [];
        AddRange(items);
    }

    /// <summary>Initializes a new instance of this class.</summary>
    public JsonArray(IEnumerable<JsonValue> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        list = [.. items];
    }

    /// <summary>Gets the count of the contained items.</summary>
    public override int Count => list.Count;

    bool ICollection<JsonValue>.IsReadOnly => false;

    /// <summary>Gets or sets the value for the specified index.</summary>
    public override sealed JsonValue this[int index]
    {
        get => list[index];
        set => list[index] = value;
    }

    /// <summary>The type of this value.</summary>
    public override JsonType JsonType => JsonType.Array;

    /// <summary>Adds a new item.</summary>
    public void Add(JsonValue item) => list.Add(item);

    /// <summary>Adds a range of items.</summary>
    public void AddRange(IEnumerable<JsonValue> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        list.AddRange(items);
    }

    /// <summary>Clears the array.</summary>
    public void Clear() => list.Clear();

    /// <summary>Determines whether the array contains a specific value.</summary>
    public bool Contains(JsonValue item) => list.Contains(item);

    /// <summary>Copies the elements to an System.Array, starting at a particular System.Array index.</summary>
    public void CopyTo(JsonValue[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

    /// <summary>Determines the index of a specific item.</summary>
    public int IndexOf(JsonValue item) => list.IndexOf(item);

    /// <summary>Inserts an item.</summary>
    public void Insert(int index, JsonValue item) => list.Insert(index, item);

    /// <summary>Removes the specified item.</summary>
    public bool Remove(JsonValue item) => list.Remove(item);

    /// <summary>Removes the item with the specified index.</summary>
    public void RemoveAt(int index) => list.RemoveAt(index);

    IEnumerator<JsonValue> IEnumerable<JsonValue>.GetEnumerator() => list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();
}