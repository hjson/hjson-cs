using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hjson
{
	internal class JsonWriter
	{
		private readonly bool format;

		public JsonWriter(bool format) => this.format = format;

		private void nl(TextWriter tw, int level)
		{
			if (this.format)
			{
				tw.Write(JsonValue.eol);
				tw.Write(new string(' ', level * 2));
			}
		}

		public void Save(JsonValue value, TextWriter tw, int level)
		{
			var following = false;
			switch (value.JsonType)
			{
				case JsonType.Object:
					if (level > 0) this.nl(tw, level);
					tw.Write('{');
					foreach (var pair in ((JsonObject)value))
					{
						if (following) tw.Write(",");
						this.nl(tw, level + 1);
						tw.Write('\"');
						tw.Write(EscapeString(pair.Key));
						tw.Write("\":");
						var nextType = pair.Value != null ? (JsonType?)pair.Value.JsonType : null;
						if (this.format && nextType != JsonType.Array && nextType != JsonType.Object) tw.Write(" ");
						if (pair.Value == null) tw.Write("null");
						else this.Save(pair.Value, tw, level + 1);
						following = true;
					}
					if (following) this.nl(tw, level);
					tw.Write('}');
					break;
				case JsonType.Array:
					if (level > 0) this.nl(tw, level);
					tw.Write('[');
					foreach (var v in ((JsonArray)value))
					{
						if (following) tw.Write(",");
						if (v != null)
						{
							if (v.JsonType != JsonType.Array && v.JsonType != JsonType.Object) this.nl(tw, level + 1);
							this.Save(v, tw, level + 1);
						}
						else
						{
							this.nl(tw, level + 1);
							tw.Write("null");
						}
						following = true;
					}
					if (following) this.nl(tw, level);
					tw.Write(']');
					break;
				case JsonType.Boolean:
					tw.Write(value ? "true" : "false");
					break;
				case JsonType.String:
					tw.Write('"');
					tw.Write(EscapeString(((JsonPrimitive)value).GetRawString()));
					tw.Write('"');
					break;
				default:
					tw.Write(((JsonPrimitive)value).GetRawString());
					break;
			}
		}

		internal static string EscapeString(string src)
		{
			if (src == null) return null;

			for (var i = 0; i < src.Length; i++)
			{
				if (getEscapedChar(src[i]) != null)
				{
					var sb = new StringBuilder();
					if (i > 0) sb.Append(src, 0, i);
					return doEscapeString(sb, src, i);
				}
			}
			return src;
		}

		private static string doEscapeString(StringBuilder sb, string src, int cur)
		{
			var start = cur;
			for (var i = cur; i < src.Length; i++)
			{
				var escaped = getEscapedChar(src[i]);
				if (escaped != null)
				{
					sb.Append(src, start, i - start);
					sb.Append(escaped);
					start = i + 1;
				}
			}
			sb.Append(src, start, src.Length - start);
			return sb.ToString();
		}

		private static string getEscapedChar(char c)
		{
			switch (c)
			{
				case '\"': return "\\\"";
				case '\t': return "\\t";
				case '\n': return "\\n";
				case '\r': return "\\r";
				case '\f': return "\\f";
				case '\b': return "\\b";
				case '\\': return "\\\\";
				default: return null;
			}
		}
	}
}
