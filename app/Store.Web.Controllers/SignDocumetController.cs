using System.Web.Mvc;
using SharpArch.Core;
using Store.Core;
using Telerik.Web.Mvc;
using System.Collections.Generic;
using Store.Data;
using SharpArch.Web.NHibernate;

namespace Store.Web.Controllers
{
    [HandleError]
    public class SignDocumetController : ViewedController
    {
        private readonly CriteriaRepository<Worker> workerRepository;
        private readonly CriteriaRepository<SignDocumet> signDocumetRepository;        
        private readonly OrganizationRepository organizationRepository;
        private readonly CriteriaRepository<StorageName> storageNameRepository;
        private readonly CriteriaRepository<SignDocTypes> signDocTypesRepository;
        private readonly CriteriaRepository<SignTypes> signTypesRepository;

        public SignDocumetController(CriteriaRepository<Worker> workerRepository,
                                 CriteriaRepository<SignDocumet> signDocumetRepository,
                                 OrganizationRepository organizationRepository,
                                 CriteriaRepository<StorageName> storageNameRepository,
                                 CriteriaRepository<SignDocTypes> signDocTypesRepository,
                                 CriteriaRepository<SignTypes> signTypesRepository)
        {
            Check.Require(signDocumetRepository != null, "subscriptionRepository may not be null");
            this.workerRepository = workerRepository;
            this.signDocumetRepository = signDocumetRepository;
            this.organizationRepository = organizationRepository;
            this.storageNameRepository = storageNameRepository;
            this.signDocTypesRepository = signDocTypesRepository;
            this.signTypesRepository = signTypesRepository;
        }


        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_SIGNDOCUMET_VIEW + ", " + DataGlobals.ROLE_SIGNDOCUMET_EDIT))]
        public ActionResult Index()
        {
            if (Session["_idOrg"] == null) Session["_idOrg"] = getOrgByArmId();
            int curOrg = getIntCurrentEnterpriseId();

            Dictionary<string, object> query = new Dictionary<string, object>();

            query.Add("Organization.Id", curOrg);
            query.Add("[in]Id", ((Store.Core.Account.User)Session[DataGlobals.ACCOUNT_KEY]).ObjectsCSV);
            IList<StorageName> storageNames = storageNameRepository.GetByCriteria(query);

            SelectList storageNameList = new SelectList(storageNames, "Id", "Name", Session[DataGlobals.STORAGE_NAME_ID_LIST] != null ? (string)Session[DataGlobals.STORAGE_NAME_ID_LIST] : "-1");
            ViewData[DataGlobals.REFERENCE_STORAGE_SHOP_NAME] = storageNameList;
            if ((storageNames.Count > 0) && ((string)Session[DataGlobals.STORAGE_NAME_ID_LIST] == null))
            {
                Session[DataGlobals.STORAGE_NAME_ID_LIST] = storageNames[0].Id.ToString();
            }
            IList<SignDocTypes> signDocTypes = signDocTypesRepository.GetAll();
            ViewData[DataGlobals.REFERENCE_SIGNDOCTYPE] = signDocTypes;
            IList<SignTypes> signTypes = signTypesRepository.GetAll();
            ViewData[DataGlobals.REFERENCE_SIGNTYPE] = signTypes;

            IList<Organization> shops = organizationRepository.GetShopsByEnterprise(curOrg);
            ViewData[DataGlobals.REFERENCE_SHOP] = shops;

            return View(viewName);
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
        [HttpPost]
        public ActionResult _GetUnits(int idShop)
        {
            string idOrg = getCurrentEnterpriseId();
            int idEnterprise = System.Int32.Parse(idOrg);

            IList<Organization> units = organizationRepository.GetUnitByShop(idEnterprise, idShop);

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new SelectList(units, "Id", "Name")
            };
        }

        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_SIGNDOCUMET_EDIT))]
        public ActionResult _GetWorker(string text)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            Dictionary<string, object> orderParams = new Dictionary<string, object>();

            IList<Worker> workers = null;
            int tabn = -1;
            if (int.TryParse(text, out tabn))
                queryParams.Add("TabN", tabn);
            else
                queryParams.Add("Fio", text);
            queryParams.Add("RootOrganization", getIntCurrentEnterpriseId());
            orderParams.Add("Fio", ASC);
            workers = workerRepository.GetByLikeCriteria(queryParams, orderParams);
            return new JsonResult
            {
                Data = new SelectList(workers, "Id", "WorkerInfo")
            };
        }

        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_SUBSCRIPTION_EDIT))]
        public ActionResult _addOrEditRecord(int id, int unitId, int signTypeId, int  signDocTypeId,
                string prikaz, string fio, string workplaceName, int? tabn, int? workerId, int? ord, int storagenameId, int shopId)
        {
            SignDocumet signDocumet = signDocumetRepository.Get(id);
            //Если вставляется новая запись, то пытаемся найти запись с выбранным цехом
            if (signDocumet == null)
            {
                    signDocumet = new SignDocumet();
                    signDocumet.OrganizationId = getIntCurrentEnterpriseId();
                    signDocumet.StorageNameId = storagenameId;
                    signDocumet.ShopId = shopId;
            }

            if (unitId != null && unitId != 0)
            {
                signDocumet.Unit = organizationRepository.Get(unitId);
            }
            else
            {
                signDocumet.Unit = null;
            }
            SignTypes signType = signTypesRepository.Get(signTypeId);
            signDocumet.SignType = signType;
            signDocumet.CodeSign = signType.Code;
            signDocumet.NameSign = signType.Name;
            SignDocTypes signDocType = signDocTypesRepository.Get(signDocTypeId);
            signDocumet.SignDocType = signDocType;
            signDocumet.CodeDocumetn = signDocType.Code;
            signDocumet.Value = prikaz;
            if (workerId.HasValue && workerId != 0 &&  workerId != -1)
            {
                signDocumet.Worker = workerRepository.Get(workerId.Value);
                signDocumet.Tabn = signDocumet.Worker.TabN;
            }
            //if (tabn.HasValue) signDocumet.Tabn = tabn;
            if (fio != null)
                signDocumet.Fio = fio;
            else
                signDocumet.Fio = signDocumet.Worker.Fio;

            if (ord.HasValue) signDocumet.Ord = ord;
            signDocumet.WorkPlaceName = workplaceName;
            // сохраняем изменения
            signDocumetRepository.SaveOrUpdate(signDocumet);
            return null;
        }

        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_SUBSCRIPTION_EDIT))]
        public ActionResult _delRecord(int id) {
            SignDocumet signDocumet = signDocumetRepository.Get(id);
            signDocumetRepository.Delete(signDocumet);
            return null;
        }


        


        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_SIGNDOCUMET_VIEW + ", " + DataGlobals.ROLE_SIGNDOCUMET_EDIT))]
        public ActionResult _SelectSignDocumet(int storagenameId, int shopId, int? unitId)
        {
            IList<SignDocumet> list = new List<SignDocumet>();

            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("OrganizationId", getIntCurrentEnterpriseId());
            queryParams.Add("StorageNameId", storagenameId);
            if (shopId != null &&  shopId!=0 )
                queryParams.Add("ShopId", shopId);
            if (unitId != null)
                queryParams.Add("Unit.Id", unitId);
            
            //else
               //queryParams.Add("[]Unit", null);
            
           Dictionary<string, object> orderParams = new Dictionary<string, object>();
            orderParams.Add("StorageNameId", ASC);
            if (shopId != null &&  shopId!=0 ) orderParams.Add("ShopId", ASC);
            if (unitId != null) orderParams.Add("Unit", ASC);
            orderParams.Add("CodeDocumetn", ASC);
            orderParams.Add("NameSign", ASC);
            orderParams.Add("Ord", ASC);


            IList<SignDocumet> signDocumets = signDocumetRepository.GetByLikeCriteria(queryParams, orderParams);
            foreach (SignDocumet item in signDocumets)
            {
                SignDocumet newItem = rebuildSignDocumet(item);
                list.Add(newItem);
            }
            return View(new GridModel(list));
        }
    }
}
