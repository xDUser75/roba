<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Store.Core.NomGroup>" %>

      <%= Html.Telerik().ComboBox()
     
             .Name(ViewData.TemplateInfo.GetFullHtmlFieldName(string.Empty))
             .AutoFill(true)
             .DataBinding(binding => binding.Ajax()
                                                     .Select("_GetNomGroups", "Normas")
                                                     .Delay(400)
                                                     .Cache(true)
                                                     )
            .HtmlAttributes(new { style = string.Format("width:{0}px", 300) })
            .Filterable(filtering =>
                    filtering
                    .FilterMode(AutoCompleteFilterMode.StartsWith)
                    .MinimumChars(1)
            )
            .HighlightFirstMatch(true)
            //.ClientEvents(events => events.OnDataBinding("onComboBoxDataBinding"))
            .OpenOnFocus(false)
    %>
   <script type="text/javascript">
       function onComboBoxDataBinding(e) {
           if (document.getElementById("_isActiveNorma")) {
               var isActive = document.getElementById("_isActiveNorma").value;
               e.data = $.extend({}, e.data, { isActive: isActive });

           }
            }
    </script>
