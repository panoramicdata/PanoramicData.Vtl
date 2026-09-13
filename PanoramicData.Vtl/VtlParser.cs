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
	private const char VARIABLE_PREFIX_CHARACTER = '$';

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
		var parseModeStack = new ParseModeStack();
		var sb = new StringBuilder();
		var lineNumber = 0;
		foreach (var line in GetLines(text))
		{
			lineNumber++;

			if (TryProcessDirective(line.TrimEnd(), lineNumber, variables, parseModeStack))
			{
				continue;
			}

			if (parseModeStack.IsEmitting)
			{
				sb.Append(Substitute(line, variables));
			}
		}

		return sb.ToString();
	}

	/// <summary>
	/// Processes the line if it is a directive.
	/// </summary>
	/// <returns><c>true</c> if the line was a directive and has been processed; otherwise, <c>false</c>.</returns>
	private bool TryProcessDirective(
		string trimmedLine,
		int lineNumber,
		Dictionary<string, object> variables,
		ParseModeStack parseModeStack)
	{
		if (TryGetDirectiveArgument(trimmedLine, "#set", out var setSpec))
		{
			ProcessSet(setSpec, variables);
			return true;
		}

		if (TryGetDirectiveArgument(trimmedLine, "#if", out var ifExpression))
		{
			parseModeStack.If(Calculate(ifExpression, variables));
			return true;
		}

		if (TryGetDirectiveArgument(trimmedLine, "#elseif", out var elseIfExpression))
		{
			parseModeStack.ElseIf(() => Calculate(elseIfExpression, variables), lineNumber);
			return true;
		}

		if (trimmedLine.StartsWith("#else"))
		{
			parseModeStack.Else(lineNumber);
			return true;
		}

		if (trimmedLine.StartsWith("#end"))
		{
			parseModeStack.End(lineNumber);
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

	private bool Calculate(string text, Dictionary<string, object> variables)
	{
		var expressionText = Substitute(text, variables);
		var expression = new ExtendedExpression(expressionText);
		return expression.Evaluate() as bool?
			?? throw new ParseException($"Function does not evaluate as a boolean: '{expressionText}'");
	}

	/// <summary>
	/// Replaces each variable reference in the specified text with the variable's value.
	/// </summary>
	/// <param name="text">The text in which to substitute variables.</param>
	/// <param name="variables">The variables to substitute.</param>
	/// <returns>The text, with all known variable references replaced.</returns>
	private string Substitute(string text, Dictionary<string, object> variables)
	{
		var variablePrefixString = _vtlParserOptions.VariablePrefixCharacter ?? VARIABLE_PREFIX_CHARACTER;

		foreach (var variable in variables)
		{
			var value = variable.Value.ToString();
			text = text
				// Form: $variableName
				.Replace($"{variablePrefixString}{variable.Key}", value)
				// Form: ${variableName}
				.Replace($"{variablePrefixString}{{{variable.Key}}}", value);
		}

		return text;
	}

	private static void ProcessSet(string setSpec, Dictionary<string, object> variables)
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
		var newLineString = _vtlParserOptions.NewLineOverride
			?? (text.Any(t => t == '\r') ? "\r\n" : "\n");
		var sb = new StringBuilder();
		foreach (var character in text)
		{
			switch (character)
			{
				case '\r':
					break;
				case '\n':
					sb.Append(newLineString);
					yield return sb.ToString();
					sb.Clear();
					break;
				default:
					sb.Append(character);
					break;
			}
		}

		yield return sb.ToString();
	}
}
