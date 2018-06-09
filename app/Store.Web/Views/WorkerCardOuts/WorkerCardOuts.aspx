<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Store.Core" %>
<%@ Import Namespace="Store.Data" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <script type="text/javascript" src="Scripts/Base.js"></script>
    <script type="text/javascript" src="Scripts/Lock.js"></script>
<h3>Списание спецодежды по акту</h3>
    <table border="0">
        <tr>
            <td align="right">Табельный/Ф.И.О</td>
            <td>
            <%= Html.Telerik().ComboBox()
                     .Name("WorkerWorkplaceCombo")
                     .AutoFill(false)
                     .DataBinding(binding => binding.Ajax()
                        .Select("_GetWorkerWorkplaces", "WorkerCards", new { isActive = false })
                        .Delay(400)
                        //.Cache(false)
                        )
                    .HtmlAttributes(new { @style = "width:700px" })
                    .Filterable(filtering =>
                        filtering
                        .FilterMode(AutoCompleteFilterMode.Contains)
                        .MinimumChars(1)
                    )
                    //.Items(items =>
                    //    {
                    //    if (Session["workerWorkplaceId"] != null)
                    //        items.Add().Value("" + (int)Session["workerWorkplaceId"]).Text((string)Session["workerWorkplaceText"]);
                    //    })
                    .SelectedIndex(0)
                    .ClientEvents(events => events.OnChange("clearWorkerCard"))
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
            %>
            </td>
            <td align="center"><input type="button" value="Найти" class="t-button" onclick="findWorkerCard()"/></td>
        </tr>    
    </table>

    <div id="contentWorkerCard" style="display:<%: Session["workerWorkplaceId"] != null?"block":"none" %>">
<%
    Html.Telerik().Grid<WorkerWorkplace>()
         .Name("WorkerWorkplace")
         .DataBinding(dataBinding =>
         {
             dataBinding
               .Ajax()
               .Select("Select_Worker", "WorkerCards");
         })
         .Columns(columns =>
         {
             columns.Bound(x => x)
               .ClientTemplate("Пол: <#= Worker.Sex != null?\"<b>\"+Worker.Sex.Name+\"</b>\":\"\" #>"
                 + "&nbsp;|&nbsp;<#= Worker.NomBodyPartSizes[0] != null?Worker.NomBodyPartSizes[0].NomBodyPart.Name+\": <b>\"+Worker.NomBodyPartSizes[0].SizeNumber+\"</b>\":\"\" #>"
                 + "&nbsp;|&nbsp;<#= Worker.NomBodyPartSizes[1] != null?Worker.NomBodyPartSizes[1].NomBodyPart.Name+\": <b>\"+Worker.NomBodyPartSizes[1].SizeNumber+\"</b>\":\"\" #>"
                 + "&nbsp;|&nbsp;<#= Worker.NomBodyPartSizes[2] != null?Worker.NomBodyPartSizes[2].NomBodyPart.Name+\": <b>\"+Worker.NomBodyPartSizes[2].SizeNumber+\"</b>\":\"\" #>"
                 + "&nbsp;|&nbsp;<#= Worker.NomBodyPartSizes[3] != null?Worker.NomBodyPartSizes[3].NomBodyPart.Name+\": <b>\"+Worker.NomBodyPartSizes[3].SizeNumber+\"</b>\":\"\" #>"
                 + "&nbsp;|&nbsp;Категория: <#= Worker.WorkerCategory != null?\"<b>\"+Worker.WorkerCategory.Name+\"</b>\":\"---\" #>"
                 + "&nbsp;|&nbsp;Группа: <#= Worker.WorkerGroup != null?\"<b>\"+Worker.WorkerGroup.Name+\"</b>\":\"---\" #>"
                 + "</br>Норма: <#= Organization.NormaOrganization != null?\"<b>\"+Organization.NormaOrganization.Norma.Name+\"</b>\":\"\" #>");
         })
         .HtmlAttributes(new { @class = "attachment-grid" })
         .ClientEvents(events => events
             .OnLoad("HideHeader")
             .OnDataBinding("dataBinding"))
         .Footer(false)
         .Render();
%>
<br />
<% 
    Html.Telerik().Grid<WorkerNorma>()
         .Name("WorkerNorma")
        //.ToolBar(commands => {
        //    //commands.Insert().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Добавить" });
        //    //commands.Insert().ButtonType(GridButtonType.Text).ImageHtmlAttributes("style='visible:none;'");
        //    if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
        //        HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_CARD_EDIT))
        //    {
        //        commands.SubmitChanges();
        //        commands.Custom()
        //            .Text("Отмена списания")
        //            .HtmlAttributes(new { id = "cancelOper", onclick = "onCancelOper()" })
        //            .Url("#");
        //    }
        //})
         .ToolBar(toolBar =>
             {
                 if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                     HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_CARD_OUT_EDIT))
                 {
                     toolBar.SubmitChanges();
                     toolBar.Template(() =>
                         {
                         %>
                            <a class="t-button t-grid-save-changes" href="#">Сохранить изменения</a>
                            <a class="t-button t-grid-cancel-changes" href="#">Отменить изменения</a>
                            <!--a class="t-button" id="cancelOper" onclick="onCancelOper()" href="#">Отмена списания</a-->

                            <label for="docNum" >№ док.:</label>
                            <input class="t-input" style="vertical-align: middle;" type="text" id="docNum" value="" size="8"/>
                            <label for="docDate">Дата док.:</label>
                            <%= Html.Telerik().DatePicker()
                                    .Name("docDate")
                                    .HtmlAttributes(new { id = "docDate_wrapper", style = "vertical-align: middle; width:80px" })
                                    .InputHtmlAttributes(new {size = "4"}) // не работает
                                    .Value(DateTime.Today)
                                    .Format(DataGlobals.DATE_FORMAT)
                            %>
                            &nbsp;&nbsp;&nbsp;
                            <%= Html.Telerik().DropDownList()
                                    .Name("operType")
                                    .HtmlAttributes(new { @style = "vertical-align: middle; width:150px" })
                                    .BindTo(new SelectList((IEnumerable)ViewData[DataGlobals.REFERENCE_OPER_TYPE], "Id", "Name"))
                                    .ClientEvents(events => events.OnChange("changeOperType"))
                            %>
                            &nbsp;
                            <label for="motiv">Основание:</label>
                            <% Html.Telerik().DropDownList()
                                    .Name("motiv")
                                    .HtmlAttributes(new { @style = "vertical-align: middle; width:100px" })
                                    .ClientEvents(events => events.OnDataBinding("onMotivDataBinding")
                                                                  .OnChange("changeMotiv"))
                                    
                                    .DataBinding(dataBinding => dataBinding.Ajax()
                                                               .Select("Select_Motivs", "WorkerCardOuts"))
                                     .Render();
                            %>
                            &nbsp;
                            <% Html.Telerik().DropDownList()
                                    .Name("cause")
                                    .HtmlAttributes(new { @style = "vertical-align: middle; width:220px" })
                                    .ClientEvents(events => events.OnDataBinding("onCauseDataBinding"))
                                    
                                    .DataBinding(dataBinding => dataBinding.Ajax()
                                                               .Select("Select_Causes", "WorkerCardOuts"))
                                     .Render();
                            %>                         
                         <%
                         });
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
               .Select("Select", "WorkerCardOuts")
               .Insert("Save", "WorkerCardOuts")
               .Update("Update", "WorkerCardOuts")
               .Delete("Delete", "WorkerCardOuts");
         })
         .Columns(columns =>
         {
             //columns.Command(commands =>
             //{
             //    if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
             //        HttpContext.Current.User.IsInRole(DataGlobals.ROLE_WORKER_EDIT))
             //    {
             //        //commands.Edit().ButtonType(GridButtonType.Image).HtmlAttributes(new { style = "display:none" }).ImageHtmlAttributes(new { title = "Редактировать" });
             //        commands.Edit().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Редактировать" });
             //        commands.Delete().ButtonType(GridButtonType.Image).ImageHtmlAttributes(new { title = "Удалить" });
             //    }
             //}).Width(90).Title("");
             columns.Bound(x => x.StorageNumber).Width(80).Title("Склад").ReadOnly();
             columns.Bound(x => x.StorageInfo)
               .EditorTemplateName("StorageNomTemplate")
                 //.ClientTemplate("<#= Storage.StorageInfo #>")
               .Title("[Код SAP] Номенклатура (размер, рост, износ, кол-во)")
               .ReadOnly();
             //.Width(400);
             columns.Bound(x => x.PresentQuantity)
                 .Width(100)
                 .Title("На руках")
                 .ReadOnly();
             columns.Bound(x => x.ReceptionDate)
                 .Format("{0:" + DataGlobals.DATE_FORMAT + "}")
                 .Width(100)
                 .Title("Выдано")
                 .ReadOnly();
             columns.Bound(x => x.PutQuantity)
                 .Width(100)
                 .Title("Списать");
             //columns.Bound(x => x.NormaContentId).Hidden();
             columns.Bound(x => x.StorageId).Hidden();
         })
         .ClientEvents(events => events
             //.OnEdit("onEdit")
             .OnError("onError")
             .OnDataBound("onDataBound")
             //.OnSave("onSave")
             .OnSubmitChanges("onSubmit")
             .OnDataBinding("dataBinding"))
         .Editable(editing => editing.Mode(GridEditMode.InCell))
        //.Pageable(x => x.PageSize(10))
        //.Sortable()
        //.Groupable()
        //.Filterable()
        //.TableHtmlAttributes("width='70%'")
         .Scrollable(x => x.Height(300))
         .Render();
%>
</div>
<script type="text/javascript">
//  var normaContentId;
  function onEdit(e) {
  }

  function clearWorkerCard() {
      $("#WorkerWorkplace").data("tGrid").dataBind(new Array());
      $("#WorkerNorma").data("tGrid").dataBind(new Array());
  }
  function findWorkerCard() {
      $("#WorkerWorkplace").data("tGrid").dataBind(new Array());
      $("#WorkerNorma").data("tGrid").dataBind(new Array());
      var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
      var text = $("#WorkerWorkplaceCombo").data("tComboBox").text();
      if (workerWorkplaceId != "" && workerWorkplaceId != text) {
          $("#WorkerWorkplace").data("tGrid").rebind();
          $("#WorkerNorma").data("tGrid").rebind();
          document.getElementById("contentWorkerCard").style.display = "block";
      }
      $("#operType").data("tDropDownList").value(<%=DataGlobals.OPERATION_WORKER_OUT %>);
       $("#cause").data("tDropDownList").value(<%=DataGlobals.CAUSE_OPERATION_WORKER_OUT %>);
  }

  function dataBinding(args) {
      var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
      var workerWorkplaceText = $("#WorkerWorkplaceCombo").data("tComboBox").text();
      args.data = $.extend(args.data, { workerWorkplaceId: workerWorkplaceId, workerWorkplaceText: workerWorkplaceText });
  }

  function onSave(e) {
      var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
      e.data = $.extend(e.data, { workerWorkplaceId: workerWorkplaceId });
  }

  function onSubmit(e) {
      var docNum = document.getElementById("docNum").value;
      if (docNum == "") {
          alert("№ документа не задан!");
          return false;
      }
      if (nkmk.Lock.isFree()) {
          nkmk.Lock.setBusy();
          nkmk.Lock.printBusyMessage("Идет обработка данных...");
      }
      var docDate = document.getElementById("docDate").value;
      var motiv = document.getElementById("motiv").value;
      var operType = document.getElementById("operType").value;
      var cause = document.getElementById("cause").value;
      e.updated[0] = $.extend(e.updated[0], { DocNumber: docNum, DocDate: docDate, MotivId: motiv, OperTypeId: operType , CauseId: cause});
  }

  function onCancelOper() {
      cssConfirm("Отменить списание?", "Да", "Нет", function (state) {
         if (state) {     
              if (nkmk.Lock.isFree()) {
                  nkmk.Lock.setBusy();
                  nkmk.Lock.printBusyMessage("Идет обработка данных...");
              }
              //var $cancelOper = $('#cancelOper');
              $.post("WorkerCardOuts/CancelOper", function (data) {
                  var $grid = $("#WorkerNorma").data("tGrid");
                  $grid.rebind();
              });
         }
      });
  }

  function onStorageComboBoxDataBinding(e) {
      //var nomGroupId = e.row.cells[1].innerHTML;
      //alert(nomGroupId);
      //e.data = $.extend({}, e.data, { normaContentId: normaContentId });
  }

  function HideHeader() {
      $('.attachment-grid .t-header').hide();
  };

  function onError(args) {
      nkmk.Lock.setFree();
      if (args.textStatus == "modelstateerror" && args.modelState) {
          var message = "Errors:\n";
          $.each(args.modelState, function (key, value) {
              if ('errors' in value) {
                  $.each(value.errors, function () {
                      message += "["+key+"] "+this + "\n";
                  });
              }
          });
          args.preventDefault();
          alert(message);
      }
  }

  function onDataBound(e) {
      nkmk.Lock.setFree();
      changeOperType(e);
  }
  function changeOperType(args) {
     
      var motiv = $("#motiv").data("tDropDownList");
      motiv.reload(); 
   }

   function onMotivDataBinding(args) {
      var operTypeId = $("#operType").data("tDropDownList").value();
      args.data = $.extend(args.data, { operTypeId: operTypeId});
  }
   function onCauseDataBinding(args) {
      var motivId = $("#motiv").data("tDropDownList").value();
      args.data = $.extend(args.data, { motivId: motivId});
  }

  function changeMotiv(args) {
     
//      var motiv = $("#motiv").data("tDropDownList");
//      motiv.reload(); 
       var cause = $("#cause").data("tDropDownList");
       cause.reload();
   }

  </script>
</asp:Content>
