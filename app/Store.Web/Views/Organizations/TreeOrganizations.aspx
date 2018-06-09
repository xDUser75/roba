<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SimpleSite.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<Store.Core.Organization>>" %>
<%@ Import Namespace="Store.Data" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <script type="text/javascript" src="../Scripts/Base.js"></script>
    <script type="text/javascript" src="../Scripts/Lock.js"></script>

<style>
.t-norma
{
    color:red;
    background: #ccc;

}

</style>
<div id="mainMenu"></div>

<h3>Структура организации</h3>

<input type="button" id="ButtonSelect" value="Сохранить изменения" onclick="closeCurrentWindow()"/>
<div style="height : 500px; overflow: scroll">
<%= Html.Telerik().TreeView()
        .Name("AjaxTreeView")
        .ShowCheckBox(true)
//        .ShowLines(true)
        .BindTo(Model, (item, organization) =>
        {
            item.Text = organization.ShopNumber+" "+organization.Name;
            item.HtmlAttributes.Add("class", "upper_row");
            item.Value = organization.Id.ToString();
            item.Checkable=false;
            item.LoadOnDemand = organization.Childs.Count > 0;
        })
        .ClientEvents(events => events.OnChecked("onChecked")
                                       .OnSelect("onSelect")
                                       .OnDataBinding("onTreeViewBinding")
                                       .OnDataBound("onTreeViewDataBound")
                                        )
        .DataBinding(dataBinding => dataBinding
            .Ajax().Select("_AjaxLoading", "Organizations")
        )
%>
</div>

    <div id="NormaContentDiv">
    <!--#include file="../Shared/NormaContent.inc"-->


    </div>


<script type="text/javascript">
    var allCheckedItem = "";

    function onSelect(e) {
        var treeview = $('#AjaxTreeView').data('tTreeView');
        var normaContentsGrid = $('#NormaContents').data('tGrid');
        var normasGrid = $('#Normas').data('tGrid');
        var nodeElement = e.item;
        var nodeText = treeview.getItemText(nodeElement);
        var idNorma = treeview.getItemValue(nodeElement);
        document.getElementById("idNorma").value=idNorma;
    <%     
    if (Request["hideHeader"]!="true"){ %>

        if ( nodeText.indexOf('Норма')!=-1){
            normaContentsGrid.rebind({
                Id: idNorma
            });
            normasGrid.rebind({
                Id: idNorma
            });

            var window = $("#Window").data("tWindow");
            window.title(nodeText);
            window.center().open();        
        }
  <% } %>

    }

    function checkListOrg(num) {
        if (allCheckedItem.indexOf(num + ",") == -1) { allCheckedItem = allCheckedItem + num + ","; }
        else {
            strt = allCheckedItem.indexOf(num + ",");
            selGrp = allCheckedItem;
            str = num + ",";
            leng = str.length;
            allCheckedItem = allCheckedItem.substring(0, strt) + selGrp.substring(strt + leng);
        }
    }

    function onChecked(e) {
        var treeview = $(this).data('tTreeView');
        var nodeElement = e.item;
        var idOrg = treeview.getItemValue(nodeElement);
       checkListOrg(idOrg);
    }

    function closeCurrentWindow() {
        if (allCheckedItem == "") {
            alert("Рабочее место не выбрано!");
            return false;
        }
//        dialogArguments.NormId = allCheckedItem.substring(0, allCheckedItem.length -1);
        dialogArguments.NormId = allCheckedItem;
        window.close(self);
        var treeview = $('#AjaxTreeView').data('tTreeView');
//       var nodeElement = e.item;
          $.post("Organizations/_AjaxLoading", function (data) {
            null;
      });

    }

    var currentLoadingNode = null;
    function onTreeViewBinding(args) {
        currentLoadingNode = args.item;
    }

    function onTreeViewDataBound(e){
    <% if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
           HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_EDIT) ||
           HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_APPROVED)){%>
        var elements=$("span.Approved-show").parent("span.t-in");
        for (i=0; i<elements.length; i++){
            if (elements[i].innerHTML.indexOf("document_delete.gif") == -1){
                elements[i].innerHTML="<img class='t-image' style='cursor:pointer' width='24' height='24' onClick='delNormaFromTree(this);event.cancelBubble=true;' alt='' src='../Content/Images/document_delete.gif'/>"+elements[i].innerHTML;
            }
        }
        var elements=$("span.Approved-hide").parent("span.t-in");
        for (i=0; i<elements.length; i++){
            if (elements[i].innerHTML.indexOf("document_delete.gif") == -1){
                elements[i].innerHTML="<img class='t-image' style='cursor:pointer' width='24' height='24' onClick='delNormaFromTree(this);event.cancelBubble=true;' alt='' src='../Content/Images/document_delete.gif'/>"+elements[i].innerHTML;
            }
        }
    <%}%>

    <% if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) || 
            HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_APPROVED)){%>

        var elements=$("li.t-item").not(".upper_row").children("div.t-mid, div.t-top, div.t-bot").children("span.t-in");
        for (i=0; i<elements.length; i++){        
            if (elements[i].innerHTML.toUpperCase().indexOf("<IMG ") == -1){
                var innerHTML = "<img onMouseEnter='onMouseEnter(this)' onMouseLeave='onMouseLeave(this)' class='t-image' title='Применить все нормы' style='cursor:pointer' width='16' height='16' onClick='updateAllNorma(this,true);event.cancelBubble=true;' alt='' src='../Content/Images/apply_button.gif'/> "; 
                innerHTML = innerHTML + "<img onMouseEnter='onMouseEnter(this)' onMouseLeave='onMouseLeave(this)' class='t-image' title='Отменить все нормы' style='cursor:pointer' width='16' height='16' onClick='updateAllNorma(this,false);event.cancelBubble=true;' alt='' src='../Content/Images/undo_button.gif'/>"
                elements[i].innerHTML = innerHTML+elements[i].innerHTML;
            }
        }
    <%}%>

    }

   function updateAllNorma(obj,status){
       var message = "Утвердить все нормы?";
       if (!status) message = "Отменить утверждение всех норм?";
       cssConfirm(message, "Да", "Нет", function (state) {
            if (state) {
                var rootId="";
                var parentElem=obj.parentElement.parentElement;
                var vals=getElementByName(parentElem,"t-input");
                if (vals){
                    rootId=vals.value;
                }
                if (rootId != ""){
                    if (nkmk.Lock.isFree()) {
                        nkmk.Lock.setBusy();
                        nkmk.Lock.printBusyMessage("Идет обновление данных...");
                    }
                    $.ajax({
                        url: '_UpdateAllSelectedNormas',
                        contentType: 'application/x-www-form-urlencoded',
                        type: 'POST',
                        dataType: 'json',
                        error: function (xhr, status) {
                            nkmk.Lock.setFree();
                            onGridErrorEvent(xhr);
                        },
                        data: {
                            rootId: rootId,
                            status: status
                        },
                        success: function (result) {
                            nkmk.Lock.setFree();
                            if ((result)&&(result.modelState)) {
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
                                var elements=jQuery(parentElem.parentElement).find("span.t-sprite");
                                for (i=0; i<elements.length; i++){
                                    if (status){
                                        elements[i].className = "t-sprite Approved-show";
                                    } else {
                                        elements[i].className = "t-sprite Approved-hide";
                                    }
                                }
                            }
                        },
                        error: function (xhr, str) {
                            nkmk.Lock.setFree();
                            alert("Возникла ошибка: " + xhr.responseCode);
                        }
                    });
                } //  if (normId != "")
           }
       });
   }

   function onMouseEnter(img){
    img.width="13";
    img.height="13";
   }

   function onMouseLeave(img){
    img.width="16";
    img.height="16";
   }
   function getElementByName(parentObj, name) {
        var elem = parentObj.children;
        if (elem) {
            for (i = 0; i < elem.length; i++) {
                divClassName = elem[i].className;
                if (divClassName){
                    if (divClassName.indexOf(name) >= 0) {
                        return elem[i];
                    }
                }
            }
        }
        return null;
    }

    function delNormaFromTree(obj){
<%if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
           HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_EDIT) ||
           HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_APPROVED)){
           %>
        var normId="";
        var workerWorkPlaceId="";
        var parantElem=obj.parentElement.parentElement;
        var vals=getElementByName(parantElem,"t-input");
        if (vals){
            normId=vals.value;
        }
        var workerWorkPlaceId;
        parantElem=parantElem.parentElement.parentElement.parentElement;
        parantElem=parantElem.getElementsByTagName('DIV');
        if (parantElem){
            var vals=getElementByName(parantElem[0],"t-input");
            if (vals){
                workerWorkPlaceId=vals.value;
            }
        }
        cssConfirm("Вы подтверждаете удаление нормы?", "Да", "Нет", function (state) {
            if (state) {
                <% if (!(Request["hideHeader"]=="true")){ %>
                    if (!sendRequest("../Normas/DeleteNormaFromWorkerWorkplace", "normId=" + normId + "&workerWorkPlaceId=" + workerWorkPlaceId, checkStatus)) {
                        normId="";
                        workerWorkPlaceId="";
                        alert("Не могу вызвать запрос на удаление нормы на рабочем месте!");
                    }
                    <%} else { %>
                    dialogArguments.normTreeId=normId;
                    dialogArguments.workerWorkPlaceTreeId=workerWorkPlaceId;
                    window.close(self);
                    <%}%>
                } else {
                    dialogArguments.normTreeId="";
                    dialogArguments.workerWorkPlaceTreeId="";
                }
            });
        <%}%>
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

    function checkStatus() {
        if (req.readyState == 4) {
            if (req.status == 200) { 
                alert("Норма успешно удалена");
                document.location.href=document.location.href;
            } else {
                alert("Произошла ошибка запроса " + req.status + ":\n" + req.statusText);
            }
        }
    }


    <%     
    if (Request["hideHeader"]=="true"){ %>
        document.getElementById("header").style.display = "none";
        document.getElementById("ButtonSelect").style.display = "block";
    <%} else {%>
        document.getElementById("ButtonSelect").style.display = "none";
    <%}%>

</script>

</asp:Content>

