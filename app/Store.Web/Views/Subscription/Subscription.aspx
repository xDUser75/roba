<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" Culture="ru-RU"%>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<h3>Ответственные лица</h3>

             <%:
         Html.Telerik().Grid<Store.Core.Subscription>()
                .Name("Subscription")
                .ToolBar(
                     commands =>
                         {
                           if (HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_ADMIN) ||
                               HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_SUBSCRIPTION_EDIT)
                              )
                           {
                               commands.Custom()
                                   .ButtonType(GridButtonType.Image)
                                   .ImageHtmlAttributes(new { @class = "t-icon t-add" })
                                   .HtmlAttributes(new { title = "Добавить новые данные" })
                                   .Url("javascript:editRecord(-1)");                             
                              }
                           }
                 )
                .DataKeys(keys =>
                {
                  keys.Add(x => x.Id);
                })              
                .Columns(columns=>
                {
                  if (HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_ADMIN) ||
                      HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_SUBSCRIPTION_EDIT)
                     )
                  {
                    columns.Bound(x => x.Id)
                        .Title("")
                        .ClientTemplate("<a onClick='editRecord(<#= Id #>,<#= Organization.Id #>,\"<#= OrganizationInfo #>\",<#= WorkerInfo1Id #>,\"<#= WorkerInfo1 #>\",<#= WorkerInfo2Id #>,\"<#= WorkerInfo2 #>\",<#= WorkerInfo3Id #>,\"<#= WorkerInfo3 #>\")' title='Редактировать' class='t-button t-button-icon' href='#'><span class='t-icon t-edit'></span></a><a onClick='delRecord(\"<#= Id #>\")' title='Удалить' class='t-button t-button-icon' href='#'><span class='t-icon t-delete'></span></a>")
                        .Filterable(false)
                        .Width("85px")
                        .Sortable(false);
                  }
                  columns.Bound(c => c.Organization.ShopNumber);
                  columns.Bound(c => c.OrganizationInfo);
                  columns.Bound(c => c.WorkerInfo1);
                  columns.Bound(c => c.WorkerInfo2);
                  columns.Bound(c => c.WorkerInfo3);
                })
                .DataBinding(dataBinding => dataBinding.Ajax()
                                .Select("_SelectSubscription", "Subscription")
                  )
                  .Editable(editing => editing.Mode(GridEditMode.InLine))
                  .Pageable(pager => pager.PageSize(20))
//                .Scrollable(x => x.Height("550px"))
                  .Sortable()
                  .Filterable()                  
             %>
    <% 
// Окно редактирования записи
        Html.Telerik().Window()
           .Name("Window")
           .Width(358)
           .Height(225)
           .Draggable(true)
           .Modal(true)
           .Visible(false)
           .Buttons(b => b.Close())
           .Content(() =>
           { %>
           <input type="hidden" id="selectRecord"/>
           Подразделение:<br/>
            <%= Html.Telerik().ComboBox()
                     .Name("ShopCombo")
                     .AutoFill(false)
                     .Encode(false)
                     .DataBinding(binding => binding.Ajax()
                        .Select("_GetShop", "Subscription")
                        .Delay(600)
                    .Cache(false)
                        )
                    .HtmlAttributes(new { @style = "width:350px" })
                    .Filterable(filtering =>
                        filtering
                        .FilterMode(AutoCompleteFilterMode.Contains)
                        .MinimumChars(2)
                    )
                    .SelectedIndex(0)
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
            %>
           Руководитель подразделения:<br/>
            <%= Html.Telerik().ComboBox()
                     .Name("Worker1Combo")
                     .AutoFill(false)
                     .Encode(false)
                     .DataBinding(binding => binding.Ajax()
                        .Select("_GetWorker", "Subscription")
                        .Delay(600)
                    .Cache(false)
                        )
                    .HtmlAttributes(new { @style = "width:350px" })
                    .Filterable(filtering =>
                        filtering
                        .FilterMode(AutoCompleteFilterMode.Contains)
                        .MinimumChars(2)
                    )
                    .SelectedIndex(0)
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)                    
            %>
            <br/>
           Ответственный за нормы:<br/>
            <%= Html.Telerik().ComboBox()
                     .Name("Worker2Combo")
                     .AutoFill(false)
                     .Encode(false)
                     .DataBinding(binding => binding.Ajax()
                        .Select("_GetWorker", "Subscription")
                        .Delay(600)
                    .Cache(false)
                        )
                    .HtmlAttributes(new { @style = "width:350px" })
                    .Filterable(filtering =>
                        filtering
                        .FilterMode(AutoCompleteFilterMode.Contains)
                        .MinimumChars(2)
                    )
                    .SelectedIndex(0)
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
            %>
            <br/>
           Ответственный за выходной документ:<br/>
            <%= Html.Telerik().ComboBox()
                     .Name("Worker3Combo")
                     .AutoFill(false)
                     .Encode(false)
                     .DataBinding(binding => binding.Ajax()
                        .Select("_GetWorker", "Subscription")
                        .Delay(600)
                    .Cache(false)
                        )
                    .HtmlAttributes(new { @style = "width:350px" })
                    .Filterable(filtering =>
                        filtering
                        .FilterMode(AutoCompleteFilterMode.Contains)
                        .MinimumChars(2)
                    )
                    .SelectedIndex(0)
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
            %>
            <br/>
            <br/>
            <div align="center">
                <input type="button" value="Сохранить" class="t-button" onclick="saveData()"/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <input type="button" value="Отмена" class="t-button" onclick="coloseWindow()"/>
            </div>
            <%})           
           .Render();
    %>

<script type="text/javascript" language="javascript">
    function delRecord(id) {
        cssConfirm("Удалить выбранную запись?", "Да", "Нет", function (state) {
            if (state) {
                $.ajax({
                    url: 'Subscription/_delRecord',
                    contentType: 'application/x-www-form-urlencoded',
                    type: 'POST',
                    dataType: 'json',
                    error: function (xhr, status) {
                        onGridErrorEvent(xhr);
                    },
                    data: {
                        id: id
                    },
                    success: function (result) {
                        if ((result) && (result.modelState)) {
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
                            $("#Subscription").data("tGrid").rebind();
                        }
                    }
                });
            }
        });
    }

    function editRecord(id, orgId, orgName, workerId1, workerName1, workerId2, workerName2, workerId3, workerName3) {
        document.getElementById("selectRecord").value = id;
        var window = $('#Window').data('tWindow');
        if (id == -1) {
            window.title("Добавление подписей");
            var combobox = $("#ShopCombo").data("tComboBox");
            combobox.value(orgName);
            combobox.enable();
            $("#ShopCombo").data("tComboBox").value("");            
            $("#Worker1Combo").data("tComboBox").value("");
            $("#Worker2Combo").data("tComboBox").value("");
            $("#Worker3Combo").data("tComboBox").value("");            
        } else {
            window.title("Редактирование подписей");
            var combobox = $("#ShopCombo").data("tComboBox");
            combobox.value(orgId); 
            combobox.text(orgName);
            combobox.disable();
            $("#Worker1Combo").data("tComboBox").value(workerId1);
            $("#Worker1Combo").data("tComboBox").text(workerName1);
            $("#Worker2Combo").data("tComboBox").value(workerId2);
            $("#Worker2Combo").data("tComboBox").text(workerName2);
            $("#Worker3Combo").data("tComboBox").value(workerId3);
            $("#Worker3Combo").data("tComboBox").text(workerName3);
        }
        window.center().open();
    }

    function saveData() {
        var recordId = document.getElementById("selectRecord").value;
        var orgId = "";
        var shop = $("#ShopCombo").data("tComboBox");
        if (shop.value() != shop.text()) {
            orgId = shop.value();
        }

        if (orgId == "") {
            alert("Подразделение не выбрано!");
            return false;
        }
        var workerId1 = -1;
        var combo = $("#Worker1Combo").data("tComboBox");
        if (combo.value() != combo.text()) {
            workerId1 = combo.value();
        }

        var workerId2 = -1;
        combo = $("#Worker2Combo").data("tComboBox");
        if (combo.value() != combo.text()) {
            workerId2 = combo.value();
        }

        var workerId3 = -1;
        combo = $("#Worker3Combo").data("tComboBox");
        if (combo.value() != combo.text()) {
            workerId3 = combo.value();
        }

        $.ajax({
            url: 'Subscription/_addOrEditRecord',
            contentType: 'application/x-www-form-urlencoded',
            type: 'POST',
            dataType: 'json',
            error: function (xhr, status) {
                onGridErrorEvent(xhr);
            },
            data: {
                id: recordId,
                orgId: orgId,
                WorkerId1: workerId1,
                WorkerId2: workerId2,
                WorkerId3: workerId3
            },
            success: function (result) {
                if ((result) && (result.modelState)) {
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
                    $("#Subscription").data("tGrid").rebind();
                    coloseWindow();
                }
            }
        });

    }

    function coloseWindow() {
        var window = $('#Window').data('tWindow');
        window.close();        
    }
</script>
</asp:Content>