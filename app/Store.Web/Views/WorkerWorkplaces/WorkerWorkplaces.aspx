<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<Store.Core.WorkerWorkplace>>" %>
<%@ Import Namespace="Store.Data" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<h3>Рабочие места</h3>
    <table border="0">
        <tr>
            <td align="right">Табельный/Ф.И.О</td>
            <td><input type="text" size="20" name="param" id="param" /></td>
            <td align="center"><input type="button" value="Найти" class="t-button" onclick="findWorkers()"/></td>
        </tr>    
    </table>
<%: 
   Html.Telerik().Grid(Model)
        .Name("Workplace")
        .ToolBar(commands => commands.Insert().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Добавить" }))
        .DataKeys(keys =>
        {
          keys.Add(x => x.Id);
        })
        .DataBinding(dataBinding =>
        {
          dataBinding
            //.Server()
            .Ajax()
            .Select("Select", "WorkerWorkplaces")
            .Insert("Save", "WorkerWorkplaces")
            .Update("Update", "WorkerWorkplaces")
            .Delete("Delete", "WorkerWorkplaces");
        })
        .Columns(columns =>
        {
          columns.Command(commands =>
          {
            if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_EDIT))
            {
              commands.Edit().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Редактировать" });
              commands.Delete().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Удалить" });
            }
          }).Width(90);
          //columns.Bound(x => x.Id).Width(100);
          columns.Bound(x => x.WorkerTabn)
              //.ClientTemplate("<#= Worker.TabN #>")
              .Title("Таб. №").Width(80).ReadOnly();
         //columns.Bound(x => x.Worker.Fio).Title("Работник");
         //columns.Bound(x => x.Organization.Name).Title("Организация");
         columns.Bound(x => x.Worker)
           .ClientTemplate("<#= Worker.Fio #>")
           .EditorTemplateName("WorkerTemplate")
           .Width(300).Title("Работник");
         columns.Bound(x => x.Organization)
           .ClientTemplate("<#= Organization.FullName #>")
           .EditorTemplateName("OrganizationTemplate")
           .Title("Рабочее место");
        columns.Bound(x => x.IsActive)
            .ClientTemplate("<input type='checkbox' disabled='true'  name='isactive' <#= (IsActive)?'checked':'' #> />")
            .Width(80).Title("Активный");
        })
        .ClientEvents(events => events
            .OnEdit("onEdit")
            .OnDataBinding("dataBinding"))
        .Editable(editing => editing.Mode(GridEditMode.InLine))
        //.Pageable()
        .Scrollable(scroll => scroll.Height(400))
        //.Groupable()
        //.Filterable()
        .Sortable()
%>
<script type="text/javascript">
  function onEdit(e) {
    if (e.dataItem != null) {
      var obj = e.dataItem['Worker'];
      $(e.form).find('#Worker').data('tComboBox').value((obj == null) ? -1 : obj.Id);
      $(e.form).find('#Worker').data('tComboBox').text((obj == null) ? -1 : obj.Fio);

      obj = e.dataItem['Organization'];
      $(e.form).find('#Organization').data('tComboBox').value((obj == null) ? -1 : obj.Id);
      $(e.form).find('#Organization').data('tComboBox').text((obj == null) ? -1 : obj.FullName);
    }
  }

  function findWorkers() {
      var param = document.getElementById('param').value;
      if (param != "") {
          $("#Workplace").data("tGrid").rebind();
      }
  }

  function dataBinding(args) {
      var param = document.getElementById('param').value;
      args.data = $.extend(args.data, { param: param });
  }

//  function onEdit(e) {
//    $(e.form).find('#Worker_Fio').data('tComboBox').select(function (dataItem) {
//      return dataItem.Text == e.dataItem['Worker_Id'];
//    });
//  }
//</script>
</asp:Content>
