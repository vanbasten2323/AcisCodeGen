namespace Microsoft.Cloud.Engineering.RdfeExtension
{
	using System.Collections.Generic;
	using Microsoft.WindowsAzure.Wapd.Acis.Contracts;
	using Microsoft.Cloud.Engineering.RdfeExtension.OperationGroups;
	using Microsoft.Cloud.Engineering.RdfeExtension.Parameters;

	public class ListServiceSettingsOperation : RDFEROOperation
	{
		public override string OperationName
		{
			get { return "List Service Settings"; }
		}

		public override IAcisSMEOperationGroup OperationGroup
		{
			get { return new OSManagementOperationGroup(); }
		}

		public IAcisSMEOperationResponse ListServiceSettings( , IAcisSMEEndpoint endpoint, IAcisSMEOperationProgressUpdater updater, OperationExecutionContext context)
		{
			SmeRdfeHelper helper = new SmeRdfeHelper(endpoint, updater, this, context);
			return helper.ListServiceSettings();
		}

		public override IEnumerable<IAcisSMEParameterRef> Parameters
		{
			get
			{
				return new[]
					{
						ParamRefFromParam.Get<Param>()
					};
			}
		}
	}
}
