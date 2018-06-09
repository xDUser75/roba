using System.Web.Mvc;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using SharpArch.Core.PersistenceSupport;
using Store.Core;
using Telerik.Web.Mvc;
using System.Collections.Generic;
using Store.Data;
using System;

namespace Store.Web.Controllers
{
    [HandleError]
    public class WorkerCardInsController : ViewedController
    {
        private readonly CriteriaRepository<WorkerCardHead> workerCardRepository;
        private readonly CriteriaRepository<NormaOrganization> normaOrganizationRepository;
        private readonly CriteriaRepository<Worker> workerRepository;
        private readonly CriteriaRepository<WorkerWorkplace> workerWorkplaceRepository;
        private readonly CriteriaRepository<NormaContent> normaContentRepository;
        private readonly CriteriaRepository<Storage> storageRepository;
        private readonly CriteriaRepository<OperType> operTypeRepository;
        private readonly CriteriaRepository<Organization> organizationRepository;
        private readonly CriteriaRepository<Operation> operationRepository;
        private readonly CriteriaRepository<NomBodyPartSize> nomBodyPartSizeRepository;

        public WorkerCardInsController(CriteriaRepository<WorkerCardHead> workerCardRepository,
            CriteriaRepository<NormaOrganization> normaOrganizationRepository,
            CriteriaRepository<Worker> workerRepository,
            CriteriaRepository<NormaContent> normaContentRepository,
            CriteriaRepository<WorkerWorkplace> workerWorkplaceRepository,
            CriteriaRepository<Storage> storageRepository,
            CriteriaRepository<OperType> operTypeRepository,
            CriteriaRepository<Organization> organizationRepository,
            CriteriaRepository<Operation> operationRepository,
            CriteriaRepository<NomBodyPartSize> nomBodyPartSizeRepository)
        {
            Check.Require(workerCardRepository != null, "workerWorkplaceRepository may not be null");
            this.workerCardRepository = workerCardRepository;
            this.normaOrganizationRepository = normaOrganizationRepository;
            this.workerRepository = workerRepository;
            this.normaContentRepository = normaContentRepository;
            this.workerWorkplaceRepository = workerWorkplaceRepository;
            this.storageRepository = storageRepository;
            this.operTypeRepository = operTypeRepository;
            this.organizationRepository = organizationRepository;
            this.operationRepository = operationRepository;
            this.nomBodyPartSizeRepository = nomBodyPartSizeRepository;
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_EDIT + ", " + DataGlobals.ROLE_WORKER_VIEW))]
        public ActionResult Index()
        {
            return View(viewName);
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_EDIT + ", " + DataGlobals.ROLE_WORKER_VIEW))]
        public ActionResult Select_Worker(int? workerWorkplaceId)
        {
            IList<WorkerWorkplace> workerWorkplaces = new List<WorkerWorkplace>();
            if (workerWorkplaceId != null)
            {
                WorkerWorkplace workerWorkplace = workerWorkplaceRepository.Get(workerWorkplaceId.Value);
                workerWorkplaces.Add(rebuildWorkerWorkplace(workerWorkplace));
            }
            return View(new GridModel(workerWorkplaces));
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_EDIT + ", " + DataGlobals.ROLE_WORKER_VIEW))]
        public ActionResult Select(int? workerWorkplaceId)
        {
            IList<WorkerNorma> workerNormas = new List<WorkerNorma>();
            WorkerNorma workerNorma = null;

            if (workerWorkplaceId != null)
            {
                WorkerWorkplace workerWorkplace = workerWorkplaceRepository.Get(workerWorkplaceId.Value);
                //ViewData["worker"] = workerWorkplace.Worker;

                
                //HttpContext.Cache.Insert("workerWorkplaceId", workerWorkplaceId);
                Session.Add("workerWorkplaceId", workerWorkplaceId);

                Dictionary<string, object> query = new Dictionary<string, object>();
                query.Add("WorkerWorkplace", workerWorkplace);
                WorkerCardHead workerCardHead = workerCardRepository.FindOne(query);

                foreach (WorkerCardContent workerCardContent in workerCardHead.WorkerCardContents)
                {
                    workerNorma = new WorkerNorma();
                    workerNorma.StorageId = workerCardContent.Storage.Id;
                    workerNorma.StorageInfo = workerCardContent.Storage.StorageInfo;
                    workerNorma.ReceptionDate = workerCardContent.Operation.OperDate;
                    workerNorma.PresentQuantity = workerCardContent.Quantity;
                    workerNormas.Add(workerNorma);
                }

            }
            return View(new GridModel(workerNormas));
        }

        private int getQuantityNomOnStorageByNomGroup(int nomGroupId)
        {
            int result = 0;
            Dictionary<string, object> query = new Dictionary<string, object>();
            query.Add("Nomenclature.NomGroup.Id", nomGroupId);
            IList<Storage> storages = storageRepository.GetByCriteria(query);
            foreach (Storage curStorage in storages)
            {
                result += curStorage.Quantity;
            }
            return result;
        }

        private Storage getStorageForWorkerNorma(NormaContent normaContent, Worker worker)
        //private Storage getStorageForWorkerNorma(NormaContent normaContent, IList<NomBodyPartSize> NomBodyPartSizes)
        {
            //Storage storage = null;
            List<Storage> storages = null;
            NomBodyPart normaBodyPart = normaContent.NomGroup.NomBodyPart;
            NomBodyPartSize workerBodyPartSize = null;

            foreach (var item in worker.NomBodyPartSizes)
            //foreach (var item in NomBodyPartSizes)
            {
                if (item.NomBodyPart.Id == normaBodyPart.Id)
                {
                    workerBodyPartSize = item;
                    break;
                }
            }

            Dictionary<string, object> query = new Dictionary<string, object>();
            query.Add("StorageName.Organization.Id", int.Parse(getCurrentEnterpriseId()));
            query.Add("Nomenclature.NomGroup.Id", normaContent.NomGroup.Id);
            query.Add("NomBodyPartSize.NomBodyPart.Id", normaBodyPart.Id);
            if (workerBodyPartSize != null && DataGlobals.SIZ_SIZE_ID != normaBodyPart.Id)
                query.Add("NomBodyPartSize.SizeNumber", workerBodyPartSize.SizeNumber);
            if (DataGlobals.CLOTH_SIZE_ID == normaBodyPart.Id)
                query.Add("Growth.SizeNumber", worker.Growth);
            storages = (List<Storage>)storageRepository.GetByCriteria(query);

            if (storages == null || storages.Count == 0)
            {
                query.Clear();
                query.Add("StorageName.Organization.Id", int.Parse(getCurrentEnterpriseId()));
                foreach (var normaNomGroup in normaContent.NormaNomGroups)
                {
                    query.Add("Nomenclature.NomGroup.Id", normaNomGroup.NomGroup.Id);
                    storages.AddRange(storageRepository.GetByLikeCriteria(query));
                    query.Remove("Nomenclature.NomGroup.Id");

                }

            }

            if (storages == null || storages.Count == 0)
            {
                storages.Add(new Storage());
            }

            return storages[0];
        }

        private WorkerCardContent getWorkerCard(IList<NormaNomGroup> normaNomGroups, IList<WorkerCardContent> workerCards)
        {
            int workerNomGroupId;
            WorkerCardContent workerCard = null;
            WorkerCardContent outWorkerCard = null;
            NomGroup nomGroup;

            foreach (NormaNomGroup item in normaNomGroups)
            {
                nomGroup = item.NomGroup;
                foreach (WorkerCardContent curWorkerCard in workerCards)
                {
                    workerNomGroupId = curWorkerCard.Storage.Nomenclature.NomGroup.Id;
                    if (nomGroup.Id == workerNomGroupId)
                    {
                        workerCard = rebuildWorkerCard(curWorkerCard);

                        //workerCards.Remove(curWorkerCard);

                        if (outWorkerCard == null)
                            outWorkerCard = workerCard;
                        else
                            //outWorkerCard.Operation = workerCard.Operation;
                            outWorkerCard.Quantity += workerCard.Quantity;
                    }
                }
            }
            //if (outWorkerCard == null)
            //{
            //    outWorkerCard = new WorkerCardContent();
            //    outWorkerCard.Storage = new Storage();
            //    outWorkerCard.Storage.Nomenclature = new Nomenclature();
            //}

            return outWorkerCard;
        }

        private WorkerCardContent rebuildWorkerCard(WorkerCardContent inWorkerCard)
        {
            WorkerCardContent outWorkerCard;
            Storage storage;
            Operation operation;

            storage = rebuildStorage(inWorkerCard.Storage);
            operation = rebuildOperation(inWorkerCard.Operation);

            outWorkerCard = new WorkerCardContent(inWorkerCard.Id);
            outWorkerCard.Storage = storage;
            outWorkerCard.Operation = operation;
            outWorkerCard.Quantity = inWorkerCard.Quantity;

            return outWorkerCard;
        }

        //[GridAction]
        //[Transaction]
        //[Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_EDIT + ", " + DataGlobals.ROLE_WORKER_VIEW))]
        //public ActionResult Select()
        //{
        //    //IList<Worker> workers = workerRepository.GetAll();
        //    //return View(new GridModel(workers));
        //    return getAllAndView();
        //}

        //[AcceptVerbs(HttpVerbs.Post)]
        //[GridAction]
        //[Transaction]
        //[Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_WORKER_EDIT))]
        ////public ActionResult Save(WorkerWorkplace workerWorkplace)
        //public ActionResult Save()
        //{
        //    WorkerWorkplace workerWorkplace = new WorkerWorkplace();
        //    if (TryUpdateModel(workerWorkplace))
        //        workplaceRepository.SaveOrUpdate(workerWorkplace);
        //    return getAllAndView();
        //    //return View(workers);
        //    //return RedirectToAction("Select");
        //}

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_WORKER_EDIT))]
//        public ActionResult Update(string id)
        //public ActionResult Update(WorkerNorma workerNorma)
        public ActionResult Update([Bind(Prefix = "updated")]IEnumerable<WorkerNorma> workerNormas)
        {
            //int workerWorkplaceId = (int)HttpContext.Cache.Get("workerWorkplaceId");
            int workerWorkplaceId = (int)Session["workerWorkplaceId"];

            if (workerNormas != null)
            {
                int i = -1;
                foreach (var workerNorma in workerNormas)
                {
                    i++; 
                    if (workerNorma.PutQuantity <= 0)
                        continue;
                    Dictionary<string, object> query = new Dictionary<string, object>();
                    query.Add("Id", workerWorkplaceId);
                    WorkerWorkplace workerWorkplace = workerWorkplaceRepository.Get(workerWorkplaceId);

                    //TryUpdateModel(workerNorma);

                    //WorkerCard workerCard = workerNorma.WorkerCard;
                    //WorkerCard workerCard = new WorkerCard();
                    query.Clear();
                    query.Add("WorkerWorkplace", workerWorkplace);

                    WorkerCardHead workerCardHead = workerCardRepository.FindOne(query);
                    WorkerCardContent workerCardContent = new WorkerCardContent();
                    if (workerCardHead == null)
                    {
                        workerCardHead = new WorkerCardHead();
                        workerCardHead.WorkerCardContents = new List<WorkerCardContent>();
                        workerCardHead.WorkerWorkplace = workerWorkplace;
                    }

                    workerCardContent.Quantity = workerNorma.PutQuantity;
                    //workerCardContent.Storage = workerNorma.WorkerCardContent.Storage;
                    if (workerNorma.StorageId == 0)
                        ModelState.AddModelError("updated[" + i + "].StorageName", "Выберите номенклатуру");
                    else
                    {
                        workerCardContent.Storage = storageRepository.Get(workerNorma.StorageId);
                        workerCardContent.WorkerCardHead = workerCardHead;

                        if (workerCardContent.Storage.Quantity - workerNorma.PutQuantity < 0)
                            ModelState.AddModelError("updated[" + i + "].StorageName", "На складе недостаточно кол-ва");

                        if (workerNorma.PresentQuantity + workerNorma.PutQuantity > workerNorma.NormaQuantity)
                            ModelState.AddModelError("updated[" + i + "].StorageName", "На руках есть номенклатура у которой не истек срок");

                    }

                    if (ModelState.IsValid)
                    {
                        //replaceIfExist(workerCardHead.WorkerCardContents, workerCardContent, workerNorma);

                        Operation oper = new Operation();
                        oper.OperDate = DateTime.Now;
                        oper.OperType = operTypeRepository.Get(DataGlobals.OPERATION_WORKER_IN);
                        oper.Organization = organizationRepository.Get(workerWorkplace.RootOrganization);
                        oper.Quantity = workerCardContent.Quantity;
                        oper.Storage = workerCardContent.Storage;
                        workerCardContent.Operation = oper;
                        oper.Storage.Quantity -= oper.Quantity;
                        oper.WorkerWorkplace = workerWorkplace;

                        operationRepository.SaveOrUpdate(oper);
                        workerCardRepository.SaveOrUpdate(workerCardHead);
                    }
                }
            }
            return Select(workerWorkplaceId);
        }

        //private void replaceIfExist(IList<WorkerCardContent> workerCardContents, WorkerCardContent workerCardContent, WorkerNorma workerNorma)
        //{
        //    bool isFind = false;
        //    foreach (WorkerCardContent item in workerCardContents)
        //    {
        //        if (item.Storage.Id == workerCardContent.Storage.Id)
        //        {
        //            isFind = true;
        //            if (workerNorma.PresentQuantity < 0)
        //            {
        //                Operation oper = new Operation();
        //                oper.OperDate = DateTime.Now;
        //                oper.OperType = operTypeRepository.Get(DataGlobals.OPERATION_WORKER_OUT);
        //                oper.Organization = organizationRepository.Get(item.WorkerCardHead.WorkerWorkplace.RootOrganization);
        //                oper.Quantity = workerCardContent.Quantity;
        //                oper.Storage = workerCardContent.Storage;
        //                oper.Storage.Quantity -= oper.Quantity;
        //                oper.WorkerWorkplace = item.WorkerCardHead.WorkerWorkplace;

        //                operationRepository.SaveOrUpdate(oper);

        //                workerCardContents[workerCardContents.IndexOf(item)].Quantity = workerCardContent.Quantity;
        //            }
        //            else
        //                workerCardContents[workerCardContents.IndexOf(item)].Quantity += workerCardContent.Quantity;

        //            workerCardContents[workerCardContents.IndexOf(item)].Storage = workerCardContent.Storage;
        //            workerCardContents[workerCardContents.IndexOf(item)].Operation = workerCardContent.Operation;
        //        }
        //    }
        //    if (!isFind)
        //        workerCardContents.Add(workerCardContent);
        //}

        //[AcceptVerbs(HttpVerbs.Post)]
        //[GridAction]
        //[Transaction]
        //[Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_WORKER_EDIT))]
        //public ActionResult Update(string id)
        //{
        //    WorkerWorkplace wwp = workplaceRepository.Get(int.Parse(id));
        //    if (TryUpdateModel(wwp))
        //    {
        //        //int workerId = int.Parse(Request.Params["Worker[Id]"]);
        //        int workerId = -1;
        //        string workerFio = Request.Params["Worker.Fio"];
        //        Worker worker = null;
        //        if (int.TryParse(workerFio, out workerId))
        //            worker = workerRepository.Get(workerId);

        //        //int orgId = int.Parse(Request.Params["Organization[Id]"]);
        //        int orgId = -1;
        //        string orgName = Request.Params["Organization.Name"];
        //        Organization org = null;
        //        if (int.TryParse(orgName, out orgId))
        //            org = organizationRepository.Get(orgId);

        //        WorkerWorkplace workplace = null;
        //        if (id != null)
        //            workplace = workplaceRepository.Get(int.Parse(id));
        //        else
        //            workplace = new WorkerWorkplace();
        //        if (worker != null)
        //            workplace.Worker = worker;
        //        if (org != null)
        //            workplace.Organization = org;
        //        workplaceRepository.SaveOrUpdate(workplace);
        //    }
        //    return getAllAndView();
        //    //return View(workers);
        //    //return RedirectToAction("Select");
        //}

        //[AcceptVerbs(HttpVerbs.Post)]
        //[GridAction]
        //[Transaction]
        //[Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_WORKER_EDIT))]
        ////public ActionResult Delete(WorkerWorkplace worker)
        //public ActionResult Delete(int id)
        //{
        //    //workplaceRepository.Delete(worker);
        //    WorkerWorkplace workerWorkplace = workplaceRepository.Get(id);
        //    workplaceRepository.Delete(workerWorkplace);
        //    return getAllAndView();
        //}

        //private ActionResult getAllAndView()
        //{
        //    Dictionary<string, object> queryParams = new Dictionary<string, object>();
        //    //queryParams.Add("Organization.Parent.Id", int.Parse("50005303"));
        //    queryParams.Add("Organization.Parent.Id", int.Parse("50004744"));

        //    IList<WorkerCard> workerCards = workerCardRepository.GetAll();


        //    IList<WorkerCard> newList = new List<WorkerCard>();
        //    foreach (WorkerCard current in workerCards)
        //    {
        //        //Organization organization = current.Organization;
        //        WorkerWorkplace workerWorkplace = new WorkerWorkplace(current.WorkerWorkplace.Id);
        //        workerWorkplace.Organization = current.WorkerWorkplace.Organization;
        //        workerWorkplace.Worker = current.WorkerWorkplace.Worker;

        //        //NormaOrganization normaOrganization = new NormaOrganization(current.NormaOrganization.Id);
        //        //normaOrganization.Norma = current.NormaOrganization.Norma;
        //        //normaOrganization.Organization = current.NormaOrganization.Organization;

        //        Operation operation = new Operation(current.Operation.Id);
        //        operation.DocNumber = current.Operation.DocNumber;
        //        operation.Motiv = current.Operation.Motiv;
        //        operation.OperDate = current.Operation.OperDate;
        //        operation.OperType = current.Operation.OperType;
        //        operation.Quantity = current.Operation.Quantity;
        //        operation.Storage = current.Operation.Storage;
        //        operation.Wear = current.Operation.Wear;

        //        WorkerCard workerCard = new WorkerCard(current.Id);
        //        workerCard.WorkerWorkplace = workerWorkplace;
        //        //workerCard.NormaOrganization = normaOrganization;
        //        workerCard.Operation = operation;

        //        newList.Add(workerCard);
        //    }

        //    //newList = workerWorkplaces;
        //    return View(new GridModel(newList));
        //}

        [HttpPost]
        public ActionResult _GetNomenclaturesOnStorage(string normaContentId, string text)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            
            List<Storage> storages = new List<Storage>();

            int code = -1;
            if (int.TryParse(text, out code))
                queryParams.Add("Nomenclature.ExternalCode", text);
            else
                queryParams.Add("Nomenclature.Name", text);
            queryParams.Add("StorageName.Organization.Id", int.Parse(getCurrentEnterpriseId()));

            if (normaContentId != null)
            {
                NormaContent normaContent = normaContentRepository.Get(int.Parse(normaContentId));

                foreach (var normaNomGroup in normaContent.NormaNomGroups)
                {
                    queryParams.Add("Nomenclature.NomGroup.Id", normaNomGroup.NomGroup.Id);
                    storages.AddRange(storageRepository.GetByLikeCriteria(queryParams));
                    queryParams.Remove("Nomenclature.NomGroup.Id");

                }
            }
            else
                storages.AddRange(storageRepository.GetByLikeCriteria(queryParams));
            
            return new JsonResult
            {
                Data = new SelectList(storages, "Id", "StorageInfo")
            };
        }

        [HttpPost]
        public ActionResult _GetNomenclatures(string nomGroupId, string text)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("Nomenclature.Name", text);
            //queryParams.Add("NomGroup.Id", int.Parse(nomGroupId));

            //IList<Nomenclature> nomenclatures = nomenRepository.GetByLikeCriteria(queryParams);
            IList<Nomenclature> nomenclatures = new List<Nomenclature>();
            IList<Storage> storages = storageRepository.GetByLikeCriteria(queryParams);
            foreach (Storage item in storages)
            {
                //Nomenclature nomenclature = new Nomenclature(item.Id);
                //nomenclature.Name = item.Nomenclature.Name;
                //nomenclatures.Add(nomenclature);
                nomenclatures.Add(item.Nomenclature);
            }

            return new JsonResult
            {
                Data = new SelectList(nomenclatures, "Id", "Name")
            };
        }

        [HttpPost]
        public ActionResult _GetWorkerWorkplaces(string text)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            int tabn = -1;
            if (int.TryParse(text, out tabn))
                queryParams.Add("Worker.TabN", tabn);
            else
                queryParams.Add("Worker.Fio", text);
            queryParams.Add("RootOrganization", int.Parse(getCurrentEnterpriseId()));
            queryParams.Add("IsActive", true);

            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            orderParams.Add("Worker.Fio", ASC);

            IList<WorkerWorkplace> workerWorkplaces = workerWorkplaceRepository.GetByLikeCriteria(queryParams, orderParams);

            return new JsonResult
            {
                Data = new SelectList(workerWorkplaces, "Id", "WorkplaceInfo")
            };
        }

        [HttpPost]
        public ActionResult _GetWorkers(string text)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("Fio", text);

            IList<Worker> workers = workerRepository.GetByLikeCriteria(queryParams);

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new SelectList(workers, "Id", "Fio")
            };
        }

        //[HttpPost]
        //public ActionResult _GetOrganizations(string text)
        //{
        //    Dictionary<string, object> queryParams = new Dictionary<string, object>();
        //    queryParams.Add("Name", text);

        //    IList<Organization> organizations = organizationRepository.GetByLikeCriteria(queryParams);

        //    return new JsonResult
        //    {
        //        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
        //        Data = new SelectList(organizations, "Id", "Name")
        //    };
        //}

        //public IList<T> RebuildEntity(IList<T> entity)
        //{
        //    IList<T> newList = new List<T>();
        //    return newList;
        //}

    }
}
