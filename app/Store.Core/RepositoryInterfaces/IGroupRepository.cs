using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Core.PersistenceSupport;
using Store.Core.Account;

namespace Store.Core.RepositoryInterfaces
{
    public interface IGroupRepository : ICriteriaRepository<Group>
    {
        void SaveDebugInfo(string name, string text);
    }
}
