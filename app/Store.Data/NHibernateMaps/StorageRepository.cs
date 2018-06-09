using Store.Core;
using Store.Core.RepositoryInterfaces;
using System.Collections.Generic;
using System;
using Store.Core.Account;

namespace Store.Data
{
    public class NomGroupPriceRepository : CriteriaRepository<NomGroupPrice>
    {
        public int TruncNomGroupPrice(int organizationId, int periodProceId)
        {
            return Session.Delete("from NomGroupPrice s where s.OrganizationId = :orgId and PeriodPrice.Id=:priceId", new Object[2] { organizationId, periodProceId }, new NHibernate.Type.IType[2] { NHibernate.NHibernateUtil.Int32, NHibernate.NHibernateUtil.Int32 });
        }
    }
}
