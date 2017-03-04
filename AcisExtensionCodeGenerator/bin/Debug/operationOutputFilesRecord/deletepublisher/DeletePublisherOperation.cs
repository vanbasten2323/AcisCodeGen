namespace Microsoft.Cloud.Engineering.RdfeExtension
{
	using System.Collections.Generic;
	using Microsoft.WindowsAzure.Wapd.Acis.Contracts;
    using Microsoft.Cloud.Engineering.RdfeExtension.OperationGroups;
    using Microsoft.Cloud.Engineering.RdfeExtension.Parameters;

    public class DeletePublisherOperation : RDFERWOperation
	{
		public override string OperationName
		{
			get { return "Delete Publisher"; }
		}

		public override IAcisSMEOperationGroup OperationGroup
		{
			get { return new PlatformImageRepositoryManagementOperationGroup(); }
		}

		public IAcisSMEOperationResponse DeletePublisher(string Subscription, string PublisherCode, string approver, string approverLink, IAcisSMEEndpoint endpoint, IAcisSMEOperationProgressUpdater updater, OperationExecutionContext context)
		{
			SmeRdfeHelper helper = new SmeRdfeHelper(endpoint, updater, this, context);
			return helper.DeletePublisher(Subscription, PublisherCode);
		}

		public override IEnumerable<IAcisSMEParameterRef> Parameters
		{
			get
			{
				return new[]
					{
                        AcisWellKnownParameters.Get(ParameterName.SubscriptionId),
                        ParamRefFromParam.Get<PublisherCodeParam>(), 
						AcisWellKnownParameters.Get(ParameterName.Approver),
						AcisWellKnownParameters.Get(ParameterName.ApproverLink)
					};
			}
		}
	}
}
