<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<int>" %>

    <%= Html.Telerik().ComboBox()
             .Name(ViewData.TemplateInfo.GetFullHtmlFieldName(string.Empty))
             .AutoFill(true)
             .DataBinding(binding => binding.Ajax()
                                            .Select("_GetWorker", "SignDocumet")
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
            .OpenOnFocus(false)
    %>
