namespace Hjson
{
  /// <summary>Defines the known json types.</summary>
  public enum JsonType
  {
    // Null is not used: the primitive will be null instead of the JsonPrimitive containing null

    /// <summary>Json value of type string.</summary>
    String,
    /// <summary>Json value of type number.</summary>
    Number,
    /// <summary>Json value of type object.</summary>
    Object,
    /// <summary>Json value of type array.</summary>
    Array,
    /// <summary>Json value of type boolean.</summary>
    Boolean,
  }
}
