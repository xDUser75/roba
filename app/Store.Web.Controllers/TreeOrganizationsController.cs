using System.Web.Mvc;
using System.Collections;
using System.Linq;
using System;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using SharpArch.Core.PersistenceSupport;
using Store.Core;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.UI;
using System.Collections.Generic;
using Store.Core.RepositoryInterfaces;
using Store.Data;


namespace Store.Web.Controllers
{
    [HandleError]
    public class TreeOrganizationsController : ViewedController
    {
        private readonly CriteriaRepository<Organization> treeOrganizationRepository;

        public TreeOrganizationsController(CriteriaRepository<Organization> treeOrganizationRepository)
        {
            Check.Require(treeOrganizationRepository != null, "treeOrganizationRepository may not be null");

            this.treeOrganizationRepository = treeOrganizationRepository;
        }

        [Transaction]
        public ActionResult Index()
        {
                return View("TreeOrganizations");
            //viewName
        }
      


    }
}
