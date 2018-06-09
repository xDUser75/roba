<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Store.Data" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<style>
.t-copy
{
    background-image:url("Content/Images/copy.gif");
}


</style>
<table width="100%">
  
<tr valign="top">
    <th width="30%" align="left">Нормы</th>
    <th width="70%" align="left"><font color="red"><span id='normaName'></span></font></th>
</tr>
<tr>            
    <td colspan="2">Выберите цех&nbsp;
        <%:
        Html.Telerik().ComboBox()
                .Name("Shops")
                .HtmlAttributes(new { style = "width: 400px" })
                .DataBinding(binding => binding.Ajax()
                                            .Select("_GetShops", "Normas")
                                            .Cache(false))
            .Filterable(filtering =>
                filtering
                .FilterMode(AutoCompleteFilterMode.Contains)
                .MinimumChars(3)
            )
            .Items(items =>
            {
                if (Session["shopNumber"] != null)
                    items.Add().Value("" + (string)Session["shopNumber"]).Text((string)Session["shopInfo"]);
            })
            .SelectedIndex(0)
            .HighlightFirstMatch(true)
            .OpenOnFocus(false)
            %>  
            <input type="button" value="Найти" class="t-button" onclick="findNorma()"/>                                
    </td>
</tr>

</table>

<%
	Html.Telerik().TabStrip()
            .Name("TabStrip")
            .Items(tabstrip =>
            {
                tabstrip.Add()
                    .Text("Нормы")
                    .Content(() =>
                    {

                        Html.Telerik().Grid<Store.Core.Norma>()
                               .Name("Normas")

                                .ToolBar(commands =>
                                    {
                                        if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                                            HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_EDIT) ||
                                            HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_APPROVED))
                                        {

                                        commands.Insert().ButtonType(GridButtonType.Image).HtmlAttributes(new { title = "Добавить" });
                                        }  

                                    }

                                    )
                                .DataKeys(keys =>
                                {
                                    keys.Add(x => x.Id);
                                })
                                .Columns(columns =>
                                {
                                columns.Command(commands =>
                                {
                                    if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                                        HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_EDIT) ||
                                        HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_APPROVED))
                                    {
                                        commands.Edit().ButtonType(GridButtonType.Image).HtmlAttributes(new { title = "Изменить", style = "DISPLAY:inline" });
                                        commands.Delete().ButtonType(GridButtonType.Image).HtmlAttributes(new { title = "Удалить", style = "DISPLAY:inline" });
                                    }
                                })
                                .Width(90);
                                    
                                    
                                columns.Bound(c => c).ClientTemplate("<img src=Content/Images/document_certificate.gif  <#= (IsApproved==true?\"style =display:block\":\"style = display:none\")  #> />")
                                    .Title("Утв.")                                        
                                    .Width(40)
                                    .ReadOnly()
                                    .Filterable(false)
                                    .Sortable(false);
                                    columns.Bound(x => x.Name).Title("Наименование нормы");

//                                columns.Bound(x => x.IsActive).Width("50px")
//                                        .Filterable(false)
//                                        .ClientTemplate("<input type='checkbox' disabled='true'  name='isactive' <#= (IsActive==null || IsActive)?'checked':'' #> />");
                                columns.Bound(x => x.Id)
                                        .Width("50px")
                                        .Filterable(false)
                                        .Title("Копировать")
                                        .ClientTemplate("<a title='Копировать'  class='t-button t-button-icon' " + 
                                        ((HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) || HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_EDIT))?" href='javascript:copyButtonClick(\"<#= Id #>\")'":"")+
                                        "><span class='t-icon t-copy'></span></a>");
                                columns.Bound(x => x.IsActive).Hidden();
                                columns.Bound(x => x.Id).Hidden();
                                columns.Bound(x => x.IsApproved).Hidden();
                                columns.Bound(x => x.Organization).Hidden();
                                                                            })
                                
                                .DataBinding(dataBinding => dataBinding.Ajax()
                                    .Select("_SelectionClientSide_Normas", "Normas")
                                    .Insert("Insert", "Normas")
                                    .Update("Save", "Normas")
                                    .Delete("Delete", "Normas"))

                                .Editable(editing => editing.Mode(GridEditMode.InLine))
                                                               .Sortable()
                                .Selectable()
                                .Resizable(x => x.Columns(true))                                
                                .Pageable(x => x.PageSize(50))
                                .Scrollable(x => x.Height(300))
                                .Filterable()
                                .ClientEvents(events => events.OnRowSelect("onRowSelected")
                                                              .OnRowDataBound("Norma_onRowDataBound")
                                                              .OnDelete("onRowDeleted")
                                                              .OnDataBinding("dataBindingNorma")
                                                              .OnError("onError"))
                                .RowAction(row => row.Selected = row.DataItem.Id.Equals(ViewData["id"]))
                                .Render();
                    
                    });


                tabstrip.Add()
                    .Text("Содержание нормы")
                    .Content(() =>
                    {
        
%>
<form name="form2" action ="Normas/InsertNormaOrganization">
<input type="hidden" name="NormaId4Tree" id="NormaId4Tree" value=""/>
<input type="hidden" name="IsApproved" id="IsApproved" value=""/>
<input type="hidden" name="OrgString" id="OrgString" value=""/>

<input type="hidden" name="ROLE_ADMIN" id="ROLE_ADMIN" value="<%: HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) %>"/>
<input type="hidden" name="ROLE_NORMA_EDIT" id="ROLE_NORMA_EDIT" value="<%: HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_EDIT) %>"/>
<input type="hidden" name="ROLE_NORMA_APPROVED" id="ROLE_NORMA_APPROVED" value="<%: HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_APPROVED) %>"/>

</form>
<table>
<tr>
<td width="60%">
          

<%
    Html.Telerik().Grid((IEnumerable<Store.Core.NormaContent>)ViewData["NormaContents"])
        .Name("NormaContents")

        .ToolBar(commands =>
            {
                if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                    HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_EDIT) ||
                    HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_APPROVED))
                {

                    commands.Insert().ButtonType(GridButtonType.Image).HtmlAttributes(new { title = "Добавить", style = "DISPLAY:inline" });
                }   
            })

        .DataKeys(keys =>
        {
            keys.Add(c => c.Id);
        })
        .DataBinding(dataBinding => dataBinding.Ajax()
                .Select("_SelectionClientSide_NormaContents", "Normas")
                .Insert("InsertContent", "Normas")
                .Update("SaveContent", "Normas")
                .Delete("DeleteContent", "Normas"))
        .Columns(columns =>
        {
            columns.Command(commands =>
            {
                if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) || HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_APPROVED) ||
                    HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_EDIT)    )
                {

                    commands.Edit().ButtonType(GridButtonType.Image).HtmlAttributes(new { title = "Изменить", style = "DISPLAY:inline" });
                    commands.Delete().ButtonType(GridButtonType.Image).HtmlAttributes(new { title = "Удалить", style = "DISPLAY:inline" });

                }
            }).Width(90);
            columns.Bound(c => c.NomGroup).Width(50).Title("")
                                           .Filterable(false)
                                           .ReadOnly(true)
                                           .ClientTemplate("<#= (NomGroup!=null && NomGroup.IsWinter)?'<font color=blue>зима</font>': (NomGroup!=null && NomGroup.NomBodyPart.Id==" + DataGlobals.SIZ_SIZE_ID + ")?(NomGroup.NomBodyPart.Name):'' #>");
            columns.Bound(c => c.NomGroup).ClientTemplate("<#= NomGroup.Name #>").Title("Группы материалов");
            columns.Bound(c => c.InShop).Width(50).Title("Цех")
                                        .Filterable(false)
                                        .ClientTemplate("<input type='checkbox' disabled='true'  name='isactive' <#= (InShop)?'checked':'' #> />");
            columns.Bound(c => c.Quantity).Width(50).Title("Кол-во").Filterable(false).HeaderHtmlAttributes(new { title = "Количество по норме предприятия" }).HtmlAttributes(new { title = "Количество по норме предприятия" });
            columns.Bound(c => c.UsePeriod).Width(50).Title("Пер.").Filterable(false).HeaderHtmlAttributes(new { title = "Период использования" });
            columns.Bound(c => c.QuantityTON).Width(50).Title("ТОН").Filterable(false).HeaderHtmlAttributes(new { title = "Количество по ТОН РФ" }).HtmlAttributes(new { title = "Количество по ТОН РФ"});
            columns.Bound(c => c.NormaId).Hidden();
            columns.Bound(c => c.Id).Hidden();
            columns.Bound(c => c.IsActive).Hidden();
        })

        .Editable(editing => editing.Mode(GridEditMode.InLine))
        .ClientEvents(events => events.OnEdit("onEditNomGroup")
                                      .OnRowDataBound("Grid_onRowDataBound")
                                      .OnRowSelect("onRowContentSelected")
                                      .OnDelete("onRowDeletedContent")
                                      .OnError("onError"))
        .Scrollable(x => x.Height(300))
        .Resizable(x => x.Columns(true))                                
        .Sortable()
        .Selectable()
        .Filterable()
        .Render();

%>

</td>
<td>
<%
             Html.Telerik().Grid<Store.Core.NormaNomGroup>()
                       .Name("NormaNomGroups")
                       .ToolBar(commands => 
                           {
                            if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_EDIT) ||
                                HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_APPROVED))
                            {

                                commands.Insert().ButtonType(GridButtonType.Image).HtmlAttributes(new { title = "Добавить" });
                            }
                           })
                       .DataKeys(keys =>
                        {
                            keys.Add(o => o.Id);
                        })
                       .Columns(columns =>
                        {
                            columns.Command(commands =>
                            {
                                if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) || HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_APPROVED) ||
                                   HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_EDIT) )
                                {

                                    commands.Edit().ButtonType(GridButtonType.Image).HtmlAttributes(new { title = "Изменить", style = "DISPLAY:inline" });
                                    commands.Delete().ButtonType(GridButtonType.Image).HtmlAttributes(new { title = "Удалить", style = "DISPLAY:inline" });

                                }
                            }).Width(100);
                            columns.Bound(o => o.NomGroup).ClientTemplate("<#= NomGroup.Name #>").Title("Группы замены");
                        })
                     .ClientEvents(events => events.OnEdit("onEditNomGroup")
                                                   .OnError("onError")
                                          
                    // Договорились с Лихолобовой Н. , что группы замены не давать редактировать после утверждения
                                                 .OnRowDataBound("Grid_onRowDataBound")
                                                   )
                     .DataBinding(dataBinding => dataBinding.Ajax()
                                 .Select("_Selection_NormaNomGroups", "Normas")
                                 .Insert("InsertNormaNomGroup", "Normas")
                                 .Update("SaveNormaNomGroup", "Normas")
                                 .Delete("DeleteNormaNomGroup", "Normas")
                                 )
                     .Editable(editing => editing.Mode(GridEditMode.InLine))
        .Scrollable(x => x.Height(300))
        .Resizable(resizing => resizing.Columns(true))
        .Sortable()
        .Selectable()
        .Filterable()
        .Render();

        %>

</td>
</tr>
</table>
<%
                    });
                tabstrip.Add()
                    .Text("Рабочие места")
                    .Content(() =>
                    {
//                        Html.Telerik().Grid((IEnumerable<Store.Core.NormaOrganization>)ViewData["NormaOrganization"])
                        Html.Telerik().Grid((IEnumerable<Store.Core.NormaOrganizationSimple>)ViewData["NormaOrganizationSimple"])
                             .Name("NormaOrganization")
                             .ToolBar(
                             commands =>
                             {
                                if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                                    HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_EDIT) ||
                                    HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_APPROVED))
                                {

                                 commands
                                     .Custom()
                                     .Text("Организация")
                                     .HtmlAttributes(new { title = "Добавить рабочее место" })
                                     .Url("javascript:TreeOnClick()");
                             
                                }
                            }

                              )
                             .DataKeys(keys =>
                             {
                                 keys.Add(c => c.Id);
                             })
                                  .DataBinding(dataBinding => dataBinding.Ajax()
                                          .Select("_Selection_NormaOrganizations", "Normas")
                                          .Delete("DeleteNormaOrganization", "Normas"))
                                  .Columns(columns =>
                                  {
                                      columns.Command(commands =>
                                      {
                                          if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) || HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_APPROVED) ||
                                              HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_EDIT))
                                          {
                                              commands.Delete().ButtonType(GridButtonType.Image).HtmlAttributes(new { title = "Удалить" , style = "DISPLAY:inline"});

                                          }
                                      }).Width(50);
                                      columns.Bound(x => x.Sort).Width("100px").Title("Шифр");
                                      columns.Bound(c => c.OrganizationFullName).Title("Рабочее место");
                                      columns.Bound(x => x.IsActive).Width("50px")
                                         .Filterable(false)
                                         .ClientTemplate("<input type='checkbox' disabled='true'  name='isactive' <#= (IsActive)?'checked':'' #> />");

                                      columns.Bound(c => c.Norma).Hidden();

                                  })
                            // Договорились с Лисовицким А.А. , что группы замены давать редактировать после утверждения
                           // .ClientEvents(events => events.OnRowDataBound("Grid_onRowDataBound"))
                                .ClientEvents(events => events.OnError("onError"))
                                .Pageable(c => c.PageSize(50))
                                .Scrollable(c => c.Height(300))
                                .Sortable()
                                .Selectable()
                                .Filterable()
                                .Render();
                    });

                    
                           
        })
            .SelectedIndex(0)
            .Render();
        
        %>


<script type="text/javascript">
    function onError(args) {
        if (args.textStatus == "modelstateerror" && args.modelState) {
            var message = "Ошибки:\n";
            $.each(args.modelState, function (key, value) {
                if ('errors' in value) {
                    $.each(value.errors, function () {
                        message +=  this + "\n";
                    });
                }
            });
            try {
                args.preventDefault();
            }
            catch (g) { }
            alert(message);
        } else {
            var xhr = args.XMLHttpRequest;
            if (args.textStatus='error') {
                var window = $('#Window_alert').data('tWindow');
                window.content(xhr.responseText).center().open();
            }
        }
    }
 
    var orgToolPanel =      getGridElement('NormaOrganization', 'div', 't-grid-top');
    var orgToolButton =     getGridElement('NormaOrganization', 'a',   't-button');
    var contentToolPanel =  getGridElement('NormaContents',     'div', 't-grid-top');
    var nomGroupToolPanel = getGridElement('NormaNomGroups',    'div', 't-grid-top');
    var NormId = 0;
    var NormTreeId = "";
    var workerWorkPlaceTreeId = "";

    var req;

    function findNorma() {

        var shopNumber = $("#Shops").data("tComboBox").value();
            $("#Normas").data("tGrid").rebind();
    }

    function dataBindingNorma(args) {
        var shopNumber = $("#Shops").data("tComboBox").value();
        var shopInfo = $("#Shops").data("tComboBox").text();
        args.data = $.extend(args.data, { shopNumber: shopNumber, shopInfo: shopInfo });
    }

    function normaNomGroups_onRowDataBound(e) {
        var grid = $(this).data('tGrid');
        expandFirstRow(grid, e.row);
    }
    function expandFirstRow(grid, row) {
        if (grid.$rows().index(row) == 0) {
            grid.expandRow(row);
        }
    }
// Скрываю нижнюю часть грида, чтобы было больше места на экране
    var normaGridBottom    = getGridElement('Normas',            'div', 't-grid-bottom');
    var orgGridBottom      = getGridElement('NormaOrganization', 'div', 't-grid-bottom');
    var contentGridBottom  = getGridElement('NormaContents',     'div', 't-grid-bottom');
    var nomGroupGridBottom = getGridElement('NormaNomGroups',    'div', 't-grid-bottom');


    function getGridElement(idGrid, element, name) {
        var grid = document.getElementById(idGrid);
        if (grid) {
            divElements = grid.getElementsByTagName(element);
            for (i = 0; i < divElements.length; i++) {
                divClassName = divElements[i].className;
                if (divClassName.indexOf(name) > 0) {
                    return divElements[i];
                }
            }
        }
        return null;
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

//    function checkStatus() {
//        if (req.readyState == 4) {
//            if (req.status == 200) { 
//            var normaOrganizationGrid = $('#NormaOrganization').data('tGrid');
//            var NormaId = document.getElementById("NormaId4Tree").value;   
//            normaOrganizationGrid.rebind({
//                Id: NormaId
//            });
//        } else {
//                var window = $('#Window_alert').data('tWindow');
//                window.content(req.responseText).center().open();
//            }
//        }
//    }

    function checkNormaStatus() {
        if (req.readyState == 4) {
            if (req.status == 200) {
                alert("Копирование прошло успешно!");
                document.location.href = document.location.href;
            } else {
                alert("Произошла ошибка запроса " + req.status + ":\n" + req.statusText);
            }
        }
    }

    function TreeOnClick() {
        normTreeId = "";
        workerWorkPlaceTreeId = "";
        var result = window.showModalDialog("Organizations/ShowTree?hideHeader=true", window.self, 'dialogHeight=750px;dialogWidth=1100px; status: no; help: no; center: yes; resizable: no; scroll: no');
            if (NormId != 0) {
                $.ajax({
                    url: 'Normas/InsertNormaOrganization',
                    contentType: 'application/x-www-form-urlencoded',
                    type: 'POST',
                    dataType: 'json',
                    error: function (xhr, status) {
                        onGridErrorEvent(xhr);
                    },
                    data: "OrgString=" + NormId + "&NormaId4Tree=" + document.getElementById("NormaId4Tree").value,
                    success: function (result) {
                        var normaOrganizationGrid = $('#NormaOrganization').data('tGrid');
                        var NormaId = document.getElementById("NormaId4Tree").value;
                        normaOrganizationGrid.rebind({
                            Id: NormaId
                        });
                        NormId = 0;
                        if (result.modelState) {
                            result.textStatus = "modelstateerror";
                            onError(result);
                        }
                    }
                });
            }
    }


    function onRowSelected(e) {
        var normaContentsGrid = $('#NormaContents').data('tGrid');
        var normaNomGroupsGrid = $('#NormaNomGroups').data('tGrid');
        var normaOrganizationGrid = $('#NormaOrganization').data('tGrid');
        var normaGrid = $('#Normas').data('tGrid');
        
        var dataItem = normaGrid.dataItem(e.row);

        var NormaId = dataItem['Id']; ;
        var NormaContentId = 0;
        document.form2.NormaId4Tree.value = NormaId; 
           var normaID = dataItem['Id'];
            var normaName = dataItem['Name'];
            var IsApproved = dataItem['IsApproved'];
            document.form2.IsApproved.value = IsApproved; 
            if ((document.getElementById("ROLE_ADMIN").value == "False" ||
                 document.getElementById("ROLE_NORMA_APPROVED").value == "False") &&
                 document.getElementById("ROLE_NORMA_EDIT").value == "True" && IsApproved) {

                if (contentToolPanel) {
                    contentToolPanel.style.display = "none";
//                    var innerHtml = contentToolPanel.innerHTML;
//                    innerHtml = innerHtml.replace("inline", "none");
//                    contentToolPanel.innerHTM = innerHtml;

                }
// Договорились с Лихолобовой , что группы замены не давать редактировать после утверждения
                if (nomGroupToolPanel)
                    nomGroupToolPanel.style.display = "none";
            }
            else {
                if (contentToolPanel)
                    contentToolPanel.style.display = "block";
                if (nomGroupToolPanel)
                    nomGroupToolPanel.style.display = "block";
            }


            if ($('#normaID').text() != normaID) {
                $('#normaID').text(normaID);
                $('#normaName').text(normaName);
            }

           normaContentsGrid.rebind({
                Id: NormaId
            });
            normaOrganizationGrid.rebind({
                Id: NormaId
            });
            normaNomGroupsGrid.rebind({
                NormaContentId: NormaContentId
            });

            if (orgToolButton) {
                orgToolButton.style.display = "";
            }

    }


    function onRowDeleted(e) {
       alert("ВНИМАНИЕ!!! Эти рабочие места остануться без нормы! ВЫДАЧА спецодежды сотрудникам будет невозможной.");
            var normaContentsGrid = $('#NormaContents').data('tGrid');
            var normaOrganizationGrid = $('#NormaOrganization').data('tGrid');
            var normaNomGroupGrid = $('#NormaNomGroups').data('tGrid');
            var NormaId = 0;
        
            normaContentsGrid.rebind({
                Id: NormaId
            });
            normaOrganizationGrid.rebind({
                Id: NormaId
            });
            normaNomGroupGrid.rebind({
                NormaContentId: NormaId
            });
    }

    function onRowDeletedContent(e) {
        var normaNomGroupGrid = $('#NormaNomGroups').data('tGrid');
        var NormaContentId = 0;

        normaNomGroupGrid.rebind({
            NormaContentId : NormaContentId
        });


    }
    function onRowContentSelected(e) {
        var normaNomGroupsGrid = $('#NormaNomGroups').data('tGrid');
        var dataItem = normaNomGroupsGrid.dataItem(e.row);

//        var NormaContentId = dataItem['Id'];

        var NormaContentId = e.row.cells[7].innerHTML;
        normaNomGroupsGrid.rebind({
            NormaContentId : NormaContentId
        });
    }


//    if (contentToolPanel) {

//        contentToolPanel.style.display = "none";

//    }
//    if (nomGroupToolPanel) {
//        nomGroupToolPanel.style.display = "none";
//    }

    function copyButtonClick(id) {
        if (!sendRequest("Normas/Copy", "Id=" + id, checkNormaStatus)) {
            alert("Не могу вызвать запрос на копирование норм!");
        }
    }




    function onEditNomGroup(e) {
        if (e.dataItem != null) {
            var obj = e.dataItem['NomGroup'];
            $(e.form).find('#NomGroup').data('tComboBox').value((obj == null) ? "" : obj.Id);
            $(e.form).find('#NomGroup').data('tComboBox').text((obj == null) ? "" : obj.Name);
        }
    }

    function Norma_onRowDataBound(e) {
        var row = e.row;
        var dataItem = e.dataItem;

        if (dataItem.IsApproved && document.getElementById("ROLE_NORMA_EDIT").value=="True") {
         var innerHtml = row.cells[0].innerHTML;
         var innerHtml1;
         innerHtml = innerHtml.replace("inline", "none");

         innerHtml1 = innerHtml.replace("inline", "none");
         row.cells[0].innerHTML = innerHtml1;
        }
 }

    function Grid_onRowDataBound(e) {
        var row = e.row;
        var isApproved = document.getElementById("IsApproved").value;

        if (isApproved == "true" && document.getElementById("ROLE_NORMA_EDIT").value == "True") {
            var innerHtml = row.cells[0].innerHTML;
            var innerHtml1;
            innerHtml = innerHtml.replace("inline", "none");

            innerHtml1 = innerHtml.replace("inline", "none");
            row.cells[0].innerHTML = innerHtml1;
        }
    }
</script>
</asp:Content>