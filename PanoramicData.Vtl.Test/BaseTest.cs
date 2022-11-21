namespace PanoramicData.Vtl.Test;

public abstract class BaseTest
{
	protected BaseTest(VtlParserOptions? vtlParserOptions = null)
	{
		VtlParser = new VtlParser(vtlParserOptions ?? new());
	}

	public VtlParser VtlParser { get; }
}