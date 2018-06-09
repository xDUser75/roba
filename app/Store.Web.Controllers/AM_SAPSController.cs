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
    public class AM_SAPSController : ViewedController
    {
        private readonly OrganizationRepository organizationRepository;        

        public AM_SAPSController(OrganizationRepository organizationRepository)
        {            Check.Require(organizationRepository != null, "AM_SAPRepository may not be null");
            this.organizationRepository = organizationRepository;
        }

        [HttpPost]
        public ActionResult _GetShops()
        {
            string idOrg = getCurrentEnterpriseId();
            int idEnterprise = System.Int32.Parse(idOrg);

            IList<Organization> shops = organizationRepository.GetShopsByEnterprise(idEnterprise);

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new SelectList(shops, "Id", "ShopInfo")
            };
        }

        [Transaction]
        public ActionResult Index() {
   //         AM_SAPRepository.GetAmSaps();
            return View(viewName);
        }

        [Transaction]
        [GridAction]
        [HttpPost]
        public ActionResult GetAM_SAP(string shopId, string childCare) 
        {
            string idOrg = getCurrentEnterpriseId();
            string shopNumber="";
            if (shopId != null && shopId != "" && shopId != "0")
                shopNumber = (organizationRepository.Get(int.Parse(shopId))).ShopNumber;
            //else
            //    shopNumber = "null";
            string assemblyName = ApplicationConfig.ReadVariable("/Configuration/Organization[@id=" + idOrg + "]/InterfaceLoadOrganization/AssemblyName");
            string className = ApplicationConfig.ReadVariable("/Configuration/Organization[@id=" + idOrg + "]/InterfaceLoadOrganization/ClassName");
            IExternalLoaderOrganization loader = (IExternalLoaderOrganization)Store.Core.Utils.Reflection.LoadClassObject(assemblyName,className);
            if (loader != null)
            {
                
                if (shopId == "") shopId = "0";
                    string error = loader.LoadOrganization(shopId, shopNumber, idOrg, Session.SessionID, childCare);
                    if (error.Length > 0) {
                        ModelState.AddModelError("", error);
                    }
            }
            return View(new GridModel(new List<Object>()));
        }
    }
}
