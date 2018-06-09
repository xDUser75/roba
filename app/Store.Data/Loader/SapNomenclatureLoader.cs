using System;
using System.Collections.Generic;
using Oracle.DataAccess.Client;
using System.Data;
using Store.Core.External.Interfaсe;
using System.Globalization;
using SAP.Middleware.Connector;
using Store.Core;
using System.Xml;
using System.IO;
using System.Configuration;


namespace Store.Data.Loader
{
    public class ER_SAP_Config : IDestinationConfiguration
    {
        RfcDestinationManager.ConfigurationChangeHandler changeHandler;

        public RfcConfigParameters GetParameters(String destinationName)
        {
            RfcConfigParameters param = new RfcConfigParameters();
            XmlDocument xmlDoc = new XmlDocument();
            // Загружаем XML-документ из файла
            var physicalFilePath = Path.Combine(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, "Web.config");
            xmlDoc.Load(physicalFilePath);
            XmlNode findNode = xmlDoc.SelectSingleNode("/configuration/SAP.Middleware.Connector/ClientSettings/DestinationConfiguration/destinations/add[@NAME='" + destinationName + "']");
            if (findNode == null) return null;
            foreach (XmlAttribute item in findNode.Attributes)
            {
                param.Add(item.Name, item.Value);
            }
            return param;
        }

        public bool ChangeEventsSupported()
        {
            return false;
        }
        public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged
        {
            add
            {
                changeHandler = value;
            }
            remove
            {
                //do nothing
            }
        }
    }

    public class SapNomenclatureLoader : ExternalLoader, IExternalLoaderNomenclature
    {

        public string LoadNomenclature(string organizationId, string sessionId)
        {
            string error = "";
            List<AM_SAP> amSaps = new List<AM_SAP>();
            var conn = getSqlConnection();
            if (conn == null) {
                conn = createSQLConnection();
                setSqlConnection(conn);
            }
            if (conn.State != ConnectionState.Open) conn.Open();
                try
                {

                    Dictionary<string, string> openWith = new Dictionary<string, string>();
                    string IM_WERKS = getOrganizationBurks(int.Parse(organizationId));
                    openWith.Add("IM_WERKS", IM_WERKS);
                    //openWith.Add("IM_MATNR", "2100657");

                    try
                    {
                        RfcDestinationManager.RegisterDestinationConfiguration(new ER_SAP_Config());
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message);
                    }
                    String NOMENCLATURE_RFC = ConfigurationManager.AppSettings["SAP_ERP_" + organizationId];
                    RfcDestination destination = RfcDestinationManager.GetDestination(NOMENCLATURE_RFC);
                    IRfcFunction function = null;


                    try
                    {
                        function = destination.Repository.CreateFunction("ZM_RFC_MATNR_OVERALL");
                        foreach (KeyValuePair<string, string> kvp in openWith)
                        {
                            function.SetValue(kvp.Key, kvp.Value);
                        }
                        //function.SetValue("IM_MATNR", "2100657");
                        function.Invoke(destination);
                    }
                    catch (RfcBaseException e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.ToString());
                    }

                    if (function != null)
                    {
                        //Get the function table parameter COMPANYCODE_LIST
                        IRfcTable codeses = function.GetTable("PIT_OBJECTS");
                        if (codeses.RowCount == 0)
                        {
                            System.Diagnostics.Debug.WriteLine("Данные не выбраны");
                        }

                    else 
                    {
                        using (var transaction = conn.BeginTransaction())
                        {
                            OracleCommand cmd = new OracleCommand();
                            cmd.Connection = conn;
                            foreach (var codes in codeses)
                            {
                                cmd.CommandText = "Insert into NOMENCLATURE_SAPS (organizationid,  sessionid,  matnr,  maktx,  extwg,  ewbez,  zzvngm,  meins,  msehl3,  "+
                                    "msehl,  lv_m,  bz_m,  mtstb,  ersda,  laeda,  lw_w,"+
                                    "bz_w,  class, kschl,  atnam, ATKLA, atklt,  atbez,  val,  okei_code,  okei_name) " +
                                    "VALUES (" + organizationId +","
                                    + "'"+ sessionId + "',"
                                    + "'" + codes.GetString("MATNR") + "',"
                                    + "'" + codes.GetString("MAKTX") + "',"
                                    + "'" + codes.GetString("EXTWG") + "',"
                                    + "'" + codes.GetString("EWBEZ") + "',"
                                    + "'" + codes.GetString("ZZVNGM") + "',"
                                    + "'" + codes.GetString("MEINS") + "',"
                                    + "'" + codes.GetString("MSEHL3") + "',"
                                    + "'" + codes.GetString("MSEHL") + "',"
                                    + "'" + codes.GetString("LV_M") + "',"
                                    + "'" + codes.GetString("BZ_M") + "',"
                                    + "'" + codes.GetString("MTSTB") + "',"
                                    +"to_date('"+ codes.GetString("ERSDA") + "','yyyy-mm-dd' ),"
                                    + "to_date('" + codes.GetString("LAEDA") + "','yyyy-mm-dd' ),"
                                    + "'" + codes.GetString("LW_W") + "',"
                                    + "'" + codes.GetString("BZ_W") + "',"
                                    + "'" + codes.GetString("CLASS") + "',"
                                    + "'" + codes.GetString("KLSCHL") + "',"
                                    + "'" + codes.GetString("ATNAM") + "',"
                                    + "'" + codes.GetString("ATKLA") + "',"
                                    + "'" + codes.GetString("ATKLT") + "',"
                                    + "'" + codes.GetString("ATBEZ") + "',"
                                    + "'" + codes.GetString("VAL") + "',"
                                    + "'" + codes.GetString("OKEI_CODE") + "',"
                                    + "'" + codes.GetString("OKEI_NAME") 
                                    + "')";
                                cmd.ExecuteNonQuery();
                            }
                            transaction.Commit();
                            // Запускаем процедуру разбора данных RFC в структуру данных АС
                            try
                            {
                                cmd = new OracleCommand("PCK_STORE.nomenclature_rfc_SAP", conn);
                                cmd.CommandType = CommandType.StoredProcedure;

                                OracleParameter inval = new OracleParameter("organization_Id", OracleDbType.Int32);
                                inval.Direction = ParameterDirection.Input;
                                inval.Value = int.Parse(organizationId);
                                cmd.Parameters.Add(inval);

                                inval = new OracleParameter("session_id", OracleDbType.Varchar2);
                                inval.Direction = ParameterDirection.Input;
                                inval.Value = sessionId;
                                cmd.Parameters.Add(inval);

                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                error = error + e.Message;
                                System.Diagnostics.Debug.WriteLine(error);
                            }
                        }
                    }
                }
                }
                catch (Exception e)
                {
                    error = e.Message;
                    System.Diagnostics.Debug.WriteLine(error);
                }
                finally
                {
                    if (conn.State != ConnectionState.Closed)
                        conn.Close();
                }
            return error;
        }

        private string getOrganizationBurks(int orgId)
        {
            string burks = "";
            OracleCommand cmd = new OracleCommand("Select bukrs from Organizations where id=" + orgId);
            cmd.Connection = getSqlConnection();
            cmd.CommandType = CommandType.Text;
            try
            {
                OracleDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    burks = Convert.ToString(reader["BUKRS"]);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            finally
            {
                cmd.Dispose();
            }
            return burks;
        }



    }


}
