<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Store.Core" %>
<%@ Import Namespace="Store.Data" %>

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
    <div id="NormaContentDiv">
    <!--#include file="../Shared/NormaContent.inc"-->
    </div>

<h3>Сотрудники</h3>
    <table border="0">
        <tr>
            <td align="right">Табельный/Фамилия</td>
            <td><input type="text" size="20" name="param" id="param" onkeypress=""/></td>
            <td align="center"><input type="button" value="Найти" class="t-button" onclick="findWorkers()"/></td>
        </tr>    
    </table>
<%: 
   Html.Telerik().Grid<Worker>()
        .Name("Workers")
        .ToolBar(commands => {
           if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_EDIT))
            {
//            commands.Insert().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Добавить" });
            }
            })
        .DataKeys(keys =>
        {
          keys.Add(x => x.Id);
        })
        .DataBinding(dataBinding =>
        {
          dataBinding
            //.Server()
            .Ajax()
            .Select("Select", "Workers")
            .Insert("Save", "Workers")
            .Update("Save", "Workers")
            .Delete("Delete", "Workers");
        })
        .Columns(columns =>
        {
          columns.Command(commands =>
          {
            if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_EDIT))
            {
              commands.Edit().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Редактировать" });
              commands.Delete().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Удалить" });
            }
          }).Width(90).Title("");
          columns.Bound(x => x.TabN).Width(100).Title("Табельный").ReadOnly();
          columns.Bound(x => x.Fio).Title("Ф.И.О.").ReadOnly();
          columns.Bound(x => x.IsTabu)
                .ClientTemplate("<input title='Признак разрешить или запретить выдачу сотруднику СИЗ в салоне (галочка - запрет)' type='checkbox' disabled='true'  name='istabu' <#= (IsTabu)?'checked':'' #> />")
                .Width(80).Title("Запрет");

          columns.Bound(x => x.Sex).Width(80).ReadOnly()
            .ClientTemplate("<#=(Sex==null?'':Sex.Name)#>")
            .Title("Пол");
          columns.Bound(x => x.ChildcareBegin).Format("{0:" + DataGlobals.DATE_FORMAT + "}").Width(100).Title("Нач. ДО").ReadOnly();
          columns.Bound(x => x.ChildcareEnd).Format("{0:" + DataGlobals.DATE_FORMAT + "}").Width(100).Title("Ок. ДО").ReadOnly();
          columns.Bound(x => x.WorkDate).Format("{0:" + DataGlobals.DATE_FORMAT + "}").Width(100).Title("Прием на работу").ReadOnly();
          columns.Bound(x => x.DateZ).Format("{0:" + DataGlobals.DATE_FORMAT + "}").Width(100).Title("Увольнение").ReadOnly();

          if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN))
            columns.Bound(x => x.IsActive)
                .ClientTemplate("<input type='checkbox' disabled='true'  name='isactive' <#= (IsActive)?'checked':'' #> />")
                .Width(80).Title("Активный");
          columns.Bound(x => x.Id).Hidden(true);
          //columns.Bound(x => x.NomBodyPartSizes).Hidden();
        })

        .ClientEvents(events => events
            .OnDataBinding("dataBinding")
            .OnError("onGridErrorEvent")
            .OnRowSelect("onRowSelected"))
        .Editable(editing => editing.Mode(GridEditMode.InLine))
        //.Pageable()
        .Selectable()
        .Scrollable(x => x.Height(150))
        .Sortable()
        //.Groupable()
        .Filterable()
%>
<div>&nbsp;</div>
<div id="workersInfo">&nbsp;</div>
<div>&nbsp;</div>
<% Html.Telerik().TabStrip()
        .Name("TabStrip")
        .Items(tabstrip =>
        {
            tabstrip.Add()
               .Text("Размеры сотрудника")
               .Content(() =>
               {%>
<%Html.Telerik().Grid((IEnumerable<Store.Core.WorkerSize>)ViewData["WorkerSizes"])
        .Name("WorkerSizes")
        .ToolBar(toolBar => toolBar.Template(() =>
        { %>
                <a title="Добавить" class='t-button t-button-icon' href='javascript:editUpdateClick(0,0,0)'><span class='t-icon t-ins'></span></a>
         <% }))


        .DataKeys(keys =>
        {
            keys.Add(x => x.Id);
        })
        .Columns(columns =>
        {
            if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_EDIT))
            {

            columns.Bound(x => x.Id)
              .Width("100px")
              .Title("Действия")
              .ClientTemplate("<a title='Изменить' class='t-button t-button-icon' href='javascript:editUpdateClick(<#= Id #>,<#= NomBodyPartSize.NomBodyPart.Id #>,<#= NomBodyPartSize.Id #>)'><span class='t-icon t-upd'></span></a><a title='Удалить' class='t-button t-button-icon' href='javascript:deleteClick(\"<#= Id #>\")'><span class='t-icon t-del'></span></a>");
              }
            columns.Bound(c => c.NomBodyPartSize.NomBodyPart).Title("Вид размера").ClientTemplate("<#= NomBodyPartSize==null?'':NomBodyPartSize.NomBodyPart.Name #>");
            columns.Bound(c => c.NomBodyPartSize.SizeNumber).Title("Размер").Width(100).ClientTemplate("<#= (NomBodyPartSize==null?'':NomBodyPartSize.SizeNumber )#>");
            columns.Bound(c => c.NomBodyPartSize.NomBodyPart.Id).Hidden();
            columns.Bound(c => c.Worker.Id).Hidden();
        })
        .DataBinding(dataBinding => dataBinding.Ajax()
          //.Select("_SelectionClientSide_Worker", "Workers", new { id = "-1" })
          .Select("_SelectionClientSide_WorkerSizes", "Workers", new { id = "-1" })
          .Insert("_Insert_WorkerSizes", "Workers")
          .Update("_Update_WorkerSizes", "Workers")
          .Delete("_Delete_WorkerSizes", "Workers")
          )

          .Editable(editing => editing.Mode(GridEditMode.InLine))
          //.Pageable(pager => pager.PageSize(5))
          .Scrollable(c => c.Height(100))
        .Render();        
%>                    <%})
                    .Selected(true);
            tabstrip.Add()
                    .Text("Рабочие места")
                    .Content(() =>
                    {%>
<%: 
   Html.Telerik().Grid((IEnumerable<Store.Core.WorkerWorkplace>)ViewData["WorkerWorkPlace"])
          .Name("Workplace")
        .ToolBar(commands => 
        {
           if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_EDIT))
            {
//                commands.Insert().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Добавить" });
            }
        })
        .DataKeys(keys =>
        {
          keys.Add(x => x.Id);
        })
        .DataBinding(dataBinding =>
        {
          dataBinding
            //.Server()
            .Ajax()
            .Select("SelectWorkerWorplace", "Workers")
            .Insert("SaveWorkerWorplace", "Workers")
            .Update("UpdateWorkerWorplace", "Workers")
            .Delete("DeleteWorkerWorplace", "Workers");
        })
        .Columns(columns =>
        {
          columns.Command(commands =>
          {
            if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_EDIT))
            {
              commands.Edit().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Редактировать" });
              commands.Delete().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Удалить" });
            }
          }).Width(90);
         columns.Bound(x => x.StorageNumber).Width(50).Title("Склад").ReadOnly();
         columns.Bound(x => x.Organization.ShopNumber).Width(50).Title("Цех").ReadOnly();
         columns.Bound(x => x.Organization)
           .ClientTemplate("<#= Organization.FullName #>")
           .EditorTemplateName("OrganizationTemplate")
           .Title("Рабочее место");
        columns.Bound(x => x.Organization.OrganizationMvzName).Width(350).Title("МВЗ").ReadOnly();
        columns.Bound(x => x.BeginDate).Format("{0:" + DataGlobals.DATE_FORMAT + "}").Width(100).Title("Дата").ReadOnly();
        columns.Bound(x => x.IsActive)
            .ClientTemplate("<input type='checkbox' disabled='true'  name='isactive' <#= (IsActive)?'checked':'' #> />")
            .Width(80).Title("Активный");
         columns.Bound(x => x.Organization.Id).Hidden();
        })
        .ClientEvents(events => events
//          .OnEdit("onEdit")
            .OnError("onGridErrorEvent")
            .OnDataBinding("dataBindingWorkerWorkPlace")
            .OnRowSelect ("workerPlaceRowSelect"))
        .Editable(editing => editing.Mode(GridEditMode.InLine))
        .Pageable(pager => pager.PageSize(5))
        //.Scrollable(scroll => scroll.Height(200))
        //.Groupable()
        //.Filterable()
        .Selectable()
        .Sortable()
%>
                    <%});            
        })
        .Render(); %>
<input type="hidden" id="_itemSize" name="_itemSize" />
<input type="hidden" id="_workerId" name="_workerId" />
  <% Html.Telerik().Window()
           .Name("WindowSize")
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
                         .OnChange("onStorageListBoxBinding")
                  )
               %>
               <br />
               <br />
               Размер:
               <%:
                   Html.Telerik().DropDownList()
                  .Name("DropDownList1")
                  .DataBinding(binding => binding.Ajax().Select("_Select_NombodyPartSize", "Workers"))
                  .HtmlAttributes(new { style = "width: 400px" })
                  .ClientEvents(events => events
                    .OnDataBinding("onStorageListBoxBinding")
                    .OnDataBound("onSizeListBoxBound")
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
      function findWorkers() {
          //var workerWorkplaceId = $("#WorkerCombo").data("tComboBox").value();
          document.getElementById("workersInfo").innerHTML="&nbsp;";
          var param = document.getElementById('param').value;
          if (param != "") {
              $("#Workers").data("tGrid").rebind();
          }
      }

      function dataBinding(args) {
          //var param = $("#WorkerCombo").data("tComboBox").value();
          var param = document.getElementById('param').value;
          args.data = $.extend(args.data, { param: param });
      }

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
            workerId = e.row.cells[e.row.cells.length-1].innerHTML;
            document.getElementById("_workerId").value = workerId;
        }
        document.getElementById("_itemSize").value = "";
        if (e) {
            document.getElementById("workersInfo").innerHTML="Информация о сотруднике:"+e.row.cells[1].innerHTML+" - " + e.row.cells[2].innerHTML;
        }
        var data = new Array();
        workerSizesGrid.dataBind(data);
        // rebind the related grid
        workerSizesGrid.rebind({
            id: workerId
        });

        var workerWorkplaceGrid = $('#Workplace').data('tGrid');
        workerWorkplaceGrid.dataBind(data);
        workerWorkplaceGrid.rebind({
            param: workerId
        });


        if (editPanel) {
            editPanel.style.display = "";
        }
    }

    function selectSizeListBox(id) {
        var combobox = $("#DropDownList1").data("tDropDownList");
//        for (i=1;i<=dropDownListCount.length;i++){
//            if (id==combobox.value()){
                combobox.value(id);
//            }
//        }
//        
    }
    function selectNomBodyPartListBox(id) {
        var combobox = $("#DropDownList").data("tDropDownList");
        combobox.value(id);
    }

var _nomBodyPartSizeId;

    function editUpdateClick(idBodySize, nomBodyPartId, nomBodyPartSizeId) {
        selectNomBodyPartListBox(nomBodyPartId);
        onUpdateSizeListBox(null);
        document.getElementById("_itemSize").value = idBodySize;
        selectSizeListBox(nomBodyPartSizeId);
        _nomBodyPartSizeId=nomBodyPartSizeId;
        var comboboxSizeType = $("#DropDownList").data("tDropDownList");
        if (idBodySize==0){
            comboboxSizeType.enable();
        }
        else 
        {
            comboboxSizeType.disable();
        }
        var window = $("#WindowSize").data("tWindow");
        window.center().open();
    }

    function onUpdateSizeListBox(e) {
        var combobox = $("#DropDownList1").data("tDropDownList");
        combobox.reload();
    }

    function getCellIndexByName(cells,cellName) {
        for (_curCell = 0; _curCell < cells.length; _curCell++) {
            if (cells[_curCell].member.toUpperCase()==cellName.toUpperCase()){
                return _curCell;
            }
        }
        return 0;
    }

    function onStorageListBoxBinding(e) {
        var dropDownList = $("#DropDownList").data("tDropDownList");
        var id = dropDownList.value();
        var name = dropDownList.text();
        var currentNomBodyPartSize ='';
        var grid = $('#WorkerSizes').data('tGrid');
        
        var rows = grid.$rows();
        for (i = 0; i < rows.length; i++) {
            var row = rows[i];
            var currentNomBodyPartName = "";
            var cell = row.cells[getCellIndexByName(grid.columns, "NomBodyPartSize.NomBodyPart")];
            if (cell){
                currentNomBodyPartName = cell.innerHTML;
                if (currentNomBodyPartName==name){
                 currentNomBodyPartSize = row.cells[getCellIndexByName(grid.columns, "NomBodyPartSize.SizeNumber")].innerHTML;
                 currentNomBodyPartSize = row.cells[2];

                }
            }
        }
        e.data = $.extend({}, e.data, { Id: id});
    }

    function closeWindowClick() {
        var window = $("#WindowSize").data("tWindow");
        window.close();
    }

    function checkStatus() {
        if (req.readyState == 4) {
            if (req.status == 200) { 
                var window = $("#WindowSize").data("tWindow");
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
                if  (!sendRequest("Workers/_Delete_WorkerSizes","id="+id,checkStatus)){
                     alert("Не могу вызвать запрос на удаление размера для сотрудника!");
                }
            }
        });
    }

    function editUpdateSaveClick() {        
        var combobox = $("#DropDownList1").data("tDropDownList");
        var id = combobox.value();
        var workerId = document.getElementById("_workerId").value;
        itemSize = document.getElementById("_itemSize").value;
        if (itemSize==""){
            itemSize = "0";
        }
        var metod = "_Update_WorkerSizes";
        if (itemSize == "0"){
            metod = "_Insert_WorkerSizes";
            var comboboxSizeType = $("#DropDownList").data("tDropDownList");
            var sizeTypeId = comboboxSizeType.value();
            var grid = $('#WorkerSizes').data('tGrid');
            var rows = grid.$rows();
            for (i = 0; i < rows.length; i++) {
                var row = rows[i];
                var cell = row.cells[getCellIndexByName(grid.columns,"NomBodyPartSize.NomBodyPart.Id")];
                if (cell){
                    if (cell.innerHTML == sizeTypeId) {
                        alert("У сотрудника уже присутствует такой тип размера!");
                        return false;
                    }          
                }
            }
        }
        if  (!sendRequest("Workers/"+metod,"id="+id+"&oldId="+itemSize+"&workerId="+workerId,checkStatus)){
             alert("Не могу вызвать запрос на сохранение размера для сотрудника!");
        }
    }

    if (editPanel) {
        editPanel.style.display = "none";
    }

  function dataBindingWorkerWorkPlace(args) {
      var param = document.getElementById("_workerId").value;
      args.data = $.extend(args.data, { param: param });
  }

 function onEdit(e) {
    if (e.dataItem != null) {
      var obj = e.dataItem['Worker'];
      $(e.form).find('#Worker').data('tComboBox').value((obj == null) ? -1 : obj.Id);
      $(e.form).find('#Worker').data('tComboBox').text((obj == null) ? -1 : obj.Fio);

      obj = e.dataItem['Organization'];
      $(e.form).find('#Organization').data('tComboBox').value((obj == null) ? -1 : obj.Id);
      $(e.form).find('#Organization').data('tComboBox').text((obj == null) ? -1 : obj.FullName);

      obj = e.dataItem['DropDownList'];
      $(e.form).find('#DropDownList').data('tComboBox').value((obj == null) ? -1 : obj.Id);
      $(e.form).find('#DropDownList').data('tComboBox').text((obj == null) ? -1 : obj.Name);

      obj = e.dataItem['DropDownList1'];
      $(e.form).find('#DropDownList1').data('tComboBox').value((obj == null) ? -1 : obj.Id);
      $(e.form).find('#DropDownList1').data('tComboBox').text((obj == null) ? -1 : obj.SizeNumber);

    }
  }

    function checkNormaIdStatus() {
        if (req.readyState == 4) {
            if (req.status == 200) { 
         var normaContentsGrid = $('#NormaContents').data('tGrid');
         var normasGrid = $('#Normas').data('tGrid');
         var idNorma=req.responseText;
         if (idNorma=="") {
            idNorma=-1;
         }
         normaContentsGrid.rebind({
             Id: idNorma
         });
         normasGrid.rebind({
             Id: idNorma
         });

         var window = $("#Window").data("tWindow");
         document.getElementById("idNorma").value=idNorma;
//         document.getElementById("imgApprovedDiv").style.display="none";
         window.center().open();        
            } else {
                alert("Произошла ошибка запроса " + req.status + ":\n" + req.statusText);
            }
        }
    }

 function onSizeListBoxBound(e){
        selectSizeListBox(_nomBodyPartSizeId);
 }

 function workerPlaceRowSelect(e){
            var id = e.row.cells[7].innerHTML
            if  (!sendRequest("Workers/_GetNormaIdByOrganization","id="+id,checkNormaIdStatus)){
                 alert("Не могу вызвать запрос на получения идентификатора нормы!");
            }
return false;
//        var window = $("#nomenclatureWindow").data("tWindow");
//        window.center().open();    
 }
  </script>
</asp:Content>
