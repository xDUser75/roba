<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" Culture="ru-RU"%>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<h3>Оценка качества СИЗ</h3>
             <%:
         Html.Telerik().Grid<Store.Core.TestRegisterSimple>()
                .Name("TestRegisters")
                .ToolBar(
                     commands =>
                         {
                           if (HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_ADMIN) ||
                               HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_TEST_REGISTER_EDIT)
                              )
                           {
                               commands.Custom()
                                   .ButtonType(GridButtonType.Image)
                                   .ImageHtmlAttributes(new { @class = "t-icon t-add" })
                                   .HtmlAttributes(new { title = "Добавить новые данные" })
                                   .Url("javascript:editTestRegisterRecord(-1)");                             
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
                      HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_TEST_REGISTER_EDIT)
                     )
                  {
                    columns.Bound(x => x.Id)
                        .Title("")
                        .ClientTemplate("<a onClick='editTestRegisterRecord(<#= Id #>,<#= ProvaiderId #>,\"<#= ProvaiderInfo #>\",<#= NomGroupId #>,\"<#= NomGroup #>\",\"<#= TestDateString #>\",\"<#= Model #>\")' title='Редактировать' class='t-button t-button-icon' href='#'><span class='t-icon t-edit'></span></a><a onClick='delTestRegisterRecord(\"<#= Id #>\")' title='Удалить' class='t-button t-button-icon' href='#'><span class='t-icon t-delete'></span></a>")
                        .Filterable(false)
                        .Width("85px")
                        .Sortable(false);
                  }
                  columns.Bound(c => c.TestDate).Format("{0:" + Store.Data.DataGlobals.DATE_FORMAT + "}").Width(60);
                  columns.Bound(c => c.Provaider);
                  columns.Bound(c => c.Producer);
                  columns.Bound(c => c.Model);
                  columns.Bound(c => c.NomGroup);                    
                })
                .DataBinding(dataBinding => dataBinding.Ajax()
                         .Select("_SelectTestRegister", "TestRegister")
                  )
                .ClientEvents(events => events
                    .OnRowDataBound("onRowDataBound")
                    .OnDataBinding("onDataBinding")
                    .OnRowSelect("onTestRegisterRowSelect")
                 )
                .Editable(editing => editing.Mode(GridEditMode.InLine))
                .Pageable(pager => pager.PageSize(20))
                .Scrollable(scrolling => scrolling.Height(200))
                .Selectable()
                .Sortable()
                .Filterable()                  
             %>
<br/>
<font color="black"><u>Места проведения испытаний и их результат</u></font><br/>
<div>Поставщик:&nbsp;<span id="ProvaiderInfo"></span></div>
<div>Группа номенклатур:&nbsp;<span id="NomGroupInfo"></span></div>
<div>Номенклатура:&nbsp;<span id="NomenclatureInfo"></span></div>
             <%:
         Html.Telerik().Grid<Store.Core.Certificate>()
                .Name("Certificates")
                .ToolBar(
                     commands =>
                     {
                         
                         if (HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_ADMIN) ||
                             HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_TEST_REGISTER_EDIT)
                            )
                         {
                             commands.Custom()
                                 .ButtonType(GridButtonType.Image)
                                 .ImageHtmlAttributes(new { @class = "t-icon t-add" })
                                 .HtmlAttributes(new { title = "Добавить новые данные" })
                                 .Url("javascript:editCertificateRecord(-1)");
                         }
                         
                     }
                 )
                .DataKeys(keys =>
                {
                    keys.Add(x => x.Id);
                })
                .Columns(columns =>
                {
                    if (HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_ADMIN) ||
                        HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_TEST_REGISTER_EDIT)
                       )
                    {
                        columns.Bound(x => x.Id)
                            .Title("")
                            .ClientTemplate("<a onClick='editCertificateRecord(<#= Id #>,<#= Organization.Id #>,\"<#= Organization.Name #>\",\"<#= DocNum #>\",\"<#= DocDateString #>\",\"<#= Descr #>\",<#= Result.Id #>)' title='Редактировать' class='t-button t-button-icon' href='#'><span class='t-icon t-edit'></span></a><a onClick='delCertificateRecord(\"<#= Id #>\")' title='Удалить' class='t-button t-button-icon' href='#'><span class='t-icon t-delete'></span></a>")
                            .Filterable(false)
                            .Width("85px")
                            .Sortable(false);
                    }
                    columns.Bound(c => c.DocNum).Width(70);
                    columns.Bound(c => c.DocDate).Format("{0:" + Store.Data.DataGlobals.DATE_FORMAT + "}").Width(60);
                    columns.Bound(c => c.Descr).Title("Примечание");
                    columns.Bound(c => c.Organization.Name).Title("Цех");
                    columns.Bound(c => c.Result.Name).Title("Результат");
                    columns.Bound(c => c.Result.Id).Hidden(true);
                })
                .DataBinding(dataBinding => dataBinding.Ajax()
                     .Select("_SelectCerticicates", "TestRegister")
                  )
                .ClientEvents(events => events
                    .OnRowDataBound("onRowDataBound")
                    .OnDataBinding("onCertificateDataBinding")
                    .OnDataBound("onCertificateDataBound")
                 )
                .Editable(editing => editing.Mode(GridEditMode.InLine))
                .Scrollable(scroll => scroll.Height(120))
             %>

    <% 
// Окно редактирования записи
        Html.Telerik().Window()
           .Name("TestRegisterWindow")
           .Width(358)
           .Height(225)
           .Draggable(true)
           .Modal(true)
           .Visible(false)
           .Buttons(b => b.Close())
           .Content(() =>
           { %>
           <input type="hidden" id="editTestRegisterRowId"/>
           Дата ввода:<br/>                
           <%: Html.Telerik().DatePicker()
                  .Name("TestDate")
                  .Value(DateTime.Now.ToShortDateString())
                  .Format(Store.Data.DataGlobals.DATE_FORMAT_FULL_YEAR)
           %>
           <br/>
           Поставщик:&nbsp;&nbsp;
           <div class="t-button t-button-icon" onclick="showProvaiderWindow()" ><span class="t-icon t-edit"></span></div>
           <br/>
            <%= Html.Telerik().ComboBox()
                     .Name("ProvaiderCombo")
                     .AutoFill(false)
                     .Encode(false)
                     .DataBinding(binding => binding.Ajax()
                        .Select("_GetProviders", "TestRegister")
                        .Delay(600)
                    .Cache(false)
                        )
                    .HtmlAttributes(new { @style = "width:350px" })
                    .Filterable(filtering =>
                        filtering
                        .FilterMode(AutoCompleteFilterMode.Contains)
                        .MinimumChars(2)
                    )
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
            %>
           Группа номенклатур:<br/>
            <%= Html.Telerik().ComboBox()
                     .Name("NomGroupCombo")
                     .AutoFill(false)
                     .Encode(false)
                     .DataBinding(binding => binding.Ajax()
                        .Select("_GetNomGroups", "TestRegister")
                        .Delay(600)
                    .Cache(false)
                        )
                    .HtmlAttributes(new { @style = "width:350px" })
                    .Filterable(filtering =>
                        filtering
                        .FilterMode(AutoCompleteFilterMode.Contains)
                        .MinimumChars(2)
                    )
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)                    
            %>
            <br/>
           Номенклатура:<br/>
           <input type="text" id="model" size="46" maxlength="50"/>
            <br/>
            <br/>
            <div align="center">
                <input type="button" value="Сохранить" class="t-button" onclick="saveTestRegisterData()"/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <input type="button" value="Отмена" class="t-button" onclick="coloseWindow('TestRegisterWindow')"/>
            </div>
            <%})           
           .Render();
    %>
<%
// Окно редактирования записи актов
        Html.Telerik().Window()
           .Name("CertificateWindow")
           .Width(358)
           .Height(250)
           .Draggable(true)
           .Modal(true)
           .Visible(false)
           .Buttons(b => b.Close())
           .Content(() =>
           { %>
           <input type="hidden" id="editCertificateRowId"/>
           <table width="100%" border="0">
           <tr>
               <td width="100">№ документа:</td>
               <td><input type="text" id="DocNumber" maxlength="50" size="15"/></td>
           </tr>
           <tr>
                <td>Дата документа:</td>
                <td>
                <%: Html.Telerik().DatePicker()
                  .Name("DocDate")
                  .Value(DateTime.Now.ToShortDateString())
                  .Format(Store.Data.DataGlobals.DATE_FORMAT_FULL_YEAR)
                %>
                </td>
           </tr>
            <tr>
                <td>Результат:</td>
                <td>
                    <%= Html.Telerik().DropDownList()
                            .Name("ResultList")
                            .BindTo(new SelectList((IEnumerable)ViewData[Store.Data.DataGlobals.REFERENCE_RESULT], "Id", "Name"))
                    %>
                </td>
           </tr>
           </table>

           Цех:<br/>
            <%= Html.Telerik().ComboBox()
                     .Name("ShopCombo")
                     .AutoFill(false)
                     .Encode(false)
                     .DataBinding(binding => binding.Ajax()
                        .Select("_GetShop", "TestRegister")
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
           Примечание:<br/>
            <textarea id="descr" rows="2" cols="41">
            </textarea>
            <br/><br/>
            <div align="center">
                <input type="button" value="Сохранить" class="t-button" onclick="saveCertificateData()"/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <input type="button" value="Отмена" class="t-button" onclick="coloseWindow('CertificateWindow')"/>
            </div>
            <%})           
           .Render();
    %>

<%
// Окно редактирования записи поставщиков
        Html.Telerik().Window()
           .Name("ProvaidersWindow")
           .Width(700)
           .Height(350)
           .Title("Поставщики")
           .Draggable(true)
           .Modal(true)
           .Visible(false)
           .Buttons(b => b.Close())
           .Content(() =>
           { %>
<%:
    Html.Telerik().Grid<Store.Core.Provaider>()
        .Name("ProvaiderGrid")
        .ToolBar(commands => {
            if (HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_ADMIN) ||
                    HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_NOMENCLATURE_EDIT))
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
              .Select("_Provaider_Select", "TestRegister")
              .Insert("_Provaider_Insert", "TestRegister")
              .Update("_Provaider_Update", "TestRegister");
        })
        .Columns(columns =>
        {
            columns.Command(commands =>
            {
                if (HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_ADMIN) ||
                    HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_NOMENCLATURE_EDIT))
                {
                    commands.Edit().ButtonType(GridButtonType.Image);
                }
            }).Width(45).Title("");
            columns.Bound(x => x.Name);
            columns.Bound(x => x.Producer);
            columns.Bound(x => x.City);
            columns.Bound(x => x.OrganizationId).Hidden(true);
        })
        .ClientEvents(events => events
             .OnError("onGridErrorEvent")
             .OnRowDataBound("onProvaiderRowDataBound")
             .OnDataBinding("onProvaiderDataBinding")
         )
        .Editable(editing => editing.Mode(GridEditMode.InLine))
        .Scrollable(scrolling => scrolling.Height(240))
        .Pageable(x => x.PageSize(30))
        .Sortable()
        .Selectable()
        .Filterable()
        .Resizable(resizing => resizing.Columns(true))
%>
            <%})           
           .Render();
    %>

<script type="text/javascript" language="javascript">
    var isUpdateColor = false;
    var testRegisterRowId = -1;
    var certificateRowId = -1;
    var refreshProvaider = false;

    function onProvaiderDataBinding(args) {
        if (refreshProvaider == false) args.preventDefault();
            else args.data = $.extend(args.data, { canUpdate: refreshProvaider });
    }

    function onDataBinding(e) {
        document.getElementById("ProvaiderInfo").innerHTML = "";
        document.getElementById("NomGroupInfo").innerHTML = "";
        document.getElementById("NomenclatureInfo").innerHTML = "";
        var data = new Array();
        var grid = $("#Certificates").data("tGrid");
        if (grid) {
            grid.dataBind(data);
        }
    }

    function onRowDataBound(e) {
        if (e.dataItem.Color != "") {
            e.row.style.backgroundColor = e.dataItem.Color;
        }
    }

    function onProvaiderRowDataBound(e) {
        $(e.row).dblclick(function () {
            var combobox = $("#ProvaiderCombo").data("tComboBox");
            combobox.value(e.dataItem.Id);
            combobox.text(e.dataItem.ProvaiderInfo);
            coloseWindow("ProvaidersWindow");
        })
    }

    function onCertificateDataBinding(args) {
        args.data = $.extend(args.data, { id: testRegisterRowId });
    }

    function onCertificateDataBound(e) {
        if (isUpdateColor) {
            updateParentGridColor();
        }
        isUpdateColor = false;
    }

    function onTestRegisterRowSelect(e) {
        var dataItem = jQuery('#TestRegisters').data('tGrid').dataItem(e.row);
        testRegisterRowId = dataItem['Id'];
        document.getElementById("ProvaiderInfo").innerHTML = dataItem.Provaider;
        document.getElementById("NomGroupInfo").innerHTML = dataItem.NomGroup;
        document.getElementById("NomenclatureInfo").innerHTML = dataItem.Model;
        $("#Certificates").data("tGrid").rebind();
    }

    function updateParentGridColor(){
        var data = $('#Certificates').data('tGrid').data;
        var minId=9999999;
        var rowColor="";
        $.each( data, function( index, row ) {
            if (row.Result.Id<minId) {
                minId = row.Result.Id;
                rowColor = row.Color;
            }
        });
        if (testRegisterRowId>0){
            var testRegisterGrid=$('#TestRegisters').data('tGrid');
            data = testRegisterGrid.data;
            $.each( data, function( index, row ) {
                if (row.Id == testRegisterRowId) {
                    var rows = testRegisterGrid.$rows();
                    rows[index].style.backgroundColor = rowColor;
                }
            });
        }
    }

    function delTestRegisterRecord(id) {
        cssConfirm("Удалить выбранную запись?", "Да", "Нет", function (state) {
            if (state) {
                $.ajax({
                    url: 'TestRegister/_delTestRegisterRecord',
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
                            $("#TestRegisters").data("tGrid").rebind();
                        }
                    }
                });
            }
        });
    }

    function delCertificateRecord(id) {
        cssConfirm("Удалить выбранную запись?", "Да", "Нет", function (state) {
            if (state) {
                $.ajax({
                    url: 'TestRegister/_delCertificateRecord',
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
                            isUpdateColor = true;
                            $("#Certificates").data("tGrid").rebind();
                        }
                    }
                });
            }
        });
    }

    function editCertificateRecord(id,organizationId,organizationName,docNum,docDate,descr,resultId) {
        document.getElementById("editCertificateRowId").value = id;
        if (Number(testRegisterRowId)<1) return;
        var window = $('#CertificateWindow').data('tWindow');
        if (id == -1) {
            window.title("Добавление акта");
            document.getElementById("DocNumber").value = "";
            document.getElementById("DocDate").value = "";
            document.getElementById("Descr").value = "";
            $("#ShopCombo").data("tComboBox").value("");
        } else {
            window.title("Редактирование акта");
            document.getElementById("DocNumber").value = docNum;
            document.getElementById("DocDate").value = docDate;
            document.getElementById("Descr").value = descr;
            var combobox = $("#ShopCombo").data("tComboBox");
            combobox.value(organizationId); 
            combobox.text(organizationName);
            var listbox = $("#ResultList").data("tDropDownList");
            listbox.value(resultId);             
        }
        window.center().open();
    }

    function editTestRegisterRecord(id, ProvaiderId, Provaider, NomGroupId, NomGroup, TestDate, Model) {
        document.getElementById("editTestRegisterRowId").value = id;
        var window = $('#TestRegisterWindow').data('tWindow');
        if (id == -1) {
            window.title("Добавление испытания");
            document.getElementById("TestDate").value="";
            document.getElementById("model").value="";
            var combobox = $("#ProvaiderCombo").data("tComboBox");
            combobox.value(""); 
            combobox.text("");
            combobox = $("#NomGroupCombo").data("tComboBox");
            combobox.value(""); 
            combobox.text("");
        } else {
            window.title("Редактирование испытания");
            document.getElementById("TestDate").value=TestDate;
            document.getElementById("model").value=Model;
            var combobox = $("#ProvaiderCombo").data("tComboBox");
            combobox.value(ProvaiderId); 
            combobox.text(Provaider);
            combobox = $("#NomGroupCombo").data("tComboBox");
            combobox.value(NomGroupId); 
            combobox.text(NomGroup);
        }
        window.center().open();
    }

    function saveTestRegisterData() {
        var recordId = document.getElementById("editTestRegisterRowId").value;
        var provaiderId = "";
        var obj = $("#ProvaiderCombo").data("tComboBox");
        if (obj.value() != obj.text())
        {
            provaiderId = obj.value();
        }
        if (provaiderId == "") {
            alert("Поставщик не выбран!");
            return false;
        }
        var nomGroupId = "";
        obj = $("#NomGroupCombo").data("tComboBox");
        if (obj.value() != obj.text())
        {
            nomGroupId = obj.value();
        }
        if (nomGroupId == "") {
            alert("Группа номенклатур не выбран!");
            return false;
        }

        var testDate = document.getElementById("testDate").value;
        if (testDate=="") {
            alert("Не указана дата ввода!");
            return false;
        }

        var model = document.getElementById("model").value;
        if (model=="") {
            alert("Введите номенклатуру!");
            return false;
        }

        $.ajax({
            url: 'TestRegister/_addOrEditTestRegister',
            contentType: 'application/x-www-form-urlencoded',
            type: 'POST',
            dataType: 'json',
            error: function (xhr, status) {
                onGridErrorEvent(xhr);
            },
            data: {
                id: recordId,
                testDate: testDate,
                provaiderId: provaiderId,
                nomGroupId: nomGroupId,
                model: model
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
                    isUpdateColor = false;
                    testRegisterRowId = -1;
                    certificateRowId = -1;
                    $("#TestRegisters").data("tGrid").rebind();
                    $("#Certificates").data("tGrid").rebind();
                    coloseWindow("TestRegisterWindow");
                }
            }
        });
    }

    function saveCertificateData() {
        var recordId = document.getElementById("editCertificateRowId").value;
        var orgId = "";
        var shop = $("#ShopCombo").data("tComboBox");
        if (shop.value() != shop.text())
        {
            orgId = shop.value();
        }
        if (orgId == "") {
            alert("Подразделение не выбрано!");
            return false;
        }
        var docNum = document.getElementById("DocNumber").value;
        if (docNum == "") {
            alert("Не введен № документа!");
            return false;
        }
        var docDate = document.getElementById("DocDate").value;
        if (docDate == "") {
            alert("Не введена дата документа!");
            return false;
        }
        var resultId = $("#ResultList").data("tDropDownList").value();
        var descr = document.getElementById("descr").value;
        $.ajax({
            url: 'TestRegister/_addOrEditCertificat',
            contentType: 'application/x-www-form-urlencoded',
            type: 'POST',
            dataType: 'json',
            error: function (xhr, status) {
                onGridErrorEvent(xhr);
            },
            data: {
                id: recordId,
                pid: testRegisterRowId,
                orgId: orgId,
                DocNum: docNum,
                DocDate: docDate,
                ResultId: resultId,
                Descr: descr
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
                    isUpdateColor = true;
                    $("#Certificates").data("tGrid").rebind();
                    coloseWindow("CertificateWindow");
                }
            }
        });

    }

    function showProvaiderWindow() {
        refreshProvaider = true;
        $("#ProvaiderGrid").data("tGrid").rebind();
        var window = $('#ProvaidersWindow').data('tWindow');
        window.center().open();
    }

    function coloseWindow(windowName) {
        var window = $('#' + windowName).data('tWindow');
        if (window) {
            window.close();
        }
    }
</script>
</asp:Content>