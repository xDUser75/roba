using Store.Core;
using Store.Core.RepositoryInterfaces;
using System.Collections.Generic;
using System;
using Store.Core.Account;

namespace Store.Data
{
    public class StorageRepository : CriteriaRepository<Storage>
    {
        public int TruncStorage(int StorageId)
        {
             return Session.Delete("from Storage s where s.StorageName.Id = :storId", new Object[1] { StorageId }, new NHibernate.Type.IType[1] { NHibernate.NHibernateUtil.Int32 });
        }
    }
}
