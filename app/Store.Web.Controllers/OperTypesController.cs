using System.Web.Mvc;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using SharpArch.Core.PersistenceSupport;
using Store.Core;
using Telerik.Web.Mvc;
using System.Collections.Generic;
using Store.Data;

namespace Store.Web.Controllers
{
    [HandleError]
    public class OperTypesController : ViewedController
    {
        private readonly IRepository<OperType> operTypeRepository;

        public OperTypesController(IRepository<OperType> operTypeRepository)
        {
            Check.Require(operTypeRepository != null, "operTypeRepository may not be null");

            this.operTypeRepository = operTypeRepository;
        }

        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_OPER_TYPE_EDIT + ", " + DataGlobals.ROLE_OPER_TYPE_VIEW))]
        public ActionResult Index()
        {
            return View(viewName);
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_OPER_TYPE_EDIT + ", " + DataGlobals.ROLE_OPER_TYPE_VIEW))]
        public ActionResult Select()
        {
            return getAllAndView();
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_OPER_TYPE_EDIT))]
        public ActionResult Save(OperType operType)
        {
            operTypeRepository.SaveOrUpdate(operType);
            return getAllAndView();
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_OPER_TYPE_EDIT))]
        public ActionResult Delete(OperType operType)
        {
            operTypeRepository.Delete(operType);
            return getAllAndView();
        }

        private ActionResult getAllAndView()
        {
            IList<OperType> operType = operTypeRepository.GetAll();
            return View(new GridModel(operType));
        }
    }
}
