<%@ Page Language="C#" MasterPageFile="~/Views/Shared/SimpleSite.Master" Inherits="System.Web.Mvc.ViewPage" %>

<%--asp:Content ID="Content1" ContentPlaceHolderID="HeadContentPlaceHolder" runat="server">
	LogOn
</asp:Content--%>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <div>
      <%--<%=this.Url.RouteUrl(this.ViewContext.RouteData.Values).Substring(this.Url.RouteUrl(this.ViewContext.RouteData.Values).LastIndexOf("/")) %><br/> --%>
      <%-- RemoteUser: <%= HttpContext.Current.Request.ServerVariables["REMOTE_USER"] --%>
      LogonUser: <%= HttpContext.Current.Request.LogonUserIdentity.Name %> <br />
      <!--AbsoluteUri: <%= HttpContext.Current.Request.Url.AbsoluteUri %><br />
      ApplicationPath: <%= HttpContext.Current.Request.ApplicationPath%><br />
      PhysicalApplicationPath: <%= HttpContext.Current.Request.PhysicalApplicationPath%><br />
      Authority: <%= HttpContext.Current.Request.Url.Authority%><br />
      Scheme: <%= HttpContext.Current.Request.Url.Scheme%><br /-->

      <%= Html.ValidationSummary() %>
          </div>
    <% using( Html.BeginForm()){ %>
    <fieldset>
        <legend>Вход в систему</legend>
<% if (HttpContext.Current.Request.LogonUserIdentity.Name == null || (bool?)ViewData["logonForce"] == true )
   { %>
        <table>
        <tr>
            <td><label for="userName">Код пользователя (таб. №):</label></td>
            <td><%= Html.TextBox("username", null, new { @class = "userbox"}) %></td>
        </tr>
        <tr>
            <td><label for="password">Пароль:</label></td>
            <td><%= Html.Password("password", null, new { @class = "passwordbox"}) %></td>
       </tr>
        <!--div>
            <label for="rememberMe">
                Remember Me:</label>
            <input type="checkbox" id="rememberMe" name="rememberMe"/>
        </div-->
<% } %>
        <tr>
            <td><label for="armId">Выберите организацию:</label></td>
            <td>
<%
    Html.Telerik().DropDownList()
                    .Name("_EnterpriceList")
                    .BindTo((SelectList)ViewData[Store.Data.DataGlobals.REFERENCE_ENTERPRICE])
                    //.Items(items =>
                    //{
                    //    items.Add().Value(Store.Data.DataGlobals.ARM_ID_OZSMK).Text("Спецодежда ОЗСМК");
                    //    items.Add().Value(Store.Data.DataGlobals.ARM_ID_EVRAZTECHNIKA).Text("Спецодежда ЕвразТехника");
                    //})
                    .HtmlAttributes(new { style = "width:300px" })
                    //.ClientEvents(events => events
                    //.OnChange("onEnterpriceListBox")
                    //)
                    .Render();
%>
            </td>
        </tr>
        </table>
        <div>
            <%-- =Html.SubmitButton() --%>
            <% =Html.Button("logon", "Вход", HtmlButtonType.Submit) %>
            <%-- =Html.Button("cancel", "Отмена", HtmlButtonType.Button) --%>
        </div>
        <% =Html.Hidden( "returnUrl", "/" ) %>
    </fieldset>
    <% } %>
</asp:Content>
