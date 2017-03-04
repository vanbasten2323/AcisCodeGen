namespace Microsoft.Cloud.Engineering.RdfeExtension
{
	using System.Collections.Generic;
	using Microsoft.WindowsAzure.Wapd.Acis.Contracts;
	using Microsoft.Cloud.Engineering.RdfeExtension.OperationGroups;
	using Microsoft.Cloud.Engineering.RdfeExtension.Parameters;

	public class RotateCacheAccountKeyOperation : RDFERWOperation
	{
		public override string OperationName
		{
			get { return "Rotate Cache Account Key"; }
		}

		public override IAcisSMEOperationGroup OperationGroup
		{
			get { return new PlatformImageRepositoryManagementOperationGroup(); }
		}

		public IAcisSMEOperationResponse RotateCacheAccountKey(string cacheAccountName, string approver, string approverLink, IAcisSMEEndpoint endpoint, IAcisSMEOperationProgressUpdater updater, OperationExecutionContext context)
		{
			SmeRdfeHelper helper = new SmeRdfeHelper(endpoint, updater, this, context);
			return helper.RotateCacheAccountKey(cacheAccountName);
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
