<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage"%>
<%@ Import Namespace="Store.Core" %>
<%@ Import Namespace="Store.Data" %>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <script type="text/javascript" src="Scripts/Base.js"></script>
    <script type="text/javascript" src="Scripts/Lock.js"></script>
<h3>Списание б/у пецодежды со склада</h3>
    <table border="0">
    <tr>
        <td valign="middle" width="60px">
            Склад:
        </td>
        <td>
            <%:
            Html.Telerik().DropDownList()
                .Name("StorageNameList")
                .BindTo((SelectList)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE_SHOP_NAME])
                .HtmlAttributes(new { style = "width: 400px" })
                .ClientEvents(events => events
                    .OnChange("updateStorageGrid")
                )
             %>
        </td>
    </tr>
    </table>

    <div id="contentWorkerCard">
        Номенклатурный №:
        <input class="t-input" style="vertical-align: middle;" type="text" id="externalCode" maxlength="20" size="14"/>
        <input type="button" value="Найти" class="t-button" onclick="updateStorageGrid()"/>
<%    
    Html.Telerik().Grid<MoveStorageOutSimple>()
        .Name("WorkerNorma")
        .ToolBar(toolBar =>
            {
                if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                    HttpContext.Current.User.IsInRole(DataGlobals.ROLE_STORAGE_MOVE_OUT_EDIT))
                {
                    toolBar.SubmitChanges();
                    toolBar.Template(() =>
                        {
                         %><table width="100%" border="0" cellpadding="0" cellspacing="0">
                         <tr>
                             <td>
                                <a class="t-button t-grid-save-changes" href="#">Сохранить изменения</a>
                                <a class="t-button t-grid-cancel-changes" href="#">Отменить изменения</a>                            
                                <label for="docNum" >№ документа:</label>
                                <input class="t-input" style="vertical-align: middle;" type="text" id="docNum" value="" size="8"/>
                                <label for="docDate">Дата документа:</label>
                                <%= Html.Telerik().DatePicker()
                                        .Name("docDate")
                                        .HtmlAttributes(new { id = "docDate_wrapper", style = "vertical-align: middle;" })
                                        .InputHtmlAttributes(new {size = "4"}) // не работает
                                        .Value(DateTime.Today)
                                        .Format(DataGlobals.DATE_FORMAT_FULL_YEAR)
                                %>
                                </td>
                                <td width="150" style="text-align:center">
                                    <a class="t-button" id="cancelOper" onclick="onCancelOper()" href="#">Отмена операции</a>
                                </td>
                            </tr>
                            </table>
                         <%
                        });
                }
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
              .Select("Select", "MoveStorageOut")
              .Insert("Save", "MoveStorageOut")
              .Update("Update", "MoveStorageOut")
              .Delete("Delete", "MoveStorageOut");
        })
        .Columns(columns =>
        {
            columns.Bound(x => x.GroupName)
            .Width(230)
            .Title("Группа номенклатур")
            .ReadOnly();
            columns.Bound(x => x.Nomenclature)
              .Encoded(false)
              .Title("[Код SAP] Номенклатура (размер, рост, износ, кол-во)")              
              .ReadOnly();
            columns.Bound(x => x.Unit)
                .Width(100)
                .Title("Ед. изм.")
                .Sortable(false)
                .ReadOnly();
            columns.Bound(x => x.Quantity)
                .ReadOnly()
                .Sortable(false)
                .Width(80)
                .Title("Кол-во");
            columns.Bound(x => x.OutQuantity)
                .Width(90)
                .Sortable(false)
                .Title("Списать");
        })
        .ClientEvents(events => events
            .OnError("onError")
            .OnDataBound("onDataBound")
            .OnSubmitChanges("onSubmit")
            .OnDataBinding("dataBinding"))
        .Editable(editing => editing.Mode(GridEditMode.InCell))
        .Scrollable(x => x.Height(400))
        .Sortable()
        .Render();
%>

  </div>
<script type="text/javascript">
  function updateStorageGrid() {
      var storageNameId = $("#StorageNameList").data("tDropDownList").value();
      if (storageNameId != "") {
          $("#WorkerNorma").data("tGrid").rebind();
      }
  }

  function dataBinding(args) {
      var storageNameId = $("#StorageNameList").data("tDropDownList").value();
      if (nkmk.Lock.isFree()) {
          nkmk.Lock.setBusy();
          nkmk.Lock.printBusyMessage("Идет обработка данных...");
      }
      var docNum = document.getElementById("docNum").value;
      var docDate = document.getElementById("docDate").value;
      var externalCode = document.getElementById("externalCode").value;
      args.data = $.extend(args.data, { storageNameId: storageNameId, docNumber: docNum, docDate: docDate, externalCode: externalCode });
  }

  function onSubmit(e) {
      var docNum = document.getElementById("docNum").value;
      if (docNum == "") {
          alert("№ документа не задан!");
          return false;
      }
      var docDate = document.getElementById("docDate").value;
      if ((docDate == "") || (docDate.length!=10)) {
          alert("Дата документа не задана или не верный формат!");
          return false;
      }

      if (nkmk.Lock.isFree()) {
          nkmk.Lock.setBusy();
          nkmk.Lock.printBusyMessage("Идет обработка данных...");
      }
      var externalCode = document.getElementById("externalCode").value;
      var storageNameId = $("#StorageNameList").data("tDropDownList").value();
      e.updated[0] = $.extend(e.updated[0], { storageNameId: storageNameId, DocNumber: docNum, DocDate: docDate, externalCode: externalCode});
  }

  function onCancelOper() {
      var docNum = document.getElementById("docNum").value;
      if (docNum == "") {
          alert("№ документа не задан!");
          return false;
      }
      var docDate = document.getElementById("docDate").value;
      if ((docDate == "") || (docDate.length != 10)) {
          alert("Дата документа не задана или не верный формат!");
          return false;
      }
      cssConfirm("Вы действительно хотите отменить списание б/у спецодежды по документу №" + docNum + " от " + docDate + "?", "Да", "Нет", function (state) {
          if (state) {

              var externalCode = document.getElementById("externalCode").value;
              var storageNameId = $("#StorageNameList").data("tDropDownList").value();
              var $cancelOper = $('#cancelOper');
              $.post("MoveStorageOut/CancelOper?storageNameId=" + storageNameId + "&docNumber=" + docNum + "&docDate=" + docDate + "&externalCode=" + externalCode, function (data) {
                  var $grid = $("#WorkerNorma").data("tGrid");
                  $grid.rebind();
              });
          }
      });
  }

  function onError(args) {
      nkmk.Lock.setFree();
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
      } else {
          var xhr = args.XMLHttpRequest;
          if (args.textStatus == 'error') {
            var window = $('#Window_alert').data('tWindow');
            window.content(xhr.responseText).center().open();
          }      
      }
 }

function onDataBound(e) {
    nkmk.Lock.setFree();
}
  </script>

</asp:Content>
