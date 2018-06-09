	<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<Store.Core.OperType>>" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<%: 
   Html.Telerik().Grid(Model)
        .Name("OperTypes")
        .ToolBar(commands => commands.Insert().ButtonType(GridButtonType.Image))
        .DataKeys(keys =>
        {
          keys.Add(x => x.Id);
        })
        .DataBinding(dataBinding =>
        {
          dataBinding
            .Ajax()
            .Select("Select", "OperTypes")
            .Insert("Save", "OperTypes")
            .Update("Save", "OperTypes")
            .Delete("Delete", "OperTypes");
        })
        .Columns(columns =>
        {
          columns.Command(commands =>
          {
            if (HttpContext.Current.User.IsInRole("admin") ||
                HttpContext.Current.User.IsInRole("workerEdit"))
            {
                commands.Edit().ButtonType(GridButtonType.Image);
                commands.Delete().ButtonType(GridButtonType.Image);
            }
          }).Width(200).Title("Действия");
          //columns.Bound(x => x.Id).Width(100);
          columns.Bound(x => x.Id).Width(100);
          columns.Bound(x => x.Name);
        })
        .Editable(editing => editing.Mode(GridEditMode.PopUp))
        .Pageable()
        //.Scrollable()
        .Sortable()
        //.Groupable()
        .Filterable()
%>
</asp:Content>
