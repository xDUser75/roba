using System.Web.Mvc;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using Store.Core;
using System.Collections.Generic;
using Store.Data;
using Store.Core.External.Interfaсe;
using Store.Core.Utils;
using Telerik.Web.Mvc;
using System;

namespace Store.Web.Controllers
{
    [HandleError]
    public class ChildCareController : ViewedController
    {
        private readonly OrganizationRepository organizationRepository;

        public ChildCareController(OrganizationRepository organizationRepository)
        {            Check.Require(organizationRepository != null, "CHILDCARERepository may not be null");
            this.organizationRepository = organizationRepository;
        }

        [Transaction]
        public ActionResult Index() {
            return View(viewName);
        }

     }
}
