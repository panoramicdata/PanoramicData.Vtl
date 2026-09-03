using AwesomeAssertions;
using System.Collections.Generic;
using Xunit;

namespace PanoramicData.Vtl.Test;

public class IfTests : BaseTest
{
	[Fact]
	public void IfEnd_Succeeds()
		=> AssertParsesTo("#if ( $foo < 10 )\nFOO\n#end\n", "FOO\n");

	[Fact]
	public void IfElseEnd_IfFalse_Succeeds()
		=> AssertParsesTo("#if ( $foo > 10 )\nFOO\n#else\nBAR\n#end\n", "BAR\n");

	[Fact]
	public void IfElseEnd_IfTrue_Succeeds()
		=> AssertParsesTo("#if ( $foo < 10 )\nFOO\n#else\nBAR\n#end\n", "FOO\n");

	[Fact]
	public void IfElseIfElseIfElseEnd_FirstElseIfTrue_Succeeds()
		=> AssertParsesTo("#if ( $foo > 10 )\nFOO\n#elseif ( $foo < 10 )\nBAR\n#elseif ( $foo < 10 )\nREE\n#else\nZAB\n#end\n", "BAR\n");

	[Fact]
	public void IfElseIfElseIfElseEnd_SecondElseIfTrue_Succeeds()
		=> AssertParsesTo("#if ( $foo > 10 )\nFOO\n#elseif ( false )\nBAR\n#elseif ( $foo < 10 )\nREE\n#else\nZAB\n#end\n", "REE\n");

	[Fact]
	public void IfElseIfElseIfElseEnd_ElseTrue_Succeeds()
		=> AssertParsesTo("#if ( $foo > 10 )\nFOO\n#elseif ( false )\nBAR\n#elseif ( false )\nREE\n#else\nZAB\n#end\n", "ZAB\n");

	[Fact]
	public void NestedIf_Succeeds()
		=> AssertParsesTo("#if ( false )\nFOO\n#if ( true )\nBAR\n#if ( true )\nZAM\n#end\nWEE\n#end\n", "BAR\nZAM\nWEE\n");

	/// <summary>
	/// Parses the template with $foo set to 1, and asserts that it succeeds and produces the expected output.
	/// </summary>
	private void AssertParsesTo(string text, string expectedText)
	{
		var variables = new Dictionary<string, object>
		{
			["foo"] = 1,
		};

		var isSuccess = VtlParser.TryParse(text, variables, out var parsedText);

		isSuccess.Should().BeTrue();
		parsedText.Should().Be(expectedText);
	}
}
