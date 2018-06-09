<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Store.Core.NomGroup>" %>
<%= Html.Encode(Model != null?Model.Name:"") %>