Operation:RegisterCacheAccount
OperationDisplayName:Register Cache Account
OperationType:RDFERWOperation
OperationGroup:PlatformImageRepositoryManagementOperation
OperationGroupDisplayName:na
ApproverParameterNeeded:Y
ApproverLinkParameterNeeded:Y
HelperFunctionName:RegisterCacheAccount
Parameter:string:cacheAccountName:Cache Account Name:CacheAccountName:AcisSMETextParameter
Parameter:string:cacheAccountCurrentKeyIndex:Cache Account Current Key Index:CacheAccountCurrentKeyIndex:AcisSMETextParameter
Parameter:string:location:Location:Location:AcisSMETextParameter
Parameter:string:cacheAccountStampName:Cache Account Stamp Name:CacheAccountStampName:AcisSMETextParameter
Parameter:string:cacheAccountIsStampActive:Cache Account Is Active:CacheAccountIsStampActive:AcisSMEBooleanParameter
Parameter:string:cacheAccountTypeOfCache:Cache Account Type of Cache:CacheAccountTypeOfCache:AcisSMETextParameter


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