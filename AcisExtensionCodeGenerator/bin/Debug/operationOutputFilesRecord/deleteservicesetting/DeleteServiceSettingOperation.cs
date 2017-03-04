namespace Microsoft.Cloud.Engineering.RdfeExtension
{
	using System.Collections.Generic;
	using Microsoft.WindowsAzure.Wapd.Acis.Contracts;
	using Microsoft.Cloud.Engineering.RdfeExtension.OperationGroups;
	using Microsoft.Cloud.Engineering.RdfeExtension.Parameters;

	public class DeleteServiceSettingOperation : RDFESuperOperation
	{
		public override string OperationName
		{
			get { return "Delete Service Setting"; }
		}

		public override IAcisSMEOperationGroup OperationGroup
		{
			get { return new OSManagementOperationGroup(); }
		}

		public IAcisSMEOperationResponse DeleteServiceSetting(string name, string approver, string approverLink, IAcisSMEEndpoint endpoint, IAcisSMEOperationProgressUpdater updater, OperationExecutionContext context)
		{
			SmeRdfeHelper helper = new SmeRdfeHelper(endpoint, updater, this, context);
			return helper.DeleteServiceSetting(name);
		}

		public override IEnumerable<IAcisSMEParameterRef> Parameters
		{
			get
			{
				return new[]
					{
						ParamRefFromParam.Get<SettingNameParam>(), 
						AcisWellKnownParameters.Get(ParameterName.Approver),
						AcisWellKnownParameters.Get(ParameterName.ApproverLink)
					};
			}
		}
	}
}
