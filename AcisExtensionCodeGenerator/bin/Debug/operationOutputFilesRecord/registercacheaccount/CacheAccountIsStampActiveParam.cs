namespace Microsoft.Cloud.Engineering.RdfeExtension
{
	using WindowsAzure.Wapd.Acis.Contracts;

	public class CacheAccountIsStampActiveParam : AcisSMEBooleanParameter
    {
		public override string Key
		{
			get { return "smecacheaccountisstampactive"; }
		}

		public override string Name
		{
			get { return "Cache Account Is Active"; }
		}
	}
}
