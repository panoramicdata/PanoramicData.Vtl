# PanoramicData.Vtl
A nuget package for processing VTL. 

Example use:
```C#
const string text =
@"Test File for $purpose
	with description ${description}
";
var variables = new Dictionary<string, object>
{
	["$purpose"] = "testing",
	["$description"] = "'the description'",
};
var vtlParser = new VtlParser();
var isSuccess = vtlParser.TryParse(text, variables, out var parsedText);
```