<%@ Import Namespace="Store.Data" %>
<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<style>
.t-ins
{
    background-image:url('Content/Images/ins_button.gif');
    width:16px;
}

.t-upd
{
    background-image:url('Content/Images/edit_button.gif');
    width:16px;
}

.t-del
{
    background-image:url('Content/Images/del_button.gif');
    width:16px;
}
</style>
<h3>Размеры работников</h3>
    <form id="form1" name="form1" runat="server"> 
    <table border="0">
        <tr>
            <td align="right">Табельный номер</td>
            <td>
            <%=Html.Telerik()
                    .IntegerTextBox()
                    .Name("tabn")
                    .EmptyMessage("")
                    .MinValue(1)
                    .Spinners(false)
%>
            </td>
            <td  align="right">Фамилия</td>
            <td><input type="text" size="20" name="fio" id="fio" /></td>
            <td align="center"><input type="button" value="Найти" class="t-button" onclick="findWorkersClick()"/></td>
        </tr>    
    </table>
    </form>
<%: 
Html.Telerik().Grid((IEnumerable<Store.Core.Worker>)ViewData["Workers"])
        .Name("Workers")
        .Columns(columns =>
        {
          columns.Bound(x => x.TabN).Width(110).Title("Таб. №");
          columns.Bound(x => x.Fio).Title("Ф.И.О.");
          columns.Bound(x => x.Sex)
               .ClientTemplate("<#= Sex.Name #>")
               .Title("Пол");
          columns.Bound(x => x.Id).Hidden(true);            
        })
        .DataBinding(dataBinding => dataBinding.Ajax()
            .Select("_SelectionClientSide_Workers",
            "WorkerSizes",new { tabn = "0",fio = "" })
            )                          
        .Pageable()
        .Sortable()
        .Selectable()
        .ClientEvents(events => events.OnRowSelect("onRowSelected"))
        .RowAction(row => row.Selected = row.DataItem.Id.Equals(ViewData["id"])) 
%>
<p>Размеры сотрудника:</p>
<%Html.Telerik().Grid((IEnumerable<Store.Core.WorkerSize>)ViewData["WorkerSizes"])
        .Name("WorkerSizes")
        .ToolBar(toolBar => toolBar.Template(() =>
                                    { %>
<a title="Добавить" class='t-button t-button-icon' href='javascript:editUpdateClick(0)'><span class='t-icon t-ins'></span></a>
                                     <% }))

        .DataKeys(keys =>
        {
            keys.Add(x => x.Id);
        })
        .Columns(columns =>
        {
            columns.Bound(x => x.Id)
              .Width("100px")
              .Title("Действия")
              .ClientTemplate("<a title='Изменить' class='t-button t-button-icon' href='javascript:editUpdateClick(<#= Id #>)'><span class='t-icon t-upd'></span></a><a title='Удалить' class='t-button t-button-icon' href='javascript:deleteClick(\"<#= Id #>\")'><span class='t-icon t-del'></span></a>");
            columns.Bound(c => c.NomBodyPartSize.NomBodyPart).Title("Вид размера").ClientTemplate("<#= NomBodyPartSize.NomBodyPart.Name #>");
            columns.Bound(c => c.NomBodyPartSize.SizeNumber).Width(100);
        })
        .DataBinding(dataBinding => dataBinding.Ajax()
          .Select("_SelectionClientSide_WorkerSizes", "WorkerSizes", new { id = "-1" })
          )
          .Editable(editing => editing.Mode(GridEditMode.InLine))
          .Pageable()
        .Render();
        
%>

<input type="hidden" id="_itemSize" name="_itemSize" />
<input type="hidden" id="_workerId" name="_workerId" />
    <% Html.Telerik().Window()
           .Name("Window")
           .Title("Размер работника")
           .Width(400)
           .Draggable(true)
           .Modal(true)
           .Visible(false)
           .Buttons(b => b.Close())
           .Content(() =>
           {%>
           <br />
               Вид размера:
               <%:
                   Html.Telerik().DropDownList()
                  .Name("DropDownList")
                  .BindTo(new SelectList((IEnumerable)ViewData[DataGlobals.REFERENCE_NOM_BODY_PART], "Id", "Name"))
                  .HtmlAttributes(new { style = "width: 400px" })
                  .ClientEvents(events => events
                         .OnClose("onUpdateSizeListBox")
                  )
               %>
               <br />
               <br />
               Размер:
               <%:
                   Html.Telerik().DropDownList()
                  .Name("DropDownList1")
                  .DataBinding(binding => binding.Ajax().Select("_Select_NombodyPartSize", "WorkerSizes"))
                  .HtmlAttributes(new { style = "width: 400px" })
                  .ClientEvents(events => events
                  .OnDataBinding("onStorageListBoxBinding")
                  )        

               %>
               <br /><br /><br />
               <table border="0" cellpadding="0" cellspacing="0" align="center">
               <tr>
                <td>
                    <input type="button" value="Отмена" class="t-button" onclick="closeWindowClick()"/>
                </td>
                <td>
                    <input type="button" value="Сохранить" class="t-button" onclick="editUpdateSaveClick()"/>
                </td>
               </tr>
               </table>
           <%})
           .Render();
    %>
<script type="text/javascript">
    var req;

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

    function getDetailGridTop() {
        var workerSizeGrid = document.getElementById('WorkerSizes');
        if (workerSizeGrid) {
            divElements = workerSizeGrid.getElementsByTagName("div");
            for (i = 0; i < divElements.length; i++) {
                divClassName = divElements[i].className;
                if (divClassName.indexOf("t-grid-top") > 0) {
                    return divElements[i];
                }
            }
        }
        return null;
    }

    var editPanel = getDetailGridTop();
    var dropDownListCount = new  Array(<%: ViewData["dropDownListCount"] %>);

    function onRowSelected(e) {
        var workerSizesGrid = $('#WorkerSizes').data('tGrid');
        var workerId = document.getElementById("_workerId").value;
        if (e != null){
            workerId = e.row.cells[3].innerHTML;
             document.getElementById("_workerId").value = workerId;
        }
        document.getElementById("_itemSize").value = "";
        // rebind the related grid
        workerSizesGrid.rebind({
            id: workerId
        });
        if (editPanel) {
            editPanel.style.display = "";
        }
    }

    function selectSizeListBox(idBodySize) {
        var combobox = $("#DropDownList").data("tDropDownList");
        for (i=1;i<=dropDownListCount.length;i++){
            if (idBodySize==combobox.value()){
                combobox.value(i);
            }
        }
        
    }

    function editUpdateClick(idBodySize) {
        selectSizeListBox(idBodySize);
        onUpdateSizeListBox(null);
        document.getElementById("_itemSize").value = idBodySize;
        var window = $("#Window").data("tWindow");
        window.open();
    }

    function findWorkersClick() {
        if (editPanel) {
            editPanel.style.display = "none";
        }
        document.getElementById("_itemSize").value = "";
        var tabn = document.getElementById('tabn').value;
        var fio = document.getElementById('fio').value;
        if ((tabn == '') && (fio == '')) {
            alert("Мало условий для поиска!");
            return false;
        }
        var workersGrid = $('#Workers').data('tGrid');
        // rebind the related grid
        workersGrid.rebind({
            tabn: tabn, fio: fio
        });
    }

    function onUpdateSizeListBox(e) {
        var combobox = $("#DropDownList1").data("tDropDownList");
        combobox.reload();
    }

    function onStorageListBoxBinding(e) {
        var dropDownList = $("#DropDownList").data("tDropDownList");
        var id = dropDownList.value();
        e.data = $.extend({}, e.data, { Id: id });
    }

    function closeWindowClick() {
        var window = $("#Window").data("tWindow");
        window.close();
    }

    function checkStatus() {
        if (req.readyState == 4) {
            if (req.status == 200) { 
                var window = $("#Window").data("tWindow");
                window.close();
                onRowSelected(null);
            } else {
                alert("Произошла ошибка запроса " + req.status + ":\n" + req.statusText);
            }
        }
    }
    
    function deleteClick(id){
        cssConfirm("Вы действительно хитите удалить эту запись?", "Да", "Нет", function (state) {
            if (state) {
                if  (!sendRequest("WorkerSizes/_Delete_WorkerSizes","id="+id,checkStatus)){
                     alert("Не могу вызвать запрос на удаление размера для сотрудника!");
                }
            }
        });
    }

    function editUpdateSaveClick() {
        var combobox = $("#DropDownList1").data("tDropDownList");
        var id = combobox.value();
        itemSize = document.getElementById("_itemSize").value;
        if (itemSize==""){
            itemSize = "0";
        }
        var metod = "_Insert_WorkerSizes";
        if (itemSize != "0"){
            metod = "_Update_WorkerSizes";
        }
        if  (!sendRequest("WorkerSizes/"+metod,"id="+id+"&oldId="+itemSize,checkStatus)){
             alert("Не могу вызвать запрос на сохранение/удаление размера для сотрудника!");
        }
    }

    if (editPanel) {
        editPanel.style.display = "none";
    }
</script>
</asp:Content>