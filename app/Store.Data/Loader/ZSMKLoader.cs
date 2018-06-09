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

namespace Store.Data.Loader
{
    public class ZSMK_SAP_Config : IDestinationConfiguration
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

    public class ZSMKLoader : ExternalLoader, IExternalLoaderOrganization
    {
        const string MY_RFC = "RFC";
        private List<AM_SAP> getOrganization(Dictionary<string, string> openWith, string organizationId)
        {
            try
            {
                RfcDestinationManager.RegisterDestinationConfiguration(new ZSMK_SAP_Config());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            RfcDestination destination = RfcDestinationManager.GetDestination(MY_RFC);
            IRfcFunction function = null;
            string strDate = DateTime.Today.ToString("yyyyMMdd");

            IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
            string format = "yyyy-MM-dd";
            List<AM_SAP> list = new List<AM_SAP>();

            try
            {
                function = destination.Repository.CreateFunction("ZP_GET_OSP");
                 
                foreach (KeyValuePair<string, string> kvp in openWith)
                {
                    function.SetValue(kvp.Key, kvp.Value);
                }

                function.Invoke(destination);
            }
            catch (RfcBaseException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            if (function != null)
            {
                //Get the function table parameter COMPANYCODE_LIST
                IRfcTable codes = function.GetTable("PIT_OBJECTS");
                if (codes.RowCount == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Данные не выбраны");
                }
                else
                {
                    //Iterate over all rows in the table COMPANYCODE_LIST
                    DateTime dt = DateTime.MinValue;

                    for (int i = 0; i < codes.RowCount; i++)
                    {
                        codes.CurrentIndex = i;
                        AM_SAP amSap = new AM_SAP();

                        amSap.OBJID = codes.GetString("OBJID");
                        amSap.OTYPE = codes.GetString("OTYPE");
                        amSap.PRIOX = codes.GetString("PRIOX");
                        amSap.PROZT = codes.GetDouble("PROZT");
                        dt = DateTime.MinValue;
                        DateTime.TryParseExact(codes.GetString("SBEGDA"), format, culture, DateTimeStyles.AssumeLocal, out dt);
                        amSap.SBEGDA = dt;
                        amSap.SCLAS = codes.GetString("SCLAS");
                        dt = DateTime.MinValue;
                        DateTime.TryParseExact(codes.GetString("SENDDA"), format, culture, DateTimeStyles.AssumeLocal, out dt);
                        amSap.SENDDA = dt;
                        amSap.SHORT = codes.GetString("SHORT");
                        amSap.SOBID = codes.GetString("SOBID");
                        amSap.STEXT = codes.GetString("STEXT");
                        amSap.LEV_HIE = codes.GetInt("LEV_HIE");

                        dt = DateTime.MinValue;
                        DateTime.TryParseExact(codes.GetString("BEGDA"), format, culture, DateTimeStyles.AssumeLocal, out dt);
                        amSap.BEGDA = dt;

                        dt = DateTime.MinValue;
                        DateTime.TryParseExact(codes.GetString("ENDDA"), format, culture, DateTimeStyles.AssumeLocal, out dt);
                        amSap.ENDDA = dt;
                        amSap.BUKRS = "";
                        amSap.PERNR = codes.GetString("PERNR");
                        amSap.R_01 = codes.GetString("R_01");
                        amSap.R_02 = codes.GetString("R_02");
                        amSap.R_03 = codes.GetString("R_03");
                        amSap.R_04 = codes.GetString("R_04");
                        amSap.R_05 = codes.GetString("R_05");
                        amSap.R_06 = codes.GetString("R_06");
                        amSap.R_07 = codes.GetString("R_07");
                        amSap.GESCH = codes.GetString("GESCH");
                        amSap.PERSK = codes.GetString("PERSK");
                        amSap.PERSG = codes.GetString("PERSG");
                        amSap.STRINF_ID = codes.GetString("STRINF_ID");
                        dt = DateTime.MinValue;
                        DateTime.TryParseExact(codes.GetString("DATP"), format, culture, DateTimeStyles.AssumeLocal, out dt);
                        amSap.DATP = dt;
                        amSap.VERB = codes.GetString("VERB");
                        if (amSap.SHORT != null && amSap.SHORT != "")
                        {
                            amSap.SHOPNUMBER = amSap.SHORT.Substring(0, 4);
                            if (amSap.SHORT.Substring(4) == "000000")
                                amSap.ISSHOP = true;
                            else
                                amSap.ISSHOP = false;
                        }
                        else
                        {
                            amSap.SHOPNUMBER = "";
                            amSap.ISSHOP = false;
                        }

                        dt = DateTime.MinValue;
                        DateTime.TryParseExact(codes.GetString("BEGDA_D"), format, culture, DateTimeStyles.AssumeLocal, out dt);
                        amSap.BEGDA_D = dt;
                        dt = DateTime.MinValue;
                        DateTime.TryParseExact(codes.GetString("ENDDA_D"), format, culture, DateTimeStyles.AssumeLocal, out dt);
                        amSap.ENDDA_D = dt;
                        amSap.MVZ = "";//DataGlobals.MVZ_EVRAZRUDA_DIVIZION;
                        amSap.MVZ_NAME = ""; //getMVZName(organizationId);
                        list.Add(amSap);
                    }
                }
            }
            return list;
        }

        private string getOrganizationBurks(int orgId){
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

        private string getOrganizationExternalCode(int orgId)
        {
            string externalcode = "";
            OracleCommand cmd = new OracleCommand("Select externalcode from Organizations where id=" + orgId);
            cmd.Connection = getSqlConnection();
            cmd.CommandType = CommandType.Text;
            try
            {
                OracleDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    externalcode = Convert.ToString(reader["externalcode"]);
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
            return externalcode;
        }

        public string LoadOrganization(string shopId, string shopNumber, string organizationId, string sessionId, string childCare)
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
                    string IM_BUKRS = getOrganizationBurks(int.Parse(organizationId));
                    string strDate = DateTime.Today.ToString("yyyyMMdd");
                    string endDate = (DateTime.Today.AddDays(5)).ToString("yyyyMMdd");

                    if (shopNumber != DataGlobals.ZSMK_DIVIZION && shopNumber != DataGlobals.EVRAZRUDA_DIVIZION)
                    {
                        Dictionary<string, string> openWith = new Dictionary<string, string>();
                        openWith.Add("IM_PLVAR", "01");
                        openWith.Add("IM_BUKRS", IM_BUKRS);
                        openWith.Add("IM_BEGDA", strDate);
                        openWith.Add("IM_ENDDA", endDate);


                        if (shopId != null && shopId != "" && shopId != "0")
                        {
                            string im_werks = shopNumber;
                            openWith.Add("IM_WERKS", im_werks);
                        }
                        amSaps = getOrganization(openWith , organizationId);
                    }
// Добавляем дивизион Сибирь                    

                    if (shopId == null || shopId == "" || shopId == "0" || shopNumber == DataGlobals.ZSMK_DIVIZION)
                    {
                        List<AM_SAP> amSapDivizions = LoadDivizionOrganization(shopId, DataGlobals.ZSMK_DIVIZION , organizationId, sessionId, childCare);
                        amSaps.AddRange(amSapDivizions);
                    }

                    // Добавляем дивизион Сибирь Евразруды
                    if (shopId == null || shopId == "" || shopId == "0" || shopNumber == DataGlobals.EVRAZRUDA_DIVIZION)
                    {
                        List<AM_SAP> amSapDivizions = LoadDivizionOrganization(shopId, DataGlobals.EVRAZRUDA_DIVIZION, organizationId, sessionId, childCare);
                        amSaps.AddRange(amSapDivizions);
                    }
                    if (shopId == "") shopId = "0";
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("organizationId", organizationId);
                    parameters.Add("shop_id", shopId);
                    parameters.Add("childCare", childCare);

                    if (amSaps.Count != 0)
                    {
                        using (var transaction = conn.BeginTransaction())
                        {
                            OracleCommand cmd = new OracleCommand();
                            cmd.Connection = conn;
                            foreach (AM_SAP amSap in amSaps)
                            {
                                cmd.CommandText = "Insert into AM_SAPS (OBJID,OTYPE,SHORT,STEXT,PROZT,BEGDA,ENDDA,PRIOX,SCLAS,SOBID,SBEGDA,SENDDA,LEV_HIE,BUKRS,PERNR,R_01,R_02,R_03,R_04,R_05,R_06,R_07,GESCH,PERSK,PERSG,SESSIONID,DATP,STRINF_ID,VERB,SHOPNUMBER,ISSHOP,BEGDA_D,ENDDA_D, MVZ,MVZNAME) " +
                                    "VALUES (" + formatStringParam(amSap.OBJID) + ","
                                    + formatStringParam(amSap.OTYPE) + ","
                                    + formatStringParam(amSap.SHORT) + ","
                                    + formatStringParam(amSap.STEXT) + ","
                                    //+ formatDoubleParam(amSap.PROZT) + ","
                                    + "null ,"
                                    + formatDateParam(amSap.BEGDA) + ","
                                    + formatDateParam(amSap.ENDDA) + ","
                                    + formatStringParam(amSap.PRIOX) + ","
                                    + formatStringParam(amSap.SCLAS) + ","
                                    + formatStringParam(amSap.SOBID) + ","
                                    + formatDateParam(amSap.SBEGDA) + ","
                                    + formatDateParam(amSap.SENDDA) + ","
                                    + formatIntParam(amSap.LEV_HIE) + ","
                                    + formatStringParam(IM_BUKRS) + ","
                                    + formatStringParam(amSap.PERNR) + ","
                                    + formatStringParam(amSap.R_01) + ","
                                    + formatStringParam(amSap.R_02) + ","
                                    + formatStringParam(amSap.R_03) + ","
                                    + formatStringParam(amSap.R_04) + ","
                                    + formatStringParam(amSap.R_05) + ","
                                    + formatStringParam(amSap.R_06) + ","
                                    + formatStringParam(amSap.R_07) + ","
                                    + formatStringParam(amSap.GESCH) + ","
                                    + formatStringParam(amSap.PERSK) + ","
                                    + formatStringParam(amSap.PERSG) + ","
                                    + formatStringParam(sessionId) + ","
                                    + formatDateParam(amSap.DATP) + ","
                                    + formatStringParam(amSap.STRINF_ID) + ","
                                    + formatStringParam(amSap.VERB) + ","
                                    + formatStringParam(amSap.SHOPNUMBER) + ","
                                    + formatBoolParam(amSap.ISSHOP) + ","
                                    + formatDateParam(amSap.BEGDA_D) + ","
                                    + formatDateParam(amSap.ENDDA_D) + ","
                                    + formatStringParam(amSap.MVZ) + ","
                                    + formatStringParam(amSap.MVZ_NAME)

                                    + ")";
                                cmd.ExecuteNonQuery();
                            }
                            transaction.Commit();
                            error = error + RunOrganizationLoad(parameters, sessionId);
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

        private List<AM_SAP> getDivizionOrganization(Dictionary<string, string> openWith, string parentExternalCode, string organizationId)
        {
            try
            {
                RfcDestinationManager.RegisterDestinationConfiguration(new ZSMK_SAP_Config());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            RfcDestination destination = RfcDestinationManager.GetDestination("RFC_EAH");
            IRfcFunction function = null;
            string strDate = DateTime.Today.ToString("yyyyMMdd");

            IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
            string format = "yyyy-MM-dd";
            List<AM_SAP> list = new List<AM_SAP>();
            string prefix = "";

            try
            {
                function = destination.Repository.CreateFunction("ZP_GET_OSP");
                foreach (KeyValuePair<string, string> kvp in openWith)
                {
                    function.SetValue(kvp.Key, kvp.Value);
                    if (kvp.Key == "IM_WERKS")
                        prefix = kvp.Value;
                }

                function.Invoke(destination);
            }
            catch (RfcBaseException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            if (function != null)
            {
                //Get the function table parameter COMPANYCODE_LIST
                IRfcTable codes = function.GetTable("PIT_OBJECTS");
                if (codes.RowCount == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Данные не выбраны");
                }
                else
                {
                    //Iterate over all rows in the table COMPANYCODE_LIST
                    DateTime dt = DateTime.MinValue;

                    for (int i = 0; i < codes.RowCount; i++)
                    {
                        codes.CurrentIndex = i;
                        AM_SAP amSap = new AM_SAP();

                        if (codes.GetString("OTYPE") == "P")
                            amSap.OBJID = "1111" + codes.GetString("OBJID").TrimStart('0');
                        else
                            amSap.OBJID = prefix + codes.GetString("OBJID");
                        amSap.SOBID = prefix + codes.GetString("SOBID");
                        amSap.OTYPE = codes.GetString("OTYPE");
                        amSap.PRIOX = codes.GetString("PRIOX");
                        amSap.PROZT = codes.GetDouble("PROZT");
                        dt = DateTime.MinValue;
                        DateTime.TryParseExact(codes.GetString("SBEGDA"), format, culture, DateTimeStyles.AssumeLocal, out dt);
                        amSap.SBEGDA = dt;
                        amSap.SCLAS = codes.GetString("SCLAS");
                        dt = DateTime.MinValue;
                        DateTime.TryParseExact(codes.GetString("SENDDA"), format, culture, DateTimeStyles.AssumeLocal, out dt);
                        amSap.SENDDA = dt;
                        amSap.SHORT = codes.GetString("SHORT");
                        amSap.STEXT = codes.GetString("STEXT");
                        amSap.LEV_HIE = codes.GetInt("LEV_HIE");

                        dt = DateTime.MinValue;
                        DateTime.TryParseExact(codes.GetString("BEGDA"), format, culture, DateTimeStyles.AssumeLocal, out dt);
                        amSap.BEGDA = dt;

                        dt = DateTime.MinValue;
                        DateTime.TryParseExact(codes.GetString("ENDDA"), format, culture, DateTimeStyles.AssumeLocal, out dt);
                        amSap.ENDDA = dt;
                        amSap.BUKRS = "";
                        amSap.PERNR = codes.GetString("PERNR");
                        amSap.R_01 = codes.GetString("R_01");
                        amSap.R_02 = codes.GetString("R_02");
                        amSap.R_03 = codes.GetString("R_03");
                        amSap.R_04 = codes.GetString("R_04");
                        amSap.R_05 = codes.GetString("R_05");
                        amSap.R_06 = codes.GetString("R_06");
                        amSap.R_07 = codes.GetString("R_07");
                        amSap.GESCH = codes.GetString("GESCH");
                        amSap.PERSK = codes.GetString("PERSK");
                        amSap.PERSG = codes.GetString("PERSG");
                        amSap.STRINF_ID = codes.GetString("STRINF_ID");
                        dt = DateTime.MinValue;
                        DateTime.TryParseExact(codes.GetString("DATP"), format, culture, DateTimeStyles.AssumeLocal, out dt);
                        amSap.DATP = dt;
                        amSap.VERB = codes.GetString("VERB");
                        amSap.SHOPNUMBER = prefix;
                        amSap.ISSHOP = false;
                        if (amSap.SHORT == "EAH")
                        {
                            amSap.ISSHOP = true;
                            amSap.SOBID = parentExternalCode;
                        }

                        dt = DateTime.MinValue;
                        DateTime.TryParseExact(codes.GetString("BEGDA_D"), format, culture, DateTimeStyles.AssumeLocal, out dt);
                        amSap.BEGDA_D = dt;
                        dt = DateTime.MinValue;
                        DateTime.TryParseExact(codes.GetString("ENDDA_D"), format, culture, DateTimeStyles.AssumeLocal, out dt);
                        amSap.ENDDA_D = dt;
                        if (prefix == DataGlobals.EVRAZRUDA_DIVIZION)
                        {
                            amSap.MVZ = DataGlobals.MVZ_ZSMK_RUDA_DIVIZION;
                            amSap.MVZ_NAME = getMVZName(organizationId, amSap.MVZ);
                        }
                        else
                        {
                            amSap.MVZ = "";
                            amSap.MVZ_NAME = "";
                        }
                        list.Add(amSap);
                    }
                }
            }
            return list;
        }

        private string getMVZName(string orgId, string MVZ)
        {
            string name = "";
            OracleCommand cmd = new OracleCommand("Select NAME from mvzs where organizationid=" + orgId + " and mvz='" + DataGlobals.MVZ_ZSMK_RUDA_DIVIZION + "'");
            cmd.Connection = getSqlConnection();
            cmd.CommandType = CommandType.Text;
            try
            {
                OracleDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    name = Convert.ToString(reader["name"]);
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
            return name;
        }

        private List<AM_SAP> LoadDivizionOrganization(string shopId, string shopNumber, string organizationId, string sessionId, string childCare)
        {
            string error = "";
            List<AM_SAP> amSaps = new List<AM_SAP>();
            try
            {
                //string IM_BUKRS = getOrganizationBurks(int.Parse(organizationId));
                string IM_BUKRS = "1000";

                string strDate = DateTime.Today.ToString("yyyyMMdd");
                string endDate = (DateTime.Today.AddDays(5)).ToString("yyyyMMdd");

                Dictionary<string, string> openWith = new Dictionary<string, string>();
                openWith.Add("IM_PLVAR", "01");
                openWith.Add("IM_BUKRS", IM_BUKRS);
                openWith.Add("IM_BEGDA", strDate);
                openWith.Add("IM_ENDDA", endDate);
                openWith.Add("IM_WERKS", shopNumber);
                //openWith.Add("IM_WERKS", "E850");
                string parentExternalCode = getOrganizationExternalCode(int.Parse(organizationId));
                amSaps = getDivizionOrganization(openWith, parentExternalCode , organizationId);
            }
            catch (Exception e)
            {
                error = e.Message;
                System.Diagnostics.Debug.WriteLine(error);
            }
            return amSaps;
        }



    }
}
