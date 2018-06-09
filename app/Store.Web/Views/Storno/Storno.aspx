<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage"%>
<%@ Import Namespace="Store.Data" %>
<%@ Import Namespace="Store.Core" %>
<%@ Import Namespace="Store.Core.Account" %>
<!--#include file="../Shared/GetReportUrl.inc"-->
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <script type="text/javascript" src="Scripts/Base.js"></script>
    <script type="text/javascript" src="Scripts/Lock.js"></script>
<h3>Сторнирование операций</h3>
    <table border="0" width="100%">
        <tr>
            <td align="right">Табельный/Ф.И.О</td>
            <td width="760">
            <%= Html.Telerik().ComboBox()
                     .Name("WorkerWorkplaceCombo")
                     .AutoFill(false)
                     .DataBinding(binding => binding.Ajax()
                        .Select("_FindWorkerWorkPlace", "Storno")
                        .Delay(400)
                        .Cache(false)
                        )
                    .HtmlAttributes(new { @style = "width:750px" })
                    .Filterable(filtering =>
                        filtering
                        .FilterMode(AutoCompleteFilterMode.Contains)
                        .MinimumChars(1)
                    )
                    //.Items(items =>
                    //    {
                    //        if (Session["workerWorkplaceId"] != null)
                    //            items.Add().Value("" + (int)Session["workerWorkplaceId"]).Text((string)Session["workerWorkplaceText"]);
                    //    })
                    .SelectedIndex(0)
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
            %>
            </td>
            <td align="left"><input type="button" value="Найти" class="t-button" onclick="findOperation()"/></td>
        </tr>    
    <tr>
        <td valign="middle" width="60px">
            Склад:
        </td>
        <td colspan="2">
            <%:
            Html.Telerik().DropDownList()
                .Name("StorageNameList")
                .BindTo((SelectList)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE_SHOP_NAME])
                .HtmlAttributes(new { style = "width: 400px" })
                .ClientEvents(events => events
                .OnChange("findOperation")
                )
             %>
        </td>
    </tr>
    <tr>
        <td colspan="3" align="right"><input type="button" value="Сторнировать документ" class="t-button" onclick="showStornoDocumParam()"/></td>
    </tr>
    </table>

    <% Html.Telerik().Window()
           .Name("Window")
           .Title("Сторнировать документ")
           .Width(350)
           .Height(180)
           .Draggable(true)
           .Modal(true)
           .Visible(false)
           .Buttons(b => b.Close())
           .Content(() =>
           {%>
                <div style="text-align:center">
                 <fieldset>
                <legend>Документ</legend>
               <table width="100%">
               <tr>
               <td style="text-align:right">Номер</td>
               <td style="text-align:left">
                   <input name="DocNumberInput" id="DocNumberInput" maxlength="10" size="8"/>
               </td>
               <td style="text-align:right">Дата</td>
               <td style="text-align:left">
                                <%= Html.Telerik().DatePicker()
                                        .Name("docDate")
                                        .HtmlAttributes(new { id = "docDate_wrapper", style = "vertical-align: middle;" })
                                        .InputHtmlAttributes(new {size = "4"}) // Р_Рч С_Р°Р+Р_С'Р°РчС'
                                        .Value(DateTime.Today)
                                        .Format(DataGlobals.DATE_FORMAT_FULL_YEAR)
                                %>

                 </td>
                 </tr>
                 <tr align="left">
                    <td colspan="4">
                        <input type="radio" id="inRadio" onclick="updateState('inRadio');"/> Приход на склад<br/>
                        <input type="radio" id="outRadio" onclick="updateState('outRadio');"/> Cписание со склада<br/>
                        <input type="radio" id="wearRadio" onclick="updateState('wearRadio');"/> Списание со склада б/у номенклатуры
                 </td>
                 </tr>
                 </table>
                 </fieldset>
               </div>
               <br/>            
             <div align="center">
               <table border="0" cellpadding="0" cellspacing="0">
               <tr>
                <td>
                    <input type="button" value="&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Отмена&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" class="t-button" onclick="closeWindowClick('#Window')"/>
                    &nbsp;&nbsp;
                </td>
                <td>
                    &nbsp;&nbsp;
                    <input type="button" value="Сторнировать" class="t-button" id="SaveSapButton" onclick="stornoDocum()"/>
                </td>
               </tr>
               </table>
               </div>
           <%})
           .Render();
    %>


<% 
   Html.Telerik().Grid<OperationSimple>()
        .Name("Operations")
        .DataKeys(keys =>
        {
            keys.Add(x => x.Id);
        })
        .DataBinding(dataBinding => dataBinding
            .Ajax()
            .Select("_SelectOperations", "Storno")
        )
        .Columns(columns =>
        {
            if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
     HttpContext.Current.User.IsInRole(DataGlobals.ROLE_STORNO_EDIT))
            {
                columns.Bound(c => c).ClientTemplate("<img <#= (RefOperationId > 0?\"src=Content/Images/document_lock.gif alt='Сторнировать запрещено'\": \" alt=Сторнировать src=Content/Images/document_delete.gif title=Сторнировать style=cursor:hand onClick=stornoClick(\"+Id+\")\") #> />")
                    .Width(40)
                    .ReadOnly()
                    .Filterable(false)
                    .Sortable(false);
            }
            else {
                columns.Bound(c => c).ClientTemplate("&nbsp;")
                    .Width(40)
                    .ReadOnly()
                    .Filterable(false)
                    .Sortable(false);                
            }
          columns.Bound(x => x.OperDate)
              .Format("{0:" + DataGlobals.DATE_FORMAT + "}")
              .Width(60);
          columns.Bound(x => x.OperType)
            .Title("Oпер.")
            .Width(90);
          columns.Bound(x => x.ShopNumber)
              .Title("Цех")
              .Width(45);
          columns.Bound(x => x.StorageNumber)
              .Title("Склад")
              .Width(45);
          columns.Bound(x => x.Nomenclature)
            .Title("Номенклатура")
            .Width(350);
          columns.Bound(x => x.DocNumber)
              .Title("№док")
/*              .ClientTemplate("<a href=\"javascript:printM11('<#= DocNumber #>', '<#= StringOperDate #>',<#= OperTypeId #>,<#= WorkerWorkPlaceId #>);\"><#= DocNumber != 0?DocNumber:\"\" #></a>")*/
              .ClientTemplate("<a href=\"javascript:printM11(<#= DocNumber #>, '<#=  StringOperDate #>',<#= OperTypeId #>,<#= WorkerWorkPlaceId #>,'" + ((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName + "','" + (HttpContext.Current.Session["_idOrg"].ToString() == DataGlobals.ORG_ID_VGOK) + "');\"><#= DocNumber != 0?DocNumber:\"\" #></a>")

              .Width(80);
          columns.Bound(x => x.Quantity)
              .Title("Кол.")
              .Width(60);
          columns.Bound(x => x.Motiv)
              .Title("Основание");
          columns.Bound(x => x.Wear)
              .HtmlAttributes(new { @align = "center" })
              .ClientTemplate("<#= Wear=='100'?'новая':Wear=='50'?'б/у':Wear=='0'?'утиль':Wear #>")
              .Width(60);
        })
        .ClientEvents(events => events
            .OnDataBinding("dataBinding")
            .OnError("onGridErrorEvent")
        )
        .Scrollable(x => x.Height(400))
        .Render();
%>
<script type="text/javascript">

    function updateState(id) {
        document.getElementById("inRadio").checked = false;
        document.getElementById("outRadio").checked = false;
        document.getElementById("wearRadio").checked = false;
        document.getElementById(id).checked = true;
    }

    function showStornoDocumParam() {
        var window = $("#Window").data("tWindow");
        window.center().open();
    }

    function closeWindowClick(winName) {
        var window = $(winName).data("tWindow");
        window.close();
    }

    function stornoDocum() {
        var dN = document.getElementById("DocNumberInput").value;
        if (dN == "") {
            alert("Не введен номер документа!");
            return false;
        }
        var dD = document.getElementById("docDate").value;
        if (dD == "") {
            alert("Не указана дата документа!");
            return false;
        }

        if (!document.getElementById("inRadio").checked && !document.getElementById("outRadio").checked && !document.getElementById("wearRadio").checked) {
            alert("Не указан тип операции!");
            return false;
        }

        var stornoType = -1;
        if (document.getElementById("inRadio").checked) {
            stornoType = <%:DataGlobals.OPERATION_STORAGE_IN%>;
        }

        if (document.getElementById("outRadio").checked) {
            stornoType = <%:DataGlobals.OPERATION_STORAGE_OUT%>;
        }

        if (document.getElementById("wearRadio").checked) {
            stornoType = <%:DataGlobals.OPERATION_STORAGE_WEAR_OUT%>;
        }
        closeWindowClick('#Window');
            $.ajax({
                url: 'Storno/_StornoExternalDocument',
                contentType: 'application/x-www-form-urlencoded',
                type: 'POST',
                dataType: 'json',
                error: function (xhr, status) {
                    onGridErrorEvent(xhr);
                },
                data: {
                    docNumber: dN,
                    docDate: dD,
                    stornoType: stornoType
                },
                success: function (result) {
                    nkmk.Lock.setFree();
                    if (result.modelState) {
                        var message = "Ошибки:\n";
                        $.each(result.modelState, function (key, value) {
                            if ('errors' in value) {
                                $.each(value.errors, function () {
                                    message += this + "\n";
                                });
                            }
                        });
                        alert(message);
                    } else { 
                        alert("Сторнированеи прошло успешно!");
                    }
                },
                error: function (xhr, str) {
                    nkmk.Lock.setFree();
                    alert("Возникла ошибка: " + xhr.responseCode);
                }
            });
    }

    function findOperation() {
            $("#Operations").data("tGrid").rebind();
    }

    function dataBinding(args) {
        var dropDownList = $("#WorkerWorkplaceCombo").data("tComboBox");
        var workerWorkPlaceId = "";
        if (dropDownList) {
            workerWorkPlaceId = dropDownList.value();
        }
        if (workerWorkPlaceId == "") {
            workerWorkPlaceId = "-1";
        }
        var storageNameId = $("#StorageNameList").data("tDropDownList").value();
        args.data = $.extend(args.data, { workerWorkPlaceId: workerWorkPlaceId, StorageNameId: storageNameId });
    }

    function stornoClick(id) {
        cssConfirm("Вы подтверждаете проведение операции?", "Да", "Нет", function (state) {
            if (state) {

                if (nkmk.Lock.isFree()) {
                    nkmk.Lock.setBusy();
                    nkmk.Lock.printBusyMessage("Идет сторнирование операции...");
                }
                var grid = $("#Operations").data("tGrid");
                var dropDownList = $("#WorkerWorkplaceCombo").data("tComboBox");
                var workerWorkPlaceId = "";
                if (dropDownList) {
                    workerWorkPlaceId = dropDownList.value();
                }
                if (workerWorkPlaceId == "") {
                    workerWorkPlaceId = "-1";
                }
                var storageNameId = $("#StorageNameList").data("tDropDownList").value();
                $.ajax({
                    url: 'Storno/_StornoOper',
                    contentType: 'application/x-www-form-urlencoded',
                    type: 'POST',
                    dataType: 'json',
                    error: function (xhr, status) {
                        onGridErrorEvent(xhr);
                    },
                    data: {
                        id: id,
                        workerWorkPlaceId: workerWorkPlaceId,
                        StorageNameId: storageNameId
                    },
                    success: function (result) {
                        nkmk.Lock.setFree();
                        if (result.modelState) {
                            var message = "Ошибки:\n";
                            $.each(result.modelState, function (key, value) {
                                if ('errors' in value) {
                                    $.each(value.errors, function () {
                                        message += this + "\n";
                                    });
                                }
                            });
                            alert(message);
                        } else { 
                            grid.rebind();
                        }
                    },
                    error: function (xhr, str) {
                        nkmk.Lock.setFree();
                        alert('Возникла ошибка: ' + xhr.responseCode);
                    }
                });
            }
        });
    }
/*
    function printM11(docNumber, docDate, operTypeID, WorkerWorkPlaceId) {
        window.open('<%= getReportUrl("Накладная M11") %>&RptOperTypeID=' + operTypeID + '&DATEN=' + docDate + '&RptParamDocum=' + docNumber + '&RptWorkerWorkplaceId=' + WorkerWorkPlaceId);
    }
*/
        function printM11(docNumber, docDate, operTypeID,  WorkerWorkPlaceId, UserName, isVgork) {
        if (isVgork == "True") {
            window.open('<%= getReportUrl("Требование на выдачу СИЗ") %>&RptUserName=' + escape(UserName) + ' &RptOperTypeID=' + operTypeID + '&DATEN=' + docDate + '&RptParamDocum=' + docNumber + '&RptWorkerWorkplaceId=' + WorkerWorkPlaceId);
        } else {
            window.open('<%= getReportUrl("Накладная M11") %>&RptUserName=' + escape(UserName) + ' &RptOperTypeID=' + operTypeID + '&DATEN=' + docDate + '&RptParamDocum=' + docNumber + '&RptWorkerWorkplaceId=' + WorkerWorkPlaceId);

        }
    }


    window.onresize = onFormResize;

    function onFormResize() {
        $("#StorageNameList").data("tDropDownList").disable();
        $("#StorageNameList").data("tDropDownList").enable();
        $("#WorkerWorkplaceCombo").data("tComboBox").disable();
        $("#WorkerWorkplaceCombo").data("tComboBox").enable();
    }

</script>
</asp:Content>
