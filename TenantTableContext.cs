using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jpd.ms.TenantTableProvider
{
    public class TenantTableContext
    {
        private readonly CloudTable _table;
        public TenantTableContext()
        {
            _table = CloudTableUtility.GetTable("ValidatingIssuer");
        }

        public IQueryable<Tenant> Tenants
        {
            get
            {
                return _table.CreateQuery<Tenant>().Where(x => x.PartitionKey == "Tenant").AsQueryable();
            }
        }

        public IQueryable<IssuingAuthorityKey> IssuingAuthorityKeys
        {
            get
            {
                return _table.CreateQuery<IssuingAuthorityKey>().Where(x => x.PartitionKey == "IssuingAuthorityKey").AsQueryable();
            }
        }

        public IQueryable<SignupToken> SignupTokens
        {
            get { return _table.CreateQuery<SignupToken>().Where(x => x.PartitionKey == "SignupToken"); }
        }

        public T Add<T>(IssuerTableEntity entity) where T : IssuerTableEntity
        {
            var item = _table.Execute(TableOperation.InsertOrMerge(entity));
            if (item.HttpStatusCode == 200)
            {
                return item.Result as T;
            }
            return null;
        }

        public void Add(IssuerTableEntity entity)
        {
            _table.Execute(TableOperation.InsertOrMerge(entity));
        }

        public void Remove(IssuerTableEntity entity)
        {
            _table.Execute(TableOperation.Delete(entity));
        }

        public void RemoveRange(IEnumerable<IssuerTableEntity> entities)
        {
            var batch = new TableBatchOperation();
            entities.ToList().ForEach(x => batch.Add(TableOperation.Delete(x)));
            if (batch.Count == 0) return;
            _table.ExecuteBatch(batch);
        }
    }
}
