extern alias Thin;
using Thin::Microsoft.Azure.ProvisioningAgent;
using Thin::Microsoft.Cis.DevExp.Services.Rdfe.Extensibility;
using Thin::Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement;
using Thin::Microsoft.ServiceModel.Web;
using CacheAccount = Thin::Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement.CacheAccount;
using Deployment = Thin::Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement.Deployment;
using FabricGeoId = Thin::Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement.FabricGeoId;
using GeoRedirectLocation = Thin::Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement.GeoRedirectLocation;
using GeoRedirectLocationsList = Thin::Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement.GeoRedirectLocationsList;
using GeoRegion = Thin::Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement.GeoRegion;
using GeoTenant = Thin::Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement.GeoTenant;
using HostedService = Thin::Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement.HostedService;
using IPRange = Thin::Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement.IPRange;
using IPRangesList = Thin::Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement.IPRangesList;
using NetworkGeoId = Thin::Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement.NetworkGeoId;
using OnBehalfSubscriptionRegistrationsType = Thin::Microsoft.Cis.DevExp.Services.Rdfe.Extensibility.OnBehalfSubscriptionRegistrationsType;
using OnBehalfSubscriptionRegistrationType = Thin::Microsoft.Cis.DevExp.Services.Rdfe.Extensibility.OnBehalfSubscriptionRegistrationType;
using OperationState = Thin::Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement.OperationState;
using RegisteredUsageKindsType = Thin::Microsoft.Cis.DevExp.Services.Rdfe.Extensibility.RegisteredUsageKindsType;
using ServiceNames = Thin::Microsoft.Cis.DevExp.Services.Rdfe.Extensibility.ServiceNames;
using Subscription = Thin::Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement.Subscription;

namespace Microsoft.Cloud.Engineering.RdfeExtension
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using System.Text;
    using System.Xml;
    using WindowsAzure.Wapd.Acis.Contracts;
    using WindowsAzure.Wapd.Acis.Contracts.SimplificationClasses;
    using Microsoft.Cloud.Engineering.RdfeExtension.DisplayHelpers;
    using Microsoft.Cloud.Engineering.RdfeExtension.Parameters;
    using static WindowsAzure.Wapd.Acis.Contracts.SimplificationClasses.StringAndFormattingUtilities;


    /// <summary>
    /// The SME RDFE Helper class
    /// </summary>
    public partial class SmeRdfeHelper
    {
        private const string AdminEndpoint = "AdminEndpoint";
        private const string PaEndPoint = "PaEndPoint";
        private const string RdfeEndpoint = "RDFEEndpoint";

        /// <summary>
        /// Initializes a new instance of the <see cref="SmeRdfeHelper"/> class.
        /// </summary>
        public SmeRdfeHelper(IAcisSMEEndpoint endpoint, IAcisSMEOperationProgressUpdater updater, IAcisSMEOperation callingOperation, OperationExecutionContext context)
        {
            this.Endpoint = endpoint;
            this.Logger = this.Endpoint.ContainingExtension.Logger;
            this.Updater = updater;
            this.Utility = new AcisSMEUtility(this.Endpoint);
            this.CallingOperation = callingOperation;
            this.Context = context;
            this.Config = new RdfeConfig(this.Endpoint);
        }

        public RdfeConfig Config { get; private set; }

        /// <summary>
        /// Gets the operation execution context
        /// </summary>
        public OperationExecutionContext Context { get; private set; }

        /// <summary>
        /// Gets the calling operation.
        /// </summary>
        public IAcisSMEOperation CallingOperation { get; private set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public IAcisSMELogger Logger { get; private set; }

        /// <summary>
        /// Gets the endpoint.
        /// </summary>
        public IAcisSMEEndpoint Endpoint { get; private set; }

        /// <summary>
        /// Gets the updater.
        /// </summary>
        public IAcisSMEOperationProgressUpdater Updater { get; private set; }

        /// <summary>
        /// Gets the utility.
        /// </summary>
        public AcisSMEUtility Utility { get; private set; }
        
        /// <summary>
        /// Lists the valid rdfe accounts.
        /// </summary>
        public IAcisSMEOperationResponse ListValidRdfeAccounts()
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               IEnumerable<string> accounts = admin.ListValidRdfeAccounts();
                                                               return string.Join("\n", accounts);
                                                           }
                                                       },
                                                       err => string.Format("Failed to list valid RDFE accounts due to {0}.", err));
        }

        private const string SasContainerName = "$logs";

        /// <summary>
        /// Gets the container SAS URI.
        /// </summary>
        public IAcisSMEOperationResponse GetContainerSasUri(string accountIdentifier, int durationInMinutes)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               SASUri uri = admin.GetContainerSasUri(accountIdentifier, SasContainerName, durationInMinutes);
                                                               return string.Format("Your SAS Uri will expire at {0} UTC.  Uri: {1}", uri.ExpiryTimeUTC, uri.SasUri);
                                                           }
                                                       },
                                                       err => string.Format("Failed to get container SAS Uri due to {0}.", err));
        }

        public IAcisSMEOperationResponse GetPublisher(string subscriptionId, string publisherCode)
        {
            return this.ExecuteManagementOperation(
                (admin, context) =>
                {
                    using (context())
                    {
                        PublisherData publisher = admin.GetPublisher(subscriptionId, publisherCode);
                        PublisherFormatter formatter = new PublisherFormatter(this.Endpoint);

                        return formatter.FormatPublisherDisplay(publisher);
                    }
                },
                err => "Unable to get publisher due to " + err);
        }

        public IAcisSMEOperationResponse AddPublisher(string subscriptionId, PublisherInputData publisherInputData)
        {
            return this.ExecuteManagementOperation(
                (admin, context) =>
                {
                    using (context())
                    {
                        string publisher = admin.AddPublisher(subscriptionId, publisherInputData);
                        PublisherFormatter formatter = new PublisherFormatter(this.Endpoint);

                        return formatter.FormatPublisherCode(publisher);
                    }
                },
                err => "Unable to add publisher due to " + err);
        }

        public IAcisSMEOperationResponse UpdatePublisher(string subscriptionId, string publisherCode, PublisherInputData publisherInputData)
        {
            return this.ExecuteManagementOperation(
                (admin, context) =>
                {
                    using (context())
                    {
                        admin.UpdatePublisher(subscriptionId, publisherCode, publisherInputData);
                        PublisherFormatter formatter = new PublisherFormatter(this.Endpoint);

                        return formatter.FormatUpdatedPublisherCode(publisherCode);
                    }
                },
                err => "Unable to update publisher due to " + err);
        }

        
        public IAcisSMEOperationResponse GetCacheAccount(string cacheAccountName)
        {
             return this.ExecuteAdministrationOperation(
                (admin, context) =>
                {
                    using (context())
                    {
                        CacheAccount account = admin.GetCacheAccount(cacheAccountName);
                        CacheAccountFormatter formatter = new CacheAccountFormatter(this.Endpoint);

                        return formatter.FormatCacheAccount(account);
                    }
                },
                err => "Unable to get cache account due to " + err);
        }

        public IAcisSMEOperationResponse ListPublishers(string subscriptionId)
        {
            return this.ExecuteManagementOperation(
                (admin, context) =>
                {
                    using (context())
                    {
                        PublisherDataCollection publisherList = admin.ListPublishers(subscriptionId);
                        PublisherFormatter formatter = new PublisherFormatter(this.Endpoint);

                        return formatter.FormatPublishersDisplay(publisherList);
                    }
                },
                err => "Unable to retrieve publishers list due to " + err);
        }

        public IAcisSMEOperationResponse ListAllPublishers()
        {
            return this.ExecuteManagementOperation(
                (admin, context) =>
                {
                    using (context())
                    {
                        PublisherDataCollection publisherList = admin.ListAllPublishers();
                        PublisherFormatter formatter = new PublisherFormatter(this.Endpoint);

                        return formatter.FormatPublishersDisplay(publisherList);
                    }
                },
                err => "Unable to retrieve all publishers list due to " + err);
        }

        /// <summary>
        /// Revokes the SAS token.
        /// </summary>
        public IAcisSMEOperationResponse RevokeSasToken(string accountName)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               bool result = admin.RevokeSasToken(accountName, SasContainerName);
                                                               return result
                                                                          ? "Successfully revoked SAS token"
                                                                          : "Did not successfully revoke SAS token.  Please contact RDFE for more information if you believe this is an error, as no error was provided back to ACIS beyond the fact that it failed.";
                                                           }
                                                       },
                                                       err => string.Format("Failed to revoke SAS token due to {0}.", err));
        }

        /// <summary>
        /// Register the cache account asynchronously.
        /// </summary>
        /// <param name="stampName">Stamp name of the cache account</param>
        /// <param name="accountName">Account name of the cache account</param>
        /// <param name="location">Location of the cache account</param>
        /// <returns>IAcisSMEOperationResponse</returns>
        public IAcisSMEOperationResponse RegisterCacheAccountAsync(string stampName, string accountName, string location)
        {
            return this.ExecuteAdministrationOperation(
                (admin, context) =>
                {
                    CacheAccountInput input = new CacheAccountInput
                    {
                        StampName = stampName,
                        AccountName = accountName,
                        Location = location
                    };
                    string trackingId;
                    using (context())
                    {
                        InsertUserAgentToOutgoingRequest();
                        admin.RegisterCacheAccountAsync(input);
                        trackingId = GetRdfeTrackingId();
                    }

                    string errorDetail;
                    if (!WaitForRdfeTracking(admin, context, trackingId, out errorDetail))
                    {
                        throw new SmeRdfeOperationStepException(string.Format("Failed to register cache account asynchronously. Error details: {0}", errorDetail ?? "<NoDetailAvailable>"));
                    }

                    return "Successfully Registered Cache Account Asynchronously.";
                }, 
                err => string.Format("Failed to register cache account asynchronously due to {0}.", err));
        }

        /// <summary>
        /// Gets the table SAS URI.
        /// </summary>
        public IAcisSMEOperationResponse GetTableSasUri(string accountIdentifier, string tableName, int durationInMinutes)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               SASUri uri = admin.GetTableSasUri(accountIdentifier, tableName, durationInMinutes);
                                                               return string.Format("Your SAS Uri will expire at {0} UTC.  Uri: {1}", uri.ExpiryTimeUTC, uri.SasUri);
                                                           }
                                                       },
                                                       err => string.Format("Failed to get table SAS Uri due to {0}.", err));
        }

        /// <summary>
        /// Revokes the table SAS token.
        /// </summary>
        public IAcisSMEOperationResponse RevokeTableSasToken(string accountName)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               bool result = admin.RevokeTableSasToken(accountName, SasContainerName);
                                                               return result
                                                                          ? "Successfully revoked SAS token"
                                                                          : "Did not successfully revoke SAS token.  Please contact RDFE for more information if you believe this is an error, as no error was provided back to ACIS beyond the fact that it failed.";
                                                           }
                                                       },
                                                       err => string.Format("Failed to revoke SAS token due to {0}.", err));
        }

        /// <summary>
        /// Configures the xstore analytics.
        /// </summary>
        public IAcisSMEOperationResponse ConfigureXstoreAnalytics(string accountIdentifier, string serviceType, string domainSuffix, LoggingLevel loggingOperationsLevel, int loggingRetentionInDays, 
                                                                  MetricsType metricsLevel, int metricsRetentionInDays)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               AnalyticsServiceSettings settings = new AnalyticsServiceSettings
                                                                                                   {
                                                                                                       DomainSuffix = domainSuffix,
                                                                                                       LoggingOperations = loggingOperationsLevel,
                                                                                                       LoggingRetentionInDays = loggingRetentionInDays,
                                                                                                       MinuteMetricsLevel = metricsLevel,
                                                                                                       MinuteMetricsRetentionInDays = metricsRetentionInDays
                                                                                                   };

                                                               bool result = admin.ConfigureXstoreAnalytics(accountIdentifier, serviceType, settings);

                                                               return result
                                                                          ? "Successfully configured XStore Analytics"
                                                                          : "Did not successfully configure XStore Analytics.  Please contact RDFE for more information if you believe this is an error, as no error was provided back to ACIS beyond the fact that it failed.";
                                                           }
                                                       },
                                                       err => string.Format("Did not successfully configure XStore Analytics due to {0}.", err));
        }

        /// <summary>
        /// Gets the xstore analytics status.
        /// </summary>
        public IAcisSMEOperationResponse GetXStoreAnalyticsStatus(string accountIdentifier, string service)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               AnalyticsServiceSettings settings = admin.GetXstoreAnalyticsStatus(accountIdentifier, service);
                                                               StringAndFormattingUtilities.ResponseFormatter formatter = new StringAndFormattingUtilities.ResponseFormatter(this.Endpoint);

                                                               formatter.FormatObject(settings);
                                                               return formatter.ToString();
                                                           }
                                                       },
                                                       err => string.Format("Could not successfully retrieve XStore Analytics due to {0}.", err));
        }

        /// <summary>
        /// Transfers contents of one subscription to another
        /// </summary>
        public IAcisSMEOperationResponse TransferSubscriptionOperation(string fromSubscription, string toSubscription, bool forceTransfer, bool skipAdminCheck)
        {
            bool checkFailed = false;
            IAcisSMEOperationResponse result = this.ExecuteManagementOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               Subscription sourceSub, destSub;
                                                               if (!skipAdminCheck)
                                                               {
                                                                   sourceSub = admin.GetSubscriptionWithDetails(fromSubscription, "Subscription,Principal");
                                                                   destSub = admin.GetSubscriptionWithDetails(toSubscription, "Subscription,Principal");

                                                                   if (!sourceSub.Principals.Any(s => destSub.Principals.Any(d => d.ID == s.ID)))
                                                                   {
                                                                       checkFailed = true;
                                                                       return string.Format("WARNING! There is no common administrator between the source and destination subscriptions. Proceeding with this transfer operation may risk exposure of customer data and secrets to a third party resulting in a serious security violation. Only continue if you are sure of the subscription IDs you have entered.");
                                                                   }
                                                                  
                                                               }
                                                               checkFailed = false;
                                                               admin.TransferToNewSubscription(fromSubscription, toSubscription, forceTransfer);

                                                               StringAndFormattingUtilities.ResponseFormatter formatter = new StringAndFormattingUtilities.ResponseFormatter(this.Endpoint);
                                                               destSub = admin.GetSubscriptionWithDetails(toSubscription, "Subscription,Principal");
                                                               string subscriptionText = formatter.FormatObject(destSub).ToString();
                                                               return string.Format("Successfully able to transfer subscription \n: {0}", subscriptionText);
                                                           }
                                                       },
                                                       err => string.Format("Could not transfer subscription due to {0}.", err));

            if (!skipAdminCheck && checkFailed)
            {
                //If the AdminCheck fails, display warning to user and check if he still wishes to proceed
                result.GetUserConfirmation = true;
            }
            return result;
        }


        /// <summary>
        /// Adds the on behalf billing subscription.
        /// </summary>
        public IAcisSMEOperationResponse AddOnBehalfBillingSubscription(string serviceName, string subscriptionId, IEnumerable<string> usageKinds, string coadminEmail)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           OnBehalfSubscriptionRegistrationType registration = new OnBehalfSubscriptionRegistrationType();
                                                           RegisteredUsageKindsType usageKindsType = new RegisteredUsageKindsType();
                                                           usageKindsType.AddRange(usageKinds);
                                                           registration.RegisteredUsageKinds = usageKindsType;
                                                           registration.SubscriptionId = subscriptionId;

                                                           using (context())
                                                           {
                                                               OnBehalfSubscriptionRegistrationsType registrations = admin.AddOnBehalfBillingSubscriptions(serviceName, registration, coadminEmail);
                                                               return "Successfully added.";
                                                           }
                                                       },
                err => string.Format("Failed to add on behalf billing subscription due to {0}.", err));
        }

        /// <summary>
        /// Removes the on behalf billing subscription.
        /// </summary>
        public IAcisSMEOperationResponse RemoveOnBehalfBillingSubscription(string serviceName, string subscriptionId)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               OnBehalfSubscriptionRegistrationType registration = new OnBehalfSubscriptionRegistrationType();
                                                               registration.SubscriptionId = subscriptionId;
                                                               admin.RemoveOnBehalfBillingSubscriptions(serviceName, registration);
                                                               return "Successfully removed.";
                                                           }
                                                       },
                err => string.Format("Failed to remove on behalf billing subscription due to {0}", err));
        }

        /// <summary>
        /// Puts the resource provider billing runtime setting.
        /// </summary>
        public IAcisSMEOperationResponse PutResourceProviderBillingRuntimeSetting(string resourceProviderNamespace, string settingName, string settingValue)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               admin.PutResourceProviderBillingRuntimeSetting(resourceProviderNamespace, settingName, settingValue);
                                                               return "Successfully set value.";
                                                           }
                                                       },
                err => string.Format("Failed to set value due to {0}.", err));
        }

        /// <summary>
        /// Deletes the resource provider billing runtime setting.
        /// </summary>
        public IAcisSMEOperationResponse DeleteResourceProviderBillingRuntimeSetting(string resourceProviderNamespace, string settingName)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               admin.DeleteResourceProviderBillingRuntimeSetting(resourceProviderNamespace, settingName);
                                                               return "Successfully deleted value.";
                                                           }
                                                       },
                err => string.Format("Failed to delete value due to {0}.", err));
        }

        /// <summary>
        /// Adds the geo region.
        /// </summary>
        public IAcisSMEOperationResponse AddGeoRegion(string contents)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               GeoRegion region = this.DeserializeFile<GeoRegion>(contents);
                                                               admin.AddGeoRegion(region);
                                                               return "Successfully added georegion";
                                                           }
                                                       },
                err => string.Format("Failed to add georegion due to {0}.", err));
        }

        /// <summary>
        /// Updates the geo region.
        /// </summary>
        public IAcisSMEOperationResponse UpdateGeoRegion(string name, string contents)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               GeoRegion region = this.DeserializeFile<GeoRegion>(contents);
                                                               admin.UpdateGeoRegion(name, region);
                                                               return "Successfully updated georegion";
                                                           }
                                                       },
                err => string.Format("Failed to update georegion due to {0}.", err));
        }

        /// <summary>
        /// Deletes the geo region.
        /// </summary>
        public IAcisSMEOperationResponse DeleteGeoRegion(string name)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               admin.DeleteGeoRegion(name);
                                                               return "Successfully deleted georegion";
                                                           }
                                                       },
                err => string.Format("Failed to delete georegion due to {0}.", err));
        }

        /// <summary>
        /// Lists the geo region.
        /// </summary>
        public IAcisSMEOperationResponse ListGeoRegions()
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               GeoRegionsList geoRegions = admin.ListGeoRegions();
                                                               DataContractSerializer serializer = new DataContractSerializer(typeof(GeoRegion));
                                                               StringBuilder builder = new StringBuilder("<GeoRegions>");
                                                               foreach (GeoRegion region in geoRegions)
                                                               {
                                                                   using (MemoryStream m = new MemoryStream())
                                                                   {
                                                                       serializer.WriteObject(m, region);

                                                                       m.Seek(0, SeekOrigin.Begin);
                                                                       using (StreamReader r = new StreamReader(m))
                                                                       {
                                                                           builder.AppendLine(r.ReadToEnd());
                                                                       }
                                                                   }
                                                               }

                                                               builder.AppendLine("</GeoRegions>");
                                                               return builder.ToString();
                                                           }
                                                       },
                err => string.Format("Failed to list geo regions due to {0}.", err));
        }

        /// <summary>
        /// Lists the geo region.
        /// </summary>
        public IAcisSMEOperationResponse GetGeoSettings()
        {
            IAcisSMEOperationResponse response = null;

            response = this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           GeoSettingsList geoSettings = null;
                                                           using (context())
                                                           {
                                                               geoSettings = admin.GetGeoSettings();                                                               
                                                           }

                                                           GeoSettingsFormatter formatter = new GeoSettingsFormatter(this.Endpoint);
                                                           return formatter.Format(geoSettings);
                                                       },
                err => string.Format("Failed to list geo settings due to {0}.", err));

            return response;
        }


        public IAcisSMEOperationResponse AddNetworkGeoId(string id, bool wideVNetCreationEnabled, bool skipValidation, IEnumerable<string> networkEndpointList)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           NetworkGeoId networkGeoId = new NetworkGeoId
                                                            {
                                                                Id = id,
                                                                WideVNetCreationEnabled = wideVNetCreationEnabled,
                                                                SkipValidation = skipValidation,
                                                                NetworkEndpoints = new NetworkEndpointList()
                                                            };
                                                           networkGeoId.NetworkEndpoints.AddRange(networkEndpointList);

                                                           using (context())
                                                           {
                                                               admin.AddNetworkGeoId(networkGeoId);
                                                               return "Successfully added Network Geo ID";
                                                           }
                                                       },
                err => string.Format("Failed to add Network Geo ID due to {0}.", err));
        }

        public IAcisSMEOperationResponse UpdateNetworkGeoId(string id, bool wideVNetCreationEnabled, bool skipValidation, IEnumerable<string> networkEndpointList)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           // Get GEO Settings
                                                           // Find the given ID, and rebuild Network Endpoints for update call

                                                           NetworkGeoId networkGeoId = new NetworkGeoId
                                                           {
                                                               Id = id,
                                                               WideVNetCreationEnabled = wideVNetCreationEnabled,
                                                               SkipValidation = skipValidation,
                                                               NetworkEndpoints = new NetworkEndpointList()
                                                           };
                                                           networkGeoId.NetworkEndpoints.AddRange(networkEndpointList);

                                                           using (context())
                                                           {
                                                               admin.UpdateNetworkGeoId(id, networkGeoId);
                                                               return "Successfully updated Network Geo ID";
                                                           }
                                                       },
                err => string.Format("Failed to update Network Geo ID due to {0}.", err));
        }

        public IAcisSMEOperationResponse DeleteNetworkGeoId(string id)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               admin.DeleteNetworkGeoId(id);
                                                               return "Successfully deleted Network Geo ID";
                                                           }
                                                       },
                err => string.Format("Failed to delete Network Geo ID due to {0}.", err));
        }

        public IAcisSMEOperationResponse AddGeoLocaleToNetworkGeoIdAssociation(string geoLocaleId, string networkGeoId)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               admin.AddGeoLocaleToNetworkGeoIdAssociation(geoLocaleId, networkGeoId);
                                                               return "Successfully added Geo Locale to Network Geo ID Association";
                                                           }
                                                       },
                err => string.Format("Failed to add Geo Locale to Network Geo ID Association due to {0}.", err));
        }

        public IAcisSMEOperationResponse DeleteGeoLocaleToNetworkGeoIdAssociation(string geoLocaleId, string networkGeoId)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               admin.DeleteGeoLocaleToNetworkGeoIdAssociation(geoLocaleId, networkGeoId);
                                                               return "Successfully deleted Geo Locale to Network Geo ID Association";
                                                           }
                                                       },
                err => string.Format("Failed to delete Geo Locale to Network Geo ID Association due to {0}.", err));
        }

        public IAcisSMEOperationResponse DetermineDeploymentFabricHost(string subscriptionId, string deploymentName)
        {
            string errorMessage;
            Subscription subscription = this.ExecuteManagementOperation<Subscription>(
                                                                             (admin, context) =>
                                                                             {
                                                                                 using (context())
                                                                                 {
                                                                                     Subscription svc = admin.GetSubscriptionWithDetails(subscriptionId, "Full");
                                                                                     return svc;
                                                                                 }
                                                                             }, err => string.Format("Failed to get subscription due to {0}", err), out errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return AcisSMEOperationResponseExtensions.SpecificErrorResponse(errorMessage);
            }
            Deployment deployment = null;
            HostedService hostedService = null;
            if (subscription.HostedServices != null)
            {
                foreach (HostedService hservice in subscription.HostedServices.Where(hservice => hservice.Deployments != null))
                {
                    foreach (Deployment deploymentInfo in hservice.Deployments.Where(deploymentInfo => deploymentName == deploymentInfo.Name))
                    {
                        deployment = deploymentInfo;
                        hostedService = hservice;
                        break;
                    }
                }
            }

            if (deployment == null)
            {
                return AcisSMEOperationResponseExtensions.SpecificErrorResponse("The specified deployment was not found in the subscription");
            }

            string geoId = hostedService.GeoId;
            
            GeoSettingsList geoSettings = this.ExecuteAdministrationOperation<GeoSettingsList>(
                                                                                           (admin, context) =>
                                                                                           {
                                                                                               using (context())
                                                                                               {
                                                                                                   return admin.GetGeoSettings();
                                                                                               }
                                                                                           },
                                                                                           err => string.Format("Unable to list geo settings due to {0}", err),
                                                                                           out errorMessage);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                return AcisSMEOperationResponseExtensions.BuildFailure(errorMessage);
            }

            GeoSetting fabricGeoIds = geoSettings.FirstOrDefault(gs => gs.Name.ToUpperInvariant().Equals("FABRICGEOIDS"));
            if (fabricGeoIds == null)
            {
                return AcisSMEOperationResponseExtensions.BuildFailure("FabricGeoIds was null after running get geosettings.");                
            }

            string fabricGeoIdsConfig = fabricGeoIds.Value;

            // code adapted from analyze IaaS tool
            Dictionary<string, string> connectionInfos = (from firstSplit in fabricGeoIdsConfig.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries)
                                                          where !String.IsNullOrEmpty(firstSplit.Trim())
                                                          let secondSplit = firstSplit.Trim().Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries)
                                                          where secondSplit != null && secondSplit.Length == 2
                                                          let thirdSplit = secondSplit[1].Trim().Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                                                          where thirdSplit != null && thirdSplit.Length > 0
                                                          let fourthSplit = thirdSplit[0].Trim().Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                                                          where fourthSplit != null && fourthSplit.Length > 0
                                                          select new {Host = secondSplit[0].Trim(), Fqdn = fourthSplit[0].Trim()}).ToDictionary(o => o.Host, o => o.Fqdn);

            return connectionInfos.ContainsKey(geoId) 
                ? AcisSMEOperationResponseExtensions.BuildSuccess(connectionInfos[geoId]) 
                : AcisSMEOperationResponseExtensions.BuildFailure("Unable to find fabric information");
        }

        public IAcisSMEOperationResponse GetSubscriptionWithDetails(string subscriptionId, string detailLevel)
        {
            return this.ExecuteManagementOperation(
                (admin, context) =>
                {
                    using (context())
                    {
                        Subscription subscription = admin.GetSubscriptionWithDetails(
                            subscriptionId, 
                            detailLevel);

                        SubscriptionDetailFormatter subscriptionFormatter = new SubscriptionDetailFormatter(
                            this.Endpoint);

                        string result = subscriptionFormatter.FormatSingle(subscription);

                        return result;
                    }
                },
                err => string.Concat(string.Format("Subscription Details not found for SubscriptionID '{0}'", subscriptionId), string.IsNullOrWhiteSpace(err) ? "" : "\n" + err));            
        }

        public IAcisSMEOperationResponse ListSubscriptionsWithDetails(string puid, string detailLevel)
        {
            return this.ExecuteManagementOperation(
                (admin, context) =>
                {
                    using (context())
                    {
                        SubscriptionList subscriptionList = admin.ListSubscriptionsWithDetails(
                            puid,
                            detailLevel);

                        SubscriptionDetailFormatter subscriptionFormatter = new SubscriptionDetailFormatter(
                            this.Endpoint);

                        string result = subscriptionFormatter.FormatList(subscriptionList);

                        return result;
                    }
                },
                err => string.Format("No subscriptions found for PUID '{0}'. Error details: '{1}'", puid, err));
        }

        public IAcisSMEOperationResponse GetExtensionResourceInformation(string serviceNameId, string resourceNameId)
        {
            return this.ExecuteAdministrationOperation(
                (admin, context) =>
                {
                    using (context())
                    {
                        ResourceType resourceType = admin.GetExtensionResource(
                            serviceNameId,
                            resourceNameId);

                        ResourceTypeFormatter resourceFormatter = new ResourceTypeFormatter(
                            this.Endpoint);

                        string result = resourceFormatter.Format(resourceType);

                        return result;
                    }
                },
                err => string.Concat(string.Format("Failed to retrieve extension resource '{0}' for extension service '{1}'", resourceNameId, serviceNameId), string.IsNullOrWhiteSpace(err) ? "" : "\n" + err));
        }

        public IAcisSMEOperationResponse PushFabricGeoIdsToNetworkEndpoints(string locationConstraintId)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               admin.PushFabricGeoIdsToNetworkEndpoints(locationConstraintId);
                                                               return "Successfully pushed Fabric Geo IDs to Network Endpoints";
                                                           }
                                                       },
                err => string.Format("Failed to push Fabric Geo IDs to Network Endpoints due to {0}.", err));
        }


        /// <summary>
        /// Updates the subscription high availability status.
        /// </summary>
        public IAcisSMEOperationResponse UpdateSubscriptionHighAvailabilityStatus(string subscriptionId, bool replicationEnabled, string preferredRegion)
        {
            return this.ExecuteManagementOperation(
                                                   (admin, context) =>
                                                   {
                                                       using (context())
                                                       {
                                                           admin.UpdateSubscriptionHighAvailabilityStatus(subscriptionId, replicationEnabled, preferredRegion);
                                                           return "Successfully updated subscription high availability status";
                                                       }
                                                   },
                err => string.Format("Failed to update subscription high availability status due to {0}.", err));
        }

        /// <summary>
        /// Discovers the puid services.
        /// </summary>
        public IAcisSMEOperationResponse DiscoverPuidServices(string puid, bool isAAPuid)
        {
            return this.ExecuteManagementOperation(
                                                   (admin, context) =>
                                                   {
                                                       using (context())
                                                       {
                                                           PuidServicesList puidServices = admin.DiscoverPuidServices(puid, isAAPuid);
                                                           StringAndFormattingUtilities.ResponseFormatter formatter = new StringAndFormattingUtilities.ResponseFormatter(this.Endpoint);
                                                           formatter.FormatList(puidServices, (f, p) =>
                                                           {
                                                               f.AppendProperty("Puid", p.Puid);
                                                               f.AppendProperty("SubscriptionID", p.SubscriptionID);
                                                               f.AppendProperty("ServiceName", p.ServiceName);
                                                               f.AppendProperty("ServiceType", p.ServiceType);
                                                               f.AppendProperty("ServiceUri", p.ServiceUri);
                                                           });

                                                           return formatter.ToString();
                                                       }
                                                   },
                err => string.Format("Failed to discover puid services due to {0}.", err));
        }

        /// <summary>
        /// Discovers the subscription services
        /// </summary>
        public IAcisSMEOperationResponse DiscoverSubscriptionServices(string subscriptionId)
        {
            return this.ExecuteManagementOperation(
                                                   (admin, context) =>
                                                   {
                                                       using (context())
                                                       {
                                                           SubscriptionServicesList subscriptionServices = admin.DiscoverSubscriptionServices(subscriptionId);
                                                           StringAndFormattingUtilities.ResponseFormatter formatter = new StringAndFormattingUtilities.ResponseFormatter(this.Endpoint);
                                                           formatter.FormatList(subscriptionServices, (f, p) =>
                                                           {
                                                               f.AppendProperty("SubscriptionID", p.SubscriptionID);
                                                               f.AppendProperty("ServiceName", p.ServiceName);
                                                               f.AppendProperty("ServiceType", p.ServiceType);
                                                               f.AppendProperty("ServiceUri", p.ServiceUri);
                                                           });

                                                           return formatter.ToString();
                                                       }
                                                   },
                err => string.Format("Failed to discover subscription services due to {0}.", err));
        }

        public IAcisSMEOperationResponse ListLatestResourceExtensions(string subscriptionId)
        {
            return this.ExecuteManagementOperation(
                                                   (admin, context) =>
                                                   {
                                                       using (context())
                                                       {
                                                           ExtensionImageList images = admin.ListLatestExtensions(subscriptionId);
                                                           return this.SerializeFile(images);
                                                       }
                                                   },
                err => string.Format("Failed to list latest resource extensions due to {0}.", err));
        }

        public IAcisSMEOperationResponse ListResourceExtensionVersions(string subscriptionId, string providerNamespace, string type)
        {
            return this.ExecuteManagementOperation(
                                                   (admin, context) =>
                                                   {
                                                       using (context())
                                                       {
                                                           ExtensionImageList images = admin.ListExtensionVersions(subscriptionId, providerNamespace, type);
                                                           return this.SerializeFile(images);
                                                       }
                                                   },
                err => string.Format("Failed to list resource extension versions due to {0}.", err));
        }

        public IAcisSMEOperationResponse PushNotification(string notificationTarget, EntityEvent entityEvent, bool? queueOnFailure)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               admin.PushNotification(notificationTarget, entityEvent, queueOnFailure.HasValue ? queueOnFailure.Value : false);
                                                               return "Successfully pushed notification";
                                                           }
                                                       },
                                                       err => string.Format("Failed to push notification due to {0}.", err));
        }

        public IAcisSMEOperationResponse ListSubscriptionOperationHistory(string subscriptionId, string fromTime, string toTime, int maxResults, string continuationToken)
        {
            return this.ExecuteManagementOperation(
                                                   (admin, context) =>
                                                   {
                                                       using (context())
                                                       {
                                                           StringAndFormattingUtilities.ResponseFormatter formatter = new StringAndFormattingUtilities.ResponseFormatter(this.Endpoint);
                                                           List<SubscriptionOperation> allOperations = new List<SubscriptionOperation>();
                                                           SubscriptionOperationCollection operations;

                                                           do
                                                           {
                                                               operations = admin.ListSubscriptionOperations(subscriptionId, fromTime, toTime, null, null, continuationToken);
                                                               allOperations.AddRange(operations.SubscriptionOperations);
                                                               maxResults -= operations.SubscriptionOperations.Count;
                                                               continuationToken = operations.ContinuationToken;
                                                           } while (maxResults > 0 && continuationToken != null);

                                                           string prependValue = string.Empty;
                                                           if (continuationToken != null)
                                                           {
                                                               prependValue = "Partial result set - call this operation again with the continuation token specified below in order to get more results\n";
                                                               formatter.AppendProperty("ContinuationToken", continuationToken);
                                                           }

                                                           formatter.FormatList(allOperations, FormatSubscriptionOperation);
                                                           return prependValue + formatter.ToString();
                                                       }
                                                   },
                err => string.Format("Failed to list operation subscription history due to {0}.", err));
        }
		
		/// <summary>
        /// Adds role size configuration.
        /// </summary>
        public IAcisSMEOperationResponse AddRoleSizeConfiguration(string roleSize, string jsonConfig)
        {
            string roleSizeConfigs = string.Empty;
            return this.ExecuteAdministrationOperation(
                (mgmt, context) =>
                {
                    using (context())
                    {
                        roleSizeConfigs = mgmt.AddRoleSizeConfiguration(roleSize, jsonConfig);
                    }

                    return roleSizeConfigs;
                }, err => "Failed to add role size config due to " + err);
        }

        /// <summary>
        /// Updates role size configuration.
        /// </summary>
        public IAcisSMEOperationResponse UpdateRoleSizeConfiguration(string roleSize, string jsonConfig)
        {
            string roleSizeConfigs = string.Empty;
            return this.ExecuteAdministrationOperation(
                (mgmt, context) =>
                {
                    using (context())
                    {
                        roleSizeConfigs = mgmt.UpdateRoleSizeConfiguration(roleSize, jsonConfig);
                    }

                    return roleSizeConfigs;
                }, err => "Failed to update role size config due to " + err);
        }

        // <summary>
        /// Lists role size configurations.
        /// </summary>
        public IAcisSMEOperationResponse ListRoleSizeConfigurations()
        {
            string roleSizeConfigs = string.Empty;
            return this.ExecuteAdministrationOperation(
                (mgmt, context) =>
                {
                    using (context())
                    {
                        roleSizeConfigs = mgmt.ListRoleSizeConfigurations();
                    }

                    return roleSizeConfigs;
                }, err => "Failed to list role size configs due to " + err);
        }

        /// <summary>
        /// Gets role size configuration.
        /// </summary>
        public IAcisSMEOperationResponse GetRoleSizeConfiguration(string roleSize)
        {
            string roleSizeConfig = string.Empty;
            return this.ExecuteAdministrationOperation(
                (mgmt, context) =>
                {
                    using (context())
                    {
                        roleSizeConfig = mgmt.GetRoleSizeConfiguration(roleSize);
                    }

                    return roleSizeConfig;
                }, err => "Failed to get role size config due to " + err);
        }

        /// <summary>
        /// Removes role size configuration.
        /// </summary>
        public IAcisSMEOperationResponse RemoveRoleSizeConfiguration(string roleSize)
        {
            string roleSizeConfigs = string.Empty;
            return this.ExecuteAdministrationOperation(
                (mgmt, context) =>
                {
                    using (context())
                    {
                        roleSizeConfigs = mgmt.RemoveRoleSizeConfiguration(roleSize);
                    }

                    return roleSizeConfigs;
                }, err => "Failed to remove role size config due to " + err);
        }

        private static void FormatSubscriptionOperation(StringAndFormattingUtilities.ResponseFormatter builder, SubscriptionOperation operation)
        {
            builder.AppendProperty("OperationId", operation.OperationId);
            builder.AppendProperty("OperationName", operation.OperationName);
            builder.AppendProperty("OperationObjectId", operation.OperationObjectId);

            if (operation.OperationCaller != null)
            {
                builder.AppendProperty("UserEmailAddress", operation.OperationCaller.UserEmailAddress);
                builder.AppendProperty("ClientIP", operation.OperationCaller.ClientIP);
                builder.AppendProperty("SubscriptionCertificateThumbprint", operation.OperationCaller.SubscriptionCertificateThumbprint);
                builder.AppendProperty("UsedServiceManagementApi", operation.OperationCaller.UsedServiceManagementApi);
            }

            if (operation.OperationParameters != null && operation.OperationParameters.Count > 0)
            {
                builder.Nest("OperationParameters");
                foreach (OperationParameter parameter in operation.OperationParameters)
                {
                    builder.AppendProperty("Name", parameter.Name);
                    if (parameter.GetValue() != null)
                    {
                        builder.AppendProperty("Value", parameter.GetValue().ToString());
                    }
                }
                builder.Unnest();
            }

            if (operation.OperationTrackingData != null)
            {
                builder.Nest("OperationTrackingData");
                builder.AppendProperty("OperationId", operation.OperationTrackingData.OperationId);
                builder.AppendProperty("OperationKind", operation.OperationTrackingData.OperationKind);
                builder.AppendProperty("OperationStatus", operation.OperationTrackingData.OperationStatus);
                builder.AppendProperty("SubscriptionId", operation.OperationTrackingData.SubscriptionId);
                builder.AppendProperty("TimeStarted", operation.OperationTrackingData.TimeStarted.ToString());
                builder.AppendProperty("TimeCompleted", operation.OperationTrackingData.TimeCompleted.ToString());
                builder.AppendProperty("HttpStatusCode", operation.OperationTrackingData.HttpStatusCode);
                if (operation.OperationTrackingData.ErrorDetail != null)
                {
                    builder.Nest("ErrorDetail");
                    builder.AppendProperty("Timestamp", operation.OperationTrackingData.ErrorDetail.Timestamp.ToString());
                    builder.AppendProperty("HostName", operation.OperationTrackingData.ErrorDetail.HostName);

                    if (operation.OperationTrackingData.ErrorDetail.Exception != null)
                    {
                        builder.AppendProperty("Exception.Message", operation.OperationTrackingData.ErrorDetail.Exception.Message);
                        builder.AppendProperty("Exception.StackTrace", operation.OperationTrackingData.ErrorDetail.Exception.StackTrace);
                        if (operation.OperationTrackingData.ErrorDetail.Exception.InnerException != null)
                        {
                            builder.AppendProperty("InnerException.Message", operation.OperationTrackingData.ErrorDetail.Exception.InnerException.Message);
                            builder.AppendProperty("InnerException.StackTrace", operation.OperationTrackingData.ErrorDetail.Exception.InnerException.StackTrace);
                        }
                    }
                    builder.AppendProperty("ExtendedDescription", operation.OperationTrackingData.ErrorDetail.ExtendedDescription);
                    builder.AppendProperty("Code", operation.OperationTrackingData.ErrorDetail.Code);

                    if (operation.OperationTrackingData.ErrorDetail.Addresses != null && operation.OperationTrackingData.ErrorDetail.Addresses.Length > 0)
                    {
                        foreach (string address in operation.OperationTrackingData.ErrorDetail.Addresses)
                        {
                            builder.AppendProperty("ErrorDetail.Address", address);
                        }
                    }
                    builder.Unnest();
                }
                builder.Unnest();
            }
        }

        /// <summary>
        /// Executes the consistency check.
        /// </summary>
        public IAcisSMEOperationResponse ExecuteConsistencyCheck(string group, int? inconsistencyLimit, bool? onlyAAPuid, string puid, string subscriptionId, int? unknownTimeLimit, int? volatileTimeLimit)
        {
            return this.ExecuteManagementOperation(
                                                   (admin, context) =>
                                                   {
                                                       using (context())
                                                       {
                                                           ConsistencyCheck input = new ConsistencyCheck();
                                                           input.Group = group;
                                                           input.InconsistencyLimit = inconsistencyLimit;
                                                           input.OnlyAAPuid = onlyAAPuid;
                                                           input.Puid = puid;
                                                           input.SubscriptionId = subscriptionId;
                                                           input.UnknownTimeLimit = unknownTimeLimit;
                                                           input.VolatileTimeLimit = volatileTimeLimit;

                                                           ConsistencyReportList consistency = admin.ConsistencyCheck(input);
                                                           StringAndFormattingUtilities.ResponseFormatter formatter = new StringAndFormattingUtilities.ResponseFormatter(this.Endpoint);
                                                           formatter.FormatList(consistency, (f, c) =>
                                                           {
                                                               f.AppendProperty("SubscriptionId", c.SubscriptionId);
                                                               f.AppendProperty("Puid", c.Puid);
                                                               f.AppendProperty("State", c.State);
                                                               f.AppendProperty("Home", c.Home);
                                                               f.AppendProperty("HomeUpdated", c.HomeUpdated);
                                                               f.AppendProperty("TimeState", c.TimeState);
                                                               f.FormatList(c.MissingEntities, this.FormatMissingEntity);
                                                               f.FormatList(c.ModifiedEntities, this.FormatModifiedEntity);
                                                               f.FormatList(c.ExtraEntities, this.FormatExtraEntity);
                                                           });

                                                           return formatter.ToString();
                                                       }
                                                   },
                err => string.Format("Failed to check consistency due to {0}.", err));
        }

        public IAcisSMEOperationResponse UpdateSubscriptionOfferType(string subscriptionId, string offerType)
        {
            return this.ExecuteManagementOperation(
                (svcMgmt, context) =>
                {
                    Subscription subscription;
                    using (context())
                    {
                        subscription = svcMgmt.GetSubscription(subscriptionId);
                    }
                    if (string.Equals(subscription.OfferType, offerType))
                    {
                        return string.Format("Subscription ID {0} already has offer type \'{1}\'.  Subscription not updated.", subscriptionId, subscription.OfferType ?? "<null>");
                    }
                    using(context())
                    {
                        // Update OfferType
                        subscription.OfferType = offerType;
                        svcMgmt.UpdateSubscription(subscription.SubscriptionID, subscription);
                    }
                    return string.Format("Subscription ID {0} updated to offer type \'{1}\'.", subscription.SubscriptionID, subscription.OfferType);
                },
                err => string.Format("Failed to update Subscription {0} to offer type \'{1}\'. error was: {2}.", subscriptionId, offerType, err));
        }

        public IAcisSMEOperationResponse UpdateFabricReservations(string fabricGeoIdStr, string fabricReservations)
        {
            return this.ExecuteAdministrationOperation(
                (admin, context) =>
                {
                    FabricGeoId fabricGeoId = new FabricGeoId
                    {
                        Id = fabricGeoIdStr,
                        Reservations = new GeoIdParser().ParseFabricReservations(fabricReservations)
                    };
                    string trackingId;
                    using (context())
                    {
                        admin.UpdateFabricGeoId(fabricGeoId.Id, fabricGeoId);
                        trackingId = GetRdfeTrackingId();
                    }

                    string errorDetail;
                    if (!WaitForRdfeTracking(admin, context, trackingId, out errorDetail))
                    {
                        throw new SmeRdfeOperationStepException(string.Format("Failed to update fabric reservations.  Error detail: {0}", errorDetail ?? "<NoDetailAvailable>"));
                    }

                    return "Updated Fabric Reservations.";
                },
                err => "Failed to update fabric reservations. " + err);
        }

        public IAcisSMEOperationResponse CheckVirtualNetworkIPAddressAvailability(string subscriptionId, string virtualNetworkName, string address)
        {
            return this.ExecuteManagementOperation(
                                                   (admin, context) =>
                                                   {
                                                       using (context())
                                                       {
                                                           AddressAvailabilityResponse addressAvailabilityResponse = admin.CheckVirtualNetworkIPAddressAvailability(subscriptionId,virtualNetworkName,address);
                                                           StringAndFormattingUtilities.ResponseFormatter formatter = new StringAndFormattingUtilities.ResponseFormatter(this.Endpoint);
                                                           formatter.FormatList(
                                                               addressAvailabilityResponse.AvailableAddresses,
                                                               (f, a) =>
                                                               {
                                                                   f.AppendProperty("Address", a.ToString());
                                                               } );
                                                           return formatter.ToString();
                                                       }
                                                   },
                err => string.Format("Failed to check virtual network IP address availability due to {0}.", err));
        }

        public IAcisSMEOperationResponse UploadHostedServiceExtension(string fileContents, string extensionName, bool overwriteIfExists)
        {
            return this.ExecuteAdministrationOperation(
                                                       (admin, context) =>
                                                       {
                                                           using (context())
                                                           {
                                                               // Required to "prime" the channel prior to calling AddGuestAgentVersion
                                                               List<string> unusedApiVersions = admin.ListApiVersions();
                                                           }

                                                           using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(fileContents)))
                                                           {
                                                               using (context())
                                                               {
                                                                   admin.UploadHostedServiceExtension(stream, extensionName, overwriteIfExists);
                                                               }
                                                           }

                                                           return "Successfully uploaded hosted service extension.";
                                                       },
                                                       err => string.Format("Failed to upload hosted service extension due to {0}.", err));
        }

        /// <summary>
        /// Enables subscription using provisioning api
        /// </summary>
        public IAcisSMEOperationResponse EnableSubscription(string subscriptionId, string accountAdminLivePuid, string accountAdminLiveEmailId, string friendlyName)
        {
            return this.ExecuteProvisioningOperation(
                                                   (pa, context) =>
                                                   {
                                                       using (context())
                                                       {
                                                           AzureProvisioningInfo azureProvisioningInfo = new AzureProvisioningInfo
                                                           {
                                                               SubscriptionId = new Guid(subscriptionId),
                                                               AccountAdminLivePuid = accountAdminLivePuid,
                                                               AccountAdminLiveEmailId = accountAdminLiveEmailId,
                                                               FriendlyName = friendlyName
                                                           };
                                                           pa.EnableSubscription(azureProvisioningInfo);
                                                           return "Successfully enabled subscription: " + subscriptionId;
                                                       }
                                                   },
                err => string.Format("Failed to enable subscription {0} due to {1}.", subscriptionId, err));
        }

        /// <summary>
        /// Disables subscription using provisioning api
        /// </summary>
        public IAcisSMEOperationResponse DisableSubscription(string subscriptionId, string accountAdminLivePuid, string accountAdminLiveEmailId, string friendName)
        {
            return this.ExecuteProvisioningOperation(
                                                   (pa, context) =>
                                                   {
                                                       using (context())
                                                       {
                                                           AzureProvisioningInfo azureProvisioningInfo = new AzureProvisioningInfo
                                                           {
                                                               SubscriptionId = new Guid(subscriptionId),
                                                               AccountAdminLivePuid = accountAdminLivePuid,
                                                               AccountAdminLiveEmailId = accountAdminLiveEmailId,
                                                               FriendlyName = friendName
                                                           };
                                                           pa.DisableSubscription(azureProvisioningInfo);
                                                           return "Successfully disabled subscription: " + subscriptionId;
                                                       }
                                                   },
                                                   err => string.Format("Failed to disable subscription {0} due to {1}.", subscriptionId, err));
        }

        #region GeoTenant Operations

        public IAcisSMEOperationResponse AddGeoTenant(
            string name,
            bool isAvailable,
            string managementEndpoint,
            string adminEndpoint,
            string umApiEndpoint,
            string secondaryOfThisLocation,
            bool geoRedirectSwitch,
            bool geoDeploymentRedirectSwitch,
            bool geoRedirectUseClientHeaderSwitch,
            GeoRedirectLocationsList geoRedirectLocations,
            IPRangesList geoRedirectIpWhitelist,
            bool skipValidation)
        {
            return this.ExecuteAdministrationOperation(
                (admin, context) =>
                {
                    GeoTenant geoTenant = new GeoTenant
                    {
                        Name = name,
                        IsAvailable = isAvailable,
                        ManagementEndpoint = managementEndpoint,
                        AdminEndpoint = adminEndpoint,
                        UMApiEndpoint = umApiEndpoint,
                        SecondaryOfThisLocation = secondaryOfThisLocation,
                        GeoRedirectSwitch = geoRedirectSwitch,
                        GeoDeploymentRedirectSwitch = geoDeploymentRedirectSwitch,
                        GeoRedirectUseClientHeaderSwitch = geoRedirectUseClientHeaderSwitch,
                        GeoRedirectLocations = geoRedirectLocations,
                        GeoRedirectIPWhiteList = geoRedirectIpWhitelist
                    };
                    using (context())
                    {
                        admin.AddGeoTenant(geoTenant, skipValidation);
                    }
                    return string.Format("Added GeoTenant \'{0}\'.", name);
                },
                err => "Failed to add GeoTenant. " + err);
        }

        public IAcisSMEOperationResponse DeleteGeoTenant(string name)
        {
            return this.ExecuteAdministrationOperation(
                (admin, context) =>
                {
                    using (context())
                    {
                        admin.DeleteGeoTenant(name);
                    }
                    return string.Format("Deleted GeoTenant \'{0}\'.", name);
                },
                err => "Failed to delete GeoTenant. " + err);
        }

        public IAcisSMEOperationResponse UpdateGeoTenant(
            string name,
            bool isAvailable,
            string managementEndpoint,
            string adminEndpoint,
            string umApiEndpoint,
            string secondaryOfThisLocation,
            bool geoRedirectSwitch,
            bool geoDeploymentRedirectSwitch,
            bool geoRedirectUseClientHeaderSwitch,
            GeoRedirectLocationsList geoRedirectLocations,
            IPRangesList geoRedirectIpWhitelist,
            bool skipValidation)
        {
            return this.ExecuteAdministrationOperation(
                (admin, context) =>
                {
                    GeoTenant geoTenant = new GeoTenant
                    {
                        Name = name,
                        IsAvailable = isAvailable,
                        ManagementEndpoint = managementEndpoint,
                        AdminEndpoint = adminEndpoint,
                        UMApiEndpoint = umApiEndpoint,
                        SecondaryOfThisLocation = secondaryOfThisLocation,
                        GeoRedirectSwitch = geoRedirectSwitch,
                        GeoDeploymentRedirectSwitch = geoDeploymentRedirectSwitch,
                        GeoRedirectUseClientHeaderSwitch = geoRedirectUseClientHeaderSwitch,
                        GeoRedirectLocations = geoRedirectLocations,
                        GeoRedirectIPWhiteList = geoRedirectIpWhitelist
                    };
                    using (context())
                    {
                        admin.UpdateGeoTenant(name, geoTenant, skipValidation);
                    }
                    return string.Format("Updated GeoTenant \'{0}\'.", name);
                },
                err => "Failed to update GeoTenant. " + err);
        }

        public IAcisSMEOperationResponse ListKnownOsVersions()
        {
            return this.ExecuteManagementOperation(
                (mgmt, context) =>
                {
                    OsFlavorList flavorList;
                    using (context())
                    {
                        flavorList = mgmt.ListOsFlavors();
                    }

                    return this.FormatObjectAsXml(flavorList);
                }, err => "Failed to retrieve OS flavor list due to: " + err);
        }

        public IAcisSMEOperationResponse ListPlatformImages()
        {
            return this.ExecuteAdministrationOperation(
                (mgmt, context) =>
                {
                    PlatformImageList platformImages;
                    using (context())
                    {
                        platformImages = mgmt.ListPlatformImages();                        
                    }

                    PlatformImageFormatter formatter = new PlatformImageFormatter(this.Endpoint);
                    return formatter.FormatPlatformImageListDisplay(platformImages);
                }, err => "Failed to list platform images due to " + err);
        }

        public IAcisSMEOperationResponse ListPlatformStorageStamps()
        {
            return this.ExecuteAdministrationOperation(
                (mgmt, context) =>
                {
                    PlatformStorageStampList storageStamps;
                    using (context())
                    {
                        storageStamps = mgmt.ListPlatformStorageStamps();
                    }

                    PlatformStorageStampFormatter formatter = new PlatformStorageStampFormatter(this.Endpoint);
                    return formatter.FormatPlatformStorageStampListDisplay(storageStamps);
                }, err => "Failed to list platform storage stamps due to " + err);
        }

        public IAcisSMEOperationResponse ListLocationsForSubscription(string subscriptionId)
        {
            return this.ExecuteManagementOperation(
                (mgmt, context) =>
                {
                    LocationList locations;
                    using (context())
                    {
                        locations = mgmt.ListLocations(subscriptionId);
                    }

                    LocationFormatter formatter = new LocationFormatter(this.Endpoint);
                    return formatter.FormatLocationListDisplay(locations);
                }, err => "Failed to retrieve locations due to " + err);
        }

        public IAcisSMEOperationResponse ListExtensionServices(ExtensionServiceFilterType filterType)
        {
            return this.ExecuteAdministrationOperation(
                (mgmt, context) =>
                {
                    ServiceNames serviceNames;
                    using (context())
                    {
                        serviceNames = mgmt.ListExtensionServices(filterType.ToString());
                    }

                    ExtensionServiceNameFormatter formatter = new ExtensionServiceNameFormatter(this.Endpoint);
                    return formatter.FormatExtensionServiceNameListDisplay(serviceNames);
                }, err => "Unable to retrieve extension service names due to " + err);
        }

        public IAcisSMEOperationResponse ListExtensibilityClientCertificates()
        {
            return this.ExecuteAdministrationOperation(
                (mgmt, context) =>
                {
                    ClientCertificateSettings certificateSettings;
                    using (context())
                    {
                        certificateSettings = mgmt.ListExtensibilityClientCertificates();
                    }

                    return this.FormatObjectAsXml(certificateSettings);
                },
                err => "Failed to list Client Certificate Settings due to " + err);
        }

        public IAcisSMEOperationResponse CreateOrUpdateServiceSetting(string name, string value)
        {
            return this.ExecuteAdministrationOperation(
                (admin, context) =>
                {
                    string trackingId;
                    using (context())
                    {
                        InsertUserAgentToOutgoingRequest();
                        admin.CreateOrUpdateServiceSetting(name, value);
                        trackingId = GetRdfeTrackingId();
                    }

                    string errorDetail;
                    if (!WaitForRdfeTracking(admin, context, trackingId, out errorDetail))
                    {
                        throw new SmeRdfeOperationStepException(string.Format("Failed to create or update service setting. Error detail: {0}", errorDetail ?? "<NoDetailAvailable>"));
                    }

                    return "Created or updated the service setting.";
                },
                err => "Failed to create or update service setting due to " + err);
        }

        /// <summary>
        /// Delete Service Setting
        /// </summary>
        /// <param name="settingName">Service Setting Name</param>
        /// <returns>IAcisSMEOperationResponse</returns>
        public IAcisSMEOperationResponse DeleteServiceSetting(string settingName)
        {
            return this.ExecuteAdministrationOperation(
                (admin, context) =>
                {
                    string trackingId;
                    using (context())
                    {
                        InsertUserAgentToOutgoingRequest();
                        admin.DeleteDynamicServiceSetting(settingName);
                        trackingId = GetRdfeTrackingId();
                    }

                    string errorDetail;
                    if (!WaitForRdfeTracking(admin, context, trackingId, out errorDetail))
                    {
                        throw new SmeRdfeOperationStepException(string.Format("Failed to delete the service setting. Error detail: {0}", errorDetail ?? "<NoDetailAvailable>"));
                    }

                    return "Deleted the service setting.";
                },
                err => string.Format("Unable to delete the service setting due to {0}.", err)); 
        }

        /// <summary>
        /// Get Service Setting
        /// </summary>
        /// <param name="settingName">Service Setting Name</param>
        /// <returns>IAcisSMEOperationResponse</returns>
        public IAcisSMEOperationResponse GetServiceSetting(string settingName)
        {
            return this.ExecuteAdministrationOperation(
                (admin, context) =>
                {
                    ServiceSetting serviceSetting;
                    using (context())
                    {
                        InsertUserAgentToOutgoingRequest();
                        serviceSetting = admin.GetServiceSetting(settingName);
                    }

                    ServiceSettingFormatter formatter = new ServiceSettingFormatter(this.Endpoint);
                    return formatter.FormatServiceSettingDisplay(serviceSetting);
                },
                err => string.Format("Unable to get service setting due to {0}.", err)); 
        }

        /// <summary>
        /// List Service Settings.
        /// </summary>
        /// <returns>IAcisSMEOperationResponse</returns>
        public IAcisSMEOperationResponse ListServiceSettings()
        {
            return this.ExecuteAdministrationOperation(
                (admin, context) =>
                {
                    ServiceSettingsList settingList;
                    using (context())
                    {
                        InsertUserAgentToOutgoingRequest();
                        settingList = admin.ListServiceSettings();
                    }

                    ServiceSettingFormatter formatter = new ServiceSettingFormatter(this.Endpoint);
                    return formatter.FormatServiceSettingsDisplay(settingList);
                },
                err => string.Format("Unable to list service settings due to {0}.", err));
        }

        public IAcisSMEOperationResponse ListAvailableFeatures()
        {
            return this.ExecuteAdministrationOperation(
                (mgmt, context) =>
                {
                    AvailableFeatureList featureList;
                    using (context())
                    {
                        featureList = mgmt.ListAvailableFeatures();
                    }

                    AvailableFeatureFormatter formatter = new AvailableFeatureFormatter(this.Endpoint);
                    return formatter.FormatAvailableFeatureListDisplay(featureList);
                }, err => "Unable to retrieve available features due to " + err);
        }

        public IAcisSMEOperationResponse FromPuidGetSubscriptionIds(string puid)
        {
            return this.ExecuteManagementOperation(
                (mgmt, context) =>
                {
                    SubscriptionList subscriptions;
                    using (context())
                    {
                        subscriptions = mgmt.ListSubscriptions(puid);
                    }

                    return string.Join(";", subscriptions.Select(sub => sub.SubscriptionID));
                }, err => "Unable to retrieve subscriptions for PUID due to " + err);
        }

        public IAcisSMEOperationResponse ListCacheAccounts()
        {
            return this.ExecuteAdministrationOperation(
                (mgmt, context) =>
                {
                    CacheAccountCollection cacheAccounts;
                    using (context())
                    {
                        cacheAccounts = mgmt.ListCacheAccounts();
                    }

                    CacheAccountFormatter formatter = new CacheAccountFormatter(this.Endpoint);
                    return formatter.FormatCacheAccountListDisplay(cacheAccounts);
                }, err => "Failed to list cache accounts due to " + err);
        }

        public IAcisSMEOperationResponse ListGuestAgentVersions()
        {
            return this.ExecuteAdministrationOperation(
                (mgmt, context) =>
                {
                    GuestAgentVersionList versionList;
                    using (context())
                    {
                        versionList = mgmt.ListGuestAgentVersions();
                    }

                    GuestAgentVersionFormatter formatter = new GuestAgentVersionFormatter(this.Endpoint);
                    return formatter.FormatGuestAgentVersionListDisplay(versionList);
                }, err => "Unable to retrieve guest agent version list due to " + err);
        }

        public IAcisSMEOperationResponse ListOsImages(string subscriptionId)
        {
            return this.ExecuteManagementOperation(
                (mgmt, context) =>
                {
                    OSImageList imageList;
                    using (context())
                    {
                        imageList = mgmt.ListOSImages(subscriptionId);
                    }

                    OsImageFormatter formatter = new OsImageFormatter(this.Endpoint);
                    return formatter.FormatOsImageListDisplay(imageList);
                }, err => "Failed to list OS images due to " + err);
        }

        public IAcisSMEOperationResponse ListOsFamilies()
        {
            return this.ExecuteManagementOperation(
                (mgmt, context) =>
                {
                    OsFamilyList flavorList;
                    using (context())
                    {
                        flavorList = mgmt.ListOsFamilies();
                    }

                    return this.FormatObjectAsXml(flavorList);
                }, err => "Failed to retrieve OS family list due to: " + err);
        }
        
        public IAcisSMEOperationResponse ListGeoTenants()
        {
            return this.ExecuteAdministrationOperation(
                (admin, context) =>
                {
                    GeoTenantsList tenantsList;
                    using (context())
                    {
                        tenantsList = admin.ListGeoTenants();
                    }

                    StringAndFormattingUtilities.ResponseFormatter formatter = new StringAndFormattingUtilities.ResponseFormatter(this.Endpoint);
                    formatter.FormatList(tenantsList, (f, tenant) =>
                    {
                        f.AppendProperties(tenant,
                            "Name",
                            "IsAvailable",
                            "ManagementEndpoint",
                            "AdminEndpoint",
                            "UMApiEndpoint",
                            "SecondaryOfThisLocation",
                            "GeoRedirectSwitch",
                            "GeoDeploymentRedirectSwitch",
                            "GeoRedirectUseClientHeaderSwitch");
                        f.FormatList(tenant.GeoRedirectLocations, (f2, redirect) =>
                        {
                            f2.AppendProperty("Name", redirect.Name, true);
                            f2.AppendProperty("Ratio", redirect.Ratio);
                        });
                        f.FormatList(tenant.GeoRedirectIPWhiteList, (f3, ipRange) =>
                        {
                            f3.AppendProperty("Lower", ipRange.Lower, true);
                            f3.AppendProperty("Upper", ipRange.Upper);
                        });
                    });

                    return formatter.ToString();
                },
                err => "Failed to list GeoTenants. " + err);
        }

          /// <summary>
        /// Parses all the publisher types to a particular value
        /// </summary>
        /// <param name="publisherTypeList"></param>
        /// <returns></returns>
        public static int ParsePublisherTypes(string publisherTypeList)
        {
            int publisherTypeVal = 0;
            foreach (string publisherTypeString in publisherTypeList.Split(';'))
            {
                PublisherType publisherType;
                if (!Enum.TryParse(publisherTypeString, out publisherType))
                {
                    throw new ArgumentOutOfRangeException(
                        string.Format("Unable to parse Publisher Type value '{0}' as a valid PublisherType.",
                            publisherTypeString));
                }
                publisherTypeVal |= (int) publisherType;
            }
            return publisherTypeVal;
        }

        /// <summary>
        /// Parses a string into a GeoRedirectLocationsList.  Expects the list to be in the form of:
        /// name,ratio|name,ratio|name,ratio  
        /// where name=another geoTenant, and ratio is a double.
        /// </summary>
        public static GeoRedirectLocationsList ParseGeoRedirectLocationsList(string input)
        {
            GeoRedirectLocationsList list = new GeoRedirectLocationsList();
            if (string.IsNullOrWhiteSpace(input)) return list;

            IEnumerable<string> redirectInputs = SplitAndTrim(input, '|');
            foreach (string redirectInput in redirectInputs)
            {
                string[] pair = SplitAndTrim(redirectInput, ',').ToArray();
                if (pair.Length == 0) continue;
                if(pair.Length != 2) throw new ArgumentException(string.Format("Invalid GeoRedirectLocation \'{0}\'.  Expecting name,ratio.", redirectInput));
                double ratio;
                if (!double.TryParse(pair[1], out ratio) || ratio < 0.0 || ratio > 1.0)
                {
                    throw new ArgumentException(string.Format("Failed to parse GeoRedirectLocation's Ratio value: \'{0}\' (must be a floating-point number between 0.0 and 1.0 inclusive.)", pair[1]));
                }
                list.Add(new GeoRedirectLocation { Name = pair[0], Ratio = ratio});
            }
            return list;
        }

        /// <summary>
        /// Parses a string into an IPRangesList.  Expects the list to be in the form of:
        /// ipRange|ipRange|ipRange where "ipRange" is either a single valid IP address, or two IP addresses
        /// separated by '-'.
        /// </summary>
        public static IPRangesList ParseIPRangesList(string input)
        {
            IPRangesList list = new IPRangesList();
            if (string.IsNullOrWhiteSpace(input)) return list;
            IEnumerable<string> rangeList = SplitAndTrim(input, '|');

            foreach(string range in rangeList)
            {
                string[] ips = SplitAndTrim(range, '-').ToArray();
                if (ips.Length > 2)
                {
                    throw new ArgumentException(string.Format("Invalid IPRange: \'{0}\'.  Found {1} values, expecting 1 or 2.", range, ips.Length));
                }

                if (ips.Length == 0) continue;
                if (ips.Length == 1)
                {
                    if (!IsValidAddress(ips[0]))
                    {
                        throw new ArgumentException(string.Format("Invalid IP Address: \'{0}\'", ips[0]), "input");
                    }
                    list.Add(new IPRange {Lower = ips[0], Upper = ips[0]});
                }
                else
                {
                    if (!IsValidAddress(ips[0]))
                    {
                        throw new ArgumentException(string.Format("Invalid IP Address: \'{0}\'", ips[0]), "input");
                    }
                    if (!IsValidAddress(ips[1]))
                    {
                        throw new ArgumentException(string.Format("Invalid IP Address: \'{0}\'", ips[1]), "input");
                    }
                    list.Add(new IPRange {Lower = ips[0], Upper = ips[1]});
                }
            }

            return list;
        }
        public static bool IsValidAddress(string ipAddress)
        {
            IPAddress ignored;
            return IPAddress.TryParse(ipAddress, out ignored);
        }

        public static IEnumerable<string> SplitAndTrim(string input, params char[] splitChars)
        {
            return input.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s));
        }

        #endregion GeoTenantOperations

        #region Operation Tracking Helpers

        private static string GetRdfeTrackingId()
        {
            return WebOperationContext.Current.IncomingResponse.Headers["operation-tracking-id"];
        }

        /// <summary>
        /// Given an RDFE Tracking ID, will wait for the RDFE operation, and return true on operation success, false otherwise.
        /// </summary>
        public bool WaitForRdfeTracking(IAdministration admin, Func<OperationContextScope> contextFunc, string trackingId, out string errorDetail)
        {
            AdminOperationTracking tracking = null;
            OperationState state = OperationState.Started;
            while(state != OperationState.Succeeded && state != OperationState.Failed)
            {
                // Wait before checking tracking status.
                System.Threading.Thread.Sleep(1000);
                using (contextFunc())
                {
                    tracking = admin.GetAdminResult(trackingId);
                }
                state = GetOperationState(tracking.OperationStatus);
            }

            if (state == OperationState.Failed && tracking != null)
            {
                errorDetail = tracking.ErrorDetail.ToString();
            }
            else
            {
                errorDetail = null;
            }
            return state == OperationState.Succeeded;
        }

        private static OperationState GetOperationState(string state)
        {
            return (OperationState)Enum.Parse(typeof(OperationState), state);
        }

        #endregion Operation Tracking Helpers

        #region Formatting Helpers

        private string FormatObjectAsXml<T>(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj", "Object to be formatted is null!");
            }

            DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
            StringBuilder outputString = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings {Indent = true};
            using (XmlWriter writer = XmlWriter.Create(outputString, settings))
            {
                serializer.WriteObject(writer, obj);
            }

            return outputString.ToString();
        }

        /// <summary>
        /// Formats the missing entity.
        /// </summary>
        private void FormatMissingEntity(StringAndFormattingUtilities.ResponseFormatter formatter, MissingEntity entity)
        {
            this.FormatEntity(formatter, entity);            
        }

        /// <summary>
        /// Formats the modified entity.
        /// </summary>
        private void FormatModifiedEntity(StringAndFormattingUtilities.ResponseFormatter formatter, ModifiedEntity entity)
        {            
            this.FormatEntity(formatter, entity);
            formatter.FormatList(entity.ModifiedKeys, (f, e) =>
            {
                f.AppendProperty("Key", e.Key);
                IEnumerable<string> valuesList = e.Values.Select(v => string.Format("[Value={0} Sources=({1}) LastUpdated={2}]", v.Value, string.Join(", ", v.Sources), v.LastUpdated));

                f.AppendProperty("Values", string.Join(", ", valuesList));
            });
        }

        /// <summary>
        /// Formats the extra entity.
        /// </summary>
        /// <param name="formatter">The formatter.</param>
        /// <param name="entity">The entity.</param>
        private void FormatExtraEntity(StringAndFormattingUtilities.ResponseFormatter formatter, ExtraEntity entity)
        {            
            this.FormatEntity(formatter, entity);
        }

        /// <summary>
        /// Formats the entity.
        /// </summary>
        /// <param name="formatter">The formatter.</param>
        /// <param name="entity">The entity.</param>
        private void FormatEntity(StringAndFormattingUtilities.ResponseFormatter formatter, Inconsistency entity)
        {
            formatter.AppendProperty("PartitionKey", entity.PartitionKey);
            formatter.AppendProperty("RowKey", entity.RowKey);
            formatter.AppendProperty("Category", entity.Category);
        }

        /// <summary>
        /// Deserializes the file.
        /// </summary>
        private T DeserializeFile<T>(string contents)
            where T : class
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            {
                return serializer.ReadObject(memoryStream) as T;
            }
        }

        /// <summary>
        /// Serializes the file.
        /// </summary>
        public string SerializeFile<T>(T obj)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            using (MemoryStream memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, obj);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(memoryStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        #endregion Formatting Helpers

        /// <summary>
        /// Fill the UserAgent field in the outgoing http request with "ACIS extension". 
        /// </summary>
        private void InsertUserAgentToOutgoingRequest()
        {
            WebOperationContext.Current.OutgoingRequest.Headers[HttpRequestHeader.UserAgent] =
                             "ACIS extension";
        }

        /// <summary>
        /// Executes the ExecuteProvisioning operation.
        /// </summary>
        internal IAcisSMEOperationResponse ExecuteProvisioningOperation(Func<IAzureProvisioningAgent, Func<OperationContextScope>, string> action, Func<string, string> errorMessage)
        {
            string ignored;
            return this.ExecuteProvisioningOperation<string, IAcisSMEOperationResponse>(
                action,
                errorMessage,
                AcisSMEOperationResponseExtensions.StandardSuccessResponse,
                AcisSMEOperationResponseExtensions.SpecificErrorResponse,
                out ignored);
        }
        
        /// <summary>
        /// Executes the ExecuteProvisioning operation.
        /// </summary>
        internal TRet ExecuteProvisioningOperation<TInitial, TRet>(Func<IAzureProvisioningAgent, Func<OperationContextScope>, TInitial> action, Func<string, string> errorMessage, Func<TInitial, TRet> conversionFunction, Func<string, TRet> errorConversionFunction, out string errorMessageValue, bool useDsts = true)
            where TInitial : class
            where TRet : class
        {
            errorMessageValue = null;
            
            try
            {
                using (ProvisioningAgentChannelFactory factory = new ProvisioningAgentChannelFactory(this.Endpoint.Name + PaEndPoint))
                {
                    bool isOpen = false;
                    IAzureProvisioningAgent provisioningAgentChannel = null;
                    try
                    {
                        factory.Credentials.ClientCertificate.Certificate = this.Endpoint.Certificate;

                        this.ConfigureFactoryBinding(factory);
                        provisioningAgentChannel = factory.CreateChannel();
                        (provisioningAgentChannel as IClientChannel).Open();

                        isOpen = true;
                        return conversionFunction(action(provisioningAgentChannel, () => new OperationContextScope(provisioningAgentChannel as IClientChannel)));
                    }
                    finally
                    {
                        if (isOpen)
                        {
                            try
                            {
                                (provisioningAgentChannel as IClientChannel).Close();
                            }
                            catch (Exception e)
                            {
                                this.Logger.LogError("SME Non-fatal cleanup exception when closing ProvisioningAgent channel: " + e);
                            }
                        }
                    }
                }
            }
            catch (SmeRdfeOperationStepException sosEx)
            {
                // Thrown when a portion of a SME operation fails but didn't throw an exception.
                // We do not want to do a dSTS / Cert auth retry in this case.
                return errorConversionFunction(errorMessage(sosEx.ToString()));
            }
            catch (Exception e)
            {
                if (this.ExceptionIsRdfeMessage(e))
                {
                    errorMessageValue = RdfeExceptionToUserMessage(e);
                    return errorConversionFunction(errorMessageValue);
                }
                else
                {
                    errorMessageValue = errorMessage(e.ToString());
                    return errorConversionFunction(errorMessageValue);
                }
            }
        }
        
        internal IAcisSMEOperationResponse RectifyResponseWithApprovers(IAcisSMEOperationResponse response)
        {
            // operation result conversion is unnecessary if the operation isn't high risk or the result was unsuccessful
            if (this.CallingOperation == null || !this.CallingOperation.IsHighRiskOperation || response == null || response.SuccessResult != SuccessResult.Success || response.Result == null || response.Result is EmailInclusiveResult)
            {
                return response;
            }

            string subject = string.Format("High Risk Operation Executed: {0}", this.CallingOperation.OperationName);

            // construct a message consisting simply of the operation, who executed it where, and a table of the parameters
            StringBuilder body = new StringBuilder();
            body.AppendFormat("<br /><b>A high risk operation {0} was executed against endpoint: {1} by user: {2}</b><br /><table><tr><th>Parameter</th><th>Value</th></tr>", this.CallingOperation.OperationName, this.Endpoint.Name, this.Context.CurrentUser.Name);

            string approvers = null;

            // the table makes use of the operation's declaration in order to get the parameter's name as displayed in the UI
            foreach (KeyValuePair<string, string> keyAndValue in this.Context.RawOperationParameters)
            {
                string paramName;
                if (AcisWellKnownParameters.IsWellKnownParameter(keyAndValue.Key))
                {
                    IAcisSMEParameter param = AcisWellKnownParameters.GetAllParameters().FirstOrDefault(p => p.Key.Equals(keyAndValue.Key, StringComparison.OrdinalIgnoreCase));
                    if (param == null)
                    {
                        continue;
                    }

                    paramName = param.Name;

                    if (keyAndValue.Key == "wellknownapprover")
                    {
                        approvers = keyAndValue.Value;
                    }
                }
                else
                {
                    IAcisSMEParameter param;
                    this.Endpoint.ContainingExtension.Parameters.TryGetValue(keyAndValue.Key, out param);

                    if (param == null)
                    {
                        continue;
                    }

                    paramName = param.Name;
                }

                body.AppendFormat("<tr><td>{0}</td><td>{1}</td></tr>", paramName, keyAndValue.Value);
            }

            body.Append("</table>");

            List<string> recipients = new List<string>();
            if (approvers != null)
            {
                IEnumerable<string> approverArray = approvers.Split(new[] {',', ';', ' '}, StringSplitOptions.RemoveEmptyEntries);
                foreach (string approver in approverArray)
                {
                    string updatedEmail;
                    if (this.FixEmailAddress(approver, out updatedEmail))
                    {
                        recipients.Add(updatedEmail);
                    }
                }                
            }            

            List<string> ccList = new List<string>();
            if (!string.IsNullOrWhiteSpace(this.Config.HighRiskOperationNotificationAlias))
            {
                string notificationEmail = this.Config.HighRiskOperationNotificationAlias;
                if (this.FixEmailAddress(notificationEmail, out notificationEmail))
                {
                    ccList.Add(notificationEmail);
                }
            }

            AcisUserClaim thisUserEmail = this.Context.CurrentUser.Claims.FirstOrDefault(claim => claim.Type == AcisClaimTypes.Email);
            string userEmailValue;
            if (thisUserEmail != null && this.FixEmailAddress(thisUserEmail.Name, out userEmailValue))
            {
                ccList.Add(userEmailValue);
            }

            EmailInclusiveResult emailResult = new EmailInclusiveResult(response.Result, subject, body.ToString(), recipients, ccList);

            response.Result = emailResult;
            return response;
        }

        private bool FixEmailAddress(string emailAddress, out string correctedEmailAddress)
        {
            emailAddress = emailAddress.Trim();
            if (!EmailValidator.IsValidEmail(emailAddress))
            {
                emailAddress = emailAddress + "@microsoft.com";
                if (!EmailValidator.IsValidEmail(emailAddress))
                {
                    correctedEmailAddress = null;
                    return false;
                }
            }

            correctedEmailAddress = emailAddress;
            return true;
        }

        /// <summary>
        /// Executes the administration operation.
        /// </summary>
        internal IAcisSMEOperationResponse ExecuteAdministrationOperation(Func<IAdministration, Func<OperationContextScope>, string> action, Func<string, string> errorMessage)
        {
            string ignored;
            return this.RectifyResponseWithApprovers(this.ExecuteAdministrationOperation<string, IAcisSMEOperationResponse>(
                action,
                errorMessage,
                AcisSMEOperationResponseExtensions.StandardSuccessResponse,
                AcisSMEOperationResponseExtensions.SpecificErrorResponse,
                out ignored));
        }

        /// <summary>
        /// Executes the administration operation.
        /// </summary>
        internal TRet ExecuteAdministrationOperation<TRet>(Func<IAdministration, Func<OperationContextScope>, TRet> action, Func<string, string> errorMessage, out string errorMessageValue)
            where TRet : class
        {
            return this.ExecuteAdministrationOperation<TRet, TRet>(
                                                               action,
                errorMessage,
                v => v, // source and target of conversion is the same type
                v => null, // using non-standard types means one has to parse errorMessageValue if they want to know why it gave back null.
                out errorMessageValue);
        }

        /// <summary>
        /// Executes the administration operation.
        /// </summary>
        internal TRet ExecuteAdministrationOperation<TInitial, TRet>(Func<IAdministration, Func<OperationContextScope>, TInitial> action, Func<string, string> errorMessage, Func<TInitial, TRet> conversionFunction, Func<string, TRet> errorConversionFunction, out string errorMessageValue)
            where TInitial : class
            where TRet : class
        {
            errorMessageValue = null;
            
            try
            {
                using (AdministrationChannelFactory factory = new AdministrationChannelFactory(this.Binding, this.Config.AdministrationUri))
                {
                    bool isOpen = false;
                    IAdministration administration = null;
                    try
                    {
                        factory.Credentials.ClientCertificate.Certificate = this.Endpoint.Certificate;

                        this.ConfigureFactoryBinding(factory);

                        administration = factory.CreateChannel();
                        administration.ToClientChannel().Open();
                        isOpen = true;
                        return conversionFunction(action(administration, () => new OperationContextScope(administration.ToContextChannel())));
                    }
                    finally
                    {
                        if (isOpen)
                        {
                            try
                            {
                                administration.ToClientChannel().Close();
                            }
                            catch (Exception e)
                            {
                                // don't allow issues with closing the channel to cause a potential actual exception from the call itself to be shown.
                                this.Logger.LogError("SME Non-fatal cleanup exception when closing channel: " + e);
                            }
                        }
                    }
                }
            }
            catch (SmeRdfeOperationStepException sosEx)
            {
                // Thrown when a portion of a SME operation fails but didn't throw an exception.
                // We do not want to do a dSTS / Cert auth retry in this case.
                return errorConversionFunction(errorMessage(sosEx.ToString()));
            }
            catch (Exception e)
            {
                if (this.ExceptionIsRdfeMessage(e))
                {
                    errorMessageValue = RdfeExceptionToUserMessage(e);
                    return errorConversionFunction(errorMessageValue);
                }
                else
                {
                    errorMessageValue = errorMessage(e.ToString());
                    return errorConversionFunction(errorMessageValue);
                }
            }
        }

        /// <summary>
        /// Executes the management operation.
        /// </summary>
        internal IAcisSMEOperationResponse ExecuteManagementOperation(Func<IServiceManagement, Func<OperationContextScope>, string> action, Func<string, string> errorMessage)
        {
            string ignored;
            return this.RectifyResponseWithApprovers(this.ExecuteManagementOperation<string, IAcisSMEOperationResponse>(
                action,
                errorMessage,
                AcisSMEOperationResponseExtensions.StandardSuccessResponse,
                AcisSMEOperationResponseExtensions.SpecificErrorResponse,
                out ignored));
        }

        /// <summary>
        /// Executes the management operation.
        /// </summary>
        internal TRet ExecuteManagementOperation<TRet>(Func<IServiceManagement, Func<OperationContextScope>, TRet> action, Func<string, string> errorMessage, out string errorMessageValue)
            where TRet : class
        {
            return this.ExecuteManagementOperation<TRet, TRet>(
                action, 
                errorMessage, 
                v => v, // source and target of conversion is the same type
                v => null, // using non-standard types means one has to parse errorMessageValue if they want to know why it gave back null.
                out errorMessageValue);
        }

        private WebHttpBinding Binding
        {
            get
            {
                WebHttpBinding binding = new WebHttpBinding
                {
                    CloseTimeout = TimeSpan.FromMinutes(3),
                    OpenTimeout = TimeSpan.FromMinutes(3),
                    ReceiveTimeout = TimeSpan.FromHours(3),
                    SendTimeout = TimeSpan.FromHours(3),
                    MaxBufferSize = 2147483647,
                    MaxBufferPoolSize = 2147483647,
                    MaxReceivedMessageSize = 2147483647,                    
                };

                binding.ReaderQuotas.MaxStringContentLength = 1048576;
                binding.ReaderQuotas.MaxBytesPerRead = 131072;
                binding.Security.Mode = WebHttpSecurityMode.Transport;
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;

                return binding;
            }
        }
        
        /// <summary>
        /// Executes the administration operation.
        /// </summary>
        internal TRet ExecuteManagementOperation<TInitial, TRet>(Func<IServiceManagement, Func<OperationContextScope>, TInitial> action, Func<string, string> errorMessage, Func<TInitial, TRet> conversionFunction, Func<string, TRet> errorConversionFunction, out string errorMessageValue)
            where TInitial : class 
            where TRet : class
        {
            errorMessageValue = null;
            
            try
            {
                using (ServiceManagementChannelFactory factory = new ServiceManagementChannelFactory(this.Binding, this.Config.ManagementUri))
                {
                    bool isOpen = false;
                    IServiceManagement administration = null;
                    try
                    {
                        factory.Credentials.ClientCertificate.Certificate = this.Endpoint.Certificate;
                        
                        this.ConfigureFactoryBinding(factory);

                        administration = factory.CreateChannel();
                        administration.ToClientChannel().Open();
                        isOpen = true;
                        return conversionFunction(action(administration, () => new OperationContextScope(administration.ToContextChannel())));
                    }
                    finally
                    {
                        if (isOpen)
                        {
                            try
                            {
                                administration.ToClientChannel().Close();
                            }
                            catch (Exception e)
                            {
                                // don't allow issues with closing the channel to cause a potential actual exception from the call itself to be shown.
                                this.Logger.LogError("SME Non-fatal cleanup exception when closing channel: " + e);
                            }
                        }
                    }
                }
            }
            catch (SmeRdfeOperationStepException sosEx)
            {
                // Thrown when a portion of a SME operation fails but didn't throw an exception.
                // We do not want to do a dSTS / Cert auth retry in this case.
                return errorConversionFunction(errorMessage(sosEx.ToString()));
            }
            catch (Exception e)
            {
                if (this.ExceptionIsRdfeMessage(e))
                {
                    errorMessageValue = RdfeExceptionToUserMessage(e);
                    return errorConversionFunction(errorMessageValue);
                }
                else
                {
                    errorMessageValue = errorMessage(e.ToString());
                    return errorConversionFunction(errorMessageValue);
                }
            }
        }

        /// <summary>
        /// Configures the factory binding.
        /// </summary>
        private void ConfigureFactoryBinding(ChannelFactory factory)
        {
            ITimeboundOperation timeboundOperation = this.CallingOperation as ITimeboundOperation;
            if (timeboundOperation != null)
            {
                factory.Endpoint.Binding.ReceiveTimeout = timeboundOperation.AllowedTimeout;
                factory.Endpoint.Binding.SendTimeout = timeboundOperation.AllowedTimeout;
            }
        }

        /// <summary>
        /// Determines if the exception is a message from RDFE about failure
        /// </summary>
        private bool ExceptionIsRdfeMessage(Exception e)
        {
            string message = e.Message;
            if (message.Contains("Details:") && message.Contains("Code:") && message.Contains("HostName:"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Turns the RDFE exception into a message for the users
        /// </summary>
        private static string RdfeExceptionToUserMessage(Exception e)
        {
            string message = e.Message;
            if (e is WebProtocolException)
            {
                int trimFrom = message.IndexOf("Exception Details:");
                if (trimFrom > 0)
                {
                    message = message.Substring(0, trimFrom);
                }
            }

            return message;
        }
    }

    public static class SmeRdfeExtensions
    {
        public static IContextChannel CastToContextChannel<T>(this T client)
        {
            return (IContextChannel)client;
        }

        public static IClientChannel CastToClientChannel<T>(this T client)
        {
            return (IClientChannel)client;
        }
    }
}
