using Store.Core;
using Store.Core.RepositoryInterfaces;
using System.Collections.Generic;
using System;
using Store.Core.Account;

namespace Store.Data
{
    public class RemainExternalRepository : CriteriaRepository<RemaindExternal>
    {
        public int TruncRemainExternal(int StorageId, String date)
        {
            IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
            string format = "dd.MM.yyyy";
            return Session.Delete("from RemaindExternal r where r.StorageName.Id = :storId and RemaindDate=:remDate", new Object[2] { StorageId, DateTime.ParseExact(date, format, culture) }, new NHibernate.Type.IType[2] { NHibernate.NHibernateUtil.Int32, NHibernate.NHibernateUtil.Date });
        }
    }
}
