﻿using SharpArch.Data.NHibernate;
using Store.Core.Account;
using Store.Core.RepositoryInterfaces;

namespace Store.Data.AccountRepository
{
    [SessionFactory(DataGlobals.ACCOUNT_DB_FACTORY_KEY)]
    public class UserRepository : CriteriaRepository<User>, IUserRepository
    {
        public void SaveDebugInfo(string name, string text)
        {
            Session.CreateSQLQuery("begin debugInfo('" + name + "','" + text + "'); end; ").ExecuteUpdate();
        }

    }
}
