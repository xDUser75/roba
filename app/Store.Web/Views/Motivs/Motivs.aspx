	<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<%: 
//Html.Telerik().ComboBox()
        //        .Name("OperTypes")
        //        .BindTo((IEnumerable<DropDownItem>)ViewData["operTypes"]);



    Html.Telerik().Grid<Store.Core.Motiv>()
            .Name("Motivs")
        .ToolBar(commands => commands.Insert())
        .DataKeys(keys =>
        {
            keys.Add(x => x.Id);
        })
        .DataBinding(dataBinding =>
        {
            dataBinding.Ajax()
              .Select("Index", "Motivs")
              .Insert("Save", "Motivs")
              .Update("Save", "Motivs")
              .Delete("Delete", "Motivs");
        })
        .Columns(columns =>
        {
            columns.Command(commands =>
            {
                commands.Edit();
                commands.Delete();
            }).Width(200).Title("Действие");
            //columns.Bound(x => x.Id).Width(100);
            columns.Bound(x => x.Id).Width(100);
            columns.Bound(x => x.Name);
           // columns.Bound(x => x.OperType.Name).Width(50);  
        })
// вложеный грид
        /*
           .DetailView(detailView => detailView.Template(e => // Set the server template
           {
               // Define a grid bound to the Orders collection of the Employee object
               // Display the details for the Employee

               Html.Telerik().Grid<Store.Core.OperType>()
                   //Ensure the Name of each grid is unique
                            .Name("OperType" + e.OperType.Name)
                            .Columns(columns =>
                            {
                                columns.Bound(o => o.Id);
                                columns.Bound(o => o.Name);
                            })
                            .Pageable();

           }))
         */
           .RowAction(row =>
           {
               //Expand the first detail view
               if (row.Index == 0)
               {
                   row.DetailRow.Expanded = true;
               }
           })
        .Editable(editing => editing.Mode(GridEditMode.PopUp))
        .Pageable()
        .Scrollable()
        .Sortable()
        .Filterable()
        .Groupable()
%>
</asp:Content>
