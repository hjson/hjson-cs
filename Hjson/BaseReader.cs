using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hjson
{
	internal abstract class BaseReader
	{
		private readonly string buffer;
		private TextReader r;
		private readonly StringBuilder sb = new StringBuilder();
		private readonly StringBuilder white = new StringBuilder();

		// peek could be removed since we now use a buffer
		private readonly List<int> peek = new List<int>();
		private bool prevLf;

		public int Line { get; private set; }
		public int Column { get; private set; }

		protected IJsonReader Reader { get; private set; }
		protected bool HasReader => this.Reader != null;

		public bool ReadWsc { get; set; }

		public BaseReader(TextReader reader, IJsonReader jsonReader)
		{
			if (reader == null) throw new ArgumentNullException("reader");
			// use a buffer so we can support reset
			this.Reader = jsonReader;
			this.buffer = reader.ReadToEnd();
			this.Reset();
		}

		public void Reset()
		{
			this.Line = 1;
			this.r = new StringReader(this.buffer);
			this.peek.Clear();
			this.white.Length = this.sb.Length = 0;
			this.prevLf = false;
		}

		public int PeekChar(int idx = 0)
		{
			if (idx < 0) throw new ArgumentOutOfRangeException();
			while (idx >= this.peek.Count)
			{
				var c = this.r.Read();
				if (c < 0) return c;
				this.peek.Add(c);
			}
			return this.peek[idx];
		}

		public virtual int SkipPeekChar()
		{
			this.SkipWhite();
			return this.PeekChar();
		}

		public int ReadChar()
		{
			int v;
			if (this.peek.Count > 0)
			{
				// normally peek will only hold not more than one character so this should not matter for performance
				v = this.peek[0];
				this.peek.RemoveAt(0);
			}
			else
			{
				v = this.r.Read();
			}

			if (this.ReadWsc && v != '\r') this.white.Append((char)v);

			if (this.prevLf)
			{
				this.Line++;
				this.Column = 0;
				this.prevLf = false;
			}

			if (v == '\n') this.prevLf = true;
			this.Column++;

			return v;
		}

		protected void ResetWhite()
		{
			if (this.ReadWsc) this.white.Length = 0;
		}

		protected virtual string GetWhite()
		{
			if (!this.ReadWsc) throw new InvalidOperationException();
			return this.white.ToString();
		}

		public static bool IsWhite(char c) => c == ' ' || c == '\t' || c == '\r' || c == '\n';

		public void SkipWhite()
		{
			for (; ; )
			{
				if (IsWhite((char)this.PeekChar())) this.ReadChar();
				else break;
			}
		}

		// It could return either long or double, depending on the parsed value.
		public JsonValue ReadNumericLiteral()
		{
			int c, leadingZeros = 0;
			double val = 0;
			bool negative = false, testLeading = true;

			if (this.PeekChar() == '-')
			{
				negative = true;
				this.ReadChar();
				if (this.PeekChar() < 0) throw this.ParseError("Invalid JSON numeric literal; extra negation");
			}

			for (var x = 0; ; x++)
			{
				c = this.PeekChar();
				if (c < '0' || c > '9') break;
				if (testLeading)
				{
					if (c == '0') leadingZeros++;
					else testLeading = false;
				}
				val = val * 10 + (c - '0');
				this.ReadChar();
			}
			if (testLeading) leadingZeros--; // single 0 is allowed
			if (leadingZeros > 0) throw this.ParseError("leading multiple zeros are not allowed");

			// fraction
			if (this.PeekChar() == '.')
			{
				var fdigits = 0;
				double frac = 0;
				this.ReadChar();
				if (this.PeekChar() < 0) throw this.ParseError("Invalid JSON numeric literal; extra dot");
				double d = 10;
				for (; ; )
				{
					c = this.PeekChar();
					if (c < '0' || '9' < c) break;
					this.ReadChar();
					frac += (c - '0') / d;
					d *= 10;
					fdigits++;
				}
				if (fdigits == 0) throw this.ParseError("Invalid JSON numeric literal; extra dot");
				val += frac;
			}

			c = this.PeekChar();
			if (c == 'e' || c == 'E')
			{
				// exponent
				int exp = 0, expSign = 1;

				this.ReadChar();
				if (this.PeekChar() < 0) throw new ArgumentException("Invalid JSON numeric literal; incomplete exponent");

				c = this.PeekChar();
				if (c == '-')
				{
					this.ReadChar();
					expSign = -1;
				}
				else if (c == '+')
				{
					this.ReadChar();
				}

				if (this.PeekChar() < 0) throw this.ParseError("Invalid JSON numeric literal; incomplete exponent");

				for (; ; )
				{
					c = this.PeekChar();
					if (c < '0' || c > '9') break;
					exp = exp * 10 + (c - '0');
					this.ReadChar();
				}

				if (exp != 0)
					val *= Math.Pow(10, exp * expSign);
			}

			if (negative) val *= -1;
			var lval = (long)val;
			if (lval == val) return lval;
			else return val;
		}

		public string ReadStringLiteral(Func<string> allowML)
		{
			// callers make sure that (exitCh == '"' || exitCh == "'")

			var exitCh = this.ReadChar();
			this.sb.Length = 0;
			for (; ; )
			{
				var c = this.ReadChar();
				if (c < 0) throw this.ParseError("JSON string is not closed");
				if (c == exitCh)
				{
					if (allowML != null && exitCh == '\'' && this.PeekChar() == '\'' && this.sb.Length == 0)
					{
						// ''' indicates a multiline string
						this.ReadChar();
						return allowML();
					}
					else
					{
						return this.sb.ToString();
					}
				}
				else if (c == '\n' || c == '\r')
				{
					throw this.ParseError("Bad string containing newline");
				}
				else if (c != '\\')
				{
					this.sb.Append((char)c);
					continue;
				}

				// escaped expression
				c = this.ReadChar();
				if (c < 0)
					throw this.ParseError("Invalid JSON string literal; incomplete escape sequence");
				switch (c)
				{
					case '"':
					case '\'':
					case '\\':
					case '/': this.sb.Append((char)c); break;
					case 'b': this.sb.Append('\x8'); break;
					case 'f': this.sb.Append('\f'); break;
					case 'n': this.sb.Append('\n'); break;
					case 'r': this.sb.Append('\r'); break;
					case 't': this.sb.Append('\t'); break;
					case 'u':
						ushort cp = 0;
						for (var i = 0; i < 4; i++)
						{
							cp <<= 4;
							if ((c = this.ReadChar()) < 0)
								throw this.ParseError("Incomplete unicode character escape literal");
							if (c >= '0' && c <= '9') cp += (ushort)(c - '0');
							else if (c >= 'A' && c <= 'F') cp += (ushort)(c - 'A' + 10);
							else if (c >= 'a' && c <= 'f') cp += (ushort)(c - 'a' + 10);
							else throw this.ParseError("Bad \\u char " + (char)c);
						}
						this.sb.Append((char)cp);
						break;
					default:
						throw this.ParseError("Invalid JSON string literal; unexpected escape character");
				}
			}
		}

		public void Expect(char expected)
		{
			int c;
			if ((c = this.ReadChar()) != expected)
				throw this.ParseError(string.Format("Expected '{0}', got '{1}'", expected, (char)c));
		}

		public void Expect(string expected)
		{
			for (var i = 0; i < expected.Length; i++)
			{
				if (this.ReadChar() != expected[i])
					throw this.ParseError(string.Format("Expected '{0}', differed at {1}", expected, i));
			}
		}

		public Exception ParseError(string msg) => new ArgumentException(string.Format("{0}. At line {1}, column {2}", msg, this.Line, this.Column));
	}
}
