using System;
using System.Globalization;
using System.Web.Mvc;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using SharpArch.Core.PersistenceSupport;
using Store.Core;
using Telerik.Web.Mvc;
using System.Collections.Generic;

namespace Store.Web.Controllers
{
    [HandleError]
    public class MotivsController : Controller
    {
        private readonly IRepository<Motiv> motivRepository;

        public MotivsController(IRepository<Motiv> motivRepository)
        {
            Check.Require(motivRepository != null, "MotivRepository may not be null");

            this.motivRepository = motivRepository;
        }

        [GridAction]
        [Transaction]
        public ActionResult Index() {
            IList<Motiv> motivs = motivRepository.GetAll();
            return View(new GridModel(motivs));
        }

        [GridAction]
        [Transaction]
        public ActionResult Save(Motiv motiv)
        {
            motivRepository.SaveOrUpdate(motiv);
            IList<Motiv> motivs = motivRepository.GetAll();
            return View(new GridModel(motivs));
        }

        [GridAction]
        [Transaction]
        public ActionResult Delete(Motiv motiv)
        {
            motivRepository.Delete(motiv);
            IList<Motiv> motivs = motivRepository.GetAll();
            return View(new GridModel(motivs));
        }

    }
}
