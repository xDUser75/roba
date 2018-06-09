<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<String>" %>
<%:
    Html.Telerik().DropDownList()
            .Name(ViewData.TemplateInfo.GetFullHtmlFieldName(string.Empty))            
            .DataBinding(binding => binding.Ajax().Select("_Select_NombodyPartSize", "Storages"))
            .ClientEvents(events => events.OnDataBinding("onSapSizeListBoxDataBinding"))
%>
