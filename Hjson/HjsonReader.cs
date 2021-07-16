using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Hjson
{
	using JsonPair = KeyValuePair<string, JsonValue>;

	internal class HjsonReader: BaseReader
	{
		private readonly StringBuilder sb = new StringBuilder();
		private readonly IEnumerable<IHjsonDsfProvider> dsfProviders = Enumerable.Empty<IHjsonDsfProvider>();

		public HjsonReader(TextReader reader, IJsonReader jsonReader, HjsonOptions options)
		  : base(reader, jsonReader)
		{
			if (options != null)
			{
				this.ReadWsc = options.KeepWsc;
				this.dsfProviders = options.DsfProviders;
			}
		}

		public JsonValue Read()
		{
			// Braces for the root object are optional

			var c = this.SkipPeekChar();
			switch (c)
			{
				case '[':
				case '{':
				return this.checkTrailing(this.ReadCore());
				default:
				try
				{
					// assume we have a root object without braces
					return this.checkTrailing(this.ReadCore(true));
				}
				catch (Exception)
				{
					// test if we are dealing with a single JSON value instead (true/false/null/num/"")
					this.Reset();
					try { return this.checkTrailing(this.ReadCore()); }
					catch (Exception) { }
					throw; // throw original error
				}
			}
		}

		private JsonValue checkTrailing(JsonValue v)
		{
			this.skipWhite2();
			if (this.ReadChar() >= 0) throw this.ParseError("Extra characters in input");
			return v;
		}

		private void skipWhite2()
		{
			while (this.PeekChar() >= 0)
			{
				while (IsWhite((char)this.PeekChar())) this.ReadChar();
				var p = this.PeekChar();
				if (p == '#' || p == '/' && this.PeekChar(1) == '/')
				{
					for (; ; )
					{
						var ch = this.PeekChar();
						if (ch < 0 || ch == '\n') break;
						this.ReadChar();
					}
				}
				else if (p == '/' && this.PeekChar(1) == '*')
				{
					this.ReadChar(); this.ReadChar();
					for (; ; )
					{
						var ch = this.PeekChar();
						if (ch < 0 || ch == '*' && this.PeekChar(1) == '/') break;
						this.ReadChar();
					}
					if (this.PeekChar() >= 0) { this.ReadChar(); this.ReadChar(); }
				}
				else
				{
					break;
				}
			}
		}

		protected override string GetWhite()
		{
			var res = base.GetWhite();
			var to = res.Length - 1;
			if (to >= 0)
			{
				// remove trailing whitespace
				for (; to > 0 && res[to] <= ' ' && res[to] != '\n'; to--) ;
				// but only up to EOL
				if (res[to] == '\n') to--;
				if (to >= 0 && res[to] == '\r') to--;
				res = res.Substring(0, to + 1);
				foreach (var c in res) if (c > ' ') return res;
			}
			return "";
		}

		public override int SkipPeekChar()
		{
			this.skipWhite2();
			return this.PeekChar();
		}

		private JsonValue ReadCore(bool objectWithoutBraces = false)
		{
			var c = objectWithoutBraces ? '{' : this.SkipPeekChar();
			if (c < 0) throw this.ParseError("Incomplete input");
			switch (c)
			{
				case '[':
				JsonArray list;
				WscJsonArray wscL = null;
				this.ReadChar();
				this.ResetWhite();
				if (this.ReadWsc) list = wscL = new WscJsonArray();
				else list = new JsonArray();
				this.SkipPeekChar();
				if (this.ReadWsc) wscL.Comments.Add(this.GetWhite());
				for (var i = 0; ; i++)
				{
					if (this.SkipPeekChar() == ']') { this.ReadChar(); break; }
					if (this.HasReader) this.Reader.Index(i);
					var value = this.ReadCore();
					if (this.HasReader) this.Reader.Value(value);
					list.Add(value);
					this.ResetWhite();
					if (this.SkipPeekChar() == ',') { this.ReadChar(); this.ResetWhite(); this.SkipPeekChar(); }
					if (this.ReadWsc) wscL.Comments.Add(this.GetWhite());
				}
				return list;
				case '{':
				JsonObject obj;
				WscJsonObject wsc = null;
				if (!objectWithoutBraces)
				{
					this.ReadChar();
					this.ResetWhite();
				}
				if (this.ReadWsc) obj = wsc = new WscJsonObject() { RootBraces = !objectWithoutBraces };
				else obj = new JsonObject();
				this.SkipPeekChar();
				if (this.ReadWsc) wsc.Comments[""] = this.GetWhite();
				for (; ; )
				{
					if (objectWithoutBraces) { if (this.SkipPeekChar() < 0) break; }
					else if (this.SkipPeekChar() == '}') { this.ReadChar(); break; }
					var name = this.readKeyName();
					this.skipWhite2();
					this.Expect(':');
					this.skipWhite2();
					if (this.HasReader) this.Reader.Key(name);
					var value = this.ReadCore();
					if (this.HasReader) this.Reader.Value(value);
					obj.Add(new JsonPair(name, value));
					this.ResetWhite();
					if (this.SkipPeekChar() == ',') { this.ReadChar(); this.ResetWhite(); this.SkipPeekChar(); }
					if (this.ReadWsc) { wsc.Comments[name] = this.GetWhite(); wsc.Order.Add(name); }
				}
				return obj;
				case '\'':
				case '"': return this.ReadStringLiteral(this.readMlString);
				default: return this.readTfnns(c);
			}
		}

		private string readKeyName()
		{
			// quotes for keys are optional in Hjson
			// unless they include {}[],: or whitespace.

			var c = this.PeekChar();
			if (c == '"' || c == '\'') return this.ReadStringLiteral(null);

			this.sb.Length = 0;
			var space = -1;
			for (; ; )
			{
				c = this.PeekChar();
				if (c < 0) throw this.ParseError("Name is not closed");
				var ch = (char)c;
				if (ch == ':')
				{
					if (this.sb.Length == 0) throw this.ParseError("Found ':' but no key name (for an empty key name use quotes)");
					else if (space >= 0 && space != this.sb.Length) throw this.ParseError("Found whitespace in your key name (use quotes to include)");
					return this.sb.ToString();
				}
				else if (IsWhite(ch))
				{
					if (space < 0) space = this.sb.Length;
					this.ReadChar();
				}
				else if (HjsonValue.IsPunctuatorChar(ch))
				{
					throw this.ParseError("Found '" + ch + "' where a key name was expected (check your syntax or use quotes if the key name includes {}[],: or whitespace)");
				}
				else
				{
					this.ReadChar();
					this.sb.Append(ch);
				}
			}
		}

		private void skipIndent(int indent)
		{
			while (indent-- > 0)
			{
				var c = (char)this.PeekChar();
				if (IsWhite(c) && c != '\n') this.ReadChar();
				else break;
			}
		}

		private string readMlString()
		{
			// Parse a multiline string value.
			var triple = 0;
			this.sb.Length = 0;

			// we are at '''
			var indent = this.Column - 3;

			// skip white/to (newline)
			for (; ; )
			{
				var c = (char)this.PeekChar();
				if (IsWhite(c) && c != '\n') this.ReadChar();
				else break;
			}
			if (this.PeekChar() == '\n') { this.ReadChar(); this.skipIndent(indent); }

			// When parsing for string values, we must look for " and \ characters.
			while (true)
			{
				var ch = this.PeekChar();
				if (ch < 0)
				{
					throw this.ParseError("Bad multiline string");
				}
				else if (ch == '\'')
				{
					triple++;
					this.ReadChar();
					if (triple == 3)
					{
						if (this.sb[this.sb.Length - 1] == '\n') this.sb.Length--;
						return this.sb.ToString();
					}
					else
					{
						continue;
					}
				}
				else
				{
					while (triple > 0)
					{
						this.sb.Append('\'');
						triple--;
					}
				}
				if (ch == '\n')
				{
					this.sb.Append('\n');
					this.ReadChar();
					this.skipIndent(indent);
				}
				else
				{
					if (ch != '\r') this.sb.Append((char)ch);
					this.ReadChar();
				}
			}
		}

		internal static bool TryParseNumericLiteral(string text, bool stopAtNext, out JsonValue value)
		{
			int c, leadingZeros = 0, p = 0;
			double val = 0;
			bool negative = false, testLeading = true;
			text += '\0';
			value = null;
			var has_digit = false;

			if (text[p] == '-')
			{
				negative = true;
				p++;
				if (text[p] == 0) return false;
			}

			if (text[p] == '+')
			{
				negative = false;
				p++;
				if (text[p] == 0) return false;
			}

			for (var x = 0; ; x++)
			{
				c = text[p];
				if (c < '0' || c > '9') break;
				if (testLeading)
				{
					if (c == '0') leadingZeros++;
					else testLeading = false;
				}
				val = val * 10 + (c - '0');
				p++;
			}
			if (testLeading) leadingZeros--; // single 0 is allowed
			if (leadingZeros > 0) return false;

			// fraction
			if (text[p] == '.')
			{
				has_digit = true;

				if (leadingZeros < 0) return false;
				var fdigits = 0;
				double frac = 0;
				p++;
				if (text[p] == 0) return false;
				double d = 10;
				for (; ; )
				{
					c = text[p];
					if (c < '0' || '9' < c) break;
					p++;
					frac += (c - '0') / d;
					d *= 10;
					fdigits++;
				}
				if (fdigits == 0) return false;
				val += frac;
			}

			c = text[p];
			if (c == 'e' || c == 'E')
			{
				// exponent
				int exp = 0, expSign = 1;

				p++;
				if (text[p] == 0) return false;

				c = text[p];
				if (c == '-')
				{
					p++;
					expSign = -1;
				}
				else if (c == '+')
				{
					p++;
				}

				if (text[p] == 0) return false;

				for (; ; )
				{
					c = text[p];
					if (c < '0' || c > '9') break;
					exp = exp * 10 + (c - '0');
					p++;
				}

				if (exp != 0)
					val *= Math.Pow(10, exp * expSign);
			}

			while (p < text.Length && IsWhite(text[p])) p++;

			var foundStop = false;
			if (p < text.Length && stopAtNext)
			{
				// end scan if we find a control character like ,}] or a comment
				var ch = text[p];
				if (ch == ',' || ch == '}' || ch == ']' || ch == '#' || ch == '/' && (text.Length > p + 1 && (text[p + 1] == '/' || text[p + 1] == '*')))
					foundStop = true;
			}

			if (p + 1 != text.Length && !foundStop) return false;

			if (negative)
			{
				if (val == 0.0) { value = -0.0; return true; }
				val *= -1;
			}


			var ok = false;
			var str = text;
			if (has_digit)
			{
				ok = double.TryParse(str, System.Globalization.NumberStyles.Float, null, out var num);
				value = num;
			}
			else
			{
				if (negative)
				{
					ok = long.TryParse(str, System.Globalization.NumberStyles.Integer, null, out var num);
					value = num;
				}
				else
				{
					ok = ulong.TryParse(str, System.Globalization.NumberStyles.Integer, null, out var num);
					value = num;
				}
			}

			return ok;

			//var lval = (long)val;
			//if (lval == val) value = lval;
			//else value = val;
			//return true;
		}

		private JsonValue readTfnns(int c)
		{
			if (HjsonValue.IsPunctuatorChar((char)c))
				throw this.ParseError("Found a punctuator character '" + c + "' when expecting a quoteless string (check your syntax)");

			this.sb.Length = 0;
			for (; ; )
			{
				var isEol = c < 0 || c == '\n';
				if (isEol || c == ',' ||
				  c == '}' || c == ']' ||
				  c == '#' ||
				  c == '/' && (this.PeekChar(1) == '/' || this.PeekChar(1) == '*'))
				{
					if (this.sb.Length > 0)
					{
						var ch = this.sb[0];
						switch (ch)
						{
							case 'f': if (this.sb.ToString().Trim() == "false") return false; break;
							case 'n': if (this.sb.ToString().Trim() == "null") return null; break;
							case 't': if (this.sb.ToString().Trim() == "true") return true; break;
							default:
							if ((ch == '-' || ch == '+') || (ch >= '0' && ch <= '9'))
							{
								if (TryParseNumericLiteral(this.sb.ToString(), false, out var res)) return res;
							}
							break;
						}
					}
					if (isEol)
					{
						// remove any whitespace at the end (ignored in quoteless strings)
						return HjsonDsf.Parse(this.dsfProviders, this.sb.ToString().Trim());
					}
				}
				this.ReadChar();
				if (c != '\r') this.sb.Append((char)c);
				c = this.PeekChar();
			}
		}
	}
}
