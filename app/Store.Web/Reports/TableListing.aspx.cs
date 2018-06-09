using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Configuration;
using Microsoft.Reporting.WebForms;


namespace Store.Web.Views.Reports
{
    public partial class TableListing : System.Web.UI.Page
    {
      
       
        protected void Page_Load(object sender, EventArgs e)
        {

         
            var repName = Session["Report"].ToString();
            var opg=Session["_idOrg"].ToString();
            
            // Номер документа для М11
            var numdoc = Session["_numdoc"].ToString();
           
            
            switch (repName)
            {
                case "ReportVedomost":    
                    this.Title = "Учет СИЗ и спецодежды (Оборотная ведомость)";
                    break;
                case "ReportРlanOutputByWorker":
                    this.Title = "Учет СИЗ и спецодежды (План выдачи спецодежды по сотрудникам)";
                    break;
                case "ReportРlanOutputByCeh":
                    this.Title = "Учет СИЗ и спецодежды (План выдачи спецодежды по подразделениям)";
                    break;
                case "ReportWorkerCards":
                    this.Title = "Учет СИЗ и спецодежды (Личная карточка(выдано))";
                    break;
                case "ReportWorkerCardsSdano":
                    this.Title = "Учет СИЗ и спецодежды (Личная карточка)";
                    break;
                case "ReportM11":
                    this.Title = "Учет СИЗ и спецодежды (Требование-накладная M11)";
                    break;
                case "ReportNormatByCeh":
                    this.Title = "Учет СИЗ и спецодежды (Нормы на рабочих местах)";
                    break;
                case "ReportOperation":
                    this.Title = "Учет СИЗ и спецодежды (Просмотр операций)";
                    break;
                default:
                    this.Title = "Учет СИЗ и спецодежды (Отчетный документ)";
                    break;
            }

            this.ReportViewer1.Visible = true;
                        

            if (!Page.IsPostBack)
            {

                //Specify the report server
                ReportViewer1.
                  ServerReport.
                  ReportServerUrl =
                  new Uri(WebConfigurationManager.
                  AppSettings["ReportServerURL"]);

                //Specify the report name
                ReportViewer1.
                  ServerReport.
                //ReportPath = Session["reportPath"].ToString();
                ReportPath = WebConfigurationManager.
                  AppSettings["ReportServerFolder"].ToString() + @"/" + repName;

                // Параметры авторизации на сервере
                ReportViewer1.ServerReport.ReportServerCredentials = new MyReportServerCredentials(Request.LogonUserIdentity);

                //Передать параметры серверу отчетов
               List<ReportParameter> paramList = new List<ReportParameter>();

                paramList.Add(new ReportParameter("RptParamOrganization", opg, false));

                if (repName == "ReportM11")
                {
                    paramList.Add(new ReportParameter("RptParamDocum", numdoc, true));
                }
                this.ReportViewer1.ServerReport.SetParameters(paramList);
                
                               
///////////

            //    //Specify the server credentials
            //    //ReportViewer1.
            //    //  ServerReport.
            //    //  ReportServerCredentials =
            //    //  new CustomReportCredentials
            //    //   (
            //    //     WebConfigurationManager.
            //    //      AppSettings["ReportServerUser"],
            //    //     WebConfigurationManager.
            //    //      AppSettings["ReportServerPassword"],
            //    //     WebConfigurationManager.
            //    //      AppSettings["ReportServerDomain"]
            //    //   );
            //    /*
            //     * With the report specified, hydrate the report
            //     * parameters based on the values in the
            //     * reportParameters hash.
            //     */
            ////    var reportParameters = (Dictionary<string,
            ////      string>)Session["reportParameters"];

            ////    foreach (var item in reportParameters)
            ////    {
            ////        ReportViewer1.
            ////          ServerReport.
            ////          SetParameters(
            ////            new List<ReportParameter>() 
            ////  { 
            ////    new ReportParameter
            ////      (item.Key, item.Value) 
            ////  });
            ////    }
            }


        }


        private class MyReportServerCredentials : IReportServerCredentials
        {

            private System.Security.Principal.WindowsIdentity _myIdentity;

            public MyReportServerCredentials(System.Security.Principal.WindowsIdentity identity)
            {
                _myIdentity = identity;
            }

            #region IReportServerCredentials Members

            public bool GetFormsCredentials(out System.Net.Cookie authCookie, out string userName, out string password, out string authority)
            {
                authCookie = null;
                userName = "";
                password = "";
                authority = "";
                return false;
            }

            public System.Security.Principal.WindowsIdentity ImpersonationUser
            {
                get { return _myIdentity; }
            }

            public System.Net.ICredentials NetworkCredentials
            {
                get { return null; }
            }

            #endregion
        }

    }
}