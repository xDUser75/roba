using System;
using System.Collections.Generic;
using Oracle.DataAccess.Client;
using System.Data;
using System.Configuration;
using System.Xml;
using System.IO;
using System.Data.Common;
using Store.Core.External;

namespace Store.Data.Loader
{
    public class ExternalLoader
    {
        private DbConnection _sqlConnection;

        //public static OracleConnection createSQLConnection() {
        //    bool isDebug = bool.Parse(ConfigurationManager.AppSettings["isDebug"]);
        //    // Создаем экземпляр класса
        //    XmlDocument xmlDoc = new XmlDocument();
        //    string connectString  = "";
        //    // Загружаем XML-документ из файла
        //    var physicalFilePath = Path.Combine(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, "NHibernate" + (isDebug ? "Debug" : "") + ".config");
        //    xmlDoc.Load(physicalFilePath);
        //    //Пришлось городить такой огород, т.к. такая конструкция:
        //    //XmlNode findNode = xmlDoc.SelectSingleNode("/hibernate-configuration/session-factory/property[@name='connection.connection_string']");            
        //    //НЕ РАБОТАЕ Т!!!
        //    XmlNode findNode=null;
        //    for (int i=0; i<xmlDoc.ChildNodes.Count; i++) { 
        //        if (xmlDoc.ChildNodes[i].Name=="hibernate-configuration"){
        //            findNode = xmlDoc.ChildNodes[i];
        //            break;
        //        }                
        //    }
        //    if (findNode == null) return null;
        //    for (int i = 0; i < findNode.ChildNodes.Count; i++)
        //    {
        //        if (findNode.ChildNodes[i].Name == "session-factory")
        //        {
        //            findNode = findNode.ChildNodes[i];
        //            break;
        //        }
        //    }
        //    if (findNode == null) return null;
        //    for (int i = 0; i < findNode.ChildNodes.Count; i++)
        //    {
        //        if ((findNode.ChildNodes[i].Name == "property") && (findNode.ChildNodes[i].Attributes["name"].Value == "connection.connection_string"))
        //        {
        //            connectString  = findNode.ChildNodes[i].InnerText;
        //            break;
        //        }
        //    }
        //    if (connectString == "") return null;
        //    OracleConnection sqlConnection = new OracleConnection(connectString);
        //    return sqlConnection;
        //}

        public static OracleConnection createSQLConnection()
        {
            // Создаем экземпляр класса
            XmlDocument xmlDoc = new XmlDocument();
            string connectString = "";
            // Загружаем XML-документ из файла
            var physicalFilePath = Path.Combine(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, "Web.config");
            xmlDoc.Load(physicalFilePath);
            //Пришлось городить такой огород, т.к. такая конструкция:
            //XmlNode findNode = xmlDoc.SelectSingleNode("/configuration/hibernate-configuration/session-factory/property[@name='connection.connection_string']");            
            //НЕ РАБОТАЕ Т!!!
            XmlNode findNode = null;
            for (int i = 0; i < xmlDoc.ChildNodes.Count; i++)
            {
                if (xmlDoc.ChildNodes[i].Name == "configuration")
                {
                    findNode = xmlDoc.ChildNodes[i];
                    break;
                }
            }
            if (findNode == null) return null;
            for (int i = 0; i < findNode.ChildNodes.Count; i++)
            {
                if (findNode.ChildNodes[i].Name == "hibernate-configuration")
                {
                    findNode = findNode.ChildNodes[i];
                    break;
                }
            }
            if (findNode == null) return null;
            for (int i = 0; i < findNode.ChildNodes.Count; i++)
            {
                if (findNode.ChildNodes[i].Name == "session-factory")
                {
                    findNode = findNode.ChildNodes[i];
                    break;
                }
            }
            if (findNode == null) return null;
            for (int i = 0; i < findNode.ChildNodes.Count; i++)
            {
                if ((findNode.ChildNodes[i].Name == "property") && (findNode.ChildNodes[i].Attributes["name"].Value == "connection.connection_string"))
                {
                    connectString = findNode.ChildNodes[i].InnerText;
                    break;
                }
            }
            if (connectString == "") return null;
            OracleConnection sqlConnection = new OracleConnection(connectString);
            return sqlConnection;
        }

        public void setSqlConnection(DbConnection connection)
        {
            _sqlConnection = connection;
        }

        public OracleConnection getSqlConnection()
        {
            return (OracleConnection)_sqlConnection;
        }

        public static string formatStringParam(string value)
        { 
            if (value==null) return "null";
            return "'"+value+"'";
        }

        public static string formatIntParam(int value)
        {            
            return  value.ToString();
        }

        public static string formatDateParam(DateTime value)
        {
            if (value == null) return "null";
            return "to_date('" + value.ToString("dd.MM.yyyy HH:mm:ss") + "', 'dd.mm.yyyy hh24:mi:ss')";
        }

        public static string formatBoolParam(Boolean value)
        {            
            if (value) return "1";
                else return "0";
        }

        public static string formatDoubleParam(Double value)
        {            
            return value.ToString().Replace(',','.');
        }

        public string RunOrganizationLoad(Dictionary<string, string> parameters, string sessionId)
        {
            var conn = getSqlConnection();
            if (conn == null) return "Не vkue соединиться с базой.";
            string organizationId = "", shop_id = "", childCare="";
            string error = "";
            foreach (KeyValuePair<string, string> kvp in parameters)
            {
                if (kvp.Key == "organizationId")
                    organizationId = kvp.Value;
                if (kvp.Key == "shop_id")
                    shop_id = kvp.Value;
                if (kvp.Key == "childCare")
                    childCare = kvp.Value;

            }

            if (conn.State != ConnectionState.Open) conn.Open();
            var transaction = conn.BeginTransaction();
            try
            {
                try
                {

                    OracleCommand cmd = new OracleCommand("SAP_ORGANIZATION.load", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    OracleParameter inval = new OracleParameter("organization_Id", OracleDbType.Int32);
                    inval.Direction = ParameterDirection.Input;
                    inval.Value = int.Parse(organizationId);
                    cmd.Parameters.Add(inval);

                    inval = new OracleParameter("shop_id", OracleDbType.Varchar2);
                    inval.Direction = ParameterDirection.Input;
                    inval.Value = shop_id;
                    cmd.Parameters.Add(inval);

                    inval = new OracleParameter("session_id", OracleDbType.Varchar2);
                    inval.Direction = ParameterDirection.Input;
                    inval.Value = sessionId;
                    cmd.Parameters.Add(inval);

                    if (childCare != null && childCare != "null" && childCare != "")
                    {
                        inval = new OracleParameter("childCare", OracleDbType.Int32);
                        inval.Direction = ParameterDirection.Input;
                        inval.Value = int.Parse(childCare);
                        cmd.Parameters.Add(inval);
                    }

                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    error = e.Message;
                    System.Diagnostics.Debug.WriteLine(error);                    
                }
            }
            finally
            {
                transaction.Commit();
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return error;
        }
/*
        public string RunNomenclaturesLoad(string organizationId, string sessionId)
        {
            var conn = getSqlConnection();
            if (conn == null) return "Не vkue соединиться с базой.";
            string error = "";

            if (conn.State != ConnectionState.Open) conn.Open();
            var transaction = conn.BeginTransaction();
            try
            {
                try
                {

                    OracleCommand cmd = new OracleCommand("PCK_STORE.nomenclature_rfc_SAP", conn);
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
                    error = e.Message;
                    System.Diagnostics.Debug.WriteLine(error);
                }
            }
            finally
            {
                transaction.Commit();
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return error;
        }

*/
    }
}
