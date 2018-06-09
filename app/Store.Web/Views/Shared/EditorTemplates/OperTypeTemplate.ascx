<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Store.Core.OperType>" %>
<%@ Import Namespace="Store.Data" %>

<%= Html.Telerik().DropDownList()
        .Name(ViewData.TemplateInfo.GetFullHtmlFieldName(string.Empty))
          .BindTo(new SelectList((IEnumerable)ViewData[DataGlobals.REFERENCE_OPER_TYPE], "Id", "Name", Model != null ? Model.Id.ToString() : null))
%>
<%--= Html.Telerik().ComboBox()
                .Name(ViewData.TemplateInfo.GetFullHtmlFieldName(string.Empty))
        .AutoFill(true)
                //.SelectedIndex(Model != null ? Model.Id : -1)
        .BindTo(new SelectList((IEnumerable)ViewData["operTypes"], "Id", "Name", Model != null ? Model.Id.ToString() : null))
        //.HtmlAttributes(new { style = string.Format("width:{0}px", 200) })
        .Filterable(filtering =>
                filtering
                .FilterMode(AutoCompleteFilterMode.StartsWith)
                .MinimumChars(3)
        )
        .HighlightFirstMatch(true)
        .OpenOnFocus(false)
--%>
