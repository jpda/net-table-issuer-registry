using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace jpd.ms.TenantTableProvider
{
    internal static class CloudTableUtility
    {
        private static CloudTableClient _cloudTableClient;

        internal static CloudTable GetTable(string tableName)
        {
            var ctc = _cloudTableClient ?? (_cloudTableClient = GetCloudTableClient());
            var table = ctc.GetTableReference(tableName);
            table.CreateIfNotExists();
            return table;
        }

        internal static CloudTableClient GetCloudTableClient()
        {
            return CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("TableStorageAccount")).CreateCloudTableClient();
        }
    }
}
