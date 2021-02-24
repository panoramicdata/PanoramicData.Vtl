namespace PanoramicData.Vtl.Test
{
	public abstract class BaseTest
	{
		protected BaseTest(VtlParserOptions? vtlParserOptions = null)
		{
			VtlParser = new VtlParser(vtlParserOptions);
		}

		public VtlParser VtlParser { get; }
	}
}