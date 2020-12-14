using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hjson
{
	internal class HjsonWriter
	{
		private readonly bool writeWsc;
		private readonly bool emitRootBraces;
		private readonly IEnumerable<IHjsonDsfProvider> dsfProviders = Enumerable.Empty<IHjsonDsfProvider>();
		private static readonly Regex needsEscapeName = new Regex(@"[,\{\[\}\]\s:#""']|\/\/|\/\*|'''");

		public HjsonWriter(HjsonOptions options)
		{
			if (options != null)
			{
				this.writeWsc = options.KeepWsc;
				this.emitRootBraces = options.EmitRootBraces;
				this.dsfProviders = options.DsfProviders;
			}
			else
			{
				this.emitRootBraces = true;
			}
		}

		private void nl(TextWriter tw, int level)
		{
			tw.Write(JsonValue.eol);
			//tw.Write(new string(' ', level * 2));
			tw.Write(new string('\t', level));
		}

		private string getWsc(string str)
		{
			if (string.IsNullOrEmpty(str)) return "";
			for (var i = 0; i < str.Length; i++)
			{
				var c = str[i];
				if (c == '\n' ||
				  c == '#' ||
				  c == '/' && i + 1 < str.Length && (str[i + 1] == '/' || str[i + 1] == '*'))
				{
					break;
				}

				if (c > ' ') return $"// {str}\n";
			}
			return str;
		}

		private string getWsc(Dictionary<string, string> white, string key) => white.ContainsKey(key) ? this.getWsc(white[key]) : "";

		private string getWsc(List<string> white, int index) => white.Count > index ? this.getWsc(white[index]) : "";

		private bool testWsc(string str) => str.Length > 0 && str[str[0] == '\r' && str.Length > 1 ? 1 : 0] != '\n';

		public void Save(JsonValue value, TextWriter tw, int level, bool hasComment, string separator, bool noIndent = false, bool isRootObject = false)
		{
			if (value == null)
			{
				tw.Write(separator);
				tw.Write("null");
				return;
			}

			// check for DSF
			var dsfValue = HjsonDsf.Stringify(this.dsfProviders, value);
			if (dsfValue != null)
			{
				tw.Write(separator);
				tw.Write(dsfValue);
				return;
			}

			switch (value.JsonType)
			{
				case JsonType.Object:
					var obj = value.Qo();
					var kw = this.writeWsc ? obj as WscJsonObject : null;
					var showBraces = !isRootObject || (kw != null ? kw.RootBraces : this.emitRootBraces);
					if (!noIndent)
					{
						this.nl(tw, level);
					}
					if (showBraces) tw.Write('{');
					else level--; // reduce level for root
					if (kw != null)
					{
						var kwl = this.getWsc(kw.Comments, "");
						foreach (var key in kw.Order.Concat(kw.Keys).Distinct())
						{
							if (!obj.ContainsKey(key)) continue;
							var val = obj[key];
							tw.Write(kwl);
							this.nl(tw, level + 1);
							kwl = this.getWsc(kw.Comments, key);

							tw.Write(escapeName(key));
							tw.Write(":");
							this.Save(val, tw, level + 1, this.testWsc(kwl), " ");
						}
						tw.Write(kwl);
						if (showBraces) this.nl(tw, level);
					}
					else
					{
						var skipFirst = !showBraces;
						foreach (var pair in obj)
						{
							if (!skipFirst) this.nl(tw, level + 1); else skipFirst = false;

							if (!string.IsNullOrEmpty(pair.Value.Comment))
							{
								tw.Write($"// {pair.Value.Comment}");
								this.nl(tw, level + 1);
							}

							tw.Write(escapeName(pair.Key));
							tw.Write(":");
							this.Save(pair.Value, tw, level + 1, false, " ");
						}
						if (showBraces) this.nl(tw, level);
					}
					if (showBraces) tw.Write('}');
					if (level == 0) tw.Write(JsonValue.eol);
					break;
				case JsonType.Array:
					int i = 0, n = value.Count;
					if (!noIndent)
					{
						if (n > 0 && !value.Inline) this.nl(tw, level);
						else tw.Write(separator);
					}
					tw.Write('[');
					WscJsonArray whiteL = null;
					string wsl = null;
					if (this.writeWsc)
					{
						whiteL = value as WscJsonArray;
						if (whiteL != null) wsl = this.getWsc(whiteL.Comments, 0);
					}
					for (; i < n; i++)
					{
						var v = value[i];
						if (whiteL != null)
						{
							tw.Write(wsl);
							wsl = this.getWsc(whiteL.Comments, i + 1);
						}

						if (value.Inline)
						{
							this.Save(v, tw, level + 1, wsl != null && this.testWsc(wsl), (i == 0) ? " " : ", ", true);
						}
						else
						{
							this.nl(tw, level + 1);
							this.Save(v, tw, level + 1, wsl != null && this.testWsc(wsl), "", true);
						}
					}
					if (whiteL != null) tw.Write(wsl);
					if (n > 0 && !value.Inline)
					{
						this.nl(tw, level);
						tw.Write(']');
					}
					else
					{
						tw.Write(" ]");
					}
					break;
				case JsonType.Boolean:
					tw.Write(separator);
					tw.Write(value ? "true" : "false");
					break;
				case JsonType.String:
					this.writeString(((JsonPrimitive)value).GetRawString(), tw, level, hasComment, separator);
					break;
				default:
					tw.Write(separator);
					tw.Write(((JsonPrimitive)value).GetRawString());
					break;
			}
		}

		private static string escapeName(string name)
		{
			if (name.Length == 0 || needsEscapeName.IsMatch(name))
				return "\"" + JsonWriter.EscapeString(name) + "\"";
			else
				return name;
		}

		private void writeString(string value, TextWriter tw, int level, bool hasComment, string separator)
		{
			if (value == "") { tw.Write(separator + "\"\""); return; }

			char left = value[0], right = value[value.Length - 1];
			char left1 = value.Length > 1 ? value[1] : '\0', left2 = value.Length > 2 ? value[2] : '\0';
			var doEscape = true; // hasComment || value.Any(c => needsQuotes(c));

			if (doEscape ||
			  BaseReader.IsWhite(left) || BaseReader.IsWhite(right) ||
			  left == '"' ||
			  left == '\'' ||
			  left == '#' ||
			  left == '/' && (left1 == '*' || left1 == '/') ||
			  HjsonValue.IsPunctuatorChar(left) ||
			  HjsonReader.TryParseNumericLiteral(value, true, out var dummy) ||
			  startsWithKeyword(value))
			{
				// If the string contains no control characters, no quote characters, and no
				// backslash characters, then we can safely slap some quotes around it.
				// Otherwise we first check if the string can be expressed in multiline
				// format or we must replace the offending characters with safe escape
				// sequences.

				if (!value.Any(c => needsEscape(c))) tw.Write(separator + "\"" + value + "\"");
				else if (!value.Any(c => needsEscapeML(c)) && !value.Contains("'''") && !value.All(c => BaseReader.IsWhite(c))) this.writeMLString(value, tw, level, separator);
				else tw.Write(separator + "\"" + JsonWriter.EscapeString(value) + "\"");
			}
			else
			{
				tw.Write(separator + value);
			}
		}

		private void writeMLString(string value, TextWriter tw, int level, string separator)
		{
			var lines = value.Replace("\r", "").Split('\n');

			if (lines.Length == 1)
			{
				tw.Write(separator + "'''");
				tw.Write(lines[0]);
				tw.Write("'''");
			}
			else
			{
				level++;
				this.nl(tw, level);
				tw.Write("'''");

				foreach (var line in lines)
				{
					this.nl(tw, !string.IsNullOrEmpty(line) ? level : 0);
					tw.Write(line);
				}
				this.nl(tw, level);
				tw.Write("'''");
			}
		}

		private static bool startsWithKeyword(string text)
		{
			int p;
			if (text.StartsWith("true") || text.StartsWith("null")) p = 4;
			else if (text.StartsWith("false")) p = 5;
			else return false;
			while (p < text.Length && BaseReader.IsWhite(text[p])) p++;
			if (p == text.Length) return true;
			var ch = text[p];
			return ch == ',' || ch == '}' || ch == ']' || ch == '#' || ch == '/' && (text.Length > p + 1 && (text[p + 1] == '/' || text[p + 1] == '*'));
		}

		private static bool needsQuotes(char c)
		{
			switch (c)
			{
				case '\t':
				case '\f':
				case '\b':
				case '\n':
				case '\r':
					return true;
				default:
					return false;
			}
		}

		private static bool needsEscape(char c)
		{
			switch (c)
			{
				case '\"':
				case '\\':
					return true;
				default:
					return needsQuotes(c);
			}
		}

		private static bool needsEscapeML(char c)
		{
			switch (c)
			{
				case '\n':
				case '\r':
				case '\t':
					return false;
				default:
					return needsQuotes(c);
			}
		}
	}
}
