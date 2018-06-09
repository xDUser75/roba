<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage"%>
<%@ Import Namespace="Store.Core" %>
<%@ Import Namespace="Store.Data" %>
<%@ Import Namespace="Store.Core.Account" %>

<!--#include file="../Shared/GetReportUrl.inc"-->

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <script type="text/javascript" src="Scripts/Base.js"></script>
    <script type="text/javascript" src="Scripts/Lock.js"></script>
<h3>Выдача спецодежды</h3>
    <table border="0">
        <tr>
            <td align="right">Табельный/Ф.И.О</td>
            <td>
            <%= Html.Telerik().ComboBox()
                     .Name("WorkerWorkplaceCombo")
                     .AutoFill(false)
                     .Encode(false)
                     .DataBinding(binding => binding.Ajax()
                        .Select("_GetWorkerWorkplaces", "WorkerCards", new { isActive = true })
                        .Delay(400)
                    //.Cache(false)
                        )
                    .HtmlAttributes(new { @style = "width:750px" })
                    .Filterable(filtering =>
                        filtering
                        .FilterMode(AutoCompleteFilterMode.Contains)
                        .MinimumChars(1)
                    )
                   //.Items(items =>
                   //     {
                   //         if (Session["workerWorkplaceId"] != null)
                   //             items.Add().Value("" + (int)Session["workerWorkplaceId"]).Text((string)Session["workerWorkplaceText"]);
                   //     })
                    .SelectedIndex(0)
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
                    .ClientEvents(events => events.OnChange("clearWorkerCard")
                                                  .OnError("onGridErrorEvent"))

            %>
            </td>
            <td align="center">
                <input type="button" value="Найти" class="t-button" onclick="findWorkerCard()"/>
                <input type="hidden" value="" name="workerStorageId"/>
            </td>
        </tr>    
    <tr>
        <td valign="middle" width="60px">
            Склад:
        </td>
        <td>
            <%:
            Html.Telerik().DropDownList()
                .Name("StorageNameList")
                //.BindTo(new SelectList((IEnumerable)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE_SHOP_NAME], "Id", "Name"))
                .BindTo((SelectList)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE_SHOP_NAME])
                //.SelectedIndex(Session["storageNameId"] != null?((IList<StorageName>)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE_SHOP_NAME]).IndexOf(new StorageName(int.Parse((string)Session["storageNameId"]))):0)
                .HtmlAttributes(new { style = "width: 400px" })
                .ClientEvents(events => events
                    .OnChange("findWorkerCard")
                )
             %>
             &nbsp;&nbsp; &nbsp;&nbsp;
            <%=
                 Html.Button("reportPropusk", "Разовый пропуск", HtmlButtonType.Button,
                "window.open('" + getReportUrl("Разовый материальный пропуск") + "&RptUserName=" + Server.UrlEncode(((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName) + "&RptWorkerWorkplaceId='+$('#WorkerWorkplaceCombo').data('tComboBox').value());",
                    new { @class = "t-button"})
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
               .ClientTemplate("<#= Worker.isActive == false?\"<font color='red'><b>УВОЛЕН</b></font><br/>\":\"\" #>"
                 + "<#= Worker.IsTabu == true?\"<font color='red'><b>Выдача спецодежды на складе запрещена!</b></font><br/>\":\"\" #>"
                 + "№ основного склада: <font color='blue'><b><#=StorageNumber#></b></font> Код РМ: <font color='blue'><b><#=WorkplaceShortName#></b></font>"
                 + "<#= MVZ != \"\"?\" МВЗ: <font color='blue'><b>\"+ MVZ +\"</b></font>\":\"\" #>"
                 + "</br>"
                 + "Пол: <#= Worker.Sex != null?\"<b>\"+Worker.Sex.Name+\"</b>\":\"\" #>"
                 + "&nbsp;|&nbsp;<#= Worker.NomBodyPartSizes[0] != null?Worker.NomBodyPartSizes[0].NomBodyPart.Name+\": <b>\"+Worker.NomBodyPartSizes[0].SizeNumber+\"</b>\":\"\" #>"
                 + "&nbsp;|&nbsp;<#= Worker.NomBodyPartSizes[1] != null?Worker.NomBodyPartSizes[1].NomBodyPart.Name+\": <b>\"+Worker.NomBodyPartSizes[1].SizeNumber+\"</b>\":\"\" #>"
                 + "&nbsp;|&nbsp;<#= Worker.NomBodyPartSizes[2] != null?Worker.NomBodyPartSizes[2].NomBodyPart.Name+\": <b>\"+Worker.NomBodyPartSizes[2].SizeNumber+\"</b>\":\"\" #>"
                 + "&nbsp;|&nbsp;<#= Worker.NomBodyPartSizes[3] != null?Worker.NomBodyPartSizes[3].NomBodyPart.Name+\": <b>\"+Worker.NomBodyPartSizes[3].SizeNumber+\"</b>\":\"\" #>"
                 + "&nbsp;|&nbsp;Категория: <#= Worker.WorkerCategory != null?\"<b>\"+Worker.WorkerCategory.Name+\"</b>\":\"---\" #>"
                 + "&nbsp;|&nbsp;Группа: <#= Worker.WorkerGroup != null?\"<b>\"+Worker.WorkerGroup.Name+\"</b>\":\"---\" #>"
                 + "</br>Норма: <#= Organization.NormaOrganization != null?\"<b>\"+Organization.NormaOrganization.Norma.Name+\"</b>\":\"\" #>");
             columns.Bound(x => x.StorageId).Hidden();
         })
         .HtmlAttributes(new { @class = "attachment-grid" })
         .ClientEvents(events => events
             .OnLoad("HideHeader")
             .OnDataBinding("dataBinding")
             .OnError("onGridErrorEvent"))
         .Footer(false)
         .Render();
%>
<br />
<%
   Html.Telerik().Grid<WorkerNorma>()
        .Name("WorkerNorma")
        //.ToolBar(commands =>
        //{
            //    commands.Insert().ButtonType(GridButtonType.Text).ImageHtmlAttributes("style='visible:none;'");
        //    if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
        //        HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_CARD_EDIT))
        //    {
        //        commands.SubmitChanges();
        //        commands.Custom()
        //            .Text("Отмена выдачи")
        //            .HtmlAttributes(new { id = "cancelOper", onclick = "onCancelOper()" })
        //            .Url("#");
        //        //.Action("CancelOper", "WorkerCards");
        //    }
        //})
        .ToolBar(toolBar =>
            {
                    toolBar.SubmitChanges();
                    toolBar.Template(() =>
                        {
                            if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) || 
                                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_CARD_EDIT))
                            {
                            %>
                                <a class="t-button t-grid-save-changes" href="#">Сохранить изменения</a>
                                <a class="t-button t-grid-cancel-changes" href="#">Отменить изменения</a>
                                <!--a class="t-button" id="cancelOper" onclick="onCancelOper()" href="#">Отмена выдачи</a-->
                                    <%
                                    if (HttpContext.Current.Session["_idOrg"].ToString()==DataGlobals.ORG_ID_VGOK)
                                    {
                                    %>
                                        <%= //HttpContext.Current.Request.ApplicationPath //Server.MachineName
                                            
                                            Html.Button("reportM11", "Печать M11 и Личной карточки", HtmlButtonType.Button,
                                            "window.open('" + getReportUrl("Накладная M11") + "&RptOperTypeID=" + DataGlobals.OPERATION_WORKER_IN + "&RptUserName=" + Server.UrlEncode(((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName) + "&RptWorkerWorkplaceId='+$('#WorkerWorkplaceCombo').data('tComboBox').value());"
                                              + "window.open('" + getReportUrl("Вкладыш в личную карточку_Евразруда") + "&RptOperTypeID=" + DataGlobals.OPERATION_WORKER_IN + "&RptUserName=" + Server.UrlEncode(((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName) + "&RptWorkerWorkplaceId='+$('#WorkerWorkplaceCombo').data('tComboBox').value());",
                                                new { @class = "t-button"})
                                        %>
                            
                                    <%}  else {%>

                                        <%=
                                            Html.Button("reportM11", "Печать M11 и Личной карточки", HtmlButtonType.Button,
                                            "window.open('" + getReportUrl("Накладная M11") + "&RptOperTypeID=" + DataGlobals.OPERATION_WORKER_IN + "&RptUserName=" + Server.UrlEncode(((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName) + "&RptWorkerWorkplaceId='+$('#WorkerWorkplaceCombo').data('tComboBox').value());"
                                              + "window.open('" + getReportUrl("Вкладыш в личную карточку") + "&RptOperTypeID=" + DataGlobals.OPERATION_WORKER_IN + "&RptUserName=" + Server.UrlEncode(((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName) + "&RptWorkerWorkplaceId='+$('#WorkerWorkplaceCombo').data('tComboBox').value());",
                                                new { @class = "t-button"})
                                        %>
                                        <!--a class="t-button" id="cancelOper" onclick="onCancelOper()" href="#">Отмена выдачи</a-->
                                    <%}  %>
                            <%
                            }
                            %>
                                <%=
                                Html.Button("printBtn", "Печать экрана", HtmlButtonType.Button, "javascript:printScreen();",new { @class = "t-button"})
                                %>
                            <!--%= Html.Label("isWinter", "Показывать зимнюю")%-->
                            <!--%= Html.CheckBox("isWinter", bool.Parse(Request.Cookies["isWinter"] != null ? Request.Cookies["isWinter"].Value : bool.FalseString), new { onClick = "onClickIsWinter(this)" })%-->
                            <%= Html.Button("onHandBtn", "На руках", HtmlButtonType.Button, "javascript:showOnHand()",new { @class = "t-button"})%>

                            <%
                            if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_CARD_EDIT))
                            {
                            %>
                                <label for="operDate">Дата операции</label>
                                <%= 
                                   Html.Telerik().DatePicker()
                                        .Name("operDate")
                                        .HtmlAttributes(new { id = "operDate_wrapper", style = "vertical-align: middle;" })
                                        .InputHtmlAttributes(new {size = "4"}) // не работает
                                        .Value(DateTime.Today)
                                        .Format(DataGlobals.DATE_FORMAT)
                                %>
                                        
                                <!--br />
                                <a class="t-button" id="moving" onclick="onMoving()" href="#" style="display:none">Перенести на</a-->
                                <%  
                                    //Html.Telerik().DropDownList()
                                    //    .Name("movingWorkerWorkplace")
                                    //    .HtmlAttributes(new { @style = "display:none; vertical-align: middle; width:700px" })
                                    //    .DataBinding(binding => binding.Ajax()
                                    //        .Select("_GetWorkerWorkplacesActive", "WorkerCards"))
                                    //    .ClientEvents(events => events.OnDataBinding("onMovingWorkerWorkplaceBinding"))
                                    //    .Render();
                                %>
                           <%
                           }
                        });
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
              .Select("Select", "WorkerCards")
              .Insert("Save", "WorkerCards")
              .Update("Update", "WorkerCards")
              .Delete("Delete", "WorkerCards");
        })
        .Columns(columns =>
        {
            if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_CARD_EDIT) ||
                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_EDIT))
            {
                columns.Bound(x => x.NormaContentName)
                    //.ClientTemplate("<a href=\"javascript:onNormacontentClick(<#= NormaContentId #>,'<#= NormaContentName #>')\"><#= NormaContentName #></a>")
                    .ClientTemplate("<div onClick=\"javascript:onNormacontentClick(<#= NormaContentId #>,'<#= NormaContentName #>')\" style=\"cursor:hand\" title=\"Добавить группу замены\"><#= NormaContentName #></div>")
                .Width(230)
                .Title("Наименование групп номенклатур")
                .ReadOnly();
            }
            else
            {
                columns.Bound(x => x.NormaContentName)
                .Width(230)
                .Title("Наименование групп номенклатур")
                .ReadOnly();
            }
            columns.Bound(x => x.NormaQuantity)
                .Width(70)
                .Title("По&nbsp;норме")
                //.HtmlAttributes(new {style = "word-wrap:normal"})
                .ReadOnly();
            columns.Bound(x => x.NormaUsePeriod)
                .Width(60)
                .Title("Период")
                .ReadOnly();
            columns.Bound(x => x.StorageNumber).Width(80).Title("Склад").ReadOnly();
            columns.Bound(x => x.StorageInfo)
              .EditorTemplateName("StorageNomTemplate")
              .Encoded(false)
              //.ClientTemplate("<#= StorageInfo #>")
              .Title("[Код SAP] Номенклатура (размер, рост, износ, кол-во)");
            //.Width(400);
            columns.Bound(x => x.PutQuantity)
                .Width(60)
                .Title("Выдать");
            columns.Bound(x => x.IsCorporate).Width(50)
                .ClientTemplate("<input type='checkbox'  disabled='disabled'   name='iscorporate' <#= ( IsCorporate)?'checked':'' #> />")
                .Title("Корп.");
            columns.Bound(x => x.PresentQuantity)
                .Width(70)
                .Title("На&nbsp;руках")
                .ReadOnly();
            columns.Bound(x => x.ReceptionDate)
                .Format("{0:" + DataGlobals.DATE_FORMAT + "}")
                .Width(70)
                .Title("Выдано")
                .ReadOnly();
            columns.Bound(x => x.DocNumber)
                //.ClientTemplate("<a href=\"javascript:printM11(<#= DocNumber #>, '<#= ReceptionDateAsString #>','" + ((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName + "','" + (HttpContext.Current.Session["_idOrg"].ToString() == DataGlobals.ORG_ID_VGOK) + "');\"><#= DocNumber != 0 ? DocNumber:\"\" #></a>")
                .ClientTemplate("<a href=\"javascript:printM11(<#= DocNumber #>, '<#= ReceptionDateAsString #>',<#= OperTypeId #>,'" + ((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName + "','" + (HttpContext.Current.Session["_idOrg"].ToString() == DataGlobals.ORG_ID_VGOK) + "');\"><#= DocNumber != 0?DocNumber:\"\" #></a>")
                .Width(70)
                .Title("№&nbsp;M11")
                .ReadOnly();
            //columns.Bound(x => x.WorkerCard.WorkerWorkplace.Id).Hidden();
            //columns.Bound(x => x.NormaContent.NormaNomGroup.NomGroup.Id).Hidden();
            columns.Bound(x => x.NormaContentId).Hidden();
            columns.Bound(x => x.StorageId).Hidden();
        })
        .ClientEvents(events => events
            .OnEdit("onEdit")
            .OnError("onError")
            .OnDataBound("onDataBound")
            .OnSave("onSave")
            //.OnDataBound("onDataBound")
            .OnSubmitChanges("onSubmit")
            .OnDataBinding("dataBinding"))
        .Editable(editing => editing.Mode(GridEditMode.InCell))
        //.Pageable(x => x.PageSize(10))
        //.Sortable()
        //.Groupable()
        //.Filterable()
        .Scrollable(x => x.Height(300))
        .Render();
%>

  </div>

<%
    Html.Telerik().Window()
           .Name("Window")
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
                                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_CARD_EDIT)
                                )
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

<%
    Html.Telerik().Window()
           .Name("OnHandWindow")
           .Title("Номенклатура на руках")
           .Width(700)
           .Height(380)
           .Draggable(true)
           .Modal(false)
           .Visible(false)
           .Buttons(b => b.Close())
           .Content(() =>
           { %>
<%:
    Html.Telerik().Grid<WorkerNorma>()
         .Name("OnHandWorkerCard")
         .DataKeys(keys =>
         {
             keys.Add(x => x.Id);
         })
         .DataBinding(dataBinding =>
         {
             dataBinding
               .Ajax()
               .Select("Select", "WorkerCardOuts");
         })
         .Columns(columns =>
         {
             columns.Bound(x => x.StorageNumber)
              .Title("Склад")
              .Width(50)
              .ReadOnly();
             columns.Bound(x => x.StorageInfo)
               .Title("[Код SAP] Номенклатура (размер, рост, износ, кол-во)")
               .ReadOnly();
             columns.Bound(x => x.PresentQuantity)
                 .Width(100)
                 .Title("На руках")
                 .ReadOnly();
             columns.Bound(x => x.ReceptionDate)
                 .Format("{0:" + DataGlobals.DATE_FORMAT + "}")
                 .Width(70)
                 .Title("Выдано")
                 .ReadOnly();
             //columns.Bound(x => x.Wear)
             //    .ClientTemplate("<#= Wear==100?\"новая\":Wear==50?\"б/у\":\"\" #>")
             //    .EditorTemplateName("WearTemplate")
             //    .Width(80)
             //    .Title("Износ");
             //columns.Bound(x => x.NormaContentId).Hidden();
             columns.Bound(x => x.StorageId).Hidden();
         })
         .ClientEvents(events => events
             .OnError("onError")
             .OnDataBinding("OnHandDataBinding"))
         .Scrollable(x => x.Height(300))
    %>
 	<%	})
		 .Render();
				 %>
<script type="text/javascript">
    var normaContentId;
    function onNormacontentClick(normaContentId, normaContentName) {
        var normaNomGroupsGrid = $('#NormaGrid').data('tGrid');
        var grid = document.getElementById("NormaNomGroups");
        normaNomGroupsGrid.rebind({
            NormaContentId: normaContentId
        });

        var window = $("#Window").data("tWindow");
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

    function onEdit(e) {
        normaContentId = e.dataItem['NormaContentId'];
        //    if (e.dataItem != null) {
        //        var obj = e.dataItem['Storage'];
        //        //if ($(e.form).find('#Storage').data('tComboBox') != null) {
        //            $(e.form).find('#Storage').data('tComboBox').value((obj == null) ? -1 : obj.Id);
        //            $(e.form).find('#Storage').data('tComboBox').text((obj == null) ? -1 : obj.StorageInfo);
        //        //}
        //    }
        //    var $combo = $(e.cell).find('#StorageInfo');
        //    if ($combo.length > 0) {
        //        var combo = $combo.data('tComboBox');
        //        combo.fill(function () {
        //            combo.value(e.dataItem['StorageInfo'].Id);
        //            combo.text(e.dataItem['StorageInfo'].StorageInfo);
        //        });
        //    }
    }

    //  function updateGrid() {
    //      $("#WorkerNorma").data("tGrid").rebind();
    //  }

    function clearWorkerCard() {
        $("#WorkerWorkplace").data("tGrid").dataBind(new Array());
        $("#WorkerNorma").data("tGrid").dataBind(new Array());
    }

    function findWorkerCard() {
        $("#WorkerWorkplace").data("tGrid").dataBind(new Array());
        $("#WorkerNorma").data("tGrid").dataBind(new Array());
        var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
        var text = $("#WorkerWorkplaceCombo").data("tComboBox").text();
        if (workerWorkplaceId != "" && workerWorkplaceId != text) {
            $("#WorkerWorkplace").data("tGrid").rebind();
            $("#WorkerNorma").data("tGrid").rebind();
            if (text.indexOf("НЕ АКТИВНА") != -1) {
                if (document.getElementById("moving"))
                    document.getElementById("moving").style.display = "inline";
                var obj = $("#movingWorkerWorkplace").data("tDropDownList");
                if (obj) {
                    obj.element.parentElement.style.display = "inline";
                    obj.reload();
                }
            }
            else {
                if (document.getElementById("moving"))
                    document.getElementById("moving").style.display = "none";
                var obj = $("#movingWorkerWorkplace").data("tDropDownList");
                if (obj) {
                    obj.element.parentElement.style.display = "none";
                }
            }
            if (document.getElementById("contentWorkerCard"))
                document.getElementById("contentWorkerCard").style.display = "block";
        }
    }

    function dataBinding(args) {
        var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
        var workerWorkplaceText = $("#WorkerWorkplaceCombo").data("tComboBox").text();
        var storageNameId = $("#StorageNameList").data("tDropDownList").value();
        var isWinter = true;
        /*
        if (document.getElementById("isWinter"))
            isWinter = document.getElementById("isWinter").checked;
        */
        args.data = $.extend(args.data, { workerWorkplaceId: workerWorkplaceId, workerWorkplaceText: workerWorkplaceText, storageNameId: storageNameId, isWinter: isWinter });
    }

    function onSave(e) {
        var $combo = $(e.cell).find('#StorageInfo');
        if ($combo.length > 0) {
            var combo = $combo.data("tComboBox"), selectItem = combo.selectedIndex > -1 ? combo.data[combo.selectedIndex] : null;

          if (selectItem) {
              var splitName = selectItem.Text.split(" ");
              //e.values["StorageInfo"] = { Id: selectItem.Value || selectItem.Text, StorageInfo: selectItem.Text };
              e.values.StorageId = selectItem.Value;
              e.values.StorageInfo = selectItem.Text.replace("&lt;","<").replace("&gt;",">");
          } else {
//              var value = combo.value();
//              e.values["StorageInfo"] = { Id: value, StorageInfo: value };
          }
        }
        
        var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
        e.data = $.extend(e.data, { workerWorkplaceId: workerWorkplaceId});
    }

    function onSubmit(e) {
        var storageNameId = $("#StorageNameList").data("tDropDownList").value();
        var workerWorkplaceInfo = $("#WorkerWorkplaceCombo").data("tComboBox").text();
        var workerStorage = $("#WorkerWorkplace").data("tGrid");
        var dataItem = workerStorage.data[0];
        var workerStorageId = dataItem.StorageId;

        if (storageNameId != workerStorageId)
        {
            if (!window.confirm(dataItem.WorkerTabn + " " + dataItem.Worker.Fio + " Прикреплен к складу " + dataItem.StorageNumber + ". Ответственность выдачи лежит на ВАС. Выдать позиции?"))
               return false;
        }
        if (workerWorkplaceInfo.indexOf('НЕ АКТИВНА') != -1) {
            alert('Выдача на старое рабочее место НЕ ВОЗМОЖНА!!! Выполните перевод');
            return false;
        }
        else {

            if (nkmk.Lock.isFree()) {
                nkmk.Lock.setBusy();
                nkmk.Lock.printBusyMessage("Идет обработка данных...");
            }
            var operDate = document.getElementById("operDate").value;
            e.updated[0] = $.extend(e.updated[0], { OperDate: operDate, storageNameId: storageNameId });
            e.data = $.extend({}, e.data, { storageNameId: storageNameId});
        }
    }

    function onStorageComboBoxDataBinding(e) {
        e.data = $.extend({}, e.data, { normaContentId: normaContentId, storageNameId: $("#StorageNameList").data("tDropDownList").value() });
    }

  function onCancelOper() {
      //alert("asdf");
      //var $cancelOper = $('#cancelOper');
      cssConfirm("Отменить выдачу?", "Да", "Нет", function (state) {
          if (state) {
              if (nkmk.Lock.isFree()) {
                  nkmk.Lock.setBusy();
                  nkmk.Lock.printBusyMessage("Идет обработка данных...");
              }
              var operDate = document.getElementById("operDate").value;
              $.post("WorkerCards/CancelOper", { OperDate: operDate }, function (data) {
                  var $grid = $("#WorkerNorma").data("tGrid");
                  $grid.rebind();
              });
          }
      });
  }

    function onMoving() {
        cssConfirm("Перевести позиции?", "Да", "Нет", function (state) {
            if (state) {
                if (nkmk.Lock.isFree()) {
                    nkmk.Lock.setBusy();
                    nkmk.Lock.printBusyMessage("Идет обработка данных...");
                }
                var workerWorkplaceNotActive = $("#WorkerWorkplaceCombo").data("tComboBox").value();
                var workerWorkplaceActive = $("#movingWorkerWorkplace").data("tDropDownList").value();
                $.post("WorkerCards/Moving", { workerWorkplaceNotActive: workerWorkplaceNotActive, workerWorkplaceActive: workerWorkplaceActive }, function (data) {
                    var $grid = $("#WorkerNorma").data("tGrid");
                    $grid.rebind();
                });
            }
        });
    }
    /*
    function onClickIsWinter(obj) {
        if (obj.checked)
            setCookie("isWinter", true, 90);
        else
            setCookie("isWinter", false, 90);
        $("#WorkerNorma").data("tGrid").rebind();
    }
*/
    function onMovingWorkerWorkplaceBinding(e) {
        var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
        e.data = $.extend(e.data, { workerWorkplaceId: workerWorkplaceId });
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
               var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
               if (workerWorkplaceId != "") {
                   $("#WorkerWorkplace").data("tGrid").rebind();
               }
               var window = $('#Window_alert').data('tWindow');
               window.content(xhr.responseText).center().open();
            }
        }
    }

    function onDataBound(e) {
        nkmk.Lock.setFree();
    }

    window.onresize = onFormResize;

    function onFormResize() { 
        $("#StorageNameList").data("tDropDownList").disable();
        $("#StorageNameList").data("tDropDownList").enable();
        $("#WorkerWorkplaceCombo").data("tComboBox").disable();
        $("#WorkerWorkplaceCombo").data("tComboBox").enable();
    }
/*
    function printM11(docNumber, docDate, UserName, isVgork) {
        if (isVgork == "True") {
            window.open('<%= getReportUrl("Требование на выдачу СИЗ") %>&RptUserName=' + escape(UserName) + ' &RptOperTypeID=<%= DataGlobals.OPERATION_WORKER_IN %>&DATEN=' + docDate + '&RptParamDocum=' + docNumber + '&RptWorkerWorkplaceId=' + $('#WorkerWorkplaceCombo').data('tComboBox').value());
        } else {
            window.open('<%= getReportUrl("Накладная M11") %>&RptUserName=' + escape(UserName) + ' &RptOperTypeID=<%= DataGlobals.OPERATION_WORKER_IN %>&DATEN=' + docDate + '&RptParamDocum=' + docNumber + '&RptWorkerWorkplaceId=' + $('#WorkerWorkplaceCombo').data('tComboBox').value());
        }
        window.open('<%= getReportUrl("Вкладыш в личную карточку") %>&RptOperTypeID=<%= DataGlobals.OPERATION_WORKER_IN %>&DATEN=' + docDate + '&RptParamDocum=' + docNumber + '&RptWorkerWorkplaceId=' + $('#WorkerWorkplaceCombo').data('tComboBox').value());
    }
*/
    function printM11(docNumber, docDate, operTypeID, UserName, isVgork) {
        if (isVgork == "True") {
            window.open('<%= getReportUrl("Требование на выдачу СИЗ") %>&RptUserName=' + escape(UserName) + ' &RptOperTypeID=' + operTypeID + '&DATEN=' + docDate + '&RptParamDocum=' + docNumber + '&RptWorkerWorkplaceId=' + $('#WorkerWorkplaceCombo').data('tComboBox').value());
        } else {
            window.open('<%= getReportUrl("Накладная M11") %>&RptUserName=' + escape(UserName) + ' &RptOperTypeID=' + operTypeID + '&DATEN=' + docDate + '&RptParamDocum=' + docNumber + '&RptWorkerWorkplaceId=' + $('#WorkerWorkplaceCombo').data('tComboBox').value());

        }
        window.open('<%= getReportUrl("Вкладыш в личную карточку") %>&RptUserName=' + escape(UserName) + '&RptOperTypeID=' + operTypeID + '&DATEN=' + docDate + '&RptParamDocum=' + docNumber + '&RptWorkerWorkplaceId=' + $('#WorkerWorkplaceCombo').data('tComboBox').value());
    }



    function printScreen() {
        var w = window.open();
        w.document.open();  // Открываем документ для записи
w.document.writeln('<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">');
w.document.writeln('<html xmlns="http://www.w3.org/1999/xhtml">');
w.document.writeln('<title>Учет СИЗ и спецодежды</title>');
w.document.writeln('<style type="text/css">');
w.document.writeln('</style>');
w.document.writeln('<body>');
w.document.writeln('<h3>Выдача спецодежды</h3>');
w.document.writeln('<table width="90%" align="center">');
w.document.writeln('<tr>');
w.document.writeln('<td width="30%">Табельный/Ф.И.О</td>');
w.document.writeln('<td id="famField">' + $('#WorkerWorkplaceCombo').data('tComboBox').text() + '</td>');
w.document.writeln('</tr>');
w.document.writeln('<tr>');
w.document.writeln('<td  width="30%">Склад:</td>');
w.document.writeln('<td id="storageField">'+$("#StorageNameList").data("tDropDownList").text()+'</td>');
w.document.writeln('</tr>');
w.document.writeln('<tr>');
var data = $("#WorkerWorkplace").data("tGrid");
w.document.writeln('<td colspan="2" id="sizeField">' + data.$tbody.context.innerHTML + '</td>');
w.document.writeln('</tr>');
w.document.writeln('</table>');
w.document.writeln('<br/>');
w.document.writeln('<table width="90%" align="center" id="contentTable" border="1" cellpadding="1" cellspacing="0">');
$("#WorkerNorma .t-grid-header-wrap tr").each(function (indx, element) {
    w.document.writeln("<tr>");
    var cellText = element.innerHTML;
    //Рисуем бордюр
    cellText = cellText.replace("<TH class=t-header scope=col>", "<TH style='border: 1px solid black;'>");
    cellText = cellText.replace(new RegExp("<TH class=t-header scope=col>", 'g'), "<TH style='border-right:1px solid black;border-bottom: 1px solid black;border-top: 1px solid black;'>");
    cellText = cellText.replace(new RegExp("<TH class=\"t-header t-last-header\" scope=col>", 'g'), "<TH style='border-right:1px solid black;border-bottom: 1px solid black;border-top: 1px solid black;'>");
    w.document.writeln(cellText);
    w.document.writeln("</tr>");
});
$("#WorkerNorma .t-grid-content tbody tr").each(function (indx, element) {
    w.document.writeln("<tr>");
    var cellText = element.innerHTML.replace(new RegExp("></TD>", 'g'), ">&nbsp;</TD>");
    //Рисуем бордюр
    cellText = cellText.replace("<TD>", "<TD style='border: 1px solid black;'>");
    cellText = cellText.replace(new RegExp("<TD>", 'g'), "<TD style='border-right:1px solid black;border-bottom: 1px solid black;'>");
    cellText = cellText.replace(new RegExp("class=t-last>", 'g'), " style='border-right:1px solid black;border-bottom: 1px solid black;'>");
    w.document.writeln(cellText);
    w.document.writeln("</tr>");
});
w.document.writeln("</table>");
w.document.writeln("</body>");
w.document.writeln("</html>");
w.document.close(); // Заканчиваем формирование документа
w.print();
}

function showOnHand() {
    var grid = $('#OnHandWorkerCard').data('tGrid');    
    grid.rebind();
    var window = $("#OnHandWindow").data("tWindow");
    window.center().open();
}

function OnHandDataBinding(args) {
    var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
    var workerWorkplaceText = $("#WorkerWorkplaceCombo").data("tComboBox").text();
    args.data = $.extend(args.data, { workerWorkplaceId: workerWorkplaceId, workerWorkplaceText: workerWorkplaceText });
}

  </script>

</asp:Content>
