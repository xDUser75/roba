<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Store.Core.Sex>" %>
<%:
    Html.Telerik().DropDownList()
            .Name("IsWinter")
            .BindTo( new [] { new SelectListItem() { Value = "0", Text = "Нет" }, new SelectListItem(){ Value = "1", Text = "Да" }})
%>
