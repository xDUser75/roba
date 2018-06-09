	<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master"  %>
<%@ Import Namespace="Store.Core" %>
<%@ Import Namespace="Store.Data" %>
<%@ Import Namespace="Store.Core.Account" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <script type="text/javascript" src="Scripts/Base.js"></script>
    <script type="text/javascript" src="Scripts/Lock.js"></script>
<h3>Выполнение вспомогательных процедур БД</h3>
<hr />

<h3>Массовый перевод без проверки на соответствия новой норме</h3>
<form name="frm" action="">
<table>
        <tr>
            <td>Старый Цех:</td>
            <td>
<%=
                  Html.Telerik().ComboBox()
                         .Name("OldShops").AutoFill(true)
                         .HtmlAttributes(new { style = "width: 400px" })
                         .DataBinding(binding => binding.Ajax()
                                                        .Select("_GetAllShops", "LoadData"))
                         .SelectedIndex(2)
                        .Filterable(filtering =>
                            filtering
                            .FilterMode(AutoCompleteFilterMode.Contains)
                            .MinimumChars(1)
                        )
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
                                                    
               %>
            </td> 
            <td>Новый Цех:</td>
            <td>
<%=
                  Html.Telerik().ComboBox()
                         .Name("NewShops").AutoFill(true)
                         .HtmlAttributes(new { style = "width: 400px" })
                         .DataBinding(binding => binding.Ajax()
                                                        .Select("_GetShops", "AM_SAPS"))
                         .SelectedIndex(2)
                        .Filterable(filtering =>
                            filtering
                            .FilterMode(AutoCompleteFilterMode.Contains)
                            .MinimumChars(1)
                        )
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
                                                    
               %>
            </td> 
        </tr>
<tr>
        <td valign="middle" colspan="4">
            Использовать склады приписки:
           <input type="radio" value="1" name="radio"  onclick="radioClick()" />Да
           <input type="radio" value="0" name="radio"  onclick="radioClick()" checked />Нет
        </td>
</tr>

    <tr id="storage" >
        <td valign="middle" width="60px">
            Старый Склад:
        </td>
        <td>
            <%:
            Html.Telerik().DropDownList()
                .Name("OldStorageNameList")
                .BindTo((SelectList)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE_SHOP_NAME])
                .HtmlAttributes(new { style = "width: 400px" })
             %>
        </td>
        <td valign="middle" width="60px">
            Новый Склад:
        </td>
        <td>
            <%:
            Html.Telerik().DropDownList()
                .Name("NewStorageNameList")
                .BindTo((SelectList)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE_SHOP_NAME])
                .HtmlAttributes(new { style = "width: 400px" })
             %>
        </td>

    </tr>


    <tr>
    <td>Таб. № </td>
    <td><input name ="tabnTransfer" type="text" value="" /></td>

    <td>Дата перевода</td>
    <td>
               <%= Html.Telerik().DatePicker()
                                 .Name("transferDate")
                                 .HtmlAttributes(new { id = "transferDate_wrapper", style = "vertical-align: middle;" })
                                 .InputHtmlAttributes(new { size = "4" }) // не работает
                                 .Value(DateTime.Today)
                                 .Format(DataGlobals.DATE_FORMAT)
                %>
    </td>
</tr>
</table>
</form>
                &nbsp;
                <%= Html.Button("btnLoadTransfer", "Запустить процедуру", HtmlButtonType.Submit,
                                                           "onClickTransfer();")%>

              
<hr />

<h3>Привязка позиций на руках к нормам</h3>
    <table> 
        <tr>
                <td>Таб. № </td>
                <td><input name ="tabn" type="text" value="" /><td>
        </tr>
        <tr>
            <td> Цех:</td>
            <td>
<%=
                  Html.Telerik().ComboBox()
                         .Name("Shops").AutoFill(true)
                         .HtmlAttributes(new { style = "width: 400px" })
                         .DataBinding(binding => binding.Ajax()
                                                        .Select("_GetShops", "AM_SAPS"))
                         .SelectedIndex(2)
                        .Filterable(filtering =>
                            filtering
                            .FilterMode(AutoCompleteFilterMode.Contains)
                            .MinimumChars(1)
                        )
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
                                                    
               %>
            </td> 
        </tr>
        <tr>
            <td> Норма:</td>
            <td>
                <%=
                  Html.Telerik().ComboBox()
                         .Name("Normas").AutoFill(true)
                         .HtmlAttributes(new { style = "width: 400px" })
                         .DataBinding(binding => binding.Ajax().Select("_GetNormas", "LoadData"))
                      .SelectedIndex(2)
                        .Filterable(filtering =>
                            filtering
                            .FilterMode(AutoCompleteFilterMode.Contains)
                            .MinimumChars(4)
                        )
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
                                                    
               %>
            </td> 
        </tr>
        <tr>
            <td>Группы номенклатур</td>
            <td>
            <%=
                  Html.Telerik().ComboBox()
                         .Name("Nomgroups").AutoFill(true)
                         .HtmlAttributes(new { style = "width: 400px" })
                         .DataBinding(binding => binding.Ajax()
                         .Select("_GetNomGroups", "Normas"))
                      .SelectedIndex(2)
                        .Filterable(filtering =>
                            filtering
                            .FilterMode(AutoCompleteFilterMode.Contains)
                            .MinimumChars(1)
                        )
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
                                                    
               %>
            </td>
        </tr>
        <tr>
            <td colspan=2  align="center" valign="middle">
                &nbsp;
            </td>   
        </tr>
        <tr>
            <td colspan=2  align="center" valign="middle">
                <%= Html.Button("btnLoadNom", "Привязать к действующей норме", HtmlButtonType.Submit,
                                            "onClick();")%>
            </td>   
        </tr>

    
    </table>
<hr />

<h3>Номенклатурный справочник</h3>
    Обновление номенклатурного справочника из внешней системы&nbsp;
    <%= Html.Button("btnLoadNom", "Обновить справочник", HtmlButtonType.Submit,
                                             "onClickLoadNom();")%>
<hr />

<h3>Списание по сроку</h3>

   Запуск процедуры списания по сроку&nbsp;
   <%= Html.Button("btnLoadOut", "Запустить процедуру", HtmlButtonType.Submit,
                                             "onClickWorkerOut();")%>
<hr />

<h3>Списание c уволеных</h3>
               <label for="demissDate">Выберите месяц(число любое)</label>
               <%= Html.Telerik().DatePicker()
                                 .Name("demissDate")
                                 .HtmlAttributes(new { id = "demissDate_wrapper", style = "vertical-align: middle;" })
                                 .InputHtmlAttributes(new { size = "4" }) // не работает
                                 .Value(DateTime.Today)
                                 .Format(DataGlobals.DATE_FORMAT)
                %>
                &nbsp;
                <%= Html.Button("btnLoadDemiss", "Запустить процедуру", HtmlButtonType.Submit,
                                             "onClickWorkerCardDismiss();")%>

               <br /><font color="red"><i>Списание проведется по работникам, уволенным ранее 01 числа предыдущего месяца</i></font>
<hr />

<h3>Списание с МОЛ</h3>

   Запуск процедуры списания с МОЛ. Спишется все, что есть на руках на момент запуска процедуры.
   &nbsp;
    <table>
     <tr>
            <td>Цех/Подразделение:</td>
            <td>
<%=
                  Html.Telerik().ComboBox()
                         .Name("MolShops").AutoFill(true)
                         .HtmlAttributes(new { style = "width: 400px" })
                         .DataBinding(binding => binding.Ajax()
                                                        .Select("_GetAllShops", "LoadData"))
                         .SelectedIndex(2)
                        .Filterable(filtering =>
                            filtering
                            .FilterMode(AutoCompleteFilterMode.Contains)
                            .MinimumChars(1)
                        )
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
                                                    
               %>
            </td> 
        </tr>
    <tr id="Tr1" >
        <td valign="middle" width="60px">
            Склад:
        </td>
        <td>
            <%:
            Html.Telerik().DropDownList()
                .Name("MolStorageNameList")
                .BindTo((SelectList)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE_SHOP_NAME])
                .HtmlAttributes(new { style = "width: 400px" })
             %>
        </td>
    </tr>
    </table>
    &nbsp;
   <%= Html.Button("btnLoadOut", "Запустить процедуру", HtmlButtonType.Submit,
                                                    "onClickMatPersonCardOut();")%>
<hr />


<h3>Добавление группы замены к существующим нормам</h3>
    <table> 
        <tr>
            <td> Цех:</td>
            <td>
<%=
                  Html.Telerik().ComboBox()
                         .Name("Shops1").AutoFill(true)
                         .HtmlAttributes(new { style = "width: 400px" })
                         .DataBinding(binding => binding.Ajax()
                                                        .Select("_GetShops", "AM_SAPS"))
                         .SelectedIndex(2)
                        .Filterable(filtering =>
                            filtering
                            .FilterMode(AutoCompleteFilterMode.Contains)
                            .MinimumChars(1)
                        )
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
                                                    
               %>
            </td> 
        </tr>
        <tr>
            <td>Группы номенклатур</td>
            <td>
            <%=
                  Html.Telerik().ComboBox()
                         .Name("Nomgroups1").AutoFill(true)
                         .HtmlAttributes(new { style = "width: 400px" })
                         .DataBinding(binding => binding.Ajax()
                         .Select("_GetNomGroups", "Normas"))
                      .SelectedIndex(2)
                        .Filterable(filtering =>
                            filtering
                            .FilterMode(AutoCompleteFilterMode.Contains)
                            .MinimumChars(1)
                        )
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
                                                    
               %>
            </td>
        </tr>
        <tr>
            <td>Группа замены</td>
            <td>
            <%=
                  Html.Telerik().ComboBox()
                         .Name("AddNomgroups").AutoFill(true)
                         .HtmlAttributes(new { style = "width: 400px" })
                         .DataBinding(binding => binding.Ajax()
                         .Select("_GetNomGroups", "Normas"))
                      .SelectedIndex(2)
                        .Filterable(filtering =>
                            filtering
                            .FilterMode(AutoCompleteFilterMode.Contains)
                            .MinimumChars(1)
                        )
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
                                                    
               %>
            </td>
        </tr>
        <tr>
            <td> Норма:</td>
            <td>
                <%=
                  Html.Telerik().ComboBox()
                         .Name("Normas1").AutoFill(true)
                         .HtmlAttributes(new { style = "width: 400px" })
                         .DataBinding(binding => binding.Ajax().Select("_GetNormas", "LoadData"))
                      .SelectedIndex(2)
                        .Filterable(filtering =>
                            filtering
                            .FilterMode(AutoCompleteFilterMode.Contains)
                            .MinimumChars(4)
                        )
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
                                                    
               %>
            </td> 
        </tr>

        <tr>
            <td colspan=2  align="center" valign="middle">
                &nbsp;
            </td>   
        </tr>
        <tr>
            <td colspan=2  align="center" valign="middle">
                <%= Html.Button("btnLoadAddNomGroup", "Добавить группу замены", HtmlButtonType.Submit, "onClickAddGroup();")%>
            </td>   
        </tr>

    
    </table>
<hr />


<script type="text/javascript">
    function radioClick() {
        if (document.frm.radio[0].checked)
            document.getElementById("storage").disabled = true;
        else
            document.getElementById("storage").disabled = false;
    }
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

    function onClickLoadNom() {
            disabled = true;
            if (nkmk.Lock.isFree()) {
                nkmk.Lock.setBusy();
                nkmk.Lock.printBusyMessage('Идет обновление справочника номенклатур...');
            }

            if (!sendRequest("LoadData/LoadNomenclatureFromSAP", "" , checkStatus)) {
                alert("Не могу вызвать запрос на обновление справочника!");
            }
                }

    function onClick() {
        disabled = true;
        if (document.getElementById("NomGroups").value == ""
             && document.getElementById("Shops").value == ""
             && document.getElementById("tabn").value == ""
             && document.getElementById("Normas").value == "") {
            alert("Ввведите параметр");
        }
        else {
            if (nkmk.Lock.isFree()) {
                nkmk.Lock.setBusy();
                nkmk.Lock.printBusyMessage('Идет привязка выданного на руки к актуальной норме...');
            }
            if (!sendRequest("LoadData/LoadNormaContent", "p_shopid=" + document.getElementById('Shops').value + "&p_nomgroupList=" + document.getElementById('Nomgroups').value + "&p_tabn=" + document.getElementById('tabn').value + "&p_normaid=" + document.getElementById('Normas').value, checkStatus)) {
                alert("Не могу вызвать запрос на выполнение процедуры!");
            }
        }
    }

    function onClickWorkerOut() {
        disabled = true;
        if (nkmk.Lock.isFree()) {
            nkmk.Lock.setBusy();
            nkmk.Lock.printBusyMessage('Идет выполнение процедуры списания по сроку...');
        }
        if (!sendRequest("LoadData/LoadWorkerCardOut", "", checkStatus)) {
            alert("Не могу вызвать запрос на выполнение процедуры!");
        }
    }

    function onClickMatPersonCardOut() {
        disabled = true;
        if (document.getElementById("MolShops").value == ""
             && document.getElementById("MolStorageNameList").value == ""
             ) {
            alert("Ввведите параметры Цех и Склад");
        }
        if (nkmk.Lock.isFree()) {
            nkmk.Lock.setBusy();
            nkmk.Lock.printBusyMessage('Идет выполнение процедуры списания с МОЛ...');
        }
        if (!sendRequest("LoadData/LoadMatpersonCardOut", "p_shopid= " + document.getElementById('MolShops').value + "&p_storagenameid=" + document.getElementById('MolStorageNameList').value, checkStatus)) {
            alert("Не могу вызвать запрос на выполнение процедуры!");
        }
    }


    function onClickWorkerCardDismiss() {
        disabled = true;
        if (nkmk.Lock.isFree()) {
            nkmk.Lock.setBusy();
            nkmk.Lock.printBusyMessage('Идет выполнение процедуры списания с уволенных...');
        }
        if (!sendRequest("LoadData/LoadWorkerCardDismiss", "p_shopid= " + document.getElementById('demissDate').value, checkStatus)) {
            alert("Не могу вызвать запрос на выполнение процедуры!");
        }
    }

    function onClickTransfer() {
        disabled = true;
        var radio=0;
        if (document.frm.radio[0].checked)
            radio = document.frm.radio[0].value;
        else
            radio = document.frm.radio[1].value;
        if (nkmk.Lock.isFree()) {
            nkmk.Lock.setBusy();
            nkmk.Lock.printBusyMessage('Идет выполнение процедуры массового перевода...');
        }
        if (!sendRequest("LoadData/LoadTransfer", "p_shopid_old=" + document.getElementById('OldShops').value + "&p_shopid_new=" + document.getElementById('NewShops').value + "&p_storagenameid_old=" + document.getElementById('OldStorageNameList').value + "&p_storagenameid_new=" + document.getElementById('NewStorageNameList').value + "&p_tabn=" + document.getElementById('tabnTransfer').value + "&p_operdate=" + document.getElementById('transferDate').value + "&p_isstorageactive=" + radio, checkStatusTransfer)) {
            alert("Не могу вызвать запрос на выполнение процедуры!");
        }
    }

    function onClickAddGroup() {
        disabled = true;
        if (document.getElementById("NomGroups1").value == ""
             && document.getElementById("Shops1").value == ""
             && document.getElementById("AddNomGroups").value == "") {
            alert("Ввведите параметр");
        }
        else {
            if (nkmk.Lock.isFree()) {
                nkmk.Lock.setBusy();
                nkmk.Lock.printBusyMessage('Идет добавление групп замены...');
            }
            if (!sendRequest("LoadData/AddChangeNomGroup", "p_shopid=" + document.getElementById('Shops1').value + "&p_nomgroupId=" + document.getElementById('Nomgroups1').value + "&add_nomgroup=" + document.getElementById('AddNomgroups').value + "&p_normaid=" + document.getElementById('Normas1').value, checkStatus)) {
                alert("Не могу вызвать запрос на выполнение процедуры!");
            }
        }
    }
    function sendRequest(url, param, callBackFunction) {
        req = false;
        try {
            req = new ActiveXObject('Msxml2.XMLHTTP');
        } catch (e) {
            try {
                req = new ActiveXObject('Microsoft.XMLHTTP');
            } catch (e) {
                if (window.XMLHttpRequest) {
                    req = new XMLHttpRequest();
                }
            }
        }
        if (req) {
            req.onreadystatechange = callBackFunction;
            req.open('POST', url, true);
            req.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
            req.send(param);
            return true;
        }
        return false;
    }


    function checkStatus() {
        if (req.readyState == 4) {
            nkmk.Lock.setFree();
            if (req.status == 200) {
                alert("Процедура выполнена успешно!!!");
            } else {
                alert("Произошла ошибка выполнения процедуры " + req.status + ":\n" + req.statusText);
            }
        }
    }


    function checkStatusTransfer() {
        if (req.readyState == 4) {
            nkmk.Lock.setFree();
            if (req.status == 200) {
                var w = req.responseText.substring(26);
                var q = parseInt(w.indexOf("n")) - 1;
                var str = w.substring(0, q);
                alert(str);
            } else {
                alert("Произошла ошибка выполнения процедуры " + req.status + ":\n" + req.statusText);
            }
        }
    }


</script>

</asp:Content>
