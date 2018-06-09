<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<% Html.Telerik().DropDownList()
             .Name(ViewData.TemplateInfo.GetFullHtmlFieldName(string.Empty))
             .BindTo(new SelectList((IEnumerable)ViewData[Store.Data.DataGlobals.REFERENCE_UNIT], "Id", "Name"))
             .Render();
%>
