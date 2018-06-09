<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Store.Core.NomGroup>" %>

      <%= Html.Telerik().ComboBox()
     
             .Name(ViewData.TemplateInfo.GetFullHtmlFieldName(string.Empty))
             .AutoFill(true)
             .DataBinding(binding => binding.Ajax()
                                                     .Select("_GetNomGroupsActive", "Normas")
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
            .OpenOnFocus(false)
    %>