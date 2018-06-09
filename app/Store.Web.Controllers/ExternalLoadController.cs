using System.Web.Mvc;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using Store.Core;
using Store.Data;
using System.Collections.Generic;
using System.Web;
using System.IO;
using Store.Core.Utils;
using System.Data;
using System;
using log4net;
using Telerik.Web.Mvc;
using NHibernate;

namespace Store.Web.Controllers
{
    [HandleError]
    public class ExternalLoadController : ViewedController
    {
        private readonly StorageNameRepository storageNameRepository;
        private readonly CriteriaRepository<Nomenclature> nomenclatureRepository;
        private readonly CriteriaRepository<Worker> workerRepository;
        private readonly CriteriaRepository<WorkerWorkplace> workerWorkplaceRepository;
        private readonly StorageRepository storageRepository;
        private readonly CriteriaRepository<Operation> operationRepository;
        private readonly CriteriaRepository<OperType> operTypeRepository;
        private readonly CriteriaRepository<WorkerCardHead> workerCardHeadRepository;
        private readonly CriteriaRepository<WorkerCardContent> workerCardContentRepository;
        private readonly CriteriaRepository<Organization> organizationRepository;
        private readonly CriteriaRepository<Motiv> motivRepository;
        private readonly RemainRepository remaindRepository;
        
        public ExternalLoadController(
                                    StorageNameRepository storageNameRepository,
                                    CriteriaRepository<Nomenclature> nomenclatureRepository,
                                    CriteriaRepository<Worker> workerRepository,
                                    CriteriaRepository<WorkerWorkplace> workerWorkplaceRepository,
                                    StorageRepository storageRepository,
                                    CriteriaRepository<Operation> operationRepository,
                                    CriteriaRepository<OperType> operTypeRepository,
                                    CriteriaRepository<WorkerCardHead> workerCardHeadRepository,
                                    CriteriaRepository<WorkerCardContent> workerCardContentRepository,
                                    CriteriaRepository<Organization> organizationRepository,
                                    CriteriaRepository<Motiv> motivRepository,
                                    RemainRepository remaindRepository
                                    )
        {
            Check.Require(workerRepository != null, "workerRepository may not be null");
            this.storageNameRepository = storageNameRepository;
            this.nomenclatureRepository = nomenclatureRepository;
            this.workerRepository = workerRepository;
            this.workerWorkplaceRepository = workerWorkplaceRepository;
            this.storageRepository = storageRepository;

            this.operationRepository = operationRepository;
            this.operTypeRepository = operTypeRepository;
            this.workerCardHeadRepository = workerCardHeadRepository;
            this.workerCardContentRepository = workerCardContentRepository;
            this.organizationRepository = organizationRepository;
            this.motivRepository = motivRepository;
            this.remaindRepository = remaindRepository;
        }


        [Transaction]
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_REMAINS_EXTERNAL_UPLOAD + ", " + DataGlobals.ROLE_REMAINS_EXTERNAL_VIEW))]
        public ActionResult Index()
        {
            Dictionary<string, object> query = new Dictionary<string, object>();
            query.Add("Organization.Id", int.Parse(getCurrentEnterpriseId()));
            query.Add("[in]Id", ((Store.Core.Account.User)Session[DataGlobals.ACCOUNT_KEY]).ObjectsCSV);
            IList<StorageName> storageNames = storageNameRepository.GetByCriteria(query);
            SelectList storageNameList = new SelectList(storageNames, "Id", "Name", Session["storageNameId"] != null ? (string)Session["storageNameId"] : "-1");
            ViewData[DataGlobals.REFERENCE_STORAGE_SHOP_NAME] = storageNameList;
            if (storageNames.Count > 0)
            {
                Session["idStorage"] = storageNames[0].Id.ToString();
            }
            int messRowCount = 0;
            IList<OperType> operTypes = operTypeRepository.GetAll();
            IList<OperType> operTypeNew = new List<OperType>();
            foreach (var op in operTypes)
            {
                if (op.Id == DataGlobals.OPERATION_STORAGE_TRANSFER_IN || op.Id == DataGlobals.OPERATION_WORKER_IN)
                    operTypeNew.Add(op);
             }
            SelectList operTypeList = new SelectList(operTypeNew, "Id", "Name");

            ViewData[DataGlobals.REFERENCE_OPER_TYPE] = operTypeList;
           int.TryParse((string)Session["row_count"], out messRowCount);
            Session.Remove("row_count");
            if (messRowCount > 0)
            {
                for (int i = 0; i < messRowCount; i++)
                {
                    ViewData["messRow" + i] = Session["messRow" + i];
                    Session.Remove("messRow" + i);
                }
            }
            ViewData["row_count"] = messRowCount.ToString();
            return View(viewName);
        }


        [Transaction]
        [AcceptVerbs(HttpVerbs.Post)]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_REMAINS_EXTERNAL_UPLOAD))]
        public void SaveOnHand(HttpResponseBase response, string loadDate, int operTypeId, IEnumerable<HttpPostedFileBase> attachments)
        {
            // The Name of the Upload component is "attachments" 
            IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
            DateTime operationDate;
            DateTime.TryParseExact(loadDate, DataGlobals.DATE_FORMAT_FULL_YEAR, null, System.Globalization.DateTimeStyles.None, out operationDate);
            if (operationDate == Null_Date)
            {
                response.Write("Ошибка в дате операции!<br/>");
                response.Flush();
                return;
            }
            foreach (var file in attachments)
            {
                // Some browsers send file names with full path. This needs to be stripped.
                var isError = false;
                var fileName = Path.GetFileName(file.FileName);
                response.Write("Обрабатывается файл <b>" + fileName + "</b><br/>");
                response.Flush();
                var physicalFilePath = Path.Combine(Server.MapPath("~/TempFiles"), fileName);
                try
                {
                    if (System.IO.File.Exists(physicalFilePath))
                    {
                        System.IO.File.Delete(physicalFilePath);
                    }
                    try
                    {
                        file.SaveAs(physicalFilePath);
                    }
                    catch (Exception e)
                    {
                        response.Write("Ошибка при охранении файла на сервере:<br/>");
                        response.Write(e.Message);
                        response.Flush();
                        isError = true;
                    }
                    System.Data.DataTable table = null;
                    string workSheetNames = "";
                    if (!isError)
                    {
                        try
                        {
                            ExcelReader excelReader = new ExcelReader(physicalFilePath, true);
                            if (excelReader.workSheetNames.Length > 0)
                            {
                                workSheetNames = excelReader.workSheetNames[0];
                                table = excelReader.GetWorksheet(workSheetNames);
                            }
                            else
                            {
                                response.Write("В файле не найден рабочий лист!<br/>");
                                response.Flush();
                                isError = true;
                            }
                        }
                        catch (Exception e)
                        {
                            response.Write("Ошибка при открытии файла:<br/>");
                            response.Write(e.Message);
                            response.Flush();
                            isError = true;
                        }
                    }
                    if (table != null)
                    {
                        response.Write("Загрузка данных производится из листа с именем '" + workSheetNames.Trim(new[] { '$' }) + "'<br/>");
                        response.Flush();
                        if (!table.Columns.Contains("Табельный номер"))
                        {
                            response.Write("Файл содержит не коррекные данные ('Табельный номер').<br/>");
                            response.Flush();
                            isError = true;
                        }
                        if (!table.Columns.Contains("Номенклатурный номер"))
                        {
                            response.Write("Файл содержит не коррекные данные ('Номенклатурный номер').<br/>");
                            response.Flush();
                            isError = true;
                        }
                        if (!table.Columns.Contains("Дата выдачи"))
                        {
                            response.Write("Файл содержит не коррекные данные ('Дата выдачи').<br/>");
                            response.Flush();
                            isError = true;
                        }
                        if (!table.Columns.Contains("Кол-во"))
                        {
                            response.Write("Файл содержит не коррекные данные ('Кол-во').<br/>");
                            response.Flush();
                            isError = true;
                        }
                        /*
                        if (!table.Columns.Contains("Период"))
                        {
                            response.Write("Файл содержит не коррекные данные ('Период').<br/>");
                            response.Flush();
                            isError = true;
                        }
                        */
                        int colTabN = table.Columns.IndexOf("Табельный номер");
                        int colNomenclature = table.Columns.IndexOf("Номенклатурный номер");
                        int colDate = table.Columns.IndexOf("Дата выдачи");
                        int colCount = table.Columns.IndexOf("Кол-во");
                        int colUsePeriod = -1;
                        if (!table.Columns.Contains("Период"))
                        {
                            colUsePeriod = table.Columns.IndexOf("Период");
                        }

                        if (!isError)
                        {
                            //                                    DataRow[] result = table.Select("F2 = '*' and F3=" + storageName.Plant + " and F4 in (" + storageName.StorageNumber + ")");
                            DataRow[] result = table.Select();
                            Dictionary<string, object> queryParams = new Dictionary<string, object>();
                            Dictionary<string, object> orderParams = new Dictionary<string, object>();
                            int idOrg = int.Parse(getCurrentEnterpriseId());
                            Organization currentOrg = organizationRepository.Get(idOrg);
                            Motiv motiv = motivRepository.Get(DataGlobals.MOTIVID_TRANSFER); 
                            foreach (DataRow row in result) // Loop over the rows.
                            {
                                if (row[colTabN] != System.DBNull.Value)
                                {
                                    int tabN = int.Parse(row[colTabN].ToString());
                                    int quantity = 0;
                                    if (!int.TryParse(row[colCount].ToString(), out quantity))
                                    {
                                        response.Write("У человека с Таб.№ " + tabN + " ошибка в кол-ве!<br/>");
                                        response.Flush();
                                        continue;
                                    }

                                    DateTime startDate;
                                    String rowDate = row[colDate].ToString();
                                    if (rowDate.IndexOf(' ')>0) rowDate = rowDate.Substring(0, rowDate.IndexOf(' '));
                                    DateTime.TryParseExact(rowDate, DataGlobals.DATE_FORMAT_FULL_YEAR, null, System.Globalization.DateTimeStyles.None, out startDate);

                                    if (startDate == Null_Date)
                                    {
                                        response.Write("У человека с Таб.№ " + tabN + " ошибочная дата выдачи!<br/>");
                                        response.Flush();
                                        continue;
                                    }

                                    if (startDate > DateTime.Now)
                                    {
                                        response.Write("У человека с Таб.№ " + tabN + " дата выдачи больше текущей даты!<br/>");
                                        response.Flush();
                                        continue;
                                    }
                                    int useperiod =0;
                                    if (colUsePeriod>=0)
                                        useperiod = int.Parse(row[colUsePeriod].ToString());

                                    // Ищем id работника  по табельному номеру, если не находим, то не берем его
                                    queryParams.Clear();
                                    queryParams.Add("RootOrganization", currentOrg.Id);
                                    //queryParams.Add("IsActive", true);
                                    queryParams.Add("TabN", tabN);
                                    Worker correntWorker = workerRepository.FindOne(queryParams);
                                    if (correntWorker == null)
                                    {
                                        response.Write("Человека с Таб.№ " + tabN + " нет в базе!<br/>");
                                        response.Flush();
                                        continue;
                                    }
                                    // Ищем ID рабочего места  
                                    queryParams.Clear();
                                    queryParams.Add("RootOrganization", currentOrg.Id);
                                    queryParams.Add("Worker", correntWorker);
                                    // Будем ипользовать последнее место работы
                                    orderParams.Clear();
                                    orderParams.Add("BeginDate", DESC);
                                    orderParams.Add("IsActive", DESC);

                                    IList<WorkerWorkplace> workerWorkplaces = workerWorkplaceRepository.GetByLikeCriteria(queryParams, orderParams);
                                    // Ищем id номенклатуры на руках( сверила все номенклатуры, в справочнике есть все)   
                                    queryParams.Clear();
                                    queryParams.Add("Organization", currentOrg);
                                    queryParams.Add("ExternalCode", row[colNomenclature].ToString());
                                    queryParams.Add("IsActive", true);
                                    IList<Nomenclature> nomenclatures = nomenclatureRepository.GetByLikeCriteria(queryParams);
                                    if (nomenclatures.Count == 0)
                                    {
                                        queryParams.Clear();
                                        queryParams.Add("Organization", currentOrg);
                                        queryParams.Add("ExternalCode", row[colNomenclature].ToString());
                                        queryParams.Add("IsActive", false);
                                        nomenclatures = nomenclatureRepository.GetByLikeCriteria(queryParams);
                                        if (nomenclatures.Count == 0)
                                        {
                                            response.Write("Номенклатура с № " + row[colNomenclature].ToString() + " не найдена в системе!<br/>");
                                            response.Flush();
                                            continue;
                                        }
                                    }

                                    // Ищем склад
                                    queryParams.Clear();
                                    queryParams.Add("AreaCode", workerWorkplaces[0].Organization.AreaCode);
                                    queryParams.Add("Organization", currentOrg);

                                    //StorageName storageName = storageNameRepository.FindOne(queryParams);
                                    StorageName storageName = workerWorkplaces[0].Organization.StorageName;
                                    if (storageName == null)
                                    {
                                        response.Write("Для человека с Таб.№ " + tabN + "не найден склад!<br/>");
                                        response.Flush();
                                        continue;
                                    }

                                    operationRepository.DbContext.BeginTransaction();
                                    try
                                    {
                                        // Ищем есть ли подобная позиция на складе (номенклатура+рост+размер), считаем, что % годностиу всех 100                                            
                                        // Если не находим, то добавляем запись на склад с нулевым кол-вом
                                        queryParams.Clear();
                                        queryParams.Add("StorageName", storageName);
                                        queryParams.Add("Nomenclature", nomenclatures[0]);
                                        queryParams.Add("Wear", "100");
                                        Storage storage = storageRepository.FindOne(queryParams);
                                        if (storage == null)
                                        {
                                            storage = new Storage();
                                            storage.StorageName = storageName;
                                            storage.Nomenclature = nomenclatures[0];
                                            storage.Quantity = 0;
                                            storage.Wear = "100";
                                            storageRepository.SaveOrUpdate(storage);
                                            storage.NomBodyPartSize = nomenclatures[0].NomBodyPartSize;
                                            storage.Growth = nomenclatures[0].Growth;
                                        }
                                        OperType operType = operTypeRepository.Get(operTypeId);
                                        
                                        //OperType operType = operTypeRepository.Get(DataGlobals.OPERATION_WORKER_IN);
                                        //OperType operType = operTypeRepository.Get(DataGlobals.OPERATION_STORAGE_TRANSFER_IN);
                                        // Добавляем операцию постановки на забаланс , вместо последней выдачи
                                        queryParams.Clear();
                                        queryParams.Add("OperType", operType);
                                        queryParams.Add("DocNumber", "0");
                                        queryParams.Add("Storage", storage);
                                        queryParams.Add("WorkerWorkplace", workerWorkplaces[0]);
                                        Operation operation = operationRepository.FindOne(queryParams);
                                        if (operation == null)
                                        {
                                            operation = new Operation();
                                            operation.DocNumber = "0";
                                            if (operType.Id == DataGlobals.OPERATION_STORAGE_TRANSFER_IN)
                                            {
                                                operation.DocDate = operationDate;
                                                operation.OperDate = operationDate;
                                                operation.Motiv = motiv;
                                            }
                                            else
                                            {
                                                operation.DocDate = startDate;
                                                operation.OperDate = startDate;
                                            }
                                            operation.OperType = operType;
                                            operation.Organization = storageName.Organization;
                                            operation.Quantity = quantity;
                                            operation.Wear = "100";
                                            operation.StorageName = storageName;
                                            operation.Storage = storage;
                                            operation.WorkerWorkplace = workerWorkplaces[0];
                                            if (operation.OperType.Id == DataGlobals.OPERATION_WORKER_IN )
                                                operation.GiveOperation = operation;
                                            operationRepository.SaveOrUpdate(operation);
                                        }
                                        // Формируем личную карточку
                                        // Если нет шапки формируеруем шапку

                                        queryParams.Clear();
                                        queryParams.Add("WorkerWorkplace", workerWorkplaces[0]);
                                        WorkerCardHead workerCardHead = workerCardHeadRepository.FindOne(queryParams);
                                        if (workerCardHead == null)
                                        {
                                            workerCardHead = new WorkerCardHead();
                                            workerCardHead.WorkerWorkplace = workerWorkplaces[0];
                                            workerCardHeadRepository.SaveOrUpdate(workerCardHead);
                                        }
                                        // Формируем содержание личной карточки
                                        queryParams.Clear();
                                        queryParams.Add("Operation", operation);
                                        queryParams.Add("WorkerCardHead", workerCardHead);
                                        WorkerCardContent workerCardContent = workerCardContentRepository.FindOne(queryParams);
                                        if (workerCardContent == null)
                                        {
                                            workerCardContent = new WorkerCardContent();
                                            workerCardContent.Operation = operation;
                                            workerCardContent.Quantity = quantity;
                                            workerCardContent.Storage = storage;
                                            workerCardContent.WorkerCardHead = workerCardHead;
                                            workerCardContent.StartDate = startDate;
                                            workerCardContent.UsePeriod = useperiod;
                                            workerCardContentRepository.SaveOrUpdate(workerCardContent);
                                        }
                                        operationRepository.DbContext.CommitTransaction();
                                    }
                                    catch (Exception e)
                                    {
                                        operationRepository.DbContext.RollbackTransaction();
                                        response.Write("Ошибка при сохранении данных в БД:<br/>");
                                        response.Write(e.Message);
                                        response.Write("<br/>");
                                        if (e.InnerException != null)
                                        {
                                            response.Write(e.InnerException.Message);
                                            response.Write("<br/>");
                                        }
                                        isError = true;
                                        response.Flush();
                                    }
                                }
                            }
                            response.Flush();
                        }
                        response.Flush();

                    }
                    if (!isError)
                    {
                        response.Write("Файл успешно обработан!");
                    }
                }
                finally
                {
                    System.IO.File.Delete(physicalFilePath);
                }
            }
        }


        [Transaction]
        [AcceptVerbs(HttpVerbs.Post)]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_REMAINS_EXTERNAL_UPLOAD))]
        public void SaveOnStorage(HttpResponseBase response,String date, int storageNameId, IEnumerable<HttpPostedFileBase> attachments)
        {
            // The Name of the Upload component is "attachments" 
            IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
            StorageName storageName = storageNameRepository.Get(storageNameId);
            if (storageName == null)
            {
                response.Write("Выбранный склад не найден в БД!<br/>");
                response.Flush();
                return;
            }
            DateTime remaindDate;
            DateTime.TryParseExact(date, DataGlobals.DATE_FORMAT_FULL_YEAR, null, System.Globalization.DateTimeStyles.None, out remaindDate);
            if (remaindDate == null)
            {
                response.Write("Ошибка в дате остатков!<br/>");
                response.Flush();
                return;
            }

            foreach (var file in attachments)
            {
                // Some browsers send file names with full path. This needs to be stripped.
                var isError = false;
                var fileName = Path.GetFileName(file.FileName);
                response.Write("Обрабатывается файл <b>" + fileName + "</b><br/>");
                response.Flush();
                var physicalFilePath = Path.Combine(Server.MapPath("~/TempFiles"), fileName);
                try
                {
                    if (System.IO.File.Exists(physicalFilePath))
                    {
                        System.IO.File.Delete(physicalFilePath);
                    }
                    try
                    {
                        file.SaveAs(physicalFilePath);
                    }
                    catch (Exception e)
                    {
                        response.Write("Ошибка при охранении файла на сервере:<br/>");
                        response.Write(e.Message);
                        response.Flush();
                        isError = true;
                    }
                    System.Data.DataTable table = null;
                    string workSheetNames = "";
                    if (!isError)
                    {
                        try
                        {
                            ExcelReader excelReader = new ExcelReader(physicalFilePath, true);
                            if (excelReader.workSheetNames.Length > 0)
                            {
                                workSheetNames = excelReader.workSheetNames[0];
                                table = excelReader.GetWorksheet(workSheetNames);
                            }
                            else {
                                response.Write("В файле не найден рабочий лист!<br/>");
                                response.Flush();
                                isError = true;
                            }
                        }
                        catch (Exception e)
                        {
                            response.Write("Ошибка при открытии файла:<br/>");
                            response.Write(e.Message);
                            response.Flush();
                            isError = true;
                        }
                    }
                    if (table != null)
                    {
                        response.Write("Загрузка данных производится из листа с именем '" + workSheetNames.Trim(new[] { '$' }) + "'<br/>");
                        response.Flush();
                            //if (!table.Columns.Contains("Код склада"))
                            //{
                            //    response.Write("Файл содержит не коррекные данные ('Код склада').<br/>");
                            //    isError = true;
                            //}
                            if (!table.Columns.Contains("Номенклатурный номер"))
                            {
                                response.Write("Файл содержит не коррекные данные ('Номенклатурный номер').<br/>");
                                response.Flush();
                                isError = true;
                            }
                            if (!table.Columns.Contains("Износ"))
                            {
                                response.Write("Файл содержит не коррекные данные ('Износ').<br/>");
                                response.Flush();
                                isError = true;
                            }
                            if (!table.Columns.Contains("Кол-во"))
                            {
                                response.Write("Файл содержит не коррекные данные ('Кол-во').<br/>");
                                response.Flush();
                                isError = true;
                            }
//                            int colStorage = table.Columns.IndexOf("Код склада");
                            int colNomenclature = table.Columns.IndexOf("Номенклатурный номер");
                            int colWear = table.Columns.IndexOf("Износ");
                            int colCount = table.Columns.IndexOf("Кол-во");

                            if (!isError)
                            {
                                //                                    DataRow[] result = table.Select("F2 = '*' and F3=" + storageName.Plant + " and F4 in (" + storageName.StorageNumber + ")");
                               
                                //remaindRepository.TruncRamains(storageNameId, date,null);
                                
                                // Удаление ранее расчитаных остатков
                                //storageRepository.TruncStorage(storageNameId);
                                
                                DataRow[] result = table.Select();
                                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                                int idOrg = int.Parse(getCurrentEnterpriseId());
                                Organization currentOrg = organizationRepository.Get(idOrg);
                                foreach (DataRow row in result) // Loop over the rows.
                                {
                                    if (row[colNomenclature] != System.DBNull.Value)
                                    {
                                        // Ищем id номенклатуры на руках( сверила все номенклатуры, в справочнике есть все)   
                                        queryParams.Clear();
                                        queryParams.Add("Organization", currentOrg);
                                        queryParams.Add("ExternalCode", row[colNomenclature].ToString());
                                        queryParams.Add("IsActive", true);
                                        IList<Nomenclature> nomenclatures = nomenclatureRepository.GetByLikeCriteria(queryParams);
                                        if (nomenclatures.Count == 0)
                                        {
                                            queryParams.Clear();
                                            queryParams.Add("Organization", currentOrg);
                                            queryParams.Add("ExternalCode", row[colNomenclature].ToString());
                                            queryParams.Add("IsActive", false);
                                            nomenclatures = nomenclatureRepository.GetByLikeCriteria(queryParams);
                                            if (nomenclatures.Count == 0)
                                            {
                                                response.Write("Номенклатура с № " + row[colNomenclature].ToString() + " не найдена в системе!<br/>");
                                                response.Flush();
                                                continue;
                                            }
                                        }

                                        int quantity = 0;
                                        if (!int.TryParse(row[colCount].ToString(), out quantity))
                                        {
                                            response.Write("У номенклатуры " + row[colNomenclature] + " ошибка в кол-ве!<br/>");
                                            response.Flush();
                                            continue;
                                        }


                                        string wear = row[colWear].ToString();
                                        if ((wear != "100") && (wear != "50"))
                                        {
                                            response.Write("У номенклатуры " + row[colNomenclature] + " ошибка в износе! Допустимые изачения 100 или 50. <br/>");
                                            response.Flush();
                                            continue;
                                        }

                                        // Ищем есть ли подобная позиция на складе (номенклатура+рост+размер), считаем, что % годностиу всех 100                                            
                                        // Если не находим, то добавляем запись на склад с нулевым кол-вом
                                        queryParams.Clear();
                                        queryParams.Add("StorageName", storageName);
                                        queryParams.Add("Nomenclature", nomenclatures[0]);
                                        queryParams.Add("Wear", wear);
                                        //Storage storage = storageRepository.FindOne(queryParams);
                                        IList<Storage> storages = storageRepository.GetByLikeCriteria(queryParams);
                                        Storage storage=null;
                                        if (storages.Count>0)
                                            storage = storages[0];

                                        if (storage == null)
                                        {
                                            storage = new Storage();
                                            storage.StorageName = storageName;
                                            storage.Nomenclature = nomenclatures[0];
                                            storage.Quantity = quantity;
                                            storage.Wear = wear;
                                            storageRepository.SaveOrUpdate(storage);
                                        }
                                        else
                                        {
                                            storage.Quantity = storage.Quantity + quantity;

                                        }
                                        storage.NomBodyPartSize = nomenclatures[0].NomBodyPartSize;
                                        storage.Growth = nomenclatures[0].Growth;

                                        Remaind remaind = new Remaind();
                                        remaind.Growth = nomenclatures[0].Growth;
                                        remaind.Nomenclature = nomenclatures[0];
                                        remaind.Quantity = quantity;
                                        remaind.Wear = int.Parse(wear);
                                        remaind.StorageName = storageName;
                                        remaind.RemaindDate = remaindDate;
                                        remaind.NomBodyPartSize = nomenclatures[0].NomBodyPartSize;
                                        remaind.ActualDate = DateTime.Now;

                                        try
                                        {
                                            storageRepository.DbContext.BeginTransaction();
                                            storageRepository.SaveOrUpdate(storage);
                                            remaind.Storage = storage;
                                            remaindRepository.SaveOrUpdate(remaind);
                                            storageRepository.DbContext.CommitTransaction();
                                        }
                                        catch (Exception e)
                                        {
                                            isError = true;
                                            response.Write("Для номенклатуры " + row[colNomenclature] + " произошла ошибка при сохранении данных в БД:<br/>");
                                            response.Write(e.Message);
                                            response.Write("<br/>");
                                            if (e.InnerException != null)
                                            {
                                                response.Write(e.InnerException.Message);
                                                response.Write("<br/>");
                                            }
                                            response.Flush();
                                            storageRepository.DbContext.RollbackTransaction();
                                        }

                                    }
                                }
                                response.Flush();
                            }
                            response.Flush();
                    }
                    if (!isError)
                    {
                        response.Write("Файл успешно обработан!");
                    }
                }
                finally
                {
                    System.IO.File.Delete(physicalFilePath);
                }
            }
        }


        [Transaction]
        [AcceptVerbs(HttpVerbs.Post)]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_REMAINS_EXTERNAL_UPLOAD))]
        public ActionResult Save(int selectType, string date, int StorageList, int OperTypeList, IEnumerable<HttpPostedFileBase> attachments)
        {
            HttpResponseBase Response = ControllerContext.HttpContext.Response;
            Response.ContentType = "text/html";
            Response.Write("<html><body>");
            if (attachments == null)
            {
                Response.Write("Файл для загрузки не выбран!");
            }
            else
            {
                if (selectType == 1) SaveOnHand(Response, date, OperTypeList, attachments);
                if (selectType == 2) SaveOnStorage(Response,date, StorageList, attachments);
            }
            Response.Write("</html></body>");
            Response.Flush();
            return null; ;
        }

    }
}
