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
    public class WorkerCardOutsController : ViewedController
    {
        private readonly CriteriaRepository<WorkerCardHead> workerCardRepository;
        private readonly CriteriaRepository<WorkerCardContent> workerCardContentRepository;
        private readonly CriteriaRepository<NormaOrganization> normaOrganizationRepository;
        private readonly CriteriaRepository<Worker> workerRepository;
        private readonly CriteriaRepository<WorkerWorkplace> workerWorkplaceRepository;
        private readonly CriteriaRepository<NormaContent> normaContentRepository;
        private readonly CriteriaRepository<Storage> storageRepository;
        private readonly CriteriaRepository<OperType> operTypeRepository;
        private readonly CriteriaRepository<Organization> organizationRepository;
        private readonly CriteriaRepository<Operation> operationRepository;
        private readonly CriteriaRepository<NomBodyPartSize> nomBodyPartSizeRepository;
        private readonly CriteriaRepository<Motiv> motivRepository;
        private readonly CriteriaRepository<Cause> causeRepository;
        private readonly CriteriaRepository<Config> configRepository;

        public WorkerCardOutsController(CriteriaRepository<WorkerCardHead> workerCardRepository,
            CriteriaRepository<WorkerCardContent> workerCardContentRepository,
            CriteriaRepository<NormaOrganization> normaOrganizationRepository,
            CriteriaRepository<Worker> workerRepository,
            CriteriaRepository<NormaContent> normaContentRepository,
            CriteriaRepository<WorkerWorkplace> workerWorkplaceRepository,
            CriteriaRepository<Storage> storageRepository,
            CriteriaRepository<OperType> operTypeRepository,
            CriteriaRepository<Organization> organizationRepository,
            CriteriaRepository<Operation> operationRepository,
            CriteriaRepository<Motiv> motivRepository,
            CriteriaRepository<Cause> causeRepository,
            CriteriaRepository<Config> configRepository,
            CriteriaRepository<NomBodyPartSize> nomBodyPartSizeRepository)
        {
            Check.Require(workerCardRepository != null, "workerWorkplaceRepository may not be null");
            this.workerCardContentRepository = workerCardContentRepository;
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
            this.motivRepository = motivRepository;
            this.causeRepository = causeRepository;
            this.configRepository = configRepository;

        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_CARD_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_VIEW))]
        public ActionResult Index()
        {
            
            Dictionary<string, object> query = new Dictionary<string, object>();
            Dictionary<string, object> order = new Dictionary<string, object>();

            String workerOperations = "'" + DataGlobals.OPERATION_WORKER_OUT ;
            // Сначала хотела дать эту привилегию только администраторам, но потом оказалось, что она нужна кладовщикам
            //if (HttpContext.User.IsInRole(DataGlobals.ROLE_ADMIN))
                workerOperations = workerOperations + "'," + "'" + DataGlobals.OPERATION_WORKER_OUT_TIME + "'";
            query.Add("[in]Id", workerOperations);
            order.Add("Id", ASC);
            ViewData[DataGlobals.REFERENCE_OPER_TYPE] = operTypeRepository.GetByCriteria(query, order);
            return View(viewName);
        }

        public ActionResult Select_Motivs(int? operTypeId)
        {

            if (operTypeId==null) 
                operTypeId=DataGlobals.OPERATION_WORKER_OUT;
            Dictionary<string, object> query = new Dictionary<string, object>();
            Dictionary<string, object> order = new Dictionary<string, object>();

            query.Add("OperType.Id", operTypeId);
            order.Add("Id", ASC);

            IList<Motiv> motivs = motivRepository.GetByCriteria(query, order);
             return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new SelectList(motivs, "Id", "Name")
            };
        }
        public ActionResult Select_Causes(int? motivId)
        {

            if (motivId == null)
                motivId = DataGlobals.CAUSE_OPERATION_WORKER_OUT;
            Dictionary<string, object> query = new Dictionary<string, object>();
            Dictionary<string, object> order = new Dictionary<string, object>();

            query.Add("Motiv.Id", motivId);
            order.Add("Id", ASC);

            IList<Cause> causes =causeRepository.GetByCriteria(query, order);
            foreach (Cause cause in causes) {
                cause.Name = cause.Id + "-" + cause.Name;
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new SelectList(causes, "Id", "Name")
            };
        }
        
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_CARD_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_VIEW
            + ", " + DataGlobals.ROLE_WORKER_CARD_OUT_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_OUT_VIEW))]
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
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_CARD_OUT_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_OUT_VIEW + ", " + DataGlobals.ROLE_WORKER_CARD_VIEW + ", " + DataGlobals.ROLE_WORKER_CARD_EDIT))]
        public ActionResult Select(int? workerWorkplaceId, string workerWorkplaceText)
        {
            IList<WorkerNorma> workerNormas = new List<WorkerNorma>();
            WorkerNorma workerNorma = null;

            // Убрали запоминание табельных. Кладовщики путаются
            //if (workerWorkplaceId == null && Session["workerWorkplaceId"] != null)
            //    workerWorkplaceId = (int)Session["workerWorkplaceId"];
            if (workerWorkplaceId != null)
            {
                WorkerWorkplace workerWorkplace = workerWorkplaceRepository.Get(workerWorkplaceId.Value);
                //ViewData["worker"] = workerWorkplace.Worker;


                Session.Add("workerWorkplaceId", workerWorkplaceId);
                Session.Add("workerWorkplaceText", workerWorkplaceText);

                Dictionary<string, object> query = new Dictionary<string, object>();
                query.Add("WorkerWorkplace", workerWorkplace);
                WorkerCardHead workerCardHead = workerCardRepository.FindOne(query);

                if (workerCardHead != null)
                {
                    workerCardHead.WorkerCardContents = reorderWorkerCardContents(workerCardHead.WorkerCardContents);

                    foreach (WorkerCardContent workerCardContent in workerCardHead.WorkerCardContents)
                    {
                        //if (workerCardContent.Quantity <= 0)
                        //    continue;
                        if (workerCardContent.Quantity > 0)
                        {
                            workerNorma = new WorkerNorma();
                            //workerNorma.Storage = rebuildStorage(workerCardContent.Storage);
                            workerNorma.Id = workerCardContent.Id;
                            workerNorma.StorageId = workerCardContent.Storage.Id;
                            workerNorma.StorageNumber = workerCardContent.Storage.StorageName.StorageNumber;
                            workerNorma.StorageInfo = workerCardContent.Storage.StorageInfo;
//                            workerNorma.ReceptionDate = workerCardContent.Operation.OperDate;
                            workerNorma.ReceptionDate = workerCardContent.StartDate;
                            workerNorma.PresentQuantity = workerCardContent.Quantity;
                            //workerNorma.PutQuantity = workerNorma.PresentQuantity;
                            workerNorma.PutQuantity = 0;
                            workerNormas.Add(workerNorma);
                        }
                    }
                }

            }
            return View(new GridModel(workerNormas));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_WORKER_CARD_OUT_EDIT))]
        public ActionResult CancelOper()
        {
            int workerWorkplaceId = (int)Session["workerWorkplaceId"];

            Dictionary<string, object> query = new Dictionary<string, object>();
            Dictionary<string, object> order = new Dictionary<string, object>();
            query.Add("WorkerWorkplace.Id", workerWorkplaceId);
            query.Add("OperType.Id", DataGlobals.OPERATION_WORKER_OUT);
            query.Add("OperDate", DateTime.Today);

            IList<Operation> operations = operationRepository.GetByLikeCriteria(query);

            string lastDocNumber = "-1";
            foreach (var item in operations)
            {
                if (int.Parse(item.DocNumber) > int.Parse(lastDocNumber))
                    lastDocNumber = item.DocNumber;
            }

            query.Clear();
            query.Add("WorkerWorkplace.Id", workerWorkplaceId);
            WorkerCardHead workerCardHead = workerCardRepository.GetByCriteria(query)[0];

            WorkerCardContent workerCardContent = null;
            foreach (var item in operations)
            {
                if (item.DocNumber != lastDocNumber)
                    continue;

                // ищем операцию по которой выдавалось
                query.Clear();
                order.Clear();
                query.Add("WorkerWorkplace.Id", workerWorkplaceId);
                query.Add("Storage.Id", item.Storage.Id);
                query.Add("OperType.Id", DataGlobals.OPERATION_WORKER_IN);
                order.Add("OperDate", DESC);
                Operation operation = operationRepository.GetByLikeCriteria(query, order)[0];

                workerCardContent = new WorkerCardContent();
                workerCardContent.WorkerCardHead = workerCardHead;
                workerCardContent.Operation = operation;
                workerCardContent.Storage = item.Storage;
                workerCardContent.Quantity = item.Quantity;

                workerCardHead.WorkerCardContents.Add(workerCardContent);

                workerCardContentRepository.SaveOrUpdate(workerCardContent);

                operationRepository.Delete(item);

            }

            workerCardRepository.SaveOrUpdate(workerCardHead);

            //return Index();
            //return RedirectToAction("Index");
            //return Select(workerWorkplaceId, (string)Session["workerWorkplaceText"], (string)Session["storageNameId"] );
            return null;
        }

        //private int getQuantityNomOnStorageByNomGroup(int nomGroupId)
        //{
        //    int result = 0;
        //    Dictionary<string, object> query = new Dictionary<string, object>();
        //    query.Add("Nomenclature.NomGroup.Id", nomGroupId);
        //    IList<Storage> storages = storageRepository.GetByCriteria(query);
        //    foreach (Storage curStorage in storages)
        //    {
        //        result += curStorage.Quantity;
        //    }
        //    return result;
        //}

        //private Storage getStorageForWorkerNorma(NormaContent normaContent, Worker worker)
        ////private Storage getStorageForWorkerNorma(NormaContent normaContent, IList<NomBodyPartSize> NomBodyPartSizes)
        //{
        //    //Storage storage = null;
        //    List<Storage> storages = null;
        //    NomBodyPart normaBodyPart = normaContent.NomGroup.NomBodyPart;
        //    NomBodyPartSize workerBodyPartSize = null;

        //    foreach (var item in worker.NomBodyPartSizes)
        //    //foreach (var item in NomBodyPartSizes)
        //    {
        //        if (item.NomBodyPart.Id == normaBodyPart.Id)
        //        {
        //            workerBodyPartSize = item;
        //            break;
        //        }
        //    }

        //    Dictionary<string, object> query = new Dictionary<string, object>();
        //    query.Add("StorageName.Organization.Id", int.Parse(getCurrentEnterpriseId()));
        //    query.Add("Nomenclature.NomGroup.Id", normaContent.NomGroup.Id);
        //    query.Add("NomBodyPartSize.NomBodyPart.Id", normaBodyPart.Id);
        //    if (workerBodyPartSize != null && DataGlobals.SIZ_SIZE_ID != normaBodyPart.Id)
        //        query.Add("NomBodyPartSize.SizeNumber", workerBodyPartSize.SizeNumber);
        //    if (DataGlobals.CLOTH_SIZE_ID == normaBodyPart.Id)
        //        query.Add("Growth.SizeNumber", worker.Growth);
        //    storages = (List<Storage>)storageRepository.GetByCriteria(query);

        //    if (storages == null || storages.Count == 0)
        //    {
        //        query.Clear();
        //        query.Add("StorageName.Organization.Id", int.Parse(getCurrentEnterpriseId()));
        //        foreach (var normaNomGroup in normaContent.NormaNomGroups)
        //        {
        //            query.Add("Nomenclature.NomGroup.Id", normaNomGroup.NomGroup.Id);
        //            storages.AddRange(storageRepository.GetByLikeCriteria(query));
        //            query.Remove("Nomenclature.NomGroup.Id");

        //        }

        //    }

        //    if (storages == null || storages.Count == 0)
        //    {
        //        storages.Add(new Storage());
        //    }

        //    return storages[0];
        //}

        private WorkerCardContent getWorkerCard(int workerCardContentId, WorkerCardHead workerCardHead)
        {
            WorkerCardContent outWorkerCard = null;

            foreach (WorkerCardContent item in workerCardHead.getActiveWorkerCardContent())
            {
                if (workerCardContentId == item.Id)
                {
                    outWorkerCard = item;
                    break;
                }
            }

            return outWorkerCard;
        }

        //private WorkerCardContent rebuildWorkerCard(WorkerCardContent inWorkerCard)
        //{
        //    WorkerCardContent outWorkerCard;
        //    Storage storage;
        //    Operation operation;

        //    storage = rebuildStorage(inWorkerCard.Storage);
        //    operation = rebuildOperation(inWorkerCard.Operation);

        //    outWorkerCard = new WorkerCardContent(inWorkerCard.Id);
        //    outWorkerCard.Storage = storage;
        //    outWorkerCard.Operation = operation;
        //    outWorkerCard.Quantity = inWorkerCard.Quantity;

        //    return outWorkerCard;
        //}

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_WORKER_CARD_OUT_EDIT))]
//        public ActionResult Update(string id)
        //public ActionResult Update(WorkerNorma workerNorma)
        public ActionResult Update([Bind(Prefix = "updated")]IEnumerable<WorkerNorma> workerNormas)
        {
            //int workerWorkplaceId = (int)HttpContext.Cache.Get("workerWorkplaceId");
            int workerWorkplaceId = (int)Session["workerWorkplaceId"];
            string docNumber = null;
            DateTime? docDate = null;
            OperType operType = null;
            DateTime? operDate = null;
            Motiv motiv = null;
            Cause cause = null;


            Dictionary<string, object> query = new Dictionary<string, object>();
            query.Add("Id", workerWorkplaceId);
            WorkerWorkplace workerWorkplace = workerWorkplaceRepository.Get(workerWorkplaceId);

            query.Clear();
            query.Add("WorkerWorkplace", workerWorkplace);

            WorkerCardHead workerCardHead = workerCardRepository.FindOne(query);

            if (workerNormas != null)
            {
                int i = -1;
                foreach (var workerNorma in workerNormas)
                {
                    i++;
                    if (i == 0)
                    {
                        docNumber = workerNorma.DocNumber;
                        //DateTime docDate = DateTime.ParseExact(workerNorma.DocDate, DataGlobals.DATE_FORMAT, null);
                        docDate = workerNorma.DocDate;
                        operType = operTypeRepository.Get(workerNorma.OperTypeId);
                        motiv = motivRepository.Get(workerNorma.MotivId);
                        query.Clear();
                        query.Add("Id", workerNorma.CauseId);

                        cause = causeRepository.FindOne(query);

                        operDate = workerNorma.DocDate;

                        if (DateTime.Today > operDate)
                            operDate = operDate.Value.AddHours(21).AddMinutes(0).AddSeconds(0);
                        else
                            operDate = operDate.Value.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddSeconds(DateTime.Now.Second);

                    }

                    if (workerNorma.PutQuantity <= 0)
                        continue;

                    WorkerCardContent workerCardContentPresent = getWorkerCard(workerNorma.Id, workerCardHead);

                    if (workerNorma.PutQuantity <= 0)
                        ModelState.AddModelError(workerCardContentPresent.Storage.Nomenclature.Name, "Списать можно кол-во больше 0");

                    if (workerNorma.PutQuantity > workerCardContentPresent.Quantity)
                        ModelState.AddModelError(workerCardContentPresent.Storage.Nomenclature.Name, "Списать можно не больше чем есть на руках");


                    if (string.IsNullOrEmpty(docNumber) || docDate == null)
                        ModelState.AddModelError("updated[" + i + "].StorageName", "Номер и дата документа должны быть заполнены");

                    string periodDateStr = getConfigParamValue(configRepository, "periodDate", getCurrentEnterpriseId());
                    string periodIsClosed = getConfigParamValue(configRepository, "periodIsClosed", getCurrentEnterpriseId());
                    DateTime periodDate;
                    DateTime.TryParseExact(periodDateStr, DataGlobals.DATE_FORMAT_FULL_YEAR, null, System.Globalization.DateTimeStyles.None, out periodDate);
                    // проверка на закрытие периода
                    if (periodIsClosed != null && periodIsClosed.Equals("1") && periodDate > operDate)
                        ModelState.AddModelError(operDate + ": ", "Период закрыт для изменений");

                }

                if (ModelState.IsValid)
                {
                    foreach (var workerNorma in workerNormas)
                    {
                        if (workerNorma.PutQuantity <= 0)
                            continue;

                        WorkerCardContent workerCardContentPresent = getWorkerCard(workerNorma.Id, workerCardHead);
                        
                        Operation oper = new Operation();
                        oper.OperDate = operDate.Value;
                        oper.OperType = operTypeRepository.Get(DataGlobals.OPERATION_WORKER_OUT);
                        oper.Organization = organizationRepository.Get(workerWorkplace.RootOrganization);
                        oper.Quantity = workerNorma.PutQuantity;
                        oper.Storage = workerCardContentPresent.Storage;
                        oper.DocNumber = docNumber;
                        oper.DocDate = docDate.Value;
                        oper.WorkerWorkplace = workerWorkplace;
                        oper.Motiv = motiv;
                        oper.Cause = cause;
                        oper.OperType = operType;
                        oper.GiveOperation = workerCardContentPresent.GiveOperation;

                        //workerCardContentPresent.Operation = oper;
                        //workerCardContentPresent.Quantity -= oper.Quantity;
                        if (workerNorma.PutQuantity == workerCardContentPresent.Quantity)
                        {
                            workerCardHead.WorkerCardContents.Remove(workerCardContentPresent);
                        }
//Теперь позиции на карточки не удаляем
//                        workerCardContentRepository.Delete(workerCardContentPresent);

                        operationRepository.SaveOrUpdate(oper);
                        if (workerNorma.PutQuantity == workerCardContentPresent.Quantity)
                        {
                            workerCardContentPresent.OperReturn = oper;
                            workerCardContentPresent.Quantity = 0;
                            workerCardContentPresent.EndDate = oper.OperDate;
                        }
                        else
                        {
                            workerCardContentPresent.Quantity = workerCardContentPresent.Quantity - workerNorma.PutQuantity;
                            WorkerCardContent wcc = new WorkerCardContent();
                            wcc.Quantity = 0;
                            wcc.StartDate = workerCardContentPresent.StartDate;
                            wcc.EndDate = operDate.Value;
                            wcc.WorkerCardHead = workerCardContentPresent.WorkerCardHead;
                            wcc.Storage = workerCardContentPresent.Storage;
                            wcc.Operation = workerCardContentPresent.Operation;
                            wcc.OperReturn = oper;
                            wcc.NormaContent = workerCardContentPresent.NormaContent;
                            wcc.GiveOperation = workerCardContentPresent.GiveOperation;
                            workerCardContentRepository.SaveOrUpdate(wcc);
                        }
                        workerCardContentRepository.SaveOrUpdate(workerCardContentPresent);

                        //workerCardRepository.SaveOrUpdate(workerCardHead);
                    }
                }
            }
            return Select(workerWorkplaceId, (string)Session["workerWorkplaceText"]);
        }


    }
}
