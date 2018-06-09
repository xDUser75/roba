<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<String>" %>
<%:
    Html.Telerik().DropDownList()
            .Name(ViewData.TemplateInfo.GetFullHtmlFieldName(string.Empty))
            .BindTo(new SelectList((IEnumerable)ViewData[Store.Data.DataGlobals.REFERENCE_NOM_BODY_PART_SIZE_GROWTH], "Id", "SizeNumber"))
%>
