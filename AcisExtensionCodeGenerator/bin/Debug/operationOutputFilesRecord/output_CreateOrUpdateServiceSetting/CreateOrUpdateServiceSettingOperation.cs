namespace Microsoft.Cloud.Engineering.RdfeExtension
{
	using System.Collections.Generic;
	using Microsoft.WindowsAzure.Wapd.Acis.Contracts;

	public class CreateOrUpdateServiceSettingOperation : RDFESuperOperation
	{
		public override string OperationName
		{
			get { return "Create or Update Service Setting Extension"; }
		}

		public override IAcisSMEOperationGroup OperationGroup
			get { return new OSManagementOperationGroup(); }
		}

		public IAcisSMEOperationResponse CreateOrUpdateServiceSetting(string name, string value, string approver, string approverLink, IAcisSMEEndpoint endpoint, IAcisSMEOperationProgressUpdater updater, OperationExecutionContext context)
		{
			SmeRdfeHelper helper = new SmeRdfeHelper(endpoint, updater, this, context);
			return helper.CreateOrUpdateServiceSetting(name, value);
		}

		public override IEnumerable<IAcisSMEParameterRef> Parameters
		{
			get
			{
				return new[]
					{
						ParamRefFromParam.Get<ServiceSettingNameParam>(), 
						ParamRefFromParam.Get<ServiceSettingValueParam>(), 
						AcisWellKnownParameters.Get(ParameterName.Approver),
						AcisWellKnownParameters.Get(ParameterName.ApproverLink)
					}
			}
		}
	}
}
