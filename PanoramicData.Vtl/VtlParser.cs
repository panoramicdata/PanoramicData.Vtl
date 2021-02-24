using PanoramicData.NCalcExtensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PanoramicData.Vtl
{
	public class VtlParser
	{
		private readonly VtlParserOptions vtlParserOptions;
		private readonly Stack<ParseMode> conditionStack = new();

		public VtlParser(VtlParserOptions? vtlParserOptions = null)
		{
			this.vtlParserOptions = vtlParserOptions ?? new();
		}

		public bool TryParse(FileInfo fileInfo, Dictionary<string, object> variables, out string result)
			=> TryParse(File.ReadAllText(fileInfo.FullName), variables, out result);

		public bool TryParse(string text, Dictionary<string, object> variables, out string result)
		{
			try
			{
				conditionStack.Push(ParseMode.Root);
				var sb = new StringBuilder();
				var lineNumber = 0;
				foreach (var line in GetLines(text))
				{
					lineNumber++;

					// Set support
					var trimmedLine = line.TrimEnd();
					if (trimmedLine.StartsWith("#set (") && trimmedLine.EndsWith(")"))
					{
						ProcessSet(line.Substring("#set (".Length, trimmedLine.Length - "#set ()".Length), variables);
						continue;
					}

					// Conditional support (if/elseif/else/end)
					if (trimmedLine.StartsWith("#if (") && trimmedLine.EndsWith(")"))
					{
						var ifExpression = trimmedLine.Substring("#if (".Length, trimmedLine.Length - "#if ()".Length);
						conditionStack.Push(Calculate(ifExpression, variables)
							? ParseMode.IfActive
							: ParseMode.IfInactive);
						continue;
					}
					if (trimmedLine.StartsWith("#elseif (") && trimmedLine.EndsWith(")"))
					{
						switch (conditionStack.Pop())
						{
							case ParseMode.IfActive:
							case ParseMode.ElseIfActive:
								conditionStack.Push(ParseMode.IfHandled);
								break;
							case ParseMode.IfInactive:
							case ParseMode.ElseIfInactive:
								conditionStack.Push(Calculate(trimmedLine.Substring("#elseif (".Length, trimmedLine.Length - "#elseif ()".Length), variables)
									? ParseMode.ElseIfActive
									: ParseMode.ElseIfInactive);
								break;
							default:
								throw new ParseException($"Unexpected elseif on line {lineNumber}");
						}
						continue;
					}
					if (trimmedLine.StartsWith("#else"))
					{
						var parseMode = conditionStack.Pop();
						switch (parseMode)
						{
							case ParseMode.IfActive:
							case ParseMode.ElseIfActive:
							case ParseMode.IfHandled:
								conditionStack.Push(ParseMode.IfHandled);
								break;
							case ParseMode.IfInactive:
							case ParseMode.ElseIfInactive:
								conditionStack.Push(ParseMode.ElseActive);
								break;
							default:
								throw new ParseException($"Unexpected elseif on line {lineNumber}");
						}
						continue;
					}
					if (trimmedLine.StartsWith("#end"))
					{
						if (conditionStack.Count == 0)
						{
							throw new ParseException($"Unexpected end on line {lineNumber}");
						}
						conditionStack.Pop();
						continue;
					}

					// TODO - loops support (foreach/end)
					// TODO - include support
					// TODO - parse support
					// TODO - evaluate support
					// TODO - break support
					// TODO - stop support
					// TODO - velocimacros support
					// TODO - math support
					// TODO - range operator support

					switch (conditionStack.Peek())
					{
						case ParseMode.Root:
						case ParseMode.Normal:
						case ParseMode.ForEach:
						case ParseMode.IfActive:
						case ParseMode.ElseIfActive:
						case ParseMode.ElseActive:
							sb.Append(Replace(line, variables));
							break;
					}
				}
				result = sb.ToString();
				return true;
			}
			catch (ParseException)
			{
				result = string.Empty;
				return false;
			}
		}

		private bool Calculate(string text, Dictionary<string, object> variables)
		{
			foreach (var kvp in variables)
			{
				text = text
					.Replace($"${kvp.Key}", kvp.Value.ToString())
					.Replace($"${{{kvp.Key}}}", kvp.Value.ToString());
			}
			var expression = new ExtendedExpression(text);
			return expression.Evaluate() as bool?
				?? throw new ParseException($"Function does not evaluate as a boolean: '{text}'");
		}

		private string Replace(string line, Dictionary<string, object> variables)
		{
			foreach (var variable in variables)
			{
				// Form: $variableName
				line = line.Replace($"${variable.Key}", variable.Value.ToString());
				// Form: ${variableName}
				line = line.Replace($"${{{variable.Key}}}", variable.Value.ToString());
			}
			return line;
		}

		private void ProcessSet(string setSpec, Dictionary<string, object> variables)
		{
			var keyValuePair = setSpec.Split('=');
			if (keyValuePair.Length != 2)
			{
				throw new ParseException("");
			}
			variables[keyValuePair[0].Trim(' ').TrimStart('$')] = keyValuePair[1].Trim(' ').Trim('"');
		}

		public IEnumerable<string> GetLines(string text)
		{
			var autoDetectedNewlineString = text.Any(t => t == '\r')
				? "\r\n"
				: "\n";
			var sb = new StringBuilder();
			for (var cursor = 0; cursor < text.Length; cursor++)
			{
				switch (text[cursor])
				{
					case '\r':
						break;
					case '\n':
						sb.Append(vtlParserOptions.NewLineOverride ?? autoDetectedNewlineString);
						yield return sb.ToString();
						sb.Clear();
						break;
					default:
						sb.Append(text[cursor]);
						break;
				}
			}
		}
	}
}
