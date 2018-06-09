<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage"%>
<%@ Import Namespace="Store.Core" %>
<%@ Import Namespace="Store.Data" %>
<%@ Import Namespace="Store.Core.Account" %>

<!--#include file="../Shared/GetReportUrl.inc"-->

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <script type="text/javascript" src="Scripts/Base.js"></script>
    <script type="text/javascript" src="Scripts/Lock.js"></script>
<h3>Возврат спецодежды на склад</h3>
    <table border="0">
        <tr>
            <td align="right">Табельный/Ф.И.О</td>
            <td>
            <%= Html.Telerik().ComboBox()
                     .Name("WorkerWorkplaceCombo")
                     .AutoFill(false)
                     .DataBinding(binding => binding.Ajax()
                        .Select("_GetWorkerWorkplaces", "WorkerCards", new { isActive = false })
                        .Delay(400)
                        //.Cache(false)
                        )
                    //.HtmlAttributes(new { @style = "width:700px", @onkeypress = "onKeyPressWorkerWorkplace()", @onfocus = "onFocusWorkerWorkplace()" })
                    .HtmlAttributes(new { @style = "width:750px" })
                    .Filterable(filtering =>
                        filtering
                        .FilterMode(AutoCompleteFilterMode.Contains)
                        .MinimumChars(1)
                    )
                    //.Items(items =>
                    //  {
                    //    if (Session["workerWorkplaceId"] != null)
                    //        items.Add().Value("" + (int)Session["workerWorkplaceId"]).Text((string)Session["workerWorkplaceText"]);
                    //    })
                    .SelectedIndex(0)
                    //.ClientEvents(events => events.OnChange("onChangeWorkerWorkplace"))
                    .ClientEvents(events => events.OnChange("clearWorkerCard"))
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
            %>
            </td>
            <td align="center"><input id="findButton" type="button" value="Найти" class="t-button" onclick="findWorkerCard()" /></td>
        </tr>    
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
                    .OnChange("findWorkerCard")
                )
             %>
        </td>
    </tr>
    </table>

    <div id="contentWorkerCard" style="display:<%: Session["workerWorkplaceId"] != null?"block":"none" %>">
<%
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
                 + "&nbsp;|&nbsp;Категория: <#= Worker.WorkerCategory != null?\"<b>\"+Worker.WorkerCategory.Name+\"</b>\":\"---\" #>"
                 + "&nbsp;|&nbsp;Группа: <#= Worker.WorkerGroup != null?\"<b>\"+Worker.WorkerGroup.Name+\"</b>\":\"---\" #>"
                 + "</br>Норма: <#= Organization.NormaOrganization != null?\"<b>\"+Organization.NormaOrganization.Norma.Name+\"</b>\":\"\" #>");
         })
         .HtmlAttributes(new { @class = "attachment-grid" })
         .ClientEvents(events => events
             .OnLoad("HideHeader")
             .OnDataBinding("dataBinding"))
         .Footer(false)
         .Render();
%>
<br />
<% 
    Html.Telerik().Grid<WorkerNorma>()
         .Name("WorkerCard")
        //.ToolBar(commands => {
        //    //commands.Insert().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Добавить" });
        //    //commands.Insert().ButtonType(GridButtonType.Text).ImageHtmlAttributes("style='visible:none;'");
        //    if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
        //        HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_CARD_EDIT)||
        //             HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_CARD_RETURN_EDIT))
        //    {
        //        commands.SubmitChanges();
        //    }
        //})
         .ToolBar(toolBar =>
             {
                 if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                     HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_CARD_EDIT)||
                     HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_CARD_RETURN_EDIT))
                 {
                     toolBar.SubmitChanges();
                     toolBar.Template(() =>
                         {
                         %>
                            <a class="t-button t-grid-save-changes" href="#">Сохранить изменения</a>
                            <a class="t-button t-grid-cancel-changes" href="#">Отменить изменения</a>
                            <%=
                                Html.Button("reportM11", "Печать М4 и Личной карточки", HtmlButtonType.Button,
                                      "window.open('" + getReportUrl("Приходный ордер М-4") + "&RptOperTypeID=" + DataGlobals.OPERATION_WORKER_RETURN + "&RptUserName=" + Server.UrlEncode(((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName) + "&RptWorkerWorkplaceId='+$('#WorkerWorkplaceCombo').data('tComboBox').value());" +
                                      "window.open('" + getReportUrl("Вкладыш в личную карточку") + "&RptUserName=" + Server.UrlEncode(((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName) + "&RptOperTypeID=" + DataGlobals.OPERATION_WORKER_RETURN + "&RptWorkerWorkplaceId='+$('#WorkerWorkplaceCombo').data('tComboBox').value());",                                        
                                        new { @class = "t-button"})
                            %>
                            <label for="motiv">Основание:</label>
                            <%= Html.Telerik().DropDownList()
                                    .Name("Motiv")
                                    .HtmlAttributes(new { @style = "vertical-align: middle; width:200px" })
                                    .BindTo(new SelectList((IEnumerable)ViewData[DataGlobals.REFERENCE_MOTIV], "Id", "Name"))
                            %>
                            &nbsp;<label for="operDate">Дата операции</label>
                            <%= Html.Telerik().DatePicker()
                                    .Name("operDate")
                                    .HtmlAttributes(new { id = "operDate_wrapper", style = "vertical-align: middle;" })
                                    .InputHtmlAttributes(new {size = "4"}) // не работает
                                    .Value(DateTime.Today)
                                    .Format(DataGlobals.DATE_FORMAT)
                            %>
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
               .Select("Select", "WorkerCardReturn")
               .Insert("Save", "WorkerCardReturn")
               .Update("Update", "WorkerCardReturn")
               .Delete("Delete", "WorkerCardReturn");
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
             columns.Bound(x => x.StorageNumber).Width(80).Title("Склад").ReadOnly();
             columns.Bound(x => x.StorageInfo)
                 //.EditorTemplateName("StorageNomTemplate")
                 //.ClientTemplate("<#= Storage.StorageInfo #>")
               .Title("[Код SAP] Номенклатура (размер, рост, износ, кол-во)")
               .ReadOnly();
             //.Width(500);
             columns.Bound(x => x.PresentQuantity)
                 .Width(100)
                 .Title("На руках")
                 .ReadOnly();
             columns.Bound(x => x.ReceptionDate)
                 .Format("{0:" + DataGlobals.DATE_FORMAT + "}")
                 .Width(100)
                 .Title("Выдано")
                 .ReadOnly();
             columns.Bound(x => x.PutQuantity)
                 .Width(100)
                 .Title("Возврат");
             columns.Bound(x => x.Wear)
                 .ClientTemplate("<#= Wear==100?\"новая\":Wear==50?\"б/у\":\"\" #>")
                 .EditorTemplateName("WearTemplate")
                 .Width(100)
                 .Title("Износ");
             //columns.Bound(x => x.NormaContentId).Hidden();
             columns.Bound(x => x.StorageId).Hidden();
         })
         .ClientEvents(events => events
             //.OnEdit("onEdit")
             .OnError("onError")
             .OnDataBound("onDataBound")
             //.OnSave("onSave")
             .OnSubmitChanges("onSubmit")
             .OnDataBinding("dataBinding"))
         .Editable(editing => editing.Mode(GridEditMode.InCell))
        //.Pageable(x => x.PageSize(10))
        //.Sortable()
        //.Groupable()
        //.Filterable()
        //.TableHtmlAttributes("width='70%'")
         .Scrollable(x => x.Height(300))
         .Render();
%>
</div>
<script type="text/javascript">
    function clearWorkerCard() {
        $("#WorkerWorkplace").data("tGrid").dataBind(new Array());
        $("#WorkerCard").data("tGrid").dataBind(new Array());
    }
    //  var normaContentId;
    function onFocusWorkerWorkplace(e) {
        //alert($("#WorkerWorkplaceCombo").data("tComboBox").value());
        $("#WorkerWorkplaceCombo").data("tComboBox").selectedIndex = -1;
        //$("#WorkerWorkplaceCombo").data("tComboBox").Value = "";
        document.getElementById("contentWorkerCard").style.display = "none";
    }
  
  function onKeyPressWorkerWorkplace(e) {
    document.getElementById("findButton").disabled = true;
    $("#WorkerWorkplaceCombo").data("tComboBox").selectedIndex = -1;
    document.getElementById("contentWorkerCard").style.display = "none";
  }

  function onChangeWorkerWorkplace(e) {
    var combo = $("#WorkerWorkplaceCombo").data("tComboBox");
    alert(combo.selectedIndex);
    if (combo.selectedIndex != undefined && combo.selectedIndex != -1) {
        combo.selectedIndex = -1;
        document.getElementById("findButton").disabled = false;
    }
  }

  function onEdit(e) {
  }

  function findWorkerCard() {
      $("#WorkerWorkplace").data("tGrid").dataBind(new Array());
      $("#WorkerCard").data("tGrid").dataBind(new Array());
      var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
      var text = $("#WorkerWorkplaceCombo").data("tComboBox").text();
      if (workerWorkplaceId != "" && workerWorkplaceId != text) {
          $("#WorkerWorkplace").data("tGrid").rebind();
          $("#WorkerCard").data("tGrid").rebind();
          document.getElementById("contentWorkerCard").style.display = "block";
      }
  }

  function dataBinding(args) {
      var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
      var workerWorkplaceText = $("#WorkerWorkplaceCombo").data("tComboBox").text();
      var storageNameId = $("#StorageNameList").data("tDropDownList").value();
      args.data = $.extend(args.data, { workerWorkplaceId: workerWorkplaceId, workerWorkplaceText: workerWorkplaceText, storageNameId: storageNameId });
      //args.data = $.extend(args.data, { workerWorkplaceItem: { Value: workerWorkplaceId, Text: workerWorkplaceText, Selected: false} });
  }

  function onSave(e) {
      var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
      var storageNameId = $("#StorageNameList").data("tDropDownList").value();
      e.data = $.extend(e.data, { workerWorkplaceId: workerWorkplaceId, storageNameId: storageNameId });
  }

  function onSubmit(e) {
      if (nkmk.Lock.isFree()) {
          nkmk.Lock.setBusy();
          nkmk.Lock.printBusyMessage("Идет обработка данных...");
      }
      var MotivId = $("#Motiv").data("tDropDownList").value();
      var operDate = document.getElementById("operDate").value;
      e.updated[0] = $.extend(e.updated[0], { MotivId: MotivId, OperDate: operDate });
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
      }
  }

  function onDataBound(e) {
      nkmk.Lock.setFree();
  }
  </script>
</asp:Content>
