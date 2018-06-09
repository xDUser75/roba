	<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master"  %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <script type="text/javascript" src="Scripts/Base.js"></script>
    <script type="text/javascript" src="Scripts/Lock.js"></script>

 <h3>Загрузка Сотрудниц в отпуске по уходу за ребенком, структуры организации</h3>
    <table> 
        <tr>
            <td>Выберите цех&nbsp;</td>
            <td>
              <%=
                  Html.Telerik().ComboBox()
                         .Name("Shops").AutoFill(true)
                         .HtmlAttributes(new { style = "width: 600px" })
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
            <td><%= Html.Button("btnSave", "Запросить данные", HtmlButtonType.Submit,"onClick();")%>
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan=2>
               <font color="red"><i>Выберите цех и нажмите на кнопку "Запросить данные"</i></font>
                <br/><b>Загрузка  Сотрудниц в отпуске по уходу за ребенком, структуры и персональных данных может занять несколько минут</b>
            </td>
        </tr>    
    </table>

<script type="text/javascript">
    function onClick() {
        if (document.getElementById('Shops').value == '') {
            alert('Введите код цеха!!!')
        }
        else {
            disabled = true;
            if (nkmk.Lock.isFree()) {
                nkmk.Lock.setBusy();
                nkmk.Lock.printBusyMessage('Идет загрузка структуры и персональных данных...');
            }
            var shopId = document.getElementById('Shops').value;
            $.ajax({
                url: 'AM_SAPS/GetAM_SAP',
                contentType: 'application/x-www-form-urlencoded',
                type: 'POST',
                dataType: 'json',
                error: function (xhr, status) {
                    nkmk.Lock.setFree();
                    xhr.textStatus = 'error';
                    onGridErrorEvent(xhr);
                },
                data: {
                    shopId: shopId,
                    childCare: 1
                },
                success: function (result) {
                    nkmk.Lock.setFree();
                    if ((result) && (result.modelState)) {
                        var message = "Ошибки:\n";
                        $.each(result.modelState, function (key, value) {
                            if ('errors' in value) {
                                $.each(value.errors, function () {
                                    message += this + "\n";
                                });
                            }
                        });
                        alert("Произошла ошибка\n" + message);
                    } else {
                        alert("Загрузка прошла успешно!");
                    }
                }
            });
        }
    }
</script>

</asp:Content>
