namespace Hjson
{
  public enum JsonType
  {
    // Null is not used: the primitive will be null instead of the JsonPrimitive containing null
    String,
    Number,
    Object,
    Array,
    Boolean,
  }
}
