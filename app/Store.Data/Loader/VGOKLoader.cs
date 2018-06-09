using System;
using System.Collections.Generic;
using Oracle.DataAccess.Client;
using System.Data;
using Store.Core.External.Interfaсe;

namespace Store.Data.Loader
{
    public class VGOKLoader : ExternalLoader, IExternalLoaderOrganization
    {
        public string LoadOrganization(string shopId, string shopNumber, string organizationId, string sessionId, string chilCare)
        {
            string error = "";
            var conn = getSqlConnection();
            if (conn == null)
            {
                conn = createSQLConnection();
                setSqlConnection(conn);
            }
            if (conn.State != ConnectionState.Open) conn.Open();

            if (shopNumber == "") shopNumber = null;
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    OracleCommand cmd = new OracleCommand("VGOK_STORE.load_data", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    OracleParameter inval = null;
                    inval=    new OracleParameter("p_organizationId", OracleDbType.Int32);
                    inval.Direction = ParameterDirection.Input;
                    inval.Value = int.Parse(organizationId);
                    cmd.Parameters.Add(inval);

                    inval = new OracleParameter("p_shopNumber", OracleDbType.Varchar2);
                    inval.Direction = ParameterDirection.Input;
                    inval.Value = shopNumber;
                    cmd.Parameters.Add(inval);

                    inval = new OracleParameter("p_SESSIONID", OracleDbType.Varchar2);
                    inval.Direction = ParameterDirection.Input;
                    inval.Value = sessionId;
                    cmd.Parameters.Add(inval);
                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    error = e.Message;
                    System.Diagnostics.Debug.WriteLine(error);
                }
                if (error.Length == 0)
                {
                    if ((shopId == "") || (shopId == null)) shopId = "0";
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("organizationId", organizationId);
                    parameters.Add("shop_id", shopId);
                    error = error + RunOrganizationLoad(parameters, sessionId);
                }
                return error;
            }
        }

    }
}
