namespace Microsoft.Cloud.Engineering.RdfeExtension.DisplayHelpers
{
    extern alias Thin;
    using System;
    using Thin::Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement;
    using Microsoft.WindowsAzure.Wapd.Acis.Contracts;
    using Microsoft.WindowsAzure.Wapd.Acis.Contracts.SimplificationClasses;

    public sealed class ServiceSettingFormatter : RdfeFormatter
    {
        public ServiceSettingFormatter(IAcisSMEEndpoint endpoint) : base(endpoint)
        {
        }

        public string FormatServiceSettingDisplay(ServiceSetting setting)
        {
            StringAndFormattingUtilities.ResponseFormatter formatter = this.GetFormatter();
            this.FormatServiceSetting(setting, formatter);

            return formatter.ToString();
        }

        public string FormatServiceSettingsDisplay(ServiceSettingsList settings)
        {
            StringAndFormattingUtilities.ResponseFormatter formatter = this.GetFormatter();
            formatter.FormatList(settings, (f, s) => this.FormatServiceSetting(s, f));

            return formatter.ToString();
        }

        private void FormatServiceSetting(ServiceSetting setting,
            StringAndFormattingUtilities.ResponseFormatter formatter)
        {
            if (formatter == null || setting == null)
            {
                return;
            }

            formatter.AppendProperties("Name", setting.Name);
            formatter.AppendProperties("Kind", setting.Kind);
            formatter.AppendProperties("Value", setting.Value);
        }
    }
}
