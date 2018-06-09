<script language="C#" runat="server">
  string getReportUrl(string reportName) {
	  String EncodeName;
	  EncodeName=Server.UrlEncode(reportName);
	  //return ConfigurationManager.AppSettings["ReportServerURL"] + "/Pages/ReportViewer.aspx?" + ConfigurationManager.AppSettings["ReportServerFolder"] + "%2f" + reportName + "&rs:Command=Render&RptParamOrganization=" + (string)Session["_idOrg"];
	  return ConfigurationManager.AppSettings["ReportServerURL"] + "/Pages/ReportViewer.aspx?" + ConfigurationManager.AppSettings["ReportServerFolder"] + "%2f" +  EncodeName + "&rs:Command=Render&RptParamOrganization=" + (string)Session["_idOrg"];
  }  
</script>
