# PanoramicData.Vtl

[![Nuget](https://img.shields.io/nuget/v/PanoramicData.Vtl)](https://www.nuget.org/packages/PanoramicData.Vtl/)
[![Nuget](https://img.shields.io/nuget/dt/PanoramicData.Vtl)](https://www.nuget.org/packages/PanoramicData.Vtl/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/0914f8af7a0542ed9a8439c224138ffc)](https://www.codacy.com/gh/panoramicdata/PanoramicData.Vtl/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=panoramicdata/PanoramicData.Vtl&amp;utm_campaign=Badge_Grade)

> **Need help with APIs or .NET?** Panoramic Data are experts in APIs and .NET, and we can help with your development. [Contact us](https://www.panoramicdata.com/contact-us) or see our [software services](https://www.panoramicdata.com/services/software).

A nuget package for processing VTL. 

Supports .NET 11.0.

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
