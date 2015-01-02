## Hjson ##

# T:Hjson.HjsonValue

Contains functions to load and save in the Hjson format.



##### M:Hjson.HjsonValue.Load(System.String)

Loads Hjson/JSON from a file.



##### M:Hjson.HjsonValue.Load(System.IO.Stream)

Loads Hjson/JSON from a stream.



##### M:Hjson.HjsonValue.Load(System.IO.TextReader,Hjson.IJsonReader)

Loads Hjson/JSON from a TextReader.



##### M:Hjson.HjsonValue.LoadWsc(System.IO.TextReader)

Loads Hjson/JSON from a TextReader, preserving whitespace and comments.



##### M:Hjson.HjsonValue.Parse(System.String)

Parses the specified Hjson/JSON string.



##### M:Hjson.HjsonValue.Save(Hjson.JsonValue,System.String)

Saves Hjson to a file.



##### M:Hjson.HjsonValue.Save(Hjson.JsonValue,System.IO.Stream)

Saves Hjson to a stream.



##### M:Hjson.HjsonValue.Save(Hjson.JsonValue,System.IO.TextWriter)

Saves Hjson to a TextWriter.



##### M:Hjson.HjsonValue.SaveWsc(Hjson.JsonValue,System.IO.TextWriter)

Saves Hjson to a string, adding whitespace and comments.



##### M:Hjson.HjsonValue.SaveAsString(Hjson.JsonValue)

Saves Hjson to a string.



# T:Hjson.WscJsonObject

Implements an object value, including whitespace and comments.



# T:Hjson.JsonObject

Implements an object value.



# T:Hjson.JsonValue

 JsonValue is the abstract base class for all values (string, number, true, false, null, object or array). 



##### M:Hjson.JsonValue.ContainsKey(System.String)

Returns true if the object contains the specified key.



##### M:Hjson.JsonValue.Save(System.String,Hjson.Stringify)

Saves the JSON to a file.



##### M:Hjson.JsonValue.Save(System.IO.Stream,Hjson.Stringify)

Saves the JSON to a stream.



##### M:Hjson.JsonValue.Save(System.IO.TextWriter,Hjson.Stringify)

Saves the JSON to a TextWriter.



##### M:Hjson.JsonValue.SaveAsString(System.Boolean)

Saves the JSON to a string.



##### M:Hjson.JsonValue.ToString(Hjson.Stringify)

Saves the JSON to a string.



##### M:Hjson.JsonValue.ToString

Saves the JSON to a string.



##### M:Hjson.JsonValue.ToValue

Returns the contained primitive value.



##### M:Hjson.JsonValue.Load(System.String)

Loads JSON from a file.



##### M:Hjson.JsonValue.Load(System.IO.Stream)

Loads JSON from a stream.



##### M:Hjson.JsonValue.Load(System.IO.TextReader,Hjson.IJsonReader)

Loads JSON from a TextReader.



##### M:Hjson.JsonValue.Parse(System.String)

Parses the specified JSON string.



##### M:Hjson.JsonValue.op_Implicit(System.Boolean)~Hjson.JsonValue

Converts from bool.



##### M:Hjson.JsonValue.op_Implicit(System.Byte)~Hjson.JsonValue

Converts from byte.



##### M:Hjson.JsonValue.op_Implicit(System.Char)~Hjson.JsonValue

Converts from char.



##### M:Hjson.JsonValue.op_Implicit(System.Decimal)~Hjson.JsonValue

Converts from decimal.



##### M:Hjson.JsonValue.op_Implicit(System.Double)~Hjson.JsonValue

Converts from double.



##### M:Hjson.JsonValue.op_Implicit(System.Single)~Hjson.JsonValue

Converts from float.



##### M:Hjson.JsonValue.op_Implicit(System.Int32)~Hjson.JsonValue

Converts from int.



##### M:Hjson.JsonValue.op_Implicit(System.Int64)~Hjson.JsonValue

Converts from long.



##### M:Hjson.JsonValue.op_Implicit(System.Int16)~Hjson.JsonValue

Converts from short.



##### M:Hjson.JsonValue.op_Implicit(System.String)~Hjson.JsonValue

Converts from string.



##### M:Hjson.JsonValue.op_Implicit(Hjson.JsonValue)~System.Boolean

Converts to bool. Also see [[|T:Hjson.JsonUtil]].



##### M:Hjson.JsonValue.op_Implicit(Hjson.JsonValue)~System.Byte

Converts to byte. Also see [[|T:Hjson.JsonUtil]].



##### M:Hjson.JsonValue.op_Implicit(Hjson.JsonValue)~System.Char

Converts to char. Also see [[|T:Hjson.JsonUtil]].



##### M:Hjson.JsonValue.op_Implicit(Hjson.JsonValue)~System.Decimal

Converts to decimal. Also see [[|T:Hjson.JsonUtil]].



##### M:Hjson.JsonValue.op_Implicit(Hjson.JsonValue)~System.Double

Converts to double. Also see [[|T:Hjson.JsonUtil]].



##### M:Hjson.JsonValue.op_Implicit(Hjson.JsonValue)~System.Single

Converts to float. Also see [[|T:Hjson.JsonUtil]].



##### M:Hjson.JsonValue.op_Implicit(Hjson.JsonValue)~System.Int32

Converts to int. Also see [[|T:Hjson.JsonUtil]].



##### M:Hjson.JsonValue.op_Implicit(Hjson.JsonValue)~System.Int64

Converts to long. Also see [[|T:Hjson.JsonUtil]].



##### M:Hjson.JsonValue.op_Implicit(Hjson.JsonValue)~System.Int16

Converts to short. Also see [[|T:Hjson.JsonUtil]].



##### M:Hjson.JsonValue.op_Implicit(Hjson.JsonValue)~System.String

Converts to string. Also see [[|T:Hjson.JsonUtil]].



##### P:Hjson.JsonValue.Count

Gets the count of the contained items for arrays/objects.



##### P:Hjson.JsonValue.JsonType

The type of this value.



##### P:Hjson.JsonValue.Item(System.Int32)

Gets or sets the value for the specified index.



##### P:Hjson.JsonValue.Item(System.String)

Gets or sets the value for the specified key.



##### M:Hjson.JsonObject.#ctor(System.Collections.Generic.KeyValuePair{System.String,Hjson.JsonValue}[])

Initializes a new instance of this class.



>You can also initialize an object using the C# add syntax: new JsonObject { { "key", "value" }, ... }



##### M:Hjson.JsonObject.#ctor(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String,Hjson.JsonValue}})

Initializes a new instance of this class.



>You can also initialize an object using the C# add syntax: new JsonObject { { "key", "value" }, ... }



##### M:Hjson.JsonObject.Add(System.String,Hjson.JsonValue)

Adds a new item.



>You can also initialize an object using the C# add syntax: new JsonObject { { "key", "value" }, ... }



##### M:Hjson.JsonObject.Add(System.Collections.Generic.KeyValuePair{System.String,Hjson.JsonValue})

Adds a new item.



##### M:Hjson.JsonObject.AddRange(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String,Hjson.JsonValue}})

Adds a range of items.



##### M:Hjson.JsonObject.Clear

Clears the object.



##### M:Hjson.JsonObject.ContainsKey(System.String)

Determines whether the array contains a specific key.



##### M:Hjson.JsonObject.CopyTo(System.Collections.Generic.KeyValuePair{System.String,Hjson.JsonValue}[],System.Int32)

Copies the elements to an System.Array, starting at a particular System.Array index.



##### M:Hjson.JsonObject.Remove(System.String)

Removes the item with the specified key.



##### M:Hjson.JsonObject.TryGetValue(System.String,Hjson.JsonValue@)

Gets the value associated with the specified key.



##### P:Hjson.JsonObject.Count

Gets the count of the contained items.



##### P:Hjson.JsonObject.Item(System.String)

Gets or sets the value for the specified key.



##### P:Hjson.JsonObject.JsonType

The type of this value.



##### P:Hjson.JsonObject.Keys

Gets the keys of this object.



##### P:Hjson.JsonObject.Values

Gets the values of this object.



##### M:Hjson.WscJsonObject.#ctor

Initializes a new instance of this class.



##### P:Hjson.WscJsonObject.Order

Defines the order of the keys.



##### P:Hjson.WscJsonObject.Comments

Defines a comment for each key. The "" entry is emitted before any element.



# T:Hjson.WscJsonArray

Implements an array value, including whitespace and comments.



# T:Hjson.JsonArray

Implements an array value.



##### M:Hjson.JsonArray.#ctor(Hjson.JsonValue[])

Initializes a new instance of this class.



##### M:Hjson.JsonArray.#ctor(System.Collections.Generic.IEnumerable{Hjson.JsonValue})

Initializes a new instance of this class.



##### M:Hjson.JsonArray.Add(Hjson.JsonValue)

Adds a new item.



##### M:Hjson.JsonArray.AddRange(System.Collections.Generic.IEnumerable{Hjson.JsonValue})

Adds a range of items.



##### M:Hjson.JsonArray.Clear

Clears the array.



##### M:Hjson.JsonArray.Contains(Hjson.JsonValue)

Determines whether the array contains a specific value.



##### M:Hjson.JsonArray.CopyTo(Hjson.JsonValue[],System.Int32)

Copies the elements to an System.Array, starting at a particular System.Array index.



##### M:Hjson.JsonArray.IndexOf(Hjson.JsonValue)

Determines the index of a specific item.



##### M:Hjson.JsonArray.Insert(System.Int32,Hjson.JsonValue)

Inserts an item.



##### M:Hjson.JsonArray.Remove(Hjson.JsonValue)

Removes the specified item.



##### M:Hjson.JsonArray.RemoveAt(System.Int32)

Removes the item with the specified index.



##### P:Hjson.JsonArray.Count

Gets the count of the contained items.



##### P:Hjson.JsonArray.Item(System.Int32)

Gets or sets the value for the specified index.



##### P:Hjson.JsonArray.JsonType

The type of this value.



##### M:Hjson.WscJsonArray.#ctor

Initializes a new instance of this class.



##### P:Hjson.WscJsonArray.Comments

Defines a comment for each item. The [0] entry is emitted before any element.



# T:Hjson.IJsonReader

Defines the reader interface.



##### M:Hjson.IJsonReader.Key(System.String)

Triggered when an item for an object is read.



##### M:Hjson.IJsonReader.Index(System.Int32)

Triggered when an item for an array is read.



##### M:Hjson.IJsonReader.Value(Hjson.JsonValue)

Triggered when a value is read.



# T:Hjson.JsonPrimitive

Implements a primitive value.



##### M:Hjson.JsonPrimitive.#ctor(System.String)

Initializes a new string.



##### M:Hjson.JsonPrimitive.#ctor(System.Char)

Initializes a new char.



##### M:Hjson.JsonPrimitive.#ctor(System.Boolean)

Initializes a new bool.



##### M:Hjson.JsonPrimitive.#ctor(System.Decimal)

Initializes a new decimal.



##### M:Hjson.JsonPrimitive.#ctor(System.Double)

Initializes a new double.



##### M:Hjson.JsonPrimitive.#ctor(System.Single)

Initializes a new float.



##### M:Hjson.JsonPrimitive.#ctor(System.Int64)

Initializes a new long.



##### M:Hjson.JsonPrimitive.#ctor(System.Int32)

Initializes a new int.



##### M:Hjson.JsonPrimitive.#ctor(System.Byte)

Initializes a new byte.



##### M:Hjson.JsonPrimitive.#ctor(System.Int16)

Initializes a new short.



##### P:Hjson.JsonPrimitive.JsonType

The type of this value.



# T:Hjson.JsonUtil

Provides Json extension methods.



##### M:Hjson.JsonUtil.Qb(Hjson.JsonValue)

Gets the bool from a JsonValue.



##### M:Hjson.JsonUtil.Qb(Hjson.JsonObject,System.String,System.Boolean)

Gets the bool value of a key in a JsonObject.



##### M:Hjson.JsonUtil.Qi(Hjson.JsonValue)

Gets the int from a JsonValue.



##### M:Hjson.JsonUtil.Qi(Hjson.JsonObject,System.String,System.Int32)

Gets the int value of a key in a JsonObject.



##### M:Hjson.JsonUtil.Ql(Hjson.JsonValue)

Gets the long from a JsonValue.



##### M:Hjson.JsonUtil.Ql(Hjson.JsonObject,System.String,System.Int64)

Gets the long value of a key in a JsonObject.



##### M:Hjson.JsonUtil.Qd(Hjson.JsonValue)

Gets the double from a JsonValue.



##### M:Hjson.JsonUtil.Qd(Hjson.JsonObject,System.String,System.Double)

Gets the double value of a key in a JsonObject.



##### M:Hjson.JsonUtil.Qs(Hjson.JsonValue)

Gets the string from a JsonValue.



##### M:Hjson.JsonUtil.Qs(Hjson.JsonObject,System.String,System.String)

Gets the string value of a key in a JsonObject.



##### M:Hjson.JsonUtil.Qv(Hjson.JsonObject,System.String)

Gets the JsonValue of a key in a JsonObject.



##### M:Hjson.JsonUtil.Qo(Hjson.JsonObject,System.String)

Gets a JsonObject from a JsonObject.



##### M:Hjson.JsonUtil.Qo(Hjson.JsonValue)

Gets the JsonObject from a JsonValue.



##### M:Hjson.JsonUtil.Qa(Hjson.JsonObject,System.String)

Gets a JsonArray from a JsonObject.



##### M:Hjson.JsonUtil.Qa(Hjson.JsonValue)

Gets the JsonArray from a JsonValue.



##### M:Hjson.JsonUtil.Qqo(Hjson.JsonObject)

Enumerates JsonObjects from a JsonObject.



##### M:Hjson.JsonUtil.ToJsonDate(System.DateTime)

Convert the date to json (unix epoch date offset).



##### M:Hjson.JsonUtil.ToDateTime(System.Int64)

Convert the json date (unix epoch date offset) to a DateTime.



# T:Hjson.JsonType

Defines the known json types.



>There is no null type as the primitive will be null instead of the JsonPrimitive containing null.



##### F:Hjson.JsonType.String

Json value of type string.



##### F:Hjson.JsonType.Number

Json value of type number.



##### F:Hjson.JsonType.Object

Json value of type object.



##### F:Hjson.JsonType.Array

Json value of type array.



##### F:Hjson.JsonType.Boolean

Json value of type boolean.



# T:Hjson.Stringify

The ToString format.



##### F:Hjson.Stringify.Plain

JSON (no whitespace).



##### F:Hjson.Stringify.Formatted

Formatted JSON.



##### F:Hjson.Stringify.Hjson

Hjson.






