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
    public class NomenclatureRepository : CriteriaRepository<Nomenclature>
    {
        public IList<Nomenclature> GetNomenclature(string text, string organizationId)
        {
            string sql ="Select * from NOMENCLATURES where organizationId= " + organizationId + " and (lower(externalCode) like lower('%" + text + "%') or lower(name) like lower('%" + text + "%') )";

            return Session.CreateSQLQuery(sql)
                   .AddEntity(typeof(Nomenclature))
                   .List<Nomenclature>();
        }

    }
}
