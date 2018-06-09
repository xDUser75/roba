<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<string>" %>
<%@ Import Namespace="Store.Data" %>

<%= Html.Telerik().DropDownList()
        .Name(ViewData.TemplateInfo.GetFullHtmlFieldName(string.Empty))
        .Items(items => 
        {
            items.Add().Value("50").Text("б/у").Selected("50".Equals(Model));
            items.Add().Value("100").Text("новая").Selected("100".Equals(Model));
//            items.Add().Value("0").Text("утиль").Selected("0".Equals(Model));
        })
%>