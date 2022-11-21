using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace PanoramicData.Vtl.Test;

public class VariableTests : BaseTest
{
	[Fact]
	public void SimpleParseSucceeds()
	{
		const string text = "Test File for $purpose\r\nwith description ${description}\r\n";
		var variables = new Dictionary<string, object>
		{
			["purpose"] = "testing",
			["description"] = "'the description'",
		};
		var vtlParser = new VtlParser();
		var isSuccess = vtlParser.TryParse(text, variables, out var parsedText);

		isSuccess.Should().BeTrue();

		parsedText.Should().Be("Test File for testing\r\nwith description 'the description'\r\n");
	}

	[Fact]
	public void SimpleParseTwoSucceeds()
	{
		const string text = "!\r\nhostname ${hostname}";
		var variables = new Dictionary<string, object>
		{
			["hostname"] = "TESTHOSTNAME",
		};
		var vtlParser = new VtlParser();
		var isSuccess = vtlParser.TryParse(text, variables, out var parsedText);

		isSuccess.Should().BeTrue();

		parsedText.Should().Be("!\r\nhostname TESTHOSTNAME");
	}

	[Fact]
	public void VariablePrefixParseSucceeds()
	{
		const string text = "Test File for +purpose";
		var variables = new Dictionary<string, object>
		{
			["purpose"] = "testing",
		};
		var vtlParser = new VtlParser(new VtlParserOptions { VariablePrefixCharacter = '+' });
		var isSuccess = vtlParser.TryParse(text, variables, out var parsedText);

		isSuccess.Should().BeTrue();

		parsedText.Should().Be("Test File for testing");
	}
}
