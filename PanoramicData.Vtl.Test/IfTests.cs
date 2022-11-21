using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace PanoramicData.Vtl.Test;

public class IfTests : BaseTest
{
	[Fact]
	public void IfEnd_Succeeds()
	{
		const string text = "#if ( $foo < 10 )\nFOO\n#end\n";
		var variables = new Dictionary<string, object>
		{
			["foo"] = 1,
		};
		var isSuccess = VtlParser.TryParse(text, variables, out var parsedText);

		isSuccess.Should().BeTrue();
		parsedText.Should().Be("FOO\n");
	}

	[Fact]
	public void IfElseEnd_IfFalse_Succeeds()
	{
		const string text = "#if ( $foo > 10 )\nFOO\n#else\nBAR\n#end\n";
		var variables = new Dictionary<string, object>
		{
			["foo"] = 1,
		};
		var isSuccess = VtlParser.TryParse(text, variables, out var parsedText);

		isSuccess.Should().BeTrue();
		parsedText.Should().Be("BAR\n");
	}

	[Fact]
	public void IfElseEnd_IfTrue_Succeeds()
	{
		const string text = "#if ( $foo < 10 )\nFOO\n#else\nBAR\n#end\n";
		var variables = new Dictionary<string, object>
		{
			["foo"] = 1,
		};
		var isSuccess = VtlParser.TryParse(text, variables, out var parsedText);

		isSuccess.Should().BeTrue();
		parsedText.Should().Be("FOO\n");
	}

	[Fact]
	public void IfElseIfElseIfElseEnd_FirstElseIfTrue_Succeeds()
	{
		const string text = "#if ( $foo > 10 )\nFOO\n#elseif ( $foo < 10 )\nBAR\n#elseif ( $foo < 10 )\nREE\n#else\nZAB\n#end\n";
		var variables = new Dictionary<string, object>
		{
			["foo"] = 1,
		};
		var isSuccess = VtlParser.TryParse(text, variables, out var parsedText);

		isSuccess.Should().BeTrue();
		parsedText.Should().Be("BAR\n");
	}

	[Fact]
	public void IfElseIfElseIfElseEnd_SecondElseIfTrue_Succeeds()
	{
		const string text = "#if ( $foo > 10 )\nFOO\n#elseif ( false )\nBAR\n#elseif ( $foo < 10 )\nREE\n#else\nZAB\n#end\n";
		var variables = new Dictionary<string, object>
		{
			["foo"] = 1,
		};
		var isSuccess = VtlParser.TryParse(text, variables, out var parsedText);

		isSuccess.Should().BeTrue();
		parsedText.Should().Be("REE\n");
	}

	[Fact]
	public void IfElseIfElseIfElseEnd_ElseTrue_Succeeds()
	{
		const string text = "#if ( $foo > 10 )\nFOO\n#elseif ( false )\nBAR\n#elseif ( false )\nREE\n#else\nZAB\n#end\n";
		var variables = new Dictionary<string, object>
		{
			["foo"] = 1,
		};
		var isSuccess = VtlParser.TryParse(text, variables, out var parsedText);

		isSuccess.Should().BeTrue();
		parsedText.Should().Be("ZAB\n");
	}

	[Fact]
	public void NestedIf_Succeeds()
	{
		const string text = "#if ( false )\nFOO\n#if ( true )\nBAR\n#if ( true )\nZAM\n#end\nWEE\n#end\n";
		var variables = new Dictionary<string, object>
		{
			["foo"] = 1,
		};
		var isSuccess = VtlParser.TryParse(text, variables, out var parsedText);

		isSuccess.Should().BeTrue();
		parsedText.Should().Be("BAR\nZAM\nWEE\n");
	}
}
