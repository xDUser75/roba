<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage"%>
<%@ Import Namespace="Store.Core" %>
<%@ Import Namespace="Store.Data" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<h3>Выдача спецодежды по требованию</h3>
    <form id="form1" name="form1" runat="server"> 
    <table border="0">
        <tr>
            <td align="right">Табельный/Ф.И.О</td>
            <td>
            <%= Html.Telerik().ComboBox()
                     .Name("WorkerWorkplaceCombo")
                     .AutoFill(true)
                     .DataBinding(binding => binding.Ajax()
                        .Select("_GetWorkerWorkplaces", "WorkerCards")
                        .Delay(1000)
                        .Cache(false)
                        )
                    .HtmlAttributes(new { @style = "width:700px" })
                    .Filterable(filtering =>
                        filtering
                        .FilterMode(AutoCompleteFilterMode.Contains)
                        .MinimumChars(3)
                    )
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
            %>
            </td>
            <td align="center"><input type="button" value="Найти" class="t-button" onclick="findWorkerCard()"/></td>
        </tr>    
    </table>
    </form>

    <div id="contentWorkerCard" style="display:none">
<%:
   Html.Telerik().Grid<WorkerWorkplace>()
        .Name("WorkerWorkplace")
        .DataBinding(dataBinding =>
        {
            dataBinding
              .Ajax()
              .Select("Select_Worker", "WorkerCards");
        })
        .Columns(columns =>
        {
            columns.Bound(x => x)
              .ClientTemplate("Пол: <#= Worker.Sex != null?\"<b>\"+Worker.Sex.Name+\"</b>\":\"\" #>"
                + "&nbsp;|&nbsp;<#= Worker.NomBodyPartSizes[0] != null?Worker.NomBodyPartSizes[0].NomBodyPart.Name+\": <b>\"+Worker.NomBodyPartSizes[0].SizeNumber+\"</b>\":\"\" #>"
                + "&nbsp;|&nbsp;<#= Worker.NomBodyPartSizes[1] != null?Worker.NomBodyPartSizes[1].NomBodyPart.Name+\": <b>\"+Worker.NomBodyPartSizes[1].SizeNumber+\"</b>\":\"\" #>"
                + "&nbsp;|&nbsp;<#= Worker.NomBodyPartSizes[2] != null?Worker.NomBodyPartSizes[2].NomBodyPart.Name+\": <b>\"+Worker.NomBodyPartSizes[2].SizeNumber+\"</b>\":\"\" #>"
                + "&nbsp;|&nbsp;<#= Worker.NomBodyPartSizes[3] != null?Worker.NomBodyPartSizes[3].NomBodyPart.Name+\": <b>\"+Worker.NomBodyPartSizes[3].SizeNumber+\"</b>\":\"\" #>"
                + "</br>Норма: <#= Organization.NormaOrganization != null?\"<b>\"+Organization.NormaOrganization.Norma.Name+\"</b>\":\"\" #>");
        })
        .HtmlAttributes(new { @class = "attachment-grid" })
        .ClientEvents(events => events
            .OnLoad("HideHeader")
            .OnDataBinding("dataBinding"))
        .Footer(false)
%>
<%: 
   Html.Telerik().Grid<WorkerNorma>()
        .Name("WorkerCardIn")
        .ToolBar(commands => {
            commands.Insert().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Добавить" });
        //    commands.Insert().ButtonType(GridButtonType.Text).ImageHtmlAttributes("style='visible:none;'");
            commands.SubmitChanges();
        })
        .DataKeys(keys =>
        {
          keys.Add(x => x.Id);
        })
        .DataBinding(dataBinding =>
        {
          dataBinding
            //.Server()
            .Ajax()
            .Select("Select", "WorkerCardIns")
            .Insert("Save", "WorkerCardIns")
            .Update("Update", "WorkerCardIns")
            .Delete("Delete", "WorkerCardIns");
        })
        .Columns(columns =>
        {
            //columns.Command(commands =>
            //{
            //    if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
            //        HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_EDIT))
            //    {
            //        //commands.Edit().ButtonType(GridButtonType.Image).HtmlAttributes(new { style = "display:none" }).ImageHtmlAttributes(new { title = "Редактировать" });
            //        commands.Edit().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Редактировать" });
            //        commands.Delete().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Удалить" });
            //    }
            //}).Width(90).Title("");
         columns.Bound(x => x.StorageInfo)
           .EditorTemplateName("StorageNomTemplate")
           //.ClientTemplate("<#= WorkerCardContent.Storage.StorageInfo #>")
           .Title("[Код SAP] Номенклатура (размер, рост, износ, цена, кол-во)");
           //.Width(400);
         columns.Bound(x => x.PutQuantity)
             .Width(100)
             .Title("Выдать");
         columns.Bound(x => x.PresentQuantity)
             .Width(100)
             .Title("На руках")
             .ReadOnly();
         columns.Bound(x => x.ReceptionDate)
             .Format("{0:" + DataGlobals.DATE_FORMAT + "}")
             .Width(100)
             .Title("Выдано")
             .ReadOnly();
         //columns.Bound(x => x.NormaContentId).Hidden();
         columns.Bound(x => x.StorageId).Hidden();
         })
        .ClientEvents(events => events
            .OnEdit("onEdit")
            .OnError("onError")
            //.OnSave("onSave")
            .OnDataBinding("dataBinding"))
        .Editable(editing => editing.Mode(GridEditMode.InCell))
        //.Pageable(x => x.PageSize(10))
        //.Sortable()
        //.Groupable()
        //.Filterable()
        //.TableHtmlAttributes("width='70%'")
        .Scrollable(x => x.Height(400))
%>
</div>
<script type="text/javascript">
//  var normaContentId;
  function onEdit(e) {
//      normaContentId = e.dataItem['NormaContentId'];
//      alert(NormaContentId);
//    if (e.dataItem != null) {
//      var obj = e.dataItem['WorkerCard.Storage.Nomenclature'];
//      $(e.form).find('#WorkerCard_Storage_Nomenclature').data('tComboBox').value((obj == null) ? -1 : obj.Fio);
//    }
//      var $combo = $(e.cell).find('#WorkerCardContent.Storage.Nomenclature');
//      if ($combo.length > 0) {
//          var combo = $combo.data('tComboBox');
//          combo.fill(function () {
//              combo.value(e.dataItem['WorkerCardContent.Storage.Nomenclature'].EmployeeID)
//          });
//      }
  }
  function findWorkerCard() {
      var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
      if (workerWorkplaceId != "") {
          $("#WorkerWorkplace").data("tGrid").rebind();
          $("#WorkerCardIn").data("tGrid").rebind();
          document.getElementById("contentWorkerCard").style.display = "block";
      }
  }
  function dataBinding(args) {
      var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
      args.data = $.extend(args.data, { workerWorkplaceId: workerWorkplaceId });
  }
  function onSave(args) {
      var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
      args.data = $.extend(args.data, { workerWorkplaceId: workerWorkplaceId });
  }
  function onStorageComboBoxDataBinding(e) {
      //var nomGroupId = e.row.cells[1].innerHTML;
      //alert(nomGroupId);
      //e.data = $.extend({}, e.data, { normaContentId: normaContentId });
  }
  function HideHeader() {
      $('.attachment-grid .t-header').hide();
  };

  function onError(args) {
      if (args.textStatus == "modelstateerror" && args.modelState) {
          var message = "Errors:\n";
          $.each(args.modelState, function (key, value) {
              if ('errors' in value) {
                  $.each(value.errors, function () {
                      message += "["+key+"] "+this + "\n";
                  });
              }
          });
          args.preventDefault();
          alert(message);
      }
  }
  </script>
</asp:Content>
