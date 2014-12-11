using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace jpd.ms.TenantTableProvider
{
    public class Tenant : IssuerTableEntity
    {
    }

    public class SignupToken : IssuerTableEntity
    {
        public DateTimeOffset ExpirationDate { get; set; }
    }

    public class IssuingAuthorityKey : IssuerTableEntity
    {
    }

    public class IssuerTableEntity : TableEntity
    {
        public string Id { get { return RowKey; } set { RowKey = value; } }

        public IssuerTableEntity()
        {
            PartitionKey = GetType().Name;
        }
    }
}
