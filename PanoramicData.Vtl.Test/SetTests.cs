using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace PanoramicData.Vtl.Test
{
	public class SetTests : BaseTest
	{
		[Fact]
		public void Set_Succeeds()
		{
			const string text = "#set ($message = \"Hello World\")\n";
			var variables = new Dictionary<string, object>();
			var isSuccess = VtlParser.TryParse(text, variables, out var parsedText);

			isSuccess.Should().BeTrue();
			parsedText.Should().Be(string.Empty);
			variables.ContainsKey("message").Should().BeTrue();
			variables["message"].Should().Be("Hello World");
		}
	}
}
