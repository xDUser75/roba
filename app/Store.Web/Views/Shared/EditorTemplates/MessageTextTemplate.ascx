<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<string>" %>
<%@ Import Namespace="Store.Data" %>

    <%= Html.Telerik().Editor()
            .Name(ViewData.TemplateInfo.GetFullHtmlFieldName(string.Empty))
            .Value(Model)
    %>
