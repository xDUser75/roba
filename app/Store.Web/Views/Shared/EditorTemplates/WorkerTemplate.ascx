<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Store.Core.Worker>" %>

    <%= Html.Telerik().ComboBox()
             .Name(ViewData.TemplateInfo.GetFullHtmlFieldName(string.Empty))
             //.Name("WorkerName")
             .AutoFill(true)
             //.SelectedIndex(Model != null ? Model.Id : -1)
             //.BindTo(new SelectList((IEnumerable)ViewData["workers"], "Id", "Fio", Model != null ? Model.Id.ToString() : null))
             .DataBinding(binding => binding.Ajax()
                                                     .Select("_GetWorkers", "WorkerWorkplaces")
                                                     .Delay(1000)
                                                     .Cache(false)
                                                     )
            .HtmlAttributes(new { style = string.Format("width:{0}px", 200) })
            .Filterable(filtering =>
                    filtering
                    .FilterMode(AutoCompleteFilterMode.Contains)
                    .MinimumChars(3)
            )
            .HighlightFirstMatch(true)
            //.ClientEvents(events => events.OnDataBinding("onComboBoxDataBinding"))
            .OpenOnFocus(false)
    %>
<%--    <script type="text/javascript">

      function onComboBoxDataBinding(e) {
        e.data = $.extend({}, e.data, { filterMode:
                $('#ComboBoxAttributes_FilterMode').data('tDropDownList').value()
        });
      }
      function onAutoCompleteDataBinding(e) {
        e.data = $.extend({}, e.data, { filterMode:
                $('#AutoCompleteAttributes_FilterMode').data('tDropDownList').value()
        });
      }
    </script>
--%>