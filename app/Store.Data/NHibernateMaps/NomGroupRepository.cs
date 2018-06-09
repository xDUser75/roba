using Store.Core;
using Store.Core.RepositoryInterfaces;
using System.Collections.Generic;
using System;
using Store.Core.Account;
using NHibernate;
using System.Collections;
using NHibernate.Criterion;

namespace Store.Data
{
    public class NomGroupRepository : CriteriaRepository<NomGroup>
    {
        public IList<NomGroup> GetNomGroup(string text, string organizationId)
        {
            string sql ="Select * from NOMGROUPS where organizationId= " + organizationId + " and (lower(externalCode) like lower('%" + text + "%') or lower(name) like lower('%" + text + "%') )";

            return Session.CreateSQLQuery(sql)
                   .AddEntity(typeof(NomGroup))
                   .List<NomGroup>();
        }

    }
}
