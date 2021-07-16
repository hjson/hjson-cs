using System.Collections.Generic;
using System.IO;

namespace Hjson
{
	using JsonPair = KeyValuePair<string, JsonValue>;

	internal class JsonReader: BaseReader
	{
		public JsonReader(TextReader reader, IJsonReader jsonReader)
		  : base(reader, jsonReader)
		{
		}

		public JsonValue Read()
		{
			var v = this.ReadCore();
			this.SkipWhite();
			if (this.ReadChar() >= 0) throw this.ParseError("Extra characters in JSON input");
			return v;
		}

		private JsonValue ReadCore()
		{
			var c = this.SkipPeekChar();
			if (c < 0) throw this.ParseError("Incomplete JSON input");
			switch (c)
			{
				case '[':
					this.ReadChar();
					if (this.SkipPeekChar() == ']')
					{
						this.ReadChar();
						return new JsonArray();
					}
					var list = new List<JsonValue>();
					for (var i = 0; ; i++)
					{
						if (this.HasReader) this.Reader.Index(i);
						var value = this.ReadCore();
						if (this.HasReader) this.Reader.Value(value);
						list.Add(value);
						c = this.SkipPeekChar();
						if (c != ',') break;
						this.ReadChar();
					}
					if (this.ReadChar() != ']')
						throw this.ParseError("Array must end with ']'");
					return new JsonArray(list);
				case '{':
					this.ReadChar();
					if (this.SkipPeekChar() == '}')
					{
						this.ReadChar();
						return new JsonObject();
					}
					var obj = new List<JsonPair>();
					for (; ; )
					{
						if (this.SkipPeekChar() == '}') { this.ReadChar(); break; }
						if (this.PeekChar() != '"') throw this.ParseError("Invalid JSON string literal format");
						var name = this.ReadStringLiteral(null);
						this.SkipWhite();
						this.Expect(':');
						this.SkipWhite();
						if (this.HasReader) this.Reader.Key(name);
						var value = this.ReadCore();
						if (this.HasReader) this.Reader.Value(value);
						obj.Add(new JsonPair(name, value));
						this.SkipWhite();
						c = this.ReadChar();
						if (c == '}') break;
						//if (c==',') continue;
					}
					return new JsonObject(obj);
				case 't':
					this.Expect("true");
					return true;
				case 'f':
					this.Expect("false");
					return false;
				case 'n':
					this.Expect("null");
					return null;
				case '"':
					return this.ReadStringLiteral(null);
				default:
					if (c >= '0' && c <= '9' || c == '-' || c == '+')
						return this.ReadNumericLiteral();
					else
						throw this.ParseError(string.Format("Unexpected character '{0}'", (char)c));
			}
		}
	}
}
