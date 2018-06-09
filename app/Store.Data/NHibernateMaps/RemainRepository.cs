using Store.Core;
using Store.Core.RepositoryInterfaces;
using System.Collections.Generic;
using System;
using Store.Core.Account;

namespace Store.Data
{
    public class RemainRepository : CriteriaRepository<Remaind>
    {
        private IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
        private string format = "dd.MM.yyyy";

        public int TruncRamains(int StorageId, String date, int? NomenclatureId)
        {
            if (NomenclatureId == null)
            {
                return Session.Delete("from Remaind r where r.StorageName.Id = :storId and RemaindDate=:remDate", new Object[2] { StorageId, DateTime.ParseExact(date, format, culture) }, new NHibernate.Type.IType[2] { NHibernate.NHibernateUtil.Int32, NHibernate.NHibernateUtil.Date });
            }
            else
            {
                return Session.Delete("from Remaind r where r.StorageName.Id = :storId and RemaindDate=:remDate and Nomenclature.Id = :NomId", new Object[3] { StorageId, DateTime.ParseExact(date, format, culture), NomenclatureId }, new NHibernate.Type.IType[3] { NHibernate.NHibernateUtil.Int32, NHibernate.NHibernateUtil.Date, NHibernate.NHibernateUtil.Int32 });
            }
        }

        public IList<RemaindEx> GetExternalRemains(int storageNameId, DateTime date, int? NomenclatureId)
        {
            if (NomenclatureId == null)
            {
                string sql = "select t.*,t.id RemaindId, (select nvl(sum (e.quantity),0) from remaindexternals e where e.remainddate=t.remainddate and e.storagenameid=t.storagenameid and e.nomenclatureid=t.nomenclatureid and e.wear=t.wear) quantityE from remainds t where t.remainddate=? and t.storagenameid=?";
                return Session.CreateSQLQuery(sql)
                    .AddEntity(typeof(RemaindEx))
                    .SetDateTime(0, date)
                    .SetInt32(1, storageNameId)
                    .List<RemaindEx>();
            }
            else
            {
                string sql = "select t.*,t.id RemaindId, (select nvl(sum (e.quantity),0) from remaindexternals e where e.remainddate=t.remainddate and e.storagenameid=t.storagenameid and e.nomenclatureid=t.nomenclatureid and e.wear=t.wear) quantityE from remainds t where t.remainddate=? and t.storagenameid=? and t.nomenclatureid=?";
                return Session.CreateSQLQuery(sql)
                    .AddEntity(typeof(RemaindEx))
                    .SetDateTime(0, date)
                    .SetInt32(1, storageNameId)
                    .SetInt32(2, (int)NomenclatureId)
                    .List<RemaindEx>();
            }
        }

        public DateTime GetMaxRemainDate(int storageNameId)
        {
            DateTime dt = DateTime.MinValue;
            try
            {
                string sql = "select nvl(a.dt, sysdate) from (select max(remainddate) dt from remainds t where storagenameid=" + storageNameId + ")a";
                dt = (DateTime)Session.CreateSQLQuery(sql).UniqueResult();
            }
            catch { }
            return dt;
        }

        public DateTime GetActialRemainDate(int storageNameId, DateTime remainDate)
        {
            DateTime dt = DateTime.MinValue;
            try
            {
                string sql = "select max(ActualDate) dt from remainds t where t.storagenameid=" + storageNameId + "and t.RemaindDate=to_date('" + remainDate.ToString(DataGlobals.DATE_FORMAT_FULL_YEAR) + "','dd.mm.yyyy')";
                dt = (DateTime)Session.CreateSQLQuery(sql).UniqueResult();
            }
            catch { }
            return dt;
        }

    }
}
