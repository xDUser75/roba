using System.Web.Mvc;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using Store.Core;
using Telerik.Web.Mvc;
using System.Collections.Generic;
using Store.Data;

namespace Store.Web.Controllers
{
    [HandleError]
    public class MatPersonController : ViewedController
    {
        private readonly CriteriaRepository<MatPersonCardHead> matPersonCardHeadRepository;
        private readonly CriteriaRepository<MatPersonCardContent> matPersonCardContentRepository;
        private readonly CriteriaRepository<Worker> workerRepository;
        private readonly StorageNameRepository storageNameRepository;
        private readonly OrganizationRepository organizationRepository;
        private readonly CriteriaRepository<WorkerWorkplace> workerWorkplaceRepository;


        public MatPersonController(
            CriteriaRepository<MatPersonCardHead> matPersonCardHeadRepository,
            CriteriaRepository<Worker> workerRepository,
            StorageNameRepository storageNameRepository,
            OrganizationRepository organizationRepository,
            CriteriaRepository<WorkerWorkplace> workerWorkplaceRepository,
            CriteriaRepository<MatPersonCardContent> matPersonCardContentRepository
            )
        {
            Check.Require(matPersonCardHeadRepository != null, "matPersonCardHeadRepository may not be null");
            this.matPersonCardHeadRepository = matPersonCardHeadRepository;
            this.workerRepository = workerRepository;
            this.organizationRepository = organizationRepository;
            this.workerWorkplaceRepository = workerWorkplaceRepository;
            this.storageNameRepository = storageNameRepository;
            this.matPersonCardContentRepository = matPersonCardContentRepository;

        }

        public ActionResult Index()
        {
            Dictionary<string, object> query = new Dictionary<string, object>();
            query.Add("Organization.Id", int.Parse(getCurrentEnterpriseId()));
            query.Add("[in]Id", ((Store.Core.Account.User)Session[DataGlobals.ACCOUNT_KEY]).ObjectsCSV);
            IList<StorageName> storageNames = storageNameRepository.GetByCriteria(query);
            SelectList storageNameList = new SelectList(storageNames, "Id", "Name", Session["storageNameId"] != null ? (string)Session["storageNameId"] : "-1");
            ViewData[DataGlobals.REFERENCE_STORAGE_SHOP_NAME] = storageNameList;
            return View(viewName);
        }

        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_MATPERSON_EDIT + ", " + DataGlobals.ROLE_MATPERSON_VIEW))]
        public ActionResult _SelectPerson(int StorageNameId)
        {
            Organization currentOrg=organizationRepository.Get(getIntCurrentEnterpriseId());
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("Organization", currentOrg);
            queryParams.Add("StorageName.Id", StorageNameId);
            queryParams.Add("IsActive", true);
            IList<MatPersonCardHead> persons = matPersonCardHeadRepository.GetByLikeCriteria(queryParams);
            List<MatPersonCardHead> model = new List<MatPersonCardHead>();
            List<string> excludeProperty = new List<string>();
            excludeProperty.Add("Organization");
            excludeProperty.Add("Worker.Organization");
            excludeProperty.Add("Worker.Growth");
            excludeProperty.Add("Worker.Sex");
            excludeProperty.Add("Worker.NomBodyPartSizes");
            excludeProperty.Add("Worker.WorkerGroup");
            excludeProperty.Add("Worker.WorkerCategory");
            
            foreach (var item in persons)
            {
                MatPersonCardHead resItem = rebuildMatPerson(item, excludeProperty);
                model.Add(resItem);
            };

            return View(new GridModel(model));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_MATPERSON_EDIT))]
        public ActionResult _DeletePerson(int workerId)
        {
            MatPersonCardHead person = matPersonCardHeadRepository.Get(workerId);
            if (person != null)
            {
                if (person.IsActive)
                {
                    Dictionary<string, object> queryParams = new Dictionary<string, object>();
                    queryParams.Add("MatPersonCardHead", person);
                    IList<MatPersonCardContent> list = matPersonCardContentRepository.GetByCriteria(queryParams);
                    int count = 0;
                    foreach (var item in list){
                        count = count+item.Quantity;
                    }
                    if (count > 0)
                    {
                        ModelState.AddModelError("", "У сотрудника есть на руках номенклатуры!");
                    }
                    else
                    {
                        person.IsActive = false;
                        matPersonCardHeadRepository.SaveOrUpdate(person);
                    }
                }
            }
            return View(new GridModel(new List<MatPersonCardHead>()));
        }

        [HttpPost]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_MATPERSON_EDIT + ", " + DataGlobals.ROLE_MATPERSON_VIEW))]
        public ActionResult _FindWorkerWorkPlace(string text)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            Dictionary<string, object> orderParams = new Dictionary<string, object>();

            IList<WorkerWorkplace> workerWorkplaces = null;
            int tabn = -1;
            if (int.TryParse(text, out tabn))
                queryParams.Add("Worker.TabN", tabn);
            else
                queryParams.Add("Worker.Fio", text);
            queryParams.Add("RootOrganization", int.Parse(getCurrentEnterpriseId()));
            queryParams.Add("IsActive", true);            
            orderParams.Add("Worker.Fio", ASC);

            workerWorkplaces = workerWorkplaceRepository.GetByLikeCriteria(queryParams, orderParams);

            return new JsonResult
            {
                Data = new SelectList(workerWorkplaces, "Id", "WorkplaceInfo")
            };
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_MATPERSON_EDIT))]
        public ActionResult _InsertPerson(int workerWorkplaceId, int StorageNameId)
        {
            Organization currentOrg = organizationRepository.Get(getIntCurrentEnterpriseId());
            WorkerWorkplace wp = workerWorkplaceRepository.Get(workerWorkplaceId);
            if (wp != null && wp.RootOrganization == currentOrg.Id)
            {
                StorageName currentStorageName = storageNameRepository.Get(StorageNameId);
                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                queryParams.Add("Organization", currentOrg);
                queryParams.Add("Worker", wp.Worker);
                queryParams.Add("StorageName", currentStorageName);
                MatPersonCardHead person = matPersonCardHeadRepository.FindOne(queryParams);
                if (person == null) {
                    person = new MatPersonCardHead();
                    person.Organization = currentOrg;
                    person.Worker = wp.Worker;
                }
                /*
                if (person.StorageName != currentStorageName)
                    ModelState.AddModelError("","Этот сотрудник уже числится на складе: " + currentStorageName.Name);
                else
                {*/
                    person.IsActive = true;
                    person.Department = wp.Organization.Parent;
                    person.StorageName = currentStorageName;
                    matPersonCardHeadRepository.SaveOrUpdate(person);
                //}
            }
            return View(new GridModel(new List<MatPersonCardHead>()));
        }
    }
}
