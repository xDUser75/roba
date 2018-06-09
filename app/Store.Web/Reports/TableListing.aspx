<%@ Page Title="Отчет" Language="C#" AutoEventWireup="true" CodeBehind="TableListing.aspx.cs" Inherits="Store.Web.Views.Reports.TableListing" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Отчет</title>
</head>
<body>
   
    <form id="form1" runat="server" target="_blank" height="90%" width="90%">
     <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="True">
    </asp:ScriptManager>
    <div style="height: 100%; width: 100%;">
    
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" Width="100%" 
            Font-Names="Verdana" Font-Size="8pt" InteractiveDeviceInfos="(Collection)" 
            ProcessingMode="Remote" WaitMessageFont-Names="Verdana" 
            WaitMessageFont-Size="14pt" Height="100%">
            
        </rsweb:ReportViewer>
    
    </div>
    </form>
</body>
</html>
