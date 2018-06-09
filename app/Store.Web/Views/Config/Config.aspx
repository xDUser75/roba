<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import Namespace="Store.Data" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<%: 
    Html.Telerik().Grid<Store.Core.Config>()
        .Name("Configs")
        .ToolBar(commands => {
            if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) || HttpContext.Current.User.IsInRole(DataGlobals.ROLE_CONFIG_EDIT))
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
            dataBinding.Ajax()
              .Select("Select", "Config")
              .Insert("Save", "Config")
              .Update("Save", "Config")
              .Delete("Delete", "Config");
        })
        .Columns(columns =>
        {
            columns.Command(commands =>
            {
                if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) || HttpContext.Current.User.IsInRole(DataGlobals.ROLE_CONFIG_EDIT))
                {
                    commands.Edit().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Редактировать" });
                    commands.Delete().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Удалить" });
                }
            }).Width(90).Title("");
            columns.Bound(x => x.ParamName);
            columns.Bound(x => x.ParamValue);
            columns.Bound(x => x.Description);
            columns.Bound(x => x.OrganizationId).Hidden();
        })
        .Editable(editing => editing.Mode(GridEditMode.InLine))
        //.Pageable()
        .Scrollable(x => x.Height(300))
        .Sortable()
        .Filterable()
%>
</asp:Content>
