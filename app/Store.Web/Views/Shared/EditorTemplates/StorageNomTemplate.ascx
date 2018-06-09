<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Store.Core.Storage>" %>

      <% Html.Telerik().ComboBox()
             .Name(ViewData.TemplateInfo.GetFullHtmlFieldName(string.Empty))
             //.AutoFill(true)
          //.SelectedIndex(Model != null ? Model.Id : -1)
          //.BindTo(new SelectList((IEnumerable)ViewData["workers"], "Id", "Fio", Model != null ? Model.Id.ToString() : null))
             .DataBinding(binding => binding.Ajax()
                  //.Select("_GetNomenclatures", "WorkerCards", new {nomGroupId = Model != null?""+Model.NomGroup.Id:"" })
                  .Select("_GetNomenclaturesOnStorage", "WorkerCards")
                  //.Delay(1000)
                  //.Cache(false)
                  )
          //.HtmlAttributes(new { style = string.Format("width:{0}px", 300) })
            //.HtmlAttributes(new { style = "width:400px" })
            .Encode(false) 
            .Filterable(filtering =>
                     filtering
                     .FilterMode(AutoCompleteFilterMode.Contains)
                     .MinimumChars(0)
             )
            //.HighlightFirstMatch(true)
            //.Value(Model != null?Model.Name:"")
            .ClientEvents(events => events.OnDataBinding("onStorageComboBoxDataBinding"))
            //.OpenOnFocus(false)
            .Render();
    %>
