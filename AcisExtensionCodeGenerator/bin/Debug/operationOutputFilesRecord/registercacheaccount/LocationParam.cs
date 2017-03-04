namespace Microsoft.Cloud.Engineering.RdfeExtension
{
	using WindowsAzure.Wapd.Acis.Contracts;

	public class LocationParam : AcisSMETextParameter
	{
		public override string Key
		{
			get { return "smelocation"; }
		}

		public override string Name
		{
			get { return "Location"; }
		}
	}
}
