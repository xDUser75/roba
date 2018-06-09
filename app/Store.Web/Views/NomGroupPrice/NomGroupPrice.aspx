<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage"%>
<%@ Import Namespace="Store.Data" %>
<%@ Import Namespace="Store.Core" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<h3>Просмотр цен на группы номенклатур и их загрузка</h3>
<table width="100%">
    <tr>
        <td align="right">Вид цены:</td>
        <td>
            <%:
            Html.Telerik().DropDownList()
                           .Name("PriceList")
                           .BindTo((SelectList)ViewData[Store.Data.DataGlobals.REFERENCE_PERIOD_PRICE])
                          .HtmlAttributes(new { style = "width: 250px" })
             %>
        </td>
        <td rowspan="2"><input type="button" value="Найти" class="t-button" onclick="updateGrid()" /></td>
<%                                        
    if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                                            HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NOM_GROUP_PRICE_UPLOAD))
    {
%>
        <td align="right" rowspan="2"><input type="button" value="Загрузить на сервер" class="t-button" onclick="loadDataClick()" /></td>
<%  }  %>
    </tr>
    <tr>
        <td align="right">Группа:</td>
        <td>
           <%:
              Html.Telerik().ComboBox()
                     .Name("NomGroup")
                     .HtmlAttributes(new { style = "width: 600px" })
                     .DataBinding(binding => binding.Ajax()
                                             .Select("_GetNomGroup", "NomGroupPrice").Cache(false))                    
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
<% 
    Html.Telerik().Grid<Store.Core.NomGroupPrice>()
        .Name("NomGroupPrice")
        .DataBinding(dataBinding =>
        {
            dataBinding
              .Ajax()
              .Select("_GetNomGroupPrice", "NomGroupPrice");
        })
        .Columns(columns =>
        {
            columns.Bound(x => x.NomGroup.ExternalCode).Width(100);
            columns.Bound(x => x.NomGroup.Name);
            columns.Bound(x => x.NomGroup.IsActive)
                .Width(100)
                .Title("Активный")
                .ClientTemplate("<input type='checkbox' disabled='true' name='isactive' <#= (NomGroup.IsActive)?'checked':'' #> />");
            columns.Bound(x => x.Price).Width(100).HtmlAttributes(new { @align = "right" }).Format("{0:0.00}");
        })
        .ClientEvents(events => events
            .OnDataBinding("onNomGroupPriceDataBinding")
            .OnError("onGridErrorEvent"))
        .Scrollable(scrolling => scrolling.Height(400))
        .Filterable()
        .Sortable()
        .Selectable()
        .Render();
%>

    <% 
        Html.Telerik().Window()
           .Name("loadDataWindow")
           .Title("Загрузка цены")
           .Width(400)
           .Height(400)
           .Draggable(true)
           .Modal(true)
           .Visible(false)
           .Buttons(b => b.Close())
           .Content(() =>
           {%>
<form id="uploadForm" method="post" enctype="multipart/form-data" action="NomGroupPrice\Save" onsubmit="return onFormSubmit();" target="upload_target">
                <table style="width:100%">
                <tr>
        <td align="right">Вид цены:</td>
        <td>
            <%:
            Html.Telerik().DropDownList()
                           .Name("PriceLoadList")
                           .BindTo((SelectList)ViewData[Store.Data.DataGlobals.REFERENCE_PERIOD_PRICE])
                          .HtmlAttributes(new { style = "width: 250px" })
             %>
        </td>
        </tr>
                </table>
<br/>
<fieldset>
<legend>Опции загрузки</legend>
<input type="radio" id="onReplase" checked="checked" onclick="updateScreenParam(1);"/>Замена данных<br/>
<input type="radio" id="onUpdate" onclick="updateScreenParam(2);"/>Обновление данных
</fieldset>
<br/>
<%if ((HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_ADMIN) ||
       HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_NOM_GROUP_PRICE_UPLOAD)))
  {%>
      <%= Html.Telerik().Upload()
            .Name("attachments")
            .Multiple(false)
      %>
<%}%>

<input name="selectType" id="selectType" type="hidden" value="1"/>
<iframe id="upload_target" name="upload_target" src="" style="width:100%;height:400;border:0px solid #fff;"></iframe>
               <table border="0" cellpadding="0" cellspacing="0" align="center">
               <tr>
                <td>
                    <input type="button" value="Отмена" class="t-button" onclick="closeWindowClick('#loadDataWindow')"/>
                    &nbsp;&nbsp;
                </td>
                <td>
                    &nbsp;&nbsp;
                    <input type="submit" value="Загрузить" class="t-button" />
                </td>
               </tr>
               </table>
</form>
           <%})
           .Render();
    %>

<script type="text/javascript">
    function onNomGroupPriceDataBinding(e) {
        var priceListId = $("#PriceList").data("tDropDownList").value();
        var nomGroupId = $("#NomGroup").data("tComboBox").value();
        e.data = $.extend({}, e.data, { priceId: priceListId, nomGropId: nomGroupId });
    }

    function updateGrid() {
        $("#NomGroupPrice").data("tGrid").rebind();
    }

    function loadDataClick() {
        var window = $("#loadDataWindow").data("tWindow");
        window.center().open();
    }


    function closeWindowClick(winName) {
        updateGrid();
        var window = $(winName).data("tWindow");
        window.close();
    }

    window.onresize = onFormResize;

    function onFormResize() {
        $("#NomGroup").data("tComboBox").disable();
        $("#NomGroup").data("tComboBox").enable();
    }

    function onFormSubmit() {
        return true;
    }


    function updateScreenParam(index) {
        document.getElementById("selectType").value = index;
        if (index == 1) {
            document.getElementById("onReplase").checked = true;
            document.getElementById("onUpdate").checked = false;
        }
        if (index == 2) {
            document.getElementById("onReplase").checked = false;
            document.getElementById("onUpdate").checked = true;
        }

    }


</script>
</asp:Content>
