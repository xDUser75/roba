using System.Web.Mvc;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using Store.Core;
using Telerik.Web.Mvc;
using System.Collections.Generic;
using Store.Data;
using System;
using System.Threading.Tasks;

namespace Store.Web.Controllers
{
    [HandleError]
    public class RemainsController : ViewedController
    {
        private readonly RemainRepository remainsRepository;
        private readonly StorageNameRepository storageNameRepository;
        private readonly CriteriaRepository<Storage> storageRepository;
        private readonly CriteriaRepository<Operation> operationRepository;
        private readonly CriteriaRepository<OperationSimple> operationSimpleRepository;
        private readonly NomenclatureRepository nomenRepository;
        private readonly IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
        private static IDictionary<string, string> tasks = new Dictionary<string, string>();

        public RemainsController(RemainRepository remainsRepository,
                                  CriteriaRepository<Storage> storageRepository,
                                  StorageNameRepository storageNameRepository,
                                  CriteriaRepository<Operation> operationRepository,
                                  CriteriaRepository<OperationSimple> operationSimpleRepository,
                                  NomenclatureRepository nomenRepository)
        {
            Check.Require(remainsRepository != null, "RemainsRepository may not be null");
            this.remainsRepository = remainsRepository;
            this.storageRepository = storageRepository;
            this.storageNameRepository = storageNameRepository;
            this.nomenRepository = nomenRepository;
            this.operationRepository = operationRepository;
            this.operationSimpleRepository = operationSimpleRepository;
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_STORAGE_MONTH_EDIT + ", " + DataGlobals.ROLE_STORAGE_MONTH_VIEW))]
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
            return View(viewName);
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_STORAGE_MONTH_EDIT + ", " + DataGlobals.ROLE_STORAGE_MONTH_VIEW))]
        public ActionResult _SelectionClientSide_Remains(string idStorage, string date, int? Nomenclature)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            if (!(idStorage != null && idStorage != "-1"))
            {
                string idStorageC = (string)Session["idStorage"];
                if (idStorageC != null) idStorage = idStorageC;
            }

            if (idStorage != null)
            {
                if (idStorage != "-1") Session.Add("idStorage", idStorage); 
                if (idStorage.Length > 0)
                {
                    List<Remaind> model = new List<Remaind>();
                    int id = System.Int32.Parse(idStorage);
                    queryParams.Add("StorageName.Id", id);
                    DateTime dt = DateTime.ParseExact(date, DataGlobals.DATE_FORMAT_FULL_YEAR, null);
                    queryParams.Add("RemaindDate", dt);
                    if (Nomenclature != null)
                    {
                        queryParams.Add("Nomenclature.Id", Nomenclature);
                    }
                    Dictionary<string, object> orderParams = new Dictionary<string, object>();
                    orderParams.Add("Nomenclature.Name", ASC);
                    IEnumerable<Remaind> remainds = remainsRepository.GetByCriteria(queryParams, orderParams);
//                    IEnumerable<RemaindEx> remainds = remainsRepository.GetExternalRemains(id, dt, Nomenclature);
                    List<string> excludeProperty = new List<string>();
                    excludeProperty.Add("Storage");
                    excludeProperty.Add("StorageName");
                    excludeProperty.Add("NomBodyPartSize.NomBodyPart");
                    excludeProperty.Add("Growth.NomBodyPart");
                    excludeProperty.Add("Nomenclature.NomBodyPart");
                    excludeProperty.Add("Nomenclature.Organization");
                    excludeProperty.Add("Nomenclature.Unit");
                    //excludeProperty.Add("Nomenclature.Sex");
                    excludeProperty.Add("Nomenclature.NomBodyPart");
                    excludeProperty.Add("Nomenclature.NomGroup");
                    foreach (var item in remainds)
                    {
                        RemaindEx resItem = rebuildRemain(item, excludeProperty);
                        //resItem.QuantityE = item.QuantityE;
                        model.Add(resItem);
                    };
                    return View(new GridModel(model));
                }
            }
            return View(viewName);
        }

        [GridAction]
        [HttpPost]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_STORAGE_MONTH_EDIT))]
        public ActionResult _SaveRemains(string idStorage, string date, int? Nomenclature)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();

            if (!(idStorage != null && idStorage != "-1"))
            {
                string idStorageC = (string)Session["idStorage"];
                if (idStorageC != null) idStorage = idStorageC;
            }

            if (idStorage != null)
            {
                if (idStorage != "-1") Session.Add("idStorage", idStorage); 
                if (idStorage.Length > 0)
                {
                    List<Remaind> model = new List<Remaind>();
                    int id = System.Int32.Parse(idStorage);
                    DateTime dt = DateTime.ParseExact(date, DataGlobals.DATE_FORMAT_FULL_YEAR, null);
                    queryParams.Add("StorageName.Id", id);
                    if (Nomenclature != null)
                    {
                        queryParams.Add("Nomenclature.Id", Nomenclature);
                    }
//                    queryParams.Add("[>]Quantity", 0);
                    IList<Storage> storages = storageRepository.GetByLikeCriteria(queryParams);
                    // Добавлена проверка на наличие начальных остатков по складу. Если раньше остатков нет, то это начальные остатки - их не пересчитываем
                    queryParams.Clear();
                    queryParams.Add("StorageName.Id", id);
                    queryParams.Add("RemaindDate", dt.AddMonths(-1));
                    IList<Remaind> reamaindsBefor = remainsRepository.GetByLikeCriteria(queryParams);
                    queryParams.Clear();
                    queryParams.Add("StorageNameId", id);
                    queryParams.Add("[<=]OperDate", dt);
                    queryParams.Add("[>=]OperDate", dt.AddMonths(-1));
                    queryParams.Add("OperTypeId", DataGlobals.OPERATION_STORAGE_IN);
                    IList<OperationSimple> operationsBefor = operationSimpleRepository.GetByLikeCriteria(queryParams);

                    if (reamaindsBefor.Count != 0 || operationsBefor.Count != 0)
                    {
                        remainsRepository.TruncRamains(id, date, Nomenclature);

                        DateTime currentDate = DateTime.Now;
                        int rowCount = 0;
                        lock (tasks)
                        {
                            tasks.Add(Session.SessionID, "Обработано 0 из " + storages.Count + " записей...|" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")); // update task progress); 
                        }
                        foreach (var item in storages)
                        {
                            //пересчитываем остатки
                            rebuildRemaind(operationRepository, remainsRepository, item, dt.AddMonths(-1), dt.AddSeconds(-1), dt, false);

                            rowCount = rowCount + 1;
                            lock (tasks)
                            {
                                tasks[Session.SessionID] = "Обработано " + rowCount + " из " + storages.Count + " записей...|" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"); // update task progress
                            }
                        };
                        lock (tasks)
                        {
                            tasks.Remove(Session.SessionID);
                        }
                    }
                    return _SelectionClientSide_Remains(idStorage, date,null);
                }
            }
            return View(viewName);
        }

        public ActionResult _Progress()
        {
            string result = "100%";
//Убираем мусор
            lock (tasks)
            {
                foreach (var item in tasks)
                {
                    string val = item.Value;
                    if (val.IndexOf('|') > 0)
                    {
                        val = val.Substring(val.IndexOf('|') + 1);
                        if (DateTime.ParseExact(val, "dd.MM.yyyy HH:mm:ss", null).AddHours(3) < DateTime.Now)
                        {
                            tasks.Remove(item.Key);
                        }
                    }
                    else
                    {
                        tasks.Remove(item.Key);
                    }
                }
                if (tasks.Keys.Contains(Session.SessionID))
                {
                    result = tasks[Session.SessionID];
                    result = result.Substring(0, result.IndexOf('|') - 1);
                }
            }
            return Json(result);
        }

        [HttpPost]
        public ActionResult _GetNomenclatures(string text)
        {
/*            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("Organization.Id", int.Parse(getCurrentEnterpriseId()));
            int code = -1;
            if (int.TryParse(text, out code))
                queryParams.Add("ExternalCode", text);
            else
                queryParams.Add("Name", text);
            IList<Nomenclature> nomens = nomenRepository.GetByLikeCriteria(queryParams);
*/
            IList<Nomenclature> nomens = nomenRepository.GetNomenclature(text, getCurrentEnterpriseId());
            Dictionary<string, string> list = new Dictionary<string, string>();
            foreach (var item in nomens)
            {
                list.Add(item.Id.ToString(), item.ExternalCode + " - " + item.Name);
            };

            return new JsonResult
            {
                Data = new SelectList(list, "Key", "Value")
            };
        }

    }
}
