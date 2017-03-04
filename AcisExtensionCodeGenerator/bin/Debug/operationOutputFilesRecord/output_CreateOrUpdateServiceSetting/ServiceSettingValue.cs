namespace Microsoft.Cloud.Engineering.RdfeExtension
{
	using WindowsAzure.Wapd.Acis.Contracts;

	public classServiceSettingValueParam : AcisSMETextParameter
	{
		///<summary>
		/// TODO
		///</summary>
		public override string Key
		{
			get { return "smeservicesettingvalue"; }
		}

		///<summary>
		/// TODO
		///</summary>
		public override string Name
		{
			get { return "Service Setting Value"; }
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
