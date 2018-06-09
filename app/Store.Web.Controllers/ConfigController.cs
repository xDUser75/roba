using System;
using System.Globalization;
using System.Web.Mvc;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using SharpArch.Core.PersistenceSupport;
using Store.Core;
using Telerik.Web.Mvc;
using System.Collections.Generic;
using Store.ApplicationServices;
using Store.Data;

namespace Store.Web.Controllers
{
    [HandleError]
    public class ConfigController : ViewedController
    {
        private readonly CriteriaRepository<Config> configRepository;

        public ConfigController(CriteriaRepository<Config> configRepository)
        {
            Check.Require(configRepository != null, "configRepository may not be null");

            this.configRepository = configRepository;
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_VIEW_ALL + "," + DataGlobals.ROLE_CONFIG_EDIT))]
        public ActionResult Index()
        {
            //IList<Config> configs = configRepository.GetAll();
            //Store.ApplicationServices.Example.Main();
            //return View(new GridModel(configs));
            return View(viewName);
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_VIEW_ALL + "," + DataGlobals.ROLE_CONFIG_EDIT))]
        public ActionResult Select()
        {
            string idOrg = getCurrentEnterpriseId();
            int idEnterprise = System.Int32.Parse(idOrg);

            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("OrganizationId", idEnterprise);

            IList<Config> configs = configRepository.GetByCriteria(queryParams);

//            IList<Config> configs = configRepository.GetAll();
            //Store.ApplicationServices.Example.Main();
            return View(new GridModel(configs));
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_CONFIG_EDIT))]
        public ActionResult Save(int? id)
        {
            string idOrg = getCurrentEnterpriseId();
            int idEnterprise = System.Int32.Parse(idOrg);

            Config config = null;
            if (id == null)
                config = new Config();
            else
                config = configRepository.Get(id.Value);
            if (TryUpdateModel(config))
               config.OrganizationId = idEnterprise;
               configRepository.SaveOrUpdate(config);
            return Select();
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_CONFIG_EDIT))]
        public ActionResult Delete(int id)
        {
            Config config = configRepository.Get(id);
            configRepository.Delete(config);
            return Select();
        }

    }
}
