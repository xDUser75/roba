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


namespace Store.Data
{

    public class PLAN_SAPRepository : CriteriaRepository<PLAN_SAP> 
        {
        public void ClearTempData(string date_plan)
        {
            Session.BeginTransaction();
            Session.CreateSQLQuery("begin REP_DATA_OUTPUT.Delete_PlanPurchaseByCeh_SAP('" + date_plan + "'); end; ").ExecuteUpdate();
            Session.Flush();
        }

        public string CreateTempData(int paramOrganization, DateTime dateN, DateTime dateEnd, int paramCeh, HttpResponseBase Response)
        {
            bool isDebug = bool.Parse(ConfigurationManager.AppSettings["isDebug"]);
            string pswd = "spec2011";
            if (isDebug) pswd = "store2011";
            string retVal = "";
            using (OracleConnection objConn = new OracleConnection(Session.Connection.ConnectionString + ";Password="+pswd))
            {
                OracleCommand objCmd = new OracleCommand();
                objCmd.Connection = objConn;
                objCmd.CommandText = "REP_DATA_OUTPUT.Get_PlanPurchaseByCeh_SAP";
                objCmd.CommandType = CommandType.StoredProcedure;
                OracleParameter param = new OracleParameter();
                param.ParameterName = "paramOrganization";
                param.Value = paramOrganization;
                param.Direction = ParameterDirection.Input;
                objCmd.Parameters.Add(param);

                param = new OracleParameter();
                param.ParameterName = "dateN_";
                param.Value = dateN.ToString("yyyyMMdd");
                param.Direction = ParameterDirection.Input;
                objCmd.Parameters.Add(param);

                param = new OracleParameter();
                param.ParameterName = "dateEnd_";
                param.Value = dateEnd.ToString("yyyyMMdd");
                param.Direction = ParameterDirection.Input;
                objCmd.Parameters.Add(param);

                param = new OracleParameter();
                param.ParameterName = "paramCeh";
                param.Value = paramCeh;
                param.Direction = ParameterDirection.Input;
                objCmd.Parameters.Add(param);

                param = new OracleParameter();
                param.ParameterName = "date_plan";
                param.Direction = ParameterDirection.Output;
                param.Size = 30;
                param.OracleDbType = OracleDbType.Varchar2;
                objCmd.Parameters.Add(param);
               
                try
                {
                    objConn.Open();
                    objCmd.ExecuteNonQuery();
                    retVal = objCmd.Parameters["date_plan"].Value.ToString();
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Exception: {0}", ex.ToString());
                }

                if ((retVal.Length > 0) && (retVal != "null"))
                {
                    objCmd = new OracleCommand();
                    objCmd.Connection = objConn;
                    objCmd.CommandText = "REP_DATA_OUTPUT.Get_Plan_SAP_Refcursor";
                    objCmd.CommandType = CommandType.StoredProcedure;
                    param = new OracleParameter();
                    param.ParameterName = "date_plan";
                    param.Value = retVal;
                    param.Direction = ParameterDirection.Input;
                    objCmd.Parameters.Add(param);

                    OracleParameter crs = new OracleParameter();
                    crs.OracleDbType = OracleDbType.RefCursor;
                    crs.Direction = ParameterDirection.Output;
                    crs.ParameterName = "crs";
                    objCmd.Parameters.Add(crs);

                    using (OracleDataReader MyReader = objCmd.ExecuteReader())
                    {
                        Encoding dos = Encoding.GetEncoding(866);
                        Encoding utf8 = Encoding.UTF8;
                        int ColumnCount = MyReader.FieldCount;
                        for (int i = 0; i < ColumnCount ; i++)
                        {
                            Response.Write(MyReader.GetName(i));
                            Response.Write(";");
                        }
                        Response.Write("\n");
                        // get the data and add the row
                        while (MyReader.Read())
                        {
                            for (int i = 0; i < ColumnCount ; i++)
                            {
                                byte[] utfBytes = utf8.GetBytes(MyReader.GetValue(i).ToString());
                                byte[] dosBytes = Encoding.Convert(utf8, dos, utfBytes);
                                Response.Write(dos.GetString(dosBytes));
                                Response.Write(";");
                            }
                            Response.Write("\n");
                        }
                        MyReader.Close();
                    }
                }
                objConn.Close();
            }
            return retVal;

        }
            
        }

}
