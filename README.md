# PanoramicData.Vtl

[![Nuget](https://img.shields.io/nuget/v/PanoramicData.Vtl)](https://www.nuget.org/packages/PanoramicData.Vtl/)
[![Nuget](https://img.shields.io/nuget/dt/PanoramicData.Vtl)](https://www.nuget.org/packages/PanoramicData.Vtl/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/0914f8af7a0542ed9a8439c224138ffc)](https://www.codacy.com/gh/panoramicdata/PanoramicData.Vtl/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=panoramicdata/PanoramicData.Vtl&amp;utm_campaign=Badge_Grade)

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