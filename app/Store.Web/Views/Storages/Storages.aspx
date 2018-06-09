<%@ Import Namespace="Store.Data" %>
<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" Culture="ru-RU"%>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <script type="text/javascript" src="<%: Url.Content("~/Scripts/Base.js")%>"></script>
    <script type="text/javascript" src="<%: Url.Content("~/Scripts/Lock.js")%>"></script>
<style type="text/css">
.t-mns
{
    background-image:url('<%: Url.Content("~/Content/Images/minus_button.gif")%>');
    width:16px;
}
</style>

<h3>Остатки на складах</h3>
<table border="0" cellpadding="0" cellspacing="0" width="100%">
    <tr>
        <td valign="middle" style="width:60px">
            Склад:
        </td>
        <td>
            <%:
            Html.Telerik().DropDownList()
                           .Name("DropDownList")
                           .BindTo((SelectList)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE_SHOP_NAME])
                          .HtmlAttributes(new { style = "width: 400px" })
                          .ClientEvents(events => events
                                  .OnChange("onUpdateStoreListBox")
                                  .OnLoad("onUpdateStoreListBox")
                          )
             %>
        </td>
        <td align="right" style="width:250px">     
<%if ((HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_ADMIN) ||
       HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_STORAGE_EDIT))) {%>
            <input type="button" value="Приход на склад" title="Импорт данных о приходе из внешней системы" class="t-button" onclick="inSapStorageClick()"/>&nbsp;&nbsp;&nbsp;
            
<%}%>

<%if ((HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_ADMIN) )) {%>
            <input type="button" value="Приход" title="Приход на склад" class="t-button" onclick="inStorageClick()"/>
<%}%>
        </td>
    </tr>
</table>
             <br/>
             <br/>
             Номенклатура на складе:
             <%:
         Html.Telerik().Grid((IEnumerable<Store.Core.Storage>)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE])
                .Name("Storages")
                .DataKeys(keys =>
                {
                  keys.Add(x => x.Id);
                })
                .Columns(columns=>
                {
                    if ((HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_ADMIN) ||
                           HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_STORAGE_EDIT)))
                    {
                    columns.Bound(x => x.Id)
                        .Width("100px")
                        .Title("")
                        .ClientTemplate("<a onClick='moveStorageClick(\"<#= Id #>\",\"<#= Quantity #>\",\"<#= Nomenclature.ExternalCode #>\",\"<#= Nomenclature.Name #>\")' title='Переместить на другой склад' class='t-button t-button-icon' href='#'><span class='t-icon t-mns'></span></a>")
                        .Filterable(false)
                        .Width("10px")
                        .Sortable(false);
                    }
                  columns.Bound(c => c.Nomenclature.ExternalCode).Width("105px").ClientTemplate("<#= Nomenclature.ExternalCode #>").Title("Номен.№").HtmlAttributes(new { @align = "center" });
                  columns.Bound(c => c.Nomenclature.NomGroup.Name).ClientTemplate("<#= Nomenclature.NomGroup.Name #>").Title("Группа");                     
                  columns.Bound(c => c.Nomenclature.Name).ClientTemplate("<#= Nomenclature.Name #>");
                  columns.Bound(c => c.Nomenclature.Unit.Name).Width("20px").HtmlAttributes(new { @align = "center" }).ClientTemplate("<#= Nomenclature.Unit!=null?Nomenclature.Unit.Name:'' #>").Title("Ед.изм.");
                  columns.Bound(c => c.Wear).Width("90px").HtmlAttributes(new { @align = "center" }).ClientTemplate("<#= Wear=='100'?'новая':Wear=='50'?'б/у':Wear=='0'?'утиль':Wear #>");
                  columns.Bound(c => c.Nomenclature.Sex.Name).Width("20px").HtmlAttributes(new { @align = "center" }).ClientTemplate("<#= Nomenclature.Sex!=null?Nomenclature.Sex.Name:'' #>").Title("Пол");
                  columns.Bound(c => c.Growth.SizeNumber).Width("75px").ClientTemplate("<#= Growth!=null?Growth.SizeNumber:null #>").HtmlAttributes(new { @align = "right" }).Title("Рост");
                  columns.Bound(c => c.NomBodyPartSize.SizeNumber).Width("55px").ClientTemplate("<#= NomBodyPartSize!=null?NomBodyPartSize.SizeNumber:'' #>").HtmlAttributes(new { @align = "right" });
                  columns.Bound(c => c.Quantity).Width("90px").HtmlAttributes(new { @align = "right" });                    
                })
                .DataBinding(dataBinding => dataBinding.Ajax()
                                                         .Select("_SelectionClientSide_Storage", "Storages")
                  )
                  .Editable(editing => editing.Mode(GridEditMode.InLine))
                  .Pageable(pager => pager.PageSize(20))
//                .Scrollable(x => x.Height("550px"))
                  .Sortable()
                  .Filterable()
             %>
     
    <% Html.Telerik().Window()
           .Name("Window")
           .Title("Операция по складу")
           .Width(600)
           .Height(350)
           .Draggable(true)
           .Modal(true)
           .Visible(false)
           .Buttons(b => b.Close())
           .Content(() =>
           {%>
    <fieldset>
        <legend>Документ</legend>
               <table border="0" cellpadding="1" cellspacing="1" width="100%">
               <tr  id="TrRow" style="display:none">
               <td colspan="4" align="center">&nbsp;</td>
               </tr>
               <tr>
               <td align="right">Тип</td>
               <td colspan="3">
               <%:
                   Html.Telerik().DropDownList()
                      .Name("DocTypeDropDownList")
                      .HtmlAttributes(new { style = "width: 300px" })
                      .BindTo(new SelectList((IEnumerable)ViewData[Store.Data.DataGlobals.REFERENCE_DOCUMENT_TYPE], "Id", "Name"))
                      .SelectedIndex(4)                      
               %>
               </td>
               </tr>
               <tr>
               <td align="right">Номер</td>
               <td>
                    <input type="text" id="DocNumber" maxlength="50" size="15"/>
               </td>
               <td align="right">Дата</td>
               <td>
               <%: Html.Telerik().DatePicker()
                      .Name("DocDate")
                      .Value(DateTime.Now.ToShortDateString())
                      .Format(Store.Data.DataGlobals.DATE_FORMAT_FULL_YEAR)
                %>
                </td>
                </tr>
                </table>
    </fieldset>       
               <table border="0" cellpadding="5" cellspacing="1" width="100%" id="inTable">
               <tr  id="motiv_row" style="display:none">
               <td align="right">Обоснование операции</td>
               <td>
               <%:
                   Html.Telerik().DropDownList()
                      .Name("DropDownList11")
                      .HtmlAttributes(new { style = "width: 200px" })
                      .BindTo(new SelectList((IEnumerable)ViewData[Store.Data.DataGlobals.REFERENCE_MOTIV], "Id", "Name"))
                      .SelectedIndex(1)
               %>
               </td>
               </tr>
               <tr id="storage_row">
               <td align="right" id="storage_row_text">Передать на</td>
               <td>
               <%:
                   Html.Telerik().DropDownList()
                       .Name("DropDownList_Storage")
                      .HtmlAttributes(new { style = "width: 300px" })
                      .BindTo(new SelectList((IEnumerable)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE_NAME], "Id", "Name"))
               %>
               </td>
               </tr>

<tr id="nomenclature_row" style="display:none">                
                <td align="right">Номенклатура</td>
                <td>
               <%:
              Html.Telerik().ComboBox()
                     .Name("Nomenclature")
                     .AutoFill(true)
                     .ClientEvents(events => events
                        .OnChange("onUpdateSizeListBox")
                     )
                     .DataBinding(binding => binding.Ajax()
                        .Select("GetNomenclatures", "Storages")
                        .Delay(1000)
                        .Cache(true)
                        )
                    .HtmlAttributes(new { style = string.Format("width:{0}px", 300) })
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
               <tr>
                <td align="right">Кол-во</td>
                <td>
                <%:Html.Telerik().NumericTextBox()
                    .Name("QuantityInput")
                    .EmptyMessage("")
                    .MinValue(1)
                    .MaxValue(5)
                    .DecimalDigits(0)
                    .Spinners(false)
                 %>
                </td>
               </tr>
<tr id="bodysize_row" style="display:none">                
                <td align="right">Размер</td>
                <td>
               <%:
                   Html.Telerik().DropDownList()
                   .Name("SizeDropDownList")
                  .DataBinding(binding => binding.Ajax().Select("_Select_NombodyPartSize", "Storages"))
                  .HtmlAttributes(new { style = "width: 100px" })
                  .ClientEvents(events => events
                  .OnDataBinding("onSizeListBoxBinding")
                  )        
               %>
               <span id="growth_row">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Рост
               <%:
                   Html.Telerik().DropDownList()
                      .Name("GrowthDownList")
                      .HtmlAttributes(new { style = "width: 100px" })
                      .BindTo(new SelectList((IEnumerable)ViewData[Store.Data.DataGlobals.REFERENCE_NOM_BODY_PART_SIZE_GROWTH], "Id", "SizeNumber"))
               %>
</span>
                </td>
               </tr>

               <tr id="wear_row" style="display:none">                
                <td align="right">% годности</td>
                <td>
                    <input type="text" id="wear" maxlength="3" size="2" value="100"/>
                </td>
               </tr>
               </table>
               <br />
               <br />
               <table border="0" cellpadding="0" cellspacing="0" width="100%">
               <tr>
                <td style="text-align:right; width:50%">
                    <input type="button" value="Отмена" class="t-button" onclick="closeWindowClick('#Window')"/>
                    &nbsp;&nbsp;&nbsp;&nbsp;
                </td>
                <td>
                    &nbsp;&nbsp;&nbsp;&nbsp;
                    <input id="rashButton" type="button" value="Сохранить" class="t-button" onclick=" disabled=true; editUpdateSaveClick()"/>
                </td>
               </tr>
               </table>
           <%}).ClientEvents(events => {
                 events.OnClose("onUpdateStoreListBox");

                    })
           .Render();
    %>

    <% Html.Telerik().Window()
           .Name("Window2")
           .Title("Импорт данных из внешней системы")
           .Width(1000)
           .Height(700)
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
               <td style="text-align:right">Тип</td>
               <td style="text-align:left">
               <%: Html.Telerik().DropDownList()
                      .Name("SapDocTypeDropDownList")
                      .HtmlAttributes(new { style = "width: 180px" })
                      .BindTo(new SelectList((IEnumerable)ViewData[Store.Data.DataGlobals.REFERENCE_DOCUMENT_TYPE], "Id", "Name"))
                      .SelectedIndex(2)
               %>
               </td>
               <td style="text-align:right">Номер</td>
               <td style="text-align:left">
                   <input name="DocNumberInput" id="DocNumberInput" maxlength="50" size="25" value="<%: Request["docNumber"]??"" %>" onchange="updateButtonStatus()"/>
               </td>
               <td style="text-align:right">Год</td>
               <td style="text-align:left">
                <%: Html.Telerik().NumericTextBox()
                    .Name("DocYearInput")
                    .EmptyMessage("")
                    //.Value(DateTime.Now.Year)
                    .DecimalDigits(0)
                    .NumberGroupSeparator("")
                    .ButtonTitleUp("Следующий год")
                    .ButtonTitleDown("Предыдущий год")
                    .InputHtmlAttributes(new { style = "width:50px" })
                                   .Value((Request["docYear"] != null ? (double?)Convert.ToDouble(Request["docYear"]) : DateTime.Now.Year))
                    .ClientEvents(events => {
                        events.OnLoad("initMaxWidth");
                        events.OnChange("updateButtonStatus");
                    })
                  %>
               </td>
               <td align="right">Дата:</td>
               <td>
                <%= Html.Telerik().DatePicker()
                         .Name("docDateIncomingStorage")
                         .Format("dd.MM.yyyy")
                %>
               </td>
               <td>
                 <%=
                     Html.Button("printBtn", "Печать", HtmlButtonType.Button, "javascript:printScreen();",new { @class = "t-button"})
                 %>
               </td>
               <td>
                 <input type="button" value="Запросить" class="t-button" onclick="getSapData()"/>
               </td>
               </tr>
               <tr><td colspan="10" align="center"><font color="red"><i><b>Дата документа не обязательный параметр</b></i></font></td></tr>
               </table>
               </fieldset>
               </div>
               <br/>
             <%:
         Html.Telerik().Grid((IEnumerable<Store.Core.COMING_SAP>)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE])
                .Name("SapGrid")
                .DataKeys(keys =>
                {
                  keys.Add(x => x.Id);
                })
                .Footer(false)
                .Editable(editing => editing.Mode(GridEditMode.InCell))
                .ToolBar(commands =>
                {
                    commands.SubmitChanges();
                })

                .Columns(columns=>
                {
                  columns.Bound(x => x.Id);
                  columns.Bound(c => c.MATERIAL).ReadOnly(true);
                  columns.Bound(c => c.QUANTITY).Width(50).ReadOnly(true);
                  columns.Bound(c => c.SexName).Width(60).EditorTemplateName("SexTemplate").Title("Пол");
                  columns.Bound(c => c.UnitName).Width(80).EditorTemplateName("UnitTemplate").Title("Ед. идм.");
                  columns.Bound(c => c.SizeName).Width(70).EditorTemplateName("SapSizeTemplate").Title("Размер");
                  columns.Bound(c => c.GrowthName).Width(70).EditorTemplateName("SapGrowthTemplate").Title("Рост");
                  columns.Bound(c => c.NomGroupName).EditorTemplateName("SapNomGroupTemplate").Title("Группа");
                  columns.Bound(c => c.NomBodyPartName).EditorTemplateName("NomBodyPartTemplate").Title("Тип размера");
                  columns.Bound(c => c.IsWinter).Width(70).ClientTemplate("<#= IsWinter=='TRUE'? 'Да' : '' #>").
                      EditorTemplateName("IsWinterTemplate").Title("Зимняя");
                  //columns.Bound(c => c.GalDocDate).Hidden(true);
                  columns.Bound(c => c.NomBodyPartId).Hidden(true);
                  columns.Bound(c => c.NomGroupId).Hidden(true);
                  columns.Bound(c => c.DocDate).Hidden(true);
                  columns.Bound(c => c.DocNumber).Hidden(true);
                  columns.Bound(c => c.DocTypeId).Hidden(true);
                  columns.Bound(c => c.MaterialId).Hidden(true);
                  columns.Bound(c => c.SexId).Hidden(true);
                  columns.Bound(c => c.GrowthId).Hidden(true);
                  columns.Bound(c => c.SizeId).Hidden(true);
                  columns.Bound(c => c.UnitId).Hidden(true);
                  columns.Bound(c => c.SAPNomGroupId).Hidden(true);
                  columns.Bound(c => c.MoveType).Hidden(true);
                  columns.Bound(c => c.Price).Hidden(true);
                    
                })
                .ClientEvents(events => events
                                  .OnEdit("onEdit")
                                  .OnError("onError")
                                  .OnSave("onSave")
                                  .OnError("onGridErrorEvent")
                                  .OnDataBound("onLoadInvoiceDataBound")
                                )
                                
                .DataBinding(dataBinding => dataBinding.Ajax()
                       .Select("_SelectionSAPData", "Storages", new { storageId = -1, docType = -1, document = "-1", year = "-1" })
                       .Update("_UpdateSapData", "Storages")

                  )
             %>
             <div align="center">
               <table border="0" cellpadding="0" cellspacing="0">
               <tr>
                <td>
                    <input type="button" value="Отмена" class="t-button" onclick="closeWindowClick('#Window2')"/>
                    &nbsp;&nbsp;
                </td>
                <td>
                    &nbsp;&nbsp;
                    <input type="button" value="Сохранить" class="t-button" id="SaveSapButton" style="display:none" onclick="editSapUpdateSaveClick()"/>
                </td>
               </tr>
               </table>
               </div>
           <%})
           .Render();
    %>

    <% 
        Html.Telerik().Window()
           .Name("inStorageWindow")
           .Title("Приход")
           .Width(990)
           .Height(700)
           .Draggable(true)
           .Modal(true)
           .Visible(false)
           .Buttons(b => b.Close())
           .Content(() =>
           {%>
                <table style="width:100%">
                <tr>
                <td style="width:50%">
           <fieldset>
        <legend>Документ</legend>
               <table border="0" cellpadding="1" cellspacing="1" width="100%">
               <tr  id="Tr1" style="display:none">
               <td colspan="4" align="center">&nbsp;</td>
               </tr>
               <tr>
               <td align="right">Тип</td>
               <td colspan="3">
               <%:
                   Html.Telerik().DropDownList()
                      .Name("DocTypeDropDownList1")
                      .HtmlAttributes(new { style = "width: 300px" })
                      .BindTo(new SelectList((IEnumerable)ViewData[Store.Data.DataGlobals.REFERENCE_DOCUMENT_TYPE], "Id", "Name"))
                      .SelectedIndex(4)                      
               %>
               </td>
               </tr>
               <tr>
               <td align="right">Номер</td>
               <td>
                    <input type="text" id="Text1" maxlength="10" size="8"/>
               </td>
               <td align="right">Дата</td>
               <td>
               <%: Html.Telerik().DatePicker()
                      .Name("DocDateIncoming")
                      .Value(DateTime.Now.ToShortDateString())
                      .Format(Store.Data.DataGlobals.DATE_FORMAT_FULL_YEAR)
                %>
                </td>
                </tr>
                </table>
</fieldset>       
</td>
<td>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
Приход с:
<br/>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
               <%:
                   Html.Telerik().DropDownList()
                       .Name("DropDownList_Storage1")
                      .HtmlAttributes(new { style = "width: 300px" })
                      .BindTo(new SelectList((IEnumerable)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE_NAME], "Id", "Name"))
               %>
</td>
</tr>
</table>
               <br/>
             <%:
         Html.Telerik().Grid((IEnumerable<Store.Core.NomenclatureSimple>)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE])
                .Name("inStorageGrid")
                .DataKeys(keys =>
                {
                  keys.Add(x => x.Id);
                })
                .Footer(false)
                .Editable(editing => editing.Mode(GridEditMode.InCell))
                .ToolBar(commands =>
                {
                    commands.Insert().ButtonType(GridButtonType.Image);
                })

                .Columns(columns=>
                {
                  columns.Bound(c => c).ClientTemplate("<A class=\"t-button t-button-icon\" href=\"#\" onClick=\"javascript:delRow(this);\" sizcache=\"12\" sizset=\"0\"><SPAN class=\"t-icon\" style=\"background-image:url('"+Url.Content("~/Content/Images/del_button.gif")+"')\"></SPAN></A>")
                          .Title("").Width(40).ReadOnly().Filterable(false).Sortable(false);
                  columns.Bound(c => c.Name).EditorTemplateName("NomenclatureTemplate");
                  columns.Bound(c => c.SizeName).Width(70).EditorTemplateName("SapSizeTemplate");
                  columns.Bound(c => c.GrowthName).Width(70).EditorTemplateName("SapGrowthTemplate");
                  columns.Bound(c => c.Wear).Width(70)
                     // .ClientTemplate("<#= Wear==100?\"новая\":Wear==50?\"б/у\":\"\" #>")
                      .EditorTemplateName("WearTemplate");
                  columns.Bound(c => c.Quantity).Width(70);
                  columns.Bound(c => c.WearId).Hidden(true);
                  columns.Bound(c => c.NameId).Hidden(true);
                  columns.Bound(c => c.NomBodyPartId).Hidden(true);
                  columns.Bound(c => c.SizeId).Hidden(true);
                  columns.Bound(c => c.GrowthId).Hidden(true);
                })
                .ClientEvents(events => events
                  .OnEdit("onEditinStorage")
                  .OnError("onErrorinStorage")
                  .OnSave("onSaveinStorage")
//             .OnDataBinding("dataBinding")
                              )
                .DataBinding(dataBinding => dataBinding.Ajax()
                       .Update("_UpdateSapData", "Storages")
                       .Select("_SelectEmptyRow", "Storages")
                  )

             %>
             <div align="center">
               <table border="0" cellpadding="0" cellspacing="0">
               <tr>
                <td>
                    <input type="button" value="Отмена" class="t-button" onclick="closeWindowClick('#inStorageWindow')"/>
                    &nbsp;&nbsp;
                </td>
                <td>
                    &nbsp;&nbsp;
                    <input type="button" value="Сохранить" class="t-button" id="Button1" onclick="editIncomingUpdateSaveClick()"/>
                </td>
               </tr>
               </table>
               </div>
           <%})
           .Render();
    %>
<input type="hidden" id="_maxQuantity" name="_maxQuantity" />
<input type="hidden" id="_idSelectedRow" name="_idSelectedRow" />

<script type="text/javascript">
    var req;
    var sizeTypeId = -1;
    //Окно загрузки накладной уже отображалось на экране пользователя?
    var isFirstShowWindows = false;

    function initMaxWidth(e) {
        var children = $(e.currentTarget).children('.t-input');
        $(children).each(function () {
            $(this).attr('maxlength', 4);
        });
    }

    function getTagValue(node, tagName) {
        if (node.getElementsByTagName(tagName).length == 0) return "";
        if (node.getElementsByTagName(tagName)[0].firstChild != null) {
            return node.getElementsByTagName(tagName)[0].firstChild.data;
        } else return "";
    }

    function getAttributeValue(node, item) {
        NamedNodeMap = node.attributes;
        for (nodeIndx = 0; nodeIndx < NamedNodeMap.length; nodeIndx++) {
            if (NamedNodeMap(nodeIndx).nodeName == item) {
                return NamedNodeMap(nodeIndx).nodeValue;
            }
        }
        return "";
    }

    function checkSapDataStatus() {
        if (req.readyState == 4) {
            nkmk.Lock.setFree();
            if (req.status == 200) {
                if (req.responseXML.documentElement == null) {
                    try {
                        req.responseXML.loadXML(req.responseText);
                    } catch (e) {
                        alert("Can't load");
                    }
                }
                var xml = req.responseXML;
                var status = "";
                status = getTagValue(xml, "message");
                var grid = $('#SapGrid').data('tGrid');
                var data = new Array();
                if (status == "OK") {
                    var xmlRows = xml.getElementsByTagName("row");
                    for (l = 0; l < xmlRows.length; l++) {
                        if (xmlRows[l].firstChild != null) {
                            var o = new Object()
                            o["Id"] = getTagValue(xmlRows[l], "Id");xml
                            //o["GalDocDate"] = getTagValue(xmlRows[l], "GalDocDate");
                            o["DocDate"] = getTagValue(xmlRows[l], "DocDate");
                            o["DocNumber"] = getTagValue(xmlRows[l], "DocNumber");
                            o["DocTypeId"] = getTagValue(xmlRows[l], "DocTypeId");
                            o["MaterialId"] = getTagValue(xmlRows[l], "MaterialId");
                            o["MATERIAL"] = "["+getTagValue(xmlRows[l], "ExternalCode")+"]"+getTagValue(xmlRows[l], "Material");
                            o["QUANTITY"] = getTagValue(xmlRows[l], "Quantity");
                            o["Price"] = getTagValue(xmlRows[l], "Price");
                            o["UOM"] = getTagValue(xmlRows[l], "UOM");
                            o["LC"] = getTagValue(xmlRows[l], "LC");
                            o["SV"] = getTagValue(xmlRows[l], "SV");
                            o["SexId"] = getTagValue(xmlRows[l], "SexId");
                            o["SexName"] = getTagValue(xmlRows[l], "SexName");
                            o["UnitId"] = getTagValue(xmlRows[l], "UnitId");
                            o["UnitName"] = getTagValue(xmlRows[l], "UnitName");
                            o["SizeId"] = getTagValue(xmlRows[l], "SizeId");
                            o["SizeName"] = getTagValue(xmlRows[l], "SizeName");
                            o["GrowthId"] = getTagValue(xmlRows[l], "GrowthId");
                            o["GrowthName"] = getTagValue(xmlRows[l], "GrowthName");
                            o["IsWinter"] = getTagValue(xmlRows[l], "IsWinter");
                            o["NomGroupId"] = getTagValue(xmlRows[l], "NomGroupId");
                            o["NomGroupName"] = getTagValue(xmlRows[l], "NomGroupName");
                            o["NomBodyPartId"] = getTagValue(xmlRows[l], "NomBodyPartId");
                            o["NomBodyPartName"] = getTagValue(xmlRows[l], "NomBodyPartName");
                            o["SAPNomGroupId"] = getTagValue(xmlRows[l], "SAPNomGroupId");
                            o["SAPNomGroupName"] = getTagValue(xmlRows[l], "SAPNomGroupName");
                            o["MoveType"] = getTagValue(xmlRows[l], "MoveType");
                            data[l]=o;
                        }
                    }
                    grid.dataBind(data);
                    if (data.length > 0) document.getElementById("SaveSapButton").style.display = "";
                        else document.getElementById("SaveSapButton").style.display = "none";
                } else {
                    grid.dataBind(data);
                    alert(status);
                }
            } else {
                var window = $('#Window_alert').data('tWindow');
                if (window) {
                    window.content(req.responseText).center().open();
                } else{
                    alert("Произошла ошибка запроса " + req.status + ":\n" + req.statusText);
                }
            }
        }
    }

    function getSapData() {
        var docTypeListBox = $("#SapDocTypeDropDownList").data("tDropDownList");
        var docType  = docTypeListBox.value();
        var docNumber = document.getElementById("DocNumberInput").value;
        if (docNumber==""){
            alert("№ документа не указан!");
            return false;
        }
        var docYearInput = $("#DocYearInput").data("tTextBox");
        var docDate  = document.getElementById('docDateIncomingStorage').value;

        var docYear = docYearInput.value();
        if (docYear==""){
            alert("Год документа не указан!");
            return false;
        }
        var storageIdListBox = $("#DropDownList").data("tDropDownList");
        var storageId = storageIdListBox.value();
        if (!sendRequest('<%: Url.Content("~/Storages/_SelectionSAPData")%>', "storageId=" + storageId + "&docType=" + docType + "&document=" + docNumber + "&year=" + docYear+ "&docDate=" + docDate, checkSapDataStatus)) {
            alert("Не могу вызвать запрос на получение данных из внешней системы!");
        } else {
            if (nkmk.Lock.isFree()) {
              nkmk.Lock.setBusy();
              nkmk.Lock.printBusyMessage("Идет загрузка данных из внешней системы");        
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

    function onUpdateStoreListBox(e) {
        var dropDownList = $("#DropDownList").data("tDropDownList");
<% if (Request["storageCode"]!=null && Request["docNumber"]!=null && ViewData["storageId"]!=null) {%>
        //Если в запросе есть параметры на загрузку накладной, то вызываем окно для ее загрузки.
        if (isFirstShowWindows){
            dropDownList.value('<%:ViewData["storageId"]%>'); 
            setTimeout(inSapStorageClick, 400);            
        }
<%}%>
        var storageId = "";
        if (dropDownList) {
            storageId = dropDownList.value();
        }
        if (storageId == "") {
            storageId = "-1";
        }
        if (storageId != "-1") {
            var storagesGrid = $('#Storages').data('tGrid');
            // rebind the related grid
            if (storagesGrid) {
                storagesGrid.rebind({
                    idStorage: storageId
                });
            }
        }
    }

    function inSapStorageClick() {
        $('.t-toolbar.t-grid-toolbar.t-grid-top').css("display","none");
        var dropDownList = $("#DropDownList").data("tDropDownList");
        var storageId = dropDownList.value();
        if (storageId == "") {
            alert("Склад не выбран!")
            return false;
        }

        var docTypeListBox = $("#SapDocTypeDropDownList").data("tDropDownList");
        docTypeListBox.value(<%: Store.Data.DataGlobals.DOCTYPE_TTN %>);

        var window = $("#Window2").data("tWindow");
        window.center().open();
    }

    function inStorageClick() {
        var dropDownList = $("#DropDownList").data("tDropDownList");
        var storageId = dropDownList.value();
        if (storageId == "") {
            alert("Склад не выбран!")
            return false;
        }
        onUpdateSizeListBox(null);
        document.getElementById("TrRow").style.display = "none";
        document.getElementById("storage_row_text").innerHTML = "Отправитель";
        document.getElementById("motiv_row").style.display = "none";
//        document.getElementById("wear_row").style.display = "none";
        document.getElementById("wear_row").value = "100";
        document.getElementById("nomenclature_row").style.display = "";

        document.getElementById("bodysize_row").style.display = "";
        document.getElementById("nomenclature_row").style.display = "";
        document.getElementById("bodysize_row").style.display = "";
//        document.getElementById("price_row").style.display = "";
        var numericTextBox = $("#QuantityInput").data("tTextBox");
        numericTextBox.minValue = 1;
        numericTextBox.maxValue = 99999;
        numericTextBox.value(null);
        $("#Window").find(".t-window-title").html("Приход СИЗ на склад");

        var docTypeListBox = $("#DocTypeDropDownList").data("tDropDownList");
        docTypeListBox.value(<%: Store.Data.DataGlobals.DOCTYPE_TTN %>);
        var window = $("#inStorageWindow").data("tWindow");
      // var window = $("#Window").data("tWindow");
      var grid = $('#inStorageGrid').data('tGrid');
     var data = new Array();

    grid.dataBind(data);

        window.center().open();
    }

    function closeWindowClick(winName) {
        onUpdateStoreListBox(null);
        var window = $(winName).data("tWindow");
        window.close();
    }

    function moveStorageClick(id, maxQuantity, ecode, name) {
        if (maxQuantity < 1) {
            alert("Этой номенклатуры на складе не осталось!");
            return false;
        }
        document.getElementById("rashButton").disabled=false;
        document.getElementById("TrRow").cells[0].innerHTML = ecode + ' - ' + name + '<br /><br />';
        document.getElementById("TrRow").style.display = "";
        document.getElementById("storage_row_text").innerHTML = "Передать на";
        document.getElementById("motiv_row").style.display = "none";
        document.getElementById("wear_row").style.display = "none";
        document.getElementById("nomenclature_row").style.display = "none";
        document.getElementById("bodysize_row").style.display = "none";
        document.getElementById("growth_row").style.display = "none";
//        document.getElementById("price_row").style.display = "none";
        document.getElementById("_maxQuantity").value = maxQuantity;
        document.getElementById("_idSelectedRow").value = id;
        var numericTextBox = $("#QuantityInput").data("tTextBox");
        numericTextBox.maxValue = maxQuantity;

        var docTypeListBox = $("#DocTypeDropDownList").data("tDropDownList");
        docTypeListBox.value(<%: Store.Data.DataGlobals.DOCTYPE_TTN %>);

        var wnd = $("#Window").data("tWindow");
        $("#Window").find(".t-window-title").html("Операция по складу");
        wnd.center().open();
    }

    function checkStatus() {
        if (req.readyState == 4) {
            if (req.status == 200) {                
                if (document.getElementById("bodysize_row").style.display != "") {
                    closeWindowClick('#Window');
                } else {
                    var numericTextBox = $("#QuantityInput").data("tTextBox");
                    numericTextBox.value(null);
                    var nomComboBox = $("#Nomenclature").data("tComboBox");
                    nomComboBox.value(null);
                    nomComboBox.text("");
                }
            } else {
                alert("Произошла ошибка запроса " + req.status + ":\n" + req.statusText);
            }
        }
    }

    function checkVal(val, possible) {
        for (var i = 0; i <= val.length; i++) {
            if (possible.indexOf(val.charAt(i)) == -1)
                return false;
        }
        return true;
    }

    function editUpdateSaveClick() {
        //Номер документа
        var docNumber = document.getElementById("DocNumber").value;
        if (docNumber == "") {
            alert("№ документа должен быть задан!");
            document.getElementById("rashButton").disabled=false;
            return false;
        }
        //Дата документа
        var docDate = document.getElementById("DocDate").value;
        if (docDate == "") {
            alert("Дата документа должна быть задана!");
            document.getElementById("rashButton").disabled=false;
            return false;
        }
        // Тип документа
        var docTypeDownList = $("#DocTypeDropDownList").data("tDropDownList");
        var DocTypeId = docTypeDownList.value();
        //Кол-во
        var numericTextBox = $("#QuantityInput").data("tTextBox");
        var countNomenclature = numericTextBox.value();
        if ((countNomenclature == "") || (countNomenclature==null)) {
            alert("Не введено кол-во!");
            document.getElementById("rashButton").disabled=false;
            return false;
        }

        if (document.getElementById("nomenclature_row").style.display == "none") {
            //Передача в цех
            if (countNomenclature > Number(document.getElementById("_maxQuantity").value)) {
                alert("Превышено максимальное кол-во!");
                document.getElementById("rashButton").disabled=false;
                return false;
            }

            //Id склада на который передается
            var dropDownList = $("#DropDownList_Storage").data("tDropDownList");
            var storIdOut = dropDownList.value();
            
            var idSelectedRow = document.getElementById("_idSelectedRow").value;

            if (!sendRequest("<%: Url.Content("~/Storages/_OutNomenclature")%>", "id=" + idSelectedRow + "&storageId=" + storIdOut + "&docNumber=" + docNumber + "&docDate=" + docDate + "&DocTypeId=" + DocTypeId + "&Quantity=" + countNomenclature+ "&StorIdOut=" + storIdOut, checkStatus)) {
                alert("Не могу вызвать запрос на перемещение номенклатуры со склада на склад!");
                document.getElementById("rashButton").disabled=false;
            }
        } else {
            //Приход на склад
            //Id склада
            var dropDownList = $("#DropDownList").data("tDropDownList");
            var storageId = dropDownList.value();

            //Id склада (c какого)
            var dropDL = $("#DropDownList_Storage").data("tDropDownList");
            var storageIdIncoming = dropDL.value();

            //Номенклатура
            var nomComboBox = $("#Nomenclature").data("tComboBox");
            var nomId = nomComboBox.value();
            if (nomId == "") {
                alert("Номенклатура должна быть выбрана!");
                return false;
            }
            nomId = nomId.substring(0, nomId.indexOf("|"));
            // % износа
            var wear = document.getElementById("wear").value;
            if (wear == "") {
                alert("% годности должен быть задан!");
                return false;
            }

            //Размер
            var sizeDownList = $("#SizeDropDownList").data("tDropDownList");
            var sizeId = sizeDownList.value();
            //Рост
            var growth = -1;
            if (Number(sizeTypeId) == <%: Store.Data.DataGlobals.CLOTH_SIZE_ID %>) {
                var growthDownList = $("#GrowthDownList").data("tDropDownList");
                growth = growthDownList.value();
            }
            if (!sendRequest("<%: Url.Content("~/Storages/_InNomenclature")%>", "storageId=" + storageId + "&docNumber=" + docNumber + "&docDate=" + docDate + "&DocTypeId=" + DocTypeId + "&Quantity=" + countNomenclature + "&Growth=" + growth + "&Wear=" + wear + "&NomenclatureId=" + nomId + "&SizeId=" + sizeId + "&StorIdIncoming=" + storageIdIncoming, checkStatus)) {
                alert("Не могу вызвать запрос на перемещение номенклатуры со склада на склад!");
            }
        }
        document.getElementById("rashButton").disabled="false";
    }

    function onSizeListBoxBinding(e) {
        if (Number(sizeTypeId) == <%: Store.Data.DataGlobals.CLOTH_SIZE_ID %>) document.getElementById("growth_row").style.display = "";
            else document.getElementById("growth_row").style.display = "none";
        e.data = $.extend({}, e.data, { Id: sizeTypeId });
    }

    function onUpdateSizeListBox(e) {
        var nomComboBox = $("#Nomenclature").data("tComboBox");
        var nomId = nomComboBox.value();         
        if (nomId != "") {
            sizeTypeId = nomId.substring(nomId.indexOf("|") + 1);
            nomId = nomId.substring(0, nomId.indexOf("|"));
            var sizeDownList = $("#SizeDropDownList").data("tDropDownList");
            sizeDownList.reload();
        }
    }

    function onError(args) {
        if (args.textStatus == "modelstateerror" && args.modelState) {
            var message = "Errors:\n";
            $.each(args.modelState, function (key, value) {
                if ('errors' in value) {
                    $.each(value.errors, function () {
                        message += "[" + key + "] " + this + "\n";
                    });
                }
            });
            args.preventDefault();
            alert(message);
        }
    }

    function onSave(e) {
        var grid = $('#SapGrid').data('tGrid');
        var allData = grid.data;
        curRrow = e.cell.parentElement;
        var dataItem = e.dataItem;
        var $listBox = $(e.cell).find('#SexName');
        if ($listBox.length > 0) {
            var list = $listBox.data("tDropDownList");
            selectItem = null;
            if (list.selectedIndex)
                selectItem = list.data[list.selectedIndex];
            else {
                if ((list.data) && (list.data.length > 0)) {
                    selectItem = list.data[0];
                }
            }
            if (selectItem != null) {
                allData[curRrow.rowIndex - 1].SexId = selectItem.Value;
                allData[curRrow.rowIndex - 1].SexName = selectItem.Text;
                grid.dataBind(allData);
            }
        }

        $listBox = $(e.cell).find('#SizeName');
        if ($listBox.length > 0) {
            var list = $listBox.data("tDropDownList");
            selectItem = null;
            if (list.selectedIndex)
                selectItem = list.data[list.selectedIndex];
            else {
                if ((list.data) && (list.data.length > 0)) {
                    selectItem = list.data[0];
                }
            }
            if (selectItem != null) {
                allData[curRrow.rowIndex-1].SizeId = selectItem.Value;
                allData[curRrow.rowIndex-1].SizeName = selectItem.Text;
                grid.dataBind(allData);
            }
        }


        $listBox = $(e.cell).find('#IsWinter');
        if ($listBox.length > 0) {
            if ($listBox[0].value != null) {
            if ($listBox[0].value == "1") {
                allData[curRrow.rowIndex-1].IsWinter = "TRUE";
            }
            if ($listBox[0].value == "0") {
                allData[curRrow.rowIndex-1].IsWinter = "FALSE";
            } 
            } else {
               allData[curRrow.rowIndex-1].IsWinter = null;
            }
            grid.dataBind(allData);
        }

        $listBox = $(e.cell).find('#GrowthName');
        if ($listBox.length > 0) {
            if (dataItem["NomBodyPartId"] == "<%: Store.Data.DataGlobals.CLOTH_SIZE_ID %>") {
                var list = $listBox.data("tDropDownList");
                selectItem = null;
                if (list.selectedIndex)
                    selectItem = list.data[list.selectedIndex];
                else {
                    if ((list.data) && (list.data.length > 0)){
                        selectItem = list.data[0];
                    }
                }
                if (selectItem != null) {
                    allData[curRrow.rowIndex - 1].GrowthId = selectItem.Value;
                    allData[curRrow.rowIndex - 1].GrowthName = selectItem.Text;
                    grid.dataBind(allData);
                }
            } else {
                allData[curRrow.rowIndex - 1].GrowthId = null;
                allData[curRrow.rowIndex - 1].GrowthName = null;
                grid.dataBind(allData);
            }
        }

        $listBox = $(e.cell).find('#NomGroupName');
        if ($listBox.length > 0) {
            var cBox = $listBox.data("tComboBox");
            if (cBox.value() == null) {
                allData[curRrow.rowIndex - 1].NomGroupId = null;
                allData[curRrow.rowIndex - 1].NomGroupName = "";
                currentNomBodyPartId = null;
                grid.dataBind(allData);
            } else {
                nomGroupId = cBox.value();
                currentNomBodyPartId = nomGroupId.substring(0, nomGroupId.indexOf("|"));
                nomGroupId = nomGroupId.substring(nomGroupId.indexOf("|")+1);
                currentNomBodyPartName =  nomGroupId.substring(0, nomGroupId.indexOf("|"));
                nomGroupId = nomGroupId.substring(nomGroupId.indexOf("|")+1);
                isWinter = nomGroupId.substring(0, nomGroupId.indexOf("|"));
                allData[curRrow.rowIndex - 1].NomGroupId = nomGroupId.substring(nomGroupId.indexOf("|")+1);
                allData[curRrow.rowIndex - 1].NomGroupName = cBox.text();
                allData[curRrow.rowIndex - 1].NomBodyPartId = currentNomBodyPartId;
                allData[curRrow.rowIndex - 1].NomBodyPartName = currentNomBodyPartName;
                if ((isWinter.toUpperCase()=="TRUE")||(isWinter=="1")){
                    allData[curRrow.rowIndex - 1].IsWinter = "TRUE";
                }
                if ((isWinter.toUpperCase()=="FALSE")||(isWinter=="0")){
                    allData[curRrow.rowIndex - 1].IsWinter = "FALSE";
                }
                grid.dataBind(allData);
            }
        }         

        $listBox = $(e.cell).find('#UnitName');
        if ($listBox.length > 0) {
            var list = $listBox.data("tDropDownList");
            selectItem = null;
            if (list.selectedIndex)
                selectItem = list.data[list.selectedIndex];
            else {
                if ((list.data) && (list.data.length > 0)) {
                    selectItem = list.data[0];
                }
            }
            if (selectItem != null) {
                allData[curRrow.rowIndex - 1].UnittId = selectItem.Value;
                allData[curRrow.rowIndex - 1].UnitName = selectItem.Text;
                grid.dataBind(allData);
            }
        }

        $listBox = $(e.cell).find('#NomBodyPartName');
        if ($listBox.length > 0) {
            var list = $listBox.data("tDropDownList");
            selectItem = null;
            if (list.selectedIndex)
                selectItem = list.data[list.selectedIndex];
            else {
                if ((list.data) && (list.data.length > 0)) {
                    selectItem = list.data[0];
                }
            }
            if (selectItem != null) {
                allData[curRrow.rowIndex - 1].NomBodyPartId = selectItem.Value;
                allData[curRrow.rowIndex - 1].NomBodyPartName = selectItem.Text;
                grid.dataBind(allData);
            }
        }
        //        var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
        //        e.data = $.extend(e.data, { workerWorkplaceId: workerWorkplaceId });
    }

    function onSapSizeListBoxDataBinding(e) {
        if (Number(currentNomBodyPartId) > 0) {
            e.data = $.extend({}, e.data, { Id: currentNomBodyPartId });
        }
        else {
            e.preventDefault();
            alert("Выберите сначала тип размера!");
        }
    }

    function onEdit(e) {
        var dataItem = e.dataItem;
        var mode = e.mode;
        var form = e.form;
        currentNomBodyPartId = dataItem["NomBodyPartId"];
    }

    function getCellIndexByName(cells,cellName) {
        for (_curCell = 0; _curCell < cells.length; _curCell++) {
            if (cells[_curCell].member.toUpperCase()==cellName.toUpperCase()){
                return _curCell;
            }
        }
        return 0;
    }

    function editSapUpdateSaveClick() {
        var grid = $('#SapGrid').data('tGrid');
        var rows = grid.$rows();
        for (i = 0; i < rows.length; i++) {
            var row = rows[i];
            var currenNomenclature = row.cells[getCellIndexByName(grid.columns, "MATERIAL")].innerHTML;
            if (row.cells[getCellIndexByName(grid.columns,"SexId")].innerHTML == "") {
                alert("Для номенклатуры:\r\n" + currenNomenclature+"\r\nне задан пол!");
                return false;
            }          

            if (row.cells[getCellIndexByName(grid.columns,"UnitId")].innerHTML == "") {
                alert("Для номенклатуры:\r\n" + currenNomenclature+"\r\nне указанно ед. изм.!");
                return false;
            }

            if (row.cells[getCellIndexByName(grid.columns,"NomBodyPartId")].innerHTML == "") {
                alert("Для номенклатуры:\r\n" + currenNomenclature+"\r\nне задан тип размера!");
                return false;
            }
            if (row.cells[getCellIndexByName(grid.columns,"NomBodyPartId")].innerHTML != "<%: Store.Data.DataGlobals.SIZ_SIZE_ID %>" &&
            row.cells[getCellIndexByName(grid.columns,"NomBodyPartId")].innerHTML != "<%: Store.Data.DataGlobals.GLOVE_SIZE_ID %>" &&
            row.cells[getCellIndexByName(grid.columns,"NomBodyPartId")].innerHTML != "<%: Store.Data.DataGlobals.HAND_SIZE_ID %>" &&
            row.cells[getCellIndexByName(grid.columns,"NomBodyPartId")].innerHTML != "<%: Store.Data.DataGlobals.RESP_SIZE_ID %>" 
            ) {
                if (row.cells[getCellIndexByName(grid.columns,"SizeId")].innerHTML == "") {
                    alert("Для номенклатуры:\r\n" + currenNomenclature+"\r\nне задан размер!");
                    return false;
                }
            }

            var idGroup=row.cells[getCellIndexByName(grid.columns,"SAPNomGroupId")].innerHTML;
            if (idGroup.length < 7) {
                alert("Номенклатура:\r\n" + currenNomenclature+"\r\nне включена в группу номенклатур.\r\nОбращайтесь в службу эксплуатации!");
                return false;
            }

            if (row.cells[getCellIndexByName(grid.columns,"NomGroupId")].innerHTML == "") {
                alert("Для номенклатуры:\r\n" + currenNomenclature+"\r\nне задана группа номенклатуры!");
                return false;
            }

            if ((row.cells[getCellIndexByName(grid.columns,"GrowthId")].innerHTML == "") && (row.cells[getCellIndexByName(grid.columns,"NomBodyPartId")].innerHTML=="2")) {
                alert("Для номенклатуры:\r\n" + currenNomenclature + "\r\nне задан рост!");
                return false;
            }
        }

        var storegeDropDownList = $("#DropDownList").data("tDropDownList");
        var postData = "storageId=" + storegeDropDownList.value();
        var gridData = grid.data;
        for (i = 0; i < rows.length; i++) {
            var row = rows[i];
            var columns = row.cells;
            var rowGridData = gridData[i];
            for (j = 0; j < grid.columns.length; j++) {
//                if ((i != 0) || (j != 0)) {
//                    postData = postData + "&";
//                }
                postData = postData + "&updated[" + i + "]." + grid.columns[j].member + "=" + rowGridData[grid.columns[j].member];
            }
        }
        if (!sendRequest("<%: Url.Content("~/Storages/_SaveSapData")%>", postData, checkSaveSapDataStatus)) {
            alert("Не могу вызвать запрос на сохранение данных из внешней системы!");
        } else {
              nkmk.Lock.setBusy();
              nkmk.Lock.printBusyMessage("Идет сохранение данных...");
        }
    }

    function checkSaveSapDataStatus() {
        if (req.readyState == 4) {
            nkmk.Lock.setFree();
            if (req.status == 200) {
                if (req.responseXML.documentElement == null) {
                    try {
                        req.responseXML.loadXML(req.responseText);
                    } catch (e) {
                        alert("Can't load");
                    }
                }
                if (req.responseText!="")
                    alert(req.responseText);
                else {
                   closeWindowClick('#Window2');
                   alert("Данные успешно сохранены!");
                }
            } else {
                onGridErrorEvent(req);
            }
        }
    }

//Приход на склад
    function onEditinStorage(e) {
        var dataItem = e.dataItem;
        var mode = e.mode;
        var form = e.form;
        currentNomBodyPartId = dataItem["NomBodyPartId"];
    }

    function onErrorinStorage(args) {
        if (args.textStatus == "modelstateerror" && args.modelState) {
            var message = "Errors:\n";
            $.each(args.modelState, function (key, value) {
                if ('errors' in value) {
                    $.each(value.errors, function () {
                        message += "[" + key + "] " + this + "\n";
                    });
                }
            });
            args.preventDefault();
            alert(message);
        }
    }

    function onSaveinStorage(e) {
     var curRow = $(e.cell).closest('tr');
     var cells = $(curRow[0]).find('td');
      var $combo = $(e.cell).find('#Name');
      if ($combo.length > 0) {
          var combo = $combo.data("tComboBox"),
                    selectItem = combo.selectedIndex > -1 ? combo.data[combo.selectedIndex] : null;

          if (selectItem) {
              e.values.NameId = selectItem.Value.substring(0, selectItem.Value.indexOf("|"));
              e.values.NomBodyPartId = selectItem.Value.substring(selectItem.Value.indexOf("|")+1);
              currentNomBodyPartId = e.values.NomBodyPartId;
              e.values.Name = selectItem.Text;
              cells[cells.length-4].innerHTML = e.values.NameId;
              cells[cells.length-3].innerHTML = currentNomBodyPartId;
          }
      }
      
      var $size = $(e.cell).find('#SizeName');
      if ($size.length > 0) {
          var size = $size.data("tDropDownList"),
                    selectItem = size.selectedIndex > -1 ? size.data[size.selectedIndex] : null;
          if (selectItem) {
              e.values.SizeId = selectItem.Value;
              e.values.SizeName = selectItem.Text;
              cells[cells.length-2].innerHTML = e.values.SizeId;
          }
      }

      var $wear = $(e.cell).find('#Wear');

          if ($wear.length <= 0) {
               e.values.WearId=100 ;
               e.values.Wear = "новая"
               cells[cells.length-5].innerHTML = e.values.WearId;
          }
         else {
           var wear = $wear.data("tDropDownList"),
                    selectItem = wear.selectedIndex > -1 ? wear.data[wear.selectedIndex] : null;
              if (selectItem) {
                  e.values.WearId = selectItem.Value;
                  e.values.Wear = selectItem.Text;
                  cells[cells.length-5].innerHTML = e.values.WearId;
              }
      }

      var $growth = $(e.cell).find('#GrowthName');
      if ($growth.length > 0) {
          var growth = $growth.data("tDropDownList"),
                    selectItem = growth.selectedIndex > -1 ? growth.data[growth.selectedIndex] : null;
          if (selectItem) {
              e.values.GrowthId = selectItem.Value;
              e.values.GrowthName = selectItem.Text;
              cells[cells.length-1].innerHTML = e.values.GrowthId;
          }
      }                             
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
        w.document.writeln('<h3>Приход из внешней системы</h3>');
        w.document.writeln('<table width="90%" align="center">');
        w.document.writeln('<tr>');
        w.document.writeln('<td  align="right" width="20%">Склад:</td>');
        w.document.writeln('<td width="20%" id="storageField"><b>'+$("#DropDownList").data("tDropDownList").text()+'</b></td>');
        w.document.writeln('<td>&nbsp;</td>');
        w.document.writeln('<td width="10%" align="right">Документ:</td>');
        w.document.writeln('<td width="10%"><b>'+document.getElementById("DocNumberInput").value+'</b></td>');
        w.document.writeln('</tr>');
        w.document.writeln('</table>');
        w.document.writeln('<br/>');
        w.document.writeln('<table width="90%" align="center" id="contentTable" border="1" cellpadding="1" cellspacing="0">');
        $("#SapGrid .t-grid-header tr").each(function (indx, element) {
            w.document.writeln("<tr>");
            var cellText = element.innerHTML;
            //Рисуем бордюр
        //    cellText = cellText.replace("<TH class=t-header scope=col>", "<TH style='border: 1px solid black;'>");
            cellText = cellText.replace(new RegExp("<TH class=t-header scope=col", 'g'), "<TH style='border-right:1px solid black;border-bottom: 1px solid black;border-top: 1px solid black;'");
            cellText = cellText.replace(new RegExp("<TH class=\"t-header t-last-header\" scope=col>", 'g'), "<TH style='border-right:1px solid black;border-bottom: 1px solid black;border-top: 1px solid black;'>");
            w.document.writeln(cellText);
            w.document.writeln("</tr>");
        });
        $("#SapGrid tbody tr").each(function (indx, element) {
            w.document.writeln("<tr>");
            var cellText = element.innerHTML.replace(new RegExp("></TD>", 'g'), ">&nbsp;</TD>");
            //Рисуем бордюр
          //  cellText = cellText.replace("<TD>", "<TD style='border: 1px solid black;'>");
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

    function updateButtonStatus(){
        var grid = $('#SapGrid').data('tGrid');    
        grid.dataBind(new Array());
        document.getElementById("SaveSapButton").style.display = "none";
    }

    //Удаляем запись из табл. для ручного ввода прихода на склад
    function delRow(obj){
        var grid = $('#inStorageGrid').data('tGrid');
        var tr = $(obj).closest('tr'); 
        grid.deleteRow(tr);
        emptyRow=$("#inStorageGrid .t-no-data");
        if ($("#inStorageGrid tr").length>2){
            emptyRow[0].style.display="none";
        } else {
            emptyRow[0].style.display="block";
        }
        return false;
    }

    function formatDataGridCell(val){
        var value = val;
        if (value) {
            value = value.substring(value.indexOf("</SPAN>")+7); 
        }
        return value;
    }

        function editIncomingUpdateSaveClick() {
        var docNumber=document.getElementById("Text1").value;
        if (docNumber == "") {
            alert("Не указан номер документа!");
            return false;
        }

        var docDate=document.getElementById("DocDateIncoming").value;
        if (docDate == "") {
            alert("Не указана дата документа!");
            return false;
        }

        var grid = $('#inStorageGrid').data('tGrid');
        var rows = grid.$rows();
        
        for (i = 0; i < rows.length; i++) {
            var row = rows[i];
            if (row.cells[getCellIndexByName(grid.columns, "Name")]){
                var currenNomenclature = formatDataGridCell(row.cells[getCellIndexByName(grid.columns, "Name")].innerHTML);
                if ((currenNomenclature) || (currenNomenclature=="")){
                    if (row.cells[row.cells.length-3].innerHTML == "") {
                        alert("Для номенклатуры:\r\n" + currenNomenclature+"\r\nне задан тип размера!");
                        return false;
                    }
                    if (row.cells[row.cells.length-3].innerHTML != "<%: Store.Data.DataGlobals.SIZ_SIZE_ID %>" &&
                    row.cells[row.cells.length-3].innerHTML != "<%: Store.Data.DataGlobals.GLOVE_SIZE_ID %>" &&
                    row.cells[row.cells.length-3].innerHTML != "<%: Store.Data.DataGlobals.HAND_SIZE_ID %>" &&
                    row.cells[row.cells.length-3].innerHTML != "<%: Store.Data.DataGlobals.RESP_SIZE_ID %>") {
                        if (row.cells[row.cells.length-2].innerHTML == "") {
                            alert("Для номенклатуры:\r\n" + currenNomenclature+"\r\nне задан размер!");
                            return false;
                        }
                    }

                    if (row.cells[row.cells.length-5].innerHTML == "") {
                        alert("Для номенклатуры:\r\n" + currenNomenclature+"\r\nне указан износ!");
                        return false;
                    }

                    if ((row.cells[row.cells.length-1].innerHTML == "") && (row.cells[row.cells.length-3].innerHTML=="2")) {
                        alert("Для номенклатуры:\r\n" + currenNomenclature + "\r\nне задан рост!");
                        return false;
                    }
                    var quantity = formatDataGridCell(row.cells[getCellIndexByName(grid.columns,"Quantity")].innerHTML);
                    if (quantity == "") {
                        alert("Для номенклатуры:\r\n" + currenNomenclature+"\r\n не задано количество!");
                        return false;
                    } else {
                        if (!(Number(quantity)>0)){
                           alert("Для номенклатуры:\r\n" + currenNomenclature+"\r\n задано ошибочное количество!");
                           return false;
                        }
                    }
                } else {
                    alert("Не выбрана номенклатура!");
                    return false;
                }
            }
        }
        
        var docTypeDropDownList = $("#DocTypeDropDownList1").data("tDropDownList");
        var StorIncomingDropDownList = $("#DropDownList_Storage1").data("tDropDownList");
        var storegeDropDownList = $("#DropDownList").data("tDropDownList");
        var postData = "storageId=" + storegeDropDownList.value()+ "&StorIdIncoming="+StorIncomingDropDownList.value() +"&docTypeId="+docTypeDropDownList.value()+"&docNumber="+docNumber+"&docDate="+docDate;
        for (i = 0; i < rows.length; i++) {
            var row = rows[i];
            if (row.cells[getCellIndexByName(grid.columns, "Name")]){
                var currenNomenclature = row.cells[getCellIndexByName(grid.columns, "Name")].innerHTML;
                currenNomenclature = currenNomenclature.substring(currenNomenclature.indexOf("</SPAN>")+6); 
                if ((currenNomenclature) || (currenNomenclature=="")){
                   postData = postData + "&updated[" + i + "].WearId=" + row.cells[row.cells.length-5].innerHTML;
                   postData = postData + "&updated[" + i + "].NameId=" + row.cells[row.cells.length-4].innerHTML;
                   postData = postData + "&updated[" + i + "].NomBodyPartId=" + row.cells[row.cells.length-3].innerHTML;
                   postData = postData + "&updated[" + i + "].SizeId=" + formatDataGridCell(row.cells[row.cells.length-2].innerHTML);
                   postData = postData + "&updated[" + i + "].GrowthId=" + formatDataGridCell(row.cells[row.cells.length-1].innerHTML);
                   postData = postData + "&updated[" + i + "].Quantity=" + formatDataGridCell(row.cells[getCellIndexByName(grid.columns,"Quantity")].innerHTML);
                } 
            }
        }
        if (!sendRequest("<%: Url.Content("~/Storages/_SaveIncomingData")%>", postData, checkSaveIncomingDataStatus)) {
            alert("Не могу вызвать запрос на сохранение данных по приходу на склад!");
        } else {
              nkmk.Lock.setBusy();
              nkmk.Lock.printBusyMessage("Идет сохранение данных...");
        }
    }

    function checkSaveIncomingDataStatus() {
        if (req.readyState == 4) {
            nkmk.Lock.setFree();
            if (req.status == 200) {
                if (req.responseText != "") {
                    alert(req.responseText);
                    return false;
                }
                closeWindowClick('#inStorageWindow');
                alert("Данные успешно сохранены!");
                onUpdateStoreListBox(null);
            } else {
                onGridErrorEvent(req);
            }
        }
    }

    var currentNomBodyPartId = null;
    //Обновление номенклатуры(Grid)
//    window.setTimeout("onUpdateStoreListBox(null);", 1500);
<% if (Request["storageCode"]!=null && Request["docNumber"]!=null && ViewData["storageId"]!=null) {%>
    isFirstShowWindows = true;
<%}%>
<% if (Request["storageCode"]!=null && ViewData["storageId"]==null) {%>
    alert("В справочнике не найден склад с кодом <%:Request["storageCode"] %>!");
<%}%>

   function onLoadInvoiceDataBound(){
<% if (Request["storageCode"]!=null && Request["docNumber"]!=null && ViewData["storageId"]!=null) {%>
    if (isFirstShowWindows==true){
        isFirstShowWindows = false;
        getSapData();
    }
<%}%>
   }
</script>
</asp:Content>