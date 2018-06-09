using Quartz;
using System;
using System.Net;
using System.Configuration;
using Store.Core.Utils;
using Store.Core.External.Interfaсe;
using Oracle.DataAccess.Client;
using System.Data;
using System.Data.Common;

namespace Store.Web.Controllers
{
    public class UpdateOrganizationJob : IJob
    {
        //public static DbConnection conn;

/*
        public static OracleConnection getCurrentSqlConnection() 
        {
            return (OracleConnection) conn;
        }
*/
        public static void insertError(string text, DbConnection conn)
        {
            using (var transaction = conn.BeginTransaction())
            {
                using (OracleCommand cmd = new OracleCommand())
                {
                    //cmd.Connection = getCurrentSqlConnection();
                    cmd.Connection = (OracleConnection)conn;
                    cmd.CommandText = "Insert into ERRORS (ERROR,DATE_Z) VALUES ('" + text + "'," + Store.Data.Loader.ExternalLoader.formatDateParam(DateTime.Now) + ")";
                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }

        public static void insertJobStatus(string idOrganization, string serverHostName, DbConnection conn)
        {
            using (var transaction = conn.BeginTransaction())
            {
                using (OracleCommand cmd = new OracleCommand())
                {
                    //cmd.Connection = getCurrentSqlConnection();
                    cmd.Connection = (OracleConnection)conn;
                    cmd.CommandText = "Insert into JOBSTATUS (ORGANIZATIONID,SERVERNAME,RUNDATE) VALUES (" + idOrganization + ",'" + serverHostName + "'," + Store.Data.Loader.ExternalLoader.formatDateParam(DateTime.Now) + ")";
                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }

        public static void updateJobStatus(string idOrganization, string serverHostName, DbConnection conn)
        {
            using (var transaction = conn.BeginTransaction())
            {
                using (OracleCommand cmd = new OracleCommand())
                {
                    //cmd.Connection = getCurrentSqlConnection();
                    cmd.Connection = (OracleConnection)conn;
                    cmd.CommandText = "update JOBSTATUS set SERVERNAME='" + serverHostName + "', RUNDATE=" + Store.Data.Loader.ExternalLoader.formatDateParam(DateTime.Now) + " where ORGANIZATIONID=" + idOrganization;
                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }

        public static Boolean canRunJob(string idOrganization, string serverHostName, out string status, DbConnection conn)
        {
            Boolean result = false;
            String organizations = ConfigurationManager.AppSettings["QuartzJobOrganization"];

            String[] orgArray = organizations.Split(',');
            foreach (string item in orgArray) {
                if (item == idOrganization) result = true;
            }
            if (result == false)
            {
                status = "В файле Web.config нет этого идентификатора.";
                insertError("В файле Web.config нет этого идентификатора. - " + idOrganization + " " + serverHostName, conn);
                return false;
            }
            result = false;
            Boolean hasRecord = false;
            string jobStatusServerName = "";
            DateTime jobStatusRunDate=DateTime.Now;
            OracleCommand cmd = new OracleCommand("Select * from JobStatus where ORGANIZATIONID=" + idOrganization);

            //cmd.Connection = getCurrentSqlConnection();
            cmd.Connection = (OracleConnection)conn;
            cmd.CommandType = CommandType.Text;

            try
            {

                OracleDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    jobStatusServerName = Convert.ToString(reader["SERVERNAME"]);
                    jobStatusRunDate = Convert.ToDateTime(reader["RUNDATE"]);
                    hasRecord = true;
                }
                else 
                {
                    hasRecord = false;
                }
            }
            catch (Exception)
            {
                hasRecord = false;
            }
            finally
            {
                cmd.Dispose();
            }            
            // Job не выполнялся для заданного предприятия
            if (hasRecord==false)
            {
                try
                {
                    insertJobStatus(idOrganization, serverHostName, conn);
                    result = true;
                    status = "";
                }
                catch (Exception e)
                {
                    result = false;
                    status = e.ToString();
                }
                return result;
            }
            // Job выполнялся для заданного предприятия
            else
            {
                // Если Job запускается с последнего сервера
                if (jobStatusServerName == serverHostName)
                {
                    DateTime currentDate = DateTime.Now;
                    TimeSpan delta = currentDate - jobStatusRunDate;
                    if (delta.TotalHours > 1)
                    {

                        try
                        {
                            updateJobStatus(idOrganization, serverHostName, conn); 
                            result = true;
                            status = "";
                        }
                        catch (Exception e)
                        {                            
                            result = false;
                            status = e.ToString();
                            insertError("ERROR quartz-job - " + status + " " + serverHostName + " " + idOrganization, conn);
                        }
                    }
                    else
                    {
                        result = false;
                        status = "Job запускался менее часа назад с этого сервера: " + serverHostName + ".";
                        insertError("ERROR quartz-job - " + status + " " + serverHostName + " " + idOrganization, conn);
                    }
                    return result;
                }
                // Сервера разные...
                // Проверяем время последнего запуска
                else
                {
                    DateTime currentDate = DateTime.Now;
                    TimeSpan delta = currentDate - jobStatusRunDate;
                    if ((delta.TotalDays == 0) || (delta.TotalDays > 1))
                    {
                        if (delta.TotalHours > 1)
                        {
                            try
                            {
                                updateJobStatus(idOrganization, serverHostName, conn); 
                                result = true;
                                status = "";
                            }
                            catch (Exception e)
                            {
                                result = false;
                                status = e.ToString();
                                insertError("ERROR quartz-job - " + status + " " + serverHostName + " " + idOrganization, conn);
                            }
                            return result;
                        }
                        //Job запускался сегодня
                        else
                        {
                            status = "Job запускался менее 2-х часов назад с другого сервера.";
                            insertError("ERROR quartz-job - "+status+" " + serverHostName + " " + idOrganization, conn);
                            return false;
                        }
                    }
                    //Job запускался вчера
                    else
                    {
                        status = "Job запускался вчера с другого сервера.";
                        insertError("ERROR quartz-job - " + status + " " + serverHostName + " " + idOrganization, conn);
                        return false;
                    }
                }
            }
        }

        public void Execute(JobExecutionContext context)
        {
            bool isDebug = bool.Parse(ConfigurationManager.AppSettings["isDebug"]);
            JobDataMap data = context.JobDetail.JobDataMap;
            String organizationId = data.GetString("organizationId");
            //isDebug = false;
            // На отладочном сервере не обновляем организации
            if (isDebug == false)
            {
                System.Diagnostics.Debug.WriteLine("Job запустился " + DateTime.Now.ToString());
                string serverHostName = "";
                string domainName = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
                string hostName = Dns.GetHostName();
                if (!hostName.Contains(domainName))
                    serverHostName = hostName + "." + domainName;
                else
                    serverHostName = hostName;
                string status = "";
                DbConnection conn = Store.Data.Loader.ExternalLoader.createSQLConnection();
                if (conn == null)
                {
                    System.Diagnostics.Debug.WriteLine("Не удалось создать подключение к БД!");
                    return;
                }
                try
                {
                    conn.Open();
                    insertError("quartz-job - " + organizationId + " " + serverHostName, conn);

                    if (canRunJob(organizationId, serverHostName, out status, conn))
                    {
//                        insertError("canRunJob - " + true + " " + organizationId + " " + serverHostName);
                        DateTime currentDate = DateTime.Now;
                        string assemblyName = ApplicationConfig.ReadVariable("/Configuration/Organization[@id=" + organizationId + "]/InterfaceLoadOrganization/AssemblyName");
                        string className = ApplicationConfig.ReadVariable("/Configuration/Organization[@id=" + organizationId + "]/InterfaceLoadOrganization/ClassName");
                        object loader = Store.Core.Utils.Reflection.LoadClassObject(assemblyName, className);

                        if (loader != null)
                        {
                            try
                            {
                                ((Store.Data.Loader.ExternalLoader)loader).setSqlConnection(conn);
                                string error = ((IExternalLoaderOrganization)loader).LoadOrganization("0", "0", organizationId, "quartz-job " + currentDate.ToString() + " " + serverHostName, "0");
                                if (error.Length > 0) insertError("ERROR quartz-job - " + serverHostName + " " + organizationId + " " + error, conn);
                                String retData = "Дата окончания работы: " + DateTime.Now.ToString();
                                System.Diagnostics.Debug.WriteLine(retData);
                                insertError(retData, conn);

                            }
                            catch (Exception e)
                            {
                                insertError("ERROR quartz-job - " + serverHostName + " " + organizationId + " " + e.Message, conn);
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Не удалось загрузить класс!");
                        }
                    }
                    else
                    {
                        insertError("Работа Job'а пропущена! " + organizationId + " " + serverHostName, conn);
                        System.Diagnostics.Debug.WriteLine("Работа Job'а пропущена!");
                        System.Diagnostics.Debug.WriteLine(status);
                    }
                }
                finally
                {
                    if (conn.State != ConnectionState.Closed)
                        conn.Close();
                }
            }
        }
    }
}