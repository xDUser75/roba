using Quartz;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using System.Web;
using System.Configuration;

namespace Store.Web.Controllers
{
    public class UpdateOrganizationJob : IJob
    {

        public void Execute(JobExecutionContext context) 
        {
            String serverName = ConfigurationManager.AppSettings["DeployServerName"];
            String applicationVirtualPath = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            NetworkCredential nc = new NetworkCredential("?", "?"); 
            WebClient client = new WebClient();
            client.Credentials = nc;
            try
            {
//                client.DownloadData(serverName + applicationVirtualPath + "/AM_SAPS/RunJob");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Обновление структуры организации не прошло: " + ex.ToString());
            }
        }
    }
}
