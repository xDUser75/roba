using System.Web.Mvc;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using Store.Core;
using Telerik.Web.Mvc;
using System.Collections.Generic;
using Store.Data;
using System;
using System.Text;
using SharpArch.Data.NHibernate;
using NHibernate;

namespace Store.Web.Controllers
{
    [HandleError]
    public class WorkerCardsController : ViewedController
    {
        private static readonly string STORAGE_ID_LIST_KEY = "StorageId";
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
        private readonly CriteriaRepository<NomBodyPartSize> nomBodyPartSizeRepository;
        private readonly CriteriaRepository<Config> configRepository;
        private readonly CriteriaRepository<Sex> sexRepository;

        public WorkerCardsController(CriteriaRepository<WorkerCardHead> workerCardRepository,
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
            CriteriaRepository<NomBodyPartSize> nomBodyPartSizeRepository,
            CriteriaRepository<Sex> sexRepository)
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
            this.nomBodyPartSizeRepository = nomBodyPartSizeRepository;
            this.configRepository = configRepository;
            this.sexRepository = sexRepository;
        }

        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_CARD_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_VIEW))]
        public ActionResult Index()
        {
            Dictionary<string, object> query = new Dictionary<string, object>();
            query.Add("Organization.Id", int.Parse(getCurrentEnterpriseId()));
            query.Add("[in]Id", ((Store.Core.Account.User)Session[DataGlobals.ACCOUNT_KEY]).ObjectsCSV);
            IList<StorageName> storageNames = storageNameRepository.GetByCriteria(query);

            //SelectList storageNameList = new SelectList(storageNameRepository.GetStorageShops(getCurrentEnterpriseId()), "Id", "Name", Session["storageNameId"] != null ? (string)Session["storageNameId"] : "0");
            SelectList storageNameList = new SelectList(storageNames, "Id", "Name", Session["storageNameId"] != null ? (string)Session["storageNameId"] : "0");

            //if (Session["storageNameId"] != null)
            //{
            //    foreach (var item in storageNameList)
            //    {
            //        if (item.Value == (string)Session["storageNameId"])
            //            storageNameList.Items.SelectedValue = item;
            //    }
            //}
            ViewData[DataGlobals.REFERENCE_STORAGE_SHOP_NAME] = storageNameList;
            Store.Core.Utils.LockList lockList = (Store.Core.Utils.LockList)HttpContext.Cache.Get(STORAGE_ID_LIST_KEY);
            if (lockList == null) {
                lockList = new Store.Core.Utils.LockList();
                HttpContext.Cache.Insert(STORAGE_ID_LIST_KEY, lockList);
            }
            return View(viewName);
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_CARD_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_VIEW + ", " + DataGlobals.ROLE_WORKER_CARD_RETURN_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_RETURN_VIEW
             + ", " + DataGlobals.ROLE_WORKER_CARD_OUT_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_OUT_VIEW))]
        public ActionResult Select_Worker(int? workerWorkplaceId)
        {
            IList<WorkerWorkplace> workerWorkplaces = new List<WorkerWorkplace>();
            //if (workerWorkplaceId == null && Session["workerWorkplaceId"] != null)
            //    workerWorkplaceId = (int)Session["workerWorkplaceId"];
            if (workerWorkplaceId != null)
            {
                WorkerWorkplace workerWorkplace = workerWorkplaceRepository.Get(workerWorkplaceId.Value);
                workerWorkplaces.Add(rebuildWorkerWorkplace(workerWorkplace));
            }
            return View(new GridModel(workerWorkplaces));
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_CARD_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_VIEW))]
        public ActionResult Select(int? workerWorkplaceId, string workerWorkplaceText, string storageNameId, bool isWinter)
        {
            IList<WorkerNorma> workerNormas = new List<WorkerNorma>();
            WorkerNorma workerNorma = null;
            StorageName storageName = storageNameRepository.Get(int.Parse(storageNameId));
            string storageNumber = storageName.StorageNumber;

            //workerWorkplaceId = (int?)Session["workerWorkplaceId"];

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
                query.Add("Organization", workerWorkplace.Organization);
                query.Add("Norma.Organization.Id", int.Parse(getCurrentEnterpriseId()));
                query.Add("Norma.IsActive", true);
                IList<NormaOrganization> normaOrganizations = normaOrganizationRepository.GetByCriteria(query);
                //NormaOrganization normaOrganization = normaOrganizationRepository.FindOne(query);
                if (normaOrganizations != null && normaOrganizations.Count > 0)
                {
                    NormaOrganization normaOrganization = normaOrganizations[0];

                    // пересортировка - СИЗ в конец списка
                    IList<NormaContent> normaContents = reorderNormaContents(normaOrganization.Norma.NormaContents, isWinter);

                    query.Clear();
                    query.Add("WorkerWorkplace", workerWorkplace);
                    WorkerCardHead workerCardHead = workerCardRepository.FindOne(query);
                    WorkerCardContent workerCardContent = null;
                    //NomGroup nomGroup;
                    //NormaNomGroup normaNomGroup;
                    //NormaContent normaContent;
                    //int quantOnStorage;

                    foreach (NormaContent curNormaContent in normaContents)
                    {
                        if (workerCardHead != null)
                            // ищем на руках номенклатуру по текущей группе номенклатур
                            workerCardContent = getWorkerCardPresent(curNormaContent.NormaNomGroups, workerCardHead.getActiveWorkerCardContent());
                        // на неактивном рабочем месте показывает только то, что есть на руках по норме
                        if (workerWorkplace.IsActive == false && workerCardContent == null)
                            continue;
                        //workerNormas.Add(new WorkerNorma(normaContent, workerCardContent, curNormaContent.Quantity));
                        //workerNorma = new WorkerNorma(nomGroup, workerCardContent, curNormaContent.Quantity);

                        workerNorma = new WorkerNorma();
                        //if (workerCardContent != null)
                        //    workerNorma.Storage = rebuildStorage(workerCardContent.Storage);
                        //else
                        //    workerNorma.Storage = rebuildStorage(storage);
                        workerNorma.IsCorporate = false;
                        workerNorma.NormaContentId = curNormaContent.Id;
                        workerNorma.NormaContentName = curNormaContent.NomGroup.Name;
                        workerNorma.NormaQuantity = curNormaContent.Quantity;
                        workerNorma.NormaUsePeriod = curNormaContent.UsePeriod;
                        workerNorma.PutQuantity = curNormaContent.Quantity;
                        if (workerCardContent != null)
                        {
                            if (workerCardContent.Quantity > 0)
                            {
                                
                                workerNorma.PresentQuantity = workerCardContent.Quantity;
//                                workerNorma.ReceptionDate = workerCardContent.Operation.OperDate;
                                workerNorma.ReceptionDate = workerCardContent.StartDate;
                                if (workerCardContent.Operation.OperType.Id == DataGlobals.OPERATION_STORAGE_TRANSFER_IN && workerCardContent.Operation.TransferOperation!=null) {
                                    //query.Clear();
                                    //query.Add("OperReturn", workerCardContent.Operation.TransferOperation);
                                    //IList<WorkerCardContent> wccs = workerCardContentRepository.GetByLikeCriteria(query);
                                    //if (wccs.Count > 0)
                                    //{
                                    //    if (wccs[0].Operation.Id ==DataGlobals.OPERATION_WORKER_IN)
                                    //        workerNorma.DocNumber = wccs[0].Operation.DocNumber;
                                    //}
                                    workerNorma.DocNumber = getDocNumber(workerCardContent.Operation.TransferOperation);
                                }
                                else 
                                    workerNorma.DocNumber = workerCardContent.Operation.DocNumber;

                                if (workerCardContent.Quantity < curNormaContent.Quantity) {
                                    Storage storage = getStorageForWorkerNorma(curNormaContent, workerWorkplace.Worker, storageNameId);
                                    workerNorma.StorageId = storage.Id;
                                    workerNorma.StorageInfo = storage.StorageInfo;

                                    if (storage.Id != 0)
                                        workerNorma.StorageNumber = storage.StorageName.StorageNumber;
                                    else
                                        workerNorma.StorageNumber = storageNumber;
                                }
                                else
                                {
                                    workerNorma.StorageId = workerCardContent.Storage.Id;
                                    workerNorma.StorageNumber = workerCardContent.Storage.StorageName.StorageNumber;
                                    workerNorma.StorageInfo = workerCardContent.Storage.StorageInfo;
                                }
                                workerNorma.IsCorporate = workerCardContent.IsCorporate;
                                workerNorma.OperationId = workerCardContent.OperationId;
                                workerNorma.OperTypeId = workerCardContent.Operation.OperType.Id;
                                workerNorma.WorkerCardContentId = workerCardContent.WorkerCardContentId;
                                //Удаляем номенклатуру из списка поиска.
                                //Иначе могут двоиться записи (если основная группа и группа замены
                                //ссылаются друг на друга)
                                workerCardHead.removeWorkerCardContent(workerCardContent);                                
                            }
                            else
                            {
                                Storage storage = getStorageForWorkerNorma(curNormaContent, workerWorkplace.Worker, storageNameId);
                                    workerNorma.StorageId = storage.Id;
                                    workerNorma.StorageInfo = storage.StorageInfo;

                                    if (storage.Id != 0)
                                        workerNorma.StorageNumber = storage.StorageName.StorageNumber;
                                    else
                                        workerNorma.StorageNumber = storageNumber;
                            }
                        }
                        else
                        {
                            Storage storage = getStorageForWorkerNorma(curNormaContent, workerWorkplace.Worker, storageNameId);
                            workerNorma.StorageId = storage.Id;
                            workerNorma.StorageInfo = storage.StorageInfo;

                            if (storage.Id != 0)
                                workerNorma.StorageNumber = storage.StorageName.StorageNumber;
                            else
                                workerNorma.StorageNumber = storageNumber;
                            if (workerCardContent.Quantity < curNormaContent.Quantity) { 
                            
                            }
                        
                        }
                        //if (workerCardContent.Operation != null && workerCardContent.Operation.OperDate.AddMonths(curNormaContent.UsePeriod) > DateTime.Now)
                        //if (workerCardContent != null && workerCardContent.Operation.OperDate.AddMonths(curNormaContent.UsePeriod) > DateTime.Now)
                        if (workerCardContent != null)
                            if (workerCardContent.Quantity > 0)
                            {
                                workerNorma.PresentQuantity = workerCardContent.Quantity;
                                DateTime dt = workerCardContent.Operation.OperDate.AddMonths(curNormaContent.UsePeriod);
                                double qq = double.Parse((workerNorma.PresentQuantity/workerNorma.PutQuantity).ToString());
//                                if (workerNorma.PutQuantity > 0 && workerCardContent.Operation.OperDate.AddMonths(curNormaContent.UsePeriod * (workerNorma.PresentQuantity / workerNorma.PutQuantity)) > DateTime.Now)
                                if (workerNorma.PutQuantity > 0 && workerNorma.PresentQuantity>0)
                                        workerNorma.PutQuantity -= workerCardContent.Quantity;
                                else
                                    workerNorma.PutQuantity = workerCardContent.Quantity;
//                                workerNorma.ReceptionDate = workerCardContent.Operation.OperDate;
                                workerNorma.ReceptionDate = workerCardContent.StartDate;
                            }
                        workerNormas.Add(workerNorma);
                    }

//                    foreach (WorkerCardContent curWorkerCard in workerCards[0].WorkerCardContents)
//                    {
////                        nomCroup = new NomGroup(curWorkerCard.Storage.Nomenclature.NomGroup.Id, curWorkerCard.Storage.Nomenclature.NomGroup.Name);
//                        normaContent = new NormaContent();
////                        normaContent.NomGroup = nomCroup;
//                        quantOnStorage = 0;
////                        quantOnStorage = getQuantityNomOnStorageByNomGroup(nomCroup.Id);
                        
//                        workerCard = rebuildWorkerCard(curWorkerCard);
//                        //workerNormas.Add(new WorkerNorma(normaContent, workerCard, quantOnStorage));
//                    }
                }

            }
            return View(new GridModel(workerNormas));
            //return View(viewName, normaContents);
            //return View(viewName);
        }

// рекурсивный метод поиска номера документа по которому производилась выдача, дабы после всех переводов найти все-таки этот номер
        private string  getDocNumber (Operation oper )
        {
            string docNumber = "";
            Dictionary<string, object> query = new Dictionary<string, object>();
            query.Add("OperReturn", oper);
            IList<WorkerCardContent> wccs = workerCardContentRepository.GetByLikeCriteria(query);
            if (wccs.Count > 0)
            {
                if (wccs[0].Operation.OperType.Id == DataGlobals.OPERATION_WORKER_IN)
                {
                    docNumber = wccs[0].Operation.DocNumber;
                    return docNumber;
                }
                else
                {
                   if (wccs[0].Operation.TransferOperation!=null)
                       docNumber = getDocNumber(wccs[0].Operation.TransferOperation);
                }
            }
            return docNumber;
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

        private Storage getStorageForWorkerNorma(NormaContent normaContent, Worker worker, string storageNameId)
        {
            List<Storage> storages = new List<Storage>();
            NomBodyPart normaBodyPart = normaContent.NomGroup.NomBodyPart;
            NomBodyPartSize workerBodyPartSize = null;

            // ищем размер сотрудника для данной нормы
            foreach (var item in worker.NomBodyPartSizes)
            {
                if (item.NomBodyPart.Id == normaBodyPart.Id)
                {
                    workerBodyPartSize = item;
                    break;
                }
            }

            Dictionary<string, object> query = new Dictionary<string, object>();
            Dictionary<string, object> order = new Dictionary<string, object>();
            
            // попытка найти полное соответствие по всем параметрам
            // в том числе и в группах замены, только пол рассматриваем + унисекс
            query.Add("StorageName.Organization", organizationRepository.Get(int.Parse(getCurrentEnterpriseId())));
            query.Add("StorageName", storageNameRepository.Get(int.Parse(storageNameId)));
            query.Add("[>]Quantity", 0);
            query.Add("Wear", "100");
            //Добавляем обязательный поиск по унисексу и сортируем по ID, т.е. вначале 1 или 2 потом 3. 
            //Номенклатура подберется, если есть нужного пола, если нет, то унисекс
            StringBuilder sexes = new StringBuilder();
            if (worker.Sex != null)
            {
                sexes.Append("'" + worker.Sex.Id + "',");
            }
            sexes.Append("'" + DataGlobals.UNISEX_ID + "'");
//            query.Add("[in]Nomenclature.Sex", worker.Sex);
            query.Add("[in]Nomenclature.Sex.Id", sexes.ToString());
            order.Add("Nomenclature.Sex.Id", ASC);

            if (workerBodyPartSize != null && DataGlobals.SIZ_SIZE_ID != normaBodyPart.Id)
            {
                query.Add("NomBodyPartSize.NomBodyPart", normaBodyPart);
                query.Add("NomBodyPartSize.SizeNumber", workerBodyPartSize.SizeNumber);
                order.Add("NomBodyPartSize.SizeNumber", ASC);
            }
            if (DataGlobals.CLOTH_SIZE_ID == normaBodyPart.Id)
            {
                query.Add("Growth.SizeNumber", worker.Growth);
                order.Add("Growth.SizeNumber", ASC);
            }
            order.Add("Wear", DESC);

            // формирование основной группы
            StringBuilder nomGroupBase = new StringBuilder();
            //Список групп замены
            StringBuilder nomGroups = new StringBuilder();
            foreach (var normaNomGroup in normaContent.NormaNomGroups)
            {
                if (normaNomGroup.IsBase == true)
                {
                    nomGroupBase.Append("'" + normaNomGroup.NomGroup.Id + "',");
                    break;
                }
            }
            nomGroupBase.Remove(nomGroupBase.Length - 1, 1);

            foreach (var normaNomGroup in normaContent.NormaNomGroups)
            {
                if (normaNomGroup.IsBase == true)
                    continue;
                nomGroups.Append("'" + normaNomGroup.NomGroup.Id + "',");
            }

            // разделил запросы по основной группе и группам замены,
            // чтобы сначала списка шли позиции по основной группе
            query.Add("[in]Nomenclature.NomGroup.Id", nomGroupBase.ToString());
            storages.AddRange(storageRepository.GetByLikeCriteria(query, order));

            if (nomGroups.Length > 0)
            {
                nomGroups.Remove(nomGroups.Length - 1, 1);
                query.Remove("[in]Nomenclature.NomGroup.Id");
                query.Add("[in]Nomenclature.NomGroup.Id", nomGroups.ToString());
                storages.AddRange(storageRepository.GetByLikeCriteria(query, order));
            }

            
            // если не нашли, то попытка найти одежду прежнего размера, но большего раста в основной группе
            if (DataGlobals.CLOTH_SIZE_ID == normaBodyPart.Id)
            {
                //Больший рост
                NomBodyPartSize biggerSizeNumber = null;
                if (storages == null || storages.Count == 0)
                {
                    biggerSizeNumber = getBiggerSize(DataGlobals.GROWTH_SIZE_ID, worker.Growth);
                    if (biggerSizeNumber != null)
                    {
                        query.Remove("Growth.SizeNumber");
                        query.Remove("[in]Nomenclature.NomGroup.Id");
                        query.Add("Growth.SizeNumber", biggerSizeNumber.SizeNumber);
                        query.Add("[in]Nomenclature.NomGroup.Id", nomGroupBase.ToString());

                        storages.AddRange(storageRepository.GetByLikeCriteria(query, order));
                    }
                }
                // если не нашли, то попытка найти одежду прежнего размера, но большего раста в группах замены
                if ((storages == null || storages.Count == 0) && (biggerSizeNumber != null) && (nomGroups.Length > 0))
                {
                    query.Remove("[in]Nomenclature.NomGroup.Id");
                    query.Add("[in]Nomenclature.NomGroup.Id", nomGroups.ToString());
                    storages.AddRange(storageRepository.GetByLikeCriteria(query, order));
                }
            }

            // если не нашли, то попытка найти одежду большего размера, но прежнего раста
            if (workerBodyPartSize != null && DataGlobals.SIZ_SIZE_ID != normaBodyPart.Id)
            {
                //Больший размер
                NomBodyPartSize biggerSizeNumber = null;

                if (storages == null || storages.Count == 0)
                {
                    biggerSizeNumber = getBiggerSize(DataGlobals.CLOTH_SIZE_ID, workerBodyPartSize.SizeNumber);
                    if (biggerSizeNumber != null)
                    {
                        query.Remove("NomBodyPartSize.SizeNumber");
                        query.Remove("[in]Nomenclature.NomGroup.Id");

                        query.Add("NomBodyPartSize.SizeNumber", biggerSizeNumber.SizeNumber);
                        query.Add("[in]Nomenclature.NomGroup.Id", nomGroupBase.ToString());
                        if (DataGlobals.CLOTH_SIZE_ID == normaBodyPart.Id)
                        {
                            query.Remove("Growth.SizeNumber");
                            query.Add("Growth.SizeNumber", worker.Growth);

                        }
                        storages.AddRange(storageRepository.GetByLikeCriteria(query, order));
                    }
                }
                // если не нашли, то попытка найти одежду большего размера, но прежнего раста в группах замены
                if ((storages == null || storages.Count == 0) && (biggerSizeNumber != null) && (nomGroups.Length > 0))
                {
                    query.Remove("[in]Nomenclature.NomGroup.Id");
                    query.Add("[in]Nomenclature.NomGroup.Id", nomGroups.ToString());
                    storages.AddRange(storageRepository.GetByLikeCriteria(query, order));
                }

            }

            // если не нашли, то попытка поиска без роста и пола
            if (storages == null || storages.Count == 0)
            {
                //Восстанавливаем прежний размер
                if (workerBodyPartSize != null && DataGlobals.SIZ_SIZE_ID != normaBodyPart.Id)
                {
                    query.Remove("NomBodyPartSize.SizeNumber");
                    query.Add("NomBodyPartSize.SizeNumber", workerBodyPartSize.SizeNumber);
                }

                query.Remove("Growth.SizeNumber");
                query.Remove("[in]Nomenclature.Sex.Id");
                query.Remove("[in]Nomenclature.NomGroup.Id");
                query.Add("[in]Nomenclature.NomGroup.Id", nomGroupBase.ToString());
                storages.AddRange(storageRepository.GetByLikeCriteria(query, order));

                if (nomGroups.Length > 0)
                {
                    query.Remove("[in]Nomenclature.NomGroup.Id");
                    query.Add("[in]Nomenclature.NomGroup.Id", nomGroups.ToString());
                    storages.AddRange(storageRepository.GetByLikeCriteria(query, order));
                }
            }

            // если не нашли, то попытка поиска без размера
            if (storages == null || storages.Count == 0)
            {
                query.Remove("NomBodyPartSize.NomBodyPart");
                query.Remove("NomBodyPartSize.SizeNumber");

                query.Remove("[in]Nomenclature.NomGroup.Id");
                query.Add("[in]Nomenclature.NomGroup.Id", nomGroupBase.ToString());
                storages.AddRange(storageRepository.GetByLikeCriteria(query, order));

                if (nomGroups.Length > 0)
                {
                    query.Remove("[in]Nomenclature.NomGroup.Id");
                    query.Add("[in]Nomenclature.NomGroup.Id", nomGroups.ToString());
                    storages.AddRange(storageRepository.GetByLikeCriteria(query, order));
                }
            }

            // если у позиций нет размеров, то из сортировки их нужно убрать,
            // чтобы запрос вернул хоть что-то
            if (storages == null || storages.Count == 0)
            {
                order.Remove("NomBodyPartSize.SizeNumber");
                order.Remove("Growth.SizeNumber");

                query.Remove("[in]Nomenclature.NomGroup.Id");
                query.Add("[in]Nomenclature.NomGroup.Id", nomGroupBase.ToString());
                storages.AddRange(storageRepository.GetByLikeCriteria(query, order));

                if (nomGroups.Length > 0)
                {
                    query.Remove("[in]Nomenclature.NomGroup.Id");
                    query.Add("[in]Nomenclature.NomGroup.Id", nomGroups.ToString());
                    storages.AddRange(storageRepository.GetByLikeCriteria(query, order));
                }
            }

            // если совсем ничего не найдено, возвращаем пустую позицию
            if (storages == null || storages.Count == 0)
            {
                storages.Add(new Storage());
            }

            // попытка найти позицию большего размера
            if (storages.Count > 1 && workerBodyPartSize != null && DataGlobals.SIZ_SIZE_ID != normaBodyPart.Id)
            {
                String nextSizeNumber = null;
                for (int i = 1; i < 3; i++)
                {
                    nextSizeNumber = (double.Parse(workerBodyPartSize.SizeNumber) + i).ToString();
                    foreach (var item in storages)
                    {
                        if (item.NomBodyPartSize != null && nextSizeNumber.Equals(item.NomBodyPartSize.SizeNumber))
                            return item;
                    }
                }
            }

            return storages[0];
        }

        private WorkerCardContent getWorkerCardPresent(IList<NormaNomGroup> normaNomGroups, IList<WorkerCardContent> workerCards)
        {
            int workerNomGroupId;
//            int quntity;
            WorkerCardContent outWorkerCard = new WorkerCardContent();
//            WorkerCardContent outWorkerCard = null;
            NomGroup nomGroup;

            foreach (NormaNomGroup item in normaNomGroups)
            {
                nomGroup = item.NomGroup;
                foreach (WorkerCardContent curWorkerCard in workerCards)
                {
//                    workerNomGroupId = curWorkerCard.Storage.Nomenclature.NomGroup.Id;
                    if (curWorkerCard.NormaContent != null)
                        workerNomGroupId = curWorkerCard.NormaContent.NomGroup.Id;

                    else
                        workerNomGroupId = curWorkerCard.Storage.Nomenclature.NomGroup.Id;

                    if (curWorkerCard.Quantity > 0 && nomGroup.Id == workerNomGroupId)
                    {
                        if (outWorkerCard.NormaContent == null)
                        {
//                            outWorkerCard.Quantity += curWorkerCard.Quantity;
                            outWorkerCard.Operation = new Operation();
                            outWorkerCard.Operation.OperDate = curWorkerCard.Operation.OperDate;
                            outWorkerCard.StartDate = curWorkerCard.StartDate;
                            outWorkerCard.Operation.DocNumber = curWorkerCard.Operation.DocNumber;
                            outWorkerCard.Storage = curWorkerCard.Storage;
                            outWorkerCard.NormaContent = curWorkerCard.NormaContent;
                            outWorkerCard.IsCorporate = curWorkerCard.IsCorporate;
                            outWorkerCard.OperationId = curWorkerCard.Operation.Id;
                            outWorkerCard.WorkerCardContentId = curWorkerCard.Id;
                            outWorkerCard.Operation = curWorkerCard.Operation;
                            outWorkerCard.GiveOperation = curWorkerCard.GiveOperation;
                        }
  //                      else
                            outWorkerCard.Quantity += curWorkerCard.Quantity;
                   }
                }
            }

            return outWorkerCard;
        }

        private WorkerCardContent getWorkerCardMove(IList<NormaNomGroup> normaNomGroups, IList<WorkerCardContent> workerCards)
        {
            int workerNomGroupId;
//            int quntity;
            WorkerCardContent outWorkerCard = null;
            NomGroup nomGroup;

            foreach (NormaNomGroup item in normaNomGroups)
            {
                nomGroup = item.NomGroup;
                foreach (WorkerCardContent curWorkerCard in workerCards)
                {
                    //                    workerNomGroupId = curWorkerCard.Storage.Nomenclature.NomGroup.Id;
                    if (curWorkerCard.NormaContent != null)
                        workerNomGroupId = curWorkerCard.NormaContent.NomGroup.Id;

                    else
                        workerNomGroupId = curWorkerCard.Storage.Nomenclature.NomGroup.Id;

                    if (curWorkerCard.Quantity > 0 && nomGroup.Id == workerNomGroupId)
                    {
                        if (outWorkerCard == null)
                        {
                                outWorkerCard = curWorkerCard;
                               break;
                        }
                    }
                }
            }

            return outWorkerCard;
        }

        private WorkerCardContent getWorkerCard(IList<NormaNomGroup> normaNomGroups, IList<WorkerCardContent> workerCards, int storageId)
        {
            int workerNomGroupId;
            WorkerCardContent outWorkerCard = null;
            NomGroup nomGroup;

            foreach (NormaNomGroup item in normaNomGroups)
            {
                nomGroup = item.NomGroup;
                foreach (WorkerCardContent curWorkerCard in workerCards)
                {
                    workerNomGroupId = curWorkerCard.Storage.Nomenclature.NomGroup.Id;
                    if (nomGroup.Id == workerNomGroupId && curWorkerCard.Storage.Id == storageId && curWorkerCard.Quantity == 0)
                    {
                        //workerCard = rebuildWorkerCard(curWorkerCard);
                        outWorkerCard = curWorkerCard;
                        break;
                    }
                }
            }

            return outWorkerCard;
        }

        private WorkerCardContent rebuildWorkerCard(WorkerCardContent inWorkerCard)
        {
            WorkerCardContent outWorkerCard;
            Storage storage;
            Operation operation, giveOperation;

            storage = rebuildStorage(inWorkerCard.Storage);
            operation = rebuildOperation(inWorkerCard.Operation);
            giveOperation = rebuildOperation(inWorkerCard.GiveOperation);

            outWorkerCard = new WorkerCardContent(inWorkerCard.Id);
            outWorkerCard.Storage = storage;
            outWorkerCard.Operation = operation;
            outWorkerCard.GiveOperation = giveOperation;
            // outWorkerCard.StartDate = operation.OperDate;
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
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_WORKER_CARD_EDIT))]
        public ActionResult Update([Bind(Prefix = "updated")]IEnumerable<WorkerNorma> workerNormas,string storageNameId)
        {
            Dictionary<string, object> query = new Dictionary<string, object>();
            int workerWorkplaceId = (int)Session["workerWorkplaceId"];
            DateTime? operDate = null;
            // перове число текущего месяца
            DateTime firstDay = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            // первое число предыдущего месяца
            DateTime minDate = firstDay.AddMonths(-1);
            /*
            string periodIsClosed = getConfigParamValue(configRepository, "periodIsClosed", getCurrentEnterpriseId());
            string periodDateStr = getConfigParamValue(configRepository, "periodDate", getCurrentEnterpriseId());
            DateTime.TryParseExact(periodDateStr, DataGlobals.DATE_FORMAT_FULL_YEAR, null, System.Globalization.DateTimeStyles.None, out minDate);
            */
            string docNumber = operationRepository.GetNextDocNumber(DataGlobals.OPERATION_WORKER_IN, getCurrentEnterpriseId());

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
                    }
                    if (workerNorma.PutQuantity <= 0)
                    {
                        Operation oper1 = operationRepository.Get(workerNorma.OperationId);
                        if (oper1 != null)
                        {
                            WorkerCardContent workerCardContent1 = workerCardContentRepository.Get(workerNorma.WorkerCardContentId);
                            oper1.IsCorporate = workerNorma.IsCorporate;
                            workerCardContent1.IsCorporate = workerNorma.IsCorporate;
                            operationRepository.SaveOrUpdate(oper1);
                            workerCardContentRepository.SaveOrUpdate(workerCardContent1);
                        }
                        continue;
                    }
                        //return Select(workerWorkplaceId);
                    //NormaContent normaContent = normaContentRepository.Get(workerNorma.NormaContentId);

                    Storage storage = null;

                    if (workerNorma.StorageId == 0)
                        ModelState.AddModelError("updated[" + i + "].Storage", "Выберите номенклатуру");
                    else
                    {
                        string periodDateStr = getConfigParamValue(configRepository, "periodDate", getCurrentEnterpriseId());
                        string periodIsClosed = getConfigParamValue(configRepository, "periodIsClosed", getCurrentEnterpriseId());
                        DateTime periodDate;
                        DateTime.TryParseExact(periodDateStr, DataGlobals.DATE_FORMAT_FULL_YEAR, null, System.Globalization.DateTimeStyles.None, out periodDate);
                        if (operDate != null && DateTime.Now < operDate)
                            ModelState.AddModelError(operDate + ": ", "Дата операции должна быть не больше текущей даты и не меньше 1-го числа предыдущего месяца");

                        storage = storageRepository.Get(workerNorma.StorageId);

                        if (firstDay > operDate)
                        {
                            // проверка на закрытие периода
                            if (periodIsClosed != null && periodIsClosed.Equals("1") && periodDate > operDate)
                                ModelState.AddModelError(operDate + ": ", "Период закрыт для изменений");
                            else
                            {
                                // ищем остатки по текущему месяцу
                                query.Clear();
                                query.Add("Storage", storage);
                                query.Add("RemaindDate", firstDay);
                                IList<Remaind> currentRemainds = remainsRepository.FindAll(query);
                                if (currentRemainds.Count > 0 && currentRemainds[0].Quantity < 1)
                                    ModelState.AddModelError(storage.Nomenclature.Name + ": ", "Остатки на складе на текущий период равны 0");
                            }
                        }

                        if (storage.Quantity - workerNorma.PutQuantity < 0)
                            ModelState.AddModelError(storage.Nomenclature.Name+": ", "На складе недостаточно кол-ва. На складе "+storage.Quantity);

                        if (workerNorma.PutQuantity > workerNorma.NormaQuantity)
                            ModelState.AddModelError(storage.Nomenclature.Name + ": ", "Превышено разрешенное по норме кол-во");

                        if ((workerNorma.PresentQuantity>0) && (workerNorma.PresentQuantity + workerNorma.PutQuantity > workerNorma.NormaQuantity))
                            ModelState.AddModelError(storage.Nomenclature.Name+": ", "На руках есть номенклатура у которой не истек срок");

                    }
                }
                query.Clear();
                query.Add("Id", workerWorkplaceId);
                WorkerWorkplace workerWorkplace = workerWorkplaceRepository.Get(workerWorkplaceId);
                if (workerWorkplace.Worker.IsTabu){
                    ModelState.AddModelError("","Выдача по норме запрещена!");
                }


                if (ModelState.IsValid)
                {
                    ISession ses = NHibernateSession.Storage.GetSessionForKey(NHibernateSession.DefaultFactoryKey);
                    foreach (var workerNorma in workerNormas)
                    {
                        if (workerNorma.PutQuantity <= 0)
                            continue;

                        Store.Core.Utils.LockList lockList = (Store.Core.Utils.LockList)HttpContext.Cache.Get(STORAGE_ID_LIST_KEY);
                        /*
                        if (lockList == null)
                        {
                            lockList = new Store.Core.Utils.LockList();
                            HttpContext.Cache.Insert(STORAGE_ID_LIST_KEY, lockList);
                        }
                         */
                        while (!lockList.addId(workerNorma.StorageId)) {}
                        ITransaction tx=ses.BeginTransaction();
                        try
                        {
                            query.Clear();
                            query.Add("WorkerWorkplace", workerWorkplace);

                            WorkerCardHead workerCardHead = workerCardRepository.FindOne(query);
                            if (workerCardHead == null)
                            {
                                workerCardHead = new WorkerCardHead();
                                workerCardHead.WorkerCardContents = new List<WorkerCardContent>();
                                workerCardHead.WorkerWorkplace = workerWorkplace;
                            }
                            WorkerCardContent workerCardContent = null;
                            workerCardContent = new WorkerCardContent();

                            Storage storage = storageRepository.Get(workerNorma.StorageId);
                            //replaceIfExist(workerCardHead.WorkerCardContents, workerCardContent, workerNorma);

                                Operation oper = null;
                                workerCardContent.NormaContent = normaContentRepository.Get(workerNorma.NormaContentId);
                                // проверка на уже выполненную операцию. Для случая одновременного открытия форм выдачи. Перед записью в БД проверяем вдруг операция уже была выполнена
                                WorkerCardContent workerCardContentReal = getWorkerCardPresent(workerCardContent.NormaContent.NormaNomGroups, workerCardHead.getActiveWorkerCardContent());
                                if ((workerCardContentReal.Quantity > 0) && (workerCardContentReal.Quantity + workerNorma.PutQuantity > workerNorma.NormaQuantity))
                                    ModelState.AddModelError("ОБНОВИТЕ СТРАНИЦУ!!! " + storage.Nomenclature.Name + ": ", "На руках есть номенклатура у которой не истек срок");

                                if (ModelState.IsValid)
                                {

                                    storage.Quantity -= workerNorma.PutQuantity;

                                    oper = new Operation();
                                    oper.OperDate = operDate.Value;
                                    oper.OperType = operTypeRepository.Get(DataGlobals.OPERATION_WORKER_IN);
                                    oper.Organization = organizationRepository.Get(workerWorkplace.RootOrganization);
                                    oper.Quantity = workerNorma.PutQuantity;
                                    oper.Storage = storage;
                                    oper.Wear = storage.Wear;
                                    oper.DocNumber = docNumber;
                                    oper.DocDate = oper.OperDate;
                                    oper.WorkerWorkplace = workerWorkplace;
                                    oper.IsCorporate = workerNorma.IsCorporate;
                                    oper.GiveOperation = oper;

                                    workerCardContent.Storage = storage;
                                    workerCardContent.Quantity = workerNorma.PutQuantity;
                                    workerCardContent.Operation = oper;
//                                    workerCardContent.NormaContent = normaContentRepository.Get(workerNorma.NormaContentId);
                                    workerCardContent.WorkerCardHead = workerCardHead;
                                    workerCardContent.StartDate = oper.OperDate;
                                    workerCardContent.IsCorporate = workerNorma.IsCorporate;
                                    workerCardContent.GiveOperation = oper;

                                    workerCardHead.WorkerCardContents.Add(workerCardContent);

                                    //storageRepository.SaveOrUpdate(storage);
                                    operationRepository.SaveOrUpdate(oper);
                                    //workerCardContentRepository.SaveOrUpdate(workerCardContent);
                                    workerCardRepository.SaveOrUpdate(workerCardHead);

                                    if (firstDay > operDate)
                                    {
                                        //пересчитываем остатки
                                        rebuildRemaind(operationRepository, remainsRepository, storage, minDate, firstDay.AddSeconds(-1), firstDay);
                                    }
                                }
                                tx.Commit();
                                lockList.removeId(workerNorma.StorageId);
                        }
                        catch (Exception e)
                        {
                            tx.Rollback();
                            lockList.removeId(workerNorma.StorageId);
                            throw e;
                        }

                    }
                }
            }
            // Отказались от галочки "Показывать зимнюю одежду". Теперь показывать нужно всю и всегда
            //            bool isWinter = bool.Parse(Request.Cookies["isWinter"] != null ? Request.Cookies["isWinter"].Value : bool.FalseString);
            bool isWinter = true;
            return Select(workerWorkplaceId, (string)Session["workerWorkplaceText"], (string)Session["storageNameId"], isWinter);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_WORKER_CARD_EDIT))]
        public ActionResult CancelOper(DateTime OperDate)
        {
            // перове число текущего месяца
            DateTime firstDay = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            // первое число предыдущего месяца
            DateTime minDate = firstDay.AddMonths(-1);

            int workerWorkplaceId = (int)Session["workerWorkplaceId"];

            Dictionary<string, object> query = new Dictionary<string, object>();
            query.Add("WorkerWorkplace.Id", workerWorkplaceId);
            query.Add("OperType.Id", DataGlobals.OPERATION_WORKER_IN);
            query.Add("OperDate", OperDate);

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

            foreach (var item in operations)
            {
                if (item.DocNumber != lastDocNumber)
                    continue;
                Storage storage = item.Storage;
                storage.Quantity += item.Quantity;
                storageRepository.SaveOrUpdate(storage);

                WorkerCardContent workerCardContent = null;
                foreach (var itemContent in workerCardHead.WorkerCardContents)
                {
                    if (itemContent.Operation == item)
                    {
                        workerCardContent = itemContent;
                        break;
                    }
                }

                workerCardHead.WorkerCardContents.Remove(workerCardContent);

                workerCardContentRepository.Delete(workerCardContent);

                operationRepository.Delete(item);

                if (firstDay > OperDate)
                {
                    //пересчитываем остатки
                    rebuildRemaind(operationRepository, remainsRepository, storage, minDate, firstDay.AddSeconds(-1), firstDay);
                }
            }

            workerCardRepository.SaveOrUpdate(workerCardHead);

            //return Index();
            //return RedirectToAction("Index");
            //return Select(workerWorkplaceId, (string)Session["workerWorkplaceText"], (string)Session["storageNameId"] );
            return null;
        }

   

        [AcceptVerbs(HttpVerbs.Post)]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_WORKER_CARD_EDIT))]
        public ActionResult Moving(int workerWorkplaceNotActive, int workerWorkplaceActive)
        {
            Dictionary<string, object> query = new Dictionary<string, object>();

            WorkerWorkplace wwpActive = workerWorkplaceRepository.Get(workerWorkplaceActive);

            query.Add("WorkerWorkplace.Id", workerWorkplaceNotActive);
            IList<WorkerCardHead> wchsNotActive = workerCardRepository.GetByCriteria(query);

            WorkerCardHead wchNotActive = null;
            if (wchsNotActive.Count > 0)
                wchNotActive = wchsNotActive[0];

            query.Clear();
            query.Add("WorkerWorkplace.Id", workerWorkplaceActive);
            IList<WorkerCardHead> wchsActive = workerCardRepository.GetByCriteria(query);
            
            WorkerCardHead wchActive = null;
            if (wchsActive.Count > 0)
                wchActive = wchsActive[0];
            else
                wchActive = new WorkerCardHead();

            query.Clear();
            query.Add("Organization", wwpActive.Organization);
            NormaOrganization no = normaOrganizationRepository.FindOne(query);

            if (wchNotActive != null && no != null)
            {
                WorkerCardContent wcc = null;
                foreach (var item in no.Norma.NormaContents)
                {
                    wcc = getWorkerCardMove(item.NormaNomGroups, wchNotActive.WorkerCardContents);
                    if (wcc != null)
                    {
                        WorkerCardContent wccNew = new WorkerCardContent();
                        wccNew.StartDate = wcc.StartDate;
                        wccNew.Storage = wcc.Storage;
                        wccNew.Operation = wcc.Operation;
                        wccNew.Quantity = wcc.Quantity;
                        wccNew.NormaContent = wcc.NormaContent;
                        wccNew.WorkerCardHead = wchActive;
                        wchActive.WorkerCardContents.Add(wccNew);

                        //wchNotActive.WorkerCardContents.Remove(wcc);
                        //workerCardContentRepository.Delete(wcc);
                        wcc.Quantity = 0;
                    }
                }

                workerCardRepository.SaveOrUpdate(wchNotActive);
                workerCardRepository.SaveOrUpdate(wchActive);
            }

            return null;
        }

        //private void replaceIfExist(IList<WorkerCardContent> workerCardContents, WorkerCardContent workerCardContent, WorkerNorma workerNorma)
        //{
        //    bool isFind = false;
        //    foreach (WorkerCardContent item in workerCardContents)
        //    {
        //        if (item.Storage.Id == workerCardContent.Storage.Id)
        //        {
        //            isFind = true;
        //            if (workerNorma.NormaQuantity - workerNorma.PresentQuantity <= 0)
        //            {
        //                Operation oper = new Operation();
        //                oper.OperDate = DateTime.Now;
        //                oper.OperType = operTypeRepository.Get(DataGlobals.OPERATION_WORKER_OUT);
        //                oper.Organization = organizationRepository.Get(item.WorkerCardHead.WorkerWorkplace.RootOrganization);
        //                oper.Quantity = workerCardContent.Quantity;
        //                oper.Storage = workerCardContent.Storage;
        //                //oper.Storage.Quantity -= oper.Quantity;
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
        public ActionResult _GetNomenclaturesOnStorage(string normaContentId, string storageNameId, string text)
        {
             Dictionary<string, object> query = new Dictionary<string, object>();
             Dictionary<string, object> order = new Dictionary<string, object>();
            
            List<Storage> storages = new List<Storage>();
            int code = -1;
            /*
            if (int.TryParse(text.Substring(2), out code))
                query.Add("Nomenclature.ExternalCode", text);
            else
                query.Add("Nomenclature.Name", text);
*/
            if (text.Length > 2 && int.TryParse(text.Substring(2), out code))
                query.Add("Nomenclature.ExternalCode", text);
            else
            {
                if (text.Length <=2) text = "";
                query.Add("Nomenclature.Name", text);
            }
            Organization currentOrg = organizationRepository.Get(int.Parse(getCurrentEnterpriseId()));
            query.Add("StorageName.Organization", currentOrg);
            query.Add("[>]Quantity", 0);
            query.Add("[!=]Wear", "50");
            order.Add("Wear", DESC);
            order.Add("Nomenclature.Sex.Id", ASC);
            order.Add("NomBodyPartSize.SizeNumber", ASC);

            if (normaContentId != null)
            {
                query.Add("StorageName.Id", int.Parse(storageNameId));
                NormaContent normaContent = normaContentRepository.Get(int.Parse(normaContentId));

                // сортировка по размеру, но если не будет размера у номенклатуры, то непопадет в выборку
                //if (normaContent.NomGroup.NomBodyPart != null && DataGlobals.SIZ_SIZE_ID != normaContent.NomGroup.NomBodyPart.Id)
                //{
                //    order.Add("Nomenclature.Sex.Id", ASC);
                //    order.Add("NomBodyPartSize.SizeNumber", ASC);
                //}

                StringBuilder nomGroupBase = new StringBuilder();
                StringBuilder nomGroups = new StringBuilder();
                foreach (var normaNomGroup in normaContent.NormaNomGroups)
                {
                    if (normaNomGroup.IsBase == true)
                    {
                        nomGroupBase.Append("'" + normaNomGroup.NomGroup.Id + "',");
                        break;
                    }
                }
                nomGroupBase.Remove(nomGroupBase.Length - 1, 1);

                foreach (var normaNomGroup in normaContent.NormaNomGroups)
                {
                    if (normaNomGroup.IsBase == true)
                        continue;
                    nomGroups.Append("'" + normaNomGroup.NomGroup.Id + "',");
                }

                query.Add("[in]Nomenclature.NomGroup.Id", nomGroupBase.ToString());
                storages.AddRange(storageRepository.GetByLikeCriteria(query, order));
                if (nomGroups.Length > 0)
                {
                    query.Remove("[in]Nomenclature.NomGroup.Id");
                    nomGroups.Remove(nomGroups.Length - 1, 1);
                    query.Add("[in]Nomenclature.NomGroup.Id", nomGroups.ToString());
                    storages.AddRange(storageRepository.GetByLikeCriteria(query, order));
                }

                if (storages.Count == 0)
                {
                    order.Remove("Nomenclature.Sex.Id");
                    order.Remove("NomBodyPartSize.SizeNumber");

                    query.Remove("[in]Nomenclature.NomGroup.Id");
                    query.Add("[in]Nomenclature.NomGroup.Id", nomGroupBase.ToString());

                    storages.AddRange(storageRepository.GetByLikeCriteria(query, order));
                    if (nomGroups.Length > 0)
                    {
                        query.Remove("[in]Nomenclature.NomGroup.Id");
                        query.Add("[in]Nomenclature.NomGroup.Id", nomGroups.ToString());
                        storages.AddRange(storageRepository.GetByLikeCriteria(query, order));
                    }
                }
            }
            else
                storages.AddRange(storageRepository.GetByLikeCriteria(query, order));
            
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
            queryParams.Add("Organization.Id", int.Parse(getCurrentEnterpriseId()));
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
        public ActionResult _GetWorkerWorkplaces(string text, bool isActive)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            Dictionary<string, object> orderParams = new Dictionary<string, object>();

            IList<WorkerWorkplace> workerWorkplaces = null;
            int tabn = -1;
            //int.TryParse(text, out tabn)
            if (int.TryParse(text, out tabn))
                queryParams.Add("Worker.TabN", tabn);
            else
                queryParams.Add("Worker.Fio", text);
            queryParams.Add("RootOrganization", int.Parse(getCurrentEnterpriseId()));

            orderParams.Add("IsActive", DESC);
            orderParams.Add("Worker.Fio", ASC);

            workerWorkplaces = workerWorkplaceRepository.GetByLikeCriteria(queryParams, orderParams);

           // if (isActive)
           // {
                // попытка найти неактивные рабочие места у работника, у которых есть карточки с выданными СИЗ
                // если такие есть, то показываем их в списке. это сигнализирует о том,
                // что необходимо списать, вернуть или перевести СИЗ с этого рабочего места прежде чем выдавать
                for (int i = 0; i < workerWorkplaces.Count; i++)
                {
                    queryParams.Clear();
                    queryParams.Add("WorkerWorkplace", workerWorkplaces[i]);
                    IList<WorkerCardHead> wchs = workerCardRepository.GetByCriteria(queryParams);
                    if ((workerWorkplaces[i].IsActive == false && wchs.Count == 0) || 
                       (workerWorkplaces[i].IsActive == false && wchs[0].getActiveWorkerCardContent().Count == 0) || 
                       (workerWorkplaces[i].IsActive == false && wchs.Count > 0 && wchs[0].WorkerCardContents.Count == 0))
                    {
                        workerWorkplaces.RemoveAt(i--);
                    }
                    // если нашлись неактивные с выданными СИЗ, то показываем только их
                    else if (workerWorkplaces[i].IsActive == false && isActive )
                    {
                        for (int j = 0; j < workerWorkplaces.Count; j++)
                        {
                            if (workerWorkplaces[i].Worker.Id == workerWorkplaces[j].Worker.Id && workerWorkplaces[j].IsActive)
                            {
                                workerWorkplaces.Remove(workerWorkplaces[j]);
                                i--;
                            }
                        }
                    }
                }

           // }

            return new JsonResult
            {
                Data = new SelectList(workerWorkplaces, "Id", "WorkplaceInfo")
            };
        }

        [HttpPost]
        public ActionResult _GetWorkerWorkplacesActive(int? workerWorkplaceId)
        {
            if (workerWorkplaceId == null)
                return new JsonResult { Data = new SelectList(new List<WorkerWorkplace>()) };
            Dictionary<string, object> queryParams = new Dictionary<string, object>();

            WorkerWorkplace wwp = workerWorkplaceRepository.Get(workerWorkplaceId.Value);

            queryParams.Add("Worker", wwp.Worker);
            queryParams.Add("RootOrganization", int.Parse(getCurrentEnterpriseId()));
            queryParams.Add("IsActive", true);

            IList<WorkerWorkplace> workerWorkplaces = workerWorkplaceRepository.GetByLikeCriteria(queryParams);

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

        private NomBodyPartSize getBiggerSize(int NomBodyPartId, string size)
        {
            if (size == null) return null;
            Dictionary<string, object> query = new Dictionary<string, object>();
            Dictionary<string, object> order = new Dictionary<string, object>();
            query.Add("NomBodyPart.Id", NomBodyPartId);            
            query.Add("IsActive", true);
            order.Add("SizeNumber", ASC);
            IList<NomBodyPartSize> rows = nomBodyPartSizeRepository.GetByLikeCriteria(query,order);
            int index=-1;
            foreach(var item in rows){
                index++;
                if (item.SizeString == size)
                {
                    index++;
                    break;
                }
            }

            if (index != -1 && rows.Count > index) return rows[index];
                else return null;            
        }

    }
}
