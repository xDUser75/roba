<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage"%>
<%@ Import Namespace="Store.Data" %>
<%@ Import Namespace="Store.Core" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<h3>Загрузка из файла остатков</h3>

    <% 
        int row_count = int.Parse((string)ViewData["row_count"]);
        bool showWindow = row_count > 0;
        
        Html.Telerik().Window()
           .Name("Window")
           .Title("Статус загрузки файла")
           .Width(600)
           .Height(350)
           .Draggable(true)
           .Modal(true)
           .Buttons(b => b.Close())
           .Content(() =>
           {%>
           <%
               for (int i = 0; i < row_count; i++)
               {
                   if (ViewData["messRow" + i] != null)
                   {
                       %>
                       <%:HttpUtility.HtmlDecode((string)ViewData["messRow" + i])%><br/>
                       <%
                   }
               }
            %>
           <%})
           .Visible(showWindow)
           .Render();
    %>

<% using (Html.BeginForm("Save", "UploadFiles",
                         FormMethod.Post, new { id = "uploadForm", enctype =
                             "multipart/form-data" })) { %>
   <table border="0" width="100%">
        <tr>
        <td align="right">Склад:</td>
            <td>            
            <%:
            Html.Telerik().DropDownList()
                           .Name("StorageList")
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
            <td><input type="button" value="Найти" class="t-button" onclick="findRemains()"/></td>
        </tr>            
    </table>
<%if ((HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_ADMIN) ||
       HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_REMAINS_EXTERNAL_UPLOAD)))
  {%>
      <%= Html.Telerik().Upload()
            .Name("attachments")
            .Multiple(false)
      %>
            <input type="submit" value="Загрузить на сервер" class="t-button" />
<%}%>
<% } %>

<%: 
   Html.Telerik().Grid<Store.Core.RemaindExternal>()
            .Name("RemainsGrid")
            .DataBinding(dataBinding =>
              dataBinding
              .Ajax()
              .Select("_SelectRemains", "UploadFiles")
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
          columns.Bound(x => x.QuantityIn)
              .Filterable(false)
              .Width(85);
          columns.Bound(x => x.QuantityOut)
              .Filterable(false)
              .Width(85);            
          columns.Bound(x => x.Quantity)
              .Filterable(false)
              .Width(85);
          columns.Bound(x => x.StringWear)
              .HtmlAttributes(new { @align = "center" })
              .Title("Износ")
              .Width(80);
        })
        .ClientEvents(events => events
            .OnDataBinding("dataBinding")
            .OnError("onError")
            )
        //.Pageable()
        .Scrollable(x => x.Height(400))
        //.Resizable(resizing => resizing.Columns(true))
        .Sortable()
        .Filterable()
        
%>
<script type="text/javascript">
    function findRemains() {
        var date = document.getElementById('date').value;
        var grid = $("#RemainsGrid").data("tGrid");
        if (date != "") {
            grid.rebind({
                dateString: date
            });
        }
    }

    function dataBinding(args) {
        var param = document.getElementById('date').value;
        var dropDownList = $("#StorageList").data("tDropDownList");
        var storageId = "";
        if (dropDownList) {
            storageId = dropDownList.value();
        }
        if (storageId == "") {
            storageId = "-1";
        }
        args.data = $.extend(args.data, { date: param, idStorage: storageId });
    }

    function onError(e) {
        onGridErrorEvent(e);
    }
</script>
</asp:Content>
