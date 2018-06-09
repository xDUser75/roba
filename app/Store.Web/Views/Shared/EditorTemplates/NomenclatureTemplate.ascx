<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<%= Html.Telerik().ComboBox()
             .Name(ViewData.TemplateInfo.GetFullHtmlFieldName(string.Empty))
              .AutoFill(true)
              .DataBinding(binding => binding.Ajax()
                                                   // .Select("GetNomenclatures", this.Url.RouteUrl(this.ViewContext.RouteData.Values).Substring(this.Url.RouteUrl(this.ViewContext.RouteData.Values).LastIndexOf("/")))
                  .Select("GetNomenclatures", "Storages")
                                                     .Delay(1000)
                                                     .Cache(false)
                                                     )
            .HtmlAttributes(new { style = string.Format("width:{0}px", 200) })
            .Filterable(filtering =>
                    filtering
                    .FilterMode(AutoCompleteFilterMode.Contains)
                    .MinimumChars(4)
            )
            .HighlightFirstMatch(true)
       //.ClientEvents(events => events.OnDataBinding("onComboBoxDataBinding"))
            .OpenOnFocus(false)
%>
