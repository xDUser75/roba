using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Store.Core;
using SharpArch.Core.PersistenceSupport.NHibernate;
using Store.Core.RepositoryInterfaces;
using NHibernate;

using SharpArch.Data.NHibernate;
using NHibernate.Criterion;
using SharpArch.Web.NHibernate;
using Oracle.DataAccess.Client;
using System.Data;
using System.Configuration;
using System.Web;
using Store.Core.External.Interfaсe;



namespace Store.Data.NHibernateMaps
{

    public class LoadDataRepository : NHibernateRepository<LoadData>, ILoadDataRepository
        {
        public void LoadNomenclatureFromSAP(string p_orgid)
        {
            Session.BeginTransaction();

            Session.CreateSQLQuery("begin PCK_STORE.nomenclatureUpdateSAP(" + p_orgid + "); end; ").ExecuteUpdate();
            Session.Flush();
        }


        public void LoadWorkerCardOut(string p_orgid)
        {
            Session.BeginTransaction();
            Session.CreateSQLQuery("begin PCK_STORE.workerCardOutByDate(" + p_orgid + "); end; ").ExecuteUpdate();
            Session.Flush();
        }
        public void LoadWorkerCardDismiss(string p_date, string p_orgid)
        {

            Session.BeginTransaction();
            Session.CreateSQLQuery("begin PCK_STORE.workerCardDismiss(to_date('" + p_date + "','dd.mm.yy'), " + p_orgid +  "); end; ").ExecuteUpdate();
            Session.Flush();
        }

        public void LoadMatPersonCardOut(string p_orgid, string p_shopid, string p_storagenameid)
        {
            Session.BeginTransaction();
            Session.CreateSQLQuery("begin PCK_STORE.matpersonCardOut(" + p_orgid + ", " + p_shopid + ", " + p_storagenameid + "); end; ").ExecuteUpdate();
            Session.Flush();
        }
        public void LoadNormaContent(string p_shopid, string p_nomgroupList, string p_tabn, string p_normaid, string idOrg)
        {
            if (p_normaid == null || p_normaid == "")
                p_normaid = "null";
            if (p_tabn==null ||p_tabn == "")
                p_tabn = "null";
            if (p_nomgroupList==null || p_nomgroupList == "")
                p_nomgroupList = "null";
            if (p_shopid==null || p_shopid == "")
                p_shopid = "null";
            Session.BeginTransaction();
            Session.CreateSQLQuery("begin PCK_STORE.load_normacontentid (" + p_tabn + "," + p_nomgroupList + "," + p_normaid + "," + p_shopid + "," + idOrg + "); end; ").ExecuteUpdate();
            Session.Flush();


        }

        public String LoadTransfer(string p_organizationid, 
                                 string p_shopid_old, 
                                 string p_shopid_new, 
                                 string p_storagenameid_old, 
                                 string p_storagenameid_new, 
                                 string p_tabn, 
                                 string p_operdate,
                                 string p_isstorageactive)
        {
            /*
            if (p_tabn == null || p_tabn == "")
                p_tabn = "null";
            */
            //Session.BeginTransaction();

            string error = "";
            var conn = Session.Connection;
            Session.Transaction.Begin();
            String message = "OK";
            if (conn.State != ConnectionState.Open) conn.Open();

               
                try
                {
                    IDbCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "PCK_STORE.transfer";
                    cmd.CommandType = CommandType.StoredProcedure;

                    
                    var outputParam = cmd.CreateParameter();
                    outputParam.ParameterName = "@message";
                    outputParam.DbType = DbType.String;
                    outputParam.Value = "";
                    outputParam.Size = 1000;
                    outputParam.Direction = ParameterDirection.InputOutput;
                    cmd.Parameters.Add(outputParam);
                    /*
                   var outputIdParam = new OracleParameter("message", OracleDbType.Varchar2);
                   outputIdParam.Direction = ParameterDirection.Output;
                   cmd.Parameters.Add(outputIdParam);
                   */
                    var parameter = cmd.CreateParameter();
                    parameter.ParameterName = "p_organizationid";
                    parameter.DbType = DbType.Int32;
                    parameter.Value = int.Parse(p_organizationid);
                    cmd.Parameters.Add(parameter);

                    parameter = cmd.CreateParameter();
                    parameter.ParameterName = "p_storagenameid_old";
                    parameter.DbType = DbType.Int32;
                    parameter.Value = int.Parse(p_storagenameid_old);
                    cmd.Parameters.Add(parameter);

                    parameter = cmd.CreateParameter();
                    parameter.ParameterName = "p_storagenameid_new";
                    parameter.DbType = DbType.Int32;
                    parameter.Value = int.Parse(p_storagenameid_new);
                    cmd.Parameters.Add(parameter);


                    parameter = cmd.CreateParameter();
                    parameter.ParameterName = "p_shopid_old";
                    parameter.DbType = DbType.Int32;
                    if (p_shopid_old == null || p_shopid_old=="")
                        parameter.Value = 0;
                    else
                        parameter.Value = int.Parse(p_shopid_old);
                    
                    cmd.Parameters.Add(parameter);


                    parameter = cmd.CreateParameter();
                    parameter.ParameterName = "p_shopid_new";
                    parameter.DbType = DbType.Int32;
                    if (p_shopid_new == null || p_shopid_new == "")
                        parameter.Value = 0;
                    else
                        parameter.Value = int.Parse(p_shopid_new);
                    cmd.Parameters.Add(parameter);

                    parameter = cmd.CreateParameter();
                    parameter.ParameterName = "p_tabn";
                    parameter.DbType = DbType.Int32;
                    if (p_tabn==null || p_tabn == "")
                        parameter.Value = 0;
                    else
                        parameter.Value = int.Parse(p_tabn);
                    cmd.Parameters.Add(parameter);

                    parameter = cmd.CreateParameter();
                    parameter.ParameterName = "p_isstorageactive";
                    parameter.DbType = DbType.Int32;
                    if (p_isstorageactive == null || p_isstorageactive == "")
                        parameter.Value = 0;
                    else
                        parameter.Value = int.Parse(p_isstorageactive);
                    cmd.Parameters.Add(parameter);

                    parameter = cmd.CreateParameter();
                    parameter.ParameterName = "@p_operdate";
                    parameter.Value = p_operdate;
                    cmd.Parameters.Add(parameter);

                    cmd.ExecuteNonQuery();
                    message = outputParam.Value.ToString();
                    //String message = cmd.Parameters["@message"].Value();
                    Session.Flush();
                    Session.Transaction.Commit();
                    conn.Close();
                }
                catch (Exception e)
                {
                    message = e.Message;
                    System.Diagnostics.Debug.WriteLine(error);
                }
                
           

//            String message = Session.CreateSQLQuery("begin PCK_STORE.transfer(" + p_organizationid + "," + p_storagenameid_old + "," +
//                    p_storagenameid_new + "," + p_shopid_old + "," + p_shopid_new + "," + p_tabn + "," + "to_date('" + p_operdate + "','dd.mm.yy')); end; ").ExecuteUpdate();

/*
            IQuery query =
             Session.CreateSQLQuery("begin PCK_STORE.transfer('1',:p_organizationid , :p_storagenameid_old ,:p_storagenameid_new ,:p_shopid_old ,:p_shopid_new ,:p_tabn , :p_operdate); end; ")
                .SetString("p_organizationid", p_organizationid)
                .SetString("p_storagenameid_old", p_storagenameid_old)
                .SetString("p_storagenameid_new", p_storagenameid_new)
                .SetString("p_shopid_old", p_shopid_old)
                .SetString("p_shopid_new", p_shopid_new)
                .SetString("p_tabn", p_tabn)
                .SetString("p_operdate", p_operdate);

            Dictionary<string, object> hm = new Dictionary<string, object>();
            query.ExecuteUpdate();
          
            Session.Flush();
*/
                return message;
        }


        public void AddChangeNomGroup(string p_shopid, string p_nomgroupid, string add_nomgroup, string p_normaid, string idOrg)
        {
            if (p_normaid == null || p_normaid == "")
                p_normaid = "null";
            if (add_nomgroup == null || add_nomgroup == "")
                add_nomgroup = "null";
            if (p_nomgroupid == null || p_nomgroupid == "")
                p_nomgroupid = "null";
            if (p_shopid == null || p_shopid == "")
                p_shopid = "null";
            Session.BeginTransaction();
            Session.CreateSQLQuery("begin PCK_STORE.add_change_nomgroup (" + idOrg + "," + p_shopid + "," + p_nomgroupid + "," + add_nomgroup + "," + p_normaid + "); end; ").ExecuteUpdate();
            Session.Flush();


        }


    
    }

}
