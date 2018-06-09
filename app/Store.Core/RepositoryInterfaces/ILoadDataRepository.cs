using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Core.PersistenceSupport.NHibernate;
using NHibernate;
using SharpArch.Data.NHibernate;
using SharpArch.Web.NHibernate;

namespace Store.Core.RepositoryInterfaces
{

    public interface ILoadDataRepository : INHibernateRepository<LoadData>
        
    {
        void LoadNomenclatureFromSAP(string p_orgi);
        void LoadNormaContent(string p_shopid, string p_nomgroupList, string p_tabn, string p_normaid, string idOrg);
        void LoadWorkerCardOut(string p_orgid);
        void LoadMatPersonCardOut(string p_orgid, string p_shopid, string p_storagenameid);
        void LoadWorkerCardDismiss(string p_date, string p_orgid);
        String LoadTransfer(string p_organizationid,
                        string p_shopid_old,
                        string p_shopid_new,
                        string p_storagenameid_old,
                        string p_storagenameid_new,
                        string p_tabn,
                        string p_operdate,
                        string p_isstorageactive);

        void AddChangeNomGroup(string p_shopid, string p_nomgroupid, string add_nomgroup, string p_normaid, string idOrg);
    }
}
