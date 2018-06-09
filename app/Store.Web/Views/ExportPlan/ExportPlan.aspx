	<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master"  %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <script type="text/javascript" src="Scripts/Base.js"></script>
    <script type="text/javascript" src="Scripts/Lock.js"></script>


 <h3>Экспорт плана закупки спецодежды</h3>
    <table> 
        <tr>

            <td>
                   <fieldset>
                <legend>Период</legend>
                Начало:
               <%: Html.Telerik().DatePicker()
                      .Name("dtN")
                      .Value(DateTime.Now.ToShortDateString())
                      .Format(Store.Data.DataGlobals.DATE_FORMAT_FULL_YEAR)
                %>
                &nbsp;&nbsp;&nbsp;&nbsp;
                Окончание:
                <%: Html.Telerik().DatePicker()
                      .Name("dtK")
                      .Value(DateTime.Now.ToShortDateString())
                      .Format(Store.Data.DataGlobals.DATE_FORMAT_FULL_YEAR)
                %>
             </fieldset>            
            Выберите цех:&nbsp;
              <%=
                  Html.Telerik().ComboBox()
                         .Name("Shops").AutoFill(true)
                         .HtmlAttributes(new { style = "width: 600px" })
                         .DataBinding(binding => binding.Ajax()
                                                        .Select("_GetShops", "ExportPlan"))
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
            <td><%= Html.Button("btnSave", "Запросить данные", HtmlButtonType.Submit,
                                                         "onClick();")%>
            </td>

        </tr>
    </table>
    <form name="ExportParam" target="_blank" action="./ExportPlan/_Select">
        <input type="hidden" name="DateN"/>
        <input type="hidden" name="DateEnd"/>
        <input type="hidden" name="CexId" value="0"/>
    </form>
<script type="text/javascript">
    function onClick() {
        if (document.getElementById("Shops").value == '') {
            alert("Введите код цеха!!!");
            return false;
        }
        document.getElementById("DateN").value = document.getElementById("dtN").value;
        document.getElementById("DateEnd").value = document.getElementById("dtK").value;
        document.getElementById("CexId").value = document.getElementById("Shops").value;
        document.forms["ExportParam"].submit();
    }

</script>

</asp:Content>
