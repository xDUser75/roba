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
    public class WorkerCardReturnController : ViewedController
    {
        private readonly CriteriaRepository<WorkerCardHead> workerCardRepository;
        private readonly CriteriaRepository<WorkerCardContent> workerCardContentRepository;
        private readonly CriteriaRepository<NormaOrganization> normaOrganizationRepository;
        private readonly CriteriaRepository<Worker> workerRepository;
        private readonly CriteriaRepository<WorkerWorkplace> workerWorkplaceRepository;
        private readonly CriteriaRepository<NormaContent> normaContentRepository;
        private readonly CriteriaRepository<Storage> storageRepository;
        private readonly CriteriaRepository<StorageName> storageNameRepository;
        private readonly CriteriaRepository<OperType> operTypeRepository;
        private readonly CriteriaRepository<Organization> organizationRepository;
        private readonly OperationRepository operationRepository;
        private readonly CriteriaRepository<Motiv> motivRepository;
        private readonly CriteriaRepository<NomBodyPartSize> nomBodyPartSizeRepository;
        private readonly CriteriaRepository<Config> configRepository;
        private readonly RemainRepository remainsRepository;

        public WorkerCardReturnController(CriteriaRepository<WorkerCardHead> workerCardRepository,
            CriteriaRepository<WorkerCardContent> workerCardContentRepository,
            CriteriaRepository<NormaOrganization> normaOrganizationRepository,
            CriteriaRepository<Worker> workerRepository,
            CriteriaRepository<NormaContent> normaContentRepository,
            CriteriaRepository<WorkerWorkplace> workerWorkplaceRepository,
            CriteriaRepository<Storage> storageRepository,
            CriteriaRepository<StorageName> storageNameRepository,
            CriteriaRepository<OperType> operTypeRepository,
            CriteriaRepository<Organization> organizationRepository,
            OperationRepository operationRepository,
            CriteriaRepository<Motiv> motivRepository,
            CriteriaRepository<Config> configRepository,
            RemainRepository remainsRepository,
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
            this.storageNameRepository = storageNameRepository;
            this.operTypeRepository = operTypeRepository;
            this.organizationRepository = organizationRepository;
            this.operationRepository = operationRepository;
            this.motivRepository = motivRepository;
            this.configRepository = configRepository;
            this.remainsRepository = remainsRepository;
            this.nomBodyPartSizeRepository = nomBodyPartSizeRepository;
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_CARD_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_VIEW + ", " + DataGlobals.ROLE_WORKER_CARD_RETURN_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_RETURN_VIEW))]
        public ActionResult Index()
        {
            Dictionary<string, object> query = new Dictionary<string, object>();
            Dictionary<string, object> order = new Dictionary<string, object>();

            query.Add("OperType.Id", DataGlobals.OPERATION_WORKER_RETURN);
            order.Add("Id", DESC);

            ViewData[DataGlobals.REFERENCE_MOTIV] = motivRepository.GetByCriteria(query, order);

            query.Clear();
            query.Add("Organization.Id", int.Parse(getCurrentEnterpriseId()));
            query.Add("[in]Id", ((Store.Core.Account.User)Session[DataGlobals.ACCOUNT_KEY]).ObjectsCSV);
            IList<StorageName> storageNames = storageNameRepository.GetByCriteria(query);

            SelectList storageNameList = new SelectList(storageNames, "Id", "Name", Session["storageNameId"] != null ? (string)Session["storageNameId"] : "0");
            ViewData[DataGlobals.REFERENCE_STORAGE_SHOP_NAME] = storageNameList;            
            
            return View(viewName);
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_CARD_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_VIEW + ", " + DataGlobals.ROLE_WORKER_CARD_RETURN_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_RETURN_VIEW))]
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
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_CARD_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_VIEW + ", " + DataGlobals.ROLE_WORKER_CARD_RETURN_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_RETURN_VIEW))]
        public ActionResult Select(int? workerWorkplaceId, string workerWorkplaceText, string storageNameId)
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
                Session.Add("storageNameId", storageNameId);


                Dictionary<string, object> query = new Dictionary<string, object>();
                Dictionary<string, object> order = new Dictionary<string, object>();
                query.Add("WorkerWorkplace", workerWorkplace);
                WorkerCardHead workerCardHead = workerCardRepository.FindOne(query);

                if (workerCardHead != null)
                {
                    //query.Clear();
                    //order.Clear();
                    //query.Add("WorkerCardHead.Id", workerCardHead.Id);
                    //order.Add("Storage.Nomenclature.Name", ASC);
                    //workerCardHead.WorkerCardContents = workerCardContentRepository.GetByCriteria(query, order);
                    workerCardHead.WorkerCardContents = reorderWorkerCardContents(workerCardHead.WorkerCardContents);

                    foreach (WorkerCardContent workerCardContent in workerCardHead.WorkerCardContents)
                    {
                        //if (workerCardContent.Quantity <= 0)
                        //    continue;
                        StorageName storageName=storageNameRepository.Get(int.Parse(storageNameId)) ;
//                        if (workerCardContent.Storage.StorageName.Id.ToString() != storageNameId)
                        if (workerCardContent.Storage.StorageName.StorageNumber !=  storageName.StorageNumber)
                            continue;
                        if (workerCardContent.Quantity > 0)
                        {
                            workerNorma = new WorkerNorma();
                            //workerNorma.Storage = rebuildStorage(workerCardContent.Storage);
                            workerNorma.Id = workerCardContent.Id;
                            workerNorma.StorageId = workerCardContent.Storage.Id;
                            workerNorma.StorageNumber = workerCardContent.Storage.StorageName.StorageNumber;
                            workerNorma.StorageInfo = workerCardContent.Storage.StorageInfo;
                            workerNorma.Wear = workerCardContent.Storage.Wear;
                            //Возвращают в основном б/у
                            //workerNorma.Wear = "50";
                            workerNorma.Wear = "";
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

            foreach (WorkerCardContent item in workerCardHead.WorkerCardContents)
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
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_WORKER_CARD_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_RETURN_EDIT))]
//        public ActionResult Update(string id)
        //public ActionResult Update(WorkerNorma workerNorma)
        public ActionResult Update([Bind(Prefix = "updated")]IEnumerable<WorkerNorma> workerNormas)
        {
            //int workerWorkplaceId = (int)HttpContext.Cache.Get("workerWorkplaceId");
            int workerWorkplaceId = (int)Session["workerWorkplaceId"];
            string docNumber = operationRepository.GetNextDocNumber(DataGlobals.OPERATION_WORKER_RETURN, getCurrentEnterpriseId());
            int? motivId = null;
            //DateTime? docDate = null;
            DateTime? operDate = null;
            // перове число текущего месяца
            DateTime firstDay = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            // первое число предыдущего месяца
            DateTime minDate = firstDay.AddMonths(-1);

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
                        if (workerNorma.OperDate != null)
                        {
                            operDate = workerNorma.OperDate.Value;

                            if (DateTime.Today > operDate)
                                operDate = operDate.Value.AddHours(21).AddMinutes(0).AddSeconds(0);
                            else
                                operDate = operDate.Value.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddSeconds(DateTime.Now.Second);
                        }
                        motivId = workerNorma.MotivId;
                    }

                    if (workerNorma.PutQuantity <= 0)
                        continue;

                    //NormaContent normaContent = normaContentRepository.Get(workerNorma.NormaContentId);

                    WorkerCardContent workerCardContentPresent = getWorkerCard(workerNorma.Id, workerCardHead);

                    //workerCardContent.Storage = workerNorma.WorkerCardContent.Storage;
                    //                    if (workerCardContentPresent.Quantity != workerNorma.PutQuantity)
                    //                        ModelState.AddModelError("updated[" + i + "].StorageName", "Списать можно только столько сколько есть на руках");


                    //if (operDate != null && (DateTime.Now < operDate || minDate > operDate))
                      if (operDate != null && DateTime.Now < operDate )
                        ModelState.AddModelError(operDate + ": ", "Дата операции должна быть не больше текущей даты и не меньше 1-го числа предыдущего месяца");
                    if (operDate < workerNorma.ReceptionDate)
                        ModelState.AddModelError(operDate + ": ", "Дата возврата не может быть меньше даты выдачи");
                    if (firstDay > operDate)
                    {
                        // проверка на закрытие периода
                        string periodIsClosed = getConfigParamValue(configRepository, "periodIsClosed", getCurrentEnterpriseId());
                        string periodDateStr = getConfigParamValue(configRepository, "periodDate", getCurrentEnterpriseId());
                        DateTime periodDate;
                        DateTime.TryParseExact(periodDateStr, DataGlobals.DATE_FORMAT_FULL_YEAR, null, System.Globalization.DateTimeStyles.None, out periodDate);
                        if (periodIsClosed != null && periodIsClosed.Equals("1") && periodDate > operDate)
                            ModelState.AddModelError(operDate + ": ", "Период закрыт для изменений");
                    }
                    if (workerNorma.PutQuantity <= 0)
                        ModelState.AddModelError( workerCardContentPresent.Storage.Nomenclature.Name, "Вернуть можно кол-во больше 0");

                    if (workerNorma.PutQuantity > workerCardContentPresent.Quantity)
                        ModelState.AddModelError(workerCardContentPresent.Storage.Nomenclature.Name, "Вернуть можно не больше чем есть на руках");

                    
                    if (workerNorma.Wear == null)
                        ModelState.AddModelError(workerCardContentPresent.Storage.Nomenclature.Name, "Введите износ");

                    
                    //if (string.IsNullOrEmpty(docNumber) || docDate == null)
                    //    ModelState.AddModelError("updated[" + i + "].StorageName", "Номер и дата документа должны быть заполнены");
                }

                if (ModelState.IsValid)
                {
                    foreach (var workerNorma in workerNormas)
                    {
                        if (workerNorma.PutQuantity <= 0)
                            continue;

                        WorkerCardContent workerCardContentPresent = getWorkerCard(workerNorma.Id, workerCardHead);                        
                        //workerCardContentPresent.Storage = null;
                        
                        Operation oper = new Operation();
                        oper.OperDate = operDate.Value;
                        oper.OperType = operTypeRepository.Get(DataGlobals.OPERATION_WORKER_RETURN);
                        oper.Organization = organizationRepository.Get(workerWorkplace.RootOrganization);
                        oper.Quantity = workerNorma.PutQuantity;
                        //oper.Storage = workerCardContentPresent.Storage;
                        oper.Wear = workerNorma.Wear;
                        oper.DocNumber = docNumber;
                        oper.DocDate = oper.OperDate;
                        oper.Motiv = motivRepository.Get(motivId.Value);
                        oper.WorkerWorkplace = workerWorkplace;
                        oper.GiveOperation = workerCardContentPresent.GiveOperation;

                        //workerCardContentPresent.Operation = oper;
                        //workerCardContentPresent.Quantity -= oper.Quantity;
                        if (workerNorma.PutQuantity == workerCardContentPresent.Quantity)
                        {
                            workerCardHead.WorkerCardContents.Remove(workerCardContentPresent);
                        }
//                        workerCardContentRepository.Delete(workerCardContentPresent);

                        //workerCardRepository.SaveOrUpdate(workerCardHead);

                        // если не утиль, ищем позицию с таким износом на складе
                        //if (int.Parse(workerNorma.Wear) > 0)
                        //{
                            query.Clear();
                            query.Add("Nomenclature", workerCardContentPresent.Storage.Nomenclature);
                            if (workerCardContentPresent.Storage.NomBodyPartSize != null)
                                query.Add("NomBodyPartSize", workerCardContentPresent.Storage.NomBodyPartSize);
                            if (workerCardContentPresent.Storage.Growth != null)
                                query.Add("Growth", workerCardContentPresent.Storage.Growth);
                            query.Add("StorageName", workerCardContentPresent.Storage.StorageName);
                            if (int.Parse(workerNorma.Wear) > 0)
                                query.Add("Wear", workerNorma.Wear);
                            IList<Storage> storages = storageRepository.GetByCriteria(query);

                            Storage storage = null;
                            if (storages.Count > 0)
                            {
                                storage = storages[0];
                                if (int.Parse(workerNorma.Wear) > 0)
                                    storage.Quantity += workerNorma.PutQuantity;
                            }
                            else
                            {
                                storage = new Storage();
                                storage.Growth = workerCardContentPresent.Storage.Growth;
                                storage.NomBodyPartSize = workerCardContentPresent.Storage.NomBodyPartSize;
                                storage.Nomenclature = workerCardContentPresent.Storage.Nomenclature;
                                storage.StorageName = workerCardContentPresent.Storage.StorageName;
                                storage.Wear = workerNorma.Wear;
                                if (int.Parse(workerNorma.Wear) > 0)
                                    storage.Quantity = workerNorma.PutQuantity;
                                else
                                    storage.Quantity = 0;
                            }
                            storageRepository.SaveOrUpdate(storage);
                            oper.Storage = storage;
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
                                wcc.EndDate = oper.OperDate;
                                wcc.WorkerCardHead = workerCardContentPresent.WorkerCardHead;
                                wcc.Storage = workerCardContentPresent.Storage;
                                wcc.Operation = workerCardContentPresent.Operation;
                                wcc.OperReturn = oper;
                                wcc.NormaContent = workerCardContentPresent.NormaContent;
                                wcc.GiveOperation = workerCardContentPresent.GiveOperation;

                                workerCardContentRepository.SaveOrUpdate(wcc);
                            }
                            workerCardContentRepository.SaveOrUpdate(workerCardContentPresent);

                            if (firstDay > operDate)
                            {
                                //пересчитываем остатки
                                rebuildRemaind(operationRepository, remainsRepository, storage, minDate, firstDay.AddSeconds(-1), firstDay);
                            }
                        //}
                    }
                }
            }
            return Select(workerWorkplaceId, (string)Session["workerWorkplaceText"], (string)Session["storageNameId"]);
        }


    }
}
