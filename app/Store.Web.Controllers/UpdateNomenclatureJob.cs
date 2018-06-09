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
    public class UpdateNomenclatureJob : IJob
    {
        //private DbConnection conn = UpdateOrganizationJob.conn;
       
        public void Execute(JobExecutionContext context)
        {
            bool isDebug = bool.Parse(ConfigurationManager.AppSettings["isDebug"]);
            JobDataMap data = context.JobDetail.JobDataMap;
            String organizationId = data.GetString("organizationId");
            //isDebug = false;
            // На отладочном сервере не обновляем организации
            if (isDebug == false)
            {
                System.Diagnostics.Debug.WriteLine("Job Nomenclature запустился " + DateTime.Now.ToString());
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
                    System.Diagnostics.Debug.WriteLine("Nomenclature Не удалось создать подключение к БД!");
                    return;
                }
                try
                {
                    conn.Open();
                    UpdateOrganizationJob.insertError("NomQuartz-job - " + organizationId + " " + serverHostName, conn);

                    if (UpdateOrganizationJob.canRunJob(organizationId, serverHostName, out status, conn))
                    {
//                        insertError("canRunJob - " + true + " " + organizationId + " " + serverHostName);
                        DateTime currentDate = DateTime.Now;


                        string assemblyName = ApplicationConfig.ReadVariable("/Configuration/Organization[@id=" + organizationId + "]/InterfaceLoadNomenclature/AssemblyName");
                        string className = ApplicationConfig.ReadVariable("/Configuration/Organization[@id=" + organizationId + "]/InterfaceLoadNomenclature/ClassName");
                        object loader = Store.Core.Utils.Reflection.LoadClassObject(assemblyName, className);

                        if (loader != null)
                        {
                            try
                            {
                                ((Store.Data.Loader.ExternalLoader)loader).setSqlConnection(conn);
                                //string error = ((IExternalLoaderOrganization)loader).LoadOrganization("0", "0", organizationId, "quartz-job " + currentDate.ToString() + " " + serverHostName, "0");
                                string error = ((IExternalLoaderNomenclature)loader).LoadNomenclature(organizationId, "quartz-job " + currentDate.ToString() + " " + serverHostName);
                                if (error.Length > 0) UpdateOrganizationJob.insertError("ERROR NomQuartz-job - " + serverHostName + " " + organizationId + " " + error, conn);
                                String retData = "Дата окончания работы NomJob: " + DateTime.Now.ToString();
                                System.Diagnostics.Debug.WriteLine(retData);
                                UpdateOrganizationJob.insertError(retData, conn);

                            }
                            catch (Exception e)
                            {
                                UpdateOrganizationJob.insertError("ERROR Nomenclature quartz-job - " + serverHostName + " " + organizationId + " " + e.Message, conn);
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Nomenclature Не удалось загрузить класс!");
                        }
                    }
                    else
                    {
                        UpdateOrganizationJob.insertError("Работа NomJob пропущена! " + organizationId + " " + serverHostName, conn);
                        System.Diagnostics.Debug.WriteLine("Работа NomJob пропущена!");
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