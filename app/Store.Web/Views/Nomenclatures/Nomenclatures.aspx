<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Store.Data" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server" style="height:100%">
<!--span id="ErrorMessage"></span-->
<h4>СПРАВОЧНИК номенклатур</h4>
<%:
    Html.Telerik().Grid<Store.Core.Nomenclature>()
        .Name("Grid_Nomenclature")
        .ToolBar(commands => {
                if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                    HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NOMENCLATURE_EDIT))
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
              .Select("Nomenclature_Select", "Nomenclatures")
              .Insert("Nomenclature_Insert", "Nomenclatures")
              .Update("Nomenclature_Update", "Nomenclatures")
              .Delete("Nomenclature_Delete", "Nomenclatures");
        })
        .Columns(columns =>
        {
            columns.Command(commands =>
            {
                if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                    HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NOMENCLATURE_EDIT))
                {
                    commands.Edit().ButtonType(GridButtonType.Image);
                    commands.Delete().ButtonType(GridButtonType.Image);
                }
            }).Width(90).Title("");
            columns.Bound(x => x.Name);
//                .Width(300);
            columns.Bound(x => x.NomBodyPart)
                .Title("Тип разм.")
                .Filterable(false)
                .Width(100)
                .ClientTemplate("<#= (NomBodyPart != null) ? NomBodyPart.Name : '' #>")
                .EditorTemplateName("NomBodyPartTemplate")
                ;
            columns.Bound(x => x.NomBodyPartSize)
                .Title("Размер")
                .Filterable(false)
                .Width(50).ReadOnly()
                .ClientTemplate("<#= (NomBodyPartSize != null) ? NomBodyPartSize.SizeNumber : '' #>")
                //.EditorTemplateName("NomBodyPartSizeTemplate")

                ;
            columns.Bound(x => x.Growth)
                .Title("Рост")
                .Filterable(false)
                .Width(50).ReadOnly()
                .ClientTemplate("<#= (Growth != null) ? Growth.SizeNumber : '' #>")
                //.EditorTemplateName("SapGrowthTemplate")
                ;
            columns.Bound(x => x.NomGroup.ExternalCode)
                .Title("Код")
                .Filterable(true)
                .Width(85) ;
            
            columns.Bound(x => x.NomGroup)
                .Title("Группа")
                .Filterable(false)
                .Width(150)
                .ClientTemplate("<#= (NomGroup != null) ? NomGroup.Name : '' #>")
                .EditorTemplateName("NomGroup4NTemplate")
                ;
            columns.Bound(x => x.Unit)
                .Filterable(false)
                .Width(70)
                .Title("Ед.изм.")
                .ClientTemplate("<#= (Unit != null) ? Unit.Name : '' #>")
                .EditorTemplateName("UnitTemplate")
                ;
            columns.Bound(x => x.Sex)
                .Filterable(false)
                .Width(50)
                .ClientTemplate("<#= (Sex != null) ? Sex.Name : '' #>")
                .EditorTemplateName("SexTemplate")
                ;
            columns.Bound(x => x.IsActive)
                .Filterable(false)
                .Width(60)
                .Title("Активный")
                .ClientTemplate("<input type='checkbox' disabled='true' name='isactive' <#= (IsActive)?'checked':'' #> />");
            columns.Bound(x => x.Enabled)
                .Filterable(false)
                .Width(60)
                .Title("Закуп")
                .ClientTemplate("<input type='checkbox' disabled='true' name='Enabled' <#= (Enabled)?'checked':'' #> />");

            //columns.Bound(x => x.StartDate)
            //    .Width(120)
            //    .Format("{0:dd.MM.yy}")
            //    .Title("Ввод");
            //columns.Bound(x => x.FinishDate)
            //    .Width(120)
            //    .Format("{0:dd.MM.yy}")
            //    .Title("Вывод");
            columns.Bound(x => x.ExternalCode)
                .Width(85);
            columns.Bound(x => x.Id).Hidden(true);
//            columns.Bound(x => x.Organization).Hidden(true);
        })
            .ClientEvents(events => events
                .OnEdit("Grid_Nomenclature_onEdit")
                .OnError("onGridErrorEvent")
                )
        .Editable(editing => editing.Mode(GridEditMode.InLine))
        .Scrollable(scrolling => scrolling.Height(440))
        .Pageable(x => x.PageSize(50))
        .Sortable()
        .Filterable()
        .Resizable(resizing => resizing.Columns(true))
%>

<script type="text/javascript">
    function Grid_Nomenclature_onEdit(e) {
        $(e.form).find('#Unit')
        .data('tDropDownList')
        .select(function (dataItem) {
                var obj = e.dataItem['Unit'];
                return dataItem.Value == ((obj == null) ? -1 : obj.Id);
              });
        $(e.form).find('#Sex')
        .data('tDropDownList')
        .select(function (dataItem) {
                var obj = e.dataItem['Sex'];
                return dataItem.Value == ((obj == null) ? -1 : obj.Id);
                });
        $(e.form).find('#NomBodyPart')
        .data('tDropDownList')
        .select(function (dataItem) {
                var obj = e.dataItem['NomBodyPart'];
                return dataItem.Value == ((obj == null) ? -1 : obj.Id);
        });
//        $(e.form).find('#NomBodyPartSize')
//        .data('tDropDownList')
//        .select(function (dataItem) {
//            var obj = e.dataItem['NomBodyPartSize'];
//            return dataItem.Value == ((obj == null) ? -1 : obj.Id);
//        });
        $(e.form).find('#NomGroup')
        .data('tDropDownList')
        .select(function (dataItem) {
                var obj = e.dataItem['NomGroup'];
                return dataItem.Value == ((obj == null) ? -1 : obj.Id);
                });
        //        .data('tComboBox')
    }

</script>

</asp:Content>
