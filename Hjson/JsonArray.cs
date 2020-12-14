using System;
using System.Collections;
using System.Collections.Generic;

namespace Hjson
{
	/// <summary>Implements an array value.</summary>
	public class JsonArray: JsonValue, IList<JsonValue>
	{
		private readonly List<JsonValue> list;

		/// <summary>Initializes a new instance of this class.</summary>
		public JsonArray(params JsonValue[] items)
		{
			this.list = new List<JsonValue>();
			this.AddRange(items);
		}

		/// <summary>Initializes a new instance of this class.</summary>
		public JsonArray(IEnumerable<JsonValue> items)
		{
			if (items == null) throw new ArgumentNullException("items");
			this.list = new List<JsonValue>(items);
		}

		/// <summary>Gets the count of the contained items.</summary>
		public override int Count => this.list.Count;

		bool ICollection<JsonValue>.IsReadOnly => false;

		/// <summary>Gets or sets the value for the specified index.</summary>
		public sealed override JsonValue this[int index]
		{
			get => this.list[index];
			set => this.list[index] = value;
		}

		/// <summary>The type of this value.</summary>
		public override JsonType JsonType => JsonType.Array;

		/// <summary>Adds a new item.</summary>
		public void Add(JsonValue item) => this.list.Add(item);

		/// <summary>Adds a range of items.</summary>
		public void AddRange(IEnumerable<JsonValue> items)
		{
			if (items == null) throw new ArgumentNullException("items");
			this.list.AddRange(items);
		}

		/// <summary>Clears the array.</summary>
		public void Clear() => this.list.Clear();

		/// <summary>Determines whether the array contains a specific value.</summary>
		public bool Contains(JsonValue item) => this.list.Contains(item);

		/// <summary>Copies the elements to an System.Array, starting at a particular System.Array index.</summary>
		public void CopyTo(JsonValue[] array, int arrayIndex) => this.list.CopyTo(array, arrayIndex);

		/// <summary>Determines the index of a specific item.</summary>
		public int IndexOf(JsonValue item) => this.list.IndexOf(item);

		/// <summary>Inserts an item.</summary>
		public void Insert(int index, JsonValue item) => this.list.Insert(index, item);

		/// <summary>Removes the specified item.</summary>
		public bool Remove(JsonValue item) => this.list.Remove(item);

		/// <summary>Removes the item with the specified index.</summary>
		public void RemoveAt(int index) => this.list.RemoveAt(index);

		IEnumerator<JsonValue> IEnumerable<JsonValue>.GetEnumerator() => this.list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => this.list.GetEnumerator();
	}
}
