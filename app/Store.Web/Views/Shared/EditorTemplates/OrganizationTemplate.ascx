<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Store.Core.Organization>" %>

    <%= Html.Telerik().ComboBox()
             .Name(ViewData.TemplateInfo.GetFullHtmlFieldName(string.Empty))
             .AutoFill(true)
             //.SelectedIndex(Model != null ? Model.Id : -1)
             //.BindTo(new SelectList((IEnumerable)ViewData["workers"], "Id", "Fio", Model != null ? Model.Id.ToString() : null))
             .DataBinding(binding => binding.Ajax()
                                                     .Select("_GetOrganizations", "WorkerWorkplaces")
                                                     .Delay(1000)
                                                     .Cache(true)
                                                     )
            .HtmlAttributes(new { style = string.Format("width:{0}px", 200) })
            .Filterable(filtering => filtering
                    .FilterMode(AutoCompleteFilterMode.Contains)
                    .MinimumChars(3)
            )
            .HighlightFirstMatch(true)
            .OpenOnFocus(false)
    %>
