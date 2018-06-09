<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage"%>
<%@ Import Namespace="Store.Core" %>
<%@ Import Namespace="Store.Data" %>
<%@ Import Namespace="Store.Core.Account" %>
<!--#include file="../Shared/GetReportUrl.inc"-->
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <script type="text/javascript" src="Scripts/Base.js"></script>
    <script type="text/javascript" src="Scripts/Lock.js"></script>
<h3>Перевод сотрудника</h3>
    <table border="0">
        <tr>
            <td align="right">Табельный/Ф.И.О</td>
            <td>
            <%= Html.Telerik().ComboBox()
                     .Name("WorkerWorkplaceCombo")
                     .AutoFill(false)
                     .Encode(false)
                     .DataBinding(binding => binding.Ajax()
                        .Select("_GetWorkerWorkplaces", "WorkerCardTransfer", new { isActive = true })
                        .Delay(400)
                        )
                    .HtmlAttributes(new { @style = "width:750px" })
                    .Filterable(filtering =>
                        filtering
                        .FilterMode(AutoCompleteFilterMode.Contains)
                        .MinimumChars(2)
                    )
                    .SelectedIndex(0)
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
                    .ClientEvents(events => events.OnChange("clearWorkerCard"))
            %>
            </td>
            <td align="center"><input type="button" value="Найти" class="t-button" onclick="findWorkerCard()"/></td>
        </tr>    
    <tr>
        <td valign="middle" width="145px">
            Текущий склад
        </td>
        <td>
            <%:
            Html.Telerik().DropDownList()
                .Name("StorageNameList")
                .BindTo((SelectList)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE_SHOP_NAME])
                .HtmlAttributes(new { style = "width: 400px" })
             %>
        </td>
    </tr>
    </table>

    <table cellpadding="0" cellspacing="1" border="0">
    <tr id="contentWorkerCard" >
<td colspan="2">
<%
    Html.Telerik().Grid<WorkerWorkplace>()
         .Name("WorkerWorkplace")
         .DataBinding(dataBinding =>
         {
             dataBinding
               .Ajax()
               .Select("Select_Worker", "WorkerCardtransfer");
         })
         .Columns(columns =>
         {
             columns.Bound(x => x)
               .ClientTemplate("<input type='hidden' id='activeWorkerWorkPlace' value='<#= Id #>'/>Пол: <#= Worker.Sex != null?\"<b>\"+Worker.Sex.Name+\"</b>\":\"\" #>"
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
             .OnDataBinding("dataBinding")
             .OnError("onGridErrorEvent"))
         .Footer(false)
         .Render();
%>
</td>
</tr>
<tr id="buttonsRow">
<td colspan="2">
<%if (HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_ADMIN) ||
      HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_WORKER_CARD_EDIT) ||
      HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_WORKER_CARD_OUT_EDIT) ||
      HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_WORKER_CARD_RETURN_EDIT))
  {%>
    <input type="button" value="Перевод" class="t-button" onclick="transferWorkerCard()"/>
    <!--input type="button" value="Возврат" class="t-button" onclick="showParamWindow(0)"/>
    <input type="button" value="Списание" class="t-button" onclick="showParamWindow(1)"/-->
    <input type="button" value="Перевод не по норме" class="t-button" onclick="transferOutWorkerCard()"/>
    <input type="button" value="Печать М11" class="t-button" onclick="printM11(<%= DataGlobals.OPERATION_STORAGE_TRANSFER_IN %>,'','')"/>

    <label for="operDate">Дата операции</label>
    <%= 
        Html.Telerik().DatePicker()
            .Name("operDate")
            .HtmlAttributes(new { id = "operDate_wrapper", style = "vertical-align: middle;" })
            .InputHtmlAttributes(new {size = "4"}) // не работает
            .Value(DateTime.Today)
            .Format(DataGlobals.DATE_FORMAT)
    %>

 <%}%>
</td>
</tr>
<tr>
<td>
<%
    Html.Telerik().Grid<NomenclatureSimple>()
        .Name("WorkerNormaDisable")
        .DataKeys(keys =>
        {
            keys.Add(x => x.Id);
        })
        .DataBinding(dataBinding =>
        {
            dataBinding
              .Ajax()
              .Select("SelectContent", "WorkerCardTransfer", new {isActive = false });
        })
        .Columns(columns =>
        {
            columns.Bound(x => x.Id)
            .ClientTemplate("<input type='checkBox' id='radio<#=Id#>' onClick='checkUpdateStatus(this, <#=Id#>)'/>")
            .Width(30)
            .Title("&nbsp;")
            .ReadOnly();
            
            columns.Bound(x => x.GroupName)
            .Width(150)
            .Title("Наименование групп номенклатур")

            .ReadOnly();

            //columns.Bound(x => x.NormaQuantity)
            //    .Width(70)
            //    .Title("По&nbsp;норме")
            //    .ReadOnly();
            //columns.Bound(x => x.NormaUsePeriod)
            //    .Width(60)
            //    .Title("Период")
            //    .ReadOnly();
            columns.Bound(x => x.StorageNumber).Width(80).Title("Склад").ReadOnly();
            columns.Bound(x => x.Name)
              .EditorTemplateName("StorageNomTemplate")
              .Encoded(false)
              .Title("[Код SAP] Номенклатура (размер, рост, износ, кол-во)");
            columns.Bound(x => x.Quantity)
                .Width(50)
                .Title("Кол.")
                .ReadOnly();
            columns.Bound(x => x.StartDate)
                .Format("{0:" + DataGlobals.DATE_FORMAT + "}")
                .Width(70)
                .Title("Выдано")
                .ReadOnly();
            columns.Bound(x => x.NormaContentId).Hidden();
            columns.Bound(x => x.StorageNameId).Hidden();
        })
        .ClientEvents(events => events
            .OnError("onError")
            .OnDataBound("onDataBoundDisable")
            .OnDataBinding("dataBinding"))
        .Scrollable(x => x.Height(300))
        .Render();
%>
  </td>
  <td>
<%
    DateTime minDt = new DateTime(1,1,1);
    Html.Telerik().Grid<NomenclatureSimple>()
        .Name("WorkerNorma")
        .DataKeys(keys =>
        {
            keys.Add(x => x.Id);
        })
        .DataBinding(dataBinding =>
        {
            dataBinding
              .Ajax()
              .Select("SelectContent", "WorkerCardTransfer", new { isActive = true });
        })
        .Columns(columns =>
        {
            if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_CARD_EDIT) ||
                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_EDIT))
            {
                columns.Bound(x => x.GroupName)
                .ClientTemplate("<div onClick=\"javascript:onNormacontentClick(<#= NormaContentId #>,'<#= GroupName #>')\" style=\"cursor:hand\" title=\"Добавить группу замены\"><#= GroupName #></div>")
                .Width(150)
                .Title("Наименование групп номенклатур")
                .ReadOnly();
            }
            //columns.Bound(x => x.NormaQuantity)
            //    .Width(70)
            //    .Title("По&nbsp;норме")
            //    .ReadOnly();
            //columns.Bound(x => x.NormaUsePeriod)
            //    .Width(60)
            //    .Title("Период")
            //    .ReadOnly();
            columns.Bound(x => x.StorageNumber)
                   .ClientTemplate("<#= StorageNumber != 0? StorageNumber:\"\"#>")
                   .Width(80).Title("Склад").ReadOnly();
            columns.Bound(x => x.Name)
              .EditorTemplateName("StorageNomTemplate")
              .Encoded(false)
              .Title("[Код SAP] Номенклатура (размер, рост, износ, кол-во)");
            columns.Bound(x => x.Quantity)
                .Width(50)
                .Title("Кол.")
                .ClientTemplate("<#= Quantity != 0?Quantity:\"\" #>")
                .ReadOnly();
            columns.Bound(x => x.StartDate)
                .Format("{0:" + DataGlobals.DATE_FORMAT + "}")
                .ClientTemplate("<#= StartDate > 1?$.telerik.formatString('{0:dd.MM.yy}', StartDate):\"\" #>")
                .Width(70)
                .Title("Выдано")
                .ReadOnly();
            columns.Bound(x => x.OperDate)
                .ClientTemplate("<#= OperDate > 1?$.telerik.formatString('{0:dd.MM.yy}', OperDate):\"\" #>")
                .Format("{0:" + DataGlobals.DATE_FORMAT + "}")
                .Width(70)
                .Title("Перевод")
                .ReadOnly();

            columns.Bound(x => x.DocNumber)
//                .ClientTemplate("<a href=\"javascript:printM11(<#= OperType #>,'<#= DocNumber #>', '<#= StartDateStr #>');\"><#= DocNumber != 0?DocNumber:\"\" #></a>")
                .ClientTemplate("<a href=\"javascript:printM11('" + DataGlobals.OPERATION_STORAGE_TRANSFER_IN + "','<#= DocNumber #>', '<#= OperDateStr#>');\"><#= DocNumber != 0?DocNumber:\"\" #></a>")
                .Width(70)
                .Title("№&nbsp;M11")
                .ReadOnly();
            
            columns.Bound(x => x.NormaContentId).Hidden();
        })
        .ClientEvents(events => events
            .OnError("onError")
            .OnDataBound("onDataBound")
            .OnDataBinding("dataBinding"))
        .Scrollable(x => x.Height(300))
        .Render();
%>
  </td>
  </tr>
    </table>
    <% Html.Telerik().Window()
           .Name("Window")
           .Title("Укажите параметры")
           .Width(300)
           .Height(180)
           .Draggable(true)
           .Modal(true)
           .Visible(false)
           .Buttons(b => b.Close())
           .Content(() =>
           {%>
           <fieldset id="documGroup">
        <legend>Документ</legend>
               <table border="0" cellpadding="1" cellspacing="1" width="100%">
               <tr  id="TrRow" style="display:none">
               <td colspan="4" align="center">&nbsp;</td>
               </tr>
               <tr>
               <td align="right">Номер</td>
               <td>
                    <input type="text" id="DocNumber" maxlength="10" size="8"/>
               </td>
               <td align="right">Дата</td>
               <td>
               <%: Html.Telerik().DatePicker()
                      .Name("DocDate")
                      .Value(DateTime.Now.ToShortDateString())
                      .Format(Store.Data.DataGlobals.DATE_FORMAT_FULL_YEAR)
                %>
                </td>
                </tr>
                </table>
</fieldset>       
               <table border="0" cellpadding="5" cellspacing="1" width="100%" id="inTable">
               <tr>
                <td align="right">Кол-во</td>
                <td>
                <%:Html.Telerik().NumericTextBox()
                    .Name("QuantityInput")
                    .EmptyMessage("")
                    .MinValue(1)
                    .MaxValue(5)
                    .DecimalDigits(0)
                    .Spinners(false)
                 %>
                </td>
               </tr>
               </table>
               <input type="hidden" id="hiddenParam"/>
               <br />
               <br />
               <table border="0" cellpadding="0" cellspacing="0" width="100%">
               <tr>
                <td style="text-align:right; width:50%">
                    <input type="button" value="Отмена" class="t-button" onclick="closeWindowClick('#Window')"/>
                    &nbsp;&nbsp;&nbsp;&nbsp;
                </td>
                <td>
                    &nbsp;&nbsp;&nbsp;&nbsp;
                    <input type="button" value="Сохранить" class="t-button" onclick="hideParamWindow()"/>
                </td>
               </tr>
               </table>
           <%})
           .Render();
    %>

<%
    Html.Telerik().Window()
           .Name("NormaWindow")
           .Title("Норма")
           .Width(700)
           .Height(520)
           .Draggable(true)
           .Modal(false)
           .Visible(false)
           .Buttons(b => b.Close())
           .Content(() =>
           { %>
<%:
             Html.Telerik().Grid<Store.Core.NormaNomGroup>()
                       .Name("NormaGrid")
                       .ToolBar(commands => 
                           {
                            if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_EDIT) ||
                                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_APPROVED) ||
                                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_CARD_EDIT))
                            {

                                commands.Insert().ButtonType(GridButtonType.Image).HtmlAttributes(new { title = "Добавить" });
                            }
                           })
                       .DataKeys(keys =>
                        {
                            keys.Add(o => o.Id);
                        })
                       .Columns(columns =>
                        {
                            columns.Command(commands =>
                            {
                                if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) || HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_APPROVED) ||
                                   HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_EDIT)
                                    || HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_CARD_EDIT))
                                {

                                    commands.Edit().ButtonType(GridButtonType.Image).HtmlAttributes(new { title = "Изменить", style = "DISPLAY:inline" });
                                    commands.Delete().ButtonType(GridButtonType.Image).HtmlAttributes(new { title = "Удалить", style = "DISPLAY:inline" });

                                }
                            }).Width(100);
                            columns.Bound(o => o.NomGroup).ClientTemplate("<#= NomGroup.Name #>").Title("Группы замены");
                        })
                     .ClientEvents(events => events.OnEdit("onEditNomGroup")
                                                   .OnError("onError")
                                                   )
                     .DataBinding(dataBinding => dataBinding.Ajax()
                                 .Select("_Selection_NormaNomGroups", "Normas")
                                 .Insert("InsertNormaNomGroup", "Normas")
                                 .Update("SaveNormaNomGroup", "Normas")
                                 .Delete("DeleteNormaNomGroup", "Normas"))
                     .Editable(editing => editing.Mode(GridEditMode.InLine))
        .Scrollable(x => x.Height(300))
        .Resizable(resizing => resizing.Columns(true))
        .Sortable()
        .Selectable()
        .Filterable()

      				%>
 	<%	})
		 .Render();
				 %>

<script type="text/javascript">
    var normaContentId;

    function clearWorkerCard() {
        $("#WorkerWorkplace").data("tGrid").dataBind(new Array());
        $("#WorkerNorma").data("tGrid").dataBind(new Array());
        $("#WorkerNormaDisable").data("tGrid").dataBind(new Array());
    }

    function findWorkerCard() {
        clearWorkerCard();
        var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
          var text = $("#WorkerWorkplaceCombo").data("tComboBox").text();
          if (workerWorkplaceId != "" && workerWorkplaceId != text) {
            $("#WorkerWorkplace").data("tGrid").rebind();
            $("#WorkerNorma").data("tGrid").rebind();
            $("#WorkerNormaDisable").data("tGrid").rebind();
        }
    }

    function dataBinding(args) {
        var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
        var workerWorkplaceText = $("#WorkerWorkplaceCombo").data("tComboBox").text();
        var storageNameId = $("#StorageNameList").data("tDropDownList").value();
        args.data = $.extend(args.data, { workerWorkplaceId: workerWorkplaceId, workerWorkplaceText: workerWorkplaceText, storageNameId: storageNameId });
    }
    
    function HideHeader() {
        $('.attachment-grid .t-header').hide();
    }

    function onError(args) {
        nkmk.Lock.setFree();
        if (args.textStatus == "modelstateerror" && args.modelState) {
            var message = "Ошибки:\n";
            $.each(args.modelState, function (key, value) {
                if ('errors' in value) {
                    $.each(value.errors, function () {
                        message += key + this + "\n";
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

    function onDataBoundDisable(e) {
        var grid = $("#WorkerNormaDisable").data("tGrid");
        var rows = grid.data;
        if ((rows) && (rows.length > 0)) {
            document.getElementById("buttonsRow").style.display = "";
        } else {
            //document.getElementById("buttonsRow").style.display = "none";
        }
        nkmk.Lock.setFree();
    }

    window.onresize = onFormResize;

    function onFormResize() { 
        $("#StorageNameList").data("tDropDownList").disable();
        $("#StorageNameList").data("tDropDownList").enable();
        $("#WorkerWorkplaceCombo").data("tComboBox").disable();
        $("#WorkerWorkplaceCombo").data("tComboBox").enable();
    }

    function getAllCheckedRow() {
        var list = "";
        var messFlag = 0;
        var row = 0;
        //Ищем все отмеченные строки
        var grid = $("#WorkerNormaDisable").data("tGrid");
        var storageNameId = $("#StorageNameList").data("tDropDownList").value();
            var rowStorageNameId = 0;

        var rows = grid.data;
        if (rows) {
            for (i = 0; i < rows.length; i++) {
                if (document.getElementById("radio" + rows[i].Id).checked) {
                    if (list != "") list = list + ",";
                    list = list + rows[i].Id;
                }
                if (storageNameId != rows[i].StorageNameId) 
                {
                    messFlag = 1;

                }
            }
        }
        if (messFlag == 1)                  
            alert('ВНИМАНИЕ! Вы переводите позиции, которые выданы в ДРУГОМ салоне');

        return list;
    }

    function getAllRow() {
        var list = "";
        var messFlag = 0;
        var row = 0;
        //Берем все строки
        var grid = $("#WorkerNormaDisable").data("tGrid");
        var storageNameId = $("#StorageNameList").data("tDropDownList").value();
        var rowStorageNameId = 0;

        var rows = grid.data;
        if (rows) {
            for (i = 0; i < rows.length; i++) {
                if (storageNameId != rows[i].StorageNameId) 
                    messFlag = 1;
                if (list != "") list = list + ",";
                list = list + rows[i].Id;

            }
        }
        if (messFlag == 1)
            alert('ВНИМАНИЕ! Вы переводите позиции, которые выданы в ДРУГОМ салоне');

        return list;
    }

    //Перевод
    function transferWorkerCard() {
        var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
        var storageNameId = $("#StorageNameList").data("tDropDownList").value();
        var workerStorage = $("#WorkerWorkplace").data("tGrid");
        var dataItem = workerStorage.data[0];
        var workerStorageId = dataItem.StorageId;
//        var listId = getAllCheckedRow();
        var operDate = document.getElementById("operDate").value;

        if (storageNameId != workerStorageId) {
            if (!window.confirm(dataItem.WorkerTabn + " " + dataItem.Worker.Fio + " Прикреплен к складу " + dataItem.StorageNumber + ". Ответственность перевода позиций лежит на ВАС. Перевести позиции?"))
                return false;
        }

        var listId = getAllRow();
        if (listId == "") {
            alert("Номенклатура не выбрана!");
            return false;
        }
        if (!window.confirm("Вы подтверждаете перевод всех номенклатур, соответсвующих НОВОЙ НОРМЕ, на активное рабочее место?")) {
            return false;
        }
        if (nkmk.Lock.isFree()) {
            nkmk.Lock.setBusy();
            nkmk.Lock.printBusyMessage("Идет обработка данных...");
        }

        $.ajax({
            url: 'WorkerCardTransfer/_TransferWorkerCard',
            contentType: 'application/x-www-form-urlencoded',
            type: 'POST',
            dataType: 'json',
            error: function (xhr, status) {
                nkmk.Lock.setFree();
                onGridErrorEvent(xhr);
            },
            data: {
                workerWorkPlaceId: workerWorkplaceId,
                storageNameId: storageNameId,
                listId: listId,
                outNorma: false,
                OperDate: operDate
            },
            success: function (result) {
                nkmk.Lock.setFree();
                if ((result) && (result.Status == "Error")) {
                    var message = "Ошибки:\n" + result.Message;
                    alert(message);
                } 
                    $("#WorkerNorma").data("tGrid").rebind();
                    $("#WorkerNormaDisable").data("tGrid").rebind();
            }
        });
    }

    //Перевод
    function transferOutWorkerCard() {
        var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
        var storageNameId = $("#StorageNameList").data("tDropDownList").value();
        var operDate = document.getElementById("operDate").value;

        var workerStorage = $("#WorkerWorkplace").data("tGrid");
        var dataItem = workerStorage.data[0];
        var workerStorageId = dataItem.StorageId;
        if (storageNameId != workerStorageId) {
            if (!window.confirm(dataItem.WorkerTabn + " " + dataItem.Worker.Fio + " Прикреплен к складу " + dataItem.StorageNumber + ". Ответственность перевода позиций лежит на ВАС. Перевести позиции?"))
                return false;
        }

        var listId = getAllCheckedRow();
        if (listId == "") {
            alert("Номенклатура для перевода не выбрана!");
            return false;
        }
        if (!window.confirm("Вы подтверждаете перевод номенклатур, НЕ соответсвующих НОВОЙ НОРМЕ, на активное рабочее место?")) {
            return false;
        }
        if (nkmk.Lock.isFree()) {
            nkmk.Lock.setBusy();
            nkmk.Lock.printBusyMessage("Идет обработка данных...");
        }

        $.ajax({
            url: 'WorkerCardTransfer/_TransferWorkerCard',
            contentType: 'application/x-www-form-urlencoded',
            type: 'POST',
            dataType: 'json',
            error: function (xhr, status) {
                nkmk.Lock.setFree();
                onGridErrorEvent(xhr);
            },
            data: {
                workerWorkPlaceId: workerWorkplaceId,
                storageNameId: storageNameId,
                listId: listId,
                outNorma: true,
                OperDate: operDate
            },
            success: function (result) {
                nkmk.Lock.setFree();
                if ((result) && (result.Status == "Error")) {
                    var message = "Ошибки:\n" + result.Message;
                    alert(message);
                } 
                    $("#WorkerNorma").data("tGrid").rebind();
                    $("#WorkerNormaDisable").data("tGrid").rebind();
            }
        });
    }



    //Возврат номенклатуры
    function returnWorkerCard() {
        selRowId = getAllCheckedRow();
        if (selRowId == "") {
            alert("Номенклатура не выбрана!");
            return false;
        }
        var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
        var storageNameId = $("#StorageNameList").data("tDropDownList").value();
        if (nkmk.Lock.isFree()) {
            nkmk.Lock.setBusy();
            nkmk.Lock.printBusyMessage("Идет обработка данных...");
        }
        count = $("#QuantityInput").data("tTextBox").value();
        $.ajax({
            url: 'WorkerCardTransfer/_ReturnWorkerCard',
            contentType: 'application/x-www-form-urlencoded',
            type: 'POST',
            dataType: 'json',
            error: function (xhr, status) {
                nkmk.Lock.setFree();
                onGridErrorEvent(xhr);
            },
            data: {
                workerCardContentId: selRowId,
                storageNameId: storageNameId,
                quantity: count
            },
            success: function (result) {
                nkmk.Lock.setFree();
                if (result.Status == "Error") {
                    alert(result.Message);
                } else {
                    $("#WorkerNormaDisable").data("tGrid").rebind();
                }
            }
        });
    }

    //Списание номенклатуры
    function debitWorkerCard() {
        selRowId = getAllCheckedRow();
        if (selRowId == "") {
            alert("Номенклатура не выбрана!");
            return false;
        }
        var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
        var storageNameId = $("#StorageNameList").data("tDropDownList").value();
        if (nkmk.Lock.isFree()) {
            nkmk.Lock.setBusy();
            nkmk.Lock.printBusyMessage("Идет обработка данных...");
        }
        docDate = document.getElementById("DocDate").value;
        docNumber = document.getElementById("DocNumber").value;
        count = $("#QuantityInput").data("tTextBox").value();
        $.ajax({
            url: 'WorkerCardTransfer/_DebitWorkerCard',
            contentType: 'application/x-www-form-urlencoded',
            type: 'POST',
            dataType: 'json',
            error: function (xhr, status) {
                nkmk.Lock.setFree();
                onGridErrorEvent(xhr);
            },
            data: {
                workerCardContentsId: selRowId,
                storageNameId: storageNameId,
                docDate: docDate,
                docNumber:docNumber,
                quantity: count
            },
            success: function (result) {
                nkmk.Lock.setFree();
                if (result.Status=="Error") {
                    alert(result.Message);
                } else {
                    $("#WorkerNormaDisable").data("tGrid").rebind();
                }
            }
        });
    }

    function getQuantityByIdRow(rowId) {
        var count = 5;
        var grid = $("#WorkerNormaDisable").data("tGrid");
        var rows = grid.data;
        if (rows) {
            for (i = 0; i < rows.length; i++) {
                if (rows[i].Id == rowId) {
                    return rows[i].Quantity;
                }
            }
        }
        return count;
    }

    function showParamWindow(param) {
        selRowId = getAllCheckedRow();
        if (selRowId == "") {
            alert("Номенклатура не выбрана!");
            return false;
        }
        if (selRowId.indexOf(",") > 0) {
            alert("Для этой операции вожможно выбрать одновременно только 1 номенклатуру!");
            return false;
        }
        if (param == 0) {
            document.getElementById("documGroup").style.display = "none";
        } else {
            document.getElementById("documGroup").style.display = "block";
        }
        var mVal = getQuantityByIdRow(selRowId);
        var quantityObject = $("#QuantityInput").data("tTextBox");
        quantityObject.maxValue = mVal;
        document.getElementById("hiddenParam").value = param;
        var numericTextBox = $("#NumericTextBox").data("tTextBox");
        var window = $("#Window").data("tWindow");
        window.center().open();
    }

    function hideParamWindow() {
        var quantityObject = $("#QuantityInput").data("tTextBox");
        if (quantityObject.value() == null) {
            alert("Укажите кол-во!");
            return false;
        }

        if (document.getElementById("documGroup").style.display == "block") {
            if (document.getElementById("DocNumber").value=="") {
                alert("Номер документа не задан!");
                return false;
            }
        }
        var window = $("#Window").data("tWindow");
        window.center().close();
        var param = document.getElementById("hiddenParam").value;
        if (param == 0) {
            returnWorkerCard();
        } else {
            debitWorkerCard();
        }
    }

    function closeWindowClick(winName) {
        var window = $(winName).data("tWindow");
        window.close();
    }

    function checkUpdateStatus(obj, rowId) {
        var storageNameId = $("#StorageNameList").data("tDropDownList").value();
        if (obj.checked) {
            var rowStorageNameId = 0;
            var grid = $("#WorkerNormaDisable").data("tGrid");
            var rows = grid.data;
            if (rows) {
                for (i = 0; i < rows.length; i++) {
                    if (rows[i].Id == rowId) {
                        rowStorageNameId = rows[i].StorageNameId;
                    }
                }
            }

            if (storageNameId != rowStorageNameId) {
                if (!window.confirm("Внимание!!!\nВыбранная номенклатура принадлежит другому складу!\nВы подтверждаете добавление ее в список на перенос на Ваш склад?"))
                    obj.checked = false;
            }
        }
    }

    function printM11(operType, docNumber, docDate) {
        var dopParam = "";
        if (docNumber != "") {
            dopParam = "&DATEN=" + docDate + "&RptParamDocum=" + docNumber;
        }
        var WorkerWorkPlaceId = "";

        if (document.getElementById("activeWorkerWorkPlace")) {
            dopParam = dopParam + "&RptWorkerWorkplaceId=" + document.getElementById("activeWorkerWorkPlace").value;
        }
        window.open('<%= getReportUrl("Накладная M11")%>&RptOperTypeID=' + operType + '&RptUserName=<%=Server.UrlEncode(((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName)%>' + dopParam);
    }

    function onNormacontentClick(normaContentId, normaContentName) {
        var normaNomGroupsGrid = $('#NormaGrid').data('tGrid');
        var grid = document.getElementById("NormaNomGroups");
        normaNomGroupsGrid.rebind({
            NormaContentId: normaContentId
        });

        var window = $("#NormaWindow").data("tWindow");
        window.title('Группы замены для ' + normaContentName);
        window.center().open();

    }
    function onEditNomGroup(e) {
        if (e.dataItem != null) {
            var obj = e.dataItem['NomGroup'];
            $(e.form).find('#NomGroup').data('tComboBox').value((obj == null) ? "" : obj.Id);
            $(e.form).find('#NomGroup').data('tComboBox').text((obj == null) ? "" : obj.Name);
        }
    }

  </script>

</asp:Content>
