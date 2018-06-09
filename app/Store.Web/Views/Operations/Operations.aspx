<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<Store.Core.OperationSimple>>" %>
<%@ Import Namespace="Store.Data" %>
<%@ Import Namespace="Store.Core" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<h3>Операции</h3>
<form name="form1" method="post">
    <table border="0" width="100%">
        <tr>
            <td colspan="4">            
            <%:
            Html.Telerik().DropDownList()
                          .Name("idStorage")
                          .BindTo((SelectList)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE_SHOP_NAME])
                          .HtmlAttributes(new { style = "width: 200px" })
                          .ClientEvents(events => events
                                  .OnChange("onUpdateStoreListBox")
                          )
             %>
            </td>
                        <td align="center" rowspan="4"><input type="submit" value="Найти" class="t-button"/></td>
            </tr>
            <tr>
            <td align="right">Дата:</td>
            <td>С
            <% 
                string dtEnd = (string)Session["dateEndString"];
                string dt = (string)Session["dateString"];                
            %>

            <%= Html.Telerik().DatePicker()
                     .Name("date")
                      .Value(dt)
                     .Format(DataGlobals.DATE_FORMAT)
            %>
            По
            <%= Html.Telerik().DatePicker()
                     .Name("dateEnd")
                     .Value(dtEnd)
                     .Format(DataGlobals.DATE_FORMAT)
            %>
            </td>
        <td align="right">Тип операции:</td>
        <td>
<%: Html.Telerik().DropDownList()
                .Name("OperTypes")
                .BindTo((SelectList)ViewData[Store.Data.DataGlobals.REFERENCE_OPER_TYPE])
                .HtmlAttributes(new { style = "width: 300px" })
 %>        
        </td>           
        </tr>
        <tr> 
        <td align="right">Цех:</td>
        <td colspan="3">
              <%:
              Html.Telerik().ComboBox()
                     .Name("Shops")
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
        </tr>
        <tr> 
        <td align="right">Номенклатура:</td>
        <td colspan="3">
              <%:
              Html.Telerik().ComboBox()
                     .Name("Nomenclature")
                     .HtmlAttributes(new { style = "width: 600px" })
                     .DataBinding(binding => binding.Ajax()
                                                          .Select("_GetNomenclatures", "Operations"))
                    .SelectedIndex(2)
                    .Filterable(filtering =>
                        filtering
                        .FilterMode(AutoCompleteFilterMode.Contains)
                        .MinimumChars(3)
                    )
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)                                                    
               %>        
        </td>
        </tr>
    </table>
</form>
<%: 
   Html.Telerik()
        .Grid(Model)
        .Name("Operations")
            //.DataKeys(keys =>
            //{
            //    keys.Add(x => x.Id);
            //})
        .DataBinding(dataBinding => dataBinding.Ajax().Select("_Filtering", "Operations"))
        .Columns(columns =>
        {
          columns.Bound(x => x.OperDate)
              .Format("{0:" + DataGlobals.DATE_FORMAT + "}")
              .Filterable(false)
              .Width(60);
          columns.Bound(x => x.OperType)
            .Title("Oпер.")
            .Filterable(false)
            .Width(90);
          columns.Bound(x => x.ShopNumber)
              .Title("Цех")
              .Filterable(false)              
              .Width(45);
          columns.Bound(x => x.Fio)
//            .ClientTemplate("<#= Partner != null?Partner.Name: ((WorkerWorkplace.Worker.TabN!=0)?'['+ WorkerWorkplace.Worker.TabN + '] '+ WorkerWorkplace.Worker.Fio:'') #>")
            .Title("Таб ФИО")
            .Width(120);
          columns.Bound(x => x.Nomenclature)
            .Title("Номенклатура")
            .Filterable(false)
            .Width(350);
          columns.Bound(x => x.DocNumber)
              .Title("№док")
              .Width(80);
          columns.Bound(x => x.Quantity)
              .Filterable(false)
              .Title("Кол.")
              .Width(60);
          columns.Bound(x => x.Motiv)
//              .ClientTemplate("<#= Motiv != null?Motiv.Name:null #>")
              .Title("Основание");
          columns.Bound(x => x.Wear)
              .HtmlAttributes(new { @align = "center" })
//              .Template("<#= Wear=='100'?'новая':Wear=='50'?'б/у':Wear=='0'?'утиль':Wear #>")
              .Filterable(false)
              .Width(60);
        })
        .ClientEvents(events => events
            .OnDataBinding("dataBinding"))
        //.Editable(editing => editing.Mode(GridEditMode.InLine))
        //.Pageable()
        .Scrollable(x => x.Height(400))
        //.Groupable()
        //.Resizable(resizing => resizing.Columns(true))
        .Filterable()
        .Sortable()
               
%>
<script type="text/javascript">
    function dataBinding(args) {
        var param = document.getElementById('date').value;
        var paramEnd = document.getElementById('dateEnd').value;
        var dropDownList = $("#idStorage").data("tDropDownList");
        var storageId = "";
        if (dropDownList) {
            storageId = dropDownList.value();
        }
        if (storageId == "") {
            storageId = "-1";
        }

        var dropDownList1 = $("#OperTypes").data("tDropDownList");
        var OperTypeId = "";
        if (dropDownList1) {
            OperTypeId = dropDownList1.value();
        }
        if (OperTypeId == "") {
            OperTypeId = "-1";
        }

        var shopNumber = $("#Shops").data("tComboBox").value();
        var nomenclatureId = $("#Nomenclature").data("tComboBox").value();
        args.data = $.extend(args.data, { dateString: param, dateEndString: paramEnd, idStorage: storageId, operType: OperTypeId, Shops: shopNumber, Nomenclature: nomenclatureId });
    }

    function onUpdateStoreListBox(e) {
        document.forms["form1"].submit();
    }

    window.onresize = onFormResize;

    function onFormResize() {
        $("#Shops").data("tComboBox").disable();
        $("#Shops").data("tComboBox").enable();
        $("#Nomenclature").data("tComboBox").disable();
        $("#Nomenclature").data("tComboBox").enable();
    }
</script>
</asp:Content>
