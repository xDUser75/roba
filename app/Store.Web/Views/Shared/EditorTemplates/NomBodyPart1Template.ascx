<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%= Html.Telerik().DropDownList()
    .Name("NomBodyPart")
    .BindTo(new SelectList((IEnumerable)ViewData["nombodyparts"], "Id", "Name"))
%>
