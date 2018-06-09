<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Store.Core.Motiv>" %>
<%@ Import Namespace="Store.Data" %>

<%= Html.Telerik().DropDownList()
        .Name(ViewData.TemplateInfo.GetFullHtmlFieldName(string.Empty))
        .BindTo(new SelectList((IEnumerable)ViewData[DataGlobals.REFERENCE_MOTIV], "Id", "Name", Model != null?Model.Id.ToString():null))
%>