namespace Microsoft.Cloud.Engineering.RdfeExtension
{
	using WindowsAzure.Wapd.Acis.Contracts;

	public classServiceSettingNameParam : AcisSMETextParameter
	{
		///<summary>
		/// TODO
		///</summary>
		public override string Key
		{
			get { return "smeservicesettingname"; }
		}

		///<summary>
		/// TODO
		///</summary>
		public override string Name
		{
			get { return "Service Setting Name"; }
		}

		///<summary>
		/// TODO
		///</summary>
		public override string HelpText
		{
			get { return; } //TODO
		}
	}
}
