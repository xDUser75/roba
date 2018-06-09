<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="Store.Core.Account" %>
<%@ Import Namespace="Store.Data" %>
<%
    if (Session[DataGlobals.ACCOUNT_KEY] != null)
  {
%>
        Здравствуйте, <b><%: ((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName %></b>
        [ <%: Html.ActionLink("LogOn", "LogOn", "LoginAccount", new { force = true }, null) %> ]
<%
    }
    else {
%> 
        [ <%: Html.ActionLink("Log On", "LogOn", "LoginAccount", new { force = true }, null) %> ]
<%
    }
%>
