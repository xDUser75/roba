	<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master"  %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
 <h3>Экспорт данных по ВГОК</h3>
         Накладная:<br/>
         <input type="text" class="input-validation-error" title="Введите № накладной" onkeyup="updateInputStatus(this)" id="nameNakl" name="nameNakl" value="<%=Request.Params["nameNakl"]%>"/>
         <br/>
         <input type="hidden" name="paramOrganization" value="<%=Request.Params["paramOrganization"]%>"/>
         <br/>
         <input type="hidden" name="dateN" value="<%=Request.Params["dateN"]%>"/>
         <input type="hidden" name="dateEnd" value="<%=Request.Params["dateEnd"]%>"/>
         <input type="hidden" name="ceh" value="<%=Request.Params["ceh"]%>"/>
         <input type="hidden" name="operTypeId" value="<%=Request.Params["operTypeId"]%>"/>
         <input type="hidden" name="paramStorage" value="<%=Request.Params["paramStorage"]%>"/>
         <input type="hidden" name="paramUchastokId" value="<%=Request.Params["paramUchastokId"]%>"/>
         <input type="hidden" name="paramSplit" value="<%=Request.Params["paramSplit"]%>"/>
         <input type="hidden" name="paramTabN" value="<%=Request.Params["paramTabN"]%>"/>
         <input type="button" value="Передать данные" onclick="validateAndSubmit()"/>

<script type="text/javascript">
    function processResponse(result) {
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
            alert("Данные успешно обработаны!");
        }
    }

    function validateAndSubmit() {
        if (document.getElementById("nameNakl").value == "") {
            alert("Номер накладной не указан!");
            return false;
        }
        var postData = "";
        //Получаем все параметры с формы
        var objArray = $("input[type='hidden'],[type='text']");
        $.each(objArray, function () {
            if (postData != "") postData = postData + "&";
            postData = postData + this.name + "=" + this.value;
        });

        //Отправляем запрос контроллеру
        $.ajax({
            url: 'ExportConsumption/_Export',
            contentType: 'application/x-www-form-urlencoded',
            type: 'POST',
            dataType: 'json',
            error: function (xhr, status) {
                onGridErrorEvent(xhr);
            },
            data: postData,
            success: function (result) {
                processResponse(result);
            },
            error: function (xhr, str) {
                onGridErrorEvent(xhr);
            }
        });

    }
    function updateInputStatus(obj) {
        if (obj.value == "") {
            obj.className = "input-validation-error";
            obj.title = "Введите № накладной";
        } else {
            obj.className = "";
            obj.title = "";
        }
    }

    document.getElementById("nameNakl").focus();
    if (document.getElementById("trigger2")) {
        document.getElementById("trigger2").style.display = "none";
    }
    updateInputStatus(document.getElementById("nameNakl"));
</script>

</asp:Content>
