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
    public class NomBodyPartsController : ViewedController
    {
        private readonly NomBodyPartRepository nombodyPartRepository;

        public NomBodyPartsController(NomBodyPartRepository nombodyPartRepository)
        {
            Check.Require(nombodyPartRepository != null, "NombodyPartRepository may not be null");
            this.nombodyPartRepository = nombodyPartRepository;
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_NOM_BODY_PART_EDIT + ", " + DataGlobals.ROLE_NOM_BODY_PART_VIEW))]
        public ActionResult Index()
        {
            return View(viewName);
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_NOM_BODY_PART_EDIT + ", " + DataGlobals.ROLE_NOM_BODY_PART_VIEW))]
        public ActionResult Select()
        {
            return getAllAndView();
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + ", " + DataGlobals.ROLE_NOM_BODY_PART_EDIT))]
        public ActionResult Save(NomBodyPart nombodyPart)
        {
            nombodyPartRepository.SaveOrUpdate(nombodyPart);
            return getAllAndView();
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NOM_BODY_PART_EDIT))]
        public ActionResult Delete(NomBodyPart nombodyPart)
        {
            nombodyPartRepository.Delete(nombodyPart);
            return getAllAndView();
        }

        private ActionResult getAllAndView()
        {
            IList<NomBodyPart> nomBodyPart = nombodyPartRepository.GetAll();
            IList<NomBodyPart> model = new List<NomBodyPart>();
            foreach (var item in nomBodyPart)
            {
                NomBodyPart nbp = rebuildNomBodyPart(item);
                model.Add(nbp);
            };
            
            return View(new GridModel(model));
        }

    }
}
