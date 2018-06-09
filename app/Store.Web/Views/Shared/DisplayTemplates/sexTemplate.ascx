<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Store.Core.Sex>" %>
<%if (Model != null){%>
    <%= Html.Encode(Model.Name)%>
<%}%>