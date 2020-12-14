namespace Hjson
{
	/// <summary>Defines the reader interface.</summary>
	public interface IJsonReader
	{
		/// <summary>Triggered when an item for an object is read.</summary>
		void Key(string name);
		/// <summary>Triggered when an item for an array is read.</summary>
		void Index(int idx);
		/// <summary>Triggered when a value is read.</summary>
		void Value(JsonValue value);
	}
}
