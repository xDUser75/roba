<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Store.Core.Sex>" %>
<%:
    Html.Telerik().DropDownList()
            .Name(ViewData.TemplateInfo.GetFullHtmlFieldName(string.Empty))
            .BindTo(new SelectList((IEnumerable)ViewData[Store.Data.DataGlobals.REFERENCE_SEX], "Id", "Name"))
%>
