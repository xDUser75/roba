<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Store.Core.Nomenclature>" %>
<%= Html.Encode(Model != null?Model.Name:"") %>