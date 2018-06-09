<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" Culture="ru-RU"%>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<h3>Комисси, подписанты документов</h3>
<table width="100%">
 <tr>
        <td>Склад:</td>
        <td>

            <%:
            Html.Telerik().DropDownList()
                           .Name("StorageDropDownList")
                           .BindTo((SelectList)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE_SHOP_NAME])
                          .HtmlAttributes(new { style = "width: 400px" })
             %>
</td>
</tr>  
<tr>            
    <td>Выберите цех</td>
    <td>
        <%:
        Html.Telerik().ComboBox()
                .Name("Shops")
                .HtmlAttributes(new { style = "width: 400px" })
                .DataBinding(binding => binding.Ajax().Select("_GetShops", "SignDocumet")
                                            .Cache(false))
            .Filterable(filtering =>
                filtering
                .FilterMode(AutoCompleteFilterMode.Contains)
                .MinimumChars(2)
            )
            .Items(items =>
            {
                if (Session["shopNumber"] != null)
                    items.Add().Value("" + (string)Session["shopNumber"]).Text((string)Session["shopInfo"]);
            })
            .SelectedIndex(0)
            .HighlightFirstMatch(true)
            .OpenOnFocus(false)
                        .ClientEvents(events => events.OnChange("onChangeShop"))
            %>  
    </td>
</tr>
<tr>            
    <td>Подразделение</td>
    <td>
      <%:
        Html.Telerik().ComboBox()
                .Name("UnitsNew")
                .HtmlAttributes(new { style = "width: 400px" })
                .DataBinding(binding => binding.Ajax()
                                               .Select("_GetUnits", "SignDocumet")
                                               .Cache(false))
               .Filterable(filtering => filtering.FilterMode(AutoCompleteFilterMode.Contains)
                                                              .MinimumChars(0))
                .SelectedIndex(0)
                .HighlightFirstMatch(true)
                .OpenOnFocus(false)
                .ClientEvents(events => events.OnDataBinding("dataBindingUnit")
                                              .OnLoad("dataBindingUnit"))

            %>  
            <input type="button" value="Найти" class="t-button" onclick="findSignDocument()"/>                                
    </td>
</tr>

</table>
             <%:
         Html.Telerik().Grid<Store.Core.SignDocumet>()
                .Name("SignDocumet")
                .ToolBar(
                     commands =>
                         {
                           if (HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_ADMIN) ||
                               HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_SIGNDOCUMET_EDIT)
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
                      HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_SIGNDOCUMET_EDIT)
                     )
                  {
                    columns.Bound(x => x.Id)
                        .Title("")
                        .ClientTemplate("<a title='Редактировать' onClick='editRecord(<#= Id #>, <#=(SignDocType!=null)? SignDocType.Id:0 #>, \"<#=(SignDocType!=null)? SignDocType.Info:\"\" #>\", <#= (SignType!=null)?SignType.Id:0 #>, \"<#=(SignType!=null)? SignType.Name:\"\" #>\", \"<#= Value #>\", <#= (Worker!=null)?Worker.Id:0 #>, \"<#=(Worker!=null)? Worker.WorkerInfo:\"\" #>\",  \"<#= Fio #>\", \"<#= WorkPlaceName #>\" , <#= (Unit!=null)?Unit.Id:0 #>, \"<#=(Unit!=null)? Unit.Name:\"\" #>\", \"<#= Tabn#>\", <#= (Ord!=null)?Ord:1 #>)'  class='t-button t-button-icon' href='#'><span class='t-icon t-edit'></span></a><a onClick='delRecord(\"<#= Id #>\")' title='Удалить' class='t-button t-button-icon' href='#'><span class='t-icon t-delete'></span></a>")
                        .Filterable(false)
                        .Width("85px")
                        .Sortable(false);
                  }
                    columns.Bound(c => c.SignDocType.Info).Title("Документ")
                                                            .ClientTemplate("<#= (SignDocType==null?CodeDocumetn:SignDocType.Info)#>")
                                                            .Width(150).EditorTemplateName("SignDocTypesTemplate");

                   columns.Bound(c => c.Ord).Title("Порядок").Width(50);
                  columns.Bound(c => c.SignType.Name).Title("Подпись")
                                                          .ClientTemplate("<#= (SignType==null?NameSign:SignType.Name)#>")
                                                          .Width(150).EditorTemplateName("SignTypesTemplate");
                  columns.Bound(c => c.Value);
                  columns.Bound(c => c.Worker.WorkerInfo).Title("Подписант ЕВРАЗ").Width(200)
                                                         .ClientTemplate("<#= (Worker==null?'':Worker.WorkerInfo)#>")
                                                         .EditorTemplateName("WorkerByTabnTemplate");
                  columns.Bound(c => c.Fio);
                  columns.Bound(c => c.WorkPlaceName);
                  columns.Bound(c => c.Unit.Name).Title("Подразделение")
                                                 .Width(300)
                                                 .ClientTemplate("<#= (Unit==null?'':Unit.Name)#>")
                                                 .EditorTemplateName("SignUnitTemplate");
                    columns.Bound(c => c.Tabn).Hidden();
                })
                .DataBinding(dataBinding => dataBinding.Ajax()
                                .Select("_SelectSignDocumet", "SignDocumet")
                                .Insert("InsertContent", "Normas")
                                .Update("SaveContent", "Normas")
                                .Delete("DeleteContent", "Normas")
                                
                               
                  )
                  .Editable(editing => editing.Mode(GridEditMode.InLine))
                  //.Pageable(pager => pager.PageSize(20))
                  .Scrollable(x => x.Height("550px"))
                  .Sortable()
                  .Filterable()
                  .ClientEvents(events => events.OnDataBinding("dataBindingSignDocument")
                                        //.OnRowSelect("onRowSelected")
                                        //.OnRowDataBound("Norma_onRowDataBound")
                                        //.OnDelete("onRowDeleted")
                                        //.OnEdit("onRowEdit")
                                        .OnError("onError")
                                        )
                  
             %>
    <% 
// Окно редактирования записи
        Html.Telerik().Window()
           .Name("Window")
           .Width(408)
           .Height(525)
           .Draggable(true)
           .Modal(true)
           .Visible(false)
           .Buttons(b => b.Close())
           .Content(() =>
           { %>
           <input type="hidden" id="selectRecord"/>
           Цех:<br/>
            <%= Html.Telerik().ComboBox()
                     .Name("ShopCombo")
                     .AutoFill(false)
                     .Encode(false)
                     .DataBinding(binding => binding.Ajax()
                        .Select("_GetShops", "SignDocumet")
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
           Подразделение:<br/>
            <%= Html.Telerik().ComboBox()
                     .Name("UnitCombo")
                     .AutoFill(false)
                     .Encode(false)
                     .DataBinding(binding => binding.Ajax()
                         .Select("_GetUnits", "SignDocumet")
                        .Delay(600)
                    .Cache(false)
                        )
                     .ClientEvents(events => events.OnDataBinding("dataBindingUnit").OnLoad("dataBindingUnit"))   
                    .HtmlAttributes(new { @style = "width:350px" })
                    .Filterable(filtering =>
                        filtering
                        .FilterMode(AutoCompleteFilterMode.Contains)
                        .MinimumChars(0)
                    )
                    .SelectedIndex(0)
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
            %>
            <br/>
            Тип документа:<br/>
            <%=  Html.Telerik().DropDownList()
                .Name("CodeDocumetn")
                .HtmlAttributes(new { @style = "width:300px" })
                .BindTo(new SelectList((IEnumerable)ViewData[Store.Data.DataGlobals.REFERENCE_SIGNDOCTYPE], "Id", "Info"))
            %>
            <br/>
            Порядок:<br/>
                <input type="text" id="Ord" value="" size="10"/>
            <br/>
            Наименование подписи:<br/>
            <%=  Html.Telerik().DropDownList()
                 .Name("NameSign")
                 .HtmlAttributes(new { @style = "width:300px" })
                .BindTo(new SelectList((IEnumerable)ViewData[Store.Data.DataGlobals.REFERENCE_SIGNTYPE], "Id", "Name"))
            %>
            <br/>
            Текст распоряжения:<br/>
                <input type="text" id="Prikaz" value="" size="50"/>
            <br/>
           Подписант ЕВРАЗ:<br/>
            <%= Html.Telerik().ComboBox()
                     .Name("WorkerCombo")
                     .AutoFill(false)
                     .Encode(false)
                     .DataBinding(binding => binding.Ajax()
                        .Select("_GetWorker", "SignDocumet")
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
            <center><b>Сторонний подписант</b></center>
            ФИО стороннего:<br/>
                <input type="text" id="FIO" value="" size="30"/>
            <br/>
            Должность стороннего:<br/>
                <input type="text" id="WorkplaceName" value="" size="30"/>
            <input type="hidden" id="Tabn" value=""/>
            <input type="hidden" id="StorageNameId" value=""/>
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
    function onError(args) {
        if (args.textStatus == "modelstateerror" && args.modelState) {
            var message = "Ошибки:\n";
            $.each(args.modelState, function (key, value) {
                if ('errors' in value) {
                    $.each(value.errors, function () {
                        message += this + "\n";
                    });
                }
            });
            try {
                args.preventDefault();
            }
            catch (g) { }
            alert(message);
        } else {
            var xhr = args.XMLHttpRequest;
            if (args.textStatus = 'error') {
                var window = $('#Window_alert').data('tWindow');
                window.content(xhr.responseText).center().open();
            }
        }
    }
    function dataBindingUnit(e) {
        var shopId = $("#Shops").data("tComboBox").value();
        if (shopId > 0) {
            e.data = $.extend({}, e.data, { idShop: shopId });
        }
        //else {
        //    e.preventDefault();
        //    alert("Выберите сначала Цех!");
        //}
    }


    function findSignDocument() {
        var shopId = $("#Shops").data("tComboBox").value();
        $("#SignDocumet").data("tGrid").rebind();
    }

    function dataBindingSignDocument(args) {
        var shopId = $("#Shops").data("tComboBox").value();
        if (shopId == "")
            shopId = 0;
        var unitId = $("#UnitsNew").data("tComboBox").value();
        var storagenameId = $("#StorageDropDownList").data("tDropDownList").value();
        args.data = $.extend(args.data, { storagenameId: storagenameId, shopId: shopId, unitId: unitId });
    }
    function onChangeShop(args) {
        var unit = $("#UnitsNew").data("tComboBox");
        var shopId = $("#Shops").data("tComboBox").value();
        if (unit != null) {
            unit.dataBind({
                idShop: shopId
            });
            unit.reload();
        }
    }

    function delRecord(id) {
        cssConfirm("Удалить выбранную запись?", "Да", "Нет", function (state) {
            if (state) {
                $.ajax({
                    url: 'SignDocumet/_delRecord',
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
                            $("#SignDocumet").data("tGrid").rebind();
                        }
                    }
                });
            }
        });
    }

    function editRecord(id, SignDocTypeId, SignDocTypeInfo, SignTypeId, SignTypeName, Value, WorkerId, WorkerInfo, Fio, WorkPlaceName, UnitId, UnitName, Tabn, Ord) {
        document.getElementById("selectRecord").value = id;
        var window = $('#Window').data('tWindow');
        var shop = $("#Shops").data("tComboBox")
        var combobox = $("#ShopCombo").data("tComboBox");
        combobox.value(shop.value());
        combobox.text(shop.text());
        combobox.disable();
        document.getElementById("Ord").value = "";

        if (id == -1) {
            window.title("Добавление подписей");
            $("#UnitCombo").data("tComboBox").value("");
            $("#WorkerCombo").data("tComboBox").value("");
            $("#NameSign").data("tDropDownList").value("");
            $("#CodeDocumetn").data("tDropDownList").value("");
            document.getElementById("Prikaz").value = "";
            document.getElementById("Tabn").value = "";
            document.getElementById("FIO").value = "";
            document.getElementById("WorkplaceName").value = "";
            document.getElementById("Ord").value = "";
        } else {
            window.title("Редактирование подписей");
            $("#UnitCombo").data("tComboBox").value(UnitId);
            $("#UnitCombo").data("tComboBox").text(UnitName);
            $("#WorkerCombo").data("tComboBox").value(WorkerId);
            $("#WorkerCombo").data("tComboBox").text(WorkerInfo);
            $("#NameSign").data("tDropDownList").value(SignTypeId);
            $("#NameSign").data("tDropDownList").text(SignTypeName);
            $("#CodeDocumetn").data("tDropDownList").value(SignDocTypeId);
            $("#CodeDocumetn").data("tDropDownList").text(SignDocTypeInfo);
            document.getElementById("Prikaz").value = Value;
            document.getElementById("Tabn").value = Tabn;
            document.getElementById("FIO").value = Fio;
            document.getElementById("WorkplaceName").value = WorkPlaceName;
            document.getElementById("Ord").value = Ord;
        }
        window.center().open();
    }

    function saveData() {
        var recordId = document.getElementById("selectRecord").value;
        var storagenameId = $("#StorageDropDownList").data("tDropDownList").value();
        var shopId = $("#ShopCombo").data("tComboBox").value();
        var unit = $("#UnitCombo").data("tComboBox");
        var signType = $("#NameSign").data("tDropDownList");
        var signDocType = $("#CodeDocumetn").data("tDropDownList");
        var unitId = "";
        var signTypeId = "";
        var signDocTypeId = "";
        var prikaz = document.getElementById("Prikaz").value;
        var fio = document.getElementById("FIO").value;
        var workplaceName = document.getElementById("WorkplaceName").value;
        var tabn = document.getElementById("Tabn").value;
        var ord = document.getElementById("Ord").value;

        if (unit.value() != unit.text()) {
            unitId = unit.value();
        }
        else
            unitId = 0;

        if (signType != null && signType.value() != signType.text()) {
            signTypeId = signType.value();
        }

        if (signDocType != null && signDocType.value() != signDocType.text()) {
            signDocTypeId = signDocType.value();
        }

        /*
        if (unitId == "") {
            alert("Подразделение не выбрано!");
            return false;
        }
        */
        if (signDocTypeId == "") {
            alert("Тип документа не выбран!");
            return false;
        }
        if (signTypeId == "") {
            alert("Тип подписи не выбран!");
            return false;
        }
        var workerId = -1;
        var combo = $("#WorkerCombo").data("tComboBox");
        if (combo != null && combo.value() != combo.text()) {
            workerId = combo.value();
        }

        $.ajax({
            url: 'SignDocumet/_addOrEditRecord',
            contentType: 'application/x-www-form-urlencoded',
            type: 'POST',
            dataType: 'json',
            error: function (xhr, status) {
                onGridErrorEvent(xhr);
            },
            data: {
                id: recordId,
                unitId: unitId,
                signTypeId: signTypeId,
                signDocTypeId: signDocTypeId,
                prikaz: prikaz,
                fio: fio,
                workplaceName: workplaceName,
                tabn: tabn,
                workerId: workerId,
                ord: ord,
                storagenameId: storagenameId,
                shopId: shopId
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
                    $("#SignDocumet").data("tGrid").rebind();
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