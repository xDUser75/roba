<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage"%>
<%@ Import Namespace="Store.Data" %>
<%@ Import Namespace="Store.Core" %>
<%@ Import Namespace="Store.Core.Account" %>
<!--#include file="../Shared/GetReportUrl.inc"-->
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <script type="text/javascript" src="Scripts/Base.js"></script>
    <script type="text/javascript" src="Scripts/Lock.js"></script>
    <script type="text/javascript">
       //Идентификатор МОЛ
        var workerWorkPlaceId = "-1";
    </script>
<h3>Дежурная спецодежда </h3>
<%
    Boolean isMol = (Boolean) ViewData["isMOL"];
%>        
    <table border="0" width="100%">   
        <tr>
            <td align="right">Табельный/Ф.И.О</td>
            <td>
<%
    if (!(Boolean)ViewData["isMOL"])
    {
     %>
            <%= Html.Telerik().ComboBox()
                     .Name("MatPersonCombo")
                     .AutoFill(false)
                     .DataBinding(binding => binding.Ajax()
//                        .Select("_FindMatPerson", "MatPersonStore")
                                .Select("_FindMatPerson", "MatPersonStore")
                        .Delay(400)
                        .Cache(false)
                        )
                    .HtmlAttributes(new { @style = "width:750px" })
                    .Filterable(filtering =>
                        filtering
                        .FilterMode(AutoCompleteFilterMode.Contains)
                        .MinimumChars(1)
                    )
                    .ClientEvents(events => events
                        .OnChange("updateGridContext")
                        .OnClose("updateGridContext")
                        .OnDataBinding("clearGridContext")
                    )                    
                    .SelectedIndex(0)
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
            %>
            &nbsp;&nbsp;
            <!-- input type="button" value="Найти" class="t-button" onclick="updateGridContext()"/-->
<%} else {%>
        <input type="text"  value="<%= ViewData["MOL"] %>"/>
<%} %>
            </td>
        </tr>
    </table>
<table width="100%">
<tr>
<td align="left">
<%
  if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
      HttpContext.Current.User.IsInRole(DataGlobals.ROLE_MOL_EDIT))
  {
%>
        <input type="button" value="Выдача МОЛ со склада" class="t-button" onclick="outStorageClick()"/>
        <input type="button" value="Возврат от МОЛ на склад" class="t-button" onclick="inStorageClick()"/>
        <!--input type="button" value="Выдача работнику" class="t-button" onclick="inPersonClick()"/>
        <input type="button" value="Возврат от работника" class="t-button" onclick="outPersonClick()"/-->
        <input type="button" value="Списание с МОЛ" class="t-button" onclick="outMolClick()"/>
        <input type="button" value="Перевод между МОЛ" class="t-button" onclick="moveMolClick()"/>
        <%}%>
        <font color="red"><b><span id="MvzName"></span></b></font>
</td>
<td align="right" width="250px">
    <%=
        Html.Button("reportMOL", "Печать Карточки МОЛ", HtmlButtonType.Button,
                                 "if (workerWorkPlaceId != '-1') window.open('" + getReportUrl("Карточка МОЛ") + "&RptMatPersonId='+getCurrentMatPersonId());",
                                    new { @class = "t-button"})
    %>

    <% if (HttpContext.Current.Session["_idOrg"].ToString()==DataGlobals.ORG_ID_VGOK){ %>
        <%=  Html.Button("reportM11", "Печать Требования", HtmlButtonType.Button,
                            "if (workerWorkPlaceId != '-1') window.open('" + getReportUrl("Требование на выдачу СИЗ") + "&RptOperTypeID=" + DataGlobals.OPERATION_MOL_STORAGE_OUT + "&RptUserName=" + Server.UrlEncode(((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName) + "&RptWorkerWorkplaceId='+getCurrentMatPersonId());",
                            new { @class = "t-button" })
        %>
                            
    <%}  else {%>

        <%=
            Html.Button("reportM11", "Печать M11", HtmlButtonType.Button,
                        "if (workerWorkPlaceId != '-1') window.open('" + getReportUrl("Накладная M11") + "&RptOperTypeID=" + DataGlobals.OPERATION_MOL_STORAGE_OUT + "&RptUserName=" + Server.UrlEncode(((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName) + "&RptWorkerWorkplaceId='+getCurrentMatPersonId());",
                                              
                        new { @class = "t-button"})
        %>
        <!--a class="t-button" id="cancelOper" onclick="onCancelOper()" href="#">Отмена выдачи</a-->
    <%}  %>
</td>
</tr>
</table>


        <% 
// Окно для прихода со склада            
        Html.Telerik().Window()
           .Name("outStorageWindow")
           .Title("Приход со склада")
           .Width(990)
           .Height(650)
           .Draggable(true)
           .Modal(true)
           .Visible(false)
           .Buttons(b => b.Close())
           .Content(() =>
          
           {%>
              
          <table width="100%">
           <tr>
           <td align="left">
 
               Дата:
               <%: Html.Telerik().DatePicker()
                       .Name("outStorageDocDate")
                      .Value(DateTime.Now.ToShortDateString())
                      .Format(Store.Data.DataGlobals.DATE_FORMAT_FULL_YEAR)
                %>
                </td>
                <td align="right">
                <%=
                    Html.Button("reportM11", "Печать Требования", HtmlButtonType.Button,
                                              "window.open('" + getReportUrl("Требование на выдачу СИЗ") + "&RptOperTypeID=" + DataGlobals.OPERATION_MOL_STORAGE_OUT + "&RptUserName=" + Server.UrlEncode(((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName) + "&RptWorkerWorkplaceId='+getCurrentMatPersonId());",
                                              new { @class = "t-button"})
                %>
                </td>
                </tr>
                </table>
             <%:
             
             Html.Telerik().Grid((IEnumerable<Store.Core.MatPersonOnHandsSimple>)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE])
                .Name("outStorageGrid")
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

                               .Columns(columns =>
                               {
                                   columns.Bound(c => c).ClientTemplate("<A class=\"t-button t-button-icon\" href=\"#\" onClick=\"javascript:delRow(this);\" sizcache=\"12\" sizset=\"0\"><SPAN class=\"t-icon\" style=\"background-image:url('Content/Images/del_button.gif')\"></SPAN></A>")
                                       .Title("")
                                       .Width(40)
                                       .ReadOnly()
                                       .Filterable(false)
                                       .Sortable(false);
                                   columns.Bound(c => c.Nomenclature).EditorTemplateName("StorageNomenclatureTemplate");
                                   columns.Bound(c => c.WorkQuantity).EditorTemplateName("Integer").Title("Кол-во").Width(100);
                                   columns.Bound(c => c.NomenclatureId).Hidden(true);                                   
                               })
                               .ClientEvents(events => events
                                 .OnEdit("onEditOutStorage")
                                 .OnError("onError")
                                 .OnSave("onSaveOutStorage")
                                )
                .DataBinding(dataBinding => dataBinding.Ajax()
                       .Update("_UpdateStorageMolData", "MatPersonStore")
                       .Select("_SelectEmptyRow", "Storages")
                  )
                  .Scrollable(x => x.Height(500))
             %>

            <div align="center">
               <table border="0" cellpadding="0" cellspacing="0">
               <tr>
                <td>
                    <input type="button" value="Отмена" class="t-button" onclick="closeWindowClick('#outStorageWindow')"/>
                    &nbsp;&nbsp;
                </td>
                <td>
                    &nbsp;&nbsp;
                    <input type="button" value="Сохранить" id="outStorageSaveButton" class="t-button" onclick="disabled=true ; outStorageSaveClick('outStorage')"/>
                </td>
               </tr>
               </table>
               </div>
           <%})
           .Render();
    %>


        <% 
// Окно для возврата на склад
        Html.Telerik().Window()
           .Name("inStorageWindow")
           .Title("Возврат на склад")
           .Width(990)
           .Height(650)
           .Draggable(true)
           .Modal(true)
           .Visible(false)
           .Buttons(b => b.Close())
           .Content(() =>
           {%>

        <table width="100%">
        <tr>
           <td align=left>
               Дата:
               <%: Html.Telerik().DatePicker()
                       .Name("inStorageDocDate")
                      .Value(DateTime.Now.ToShortDateString())
                      .Format(Store.Data.DataGlobals.DATE_FORMAT_FULL_YEAR)
                %>
           </td>
           <td align="right">
                            <%=
                                Html.Button("reportM4", "Печать М4", HtmlButtonType.Button,
                                                                                   "window.open('" + getReportUrl("Приходный ордер М-4") + "&RptOperTypeID=" + DataGlobals.OPERATION_MOL_STORAGE_IN + "&RptUserName=" + Server.UrlEncode(((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName) + "&RptWorkerWorkplaceId='+getCurrentMatPersonId());",
                                                     new { @class = "t-button" })
                           %>
            </td>
        </tr>
        </table>
             <%:             
             Html.Telerik().Grid((IEnumerable<Store.Core.MatPersonOnHandsSimple>)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE])
                .Name("inStorageGrid")
                .DataKeys(keys =>
                {
                  keys.Add(x => x.Id);
                })
                .Footer(false)
                               .ToolBar(commands =>
                               {
                                   commands.Insert().ButtonType(GridButtonType.Image);
                               })
                
                .Editable(editing => editing.Mode(GridEditMode.InCell))

                               .Columns(columns =>
                               {
                                   columns.Bound(c => c.ExternalCode).ReadOnly(true).Width(100);
                                   columns.Bound(c => c.Nomenclature).ReadOnly(true);
                                   columns.Bound(c => c.Wear)
                                       .Title("Износ")
                                       .ClientTemplate("<#= Wear=='100'?'новая':Wear=='50'?'б/у':Wear=='0'?'утиль':Wear #>")
                                       .ReadOnly(true).Width(100);                                   
                                   columns.Bound(c => c.Quantity).Title("На руках").Width(100).ReadOnly(true);
                                   columns.Bound(c => c.WorkQuantity).EditorTemplateName("Integer").Title("Вернуть").Width(100);
                                   columns.Bound(c => c.NomenclatureId).Hidden(true);                                   
                               })
                               .ClientEvents(events => events
                                 .OnError("onError")
                                )
                .DataBinding(dataBinding => dataBinding.Ajax()
                       .Update("_UpdateStorageMolData", "MatPersonStore")
                       .Select("_SelectEmptyRow", "Storages")
                  )
                  .Scrollable(x => x.Height(500))
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
                    <input type="button" value="Сохранить" id="inStorageSaveButton" class="t-button" onclick=" disabled=true ; inStorageSaveClick('inStorage')"/>
                </td>
               </tr>
               </table>
               </div>
           <%})
           .Render();
    %>


    <% 
// Окно для выдачи рабочим
        Html.Telerik().Window()
           .Name("inPersonWindow")
           .Title("Выдача рабонику")
           .Width(1200)
           .Height(650)
           .Draggable(true)
           .Modal(true)
           .Visible(false)
           .Buttons(b => b.Close())
           .Content(() =>
           {%>
           <table width="100%">
           <tr>
           <td colspan="2">
           <b>Кому отдаём:</b><br/>
            <%: Html.Telerik().ComboBox()
                     .Name("WorkerWorkPlaceCombo")
                     .AutoFill(false)
                     .DataBinding(binding => binding.Ajax()
                     .Select("_FindActiveWorkerWorkPlace", "MatPersonStore")
                        .Delay(400)
                        .Cache(false)
                        )
                    .HtmlAttributes(new { @style = "width:800px" })
                    .Filterable(filtering =>
                        filtering
                        .FilterMode(AutoCompleteFilterMode.Contains)
                        .MinimumChars(1)
                    )
                    .ClientEvents(events => events
                        .OnClose("workerWorkPlaceComboChange")
                    )
                    .SelectedIndex(0)
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
            %>
            </td>
            </tr>
            <tr>
            <td>
               Дата:
               <%: Html.Telerik().DatePicker()
                      .Name("inPersonDocDate")
                      .Value(DateTime.Now.ToShortDateString())
                      .Format(Store.Data.DataGlobals.DATE_FORMAT_FULL_YEAR)
                %>
                </td>
                <td align="right">
                  <%: Html.Button("reportM11", "Печать требования и Личной карточки", HtmlButtonType.Button, "porintInPersonM11();", new { @class = "t-button"}) %>
                </td>
            </tr>
</table>

<%
    Html.Telerik().Grid<WorkerWorkplace>()
         .Name("WorkerWorkplace")
         .DataBinding(dataBinding =>
         {
             dataBinding
               .Ajax()
               .Select("Select_Worker", "MatPersonStore");
         })
         .Columns(columns =>
         {
             columns.Bound(x => x)
               .ClientTemplate("<#= Worker.isActive == false?\"<font color='red'><b>УВОЛЕН</b></font><br/>\":\"\" #>"
                 + "<#= Worker.IsTabu == true?\"<font color='red'><b>Выдача спецодежды на складе запрещена!</b></font><br/>\":\"\" #>"
                 + "№ основного склада: <font color='blue'><b><#=StorageNumber#></b></font><br/>"
                 +"Пол: <#= Worker.Sex != null?\"<b>\"+Worker.Sex.Name+\"</b>\":\"\" #>"
                 + "&nbsp;|&nbsp;<#= Worker.NomBodyPartSizes[0] != null?Worker.NomBodyPartSizes[0].NomBodyPart.Name+\": <b>\"+Worker.NomBodyPartSizes[0].SizeNumber+\"</b>\":\"\" #>"
                 + "&nbsp;|&nbsp;<#= Worker.NomBodyPartSizes[1] != null?Worker.NomBodyPartSizes[1].NomBodyPart.Name+\": <b>\"+Worker.NomBodyPartSizes[1].SizeNumber+\"</b>\":\"\" #>"
                 + "&nbsp;|&nbsp;<#= Worker.NomBodyPartSizes[2] != null?Worker.NomBodyPartSizes[2].NomBodyPart.Name+\": <b>\"+Worker.NomBodyPartSizes[2].SizeNumber+\"</b>\":\"\" #>"
                 + "&nbsp;|&nbsp;<#= Worker.NomBodyPartSizes[3] != null?Worker.NomBodyPartSizes[3].NomBodyPart.Name+\": <b>\"+Worker.NomBodyPartSizes[3].SizeNumber+\"</b>\":\"\" #>"
                 + "&nbsp;|&nbsp;Категория: <#= Worker.WorkerCategory != null?\"<b>\"+Worker.WorkerCategory.Name+\"</b>\":\"---\" #>"
                 + "&nbsp;|&nbsp;Группа: <#= Worker.WorkerGroup != null?\"<b>\"+Worker.WorkerGroup.Name+\"</b>\":\"---\" #>"
                 + "</br>Норма: <#= Organization.NormaOrganization != null?\"<b>\"+Organization.NormaOrganization.Norma.Name+\"</b>\":\"\" #>");
             columns.Bound(x => x.StorageId).Hidden();
         })
         .HtmlAttributes(new { @class = "attachment-grid" })
         .ClientEvents(events => events
             .OnLoad("HideHeader")
             .OnDataBinding("dataBindingInPerson")
             .OnError("onGridErrorEvent"))
         .Footer(false)
         .Render();
%>

<%
   Html.Telerik().Grid<WorkerNorma>()
        .Name("inPersonGrid")
        .ToolBar(commands =>
                {
                    commands.Insert().ButtonType(GridButtonType.Image);
                })
        .DataKeys(keys =>
        {
            keys.Add(x => x.Id);
        })
        .DataBinding(dataBinding =>
        {
            dataBinding
              .Ajax()
              .Select("_SelectInPersonGrid", "MatPersonStore")
              .Update("_UpdateStorageMolData", "MatPersonStore");
        })
        .Columns(columns =>
        {
            columns.Bound(x => x.NormaContentName)
             .Width(230)
             .Title("Наименование групп номенклатур")
            .ReadOnly();
            columns.Bound(x => x.NormaQuantity)
                .Width(70)
                .Title("По&nbsp;норме")
                //.HtmlAttributes(new {style = "word-wrap:normal"})
                .ReadOnly();
            columns.Bound(x => x.NormaUsePeriod)
                .Width(60)
                .Title("Период")
                .ReadOnly();
            columns.Bound(x => x.StorageNumber).Width(80).Title("Склад").ReadOnly();
            columns.Bound(x => x.StorageInfo)
              .EditorTemplateName("StorageNomenclatureTemplate")
              .Encoded(false)
                //.ClientTemplate("<#= StorageInfo #>")
              .Title("[Код SAP] Номенклатура (размер, рост, износ, кол-во)");
            //.Width(400);
            columns.Bound(x => x.PutQuantity)
                .Width(60)
                .Title("Выдать");
            columns.Bound(x => x.PresentQuantity)
                .Width(70)
                .Title("На&nbsp;руках")
                .ReadOnly();
            columns.Bound(x => x.ReceptionDate)
                .Format("{0:" + DataGlobals.DATE_FORMAT + "}")
                .Width(70)
                .Title("Выдано")
                .ReadOnly();
            columns.Bound(x => x.DocNumber)
                //.ClientTemplate("<a href=\"javascript:printM11(<#= DocNumber #>, '<#= ReceptionDateAsString #>','" + ((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName + "','" + (HttpContext.Current.Session["_idOrg"].ToString() == DataGlobals.ORG_ID_VGOK) + "');\"><#= DocNumber != 0 ? DocNumber:\"\" #></a>")
                .ClientTemplate("<a href=\"javascript:printM11(<#= DocNumber #>, '<#= ReceptionDateAsString #>',<#= OperTypeId #>,'" + ((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName + "','" + (HttpContext.Current.Session["_idOrg"].ToString() == DataGlobals.ORG_ID_VGOK) + "');\"><#= DocNumber != 0?DocNumber:\"\" #></a>")

                .Width(70)
                .Title("№&nbsp;M11")
                .ReadOnly();
            columns.Bound(x => x.NormaContentId).Hidden();
            columns.Bound(x => x.StorageId).Hidden();
            columns.Bound(x => x.Wear).Hidden();
        })
        .ClientEvents(events => events
            .OnEdit("onEditInPerson")
            .OnError("onError")
            .OnSave("onSaveInPerson")
            .OnDataBinding("dataBindingInPerson"))
        .Editable(editing => editing.Mode(GridEditMode.InCell))
        .Scrollable(x => x.Height(340))
        .Render();
%>
<br/>
            <div align="center">
               <table border="0" cellpadding="0" cellspacing="0">
               <tr>
                <td>
                    <input type="button" value="Отмена" class="t-button" onclick="closeWindowClick('#inPersonWindow')"/>
                    &nbsp;&nbsp;
                </td>
                <td>
                    &nbsp;&nbsp;
                    <input type="button" value="Сохранить" id="inPersonSaveButton" class="t-button" onclick="disabled=true ; inPersonSaveClick('inPerson')"/>
                </td>
               </tr>
               </table>
               </div>
           <%})
             .Render();           
    %>



    <% 
// Окно для возврата от рабочих
        Html.Telerik().Window()
           .Name("outPersonWindow")
           .Title("Возврат от работника")
           .Width(990)
           .Height(650)
           .Draggable(true)
           .Modal(true)
           .Visible(false)
           .Buttons(b => b.Close())
           .Content(() =>
           {%>
           <table width="100%">
           <tr>
           <td colspan=2>
           <b>От кого возвращаем:</b>
            <%: Html.Telerik().ComboBox()
                    .Name("WorkerWorkPlaceCombo1")
                    .AutoFill(false)
                    .DataBinding(binding => binding.Ajax()
                    .Select("_FindWorkerWorkPlace", "MatPersonStore")
                        .Delay(300)
                        .Cache(false)
                     )
                    .HtmlAttributes(new { @style = "width:800px" })
                    .Filterable(filtering =>
                        filtering
                        .FilterMode(AutoCompleteFilterMode.Contains)
                        .MinimumChars(1)
                    )
                    .ClientEvents(events => events
                        .OnChange("onWorkerWorkPlaceChange")
                    )
                    .SelectedIndex(0)
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
            %>
            </td>
            </tr>
            <tr>
            <td align="left">
            Дата:
               <%: Html.Telerik().DatePicker()
                      .Name("outPersonDocDate")
                      .Value(DateTime.Now.ToShortDateString())
                      .Format(Store.Data.DataGlobals.DATE_FORMAT_FULL_YEAR)
                %>
           </td>
           <td align="right">
                            <%= Html.Button("reportM4", "Печать М4 и Личной карточки", HtmlButtonType.Button, "printOutPersonM4()", new { @class = "t-button" }) %>
            </td>
            </tr>
            </table>
             <%:             
             Html.Telerik().Grid((IEnumerable<Store.Core.MatPersonOnHandsSimple>)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE])
                .Name("outPersonGrid")
                .DataKeys(keys =>
                {
                  keys.Add(x => x.Id);
                })
                .ToolBar(commands =>
                {
                   commands.Insert().ButtonType(GridButtonType.Image);
                })                
                .Footer(false)
                .Editable(editing => editing.Mode(GridEditMode.InCell))
                .Columns(columns =>
                {
                    columns.Bound(c => c.ExternalCode).ReadOnly(true).Width(100);
                    columns.Bound(c => c.Nomenclature).ReadOnly(true);
                    columns.Bound(c => c.Quantity).ReadOnly(true).Title("Кол-во").Width(100);
                    columns.Bound(c => c.Wear).Title("Износ")
                        .ClientTemplate("<#= Wear=='100'?'новая':Wear=='50'?'б/у':Wear=='0'?'утиль':Wear #>")
                        .EditorTemplateName("WearTemplate")
                        .Width(100);
                    columns.Bound(c => c.WorkQuantity).Title("Вернуть").Width(100);
                    columns.Bound(c => c.NomenclatureId).Hidden(true);                                   
                })
                .ClientEvents(events => events
                   .OnError("onError")
                   .OnDataBinding("onOutPersonGridDataBinding")
                )
                .DataBinding(dataBinding => dataBinding.Ajax()
                       .Update("_UpdateStorageMolData", "MatPersonStore")
                       .Select("_SelectNomenclatureOnHands", "MatPersonStore")
                )
                .Scrollable(x => x.Height(500))
             %>
            <div align="center">
               <table border="0" cellpadding="0" cellspacing="0">
               <tr>
                <td>
                    <input type="button" value="Отмена" class="t-button" onclick="closeWindowClick('#outPersonWindow')"/>
                    &nbsp;&nbsp;
                </td>
                <td>
                    &nbsp;&nbsp;
                    <input type="button" value="Сохранить" id="outPersonSaveButton" class="t-button" onclick="disabled=true ; outPersonSaveClick('outPerson')"/>
                </td>
               </tr>
               </table>
               </div>
           <%})
           .Render();
    %>

    <% 
// Списание с МОЛ
        Html.Telerik().Window()
           .Name("outMolWindow")
           .Title("Списание с МОЛ")
           .Width(990)
           .Height(650)
           .Draggable(true)
           .Modal(true)
           .Visible(false)
           .Buttons(b => b.Close())
           .Content(() =>
           {%>
           <table width=100%>
           <tr>
           <td>
           <b>Причина:</b>
           </td>
           <td colspan=2>
            <%:
            Html.Telerik().DropDownList()
                   .Name("СauseDownList")
                   .BindTo(new SelectList((IEnumerable)ViewData[DataGlobals.REFERENCE_MOTIV], "Id", "Name"))
                   .HtmlAttributes(new { style = "width: 400px" })
             %>
             </td>
             </tr>
             <tr>
             <td align=left>
               Дата:
             </td>
             <td>
               <%: Html.Telerik().DatePicker()
                      .Name("outMolDocDate")
                      .Value(DateTime.Now.ToShortDateString())
                      .Format(Store.Data.DataGlobals.DATE_FORMAT_FULL_YEAR)
                %>
                </td>
                <td align=right>
                <%
               if (HttpContext.Current.Session["_idOrg"].ToString() == DataGlobals.ORG_ID_EVRAZRUDA)
               {
                %>
                
                    <%=
                        Html.Button("reportMB8", "Печать МБ-8", HtmlButtonType.Button,
                                                 "window.open('" + getReportUrl("Акт на списание MБ-8 Евразруда МОЛ") + "&RptOperTypeID=" + DataGlobals.OPERATION_MOL_OUT + "&RptWorkerWorkplaceId='+getCurrentMatPersonId()+'" + "&RptParamStorage='+getCurrentMatPersonStorageNameId()" + ");",
                                                  new { @class = "t-button" })
                    %>
                <% } else { %>
                    <%=
                                           Html.Button("reportMB8", "Печать МБ-8", HtmlButtonType.Button,
                                    "window.open('" + getReportUrl("Акт на списание MБ-8") + "&RptOperTypeID=" + DataGlobals.OPERATION_MOL_OUT + "&RptWorkerWorkplaceId='+getCurrentMatPersonId()+'" + "&RptParamStorage='+getCurrentMatPersonStorageNameId()" + ");",
                                    new { @class = "t-button"})


                    %>
                <%} %>


                </td>
                </tr>
                </table>
             <%:             
             Html.Telerik().Grid((IEnumerable<Store.Core.MatPersonOnHandsSimple>)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE])
                .Name("outMolGrid")
                .DataKeys(keys =>
                {
                  keys.Add(x => x.Id);
                })
                .ToolBar(commands =>
                {
                   commands.Insert().ButtonType(GridButtonType.Image);
                })                
                .Footer(false)
                .Editable(editing => editing.Mode(GridEditMode.InCell))
                .Columns(columns =>
                {
                    columns.Bound(c => c.ExternalCode).ReadOnly(true).Width(100);
                    columns.Bound(c => c.Nomenclature).ReadOnly(true);
                    columns.Bound(c => c.Quantity).ReadOnly(true).Title("Кол-во").Width(100);
                    columns.Bound(c => c.Wear).ReadOnly(true).ClientTemplate("<#= Wear=='100'?'новая':Wear=='50'?'б/у':Wear=='0'?'утиль':Wear #>").Title("Износ").Width(100);
                    columns.Bound(c => c.WorkQuantity).Title("Списать").Width(100);
                    columns.Bound(c => c.NomenclatureId).Hidden(true);                                   
                })
                .ClientEvents(events => events
                   .OnError("onError")
                )
                .DataBinding(dataBinding => dataBinding.Ajax()
                       .Update("_UpdateStorageMolData", "MatPersonStore")
                       .Select("_SelectEmptyRow", "Storages")
                )
                .Scrollable(x => x.Height(500))
             %>
            <div align="center">
               <table border="0" cellpadding="0" cellspacing="0">
               <tr>
                <td>
                    <input type="button" value="Отмена" class="t-button" onclick="closeWindowClick('#outMolWindow')"/>
                    &nbsp;&nbsp;
                </td>
                <td>
                    &nbsp;&nbsp;
                    <input type="button" value="Сохранить" id="outMolSaveButton" class="t-button" onclick="disabled=true; outMolSaveClick('outMol')"/>
                </td>
               </tr>
               </table>
               </div>
           <%})
           .Render();
    %>


    <% 
// Перевод между МОЛ
        Html.Telerik().Window()
           .Name("moveMolWindow")
           .Title("Перевод между МОЛ")
           .Width(990)
           .Height(650)
           .Draggable(true)
           .Modal(true)
           .Visible(false)
           .Buttons(b => b.Close())
           .Content(() =>
           {%>
               <table>
               <tr>
               <td>
               
           <b>МОЛ:</b>
            <%= Html.Telerik().ComboBox()
                     .Name("MatPersonCombo1")
                     .AutoFill(false)
                     .DataBinding(binding => binding.Ajax()
                        .Select("_FindMatPerson", "MatPersonStore")
                        .Delay(400)
                        .Cache(false)
                        )
                    .HtmlAttributes(new { @style = "width:750px" })
                    .Filterable(filtering =>
                        filtering
                        .FilterMode(AutoCompleteFilterMode.Contains)
                        .MinimumChars(1)
                    )
                    .SelectedIndex(0)
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
            %>

               </td>
               </tr>
               <tr>
               <td>
               Дата:
               <%: Html.Telerik().DatePicker()
                      .Name("moveMolDocDate")
                      .Value(DateTime.Now.ToShortDateString())
                      .Format(Store.Data.DataGlobals.DATE_FORMAT_FULL_YEAR)
                %>
                </td>
                <td align=right>
                    <%=Html.Button("reportM11", "Печать М11", HtmlButtonType.Button, "printMoveMolM11();", new { @class = "t-button"}) %>
                </td>
                </tr>
                </table>
             <%:             
             Html.Telerik().Grid((IEnumerable<Store.Core.MatPersonOnHandsSimple>)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE])
                .Name("moveMolGrid")
                .DataKeys(keys =>
                {
                  keys.Add(x => x.Id);
                })
                .ToolBar(commands =>
                {
                   commands.Insert().ButtonType(GridButtonType.Image);
                })                
                .Footer(false)
                .Editable(editing => editing.Mode(GridEditMode.InCell))
                .Columns(columns =>
                {
                    columns.Bound(c => c.ExternalCode).ReadOnly(true).Width(100);
                    columns.Bound(c => c.Nomenclature).ReadOnly(true);
                    columns.Bound(c => c.Quantity).ReadOnly(true).Title("Кол-во").Width(100);
                    columns.Bound(c => c.Wear).ReadOnly(true).ClientTemplate("<#= Wear=='100'?'новая':Wear=='50'?'б/у':Wear=='0'?'утиль':Wear #>").Title("Износ").Width(100);
                    columns.Bound(c => c.WorkQuantity).Title("Перевод").Width(100);
                    columns.Bound(c => c.NomenclatureId).Hidden(true);                                   
                })
                .ClientEvents(events => events
                   .OnError("onError")
                )
                .DataBinding(dataBinding => dataBinding.Ajax()
                       .Update("_UpdateStorageMolData", "MatPersonStore")
                       .Select("_SelectEmptyRow", "Storages")
                )
                .Scrollable(x => x.Height(500))
             %>
            <div align="center">
               <table border="0" cellpadding="0" cellspacing="0">
               <tr>
                <td>
                    <input type="button" value="Отмена" class="t-button" onclick="closeWindowClick('#moveMolWindow')"/>
                    &nbsp;&nbsp;
                </td>
                <td>
                    &nbsp;&nbsp;
                    <input type="button" value="Сохранить" id="moveMolSaveButton" class="t-button" onclick="disabled=true; moveMolSaveClick('moveMol')"/>
                </td>
               </tr>
               </table>
               </div>
           <%})
           .Render();
    %>

<% 
    Html.Telerik().Grid<MatPersonOnHands>()
        .Name("MatPerson")     
        .DataKeys(keys =>
        {
            keys.Add(x => x.Id);
        })
        .DataBinding(dataBinding => dataBinding
            .Ajax()
            .Select("_SelectOnHands", "MatPersonStore")
        )
        .Columns(columns =>
        {
          columns.Bound(x => x.NomenclatureInfo).Title("Номенклатура").EditorTemplateName("StorageNomTemplate");
          columns.Bound(x => x.Wear).ClientTemplate("<#= Wear=='100'?'новая':Wear=='50'?'б/у':Wear=='0'?'утиль':Wear #>").Width(100);
          columns.Bound(x => x.Quantity).Title("Кол-во").Width(100);
          columns.Bound(x => x.LastDocNumber)
              .ClientTemplate("<a href=\"javascript:printM11(<#= LastDocNumber #>, '<#= LastDocDateAsString #>',<#= LastOperTypeId #>, '" + ((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName + "','" + (HttpContext.Current.Session["_idOrg"].ToString() == DataGlobals.ORG_ID_VGOK) + "');\"><#= LastDocNumber!=0? LastDocNumber:\"\" #></a>") 

              .Width(70)
              .Title("№&nbsp;док.")
              .ReadOnly();
            
          columns.Bound(x => x.Nomenclature.Id).Hidden(true);
          columns.Bound(x => x.LastOperTypeId).Hidden(true);  
        })
        .ClientEvents(events => events
            .OnDataBinding("dataBinding")
            .OnDataBound("dataBound")
            .OnError("onGridErrorEvent")
        )
        .Scrollable(x => x.Height(400))
        .Render();
%>
<%if (!(Boolean)ViewData["isMOL"]) {%>
<script type="text/javascript">
    function getCurrentMatPersonId() {
        var value = $('#MatPersonCombo').data('tComboBox').value();
        var splitValue = value.split("|");
        return splitValue[0];
    }
    function getCurrentMatPersonId1() {
        var value = $('#MatPersonCombo1').data('tComboBox').value();
        var splitValue = value.split("|");
        return splitValue[0];
    }


    function getCurrentMatPersonStorageNameId() {
        var value = $('#MatPersonCombo').data('tComboBox').value();
        var splitValue = value.split("|");
        return splitValue[1];
    }

</script>
<%} else { %>
<script type="text/javascript">
    function getCurrentMatPersonId() {
        return '<%=ViewData["matPersonId"]%>';
    }

    function getCurrentMatPersonStorageNameId() {
        return '<%=ViewData["matPersonStorageNameId"]%>';
    }
</script>  
<% } %>

<script type="text/javascript">
//Режим поиска
    var findMode = 0;
    function dataBound() {
        var data = getMolOnHandData();
        var grid = $('#outMolGrid').data('tGrid');
        grid.dataBind(data);
        
        grid = $('#inStorageGrid').data('tGrid');
        grid.dataBind(data);

        grid = $('#moveMolGrid').data('tGrid');
        grid.dataBind(data);

    }

    function updateGridContext() {
        var dropDownList = $("#MatPersonCombo").data("tComboBox");
        var text = dropDownList.text();
        var splitValue = text.split(")");

        $('#MvzName').text(splitValue[1]);
        $("#MatPerson").data("tGrid").rebind(); 
    }

    function clearGridContext() {
        workerWorkPlaceId = "-1";
        var grid = $("#MatPerson").data("tGrid");
        var data = new Array();
        grid.dataBind(data);
    }

    function dataBinding(args) {
        var dropDownList = $("#MatPersonCombo").data("tComboBox");
        if (dropDownList) {
            workerWorkPlaceId = getCurrentMatPersonId();
            if (workerWorkPlaceId == dropDownList.text()) {
                workerWorkPlaceId = "";
            }
        }
        if (workerWorkPlaceId == "") {
            workerWorkPlaceId = "-1";
        }
        args.data = $.extend(args.data, { workerWorkPlaceId: workerWorkPlaceId });
    }

    function showStornoDocumParam() {
        var window = $("#Window").data("tWindow");
        window.center().open();
    }

    function closeWindowClick(winName) {
        var window = $(winName).data("tWindow");
        window.close();
    }

    window.onresize = onFormResize;
    function onFormResize() {
        $("#MatPersonCombo").data("tComboBox").disable();
        $("#MatPersonCombo").data("tComboBox").enable();
    }

    //Удаляем запись из табл. для ручного ввода выдачи со склада
    function delRow(obj) {
        var grid = $('#inStorageGrid').data('tGrid');
        var tr = $(obj).closest('tr');
        grid.deleteRow(tr);
        emptyRow = $("#inStorageGrid .t-no-data");
        if ($("#inStorageGrid tr").length > 2) {
            emptyRow[0].style.display = "none";
        } else {
            emptyRow[0].style.display = "block";
        }
        return false;
    }

    function outStorageClick(mode) {
        var window = $("#outStorageWindow").data("tWindow");
        if (workerWorkPlaceId == "-1") {
            alert("МОЛ не выбран!");
            return false;
        }
        findMode = 0;
        var grid = $('#outStorageGrid').data('tGrid');
        var data = new Array();
        grid.dataBind(data);
        window.center().open();
    }

    function inStorageClick() {
        var window = $("#inStorageWindow").data("tWindow");
        if (workerWorkPlaceId == "-1") {
            alert("МОЛ не выбран!");
            return false;
        }
        findMode = 1;
        //var grid = $('#inStorageGrid').data('tGrid');
        //grid.dataBind(getMolOnHandData());
        hideInsertGridIcon("inStorageGrid");
        window.center().open();
    }

    function getMolOnHandData() {
        var gridSource = $('#MatPerson').data('tGrid');
        var rowsData = gridSource.data;
        var data = new Array();
        for (i = 0; i < rowsData.length; i++) {
            var row = rowsData[i];
            var o = new Object()
            o["Id"] = i;
            o["Nomenclature"] = row.Nomenclature.Name;
            o["ExternalCode"] = row.Nomenclature.ExternalCode;
            o["NomenclatureId"] = row.Nomenclature.Id;
            o["Wear"] = row.Wear;
            o["Quantity"] = row.Quantity;
            o["WorkQuantity"] = 0;
            data[data.length] = o;
        }
        return data;
    }

    function inPersonClick() {
        if (workerWorkPlaceId == "-1") {
            alert("МОЛ не выбран!");
            return false;
        }
        findMode = 4;

        var window = $("#inPersonWindow").data("tWindow");
        var grid = $('#inPersonGrid').data('tGrid');
        var dropDownListWin = $("#WorkerWorkPlaceCombo").data("tComboBox");
        if (dropDownListWin) {
            dropDownListWin.value("");
            dropDownListWin.text("");
        }

        //        grid.rebind();
        workerWorkPlaceComboChange();
        hideInsertGridIcon("inPersonGrid");
        window.center().open();
        document.getElementById("WorkerWorkPlaceCombo-input").focus();
    }

    function outPersonClick() {
        if (workerWorkPlaceId == "-1") {
            alert("МОЛ не выбран!");
            return false;
        }
        findMode = 3;
        var window = $("#outPersonWindow").data("tWindow");
        var grid = $('#outPersonGrid').data('tGrid');
        var data = new Array();
        grid.dataBind(data);
        hideInsertGridIcon("outPersonGrid");
        var dropDownListWin = $("#WorkerWorkPlaceCombo1").data("tComboBox");
        if (dropDownListWin) {
            dropDownListWin.value("");
            dropDownListWin.text("");
        }
        window.center().open();
        document.getElementById("WorkerWorkPlaceCombo1-input").focus();
    }

    function moveMolClick() {
        if (workerWorkPlaceId == "-1") {
            alert("МОЛ не выбран!");
            return false;
        }
        var window = $("#moveMolWindow").data("tWindow");
//        var grid = $('#moveMolGrid').data('tGrid');
//        grid.dataBind(getMolOnHandData());
        hideInsertGridIcon("moveMolGrid");
        var dropDownListWin = $("#MatPersonCombo1").data("tComboBox");
        if (dropDownListWin) {
            dropDownListWin.value("");
            dropDownListWin.text("");
        }
        window.center().open();
        document.getElementById("MatPersonCombo1-input").focus();
    }

    function outMolClick() {
        if (workerWorkPlaceId == "-1") {
            alert("МОЛ не выбран!");
            return false;
        }
        var window = $("#outMolWindow").data("tWindow");
        //var grid = $('#outMolGrid').data('tGrid');
        //grid.dataBind(getMolOnHandData());
        hideInsertGridIcon("outMolGrid");
        window.center().open();
    }
    
    function onEditOutStorage(e) {
        var dataItem = e.dataItem;
        var mode = e.mode;
        var form = e.form;
//        currentNomBodyPartId = dataItem["NomBodyPartId"];
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

    function onSaveOutStorage(e) {
        var curRow = $(e.cell).closest('tr');
        var cells = $(curRow[0]).find('td');
        var $combo = $(e.cell).find('#Nomenclature');
        if ($combo.length > 0) {
            var combo = $combo.data("tComboBox"),
                    selectItem = combo.selectedIndex > -1 ? combo.data[combo.selectedIndex] : null;

            if (selectItem) {
                e.values.Nomenclature = selectItem.Text;                
                cells[cells.length - 1].innerHTML = selectItem.Value;
            }
        }
    }

    function onStorageComboBoxDataBinding(args) {
        if (workerWorkPlaceId == "-1") {
            alert("МОЛ не выбран!");
            return false; 
        }

        args.data = $.extend(args.data, { mode: findMode, idMol: workerWorkPlaceId, normaContentId: normaContentId });
    }

    function onWorkerWorkPlaceChange(e) {
        $('#outPersonGrid').data('tGrid').rebind();
    }

    function hideInsertGridIcon(gridName) {
        var gridButtons = $("#" + gridName + " .t-grid-add");
        if (gridButtons.length > 0) {
            gridButtons[0].style.display = "none";
        }
    }

    function onOutPersonGridDataBinding(args) {
        var dropDownList = $("#WorkerWorkPlaceCombo1").data("tComboBox");
        var wwpId = "";
        if (dropDownList) {
            wwp = dropDownList.value();
        }
        if (wwp == "") {
            wwp = "-1";
        }
        args.data = $.extend(args.data, { workerWorkPlaceId: wwp });
    }

//Сохранение данных

    function getCellIndexByName(cells,cellName) {
        for (_curCell = 0; _curCell < cells.length; _curCell++) {
            if (cells[_curCell].member.toUpperCase()==cellName.toUpperCase()){
                return _curCell;
            }
        }
        return 0;
    }

    function formatDataGridCell(val) {
        var value = val;
        if (value) {
            if (value.indexOf("</SPAN>") > 0) {
                value = value.substring(value.indexOf("</SPAN>") + 7);
            }
        }
        return value;
    }

    function processResponse(result, winName,grid) {
        if (result.modelState) {
            var message = "Ошибки:\n";
            $.each(result.modelState, function (key, value) {
                if ('errors' in value) {
                    $.each(value.errors, function () {
                        message += this + "\n";
                    });
                }
            });
            alert(message);
        } else {
            if (winName == "#inPersonWindow" || winName == "#outPersonWindow") 
                grid.rebind();
            updateGridContext();
            if (winName == "#outStorageWindow") {
                closeWindowClick(winName);
            }
        }
    }

    function processResponseCloth(result, winName) {
        if (result.modelState) {
            var message = "Ошибки:\n";
            $.each(result.modelState, function (key, value) {
                if ('errors' in value) {
                    $.each(value.errors, function () {
                        message += this + "\n";
                    });
                }
            });
            alert(message);
        } else {
            updateGridContext();
            closeWindowClick(winName);
        }
    }

//Выдача со склада МОЛ
    function outStorageSaveClick() {
        var grid = $('#outStorageGrid').data('tGrid');
        var rowsData = grid.data;
        var rows = grid.$rows();
        if (workerWorkPlaceId == "-1") {
            alert("МОЛ не выбран!");
            return false;
        }
        //Дата 
        var docDate = document.getElementById("outStorageDocDate").value;
        if (docDate == "") {
            alert("Дата должна быть задана!");
            return false;
        }

        var postData = "toMatPersonId=" + workerWorkPlaceId + "&docDate=" + docDate;
        for (i = 0; i < rows.length; i++) {
            var row = rows[i];
            if (row.cells.length > 1) {
                postData = postData + "&updated[" + i + "].Id=" + i;
                postData = postData + "&updated[" + i + "].NomenclatureId=" + formatDataGridCell(row.cells[getCellIndexByName(grid.columns, "NomenclatureId")].innerHTML);
                postData = postData + "&updated[" + i + "].Nomenclature=" + formatDataGridCell(row.cells[getCellIndexByName(grid.columns, "Nomenclature")].innerHTML);
                var WorkQuantity = formatDataGridCell(row.cells[getCellIndexByName(grid.columns, "WorkQuantity")].innerHTML);
                if (WorkQuantity == "") WorkQuantity = 0;
                postData = postData + "&updated[" + i + "].WorkQuantity=" + WorkQuantity;
            }
        }
        $.ajax({
            url: 'MatPersonStore/_outStorageSave',
            contentType: 'application/x-www-form-urlencoded',
            type: 'POST',
            dataType: 'json',
            error: function (xhr, status) {
                onGridErrorEvent(xhr);
            },
            data: postData,
            success: function (result) {
                document.getElementById("outStorageSaveButton").disabled=false;
                processResponse(result, "#outStorageWindow",grid);
            },
            error: function (xhr, str) {
                 document.getElementById("outStorageSaveButton").disabled=false;
                onGridErrorEvent(xhr);
            }
        });
    }

    function outPersonSaveClick() {
        var grid = $('#outPersonGrid').data('tGrid');
        var rowsData = grid.data;
        var rows = grid.$rows();

        if (workerWorkPlaceId == "-1") {
            alert("МОЛ не выбран!");
            return false;
        }

        var dropDownListWin = $("#WorkerWorkPlaceCombo1").data("tComboBox");
        var workerWorkPlaceIdWin = "";
        if (dropDownListWin) {
            workerWorkPlaceIdWin = dropDownListWin.value();
        }
        if (workerWorkPlaceIdWin == "") {
            alert("Сотрудник не выбран!");
            return false;
        }

        //Дата 
        var docDate = document.getElementById("outPersonDocDate").value;
        if (docDate == "") {
            alert("Дата должна быть задана!");
            return false;
        }

        var postData = "workerWorkPlaceId=" + workerWorkPlaceIdWin + "&toMatPersonId=" + workerWorkPlaceId + "&docDate=" + docDate;
        for (i = 0; i < rows.length; i++) {
            var row = rows[i];
            postData = postData + "&updated[" + i + "].Id=" + i;
            postData = postData + "&updated[" + i + "].NomenclatureId=" + rowsData[i].NomenclatureId;
            postData = postData + "&updated[" + i + "].Nomenclature=" + rowsData[i].Nomenclature;
            postData = postData + "&updated[" + i + "].Wear=" + rowsData[i].Wear;
            var WorkQuantity = formatDataGridCell(row.cells[getCellIndexByName(grid.columns, "WorkQuantity")].innerHTML);
            if (WorkQuantity == "") WorkQuantity = 0;
            postData = postData + "&updated[" + i + "].WorkQuantity=" + WorkQuantity;
        }
     //   alert(postData);
     //   return true;
        $.ajax({
            url: 'MatPersonStore/_outPersonSave',
            contentType: 'application/x-www-form-urlencoded',
            type: 'POST',
            dataType: 'json',
            error: function (xhr, status) {
                document.getElementById("outPersonSaveButton").disabled=false;
                onGridErrorEvent(xhr);
            },
            data: postData,
            success: function (result) {
                processResponse(result, "#outPersonWindow",grid);
                document.getElementById("outPersonSaveButton").disabled=false;
            },
            error: function (xhr, str) {
                document.getElementById("outPersonSaveButton").disabled=false;
                onGridErrorEvent(xhr);
            }
        });
    }

    function inPersonSaveClick() {
        var grid = $('#inPersonGrid').data('tGrid');
        var rowsData = grid.data;
        var rows = grid.$rows();
        if (workerWorkPlaceId == "-1") {
            alert("МОЛ не выбран!");
            return false;
        }

        var dropDownListWin = $("#WorkerWorkPlaceCombo").data("tComboBox");
        var workerWorkPlaceIdWin = "";
        if (dropDownListWin) {
            workerWorkPlaceIdWin = dropDownListWin.value();
        }
        if (workerWorkPlaceIdWin == "") {
            alert("Сотрудник не выбран!");
            return false;
        }
        //Дата 
        var docDate = document.getElementById("inPersonDocDate").value;
        if (docDate == "") {
            alert("Дата должна быть задана!");
            return false;
        }


        var postData = "workerWorkPlaceId=" + workerWorkPlaceIdWin + "&fromMatPersonId=" + workerWorkPlaceId + "&docDate=" + docDate;
        for (i = 0; i < rows.length; i++) {
            var row = rows[i];
            postData = postData + "&updated[" + i + "].Id=" + i;
            postData = postData + "&updated[" + i + "].NormaContentName=" + rowsData[i].NormaContentName;
            postData = postData + "&updated[" + i + "].NormaQuantity=" + rowsData[i].NormaQuantity;
            postData = postData + "&updated[" + i + "].NormaUsePeriod=" + rowsData[i].NormaUsePeriod;
            postData = postData + "&updated[" + i + "].StorageNumber=" + rowsData[i].StorageNumber;
            postData = postData + "&updated[" + i + "].PresentQuantity=" + rowsData[i].PresentQuantity;
            postData = postData + "&updated[" + i + "].StorageInfo=" + formatDataGridCell(row.cells[getCellIndexByName(grid.columns, "StorageInfo")].innerHTML);
            postData = postData + "&updated[" + i + "].NormaContentId=" + formatDataGridCell(row.cells[getCellIndexByName(grid.columns, "NormaContentId")].innerHTML);
            postData = postData + "&updated[" + i + "].Wear=" + formatDataGridCell(row.cells[getCellIndexByName(grid.columns, "Wear")].innerHTML);
            postData = postData + "&updated[" + i + "].StorageId=" + formatDataGridCell(row.cells[getCellIndexByName(grid.columns, "StorageId")].innerHTML);
            var PutQuantity = formatDataGridCell(row.cells[getCellIndexByName(grid.columns, "PutQuantity")].innerHTML);
            if (PutQuantity == "") PutQuantity = 0;
            postData = postData + "&updated[" + i + "].PutQuantity=" + PutQuantity;
        }
       // alert(postData);
       // return true;
        $.ajax({
            url: 'MatPersonStore/_inPersonSave',
            contentType: 'application/x-www-form-urlencoded',
            type: 'POST',
            dataType: 'json',
            error: function (xhr, status) {
                document.getElementById("inPersonSaveButton").disabled=false;
                onGridErrorEvent(xhr);
            },
            data: postData,
            success: function (result) {
                processResponse(result, "#inPersonWindow", grid);
                document.getElementById("inPersonSaveButton").disabled=false;
            },
            error: function (xhr, str) {
                document.getElementById("inPersonSaveButton").disabled=false;
                onGridErrorEvent(xhr);
            }
        });
    }

    function moveMolSaveClick() {
        var grid = $('#moveMolGrid').data('tGrid');
        var dropDownListWin_o = $("#MatPersonCombo").data("tComboBox");
        var rowsData = grid.data;
        var rows = grid.$rows();

        if (workerWorkPlaceId == "-1") {
            alert("МОЛ не выбран!");
            return false;
        }

        var dropDownListWin = $("#MatPersonCombo1").data("tComboBox");
        if (dropDownListWin.value()==dropDownListWin_o.value()) {
            alert("Нельзя выполнить перевод самому себе! Измените МОЛ");
            return false;
        }
        var workerWorkPlaceIdWin = "";
        if (dropDownListWin) {
            var value = dropDownListWin.value();
            var splitValue = value.split("|");
            workerWorkPlaceIdWin = splitValue[0];
        }
        if (workerWorkPlaceIdWin == "") {
            alert("МОЛ - подучатель не выбран!");
            return false;
        }
        //Дата 
        var docDate = document.getElementById("moveMolDocDate").value;
        if (docDate == "") {
            alert("Дата должна быть задана!");
            return false;
        }


        var postData = "toMatPersonId=" + workerWorkPlaceIdWin + "&fromMatPersonId=" + workerWorkPlaceId + "&docDate=" + docDate;
        for (i = 0; i < rows.length; i++) {
            var row = rows[i];
            postData = postData + "&updated[" + i + "].Id=" + i;
            postData = postData + "&updated[" + i + "].NomenclatureId=" + rowsData[i].NomenclatureId;
            postData = postData + "&updated[" + i + "].Nomenclature=" + rowsData[i].Nomenclature;
            postData = postData + "&updated[" + i + "].Wear=" + rowsData[i].Wear;
            postData = postData + "&updated[" + i + "].Quantity=" + rowsData[i].Quantity;            
            var WorkQuantity = formatDataGridCell(row.cells[getCellIndexByName(grid.columns, "WorkQuantity")].innerHTML);
            if (WorkQuantity == "") WorkQuantity = 0;
            postData = postData + "&updated[" + i + "].WorkQuantity=" + WorkQuantity;
        }
        $.ajax({
            url: 'MatPersonStore/_moveMolSave',
            contentType: 'application/x-www-form-urlencoded',
            type: 'POST',
            dataType: 'json',
            error: function (xhr, status) {
                document.getElementById("moveMolSaveButton").disabled=false;
                onGridErrorEvent(xhr);
            },
            data: postData,
            success: function (result) {
                processResponse(result, "#moveMolWindow", grid);
                document.getElementById("moveMolSaveButton").disabled=false;
            },
            error: function (xhr, str) {
                document.getElementById("moveMolSaveButton").disabled=false;
                onGridErrorEvent(xhr);
            }
        });
    }

    function outMolSaveClick() {
        var grid = $('#outMolGrid').data('tGrid');
        var rowsData = grid.data;
        var rows = grid.$rows();
        if (workerWorkPlaceId == "-1") {
            alert("МОЛ не выбран!");
            return false;
        }
        //Дата 
        var docDate = document.getElementById("outMolDocDate").value;
        if (docDate == "") {
            alert("Дата должна быть задана!");
            return false;
        }

        var causeDropDownList = $("#СauseDownList").data("tDropDownList");
        if (causeDropDownList.value() == "") {
            alert("Причина списания не выбрана!");
            return false;
        }
        var postData = "cause=" + causeDropDownList.value() + "&fromMatPersonId=" + workerWorkPlaceId + "&docDate=" + docDate;
        for (i = 0; i < rows.length; i++) {
            var row = rows[i];
            postData = postData + "&updated[" + i + "].Id=" + i;
            postData = postData + "&updated[" + i + "].NomenclatureId=" + rowsData[i].NomenclatureId;
            postData = postData + "&updated[" + i + "].Nomenclature=" + rowsData[i].Nomenclature;
            postData = postData + "&updated[" + i + "].Quantity=" + rowsData[i].Quantity;
            postData = postData + "&updated[" + i + "].Wear=" + rowsData[i].Wear;
            var WorkQuantity = formatDataGridCell(row.cells[getCellIndexByName(grid.columns, "WorkQuantity")].innerHTML);
            if (WorkQuantity == "") WorkQuantity = 0;
            postData = postData + "&updated[" + i + "].WorkQuantity=" + WorkQuantity;
        }
        $.ajax({
            url: 'MatPersonStore/_outMolSave',
            contentType: 'application/x-www-form-urlencoded',
            type: 'POST',
            dataType: 'json',
            error: function (xhr, status) {
                document.getElementById("outMolSaveButton").disabled=false;
                onGridErrorEvent(xhr);
            },
            data: postData,
            success: function (result) {
               processResponse(result, "#outMolWindow", grid);
                document.getElementById("outMolSaveButton").disabled=false;

            }
        });
    }

    function inStorageSaveClick() {
        var grid = $('#inStorageGrid').data('tGrid');
        var rowsData = grid.data;
        var rows = grid.$rows();
        if (workerWorkPlaceId == "-1") {
            alert("МОЛ не выбран!");
            return false;
        }
        //Дата 
        var docDate = document.getElementById("inStorageDocDate").value;
        if (docDate == "") {
            alert("Дата должна быть задана!");
            return false;
        }

        var postData = "fromMatPersonId=" + workerWorkPlaceId + "&docDate=" + docDate;
        for (i = 0; i < rows.length; i++) {
            var row = rows[i];
            postData = postData + "&updated[" + i + "].Id=" + i;
            postData = postData + "&updated[" + i + "].NomenclatureId=" + rowsData[i].NomenclatureId;
            postData = postData + "&updated[" + i + "].Wear=" + rowsData[i].Wear;
            postData = postData + "&updated[" + i + "].Nomenclature=" + rowsData[i].Nomenclature;
            postData = postData + "&updated[" + i + "].Quantity=" + rowsData[i].Quantity;
            var WorkQuantity = formatDataGridCell(row.cells[getCellIndexByName(grid.columns, "WorkQuantity")].innerHTML);
            if (WorkQuantity == "") WorkQuantity = 0;
            postData = postData + "&updated[" + i + "].WorkQuantity=" + WorkQuantity;
        }
        $.ajax({
            url: 'MatPersonStore/_inStorageSave',
            contentType: 'application/x-www-form-urlencoded',
            type: 'POST',
            dataType: 'json',
            error: function (xhr, status) {
                document.getElementById("inStorageSaveButton").disabled=false;
                onGridErrorEvent(xhr);
            },
            data: postData,
            success: function (result) {
                processResponse(result, "#inStorageWindow", grid);
//                document.getElementById("inStorageSaveButton").disabled=false;
            },
            error: function (xhr, str) {
                document.getElementById("inStorageSaveButton").disabled=false;
                onGridErrorEvent(xhr);
            }
        });
                document.getElementById("inStorageSaveButton").disabled=false;

    }

    function workerWorkPlaceComboChange(e) {
        $('#inPersonGrid').data('tGrid').rebind();
        $('#WorkerWorkplace').data('tGrid').rebind();        
    }

    function dataBindingInPerson(args) {
        var dropDownList = $("#WorkerWorkPlaceCombo").data("tComboBox");
        if (dropDownList) {
            toWorkerWorkPlaceId = dropDownList.value();
            if (dropDownList.value() == dropDownList.text()) {
                toWorkerWorkPlaceId = "";
            }
        }
        if (toWorkerWorkPlaceId == "") {
            toWorkerWorkPlaceId = "-1";
        }
        args.data = $.extend(args.data, { workerWorkPlaceId: toWorkerWorkPlaceId, fromMatPersonId: workerWorkPlaceId });
        
    }

    var normaContentId = -1;
    function onEditInPerson(e) {
        normaContentId = e.dataItem['NormaContentId'];                    
    }

    function onSaveInPerson(e) {
        var curRow = $(e.cell).closest('tr');
        var cells = $(curRow[0]).find('td');
         var $combo = $(e.cell).find('#StorageInfo');
         if ($combo.length > 0) {
             var combo = $combo.data("tComboBox"), selectItem = combo.selectedIndex > -1 ? combo.data[combo.selectedIndex] : null;

             if (selectItem) {
                 var grid = $('#inPersonGrid').data('tGrid');
                 var splitName = selectItem.Value.split("|");
                 e.values.StorageId = splitName[0];
                 e.values.StorageInfo = selectItem.Text.replace("&lt;", "<").replace("&gt;", ">");
                 cells[getCellIndexByName(grid.columns, "StorageId")].innerHTML = splitName[0];
                 cells[getCellIndexByName(grid.columns, "Wear")].innerHTML = splitName[1];
             } else {
                 //              var value = combo.value();
                 //              e.values["StorageInfo"] = { Id: value, StorageInfo: value };
             }
         }
//         var workerWorkplaceId = $("#WorkerWorkplaceCombo").data("tComboBox").value();
//         e.data = $.extend(e.data, { workerWorkplaceId: workerWorkplaceId });
     }


/*
     function printM11(docNumber, docDate, UserName, isVgork) {
         if (isVgork == "True") {
             window.open('<%= getReportUrl("Требование на выдачу СИЗ") %>&RptUserName=' + escape(UserName) + ' &RptOperTypeID=<%= DataGlobals.OPERATION_WORKER_IN %>&DATEN=' + docDate + '&RptParamDocum=' + docNumber + '&RptWorkerWorkplaceId=' + $('#WorkerWorkPlaceCombo').data('tComboBox').value());
         } else {
             window.open('<%= getReportUrl("Накладная M11") %>&RptUserName=' + escape(UserName) + ' &RptOperTypeID=<%= DataGlobals.OPERATION_WORKER_IN %>&DATEN=' + docDate + '&RptParamDocum=' + docNumber + '&RptWorkerWorkplaceId=' + $('#WorkerWorkPlaceCombo').data('tComboBox').value());
         }
         window.open('<%= getReportUrl("Вкладыш в личную карточку") %>&RptOperTypeID=<%= DataGlobals.OPERATION_WORKER_IN %>&DATEN=' + docDate + '&RptParamDocum=' + docNumber + '&RptWorkerWorkplaceId=' + $('#WorkerWorkPlaceCombo').data('tComboBox').value());
     }
*/
     function printM11(docNumber, docDate, operTypeID, /*WorkerWorkPlaceId,*/ UserName, isVgork) {
         if (operTypeID==11) 
             if (isVgork == "True") {
                 window.open('<%= getReportUrl("Требование на выдачу СИЗ") %>&RptUserName=' + escape(UserName) + ' &RptOperTypeID=' + operTypeID + '&DATEN=' + docDate + '&RptParamDocum=' + docNumber );
             } else {
                 window.open('<%= getReportUrl("Накладная M11") %>&RptUserName=' + escape(UserName) + ' &RptOperTypeID=' + operTypeID + '&DATEN=' + docDate + '&RptParamDocum=' + docNumber );

             }
         else if (operTypeID==15)
                 window.open('<%= getReportUrl("Приходный ордер М-4") %>&RptUserName=' + escape(UserName) + ' &RptOperTypeID=' + operTypeID + '&DATEN=' + docDate + '&RptParamDocum=' + docNumber );
         else 
                 window.open('<%= getReportUrl("Накладная M11") %>&RptUserName=' + escape(UserName) + ' &RptOperTypeID=' + operTypeID + '&DATEN=' + docDate + '&RptParamDocum=' + docNumber );

     }

     function HideHeader() {
         $('.attachment-grid .t-header').hide();
     }
     document.getElementById("MatPersonCombo-input").focus();

//Печать документов
    function porintInPersonM11() {
    var comboBox = $('#WorkerWorkPlaceCombo').data('tComboBox');
    var comboBoxValue = comboBox.value();
    if (comboBox) {
        if (comboBoxValue == comboBox.text()) {
            comboBoxValue = "";
        }
    }
    if (comboBoxValue == "") {
        alert("Сотрудник не выбран!");
        return false;
    }


<%if (HttpContext.Current.Session["_idOrg"].ToString()==DataGlobals.ORG_ID_VGOK) { %>
    window.open('<%=getReportUrl("Требование на выдачу СИЗ")%>&RptOperTypeID=<%=DataGlobals.OPERATION_MOL_WORKER_IN%>&RptUserName=<%=Server.UrlEncode(((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName)%>&RptWorkerWorkplaceId='+$('#WorkerWorkPlaceCombo').data('tComboBox').value());
    window.open('<%=getReportUrl("Вкладыш в личную карточку")%>&RptOperTypeID=<%=DataGlobals.OPERATION_MOL_WORKER_IN%>&RptUserName=<%=Server.UrlEncode(((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName)%>&RptWorkerWorkplaceId=' + $('#WorkerWorkPlaceCombo').data('tComboBox').value());
<%}  else {%>
    window.open('<%=getReportUrl("Накладная M11")%>&RptOperTypeID=<%=DataGlobals.OPERATION_MOL_WORKER_IN%>&RptUserName=<%=Server.UrlEncode(((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName)%>&RptWorkerWorkplaceId='+$('#WorkerWorkPlaceCombo').data('tComboBox').value());
    window.open('<%=getReportUrl("Вкладыш в личную карточку")%>&RptOperTypeID=<%=DataGlobals.OPERATION_MOL_WORKER_IN%>&RptWorkerWorkplaceId='+$('#WorkerWorkPlaceCombo').data('tComboBox').value());
<%} %>
}

    function printOutPersonM4() {
        var comboBox = $('#WorkerWorkPlaceCombo1').data('tComboBox');
        var comboBoxValue = comboBox.value();
        if (comboBox) {
            if (comboBoxValue == comboBox.text()) {
                comboBoxValue = "";
            }
        }
        if (comboBoxValue == "") {
            alert("Сотрудник не выбран!");
            return false;
        }
        window.open('<%=getReportUrl("Приходный ордер М-4")%>&RptOperTypeID=<%=DataGlobals.OPERATION_MOL_WORKER_RETURN%>&RptUserName=<%=Server.UrlEncode(((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName)%>&RptWorkerWorkplaceId='+$('#WorkerWorkPlaceCombo1').data('tComboBox').value());
        window.open('<%=getReportUrl("Вкладыш в личную карточку")%>&RptOperTypeID=<%=DataGlobals.OPERATION_MOL_WORKER_RETURN%>&RptUserName=<%=Server.UrlEncode(((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName)%>&RptWorkerWorkplaceId='+$('#WorkerWorkPlaceCombo1').data('tComboBox').value());
    }

    function printMoveMolM11(){
        var comboBox = $('#MatPersonCombo1').data('tComboBox');
        var comboBoxValue = comboBox.value();
        if (comboBox) {
            if (comboBoxValue == comboBox.text()) {
                comboBoxValue = "";
            }
        }
        if (comboBoxValue == "") {
            alert("МОЛ не выбран!");
            return false;
        }
        window.open('<%=getReportUrl("Накладная M11")%>&RptOperTypeID=<%=DataGlobals.OPERATION_MOL_MOVE_IN%>&RptUserName=<%=Server.UrlEncode(((User)Session[DataGlobals.ACCOUNT_KEY]).UserInfo.FullName)%>&RptWorkerWorkplaceId='+getCurrentMatPersonId1());
     }
</script>
</asp:Content>
