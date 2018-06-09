<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Store.Data" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<span id="ErrorMessage"></span>
<h4>ГРУППЫ номенклатур</h4>
<% 
    Html.Telerik().Grid<Store.Core.NomGroup>()
        .Name("Grid_NomGroup")
        .ToolBar(commands => {
                                if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                                    HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NOMGROUP_EDIT))
                                {

                                    commands.Insert().ButtonType(GridButtonType.Image);
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
              .Select("NomGroup_Select", "NomGroups")
              .Insert("NomGroup_Insert", "NomGroups")
              .Update("NomGroup_Update", "NomGroups")
              .Delete("NomGroup_Delete", "NomGroups");
        })
        .Columns(columns =>
        {
            columns.Command(commands =>
            {
                if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                    HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NOMGROUP_EDIT))
                {

                    commands.Edit().ButtonType(GridButtonType.Image);
                    commands.Delete().ButtonType(GridButtonType.Image);
                }
            }).Width(90).Title("");
            columns.Bound(x => x.Name);
            columns.Bound(x => x.NameOT);
            columns.Bound(x => x.IsWinter)
                   .Filterable(true)
                   .Width(70)
                   .ClientTemplate("<input type='checkbox' disabled='true' name='iswinter' <#= (IsWinter)?'checked':'' #> />");            
            columns.Bound(x => x.NomBodyPart).Title("Тип размера")
                .Width(170)
                .Filterable(false)
                .ClientTemplate("<#= (NomBodyPart != null) ? NomBodyPart.Name : '' #>")
                .EditorTemplateName("NomBodyPartTemplate");
            columns.Bound(x => x.IsActive)
                .Filterable(true)
                .Title("Активный")
                .Width(70)
                .ClientTemplate("<input type='checkbox' disabled='true' name='isactive' <#= (IsActive)?'checked':'' #> />");
            columns.Bound(x => x.ExternalCode)
                .Width(120);
            columns.Bound(x => x.Id).Hidden(true);
        })
        .ClientEvents(events => events
            .OnEdit("Grid_NomGroup_onEdit")
            .OnRowSelect("Grid_NomGroup_onRowSelected")
            .OnError("onGridErrorEvent"))
        .Editable(editing => editing.Mode(GridEditMode.InLine))
        //.Pageable()
        .Scrollable(scrolling => scrolling.Height(200))
        .Filterable()
        .Sortable()
        .Selectable()
        .Render();
%>
<br/>
<%
    Html.Telerik().Grid<Store.Core.Nomenclature>()
        .Name("Grid_Nomenclature")
        .ToolBar(commands => commands.Template("Список номенклатур в группе: <span id='groupName'></span> [<span id='groupID'></span>]"))
        .DataKeys(keys =>
        {
            keys.Add(x => x.Id);
        })
        .DataBinding(dataBinding =>
        {
            dataBinding
              .Ajax()
              .Select("Nomenclature_Select", "NomGroups");
        })
        .Columns(columns =>
        {
            columns.Bound(x => x.Name);
            columns.Bound(x => x.Enabled)
                .Filterable(true)
                .Width(60)
                .Title("Закуп")
                .ClientTemplate("<input type='checkbox' disabled='true' name='Enabled' <#= (Enabled)?'checked':'' #> />");

            columns.Bound(x => x.Unit)
                .Filterable(false)
                .Width(70)
                .Title("Ед.изм.")
                .ClientTemplate("<#= (Unit != null) ? Unit.Name : '' #>");
            columns.Bound(x => x.Sex)
                .Filterable(false)
                .Width(70)
                .ClientTemplate("<#= (Sex != null) ? Sex.Name : '' #>");
            columns.Bound(x => x.NomBodyPart)
                .Title("Тип размера")
                .Filterable(false)
                .Width(170)
                .ClientTemplate("<#= (NomBodyPart != null) ? NomBodyPart.Name : '' #>");
            columns.Bound(x => x.IsActive)
                .Filterable(true)
                .Width(70)
                .Title("Активный")
                .ClientTemplate("<input type='checkbox' disabled='true' name='isactive' <#= (IsActive)?'checked':'' #> />");
            //columns.Bound(x => x.StartDate)
            //    .Width(120)
            //    .Format("{0:dd.MM.yy}")
            //    .Title("Ввод");
            //columns.Bound(x => x.FinishDate)
            //    .Width(120)
            //    .Format("{0:dd.MM.yy}")
            //    .Title("Вывод");
            columns.Bound(x => x.ExternalCode)
                .Width(120);
        })
        
        .ClientEvents(events => events
            .OnError("onGridErrorEvent")
            .OnDataBinding("Grid_Nomenclature_binding"))
        .Editable(editing => editing.Mode(GridEditMode.InLine))
        .Scrollable(scrolling => scrolling.Height(120))
        .Sortable()
        .Filterable()
        .Render();
%>

<script type="text/javascript">
    var groupID;
    function Grid_NomGroup_onEdit(e) {
        $(e.form).find('#NomBodyPart')
        .data('tDropDownList')
        .select(function (dataItem) {
            try {
                var obj = e.dataItem['NomBodyPart'];
                return dataItem.Value == ((obj == null) ? -1 : obj.Id);
            } catch (Exception) {
                return false;
            }
        });
    }

    function Grid_NomGroup_onRowSelected(e) {
        var nomgroupGrid = $('#Grid_NomGroup').data('tGrid');
        var nomenclatureGrid = $('#Grid_Nomenclature').data('tGrid');
        var dataItem = nomgroupGrid.dataItem(e.row);
        if (dataItem) {
            groupID = dataItem['Id'];
            var groupName = dataItem['Name'];
            if ($('#groupID').text() != groupID) {
                $('#groupID').text(groupID);
                $('#groupName').text(groupName);
                nomenclatureGrid.rebind({
                    groupID: groupID
                });
            }
        }
    }

    function Grid_Nomenclature_binding(args) {
        args.data = $.extend(args.data, { groupID: groupID });

    }



//    function Grid_Nomenclature_onEdit(e) {
//        $(e.form).find('#Nomenclature')
//        .data('tDropDownList')
//        .select(function (dataItem) {
//            try {
//                var obj = e.dataItem['Nomenclature'];
//                return dataItem.Value == ((obj == null) ? -1 : obj.Id);
//            } catch (Exception) {
//                return false;
//            }
//        });
//    }

</script>

</asp:Content>
