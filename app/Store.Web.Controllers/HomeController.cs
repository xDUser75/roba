using System.Web.Mvc;
using System.Web;
using Store.Data;
using Store.Core;
using System.Collections.Generic;
using Telerik.Web.Mvc;
using SharpArch.Web.NHibernate;
using SharpArch.Core;

namespace Store.Web.Controllers
{
    [HandleError]
    public class HomeController : ViewedController
    {
        private readonly CriteriaRepository<Message> messageRepository;

        public HomeController(CriteriaRepository<Message> messageRepository)
        {
            Check.Require(messageRepository != null, "messageRepository may not be null");
            this.messageRepository = messageRepository;
        }

        [Transaction]
        public ActionResult Index()
        {
            //string viewName = ControllerEnums.getViewName(ControllerContext);
            if (Session[DataGlobals.ACCOUNT_KEY] == null)
                return RedirectToAction("Logon", "LoginAccount");
            Session["_idOrg"] = getOrgByArmId();
            return View(viewName);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_MESSAGE_EDIT))]
        public ActionResult _SaveMessages(int? Id)
        {
            string idOrg = getCurrentEnterpriseId();
            Message message = null;
            if (Id == null)
            {
                message = new Message();
                message.IsActive = true;
            }
            else
                message = messageRepository.Get(Id.Value);
            if (TryUpdateModel(message))
            {
                message.MessageText = HttpUtility.HtmlDecode(message.MessageText);
                if (message.MessageText != null)
                {
                    message.MessageText = message.MessageText.Replace("</p><p>", "<br/>");
                }
                message.OrganizationId = int.Parse(idOrg);
                messageRepository.SaveOrUpdate(message);
            }
            return _SelectMessages();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_MESSAGE_EDIT))]
        public ActionResult _DeleteMessages(int? Id)
        {
            Message message = messageRepository.Get(Id.Value);
            //messageRepository.Delete(message);
            message.IsActive = false;
            messageRepository.SaveOrUpdate(message);
            return _SelectMessages();
        }

        public ActionResult Report(string idRep)
        {
            Session["Report"] = idRep;
            Session["_idOrg"] = getCurrentEnterpriseId();
            Session["_numdoc"] = ""; // сюда надо мне номер документа если из Выдачи грузится(Чегодаева)
            return Redirect("~/Reports/TableListing.aspx");
        }

        [GridAction]
        [Transaction]
        public ActionResult _SelectMessages() 
        {
            string idOrg = getCurrentEnterpriseId();
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("IsActive", true);
            queryParams.Add("OrganizationId", int.Parse( idOrg));
            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            orderParams.Add("MessDate", DESC);
            IList<Message> messages = messageRepository.GetByCriteria(queryParams, orderParams);
            return View(new GridModel(messages));
        }

    }
}
