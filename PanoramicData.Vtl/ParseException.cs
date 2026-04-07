using System;

namespace PanoramicData.Vtl;

/// <summary>
/// Exception thrown when a VTL template cannot be parsed.
/// </summary>
[Serializable]
public class ParseException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ParseException"/> class.
	/// </summary>
	public ParseException()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ParseException"/> class with a specified error message.
	/// </summary>
	/// <param name="message">The error message.</param>
	public ParseException(string message) : base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ParseException"/> class with a specified error message and inner exception.
	/// </summary>
	/// <param name="message">The error message.</param>
	/// <param name="innerException">The inner exception.</param>
	public ParseException(string message, Exception innerException) : base(message, innerException)
	{
	}
}