using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Core.PersistenceSupport;
using Store.Core.Account;

namespace Store.Core.RepositoryInterfaces
{
//    public interface IUserRepository : ICriteriaRepository<User>, IRepository<User>
    public interface IUserRepository : ICriteriaRepository<User>
    {
        void SaveDebugInfo(string name, string text);
    }
}
