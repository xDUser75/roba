	<%@ Page Title="" Language="C#"  MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<Store.Core.Organization>>" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">

<% Html.Telerik().Window()            
       .Name("Window")            
       .Title("Выберете рабочее место")            
//       .LoadContentFrom("ShowTree","Organizations")       
       .Modal(true)
//       .ClientEvents(events => events.OnOpen("Window_onOpen"))
       .Scrollable(true)            
       .Resizable()            
       .Width(870)
       .Height(260)            
       .Render();    
%>    

<script type="text/javascript">
//    function Window_onOpen() {
//        var window = $(this).data('tWindow'); // $(this) is equivalent to $('#Window')
//        // Use the Window client object
//    }
    function Window_onOpen() {
        var window = $('#Window').data('tWindow');
        window.ajaxRequest("ShowTree");
    }

//    function selElem(elem, name) {
//        opener.selElem(elem, name);
//        window.close(self);
//    }

</script>


</asp:Content>

