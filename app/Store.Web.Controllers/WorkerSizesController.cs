using System.Web.Mvc;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using SharpArch.Core.PersistenceSupport;
using Store.Core;
using Telerik.Web.Mvc.Extensions;
using Store.Data;
using Telerik.Web.Mvc;
using Store.Core.RepositoryInterfaces;
using System.Collections.Generic;
using System.Web;
using NHibernate.Criterion;

namespace Store.Web.Controllers
{
    [HandleError]
    public class WorkerSizesController : ViewedController
    {
        private readonly CriteriaRepository<WorkerSize> workerSizesRepository;
        private readonly CriteriaRepository<Worker> workerRepository;
        private readonly NomBodyPartRepository nomBodyPartRepository;
        private readonly CriteriaRepository<NomBodyPartSize> nomBodyPartSizeRepository;

        public WorkerSizesController(CriteriaRepository<WorkerSize> workerSizesRepository,
            CriteriaRepository<Worker> wokerRepository,
            NomBodyPartRepository nomBodyPartRepository,
            CriteriaRepository<NomBodyPartSize> nomBodyPartSizeRepository)
        {
            Check.Require(workerSizesRepository != null, "workerSizesRepository may not be null");
            this.workerSizesRepository = workerSizesRepository;
            this.workerRepository = wokerRepository;
            this.nomBodyPartRepository = nomBodyPartRepository;
            this.nomBodyPartSizeRepository = nomBodyPartSizeRepository;
        }

        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_SIZE_EDIT + ", " + DataGlobals.ROLE_WORKER_SIZE_VIEW))]
        public ActionResult _Select_NombodyPartSize(string id, string idSize)
        {
            int ID = System.Int32.Parse(id);
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("NomBodyPart.Id", ID);

            IEnumerable<NomBodyPartSize> nomBodySize = nomBodyPartSizeRepository.FindAll(queryParams);
            IList<SelectListItem> model = new List<SelectListItem>();
            foreach (var item in nomBodySize)
            {
                model.Add(new SelectListItem { Text = item.SizeNumber.ToString(), Value = item.Id.ToString(), Selected = (item.Id.ToString() == idSize) });
            };

            return new JsonResult
            {
                Data = model
            };
        }

        public ActionResult ServerEditTemplates()
        {
            return View(viewName, workerSizesRepository.GetAll());
        }

        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_SIZE_EDIT + ", " + DataGlobals.ROLE_WORKER_SIZE_VIEW))]
        public ActionResult Index()
        {
            //HttpContext.Cache.Remove("WorkerId");
            Session.Remove("WorkerId");
            PopulateReference();
            return View(viewName);
        }


        private void PopulateReference()
        {
            IList<NomBodyPart> n = nomBodyPartRepository.GetAll();
            string arrayJavaScript = "";
            foreach (var item in n)
            {
                arrayJavaScript = arrayJavaScript + item.Id + ",";
            };

            arrayJavaScript = arrayJavaScript.Substring(0, arrayJavaScript.Length - 1);
            ViewData["dropDownListCount"] = arrayJavaScript;
            ViewData[DataGlobals.REFERENCE_NOM_BODY_PART] = n;
        }

        [GridAction]
        [AcceptVerbs(HttpVerbs.Post)]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_WORKER_SIZE_EDIT))]
        public ActionResult _Update_WorkerSizes(string id, string oldId)
        {
            //string workerId = HttpContext.Cache.Get("WorkerId").ToString();
            string workerId = Session["WorkerId"].ToString();
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("Worker.Id", int.Parse(workerId));
            queryParams.Add("Id", int.Parse(oldId));
            WorkerSize ws = workerSizesRepository.FindOne(queryParams);
            int nomBodyPart = System.Int32.Parse(id);
            NomBodyPartSize nbps = nomBodyPartSizeRepository.Get(nomBodyPart);
            ws.NomBodyPartSize = nbps;
            workerSizesRepository.SaveOrUpdate(ws);
            return new JsonResult();
        }

        [GridAction]
        [AcceptVerbs(HttpVerbs.Post)]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN  + ", " + DataGlobals.ROLE_WORKER_SIZE_EDIT))]
        public ActionResult _Insert_WorkerSizes(string id)
        {
            int nomBodyPart = System.Int32.Parse(id);
            NomBodyPartSize nbps = nomBodyPartSizeRepository.Get(nomBodyPart);
            //string workerId = HttpContext.Cache.Get("WorkerId").ToString();
            string workerId = Session["WorkerId"].ToString();
            Worker w = workerRepository.Get(int.Parse(workerId));
            WorkerSize sw = new WorkerSize();
            sw.Worker = w;
            sw.NomBodyPartSize = nbps;
            sw.IsActive = true;
            workerSizesRepository.SaveOrUpdate(sw);
            return new JsonResult();
        }

        [GridAction]
        [AcceptVerbs(HttpVerbs.Post)]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_WORKER_SIZE_EDIT))]
        public ActionResult _Delete_WorkerSizes(string id)
        {
            //string workerId = HttpContext.Cache.Get("WorkerId").ToString();
            string workerId = Session["WorkerId"].ToString();
            WorkerSize sw = workerSizesRepository.Get(System.Int32.Parse(id));
            workerSizesRepository.Delete(sw);
            return new JsonResult();
        }

        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_SIZE_EDIT + ", " + DataGlobals.ROLE_WORKER_SIZE_VIEW))]
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
                PopulateReference();
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
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_SIZE_EDIT + ", " + DataGlobals.ROLE_WORKER_SIZE_VIEW))]
        public ActionResult _SelectionClientSide_Workers(string tabn, string fio)
        {
            //HttpContext.Cache.Remove("WorkerId");
            Session.Remove("WorkerId");
            PopulateReference();
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

            if (queryParams.Count>0)
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
     }
}
