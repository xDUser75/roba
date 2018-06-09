<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Store.Core.Storage>" %>

      <% Html.Telerik().ComboBox()
             .Name(ViewData.TemplateInfo.GetFullHtmlFieldName(string.Empty))
             //.AutoFill(true)
          //.SelectedIndex(Model != null ? Model.Id : -1)
          //.BindTo(new SelectList((IEnumerable)ViewData["workers"], "Id", "Fio", Model != null ? Model.Id.ToString() : null))
             .DataBinding(binding => binding.Ajax()
                  .Select("_GetNomenclaturesOnStorage", "MatPersonStore")                  
                  .Delay(500)
                  .Cache(false)
                  )
            .Encode(false) 
            .Filterable(filtering =>
                     filtering
                     .FilterMode(AutoCompleteFilterMode.Contains)
                     .MinimumChars(0)
             )
            .ClientEvents(events => events.OnDataBinding("onStorageComboBoxDataBinding"))
            .Render();
    %>
