namespace PanoramicData.Vtl.Test;

public abstract class BaseTest
{
	protected BaseTest()
	{
		VtlParser = new VtlParser(new());
	}

	public VtlParser VtlParser { get; }
}