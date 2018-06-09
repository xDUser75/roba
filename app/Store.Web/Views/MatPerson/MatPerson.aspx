<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage"%>
<%@ Import Namespace="Store.Data" %>
<%@ Import Namespace="Store.Core" %>
<%@ Import Namespace="Store.Core.Account" %>
<!--#include file="../Shared/GetReportUrl.inc"-->
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<h3>Справочник МОЛ</h3>
    <table border="0" width="100%">   
    <tr>
        <td valign="middle" align="right">
            Склад:
        </td>
        <td width="760">
            <%:
            Html.Telerik().DropDownList()
                .Name("StorageNameList")
                .BindTo((SelectList)ViewData[Store.Data.DataGlobals.REFERENCE_STORAGE_SHOP_NAME])
                .HtmlAttributes(new { style = "width: 400px" })
                .ClientEvents(events => events
                .OnChange("findPerson")
                )
             %>
        </td>
    </tr>
        <tr>
            <td align="right">Табельный/Ф.И.О</td>
            <td>
            <%= Html.Telerik().ComboBox()
                     .Name("WorkerWorkplaceCombo")
                     .AutoFill(false)
                     .DataBinding(binding => binding.Ajax()
                        .Select("_FindWorkerWorkPlace", "MatPerson")
                        .Delay(400)
                    //.Cache(false)
                        )
                    .HtmlAttributes(new { @style = "width:750px" })
                    .Filterable(filtering =>
                        filtering
                        .FilterMode(AutoCompleteFilterMode.Contains)
                        .MinimumChars(1)
                    )
                    //.Items(items =>
                    //    {
                    //        if (Session["workerWorkplaceId"] != null)
                    //            items.Add().Value("" + (int)Session["workerWorkplaceId"]).Text((string)Session["workerWorkplaceText"]);
                    //    })
                    .SelectedIndex(0)
                    .HighlightFirstMatch(true)
                    .OpenOnFocus(false)
            %>
            <%
                if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
                    HttpContext.Current.User.IsInRole(DataGlobals.ROLE_MATPERSON_EDIT))
                {
            %>
            &nbsp;&nbsp;
            <input type="button" value="Добавить" class="t-button" onclick="addPerson()"/>
            <%}%>
            </td>
        </tr>
    </table>

<% 
   Html.Telerik().Grid<MatPersonCardHead>()
        .Name("MatPerson")     
        .DataKeys(keys =>
        {
            keys.Add(x => x.Id);
        })
        .DataBinding(dataBinding => dataBinding
            .Ajax()
            .Select("_SelectPerson", "MatPerson")
        )
        .Columns(columns =>
        {
            if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) || HttpContext.Current.User.IsInRole(DataGlobals.ROLE_MATPERSON_EDIT))
            {
                columns.Bound(c => c).ClientTemplate("<span class='t-button t-grid-delete t-button-icon'><img src='Content/Images/del_button.gif' style='cursor:hand' alt='Сторнировать запрещено' onClick='delPerson(<#= Id #>)'/></span>")
                    .Width(40)
                    .ReadOnly()
                    .Filterable(false)
                    .Sortable(false);
            }
            else
            {
                columns.Bound(c => c).ClientTemplate("&nbsp;")
                    .Width(40)
                    .ReadOnly()
                    .Filterable(false)
                    .Sortable(false);
            }
            columns.Bound(x => x.Worker.TabN).Title("Таб.№").Width(100).Filterable(true);
            columns.Bound(x => x.Worker.Fio).Title("Фамилия И.О.").Filterable(true);
          columns.Bound(x => x.StorageName.Name).Title("Склад");
          columns.Bound(x => x.Department.Mvz).Title("Код").Width("100px").Filterable(true);
          columns.Bound(x => x.Department.MvzName).Title("Подразделение").Filterable(true);          
        })
        .ClientEvents(events => events
            .OnDataBinding("dataBinding")
            .OnError("onGridErrorEvent")
        )
        .Scrollable(x => x.Height(400))
        .Filterable()
        .Render();
%>
<script type="text/javascript">

    function findPerson() {
            $("#MatPerson").data("tGrid").rebind();
    }

    function delPerson(id) {
        cssConfirm("Удалить работника?", "Да", "Нет", function (state) {
            if (state) {
                $.ajax({
                    url: 'MatPerson/_DeletePerson',
                    contentType: 'application/x-www-form-urlencoded',
                    type: 'POST',
                    dataType: 'json',
                    error: function (xhr, status) {
                        onGridErrorEvent(xhr);
                    },
                    data: {
                        workerId: id
                    },
                    success: function (result) {
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
                            findPerson();
                        }
                    },
                    error: function (xhr, str) {
                        onGridErrorEvent(xhr);
                    }
                });
            }
        });
    }
<%
  if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
      HttpContext.Current.User.IsInRole(DataGlobals.ROLE_MATPERSON_EDIT))
 {
%>
    function addPerson(){
        var dropDownList = $("#WorkerWorkplaceCombo").data("tComboBox");
        var text = $("#WorkerWorkplaceCombo").data("tComboBox").text();
        var workerWorkPlaceId = "";
        if (dropDownList && dropDownList.value() != text) {
            workerWorkPlaceId = dropDownList.value();
        }

        if (workerWorkPlaceId == ""){
            alert("На выбран сотрудник!");
            return false;
        }

        var storageNameId = $("#StorageNameList").data("tDropDownList").value();
        $.ajax({
                url: 'MatPerson/_InsertPerson',
                contentType: 'application/x-www-form-urlencoded',
                type: 'POST',
                dataType: 'json',
                error: function (xhr, status) {
                    onGridErrorEvent(xhr);
                },
                data: {
                    workerWorkplaceId: workerWorkPlaceId,
                    StorageNameId: storageNameId
                },
                success: function (result) {
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
                        findPerson();
                    }
                },
                error: function (xhr, str) {
                    onGridErrorEvent(xhr);
                }
            });
    }
<%}%>
    function dataBinding(args) {
        var dropDownList = $("#WorkerWorkplaceCombo").data("tComboBox");
        var workerWorkPlaceId = "";
        if (dropDownList) {
            workerWorkPlaceId = dropDownList.value();
        }
        if (workerWorkPlaceId == "") {
            workerWorkPlaceId = "-1";
        }
        var storageNameId = $("#StorageNameList").data("tDropDownList").value();
        args.data = $.extend(args.data, { workerWorkPlaceId: workerWorkPlaceId, StorageNameId: storageNameId });
    }

    window.onresize = onFormResize;
    function onFormResize() {
        $("#StorageNameList").data("tDropDownList").disable();
        $("#StorageNameList").data("tDropDownList").enable();
        $("#WorkerWorkplaceCombo").data("tComboBox").disable();
        $("#WorkerWorkplaceCombo").data("tComboBox").enable();
    }

</script>
</asp:Content>
