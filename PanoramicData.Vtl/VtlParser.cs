using PanoramicData.NCalcExtensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PanoramicData.Vtl;

/// <summary>
/// Parses and evaluates Velocity Template Language (VTL) templates.
/// </summary>
public class VtlParser
{
	private readonly VtlParserOptions _vtlParserOptions;
	private readonly Stack<ParseMode> _conditionStack = new();
	private const char VARIABLE_PREFIX_CHARACTER = '$';

	/// <summary>
	/// The parse modes in which template content is written to the output.
	/// </summary>
	private static readonly ParseMode[] EmittingModes =
	[
		ParseMode.Root,
		ParseMode.Normal,
		ParseMode.ForEach,
		ParseMode.IfActive,
		ParseMode.ElseIfActive,
		ParseMode.ElseActive,
	];

	/// <summary>
	/// Initializes a new instance of the <see cref="VtlParser"/> class with default options.
	/// </summary>
	public VtlParser() : this(new())
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="VtlParser"/> class with the specified options.
	/// </summary>
	/// <param name="vtlParserOptions">The parser options.</param>
	public VtlParser(VtlParserOptions vtlParserOptions)
	{
		_vtlParserOptions = vtlParserOptions;
	}

	/// <summary>
	/// Attempts to parse and evaluate a VTL template from a file.
	/// </summary>
	/// <param name="fileInfo">The file containing the VTL template.</param>
	/// <param name="variables">The variables to use during evaluation.</param>
	/// <param name="result">The evaluated output, or an empty string if parsing fails.</param>
	/// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
	public bool TryParse(FileInfo fileInfo, Dictionary<string, object> variables, out string result)
		=> TryParse(File.ReadAllText(fileInfo.FullName), variables, out result);

	/// <summary>
	/// Attempts to parse and evaluate a VTL template from a string.
	/// </summary>
	/// <param name="text">The VTL template text.</param>
	/// <param name="variables">The variables to use during evaluation.</param>
	/// <param name="result">The evaluated output, or an empty string if parsing fails.</param>
	/// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
	public bool TryParse(string text, Dictionary<string, object> variables, out string result)
	{
		try
		{
			result = Parse(text, variables);
			return true;
		}
		catch (ParseException)
		{
			result = string.Empty;
			return false;
		}
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
	private string Parse(string text, Dictionary<string, object> variables)
	{
		_conditionStack.Push(ParseMode.Root);
		var sb = new StringBuilder();
		var lineNumber = 0;
		foreach (var line in GetLines(text))
		{
			lineNumber++;

			if (TryProcessDirective(line.TrimEnd(), lineNumber, variables))
			{
				continue;
			}

			if (IsEmitting)
			{
				sb.Append(Replace(line, variables));
			}
		}

		return sb.ToString();
	}

	/// <summary>
	/// Processes the line if it is a directive.
	/// </summary>
	/// <returns><c>true</c> if the line was a directive and has been processed; otherwise, <c>false</c>.</returns>
	private bool TryProcessDirective(string trimmedLine, int lineNumber, Dictionary<string, object> variables)
	{
		if (TryGetDirectiveArgument(trimmedLine, "#set", out var setSpec))
		{
			ProcessSet(setSpec, variables);
			return true;
		}

		if (TryGetDirectiveArgument(trimmedLine, "#if", out var ifExpression))
		{
			ProcessIf(ifExpression, variables);
			return true;
		}

		if (TryGetDirectiveArgument(trimmedLine, "#elseif", out var elseIfExpression))
		{
			ProcessElseIf(elseIfExpression, lineNumber, variables);
			return true;
		}

		if (trimmedLine.StartsWith("#else"))
		{
			ProcessElse(lineNumber);
			return true;
		}

		if (trimmedLine.StartsWith("#end"))
		{
			ProcessEnd(lineNumber);
			return true;
		}

		return false;
	}

	/// <summary>
	/// Extracts the parenthesised argument of a directive, e.g. "$foo &lt; 10" from "#if ( $foo &lt; 10 )".
	/// </summary>
	/// <returns><c>true</c> if the line is the specified directive; otherwise, <c>false</c>.</returns>
	private static bool TryGetDirectiveArgument(string trimmedLine, string directive, out string argument)
	{
		var prefix = $"{directive} (";
		if (trimmedLine.StartsWith(prefix) && trimmedLine.EndsWith(")"))
		{
			argument = trimmedLine.Substring(prefix.Length, trimmedLine.Length - prefix.Length - 1);
			return true;
		}

		argument = string.Empty;
		return false;
	}

	private void ProcessIf(string expression, Dictionary<string, object> variables)
		=> _conditionStack.Push(Calculate(expression, variables)
			? ParseMode.IfActive
			: ParseMode.IfInactive);

	private void ProcessElseIf(string expression, int lineNumber, Dictionary<string, object> variables)
	{
		switch (_conditionStack.Pop())
		{
			case ParseMode.IfActive:
			case ParseMode.ElseIfActive:
				_conditionStack.Push(ParseMode.IfHandled);
				break;
			case ParseMode.IfInactive:
			case ParseMode.ElseIfInactive:
				_conditionStack.Push(Calculate(expression, variables)
					? ParseMode.ElseIfActive
					: ParseMode.ElseIfInactive);
				break;
			default:
				throw new ParseException($"Unexpected elseif on line {lineNumber}");
		}
	}

	private void ProcessElse(int lineNumber)
	{
		switch (_conditionStack.Pop())
		{
			case ParseMode.IfActive:
			case ParseMode.ElseIfActive:
			case ParseMode.IfHandled:
				_conditionStack.Push(ParseMode.IfHandled);
				break;
			case ParseMode.IfInactive:
			case ParseMode.ElseIfInactive:
				_conditionStack.Push(ParseMode.ElseActive);
				break;
			default:
				throw new ParseException($"Unexpected else on line {lineNumber}");
		}
	}

	private void ProcessEnd(int lineNumber)
	{
		if (_conditionStack.Count == 0)
		{
			throw new ParseException($"Unexpected end on line {lineNumber}");
		}

		_conditionStack.Pop();
	}

	/// <summary>
	/// Whether the current parse mode writes template content to the output.
	/// </summary>
	private bool IsEmitting => EmittingModes.Contains(_conditionStack.Peek());

	private bool Calculate(string text, Dictionary<string, object> variables)
	{
		var variablePrefixString = _vtlParserOptions.VariablePrefixCharacter ?? VARIABLE_PREFIX_CHARACTER;

		foreach (var kvp in variables)
		{
			text = text
				.Replace($"{variablePrefixString}{kvp.Key}", kvp.Value.ToString())
				.Replace($"{variablePrefixString}{{{kvp.Key}}}", kvp.Value.ToString());
		}

		var expression = new ExtendedExpression(text);
		return expression.Evaluate() as bool?
			?? throw new ParseException($"Function does not evaluate as a boolean: '{text}'");
	}

	private string Replace(string line, Dictionary<string, object> variables)
	{
		var variablePrefixString = _vtlParserOptions.VariablePrefixCharacter ?? VARIABLE_PREFIX_CHARACTER;

		foreach (var variable in variables)
		{
			// Form: $variableName
			line = line.Replace($"{variablePrefixString}{variable.Key}", variable.Value.ToString());
			// Form: ${variableName}
			line = line.Replace($"{variablePrefixString}{{{variable.Key}}}", variable.Value.ToString());
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

		variables[keyValuePair[0].Trim(' ').TrimStart(VARIABLE_PREFIX_CHARACTER)] = keyValuePair[1].Trim(' ').Trim('"');
	}

	/// <summary>
	/// Splits the specified text into lines, preserving newline characters.
	/// </summary>
	/// <param name="text">The text to split.</param>
	/// <returns>An enumerable of lines.</returns>
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
					sb.Append(_vtlParserOptions.NewLineOverride ?? autoDetectedNewlineString);
					yield return sb.ToString();
					sb.Clear();
					break;
				default:
					sb.Append(text[cursor]);
					break;
			}
		}

		yield return sb.ToString();
	}
}
