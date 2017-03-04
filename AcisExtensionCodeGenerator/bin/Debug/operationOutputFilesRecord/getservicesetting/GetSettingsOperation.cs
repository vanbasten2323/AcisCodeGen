namespace Microsoft.Cloud.Engineering.RdfeExtension.Operations.OS
{
    using System.Collections.Generic;
    using Microsoft.WindowsAzure.Wapd.Acis.Contracts;

    public class GetSettingsOperation : RDFEROOperation
    {
        public override IAcisSMEOperationGroup OperationGroup
        {
            get { return new OSManagementOperationGroup(); }
        }

        public override string OperationName
        {
            get { return "Get Settings (TSQ)"; }
        }

        public override IEnumerable<IAcisSMEParameterRef> Parameters
        {
            get { return new IAcisSMEParameterRef[0]; }
        }

        public IAcisSMEOperationResponse GetSettings(IAcisSMEEndpoint endpoint, IAcisSMEOperationProgressUpdater updater, OperationExecutionContext context)
        {
            SmeRdfeHelper helper = new SmeRdfeHelper(endpoint, updater, this, context);
            return helper.GetSettings();
        }
    }
}
