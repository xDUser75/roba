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
    public class StornoController : ViewedController
    {
        private readonly CriteriaRepository<WorkerCardContent> workerCardContentRepository;
        private readonly CriteriaRepository<Worker> workerRepository;
        private readonly CriteriaRepository<WorkerWorkplace> workerWorkplaceRepository;
        private readonly CriteriaRepository<Storage> storageRepository;
        private readonly StorageNameRepository storageNameRepository;
        private readonly CriteriaRepository<OperType> operTypeRepository;
        private readonly OperationRepository operationRepository;
        private readonly CriteriaRepository<OperationSimple> operationSimpleRepository;
        private readonly RemainRepository remainRepository;
        private readonly CriteriaRepository<Config> configRepository;
        private readonly CriteriaRepository<MatPersonCardContent> matPersonCardContentRepository;
        private readonly CriteriaRepository<MatPersonCardHead> matPersonCardHeadRepository;

        public StornoController(
            CriteriaRepository<WorkerCardContent> workerCardContentRepository,
            CriteriaRepository<Worker> workerRepository,
            CriteriaRepository<WorkerWorkplace> workerWorkplaceRepository,
            CriteriaRepository<Storage> storageRepository,
            StorageNameRepository storageNameRepository,
            CriteriaRepository<OperType> operTypeRepository,
            RemainRepository remainRepository,
            OperationRepository operationRepository,
            CriteriaRepository<OperationSimple> operationSimpleRepository,
            CriteriaRepository<Config> configRepository,
            CriteriaRepository<MatPersonCardContent> matPersonCardContentRepository,
            CriteriaRepository<MatPersonCardHead> matPersonCardHeadRepository
            )
        {
            Check.Require(operationRepository != null, "operationRepository may not be null");
            this.workerCardContentRepository = workerCardContentRepository;
            this.workerRepository = workerRepository;
            this.workerWorkplaceRepository = workerWorkplaceRepository;
            this.storageRepository = storageRepository;
            this.storageNameRepository = storageNameRepository;
            this.operTypeRepository = operTypeRepository;
            this.operationRepository = operationRepository;
            this.operationSimpleRepository = operationSimpleRepository;
            this.remainRepository = remainRepository;
            this.configRepository = configRepository;
            this.matPersonCardContentRepository = matPersonCardContentRepository;
            this.matPersonCardHeadRepository = matPersonCardHeadRepository;
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
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_STORNO_EDIT + ", " + DataGlobals.ROLE_STORNO_VIEW))]
        public ActionResult _SelectOperations(int workerWorkplaceId, int StorageNameId)
        {
            // Отображаются операции за предыдущий месяц от даты снятия остатков
            // После сторнирования операции последние снятые остатки переписываются
            DateTime dt=remainRepository.GetMaxRemainDate(StorageNameId);
            dt = dt.AddMonths(-1);
            dt = dt.AddDays(-1 * (dt.Day - 1));

// Выбирается дата открытого периода
            string periodDateStr = getConfigParamValue(configRepository, "periodDate", getCurrentEnterpriseId());
            string periodIsClosed = getConfigParamValue(configRepository, "periodIsClosed", getCurrentEnterpriseId());
            DateTime periodDate;
            DateTime.TryParseExact(periodDateStr, DataGlobals.DATE_FORMAT_FULL_YEAR, null, System.Globalization.DateTimeStyles.None, out periodDate);


            if (workerWorkplaceId <= 0) return View(new GridModel(new List < OperationSimple >()));
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            queryParams.Add("WorkerId", workerWorkplaceId);
            queryParams.Add("OrganizationId", int.Parse(getCurrentEnterpriseId()));
            queryParams.Add("StorageNameId", StorageNameId);            
//            queryParams.Add("[>=]OperDate", dt);
// Пробуем отобрать операции  по дате периода
            queryParams.Add("[>=]OperDate", periodDate);
            orderParams.Add("OperDate", ASC);
            orderParams.Add("DocNumber", ASC);
            IList<OperationSimple> list = new List<OperationSimple>();
            IList<OperationSimple> operations = operationSimpleRepository.GetByLikeCriteria(queryParams, orderParams);
            foreach (var item in operations) {
                list.Add(item);
            }
            queryParams.Remove("WorkerId");
            queryParams.Add("MolId", workerWorkplaceId);
            operations = operationSimpleRepository.GetByLikeCriteria(queryParams, orderParams);
            foreach (var item in operations)
            {
                list.Add(item);
            }
            return View(new GridModel(list));
        }

        public void Storno(int id, int workerWorkplaceId, int StorageNameId)
        {
            Operation oper = operationRepository.Get(id);
            Operation operTransfer = oper.TransferOperation;
            if (oper == null)
            {
                ModelState.AddModelError("Сторнирование операции", "Операция не найдена!");
            }
            else
            {
                if (oper.RefOperation != null)
                {
                    ModelState.AddModelError("Сторнирование операции", "Повторное сторнирование операции!");
                }
            }
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            //Если операция связана с МОЛ
            if (oper.OperType.Id.In(DataGlobals.OPERATION_MOL_STORAGE_IN,        /*Возврат дежурной на склад*/
                                    DataGlobals.OPERATION_MOL_STORAGE_OUT,       /*Выдача дежурной МОЛ со склада*/
                                    DataGlobals.OPERATION_MOL_OUT,               /*Списание дежурной с МОЛ*/
                                    DataGlobals.OPERATION_MOL_WORKER_IN,         /*Выдача дежурной от МОЛ работнику*/
                                    DataGlobals.OPERATION_MOL_WORKER_RETURN,     /*Возврат дежурной от работника МОЛ*/
                                    DataGlobals.OPERATION_MOL_MOVE_OUT,          /*Перевод дежурной. Списание с забаланса*/
                                    DataGlobals.OPERATION_MOL_MOVE_IN            /*Перевод дежурной. Постановка на забаланс*/))
            {
                queryParams.Clear();
                queryParams.Add("Operation", oper);
                var list = matPersonCardContentRepository.GetByCriteria(queryParams);
                MatPersonCardContent mpcc = null;
                if (list.Count > 0)
                {
                    mpcc = list[0];
                }
                else
                {
                    ModelState.AddModelError("Сторнирование операции", "У МОЛ не найдена номенклатура для сторнирования!");
                }


                queryParams.Clear();
                queryParams.Add("Worker.Id", workerWorkplaceId);
                var mpchList = matPersonCardHeadRepository.GetByLikeCriteria(queryParams);
                MatPersonCardHead mpch = null;
                if (mpchList.Count > 0) {
                    mpch = mpchList[0];
                }
                
                queryParams.Clear();
                queryParams.Add("MatPersonCardHead", mpch);
                queryParams.Add("Operation.Organization", oper.Organization);
                queryParams.Add("[in]Operation.OperType.Id", DataGlobals.OPERATION_MOL_STORAGE_IN+","+        /*Возврат дежурной на склад*/
                                    DataGlobals.OPERATION_MOL_STORAGE_OUT+","+       /*Выдача дежурной МОЛ со склада*/
                                    DataGlobals.OPERATION_MOL_OUT+","+               /*Списание дежурной с МОЛ*/
                                    DataGlobals.OPERATION_MOL_WORKER_IN+","+         /*Выдача дежурной от МОЛ работнику*/
                                    DataGlobals.OPERATION_MOL_WORKER_RETURN+","+     /*Возврат дежурной от работника МОЛ*/
                                    DataGlobals.OPERATION_MOL_MOVE_OUT+","+          /*Перевод дежурной. Списание с забаланса*/
                                    DataGlobals.OPERATION_MOL_MOVE_IN            /*Перевод дежурной. Постановка на забаланс*/);
                queryParams.Add("[]Operation.RefOperation", null);
                queryParams.Add("[>]Operation.DocDate", oper.DocDate);
                var mpcclist = matPersonCardContentRepository.GetByCriteria(queryParams);
                if (mpcclist.Count > 0)
                {
                    ModelState.AddModelError("Сторнирование операции", "У МОЛ была операция, которая по дате позже сторнируемой!");
                }
                if (ModelState.IsValid)
                {
                    if (oper.OperType.Id == DataGlobals.OPERATION_MOL_STORAGE_IN)
                    {
                        if (mpcc.Storage.Quantity < Math.Abs(mpcc.Quantity))
                        {
                            ModelState.AddModelError("Сторнирование операции", "На складе не хватает номенклатуры!");
                        }
                        else
                        {
                            // Т.к. при операции возврата mpcc.Quantity отрицательное, то нужно его * на -1
                            mpcc.Storage.Quantity = mpcc.Storage.Quantity - (-1) * mpcc.Quantity;
                        }
                    }
                    if (oper.OperType.Id == DataGlobals.OPERATION_MOL_STORAGE_OUT)
                    {
                        mpcc.Storage.Quantity = mpcc.Storage.Quantity + mpcc.Quantity;
                    }
                }
               // if (oper.OperType.Id == DataGlobals.OPERATION_MOL_OUT) { }
                if (oper.OperType.Id == DataGlobals.OPERATION_MOL_WORKER_IN || oper.OperType.Id == DataGlobals.OPERATION_MOL_WORKER_RETURN)
                {
                    queryParams.Clear();
                    WorkerCardContent wcc = null;
                    if (oper.OperType.Id == DataGlobals.OPERATION_MOL_WORKER_IN)
                    {
                        queryParams.Add("Operation", oper);
                    }
                    else
                    {
                        queryParams.Add("OperReturn", oper);
                    }
                    wcc = workerCardContentRepository.FindOne(queryParams);
                    if (wcc == null)
                    {
                        ModelState.AddModelError("Сторнирование операции", "Не найдена номенклатура на карточке. Сторнирование не возможно! ");
                    }
                    if (wcc.OperReturn != null)
                    {
                        ModelState.AddModelError("Сторнирование выдачи", "У работника был возврат этой номенклатуры!");
                    }
                    if ((wcc.Quantity - oper.Quantity) < 0)
                    {
                        ModelState.AddModelError("Сторнирование выдачи", "У работника на руках меньше кол-во номенклатуры!");
                    }
                    if (ModelState.IsValid)
                    {
                        wcc.Quantity = wcc.Quantity - oper.Quantity;
                    }
                }
                //Для операции "Перевод между МОЛ" обработка особая
                if (oper.OperType.Id == DataGlobals.OPERATION_MOL_MOVE_OUT || oper.OperType.Id == DataGlobals.OPERATION_MOL_MOVE_IN) {
                   
                    //Ищем операции для обоих МОЛ.
                    queryParams.Clear();
                    queryParams.Add("DocNumber", oper.DocNumber);
                    queryParams.Add("Organization", oper.Organization);
                    queryParams.Add("Storage", oper.Storage);
                    queryParams.Add("OperDate", oper.OperDate);
                    queryParams.Add("DocDate", oper.DocDate);
                    queryParams.Add("[in]OperType.Id", DataGlobals.OPERATION_MOL_MOVE_OUT+","+DataGlobals.OPERATION_MOL_MOVE_IN);
                    IList<Operation> listOper = operationRepository.GetByLikeCriteria(queryParams);

                    if (listOper.Count!=2){
                        ModelState.AddModelError("Сторнирование перевода", "Не найдены операции по перемещению между МОЛ в достаточном кол-ве!");
                    } else {
                        string docNumber = operationRepository.GetNextDocNumber(DataGlobals.OPERATION_STORAGE_TRANSFER_OUT, getCurrentEnterpriseId());

                        foreach (var item in listOper)
                        {
                            Operation stornoOper = new Operation();
                            MatPersonCardContent stornoMpcc = new MatPersonCardContent();
                            queryParams.Clear();
                            queryParams.Add("Operation", item);
                            var l = matPersonCardContentRepository.GetByCriteria(queryParams);
                            MatPersonCardContent findMpcc = null;
                            if (l.Count > 0)
                            {


                                stornoOper.Quantity = -1 * Math.Abs(item.Quantity);
                                stornoOper.DocDate = item.DocDate;
                                stornoOper.DocNumber = docNumber;
                                stornoOper.DocType = item.DocType;
                                stornoOper.IsCorporate = item.IsCorporate;
                                stornoOper.Motiv = item.Motiv;
                                stornoOper.OperDate = item.OperDate;
                                stornoOper.OperType = item.OperType;
                                stornoOper.Organization = item.Organization;
                                stornoOper.RefOperation = item;
                                stornoOper.Storage = item.Storage;
                                stornoOper.StorageName = item.StorageName;
                                stornoOper.TransferOperation = item.TransferOperation;
                                stornoOper.Wear = item.Wear;
                                stornoOper.WorkerWorkplace = item.WorkerWorkplace;
                                operationRepository.SaveOrUpdate(stornoOper);
                                item.RefOperation = stornoOper;
                                operationRepository.SaveOrUpdate(item);

                                findMpcc = l[0];
                                stornoMpcc.MatPersonCardHead = findMpcc.MatPersonCardHead;
                                stornoMpcc.Quantity = -1 * findMpcc.Quantity;
                                stornoMpcc.Storage = findMpcc.Storage;
                                stornoMpcc.Wear = findMpcc.Wear;
                                stornoMpcc.Operation = stornoOper;
                                stornoMpcc.OperType = findMpcc.OperType;
                                stornoMpcc.OperDate = findMpcc.OperDate;
                                matPersonCardContentRepository.SaveOrUpdate(stornoMpcc);
                                

                            }
                            else
                            {
                                ModelState.AddModelError("Сторнирование операции", "У МОЛ не найдена номенклатура для сторнирования перевода!");
                                break;
                            }

                        }
                    }
                }
                if (ModelState.IsValid)
                {
                    if (oper.OperType.Id.In(DataGlobals.OPERATION_MOL_STORAGE_IN,        /*Возврат дежурной на склад*/
                                            DataGlobals.OPERATION_MOL_STORAGE_OUT,       /*Выдача дежурной МОЛ со склада*/
                                            DataGlobals.OPERATION_MOL_OUT,               /*Списание дежурной с МОЛ*/
                                            DataGlobals.OPERATION_MOL_WORKER_IN,         /*Выдача дежурной от МОЛ работнику*/
                                            DataGlobals.OPERATION_MOL_WORKER_RETURN     /*Возврат дежурной от работника МОЛ*/))
                    {
                        if (oper.OperType.Id.In(DataGlobals.OPERATION_MOL_STORAGE_IN,        /*Возврат дежурной на склад*/
                                                DataGlobals.OPERATION_MOL_STORAGE_OUT       /*Выдача дежурной МОЛ со склада*/))
                        {
                            DateTime maxStornoDate = remainRepository.GetMaxRemainDate(StorageNameId);
                            maxStornoDate = remainRepository.GetActialRemainDate(StorageNameId, maxStornoDate);
                            DateTime paramDateFrom = new DateTime(oper.OperDate.Year, oper.OperDate.Month, oper.OperDate.Day);
                            paramDateFrom = paramDateFrom.AddMonths(1);
                            paramDateFrom = paramDateFrom.AddDays(-1 * (paramDateFrom.Day - 1));
                            Remaind remain = null;
                            if (maxStornoDate >= paramDateFrom)
                            {
                                queryParams.Clear();
                                queryParams.Add("RemaindDate", paramDateFrom);
                                queryParams.Add("StorageName", oper.Storage.StorageName);
                                queryParams.Add("Storage", oper.Storage);
                                Dictionary<string, object> orderParams = new Dictionary<string, object>();
                                remain = remainRepository.FindOne(queryParams);
                                //Если в остатках такой записи не оказалось, то создаем ее
                                if (remain == null)
                                {
                                    remain = new Remaind();
                                    remain.RemaindDate = paramDateFrom;
                                    remain.Storage = oper.Storage;
                                    remain.StorageName = oper.Storage.StorageName;
                                    remain.Nomenclature = oper.Storage.Nomenclature;
                                    remain.Wear = int.Parse(oper.Storage.Wear);
                                    remain.NomBodyPartSize = oper.Storage.NomBodyPartSize;
                                    remain.Growth = oper.Storage.Growth;
                                    remain.Quantity = 0;
                                }
                            }
                            if (remain != null)
                            {
                                remain.Quantity = remain.Quantity + (-1 * mpcc.Quantity);
                                remainRepository.SaveOrUpdate(remain);
                                
                            }
                        }

                        Operation stornoOper = new Operation();
                        stornoOper.DocDate = oper.DocDate;
                        string docNumber = "";
                        if (oper.OperType.Id == DataGlobals.OPERATION_MOL_STORAGE_IN || oper.OperType.Id == DataGlobals.OPERATION_MOL_WORKER_RETURN)
                        {
                            docNumber = operationRepository.GetNextDocNumber(DataGlobals.OPERATION_WORKER_RETURN, getCurrentEnterpriseId());
                        }
                        else if (oper.OperType.Id == DataGlobals.OPERATION_MOL_STORAGE_OUT || oper.OperType.Id == DataGlobals.OPERATION_MOL_WORKER_IN)
                        {
                            docNumber = operationRepository.GetNextDocNumber(DataGlobals.OPERATION_WORKER_IN, getCurrentEnterpriseId());
                        }
                        else if (oper.OperType.Id == DataGlobals.OPERATION_MOL_OUT)
                        {
                            docNumber = operationRepository.GetNextDocNumber(DataGlobals.OPERATION_MOL_OUT, getCurrentEnterpriseId());
                        }
                        else
                        {
                            docNumber = oper.DocNumber;
                        }
                        stornoOper.DocNumber = docNumber;
                        stornoOper.DocType = oper.DocType;
                        stornoOper.Motiv = oper.Motiv;
                        stornoOper.Note = oper.Note;
                        stornoOper.OperDate = oper.OperDate;
                        stornoOper.OperType = oper.OperType;
                        stornoOper.Organization = oper.Organization;
                        stornoOper.Partner = oper.Partner;
                        stornoOper.Quantity = -1 * Math.Abs(oper.Quantity);
                        stornoOper.Storage = oper.Storage;
                        stornoOper.StorageName = oper.StorageName;
                        stornoOper.Wear = oper.Wear;
                        stornoOper.WorkerWorkplace = oper.WorkerWorkplace;
                        stornoOper.RefOperation = oper;
                        operationRepository.SaveOrUpdate(stornoOper);
                        oper.RefOperation = stornoOper;
                        operationRepository.SaveOrUpdate(oper);

                        MatPersonCardContent stornoMpcc = new MatPersonCardContent();
                        stornoMpcc.MatPersonCardHead = mpcc.MatPersonCardHead;
                        stornoMpcc.Quantity = -1 * mpcc.Quantity;
                        stornoMpcc.Storage = mpcc.Storage;
                        stornoMpcc.Wear = mpcc.Wear;
                        stornoMpcc.Operation = stornoOper;
                        stornoMpcc.OperType = mpcc.OperType;
                        stornoMpcc.OperDate = mpcc.OperDate;


                        matPersonCardContentRepository.SaveOrUpdate(stornoMpcc);
                    }
                }
            }
            //Если операция связана с карточкой
            else
            {
                WorkerCardContent wcc = null;
                if (oper.OperType.Id == DataGlobals.OPERATION_WORKER_IN || oper.OperType.Id == DataGlobals.OPERATION_STORAGE_TRANSFER_IN)
                {
                    queryParams.Add("Operation", oper);
                }
                else
                {
                    queryParams.Add("OperReturn", oper);
                }
                wcc = workerCardContentRepository.FindOne(queryParams);
                if (wcc == null)
                {
                    ModelState.AddModelError("Сторнирование операции", "Не найдена номенклатура на карточке. Сторнирование не возможно! ");
                }
                Remaind remain = null;
                Storage stor = null;
                if (ModelState.IsValid)
                {

                    DateTime maxStornoDate = remainRepository.GetMaxRemainDate(StorageNameId);
                    maxStornoDate = remainRepository.GetActialRemainDate(StorageNameId, maxStornoDate);
                    DateTime paramDateFrom = new DateTime(oper.OperDate.Year, oper.OperDate.Month, oper.OperDate.Day);
                    paramDateFrom = paramDateFrom.AddMonths(1);
                    paramDateFrom = paramDateFrom.AddDays(-1 * (paramDateFrom.Day - 1));
                    if (maxStornoDate >= paramDateFrom)
                    {
                        queryParams.Clear();
                        queryParams.Add("RemaindDate", paramDateFrom);
                        //DateTime paramDateTo = new DateTime(oper.OperDate.Year, oper.OperDate.Month, oper.OperDate.Day);
                        //paramDateTo = paramDateTo.AddMonths(1);
                        //paramDateTo = paramDateTo.AddDays(-1 * paramDateTo.Day);
                        //queryParams.Add("[<=]RemaindDate", paramDateTo);
                        queryParams.Add("StorageName", oper.Storage.StorageName);
                        queryParams.Add("Storage", oper.Storage);
                        Dictionary<string, object> orderParams = new Dictionary<string, object>();
                        remain = remainRepository.FindOne(queryParams);
                        //Если в остатках такой записи не оказалось, то создаем ее
                        if (remain == null)
                        {
                            remain = new Remaind();
                            remain.RemaindDate = paramDateFrom;
                            remain.Storage = oper.Storage;
                            remain.StorageName = oper.Storage.StorageName;
                            remain.Nomenclature = oper.Storage.Nomenclature;
                            remain.Wear = int.Parse(oper.Storage.Wear);
                            remain.NomBodyPartSize = oper.Storage.NomBodyPartSize;
                            remain.Growth = oper.Storage.Growth;
                            remain.Quantity = 0;
                        }
                    }
                    stor = storageRepository.Get(oper.Storage.Id);
                    //Сторнируем выдачу работнику
                    if (oper.OperType.Id == DataGlobals.OPERATION_WORKER_IN)
                    {
                        if (wcc.OperReturn != null)
                        {
                            ModelState.AddModelError("Сторнирование выдачи", "У работника был возврат этой номенклатуры!");
                        }
                        if ((wcc.Quantity - oper.Quantity) < 0)
                        {
                            ModelState.AddModelError("Сторнирование выдачи", "У работника на руках меньше кол-во номенклатуры!");
                        }
                        if (ModelState.IsValid)
                        {
                            wcc.Quantity = wcc.Quantity - oper.Quantity;
                            stor.Quantity = stor.Quantity + oper.Quantity;
                            if (remain != null)
                            {
                                remain.Quantity = remain.Quantity + oper.Quantity;
                            }
                        }
                    }
                    //Сторнируем возврат работником
                    if (oper.OperType.Id == DataGlobals.OPERATION_WORKER_RETURN)
                    {
                        if (((stor.Quantity - oper.Quantity) < 0) & (oper.Wear != "0"))
                        {
                            ModelState.AddModelError("Сторнирование возврата", "На складе не хватает заданной номенклатуры!");
                        }
                        else
                        {
                            wcc.OperReturn = null;
                            wcc.EndDate = Null_Date;
                            wcc.Quantity = wcc.Quantity + oper.Quantity;
                            // Если не утиль, то обновляем склад
                            if (oper.Wear != "0")
                            {
                                stor.Quantity = stor.Quantity - oper.Quantity;
                            }
                        }
                        if (remain != null)
                        {
                            if (((remain.Quantity - oper.Quantity) < 0) & (oper.Wear != "0"))
                            {
                                ModelState.AddModelError("Сторнирование возврата", "После этой операции произошло снятие остатков в конце месяца. В этих остатках не хватает кол-ва!");
                            }
                            else
                            {
                                wcc.Quantity = wcc.Quantity + oper.Quantity;
                                // Если не утиль, то обновляем склад
                                if (oper.Wear != "0")
                                {
                                    stor.Quantity = stor.Quantity - oper.Quantity;
                                }
                            }
                        }
                    }
                    //Сторнируем списание с работника
                    if (oper.OperType.Id == DataGlobals.OPERATION_WORKER_OUT || oper.OperType.Id == DataGlobals.OPERATION_WORKER_OUT_TIME)
                    {
                        if (wcc.Quantity == 0)
                        {
                            wcc.OperReturn = null;
                            wcc.EndDate = Null_Date;
                        }
                        wcc.Quantity = wcc.Quantity + oper.Quantity;
                    }
                    //Сторнируем перевод  работника состояние на личных карточках
                    if (oper.OperType.Id == DataGlobals.OPERATION_STORAGE_TRANSFER_IN || oper.OperType.Id == DataGlobals.OPERATION_STORAGE_TRANSFER_OUT)
                    {
                        queryParams.Clear();
                        if (oper.OperType.Id == DataGlobals.OPERATION_STORAGE_TRANSFER_IN)
                        {
                            queryParams.Add("Operation", oper);
                            wcc = workerCardContentRepository.FindOne(queryParams);
                            workerCardContentRepository.Delete(wcc);

                            queryParams.Clear();
                            queryParams.Add("OperReturn", operTransfer);
                            wcc = workerCardContentRepository.FindOne(queryParams);
                            if (wcc.Quantity == 0)
                            {
                                wcc.OperReturn = null;
                                wcc.EndDate = Null_Date;
                            }
                            wcc.Quantity = wcc.Quantity + oper.Quantity;
                            
                        }
                        if (oper.OperType.Id == DataGlobals.OPERATION_STORAGE_TRANSFER_OUT)
                        {
                            queryParams.Clear();
                            queryParams.Add("Operation", operTransfer);
                            wcc = workerCardContentRepository.FindOne(queryParams);
                            workerCardContentRepository.Delete(wcc);
                            
                            queryParams.Clear();
                            queryParams.Add("OperReturn", oper);
                            wcc = workerCardContentRepository.FindOne(queryParams);
                            if (wcc.Quantity == 0)
                            {
                                wcc.OperReturn = null;
                                wcc.EndDate = Null_Date;
                            }
                            wcc.Quantity = wcc.Quantity + oper.Quantity;
                        }

                    }
                }
                if (ModelState.IsValid)
                {
                    workerCardContentRepository.SaveOrUpdate(wcc);
                    //Если не списание с работника
                    if (oper.OperType.Id != DataGlobals.OPERATION_WORKER_OUT)
                    {
                        // Если не утиль, то обновляем склад
                        if (oper.Wear != "0")
                        {
                            storageRepository.SaveOrUpdate(stor);
                        }
                    }
                    Operation stornoOper = new Operation();
                    stornoOper.DocDate = oper.DocDate;
                    //                stornoOper.DocNumber = oper.DocNumber;
                    string docNumber = "";
                    if (oper.OperType.Id == DataGlobals.OPERATION_WORKER_RETURN || oper.OperType.Id == DataGlobals.OPERATION_WORKER_IN)
                    {
                        docNumber = operationRepository.GetNextDocNumber(oper.OperType.Id, getCurrentEnterpriseId());
                    }
                    else if (oper.OperType.Id == DataGlobals.OPERATION_STORAGE_TRANSFER_IN || oper.OperType.Id == DataGlobals.OPERATION_STORAGE_TRANSFER_OUT)
                    {
                        docNumber = operationRepository.GetNextDocNumber(DataGlobals.OPERATION_STORAGE_TRANSFER_OUT, getCurrentEnterpriseId());

                    }
                    else
                    {
                        docNumber = oper.DocNumber;
                    }
                    stornoOper.DocNumber = docNumber;
                    stornoOper.DocType = oper.DocType;
                    stornoOper.Motiv = oper.Motiv;
                    stornoOper.Note = oper.Note;
                    stornoOper.OperDate = oper.OperDate;
                    stornoOper.OperType = oper.OperType;
                    stornoOper.Organization = oper.Organization;
                    stornoOper.Partner = oper.Partner;
                    stornoOper.Quantity = -1 * oper.Quantity;
                    stornoOper.Storage = oper.Storage;
                    stornoOper.StorageName = oper.StorageName;
                    stornoOper.Wear = oper.Wear;
                    stornoOper.WorkerWorkplace = oper.WorkerWorkplace;
                    stornoOper.RefOperation = oper;
                    stornoOper.TransferOperation = oper;
                    operationRepository.SaveOrUpdate(stornoOper);
                    oper.RefOperation = stornoOper;
                    oper.TransferOperation = stornoOper;
                    operationRepository.SaveOrUpdate(oper);
                    // Если операция перевода, то сторнируем связанную с ней
                    if (operTransfer != null)
                    {
                        Operation stornoTranserOper = new Operation();
                        stornoTranserOper.DocDate = oper.DocDate;
                        //                stornoOper.DocNumber = oper.DocNumber;
                        // Номер документа сторно будет новый, такой же как у первой операции
                        stornoTranserOper.DocNumber = docNumber;
                        stornoTranserOper.DocType = operTransfer.DocType;
                        stornoTranserOper.Motiv = operTransfer.Motiv;
                        stornoTranserOper.Note = operTransfer.Note;
                        stornoTranserOper.OperDate = operTransfer.OperDate;
                        stornoTranserOper.OperType = operTransfer.OperType;
                        stornoTranserOper.Organization = operTransfer.Organization;
                        stornoTranserOper.Partner = operTransfer.Partner;
                        stornoTranserOper.Quantity = -1 * operTransfer.Quantity;
                        stornoTranserOper.Storage = operTransfer.Storage;
                        stornoTranserOper.StorageName = operTransfer.StorageName;
                        stornoTranserOper.Wear = operTransfer.Wear;
                        stornoTranserOper.WorkerWorkplace = operTransfer.WorkerWorkplace;
                        stornoTranserOper.RefOperation = operTransfer;
                        operationRepository.SaveOrUpdate(stornoTranserOper);
                        operTransfer.RefOperation = stornoTranserOper;
                        operationRepository.SaveOrUpdate(operTransfer);
                    }

                    //Если не списание с работника
                    if (oper.OperType.Id == DataGlobals.OPERATION_WORKER_IN || oper.OperType.Id == DataGlobals.OPERATION_WORKER_RETURN)
                    {
                        if (remain != null)
                        {
                            remainRepository.SaveOrUpdate(remain);
                        }
                    }
                }
            }
        }
        
        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_STORNO_EDIT))]
        public ActionResult _StornoOper(int id, int workerWorkplaceId, int StorageNameId) {
            Storno(id, workerWorkplaceId, StorageNameId);
            IList<OperationSimple> operations = new List<OperationSimple>();
            return View(new GridModel(operations));
        }



        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_STORNO_EDIT))]
        public ActionResult _StornoExternalDocument(string docNumber, string docDate, int stornoType)
        {
            DateTime documDate = DateTime.ParseExact(docDate, DataGlobals.DATE_FORMAT_FULL_YEAR, null);
            DateTime minDate = new DateTime(DateTime.Now.Year,DateTime.Now.Month,1);
            string periodIsClosed = getConfigParamValue(configRepository, "periodIsClosed", getCurrentEnterpriseId());
            string periodDateStr = getConfigParamValue(configRepository, "periodDate", getCurrentEnterpriseId());

//            if (DateTime.Now.Day <= 5) {
//                minDate = minDate.AddMonths(-1);
//            }
            DateTime.TryParseExact(periodDateStr, DataGlobals.DATE_FORMAT_FULL_YEAR, null, System.Globalization.DateTimeStyles.None, out minDate);

            if (periodIsClosed != null && periodIsClosed.Equals("1") && documDate < minDate)
            {
                        ModelState.AddModelError("Сторнирование документа", "Минимальная дата для документа "+minDate.ToString(DataGlobals.DATE_FORMAT_FULL_YEAR));
            }

            if (ModelState.IsValid)
            {
                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                queryParams.Add("DocNumber", docNumber);
                queryParams.Add("DocDate", documDate);
                queryParams.Add("OperType.Id", stornoType);
                queryParams.Add("[]RefOperation", "");
                IList<Operation> operations = operationRepository.GetByLikeCriteria(queryParams);
                if (operations.Count == 0) {
                    ModelState.AddModelError("Сторнирование документа", "Документн № " + docNumber + " от " + docDate+" не найден или он уже сторнирован!");
                }
                if (ModelState.IsValid)
                {
                    storageRepository.DbContext.BeginTransaction();
                    //operationRepository.DbContext.BeginTransaction();

                    foreach (Operation oper in operations)
                    {
                        if (oper.Storage == null)
                        {
                            ModelState.AddModelError("Сторнирование документа", "Не найдена номенклатура на складе!");
                        }
                        else
                        {
                            Storage stor = oper.Storage;
                            Operation stornoOper = new Operation();
                            stornoOper.DocDate = oper.DocDate;
                            stornoOper.DocNumber = oper.DocNumber;
                            stornoOper.DocType = oper.DocType;
                            stornoOper.Motiv = oper.Motiv;
                            stornoOper.Note = oper.Note;
                            stornoOper.OperDate = oper.OperDate;
                            stornoOper.OperType = oper.OperType;
                            stornoOper.Organization = oper.Organization;
                            stornoOper.Partner = oper.Partner;
                            stornoOper.Quantity = -1 * oper.Quantity;
                            stornoOper.Storage = oper.Storage;
                            stornoOper.StorageName = oper.StorageName;
                            stornoOper.Wear = oper.Wear;
                            stornoOper.WorkerWorkplace = oper.WorkerWorkplace;
                            stornoOper.RefOperation = oper;
                            switch (oper.OperType.Id)
                            {
                                case DataGlobals.OPERATION_STORAGE_IN:
                                    if ((stor.Quantity - oper.Quantity) < 0)
                                        ModelState.AddModelError("Сторнирование документа", "На складе не хватает номенклатуры: " + stor.NomenclatureInfo);
                                    stor.Quantity = stor.Quantity - oper.Quantity;
                                    break;
                                case DataGlobals.OPERATION_STORAGE_OUT:
                                    stor.Quantity = stor.Quantity + oper.Quantity;
                                    break;
                                case DataGlobals.OPERATION_STORAGE_WEAR_OUT:
                                    stor.Quantity = stor.Quantity + oper.Quantity;
                                    break;
                            }
                            if (ModelState.IsValid)
                            {
                                operationRepository.SaveOrUpdate(stornoOper);
                                oper.RefOperation = stornoOper;
                                operationRepository.SaveOrUpdate(oper);
                                storageRepository.SaveOrUpdate(stor);
                            }
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        storageRepository.DbContext.CommitTransaction();
                        //operationRepository.DbContext.CommitTransaction();
                    }
                    else
                    {
                        storageRepository.DbContext.RollbackTransaction();
                        //operationRepository.DbContext.RollbackTransaction();
                    }
                }
            }
            List<Operation> oper1 = new List<Operation>();
            return View(new GridModel(oper1));
        }


        [HttpPost]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_STORNO_EDIT + ", " + DataGlobals.ROLE_STORNO_VIEW))]
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

            orderParams.Add("IsActive", DESC);
            orderParams.Add("Worker.Fio", ASC);

            workerWorkplaces = workerWorkplaceRepository.GetByLikeCriteria(queryParams, orderParams);
            //Поиск происходит по таб. №.
            //оставляем только 1 запись

            if (tabn > 0) {
                while (workerWorkplaces.Count > 1)
                {
                    workerWorkplaces.RemoveAt(0);
                }
            }
            return new JsonResult
            {
                Data = new SelectList(workerWorkplaces, "Worker.Id", "Worker.WorkerInfo")
            };
        }

    }
}
