<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Store.Data" %>
<%@ Import Namespace="Store.Core" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <script type="text/javascript" src="Scripts/Base.js"></script>
    <script type="text/javascript" src="Scripts/Lock.js"></script>
<h3>Остатки по месяцам</h3>
    <table border="0" width="100%">
        <tr>
        <td align="right">Склад:</td>
            <td>            
            <%:
            Html.Telerik().DropDownList()
                           .Name("DropDownList")
                           .BindTo((SelectList)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE_SHOP_NAME])
                          .HtmlAttributes(new { style = "width: 250px" })
             %>
            </td>
            <td align="right">Дата:</td>
            <td>
            <%= Html.Telerik().DatePicker()
                     .Name("date")
                     .Value(DateTime.Today)
                     .Format("01.MM.yyyy")
            %>
            </td>
            <td rowspan="2"><input type="button" value="Найти" class="t-button" onclick="findRemains()"/></td>
<%if ((HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_ADMIN) ||
       HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_STORAGE_MONTH_EDIT)))
  {%>

            <td align="right" rowspan="2" width="300"><input type="button" value="Зафиксировать остатки" class="t-button" onclick="saveRemains()"/></td>
<%}%>
        </tr>   
        <tr>
            <td>Номенклатура:</td>
            <td colspan="3">
      <%:
              Html.Telerik().ComboBox()
                     .Name("Nomenclature")
                     .HtmlAttributes(new { style = "width: 600px" })
                     .DataBinding(binding => binding.Ajax()
                                .Select("_GetNomenclatures", "Remains"))
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
    </table>
<%: 
   Html.Telerik().Grid<Store.Core.RemaindEx>()
            .Name("RemainsGrid")
            .DataBinding(dataBinding =>
              dataBinding
              .Ajax()
              .Select("_SelectionClientSide_Remains", "Remains")
             )
        .Columns(columns =>
        {
          columns.Bound(x => x.RemaindDate)
              .Format("{0:" + DataGlobals.DATE_FORMAT + "}")
              .Width(60);
          columns.Bound(x => x.Nomenclature.ExternalCode)
            .Width(80);
          columns.Bound(x => x.Nomenclature.Name)
            .ClientTemplate("<#= Nomenclature==null?'':Nomenclature.Name #>")
            .Title("Номенклатура");
          columns.Bound(x => x.Nomenclature.Sex.Name)
            .ClientTemplate("<#= Nomenclature.Sex==null?'':Nomenclature.Sex.Name #>")
            .Width(80)
            .Title("Пол");
          columns.Bound(x => x.NomBodyPartSize.SizeNumber)
            .ClientTemplate("<#= NomBodyPartSize==null?'':NomBodyPartSize.SizeNumber#>")
            .Title("Размер")
            .Width(80);
          columns.Bound(x => x.Growth)
            .ClientTemplate("<#= Growth==null?'':Growth.SizeNumber#>")
            .Title("Рост")
            .Filterable(false)
            .Width(80);           
          columns.Bound(x => x.Quantity)
              .Filterable(false)
              .Title("Кол-во")
              .Width(85);
          columns.Bound(x => x.StringWear)
              .HtmlAttributes(new { @align = "center" })
              .Title("Износ")
              .Width(80);
          columns.Bound(x => x.QuantityE)
              .Filterable(false)
              .Width(110);

        })
        .ClientEvents(events => events
            .OnDataBinding("dataBinding")
            .OnError("onError")
            .OnDataBound("onDataBound")
            )
        //.Pageable()
        .Scrollable(x => x.Height(400))
        //.Resizable(resizing => resizing.Columns(true))
        .Sortable()
        .Filterable()
        
%>
<script type="text/javascript">
<%if ((HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_ADMIN) ||
       HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_STORAGE_MONTH_EDIT)))
  {%>
    function saveRemains() {
        var date = document.getElementById('date').value;
        if (date=="") {
            alert("Дата не выбрана!");
            return false;
        }

        if (date.length!=10) {
            alert("Дата задана в ошибочном формате!");
            return false;
        }

        if (date.indexOf("01")!=0) {
            alert("Дата должна быть первым числом месяца!");
            return false;
        }
        var nomenclatureId = $("#Nomenclature").data("tComboBox").value();
        if (nomenclatureId != ''){
            if (!window.confirm("Внимание!\nСнятие остатков будет происходит только по выбранной номенклатуре!\nПродолжить?")) return false;            
        }
        var curDate = new Date();
        var selDate = new Date(date.substr(6),Number(date.substr(3,2))-1,date.substr(0,2));
        if (Math.abs(curDate.getTime()-selDate.getTime())>3196800000) {
            if (!window.confirm("Внимание!\nВы пытаетесь снять остатки за сильно старую дату.\nСтарые данные будут утеряны!\nПродолжить?")) return false;
        } else {
           if (!window.confirm("Вы подтверждаете проведение операции по снятию остатков на складе!\nПродолжить?")) return false;
        }
        if (nkmk.Lock.isFree()) {
            nkmk.Lock.setBusy();
            nkmk.Lock.printBusyMessage("Идет снятие остатков на складе...<br/>Этот процесс может занять около 15 мин.");        
        }
        var grid=$("#RemainsGrid").data("tGrid");
        grid.ajax.selectUrl = "Remains/_SaveRemains";
        if (date != "") {
            grid.rebind({
                dateString: date
            });
            updateStatus();
        }
    }
<%}%>
var timerId;
    function findRemains() {
        var date = document.getElementById('date').value;
        var nomenclatureId = $("#Nomenclature").data("tComboBox").value();
        var grid=$("#RemainsGrid").data("tGrid");
        grid.ajax.selectUrl = "Remains/_SelectionClientSide_Remains";
        if (date != "") {
            grid.rebind({
                dateString: date,
                Nomenclature: nomenclatureId
            });
        }
    }

    function dataBinding(args) {
        var param = document.getElementById('date').value;
        var nomenclatureId = $("#Nomenclature").data("tComboBox").value();
        var dropDownList = $("#DropDownList").data("tDropDownList");
        var storageId = "";
        if (dropDownList) {
            storageId = dropDownList.value();
        }
        if (storageId == "") {
            storageId = "-1";
        }
        args.data = $.extend(args.data, { date: param, idStorage: storageId,Nomenclature: nomenclatureId });
    }

    function onError(e) {
        nkmk.Lock.setFree();
        onGridErrorEvent(e);
        clearInterval(timerId);
    }

    function onDataBound(e){
        nkmk.Lock.setFree();
        clearInterval(timerId);
    }
    var callTimes=0;
    function updateStatus(){
        if (!nkmk.Lock.isFree()) {
      var intervalId = setInterval(function () {    
        $.post("Remains/_Progress", {}, function (progress) {
          if (progress == "100%") {
            nkmk.Lock.setFree();
            clearInterval(intervalId);
          } else {
            if (!nkmk.Lock.isFree()) {
                nkmk.Lock.printBusyMessage("Идет снятие остатков на складе...<br/>"+progress);
            }
          }
        });
      }, 30000);
        }
    }
</script>
</asp:Content>
