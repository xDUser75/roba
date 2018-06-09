<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Store.Core.NomGroup>" %>

      <%= Html.Telerik().ComboBox()
     
             .Name(ViewData.TemplateInfo.GetFullHtmlFieldName(string.Empty))
             .AutoFill(true)
             .DataBinding(binding => binding.Ajax()
                                                     .Select("_GetNomGroups", "Storages")
                                                     .Delay(400)
                                                     .Cache(true)
                                                     )
            .Filterable(filtering =>
                    filtering
                    .FilterMode(AutoCompleteFilterMode.StartsWith)
                    .MinimumChars(1)
            )
            .HighlightFirstMatch(true)
            .OpenOnFocus(false)
    %>
