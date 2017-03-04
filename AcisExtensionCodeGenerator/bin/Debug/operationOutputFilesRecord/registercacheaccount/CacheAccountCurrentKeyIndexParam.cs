namespace Microsoft.Cloud.Engineering.RdfeExtension
{
	using WindowsAzure.Wapd.Acis.Contracts;

	public class CacheAccountCurrentKeyIndexParam : AcisSMETextParameter
	{
		public override string Key
		{
			get { return "smecacheaccountcurrentkeyindex"; }
		}

		public override string Name
		{
			get { return "Current Key Index"; }
		}
	}
}
