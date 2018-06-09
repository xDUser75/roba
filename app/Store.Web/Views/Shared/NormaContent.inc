<input type="hidden" id="idNorma" />
<input type="hidden" id="IsApproved" />
<%
    Html.Telerik().Window()
           .Name("Window")
           .Title("Норма")
           .Width(700)
           .Height(520)
           .Draggable(true)
           .Modal(false)
           .Visible(false)
           .Buttons(b => b.Close())
           .Content(() =>
           { %>
			 <%

			 Html.Telerik().Grid((IEnumerable<Store.Core.Norma>)ViewData["Normas"])
                .Name("Normas")
                .DataBinding(dataBinding => dataBinding.Ajax()
                .Select("_Selection_Normas", "Normas"))
                .Columns(columns =>
                {
                        columns.Bound(c => c).ClientTemplate("<img src=../Content/Images/document_certificate.gif id=imgApprovedDiv <#= (IsApproved==true?\"style =display:block\":\"style = display:none\")  #> />" );
                        columns.Bound(c => c.Name);
						columns.Bound(c => c.Id).Hidden();
                })
		        .Footer(false)
		        .HtmlAttributes(new { @class = "attachment-grid" })
                .ClientEvents(events => events.OnLoad("HideHeader")
											  .OnDataBinding("dataBinding")
											  )
                .Render();


			 Html.Telerik().Grid((IEnumerable<Store.Core.NormaContent>
				 )ViewData["NormaContents"])
				 .Name("NormaContents")
				 .ToolBar(commands =>
				 {
				 if (HttpContext.Current.User.IsInRole(DataGlobals.ROLE_ADMIN) ||
				 HttpContext.Current.User.IsInRole(DataGlobals.ROLE_NORMA_APPROVED))
				 {
				 commands.Custom()
				 .Text("Утвердить норму")
				 .HtmlAttributes(new { id = "approvedNorma", onclick = "approvedNorma()" })
				 .Url("#");
				 commands.Custom()
				 .Text("Отменить утверждение")
				 .HtmlAttributes(new { id = "unApprovedNorma", onclick = "unApprovedNorma()" })
				 .Url("#");

				 }
				 })
				 .DataKeys(keys =>
				 {
				 keys.Add(x => x.Id);
				 })

				 .DataBinding(dataBinding => dataBinding.Ajax()
				 .Select("_SelectionClientSide_NormaContents", "Normas"))
				 .Columns(columns =>
				 {
				 columns.Bound(c => c.NomGroup.Name).Width(180).Title("Группы материалов");
				 columns.Bound(c => c.InShop).Width(50).Title("Цех")
											 .Filterable(false)
											 .ClientTemplate("<input type='checkbox' disabled='true'  name='isactive' <#= (InShop)?'checked':'' #> />");
                         columns.Bound(c => c.NormaContentInfo).Title("Группы замены");
                         columns.Bound(c => c.Quantity).Width(50).Title("Кол-во").Filterable(false).HeaderHtmlAttributes(new { title = "Количество по норме предприятия" }).HtmlAttributes(new { title = "Количество по норме предприятия" });
                         columns.Bound(c => c.UsePeriod).Width(50).Title("Пер.").Filterable(false).HeaderHtmlAttributes(new { title = "Период использования" });
                         columns.Bound(c => c.QuantityTON).Width(50).Title("ТОН").Filterable(false).HeaderHtmlAttributes(new { title = "Количество по ТОН РФ" }).HtmlAttributes(new { title = "Количество по ТОН РФ"});
                         columns.Bound(c => c.NormaId).Hidden();
                         columns.Bound(c => c.Id).Hidden();
                         columns.Bound(c => c.IsApproved).Hidden();

                         })
                         .Scrollable(x => x.Height(370))
                         .Resizable(x => x.Columns(true))
                         .Sortable()
                         .Selectable()
                         .Filterable()
                         .Render();
                         })
                         .Render();
                         %>
                         <script type="text/javascript">

    function dataBinding(args) {
    var normasGrid = $('#Normas').data('tGrid');
    var idNorma = document.getElementById("idNorma").value;
    args.data = $.extend(args.data, { Id: idNorma });

    }

    function HideHeader() {
    $('.attachment-grid .t-header').hide();
    }

    function approvedNorma() {
    $.post("../Normas/ApprovedNorma", function (data) {
    $("#Normas").data("tGrid").rebind();
    document.getElementById("IsApproved").value=1;
    alert("Норма утверждена");
    getAllElement("t-sprite Approved-show");
    });
    }

    function unApprovedNorma() {
    $.post("../Normas/UnApprovedNorma", function (data) {
    $("#Normas").data("tGrid").rebind();
    document.getElementById("IsApproved").value=0;
    alert("Утверждение отменено");
    getAllElement("t-sprite Approved-hide");
    });
    }

    function getAllElement(IsApproved) {
    var elements = $("div .t-bot");
    var currentNormaId=document.getElementById("idNorma").value;
    for (i=0;i<elements.length;i++){
		  currentElem=elements[i];
		  inputsElements=currentElem.getElementsByTagName("input");
		  for (j=0;j<inputsElements.length;j++){
		    if ((inputsElements[j].name=="itemValue") && (inputsElements[j].value==currentNormaId)){
				imgElements=currentElem.getElementsByTagName("span");
				for (z=0;z<imgElements.length;z++){
					var spanClass = imgElements[z].className;
					if (spanClass.indexOf("Approved")>0){
						imgElements[z].className=IsApproved;
						break;
					}
				}
			}
		  }
		} 
    }

</script>