<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage"%>
<%@ Import Namespace="Store.Data" %>
<%@ Import Namespace="Store.Core" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<h3>Загрузка данных из Excel в систему</h3>


<form id="uploadForm" method="post" enctype="multipart/form-data" action="ExternalLoad\Save" onsubmit="return onFormSubmit();" target="upload_target">
   <table border="0" width="100%">
   <tr>
        <td><input type="radio" id="onHand" checked="checked" onclick="updateScreenParam(1);"/>Загрузка спецодежды на руках</td>
        <td rowspan="2">
        <span id="onHandSpan">
        Операция:
            <%:
            Html.Telerik().DropDownList()
                          .Name("OperTypeList")
                          .BindTo((SelectList)ViewData[Store.Data.DataGlobals.REFERENCE_OPER_TYPE])
                          .HtmlAttributes(new { style = "width: 250px" })
             %>  
             </br>                       
        Дата загрузки: &nbsp;
            <%= Html.Telerik().DatePicker()
                     .Name("dateHand")
                     .Value(DateTime.Today)
                     .Format("dd.MM.yyyy")
            %>
             </span>


        <span id="onStorageSpan" style="display:none">
        Склад:
            <%:
            Html.Telerik().DropDownList()
                           .Name("StorageList")
                           .BindTo((SelectList)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE_SHOP_NAME])
                          .HtmlAttributes(new { style = "width: 250px" })
             %><br/>
        Остатки на: &nbsp;
            <%= Html.Telerik().DatePicker()
                     .Name("dateStorage")
                     .Value(DateTime.Today)
                     .Format("01.MM.yyyy")
            %>                          
             </span>
        </td>
        <td align="right" rowspan="2"><input type="submit" value="Загрузить на сервер" class="t-button" /></td>
   </tr>

   <tr>
        <td><input type="radio" id="onStorage" onclick="updateScreenParam(2);"/>Остатки на складах</td>
   </tr>
    </table>
<%if ((HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_ADMIN) ||
       HttpContext.Current.User.IsInRole(Store.Data.DataGlobals.ROLE_REMAINS_EXTERNAL_UPLOAD)))
  {%>
      <%= Html.Telerik().Upload()
            .Name("attachments")
            .Multiple(false)
            .ClientEvents(events => events
                .OnSelect("onSelect")            
            )

      %>
<%}%>
<input name="selectType" id="selectType" type="hidden" value="1"/>
<input name="date" id="date" type="hidden"/>
<iframe id="upload_target" name="upload_target" src="" style="width:100%;height:400;border:0px solid #fff;"></iframe>
</form>


<script type="text/javascript">
    function onFormSubmit() {
        if (document.getElementById("selectType").value == "1") {
            document.getElementById("date").value = document.getElementById("dateHand").value;
        } else {
            document.getElementById("date").value = document.getElementById("dateStorage").value;
        }
        return true;
    }


    function onSelect(e) {
        document.getElementById("upload_target").src = "";
    }

    function updateScreenParam(index) {
        document.getElementById("selectType").value = index;
        if (index == 1) {
            document.getElementById("onHandSpan").style.display = "";
            document.getElementById("onStorageSpan").style.display = "none";
            document.getElementById("onHand").checked = true;
            document.getElementById("onStorage").checked = false;
        }
        if (index == 2) {
            document.getElementById("onHandSpan").style.display = "none";
            document.getElementById("onStorageSpan").style.display = "";
            document.getElementById("onHand").checked = false;
            document.getElementById("onStorage").checked = true;
        }

    }

</script>
</asp:Content>
