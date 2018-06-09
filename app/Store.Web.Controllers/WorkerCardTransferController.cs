using System.Web.Mvc;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using SharpArch.Core.PersistenceSupport;
using Store.Core;
using Telerik.Web.Mvc;
using System.Collections.Generic;
using Store.Data;
using System;
using System.Text;
using System.Reflection;

namespace Store.Web.Controllers
{
    public class ResultState : Object
    {
        public static string ERROR = "Error";
        public static string OK = "Ok";
        private IList<string> _Message = new List<string>();
        public string Status { get; set; }
        public string Message {
            get
            {
                string result = "";
                foreach (var item in _Message)
                {
                    result = result + item + "\n";
                }
                return result;
            }
        }

        public void setMessage(string value)
        {
            this._Message.Add(value);
        }

        public bool isError()
        {
            if (this.Status == null) return false;
            return this.Status.ToUpper().Contains(ERROR);
        }
    }
    [HandleError]
    public class WorkerCardTransferController : ViewedController
    {
        private readonly CriteriaRepository<WorkerCardHead> workerCardRepository;
        private readonly CriteriaRepository<WorkerCardContent> workerCardContentRepository;
        private readonly CriteriaRepository<NormaOrganization> normaOrganizationRepository;
        private readonly CriteriaRepository<Worker> workerRepository;
        private readonly CriteriaRepository<WorkerWorkplace> workerWorkplaceRepository;
        private readonly CriteriaRepository<NormaContent> normaContentRepository;
        private readonly CriteriaRepository<Storage> storageRepository;
        private readonly StorageNameRepository storageNameRepository;
        private readonly CriteriaRepository<OperType> operTypeRepository;
        private readonly CriteriaRepository<Organization> organizationRepository;
        private readonly OperationRepository operationRepository;
        private readonly RemainRepository remainsRepository;
        private readonly CriteriaRepository<NomGroup> nomGroupRepository;
        private readonly CriteriaRepository<Config> configRepository;
        private readonly CriteriaRepository<Motiv> motivRepository;

        public WorkerCardTransferController(CriteriaRepository<WorkerCardHead> workerCardRepository,
            CriteriaRepository<WorkerCardContent> workerCardContentRepository,
            CriteriaRepository<NormaOrganization> normaOrganizationRepository,
            CriteriaRepository<Worker> workerRepository,
            CriteriaRepository<NormaContent> normaContentRepository,
            CriteriaRepository<WorkerWorkplace> workerWorkplaceRepository,
            CriteriaRepository<Storage> storageRepository,
            StorageNameRepository storageNameRepository,
            CriteriaRepository<OperType> operTypeRepository,
            CriteriaRepository<Organization> organizationRepository,
            OperationRepository operationRepository,
            RemainRepository remainsRepository,
            CriteriaRepository<Config> configRepository,
            CriteriaRepository<NomGroup> nomGroupRepository,
            CriteriaRepository<Motiv> motiveRepository)
        {
            Check.Require(workerCardRepository != null, "workerWorkplaceRepository may not be null");
            this.workerCardRepository = workerCardRepository;
            this.workerCardContentRepository = workerCardContentRepository;
            this.normaOrganizationRepository = normaOrganizationRepository;
            this.workerRepository = workerRepository;
            this.normaContentRepository = normaContentRepository;
            this.workerWorkplaceRepository = workerWorkplaceRepository;
            this.storageRepository = storageRepository;
            this.storageNameRepository = storageNameRepository;
            this.operTypeRepository = operTypeRepository;
            this.organizationRepository = organizationRepository;
            this.operationRepository = operationRepository;
            this.remainsRepository = remainsRepository;
            this.nomGroupRepository = nomGroupRepository;
            this.configRepository = configRepository;
            this.motivRepository = motiveRepository;
        }

        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_WORKER_CARD_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_RETURN_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_OUT_EDIT))]
        public ActionResult Index()
        {
            Dictionary<string, object> query = new Dictionary<string, object>();
            query.Add("Organization.Id", int.Parse(getCurrentEnterpriseId()));
            query.Add("[in]Id", ((Store.Core.Account.User)Session[DataGlobals.ACCOUNT_KEY]).ObjectsCSV);
            IList<StorageName> storageNames = storageNameRepository.GetByCriteria(query);
            SelectList storageNameList = new SelectList(storageNames, "Id", "Name", Session["storageNameId"] != null ? (string)Session["storageNameId"] : "0");
            ViewData[DataGlobals.REFERENCE_STORAGE_SHOP_NAME] = storageNameList;
            return View(viewName);
        }

        private NomenclatureSimple getEmptyNomenclature(IList<NomenclatureSimple> nomenclatures, int groupId){
            NomenclatureSimple ret = null;
            foreach (var item in nomenclatures) {
                if ((item.GroupId == groupId) && (item.NameId == 0)) 
                    return item;
            }
            return ret;
        }

        [Transaction]
        private IList<NomenclatureSimple> getNomenclaturesForWorkplace(int workerWorkplaceId, bool isActive)
        {
            IList<NomenclatureSimple> nomenclatures = new List<NomenclatureSimple>();
            IList<WorkerWorkplace> workerWorkplaces = new List<WorkerWorkplace>();
            Dictionary<string, object> query = new Dictionary<string, object>();
            Dictionary<string, object> order = new Dictionary<string, object>();
            query.Add("Worker.Id", workerWorkplaceId);
            query.Add("IsActive", isActive);
            //Выбираем рабочие места
            workerWorkplaces = workerWorkplaceRepository.GetByLikeCriteria(query);
            //Если активное рабочее место, то сначала выбираем все, что положено по норме
            if (isActive)
            {
                if (workerWorkplaces.Count > 0)
                {
                    query.Clear();
                    query.Add("Organization", workerWorkplaces[0].Organization);
                    query.Add("Norma.Organization.Id", int.Parse(getCurrentEnterpriseId()));
                    query.Add("Norma.IsActive", true);
                    IList<NormaOrganization> normaOrganizations = normaOrganizationRepository.GetByCriteria(query);
                    if (normaOrganizations != null && normaOrganizations.Count > 0)
                    {
                        IList<NormaContent> normas = normaOrganizations[0].Norma.NormaContents;
                        //normas = reorderNormaContents(normas, false);
                        foreach (var item in normas)
                        {
                            if (item.IsActive == true)
                            {
                                if (item.InShop == false)
                                {
                                    NomenclatureSimple nomenclature = new NomenclatureSimple();
                                    nomenclature.GroupId = item.NomGroup.Id;
                                    nomenclature.GroupName = item.NomGroup.Name;
                                    nomenclature.NormaContentId= item.Id;
                                    nomenclatures.Add(nomenclature);
                                }
                            }
                        }
                    }
                }
            }


            foreach (var item in workerWorkplaces)
            {
                //Для всех неактивных рабочих мест
                //Ищем номенклатуры, которые на руках
                query.Clear();
                query.Add("WorkerWorkplace", item);
                WorkerCardHead workerCardHead = workerCardRepository.FindOne(query);                
                //Если карточка найдена
                if (workerCardHead != null)
                {
                    IList<WorkerCardContent> workerCardContents = new List<WorkerCardContent>();
                    query.Clear();
                    query.Add("WorkerCardHead", workerCardHead);
                    query.Add("[>]Quantity", 0);
                    workerCardContents = workerCardContentRepository.GetByLikeCriteria(query);
                    bool addNomenclature = false;
                    foreach (var itemContens in workerCardContents)
                    {
//                        NomenclatureSimple nomenclature = getEmptyNomenclature(nomenclatures, itemContens.NormaContent.NomGroup.Id);
                        NomenclatureSimple nomenclature = getEmptyNomenclature(nomenclatures, itemContens.Storage.Nomenclature.NomGroup.Id);
                        if (nomenclature == null)
                        {
                            nomenclature = new NomenclatureSimple();
                            if (itemContens.NormaContent!=null)
                                nomenclature.NormaContentId = itemContens.NormaContent.Id;

                            addNomenclature = true;
                        }
                        else addNomenclature = false;
                        nomenclature.setId(itemContens.Id);
                        nomenclature.NameId = itemContens.Storage.Nomenclature.Id;
                        nomenclature.Name = itemContens.Storage.NomenclatureInfo;
                        if (!isActive)
                        //{
                        //    nomenclature.GroupId = itemContens.NormaContent.NomGroup.Id;
                        //    nomenclature.GroupName = itemContens.NormaContent.NomGroup.Name;

                        //}
                        //else
                        {
                            nomenclature.GroupId = itemContens.Storage.Nomenclature.NomGroup.Id;
                            nomenclature.GroupName = itemContens.Storage.Nomenclature.NomGroup.Name;
                        }

                        nomenclature.Quantity = itemContens.Quantity;
                        nomenclature.OperType = itemContens.Operation.OperType.Id;
                        nomenclature.StartDate = itemContens.StartDate;
                        nomenclature.StartDateStr = itemContens.StartDate.ToString(DataGlobals.DATE_FORMAT_FULL_YEAR);
                        if (nomenclature.OperType == DataGlobals.OPERATION_STORAGE_TRANSFER_IN)
                        {
                            nomenclature.DocNumber = itemContens.Operation.DocNumber;
                            nomenclature.OperDate = itemContens.Operation.OperDate;
                            nomenclature.OperDateStr = itemContens.Operation.OperDate.ToString(DataGlobals.DATE_FORMAT_FULL_YEAR);
                        }
                        nomenclature.StorageNameId = itemContens.Storage.StorageName.Id;
                        nomenclature.StorageNumber = itemContens.Storage.StorageName.StorageNumber;
                        if (addNomenclature)
                        {
                            nomenclatures.Add(nomenclature);
                        }
                    }
                }
            }
            return nomenclatures;
        }

        [Transaction]
        private IList<NomenclatureSimple> getNomenclaturesByParam(string Ids)
        { 
            IList<NomenclatureSimple> nomenclatures = new List<NomenclatureSimple>();
            string[] listId = Ids.Split(';');
            foreach (var id in listId)
            {
                Dictionary<string, object> query = new Dictionary<string, object>();
                Dictionary<string, object> order = new Dictionary<string, object>();
                query.Add("[in]Id", Ids);
                query.Add("[>]Quantity", 0);
                order.Add("Storage.StorageName.Id", ASC);
                IList<WorkerCardContent> workerCardContents = workerCardContentRepository.GetByLikeCriteria(query);
                foreach (var itemContens in workerCardContents)
                {
                    NomenclatureSimple nomenclature = new NomenclatureSimple(itemContens.Id);
                    nomenclature.NameId = itemContens.Storage.Nomenclature.Id;
                    nomenclature.Name = itemContens.Storage.NomenclatureInfo;
//                    nomenclature.GroupId = itemContens.NormaContent.NomGroup.Id;
//                    nomenclature.GroupName = itemContens.NormaContent.NomGroup.Name;
                    nomenclature.GroupId = itemContens.Storage.Nomenclature.NomGroup.Id;
                    nomenclature.GroupName = itemContens.Storage.Nomenclature.NomGroup.Name;
                    nomenclature.StartDate = itemContens.StartDate;
                    nomenclature.StartDateStr = itemContens.StartDate.ToString(DataGlobals.DATE_FORMAT_FULL_YEAR);
                    nomenclature.Quantity = itemContens.Quantity;
                    nomenclature.StorageNameId = itemContens.Storage.StorageName.Id;
                    nomenclature.StorageNumber = itemContens.Storage.StorageName.StorageNumber;
                    
                    nomenclature.OperType = itemContens.Operation.OperType.Id;
                    if (nomenclature.OperType == DataGlobals.OPERATION_STORAGE_TRANSFER_IN)
                    {
                        nomenclature.DocNumber = itemContens.Operation.DocNumber;
                        nomenclature.OperDate = itemContens.Operation.OperDate;
                        nomenclature.OperDateStr = itemContens.Operation.OperDate.ToString(DataGlobals.DATE_FORMAT_FULL_YEAR);
                    }

//                    nomenclature.WorkerWorkPlaceId = 
                    nomenclatures.Add(nomenclature);
                }
            }
            return nomenclatures;
        }


        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_WORKER_CARD_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_RETURN_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_OUT_EDIT))]
        public ActionResult SelectContent(int workerWorkplaceId, bool isActive)
        {
            return View(new GridModel(getNomenclaturesForWorkplace(workerWorkplaceId,isActive)));
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_WORKER_CARD_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_RETURN_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_OUT_EDIT))]
        public ActionResult Select_Worker(int? workerWorkplaceId)
        {
            IList<WorkerWorkplace> returnWorkerWorkplaces = new List<WorkerWorkplace>();
            WorkerWorkplace activeWorkerWorkplaces = null;           
            //if (workerWorkplaceId == null && Session["workerWorkplaceId"] != null)
            //    workerWorkplaceId = (int)Session["workerWorkplaceId"];
            if (workerWorkplaceId != null)
            {
                Dictionary<string, object> query = new Dictionary<string, object>();
                Dictionary<string, object> order = new Dictionary<string, object>();
                query.Add("Worker.Id", workerWorkplaceId);
                query.Add("IsActive", true);
                //Выбираем активные рабочие места
                IList<WorkerWorkplace> workerWorkplaces = workerWorkplaceRepository.GetByLikeCriteria(query);
                foreach (var item in workerWorkplaces) {
                    activeWorkerWorkplaces = item;
                    break;                
                }
                if (activeWorkerWorkplaces!=null){
                    returnWorkerWorkplaces.Add(rebuildWorkerWorkplace(activeWorkerWorkplaces));
                }
            }
            return View(new GridModel(returnWorkerWorkplaces));
        }

        [HttpPost]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_WORKER_CARD_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_RETURN_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_OUT_EDIT))]
        public ActionResult _GetWorkerWorkplaces(int? workerWorkplaceId,string text)
        {
            if ((workerWorkplaceId == null) && (text == null))
                return new JsonResult { Data = new SelectList(new List<WorkerWorkplace>()) };
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            if (workerWorkplaceId != null)
            {
                WorkerWorkplace wwp = workerWorkplaceRepository.Get(workerWorkplaceId.Value);
                queryParams.Add("Worker", wwp.Worker);
            }
            else
            {
                int tabn = -1;
                if (int.TryParse(text, out tabn))
                    queryParams.Add("Worker.TabN", tabn);
                else
                    queryParams.Add("Worker.Fio", text);
            }
            queryParams.Add("RootOrganization", int.Parse(getCurrentEnterpriseId()));
            queryParams.Add("IsActive", true);
            IList<WorkerWorkplace> workerWorkplaces = workerWorkplaceRepository.GetByLikeCriteria(queryParams);
            return new JsonResult
            {
                Data = new SelectList(workerWorkplaces, "Worker.Id", "WorkplaceInfo")
//                Data = new SelectList(workerWorkplaces, "Id", "WorkplaceInfo")
            };
        }

        private NormaContent GetNewNormaContent_old(IList<NormaOrganization> normaOrganizations, int GroupId)
        {          
            foreach (var item in normaOrganizations) { 
                foreach (var itemNormaContents in item.Norma.NormaContents){
                    if (itemNormaContents.NomGroup.Id == GroupId) return itemNormaContents;
                }                    
            }
            return null;
        }

        private NormaContent GetNewNormaContent(IList<NormaOrganization> normaOrganizations, int GroupId)
        {
            foreach (var item in normaOrganizations)
            {
                foreach (var itemNormaContents in item.Norma.NormaContents)
                {
                    if (itemNormaContents.IsActive == true)
                    {
                        foreach (var itemNormaNomgroup in itemNormaContents.NormaNomGroups)
                        {
                                if (itemNormaNomgroup.NomGroup.Id == GroupId) return itemNormaContents;
                        }
                    }
                }
            }
            return null;
        }


        [HttpPost]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", "  + DataGlobals.ROLE_WORKER_CARD_EDIT + ", "  + DataGlobals.ROLE_WORKER_CARD_RETURN_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_OUT_EDIT))]
        public ActionResult _TransferWorkerCard(int? workerWorkplaceId, int storageNameId, string listId, Boolean outNorma, string OperDate)
        {
            DateTime operDate;
            DateTime.TryParseExact(OperDate, DataGlobals.DATE_FORMAT, null, System.Globalization.DateTimeStyles.None, out operDate);
            ResultState resultState = new ResultState();
            resultState.Status = ResultState.OK;
            if (workerWorkplaceId != null) {
                Dictionary<string, object> query = new Dictionary<string, object>();
                Dictionary<string, object> order = new Dictionary<string, object>();
                query.Add("Worker.Id", workerWorkplaceId);
                query.Add("IsActive", true);
                order.Add("Storage.StorageName.Id", storageNameId);
                //Выбираем активные рабочие места
                IList<WorkerWorkplace> workerWorkplaces = workerWorkplaceRepository.GetByLikeCriteria(query);
                if (workerWorkplaces.Count == 1)
                {
                    WorkerWorkplace ww = workerWorkplaces[0];
                    //Выбираем активные рабочие места
                    // Запрет на одевание работника в салоне к которому он не приписан. Пока закомментировала, т.к. не уверена, что не будет последствий
                    /*
                    if (ww.Organization.StorageName.Id != storageNameId)
                    {
                        resultState.Status = ResultState.ERROR;
                        resultState.setMessage("НЕЛЬЗЯ ВЫПОЛНИТЬ ПЕРЕВОД!!! Работник приписан к складу " + ww.Organization.StorageName.StorageNumber);
                    }
                     */
                    string periodDateStr = getConfigParamValue(configRepository, "periodDate", getCurrentEnterpriseId());
                    string periodIsClosed = getConfigParamValue(configRepository, "periodIsClosed", getCurrentEnterpriseId());
                    DateTime periodDate;
                    DateTime.TryParseExact(periodDateStr, DataGlobals.DATE_FORMAT_FULL_YEAR, null, System.Globalization.DateTimeStyles.None, out periodDate);

                    // проверка на закрытие периода
                    if (periodIsClosed != null && periodIsClosed.Equals("1") && periodDate > operDate)
                    {
                        resultState.Status = ResultState.ERROR;
                        resultState.setMessage(OperDate + ": Период закрыт для изменений");
                    }
                    
                    if (resultState.Status == ResultState.OK)
                    {
                    
                    query.Clear();
                    query.Add("WorkerWorkplace", ww);
                    WorkerCardHead newWorkerCardHead = workerCardRepository.FindOne(query);
                    if (newWorkerCardHead == null)
                    {
                        newWorkerCardHead = new WorkerCardHead();
                        newWorkerCardHead.WorkerWorkplace = ww;
                    }
                    query.Clear();
                    query.Add("Organization", ww.Organization);
                    query.Add("Norma.Organization.Id", int.Parse(getCurrentEnterpriseId()));
                    query.Add("Norma.IsActive", true);
                    IList<NormaOrganization> normaOrganizations = normaOrganizationRepository.GetByCriteria(query);
                    IList<NomenclatureSimple> list = getNomenclaturesByParam(listId);
                    string docNumber = "00000";
                   // Убрала ниже, чтобы можно было изменять номер документа
                   docNumber = operationRepository.GetNextDocNumber(DataGlobals.OPERATION_STORAGE_TRANSFER_OUT, getCurrentEnterpriseId());
                    int? skladId=null;
                    foreach (var item in list)
                    {
                        ResultState listResultState = new ResultState();
                        listResultState.Status = ResultState.OK;

                        WorkerCardContent oldWorkerCardContent = workerCardContentRepository.Get(item.Id);
                        if (skladId!=null && skladId != oldWorkerCardContent.Storage.StorageName.Id)
                            docNumber = operationRepository.GetNextDocNumber(DataGlobals.OPERATION_STORAGE_TRANSFER_OUT, getCurrentEnterpriseId());

                        if (oldWorkerCardContent.StartDate >= operDate)
                        {
                            listResultState.Status = ResultState.ERROR;
                            listResultState.setMessage(item.Name + "   Дата перевода раньше даты выдачи!!! Введите текущую дату!");

                            resultState.Status = ResultState.ERROR;
                            resultState.setMessage(item.Name + "   Дата перевода раньше даты выдачи!!! Введите текущую дату!");
                        }

                        NormaContent newNormaContent=GetNewNormaContent(normaOrganizations,item.GroupId);
                        // Если на новом рабочем месте номенклатура не подходит по норме и выбрана опция
                        //Ставим старую норму
                        //Закомментировано, т.к. по просьбе УПУ не нужно переносить на руки, то что не по норме
                        if (newNormaContent == null && outNorma == true)
                        {
                            newNormaContent = oldWorkerCardContent.NormaContent;
                        }
//                        if (newNormaContent != null )
                        if (newNormaContent == null && outNorma != true)
                        {
                            listResultState.Status = ResultState.ERROR;
                            listResultState.setMessage(item.Name + " НЕ СООТВЕТСТВУЕТ новой норме. Выполните ВОЗВРАТ или СПИСАНИЕ по акту!");

                            resultState.Status = ResultState.ERROR;
                            resultState.setMessage(item.Name + " НЕ СООТВЕТСТВУЕТ новой норме. Выполните ВОЗВРАТ или СПИСАНИЕ по акту!");
                        }
                        else
                        {
                            Storage storageTo = null;
                            //Выбранный склад и склад номенклатуры совпадают
                            if (oldWorkerCardContent.Storage.StorageName.Id == storageNameId)
                            {
                                storageTo = oldWorkerCardContent.Storage;
                            }
                            //Выбранный склад и склад номенклатуры не совпадают
                            else
                            {
                                //Пытаемся найти номенклатуру на другом складе
                                StorageName storageName = storageNameRepository.Get(storageNameId);
                                if (storageName == null)
                                {
                                    listResultState.Status = ResultState.ERROR;
                                    listResultState.setMessage("В справочнике не найден склад с идентификатором " + storageNameId + "!");

                                    resultState.Status = ResultState.ERROR;
                                    resultState.setMessage("В справочнике не найден склад с идентификатором " + storageNameId + "!");
                                }
                                if (listResultState.Status == ResultState.OK)
                                {
                                    query.Clear();
                                    query.Add("Nomenclature", oldWorkerCardContent.Storage.Nomenclature);
                                    query.Add("StorageName", storageName);
                                    if (oldWorkerCardContent.Storage.Wear == null)
                                        query.Add("[]Wear", "");
                                    else
                                        query.Add("Wear", oldWorkerCardContent.Storage.Wear);
                                    if (oldWorkerCardContent.Storage.Growth == null)
                                        query.Add("[]Growth", "");
                                    else
                                        query.Add("Growth", oldWorkerCardContent.Storage.Growth);
                                    if (oldWorkerCardContent.Storage.NomBodyPartSize == null)
                                        query.Add("[]NomBodyPartSize", "");
                                    else
                                        query.Add("NomBodyPartSize", oldWorkerCardContent.Storage.NomBodyPartSize);
                                    IList<Storage> storages = storageRepository.GetByLikeCriteria(query);
                                    if (storages.Count > 0)
                                    {
                                        storageTo = storages[0];
                                    }
                                    else
                                    {
                                        //На новом складе не нашлась номенклатура
                                        storageTo = new Storage();
                                        storageTo.StorageName = storageName;
                                        storageTo.Nomenclature = oldWorkerCardContent.Storage.Nomenclature;
                                        storageTo.Quantity = 0;
                                        storageTo.Price = oldWorkerCardContent.Storage.Price;
                                        storageTo.Growth = oldWorkerCardContent.Storage.Growth;
                                        storageTo.NomBodyPartSize = oldWorkerCardContent.Storage.NomBodyPartSize;
                                        storageTo.Wear = oldWorkerCardContent.Storage.Wear;
                                        storageRepository.SaveOrUpdate(storageTo);
                                    }
                                }
                            }
//                            if (resultState.Status == ResultState.OK)
                            if (listResultState.Status == ResultState.OK)

                            {
                                
                                //Создаем операцию на перевод по старому месту работы
                                Operation oldOperation = new Operation();
                                oldOperation.WorkerWorkplace = oldWorkerCardContent.WorkerCardHead.WorkerWorkplace;
                                oldOperation.OperDate = operDate; //DateTime.Now;
                                oldOperation.Quantity = oldWorkerCardContent.Quantity;
                                oldOperation.OperType = operTypeRepository.Get(DataGlobals.OPERATION_STORAGE_TRANSFER_OUT);
                                oldOperation.Organization = organizationRepository.Get(int.Parse(getCurrentEnterpriseId()));
                                oldOperation.DocNumber = docNumber;
                                oldOperation.GiveOperation = oldWorkerCardContent.GiveOperation;
                                //oldOperation.DocDate=
                                query.Clear();
                                query.Add("OperType", oldOperation.OperType);
                                IList<Motiv> motivs = motivRepository.GetByLikeCriteria(query);
                                if (motivs.Count>0)
                                    oldOperation.Motiv = motivs[0];
                                oldOperation.Storage = oldWorkerCardContent.Storage;

                                //Создаем операцию на перевод по новому месту работы
                                Operation newOperation = new Operation();
                                newOperation.WorkerWorkplace = ww;
                                newOperation.OperDate = operDate; //DateTime.Now;
                                newOperation.Quantity = oldWorkerCardContent.Quantity;
                                newOperation.OperType = operTypeRepository.Get(DataGlobals.OPERATION_STORAGE_TRANSFER_IN);
                                newOperation.Organization = organizationRepository.Get(int.Parse(getCurrentEnterpriseId()));
                                newOperation.DocNumber = docNumber;
                                newOperation.GiveOperation = oldWorkerCardContent.GiveOperation;
                                //newOperation.DocDate=

                                motivs.Clear();
                                query.Clear();
                                query.Add("OperType", newOperation.OperType);
                                motivs = motivRepository.GetByLikeCriteria(query);
                                if (motivs.Count > 0)
                                    newOperation.Motiv = motivs[0];
                                newOperation.Storage = storageTo;

                                newOperation.TransferOperation = oldOperation;
                                oldOperation.TransferOperation = newOperation;
                                
                                operationRepository.SaveOrUpdate(oldOperation);
                                operationRepository.SaveOrUpdate(newOperation);
                                //Создаем новую карточку
                                WorkerCardContent newWorkerCardContent = new WorkerCardContent();
                                newWorkerCardContent.WorkerCardHead = newWorkerCardHead;
                                newWorkerCardContent.Storage = storageTo;
                                newWorkerCardContent.Quantity = oldWorkerCardContent.Quantity;
                                newWorkerCardContent.Operation = newOperation;
                                newWorkerCardContent.StartDate = oldWorkerCardContent.StartDate;
                                newWorkerCardContent.UsePeriod = oldWorkerCardContent.UsePeriod;
                                newWorkerCardContent.GiveOperation = oldWorkerCardContent.GiveOperation;

                                //Проставляем новую норму!!!
                                newWorkerCardContent.NormaContent = newNormaContent;
                                //Обнуляем старую карточку
                                oldWorkerCardContent.Quantity = 0;
                                oldWorkerCardContent.EndDate = operDate; //DateTime.Now;
                                oldWorkerCardContent.OperReturn = oldOperation;
                                //Сохраняем новую и старую карточки
                                workerCardContentRepository.SaveOrUpdate(oldWorkerCardContent);
                                workerCardContentRepository.SaveOrUpdate(newWorkerCardContent);
                            }
                        }

                        skladId = oldWorkerCardContent.Storage.StorageName.Id;
                    }
                   }
                }
                else
                {
                    //У человека несколько активных рабочих мест или рабочее место не найдено
                    resultState.Status = ResultState.ERROR;
                    resultState.setMessage("У человека больше одного рабочего места или рабочее место не найдено!");
                }                
            }
            return Json(resultState);
        }

        [HttpPost]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_WORKER_CARD_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_RETURN_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_OUT_EDIT))]
        public ActionResult _ReturnWorkerCard(string  workerCardContentId, int storageNameId, int quantity) {
            ResultState resultState = new ResultState();
            resultState.Status = ResultState.OK;
            string[] listWorkerCardContentId = workerCardContentId.Split(',');
            foreach (var item in listWorkerCardContentId)
            {
                WorkerCardContent wcc = workerCardContentRepository.Get(Int32.Parse(item));
                if (wcc == null) {
                    resultState.Status = ResultState.ERROR;
                    resultState.setMessage("Указанная номенклатура не найдена!");
                }

                if (resultState.Status == ResultState.OK) {
                    if (wcc.Storage.StorageName.Id == storageNameId)
                    {
                        Operation operation = new Operation();
                        //Возврат Перевод работника
                        operation.Motiv = motivRepository.Get(4);
                        operation.DocNumber = operationRepository.GetNextDocNumber(DataGlobals.OPERATION_WORKER_RETURN,getCurrentEnterpriseId());
                        operation.DocDate = DateTime.Now;
                        operation.OperType = operTypeRepository.Get(DataGlobals.OPERATION_WORKER_RETURN);
                        operation.WorkerWorkplace = wcc.WorkerCardHead.WorkerWorkplace;
                        operation.Organization = organizationRepository.Get(operation.WorkerWorkplace.RootOrganization);
                        operation.Quantity = quantity;
                        operation.Storage = wcc.Operation.Storage;
                        operationRepository.SaveOrUpdate(operation);

                        wcc.OperReturn = operation;
                        wcc.Quantity = 0;
                        wcc.EndDate = DateTime.Now;
                        workerCardContentRepository.SaveOrUpdate(wcc);
                    }
                    else
                    {
                        resultState.Status = ResultState.ERROR;
                        resultState.setMessage("Функция возврата спецодежды на другой склад не реализована!");
                    }
                }
            }
            return Json(resultState);
        }

        [HttpPost]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_WORKER_CARD_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_RETURN_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_OUT_EDIT))]
        public ActionResult _DebitWorkerCard(string workerCardContentId, int storageNameId,string docDate, string docNumber, int quantity)
        {
            ResultState resultState = new ResultState();
            resultState.Status = ResultState.OK;
            string[] listWorkerCardContentId=workerCardContentId.Split(',');
            IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
            foreach (var item in listWorkerCardContentId)
            {
                WorkerCardContent wcc = workerCardContentRepository.Get(Int32.Parse(item));
                if (wcc == null)
                {
                    resultState.Status = ResultState.ERROR;
                    resultState.setMessage("Указанная номенклатура не найдена!");
                }

                if (resultState.Status == ResultState.OK)
                {
                    if (wcc.Storage.StorageName.Id == storageNameId)
                    {
                        Operation operation = new Operation();
                        //Списание досрочно
                        operation.Motiv = motivRepository.Get(2);
                        operation.DocNumber = docNumber;
                        operation.DocDate = DateTime.ParseExact(docDate, DataGlobals.DATE_FORMAT_FULL_YEAR, culture);
                        operation.OperType = operTypeRepository.Get(DataGlobals.OPERATION_WORKER_OUT);
                        operation.WorkerWorkplace = wcc.WorkerCardHead.WorkerWorkplace;
                        operation.Organization = organizationRepository.Get(operation.WorkerWorkplace.RootOrganization);
                        operation.Quantity = quantity;
                        operation.Storage = wcc.Operation.Storage;
                        operationRepository.SaveOrUpdate(operation);

                        wcc.OperReturn = operation;
                        wcc.Quantity = 0;
                        wcc.EndDate = DateTime.Now;
                        workerCardContentRepository.SaveOrUpdate(wcc);
                    }
                    else
                    {
                        resultState.Status = ResultState.ERROR;
                        resultState.setMessage("Функция списания спецодежды с другого склада не реализована!");
                    }
                }
            }
            return Json(resultState);
        }
    }
}
