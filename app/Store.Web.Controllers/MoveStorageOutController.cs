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
    public class MoveStorageOutController : ViewedController
    {
        private readonly CriteriaRepository<Storage> storageRepository;
        private readonly CriteriaRepository<OperType> operTypeRepository;
        private readonly CriteriaRepository<Organization> organizationRepository;
        private readonly OperationRepository operationRepository;
        private readonly StorageNameRepository storageNameRepository;

        public MoveStorageOutController(CriteriaRepository<Storage> storageRepository,
            StorageNameRepository storageNameRepository,
            CriteriaRepository<OperType> operTypeRepository,
            CriteriaRepository<Organization> organizationRepository,
            OperationRepository operationRepository)
        {
            Check.Require(storageRepository != null, "storageRepository may not be null");
            this.storageRepository = storageRepository;
            this.operTypeRepository = operTypeRepository;
            this.organizationRepository = organizationRepository;
            this.operationRepository = operationRepository;
            this.storageNameRepository = storageNameRepository;
        }

        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_WORKER_CARD_EDIT + ", " + DataGlobals.ROLE_WORKER_CARD_VIEW))]
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


        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_STORAGE_MOVE_OUT_EDIT + ", " + DataGlobals.ROLE_STORAGE_MOVE_OUT_VIEW))]
        public ActionResult Select(string storageNameId, string externalCode)
        {
            Session.Add("storageNameId", storageNameId);
            Dictionary<string, object> query = new Dictionary<string, object>();
            Dictionary<string, object> order = new Dictionary<string, object>();
            query.Add("Wear", "50");
            query.Add("[>]Quantity", 0);
            query.Add("StorageName.Id", int.Parse(storageNameId));
            if (externalCode != null)
            {
                if (externalCode.Length > 0)
                {
                    query.Add("Nomenclature.ExternalCode", externalCode);
                }
            }
            IList<Storage> storages = storageRepository.GetByLikeCriteria(query);
            List<MoveStorageOutSimple> model = new List<MoveStorageOutSimple>();
            foreach (var item in storages){
                MoveStorageOutSimple modelItem = new MoveStorageOutSimple(item.Id);
                if (item.Nomenclature.NomGroup != null)
                {
                    modelItem.GroupName = item.Nomenclature.NomGroup.Name;
                }
                modelItem.Nomenclature = item.NomenclatureInfo;
                modelItem.Quantity = item.Quantity;
                if (item.Nomenclature.Unit != null)
                {
                    modelItem.Unit = item.Nomenclature.Unit.Name;
                }
                model.Add(modelItem);
            }
            return View(new GridModel(model));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_STORAGE_MOVE_OUT_EDIT))]
        public ActionResult Update([Bind(Prefix = "updated")]IEnumerable<MoveStorageOutSimple> updateList, [Bind(Prefix = "updated[0].storageNameId")] string storageNameId, [Bind(Prefix = "updated[0].docNumber")] string docNumber, [Bind(Prefix = "updated[0].docDate")] string docDate, [Bind(Prefix = "updated[0].externalCode")] string externalCode)
        {
            if (updateList != null)
            {
                int i = -1;
                foreach (var itemList in updateList)
                {
                    i++;
                    if (itemList.OutQuantity <= 0)
                        continue;
                    if (itemList.OutQuantity > itemList.Quantity)
                        ModelState.AddModelError("updated[" + i + "]", "Вы пытаетесь списать больше, чем есть на складе");
                }

                if (ModelState.IsValid)
                {
                    IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
                    string format = "dd.MM.yyyy";

                    foreach (var itemList in updateList)
                    {
                        if (itemList.OutQuantity <= 0)
                            continue;                       
                        Storage storage = storageRepository.Get(itemList.Id);
                        storage.Quantity -= itemList.OutQuantity;

                        Operation oper = new Operation();
                        oper.DocDate = DateTime.ParseExact(docDate, format, culture);
                        //oper.OperDate = DateTime.Now;
                        oper.OperDate = oper.DocDate.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddSeconds(DateTime.Now.Second);
                        oper.OperType = operTypeRepository.Get(DataGlobals.OPERATION_STORAGE_WEAR_OUT);
                        oper.Organization = organizationRepository.Get(storage.StorageName.Organization.RootOrganization);
                        oper.Quantity = itemList.OutQuantity;
                        oper.Storage = storage;
                        oper.DocNumber = docNumber;
                        operationRepository.SaveOrUpdate(oper);
                        storageRepository.SaveOrUpdate(storage);
                     }
                }
            }
            return Select(storageNameId, externalCode);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + "," + DataGlobals.ROLE_STORAGE_MOVE_OUT_EDIT))]
        public ActionResult CancelOper(string storageNameId, string docNumber, string docDate)
        {
           IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
           string format = "dd.MM.yyyy";
           DateTime docDt=DateTime.ParseExact(docDate, format, culture);

            Dictionary<string, object> query = new Dictionary<string, object>();
            query.Add("DocNumber", docNumber);
            query.Add("DocDate", docDt);
            query.Add("OperType.Id", DataGlobals.OPERATION_STORAGE_WEAR_OUT);
            IList<Operation> operations = operationRepository.GetByLikeCriteria(query);
            foreach (var item in operations)
            {
                Storage storage = item.Storage;
                storage.Quantity += item.Quantity;
                storageRepository.SaveOrUpdate(storage);
                operationRepository.Delete(item);
            }
            return null;
        }
    }
}
