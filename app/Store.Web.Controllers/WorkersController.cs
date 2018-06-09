using System.Web.Mvc;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using SharpArch.Core.PersistenceSupport;
using Store.Core;
using Telerik.Web.Mvc;
using System.Collections.Generic;
using Store.Data;
using System.Web;

namespace Store.Web.Controllers
{
    [HandleError]
    public class WorkersController : ViewedController
    {
        private readonly CriteriaRepository<Worker> workerRepository;
        private readonly SexRepository sexRepository;
        private readonly NomBodyPartRepository nomBodyPartRepository;
        private readonly CriteriaRepository<NomBodyPartSize> nomBodyPartSizeRepository;
        private readonly CriteriaRepository<WorkerSize> workerSizesRepository;
        private readonly CriteriaRepository<WorkerWorkplace> workplaceRepository;
        private readonly OrganizationRepository organizationRepository;
        private readonly CriteriaRepository<NormaOrganization> normaOrganizationRepository;
        private readonly CriteriaRepository<StorageName> storageNameRepository;

        public WorkersController(CriteriaRepository<Worker> workerRepository,
                                 CriteriaRepository<WorkerSize> workerSizesRepository,
                                 SexRepository sexRepository,
                                 NomBodyPartRepository nomBodyPartRepository,
                                 CriteriaRepository<NomBodyPartSize> nomBodyPartSizeRepository,
                                 CriteriaRepository<WorkerWorkplace> workplaceRepository,
                                 OrganizationRepository organizationRepository,
                                 CriteriaRepository<NormaOrganization> normaOrganizationRepository,
                                 CriteriaRepository<StorageName> storageNameRepository)

        {
            Check.Require(workerRepository != null, "workerRepository may not be null");
            this.workerRepository = workerRepository;
            this.workerSizesRepository = workerSizesRepository;
            this.sexRepository = sexRepository;
            this.nomBodyPartRepository = nomBodyPartRepository;
            this.nomBodyPartSizeRepository = nomBodyPartSizeRepository;
            this.workplaceRepository = workplaceRepository;
            this.organizationRepository = organizationRepository;
            this.normaOrganizationRepository = normaOrganizationRepository;
            this.storageNameRepository = storageNameRepository;            
        }


        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_EDIT + ", " + DataGlobals.ROLE_WORKER_VIEW))]
        public ActionResult _Select_NombodyPartSize(string id, string idSize)
        {
            int ID = System.Int32.Parse(id);
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("NomBodyPart.Id", ID);
            queryParams.Add("IsActive", true);

            IEnumerable<NomBodyPartSize> nomBodySize = nomBodyPartSizeRepository.FindAll(queryParams);
            IList<SelectListItem> model = new List<SelectListItem>();
            SelectListItem li = new SelectListItem();
            //li.Value=
            foreach (var item in nomBodySize)
            {
                model.Add(new SelectListItem { Text = item.SizeNumber.ToString(), Value = item.Id.ToString(), Selected = (item.Id.ToString() == idSize) });

            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_EDIT + ", " + DataGlobals.ROLE_WORKER_VIEW))]
        public ActionResult Index()
        {
            //IList<Worker> workers = workerRepository.GetAll();
            //Dictionary<string, object> isActive = new Dictionary<string, object>();
            //isActive.Add("IsActive", null);
            //IList<Worker> workers = workerRepository.FindAll(isActive);
            //return View(workers);
            //return View(new GridModel(workers));
            //string viewName = ControllerEnums.getViewName(ControllerContext);
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("[!in]Id", DataGlobals.EXCLUDE_ID_SIZ);
            IList<NomBodyPart> n = nomBodyPartRepository.GetByLikeCriteria(queryParams);
            string arrayJavaScript = "";
            foreach (var item in n)
            {
                arrayJavaScript = arrayJavaScript + item.Id + ",";
            };

            arrayJavaScript = arrayJavaScript.Substring(0, arrayJavaScript.Length - 1);
            ViewData["dropDownListCount"] = arrayJavaScript;
            ViewData[DataGlobals.REFERENCE_NOM_BODY_PART] = n;

            ViewData[DataGlobals.REFERENCE_SEX] = sexRepository.GetRowForWorkers();
            Session.Remove("WorkerId");
            return View(viewName);
            //return View();
        }

        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_EDIT + ", " + DataGlobals.ROLE_WORKER_VIEW))]
        public ActionResult Select(string param)
        {
            //IList<Worker> workers = workerRepository.GetAll();
            //return View(new GridModel(workers));
            //return getAllAndView(param);
            IList<Worker> newWorkers = new List<Worker>();

            if (param != null && !"".Equals(param))
            {
                //HttpContext.Cache.Insert("workersParam", param);
                Session.Add("workersParam", param);

                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                int tabn = -1;
                if (int.TryParse(param, out tabn))
                    queryParams.Add("TabN", tabn);
                else
                    queryParams.Add("Fio", param);
                queryParams.Add("RootOrganization", int.Parse(getCurrentEnterpriseId()));

                Dictionary<string, object> orderParams = new Dictionary<string, object>();
                orderParams.Add("Fio", ASC);

                IList<Worker> workers = workerRepository.GetByLikeCriteria(queryParams, orderParams);
                foreach (Worker curWorker in workers)
                {
                    Worker worker = rebuildWorker(curWorker);
                    newWorkers.Add(worker);
                }
                ViewData[DataGlobals.REFERENCE_SEX] = sexRepository.GetRowForWorkers();
            }
            return View(new GridModel(newWorkers));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_WORKER_EDIT))]
        //public ActionResult Save(Worker worker)
        public ActionResult Save(int? Id)
        {
            Worker worker = null;
            if (Id == null)
            {
                worker = new Worker();
                worker.RootOrganization = int.Parse(getCurrentEnterpriseId());
            }
            else
                worker = workerRepository.Get(Id.Value);
            if (TryUpdateModel(worker, null, null, new[] {"RootOrganization", "Growth", "NomBodyPartSizes"}))
                workerRepository.SaveOrUpdate(worker);
            //return getAllAndView((string)HttpContext.Cache.Get("workersParam"));
            //return View(workers);
            //return Select((string)HttpContext.Cache.Get("workersParam"));
            return Select((string)Session["workersParam"]);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_WORKER_EDIT))]
        public ActionResult Delete(Worker worker)
        {
            worker.Sex = null;
            workerRepository.Delete(worker);
            //return Select((string)HttpContext.Cache.Get("workersParam"));
            return Select((string)Session["workersParam"]);
        }


        [GridAction]
        [AcceptVerbs(HttpVerbs.Post)]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_WORKER_EDIT))]
        public ActionResult _Update_WorkerSizes(string id, string oldId, string workerId)
        {
            //string workerId = HttpContext.Cache.Get("WorkerId").ToString();
           // string workerId = Session["WorkerId"].ToString();
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("Worker.Id", int.Parse(workerId));
            queryParams.Add("Id", int.Parse(oldId));
            WorkerSize ws = workerSizesRepository.FindOne(queryParams);
            int nomBodyPart = System.Int32.Parse(id);
            NomBodyPartSize nbps = nomBodyPartSizeRepository.Get(nomBodyPart);
            ws.NomBodyPartSize = nbps;
            ws.NomBodyPart = nbps.NomBodyPart;
            workerSizesRepository.SaveOrUpdate(ws);
            return new JsonResult();
        }

        [GridAction]
        [AcceptVerbs(HttpVerbs.Post)]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_WORKER_EDIT))]
        public ActionResult _Insert_WorkerSizes(string id, string workerId)
        {
            int nomBodyPart = System.Int32.Parse(id);
            NomBodyPartSize nbps = nomBodyPartSizeRepository.Get(nomBodyPart);
            nbps = rebuildNomBodyPartSize(nbps);
            //string workerId = HttpContext.Cache.Get("WorkerId").ToString();
//            string workerId = Session["WorkerId"].ToString();
            Worker w = workerRepository.Get(int.Parse(workerId));
            WorkerSize sw = new WorkerSize();
            sw.Worker = w;
            sw.NomBodyPartSize = nbps;
            sw.NomBodyPart = nbps.NomBodyPart;
            sw.IsActive = true;
            workerSizesRepository.SaveOrUpdate(sw);
            return new JsonResult();
        }

        [GridAction]
        [AcceptVerbs(HttpVerbs.Post)]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_WORKER_EDIT))]
        public ActionResult _Delete_WorkerSizes(string id)
        {
            //string workerId = HttpContext.Cache.Get("WorkerId").ToString();
            string workerId = Session["WorkerId"].ToString();
            WorkerSize sw = workerSizesRepository.Get(System.Int32.Parse(id));
            workerSizesRepository.Delete(sw);
            return new JsonResult();
        }

        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_EDIT + ", " + DataGlobals.ROLE_WORKER_VIEW))]
        public ActionResult _SelectionClientSide_WorkerSizes(string id)
        {
            List<WorkerSize> model = new List<WorkerSize>();
            int idWorker = System.Int32.Parse(id);
            if (idWorker > 0)
            {
                //HttpContext.Cache.Insert("WorkerId",id);
                Session.Add("WorkerId", id);
                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                queryParams.Add("Worker.Id", idWorker);
                IEnumerable<WorkerSize> workerSize = workerSizesRepository.FindAll(queryParams);
                foreach (var item in workerSize)
                {
                    model.Add(rebuildWorkerSize(item));
                };
            }
            return View(new GridModel<WorkerSize>
            {
                Data = model
            });
        }

        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_EDIT + ", " + DataGlobals.ROLE_WORKER_VIEW))]
        public ActionResult _SelectionClientSide_Workers(string tabn, string fio)
        {
            //HttpContext.Cache.Remove("WorkerId");
            Session.Remove("WorkerId");
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            if (tabn.Length > 0)
            {
                int tbn = System.Int32.Parse(tabn);
                queryParams.Add("TabN", tbn);
                orderParams.Add("TabN", ASC);
            }
            if (fio != null)
            {
                queryParams.Add("Fio", fio);
                orderParams.Add("Fio", ASC);
            }

            if (queryParams.Count > 0)
            {
                IList<Worker> model = new List<Worker>();
                IEnumerable<Worker> workers = workerRepository.GetByLikeCriteria(queryParams, orderParams);
                foreach (var item in workers)
                {
                    model.Add(rebuildWorker(item));
                };


                return View(new GridModel<Worker>
                {
                    Data = model
                });
            }

            return View(viewName);
        }


        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_EDIT + ", " + DataGlobals.ROLE_WORKER_VIEW))]
        public ActionResult SelectWorkerWorplace(int? param)
        {
            IList<WorkerWorkplace> newWorkerWorkplace = new List<WorkerWorkplace>();

            if (param != null && param>0)
            {
                Session.Add("workerWorkplaceParam", param);
                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                queryParams.Add("Worker.id", param);
                queryParams.Add("Worker.RootOrganization", int.Parse(getCurrentEnterpriseId()));
                queryParams.Add("IsActive", true);

                Dictionary<string, object> orderParams = new Dictionary<string, object>();
                //orderParams.Add("IsActive", DESC);
                orderParams.Add("BeginDate", DESC);
                orderParams.Add("Worker.Fio", ASC);

                IList<WorkerWorkplace> workerWorkplaces = workplaceRepository.GetByLikeCriteria(queryParams, orderParams);

                foreach (WorkerWorkplace current in workerWorkplaces)
                {
                    WorkerWorkplace wwp = rebuildWorkerWorkplace(current);
                        

                    newWorkerWorkplace.Add(wwp);
                }
            }
            return View(new GridModel(newWorkerWorkplace));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_WORKER_EDIT))]
        //public ActionResult Save(WorkerWorkplace workerWorkplace)
        public ActionResult SaveWorkerWorplace()
        {
            WorkerWorkplace workerWorkplace = new WorkerWorkplace();
            workerWorkplace.Worker = workerRepository.Get((int)Session["workerWorkplaceParam"]);
            workerWorkplace.Organization = organizationRepository.Get(int.Parse(Request["Organization"]));
            workerWorkplace.RootOrganization = workerWorkplace.Organization.RootOrganization;
            workerWorkplace.IsActive = bool.Parse(Request["IsActive"]);
            workplaceRepository.SaveOrUpdate(workerWorkplace);
            return SelectWorkerWorplace((int)Session["workerWorkplaceParam"]);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_WORKER_EDIT))]
        public ActionResult UpdateWorkerWorplace(string id)
        {
            WorkerWorkplace wwp = workplaceRepository.Get(int.Parse(id));

            if (TryUpdateModel(wwp, new string[] { "Organization", "RootOrganization", "IsActive" }))
            {
                //int workerId = int.Parse(Request.Params["Worker[Id]"]);
                //int workerId = -1;
                //string workerFio = Request.Params["Worker.Fio"];
                //Worker worker = null;
                //if (int.TryParse(workerFio, out workerId))
                //    worker = workerRepository.Get(workerId);

                ////int orgId = int.Parse(Request.Params["Organization[Id]"]);
                //int orgId = -1;
                //string orgName = Request.Params["Organization.Name"];
                //Organization org = null;
                //if (int.TryParse(orgName, out orgId))
                //    org = organizationRepository.Get(orgId);

                //WorkerWorkplace workplace = null;
                //if (id != null)
                //    workplace = workplaceRepository.Get(int.Parse(id));
                //else
                //    workplace = new WorkerWorkplace();
                //if (worker != null)
                //    workplace.Worker = worker;
                //if (org != null)
                //    workplace.Organization = org;
                workplaceRepository.SaveOrUpdate(wwp);
            }
            return SelectWorkerWorplace((int)Session["workerWorkplaceParam"]);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_WORKER_EDIT))]
        //public ActionResult Delete(WorkerWorkplace worker)
        public ActionResult DeleteWorkerWorplace(int id)
        {
            WorkerWorkplace workerWorkplace = workplaceRepository.Get(id);
            workplaceRepository.Delete(workerWorkplace);
            return SelectWorkerWorplace((int)Session["workerWorkplaceParam"]);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        //public ActionResult Save(Worker worker)
        public ActionResult _GetNormaIdByOrganization(int Id) {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("Organization.Id", Id);
            NormaOrganization no = normaOrganizationRepository.FindOne(queryParams);
            if (no != null)
            {
                HttpResponseBase Response = ControllerContext.HttpContext.Response;
                Response.Write(no.Norma.Id);
            }
            return null; 
        }

    }
}
