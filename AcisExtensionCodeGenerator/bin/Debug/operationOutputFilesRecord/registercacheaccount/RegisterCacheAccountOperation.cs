namespace Microsoft.Cloud.Engineering.RdfeExtension
{
	using System.Collections.Generic;
	using Microsoft.WindowsAzure.Wapd.Acis.Contracts;
	using Microsoft.Cloud.Engineering.RdfeExtension.OperationGroups;
	using Microsoft.Cloud.Engineering.RdfeExtension.Parameters;

	public class RegisterCacheAccountOperation : RDFERWOperation
	{
		public override string OperationName
		{
			get { return "Register Cache Account"; }
		}

		public override IAcisSMEOperationGroup OperationGroup
		{
			get { return new PlatformImageRepositoryManagementOperationGroup(); }
		}

		public IAcisSMEOperationResponse RegisterCacheAccount(string cacheAccountName, string cacheAccountCurrentKeyIndex, string location, string cacheAccountStampName, string cacheAccountIsStampActive, string cacheAccountTypeOfCache, string approver, string approverLink, IAcisSMEEndpoint endpoint, IAcisSMEOperationProgressUpdater updater, OperationExecutionContext context)
		{
			SmeRdfeHelper helper = new SmeRdfeHelper(endpoint, updater, this, context);
			return helper.RegisterCacheAccount(cacheAccountName, cacheAccountCurrentKeyIndex, location, cacheAccountStampName, cacheAccountIsStampActive, cacheAccountTypeOfCache);
		}

		public override IEnumerable<IAcisSMEParameterRef> Parameters
		{
			get
			{
				return new[]
					{
						ParamRefFromParam.Get<CacheAccountNameParam>(), 
						ParamRefFromParam.Get<CacheAccountCurrentKeyIndexParam>(), 
						ParamRefFromParam.Get<LocationParam>(), 
						ParamRefFromParam.Get<CacheAccountStampNameParam>(), 
						ParamRefFromParam.Get<CacheAccountIsStampActiveParam>(), 
						ParamRefFromParam.Get<CacheAccountTypeOfCacheParam>(), 
						AcisWellKnownParameters.Get(ParameterName.Approver),
						AcisWellKnownParameters.Get(ParameterName.ApproverLink)
					};
			}
		}
	}
}
