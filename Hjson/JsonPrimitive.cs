using System;
using System.Globalization;
using System.Numerics;

namespace Hjson
{
	/// <summary>Implements a primitive value.</summary>
	public class JsonPrimitive: JsonValue
	{
		private object value;

		/// <summary>Initializes a new string.</summary>
		public JsonPrimitive(string value)
		{
			this.value = value;
		}

		/// <summary>Initializes a new char.</summary>
		public JsonPrimitive(char value)
		{
			this.value = value.ToString();
		}

		/// <summary>Initializes a new bool.</summary>
		public JsonPrimitive(bool value)
		{
			this.value = value;
		}

		/// <summary>Initializes a new decimal.</summary>
		public JsonPrimitive(decimal value)
		{
			this.value = value;
		}

		/// <summary>Initializes a new double.</summary>
		public JsonPrimitive(double value)
		{
			this.value = value;
		}

		/// <summary>Initializes a new float.</summary>
		public JsonPrimitive(float value)
		{
			this.value = (double)value;
		}

		/// <summary>Initializes a new long.</summary>
		public JsonPrimitive(long value)
		{
			this.value = value;
		}

		/// <summary>Initializes a new ulong.</summary>
		public JsonPrimitive(ulong value)
		{
			this.value = value;
		}

		/// <summary>Initializes a new int.</summary>
		public JsonPrimitive(int value)
		{
			this.value = (long)value;
		}

		/// <summary>Initializes a new byte.</summary>
		public JsonPrimitive(byte value)
		{
			this.value = (long)value;
		}

		/// <summary>Initializes a new short.</summary>
		public JsonPrimitive(short value)
		{
			this.value = (long)value;
		}

		private JsonPrimitive() 
		{ 
		
		}

		public static new JsonPrimitive FromObject(object value)
		{
			return new JsonPrimitive { value = value };
		}

		internal object Value => this.value;

		/// <summary>The type of this value.</summary>
		public override JsonType JsonType
		{
			get
			{
				if (this.value == null) return JsonType.String;

				var type = this.value.GetType();
				if (type == typeof(bool)) return JsonType.Boolean;
				else if (type == typeof(string)) return JsonType.String;
				else if (type == typeof(byte) ||
				  type == typeof(sbyte) ||
				  type == typeof(short) ||
				  type == typeof(ushort) ||
				  type == typeof(int) ||
				  type == typeof(uint) ||
				  type == typeof(long) ||
				  type == typeof(ulong) ||
				  type == typeof(float) ||
				  type == typeof(double) ||
				  type == typeof(decimal))
				{
					return JsonType.Number;
				}

				return JsonType.Unknown;
			}
		}

		internal string GetRawString()
		{
			switch (this.JsonType)
			{
				case JsonType.String:
					return ((string)this.value) ?? "";
				case JsonType.Number:
					// use ToLowerInvariant() to convert E to e
					return ((IFormattable)this.value).ToString("G", NumberFormatInfo.InvariantInfo).ToLowerInvariant();
				default:
					throw new InvalidOperationException();
			}
		}
	}
}
