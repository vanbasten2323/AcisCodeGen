namespace Microsoft.Cloud.Engineering.RdfeExtension
{
	using WindowsAzure.Wapd.Acis.Contracts;

	public class CacheAccountTypeOfCacheParam : AcisSMETextParameter
	{
		public override string Key
		{
			get { return "smecacheaccounttypeofcache"; }
		}

		public override string Name
		{
			get { return "Type of Cache"; }
		}

        public override string HelpText
        {
            get { return "Current legal type is: Image."; }
        }
    }
}
