using System;
using System.Collections.Generic;
using System.Linq;

namespace PanoramicData.Vtl;

/// <summary>
/// Tracks the nesting of conditional directives encountered while parsing a template,
/// and determines whether the current position emits content to the output.
/// </summary>
internal sealed class ParseModeStack
{
	private readonly Stack<ParseMode> _stack = new();

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
	/// Initializes a new instance of the <see cref="ParseModeStack"/> class at the root of a template.
	/// </summary>
	internal ParseModeStack()
	{
		_stack.Push(ParseMode.Root);
	}

	/// <summary>
	/// Whether the current parse mode writes template content to the output.
	/// </summary>
	internal bool IsEmitting => EmittingModes.Contains(_stack.Peek());

	/// <summary>
	/// Begins a conditional block.
	/// </summary>
	/// <param name="isActive">Whether the condition evaluated to <c>true</c>.</param>
	internal void If(bool isActive)
		=> _stack.Push(isActive
			? ParseMode.IfActive
			: ParseMode.IfInactive);

	/// <summary>
	/// Moves to an alternative branch of the current conditional block.
	/// </summary>
	/// <param name="evaluateCondition">
	/// Evaluates the branch's condition.  This is called only when no earlier branch has been taken,
	/// so that conditions in skipped branches are never evaluated.
	/// </param>
	/// <param name="lineNumber">The line number of the directive, used in error messages.</param>
	internal void ElseIf(Func<bool> evaluateCondition, int lineNumber)
		=> _stack.Push(PopBranch("elseif", lineNumber)
			? ParseMode.IfHandled
			: evaluateCondition()
				? ParseMode.ElseIfActive
				: ParseMode.ElseIfInactive);

	/// <summary>
	/// Moves to the final branch of the current conditional block.
	/// </summary>
	/// <param name="lineNumber">The line number of the directive, used in error messages.</param>
	internal void Else(int lineNumber)
		=> _stack.Push(PopBranch("else", lineNumber)
			? ParseMode.IfHandled
			: ParseMode.ElseActive);

	/// <summary>
	/// Ends the current conditional block.
	/// </summary>
	/// <param name="lineNumber">The line number of the directive, used in error messages.</param>
	internal void End(int lineNumber)
	{
		if (_stack.Count <= 1)
		{
			throw new ParseException($"Unexpected end on line {lineNumber}");
		}

		_stack.Pop();
	}

	/// <summary>
	/// Pops the current conditional branch.
	/// </summary>
	/// <param name="directive">The directive being processed, used in error messages.</param>
	/// <param name="lineNumber">The line number of the directive, used in error messages.</param>
	/// <returns><c>true</c> if an earlier branch of this block has already been taken; otherwise, <c>false</c>.</returns>
	private bool PopBranch(string directive, int lineNumber)
	{
		if (_stack.Count <= 1)
		{
			throw new ParseException($"Unexpected {directive} on line {lineNumber}");
		}

		return _stack.Pop() switch
		{
			ParseMode.IfActive or ParseMode.ElseIfActive or ParseMode.IfHandled => true,
			ParseMode.IfInactive or ParseMode.ElseIfInactive => false,
			_ => throw new ParseException($"Unexpected {directive} on line {lineNumber}"),
		};
	}
}
