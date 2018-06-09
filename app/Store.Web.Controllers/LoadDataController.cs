using System;
using System.Web.Mvc;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using Store.Core;
using Telerik.Web.Mvc;
using System.Collections.Generic;
using Store.Core.RepositoryInterfaces;
using NHibernate;
using Store.Data;
using SharpArch.Data.NHibernate;
using Quartz;
using System.Web;
using Store.Core.Utils;
using Store.Core.External.Interfaсe;


namespace Store.Web.Controllers
{
    [HandleError]
    public class LoadDataController : ViewedController
    {
        private readonly ILoadDataRepository loadDataRepository;
        private readonly CriteriaRepository<Norma> normaRepository;
        private readonly CriteriaRepository<NomGroup> nomGroupRepository;
        private readonly CriteriaRepository<StorageName> storageNameRepository;
        private readonly OrganizationRepository organizationRepository;

        public LoadDataController(ILoadDataRepository loadDataRepository, CriteriaRepository<Norma> normaRepository, CriteriaRepository<NomGroup> nomGroupRepository,
                                  CriteriaRepository<StorageName> storageNameRepository, OrganizationRepository organizationRepository)
        {
            Check.Require(loadDataRepository != null, "LoadDataRepository may not be null");

            this.loadDataRepository = loadDataRepository;
            this.normaRepository = normaRepository;
            this.nomGroupRepository = nomGroupRepository;
            this.storageNameRepository = storageNameRepository;
            this.organizationRepository = organizationRepository;
        }

        [HttpPost]
        public ActionResult _GetNormas(string text)
        {
            string idOrg = getCurrentEnterpriseId();
            int idEnterprise = System.Int32.Parse(idOrg);
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            Dictionary<string, object> order = new Dictionary<string, object>();

            queryParams.Add("Organization.Id", idEnterprise);
            queryParams.Add("Name", text);
            queryParams.Add("IsActive", true);
            order.Add("Name", ASC);
            IList<Norma> normas = normaRepository.GetByLikeCriteria(queryParams, order);

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new SelectList(normas, "Id", "Name")
            };
        }

        public ActionResult _GetAllShops()
        {
            string idOrg = getCurrentEnterpriseId();
            int idEnterprise = System.Int32.Parse(idOrg);

            IList<Organization> shops = organizationRepository.GetAllShopsByEnterprise(idEnterprise);

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new SelectList(shops, "Id", "ShopInfo")
            };
        }

        [Transaction]
        public ActionResult Index() {
            Dictionary<string, object> query = new Dictionary<string, object>();
            query.Add("Organization.Id", int.Parse(getCurrentEnterpriseId()));
            query.Add("[in]Id", ((Store.Core.Account.User)Session[DataGlobals.ACCOUNT_KEY]).ObjectsCSV);
            IList<StorageName> storageNames = storageNameRepository.GetByCriteria(query);
            SelectList storageNameList = new SelectList(storageNames, "Id", "Name", Session["storageNameId"] != null ? (string)Session["storageNameId"] : "0");
            ViewData[DataGlobals.REFERENCE_STORAGE_SHOP_NAME] = storageNameList;            

            return View(viewName);
        }

  
        [Transaction]
        public ActionResult LoadNomenclatureFromSAP()
        {
            string idOrg = getCurrentEnterpriseId();
            string assemblyName1="";
            if (idOrg == DataGlobals.ORG_ID_EVRAZRUDA)
            {
                assemblyName1 = ApplicationConfig.ReadVariable("/Configuration/Organization[@id=" + idOrg + "]/InterfaceLoadNomenclature/AssemblyName");
                string className1 = ApplicationConfig.ReadVariable("/Configuration/Organization[@id=" + idOrg + "]/InterfaceLoadNomenclature/ClassName");
                IExternalLoaderNomenclature nomloader = (IExternalLoaderNomenclature)Store.Core.Utils.Reflection.LoadClassObject(assemblyName1, className1);

                if (nomloader != null)
                {

                    string error = nomloader.LoadNomenclature(idOrg, Session.SessionID);
                    if (error.Length > 0)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

            }
            else
                loadDataRepository.LoadNomenclatureFromSAP(idOrg);
            return RedirectToAction("Index");
        }

        [Transaction]
        public ActionResult LoadWorkerCardOut()
        {
            string idOrg = getCurrentEnterpriseId();
            loadDataRepository.LoadWorkerCardOut(idOrg);
            return RedirectToAction("Index");
        }
        [Transaction]
        public ActionResult LoadWorkerCardDismiss(string p_dt)
        {
            string idOrg = getCurrentEnterpriseId();

            loadDataRepository.LoadWorkerCardDismiss(p_dt, idOrg);
            return RedirectToAction("Index");
        }

        [Transaction]
        public ActionResult LoadMatpersonCardOut(string p_shopid, string p_storagenameid)
        {
            string p_org_id = getCurrentEnterpriseId();
            loadDataRepository.LoadMatPersonCardOut(p_org_id, p_shopid, p_storagenameid);
            return RedirectToAction("Index");
        }

        [Transaction]
        public ActionResult LoadNormaContent(string p_shopid, string p_nomgroupList,string p_tabn, string p_normaid)
        {
            string idOrg = getCurrentEnterpriseId();
            loadDataRepository.LoadNormaContent(p_shopid, p_nomgroupList, p_tabn, p_normaid, idOrg);
            return RedirectToAction("Index");
        }
        [Transaction]
        public ActionResult LoadTransfer(string p_shopid_old, string p_shopid_new, string p_storagenameid_old, string p_storagenameid_new, string p_tabn, string p_operdate, string p_isstorageactive)
        {
            ResultState resultState = new ResultState();
            string p_organizationid = getCurrentEnterpriseId();
            String message="";
            if (p_shopid_old == null || p_shopid_new == null || p_shopid_old == "" || p_shopid_new == "")
                message = "Старый и новый Цех должны быть выбраны";
            else
                message = loadDataRepository.LoadTransfer(p_organizationid, p_shopid_old, p_shopid_new, p_storagenameid_old, p_storagenameid_new, p_tabn, p_operdate, p_isstorageactive);
            resultState.setMessage(message);
            //return RedirectToAction("Index");
            return Json(resultState);
        }
        
        [Transaction]
        public ActionResult AddChangeNomGroup(string p_shopid, string p_nomgroupid, string add_nomgroup, string p_normaid)
        {
            string idOrg = getCurrentEnterpriseId();
            loadDataRepository.AddChangeNomGroup(p_shopid, p_nomgroupid, add_nomgroup, p_normaid, idOrg);
            return RedirectToAction("Index");
        }


    }
}
