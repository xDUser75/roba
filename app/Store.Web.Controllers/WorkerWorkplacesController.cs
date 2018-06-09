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
    public class WorkerWorkplacesController : ViewedController
    {
        private readonly CriteriaRepository<WorkerWorkplace> workplaceRepository;
        private readonly CriteriaRepository<Organization> organizationRepository;
        private readonly CriteriaRepository<Worker> workerRepository;

        public WorkerWorkplacesController(CriteriaRepository<WorkerWorkplace> workplaceRepository,
            CriteriaRepository<Organization> organizationRepository,
            CriteriaRepository<Worker> workerRepository)
        {
            Check.Require(workplaceRepository != null, "workerWorkplaceRepository may not be null");
            this.workplaceRepository = workplaceRepository;
            this.organizationRepository = organizationRepository;
            this.workerRepository = workerRepository;
        }

        [GridAction]
        [Transaction]
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
            return View(viewName);
            //return View();
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_EDIT + ", " + DataGlobals.ROLE_WORKER_VIEW))]
        public ActionResult Select(string param)
        {
            //IList<Worker> workers = workerRepository.GetAll();
            //return View(new GridModel(workers));
            IList<WorkerWorkplace> newWorkerWorkplace = new List<WorkerWorkplace>();

            if (param != null && !"".Equals(param))
            {
                //HttpContext.Cache.Insert("workerWorkplaceParam", param);
                Session.Add("workerWorkplaceParam", param);

                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                int tabn = -1;
                if (int.TryParse(param, out tabn))
                    queryParams.Add("Worker.TabN", tabn);
                else
                    queryParams.Add("Worker.Fio", param);
                //queryParams.Add("Organization.Parent.Id", int.Parse("50005303"));
                queryParams.Add("Worker.RootOrganization", int.Parse(getCurrentEnterpriseId()));
                queryParams.Add("IsActive", true);

                Dictionary<string, object> orderParams = new Dictionary<string, object>();
                orderParams.Add("Worker.Fio", ASC);

                IList<WorkerWorkplace> workerWorkplaces = workplaceRepository.GetByLikeCriteria(queryParams, orderParams);

                foreach (WorkerWorkplace current in workerWorkplaces)
                {
                    //Worker worker = rebuildWorker(current.Worker);
                    //Organization organization = rebuildOrganization(current.Organization);
                    //WorkerWorkplace wwp = new WorkerWorkplace(current.Id);
                    //wwp.Worker = worker;
                    //wwp.Organization = organization;
                    //wwp.IsActive = current.IsActive;

                    WorkerWorkplace wwp = rebuildWorkerWorkplace(current);
                    newWorkerWorkplace.Add(wwp);
                }
            }
            //newList = workerWorkplaces;
            return View(new GridModel(newWorkerWorkplace));
            //return getAllAndView();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_WORKER_EDIT))]
        //public ActionResult Save(WorkerWorkplace workerWorkplace)
        public ActionResult Save()
        {
            WorkerWorkplace workerWorkplace = new WorkerWorkplace();
            if (TryUpdateModel(workerWorkplace))
            {
//                Organization rootOrg = workerWorkplace.Organization.RootOrganization;
                workerWorkplace.RootOrganization = workerWorkplace.Organization.RootOrganization;
                workplaceRepository.SaveOrUpdate(workerWorkplace);
            }
            //return getAllAndView();
            //return View(workers);
            //return Select((string)HttpContext.Cache.Get("workerWorkplaceParam"));
            return Select((string)Session["workerWorkplaceParam"]);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_WORKER_EDIT))]
        public ActionResult Update(string id)
        {
            WorkerWorkplace wwp = workplaceRepository.Get(int.Parse(id));
            if (TryUpdateModel(wwp))
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
            //return getAllAndView();
            //return View(workers);
            //return RedirectToAction("Select");
            return Select((string)Session["workerWorkplaceParam"]);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_WORKER_EDIT))]
        //public ActionResult Delete(WorkerWorkplace worker)
        public ActionResult Delete(int id)
        {
            //workplaceRepository.Delete(worker);
            WorkerWorkplace workerWorkplace = workplaceRepository.Get(id);
            workplaceRepository.Delete(workerWorkplace);
            //return getAllAndView();
            //return Select((string)HttpContext.Cache.Get("workerWorkplaceParam"));
            return Select((string)Session["workerWorkplaceParam"]);
        }

        //private ActionResult getAllAndView()
        //{
        //    Dictionary<string, object> queryParams = new Dictionary<string, object>();
        //    //queryParams.Add("Organization.Parent.Id", int.Parse("50005303"));
        //    queryParams.Add("Organization.Parent.Id", int.Parse("50004744"));
        //    //queryParams.Add("Organization.RootOrganization", int.Parse(getCurrentEnterpriseId()));
            
        //    IList<WorkerWorkplace> workerWorkplaces = workplaceRepository.GetByCriteria(queryParams);


        //    IList<WorkerWorkplace> newWorkerWorkplace = new List<WorkerWorkplace>();
        //    foreach (WorkerWorkplace current in workerWorkplaces)
        //    {
        //        Worker worker = rebuildWorker(current.Worker);
        //        Organization organization = rebuildOrganization(current.Organization);

        //        newWorkerWorkplace.Add(new WorkerWorkplace(current.Id, worker, organization));
        //    }

        //    //newList = workerWorkplaces;
        //    return View(new GridModel(newWorkerWorkplace));
        //}

        [HttpPost]
        public ActionResult _GetWorkers(string text)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            int tabn = -1;
            if (int.TryParse(text, out tabn))
                queryParams.Add("TabN", tabn);
            else
                queryParams.Add("Fio", text);
            queryParams.Add("RootOrganization", int.Parse(getCurrentEnterpriseId()));

            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            orderParams.Add("Fio", ASC);

            IList<Worker> workers = workerRepository.GetByLikeCriteria(queryParams, orderParams);

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new SelectList(workers, "Id", "WorkerInfo")
            };
        }

        [HttpPost]
        public ActionResult _GetOrganizations(string text)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("Name", text);
            queryParams.Add("IsWorkPlace", true);
            IList<Organization> organizations = organizationRepository.GetByLikeCriteria(queryParams);

            //return new JsonResult
            //{
            //    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            //    Data = new SelectList(organizations, "Id", "FullName")
            //};
            return new JsonResult
            {
                Data = new SelectList(organizations, "Id", "FullName")
            };
        }

        //public IList<T> RebuildEntity(IList<T> entity)
        //{
        //    IList<T> newList = new List<T>();
        //    return newList;
        //}

    }
}
