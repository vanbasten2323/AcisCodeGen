namespace Microsoft.Cloud.Engineering.RdfeExtension
{
	using System.Collections.Generic;
	using Microsoft.WindowsAzure.Wapd.Acis.Contracts;
    using Microsoft.Cloud.Engineering.RdfeExtension.Parameters;
    using Microsoft.Cloud.Engineering.RdfeExtension.OperationGroups;

	public class UnregisterCacheAccountOperation : RDFERWOperation
	{
		public override string OperationName
		{
			get { return "Unregister Cache Account"; }
		}

		public override IAcisSMEOperationGroup OperationGroup
		{
			get { return new PlatformImageRepositoryManagementOperationGroup(); }
		}

		public IAcisSMEOperationResponse UnregisterCacheAccount(string accountname, string approver, string approverLink, IAcisSMEEndpoint endpoint, IAcisSMEOperationProgressUpdater updater, OperationExecutionContext context)
		{
			SmeRdfeHelper helper = new SmeRdfeHelper(endpoint, updater, this, context);
			return helper.UnregisterCacheAccount(accountname);
		}

		public override IEnumerable<IAcisSMEParameterRef> Parameters
		{
			get
			{
				return new[]
					{
						ParamRefFromParam.Get<CacheAccountNameParam>(), 
						AcisWellKnownParameters.Get(ParameterName.Approver),
						AcisWellKnownParameters.Get(ParameterName.ApproverLink)
					};
			}
		}
	}
}
