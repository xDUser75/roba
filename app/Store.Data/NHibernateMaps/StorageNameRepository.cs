using Store.Core;
using Store.Core.RepositoryInterfaces;
using System.Collections.Generic;
using System;

namespace Store.Data
{
    public class StorageNameRepository : CriteriaRepository<StorageName>
    {

        public IList<StorageName> GetStorageShops(String orgId)
        {
            if (orgId == null) orgId = "-1";
            IList<StorageName> storage = Session.CreateQuery("from StorageName S where S.Organization.Id =" + orgId + " and OrgTreeId=" + orgId + "  and isActive=1  Order by Name").List<StorageName>();
            return storage;
        }
        public IList<StorageName> GetShops(String orgId)
        {
            if (orgId == null) orgId = "-1";
//            IList<StorageName> storage = Session.CreateQuery("from StorageName S where S.Organization.Id =" + orgId + " and S.Organization.Id != OrgTreeId Order by Name").List<StorageName>();
            IList<StorageName> storage = Session.CreateQuery("from StorageName S where S.Organization.Id =" + orgId + " and S.IsSalon !=1  and isActive=1  Order by Id").List<StorageName>();
            return storage;
        }
        public IList<StorageName> GetAllShops(String orgId, String carrentStorageNameId)
        {
            if (orgId == null) orgId = "-1";
            //            IList<StorageName> storage = Session.CreateQuery("from StorageName S where S.Organization.Id =" + orgId + " and S.Organization.Id != OrgTreeId Order by Name").List<StorageName>();
            IList<StorageName> storage = Session.CreateQuery("from StorageName S where S.Organization.Id =" + orgId + "  and isActive=1 and id!=" + carrentStorageNameId + "  Order by Id").List<StorageName>();
            return storage;
        }

        public IList<StorageName> GetSenders()
        {
            IList<StorageName> storage = Session.CreateQuery("from StorageName S where OrgTreeId = 1 and isActive=1 Order by Name").List<StorageName>();
            return storage;
        }

    }
}
