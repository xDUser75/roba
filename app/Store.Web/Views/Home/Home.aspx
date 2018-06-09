<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Store.Data" %>
<%@ Import Namespace="Store.Core" %>
<%@ Import Namespace="Store.Core.Account" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<h3 style="text-align:center">Сообщения для пользователей</h3>
<%
    Html.Telerik().Grid<Message>()
        .Name("MessagesGrid")
                .ToolBar(commands => {
           if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_MESSAGE_EDIT))
            {
            commands.Insert().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Добавить" });
            }
            })
        .DataKeys(keys =>
        {
          keys.Add(x => x.Id);
        })
        .DataBinding(dataBinding => 
        {
            dataBinding
            .Ajax()
            .Select("_SelectMessages", "Home")
            .Insert("_SaveMessages", "Home")
            .Update("_SaveMessages", "Home")
            .Delete("_DeleteMessages", "Home");
        })
        .Columns(columns =>
        {
          columns.Command(commands =>
          {
            if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_MESSAGE_EDIT))
            {
              commands.Edit().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Редактировать" });
              commands.Delete().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Удалить" });
            }
          }).Width(90).Title("");
            columns.Bound(x => x.MessDate)
            .Width(100);
            columns.Bound(x => x.MessageText)
                .Encoded(false)
                .EditorTemplateName("MessageTextTemplate");
        })
        .ClientEvents(events => events
            .OnError("onGridErrorEvent")            
         )
//        .Editable(editing => editing.Mode(GridEditMode.InLine))
//        .Pageable(paging => paging.PageSize(5))
        .Render();
%>
</asp:Content>
