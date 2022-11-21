using System.IO;

namespace PanoramicData.Vtl;

public class VtlParserOptions
{
	/// <summary>
	/// If this is not set, \n or \r\n mode will be detected
	/// based on there being ANY \r characters in the input text
	/// </summary>
	public string? NewLineOverride { get; set; }

	/// <summary>
	/// The directoryInfo of the directory containing the reference files
	/// </summary>
	public DirectoryInfo? ReferenceDirectory { get; set; }

	/// <summary>
	/// If this is not set, $ will be used as the default variable prefix character
	/// </summary>
	public char? VariablePrefixCharacter { get; set; }
}