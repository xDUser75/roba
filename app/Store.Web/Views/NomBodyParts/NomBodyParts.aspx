<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<Store.Core.NomBodyPart>>" %>
<%@ Import Namespace="Store.Data" %>
<%@ Import Namespace="Store.Core" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<%: 
    Html.Telerik().Grid(Model)
                .Name("NombodyParts")
                .ToolBar(commands => commands.Insert().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Добавить" }))
        .DataKeys(keys =>
        {
            keys.Add(x => x.Id);
        })
        .DataBinding(dataBinding =>
        {
            dataBinding.Ajax()
              .Select("Select", "NomBodyParts")
              .Insert("Save",   "NomBodyParts")
              .Update("Save",   "NomBodyParts")
              .Delete("Delete", "NomBodyParts");
        })
        .Columns(columns =>
        {
            if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NOM_BODY_PART_EDIT))
            {

                columns.Command(commands =>
                {
                    commands.Edit().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title= "Редактировать" });
                    commands.Delete().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Удалить" }); 
                }).Width(120).Title("Действия");
            }
            columns.Bound(x => x.Name);
            columns.Bound(x => x.Id).Hidden();            
        })
        .Editable(editing => editing.Mode(GridEditMode.InLine))
        .Pageable()
        .Scrollable()
%>
</asp:Content>
