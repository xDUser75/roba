using System.Web.Mvc;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using SharpArch.Core.PersistenceSupport;
using Store.Core;
using Telerik.Web.Mvc.Extensions;
using Store.Data;
using Telerik.Web.Mvc;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

namespace Store.Web.Controllers
{
    [HandleError]
    public class OperationsController : ViewedController
    {
        private readonly OperationRepository operationRepository;
        private readonly CriteriaRepository<OperationSimple> operationSimpleRepository;
        private readonly StorageNameRepository storageNameRepository;
        private readonly NomenclatureRepository nomenRepository;
        private readonly CriteriaRepository<OperType> operationTypeRepository;

        public OperationsController(OperationRepository operationRepository,
                                    StorageNameRepository storageNameRepository,
                                    NomenclatureRepository nomenRepository,
                                    CriteriaRepository<OperationSimple> operationSimpleRepository,
                                    CriteriaRepository<OperType> operationTypeRepository)
        {
            Check.Require(operationRepository != null, "operationRepository may not be null");
            this.operationRepository = operationRepository;
            this.operationTypeRepository = operationTypeRepository;
            this.operationSimpleRepository = operationSimpleRepository;
            this.nomenRepository = nomenRepository;
            this.storageNameRepository = storageNameRepository;
        }

        [GridAction]
        [Transaction]
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_OPERATION_EDIT + ", " + DataGlobals.ROLE_OPERATION_VIEW))]
        public ActionResult Index(string date, string dateEnd, string idStorage, int? OperTypes, int? Shops, int? Nomenclature)
        {
            IList<OperType> operationTypes = operationTypeRepository.GetAll();
            OperType oType=new OperType(0,"Любой");
            operationTypes.Insert(0, oType);
            SelectList operationTypesList = new SelectList(operationTypes, "Id", "Name");
            ViewData[Store.Data.DataGlobals.REFERENCE_OPER_TYPE] = operationTypesList;
            
            Dictionary<string, object> query = new Dictionary<string, object>();
            query.Add("Organization.Id", int.Parse(getCurrentEnterpriseId()));
            query.Add("[in]Id", ((Store.Core.Account.User)Session[DataGlobals.ACCOUNT_KEY]).ObjectsCSV);
            IList<StorageName> storageNames = storageNameRepository.GetByCriteria(query);
            SelectList storageNameList = new SelectList(storageNames, "Id", "Name", Session["storageNameId"] != null ? (string)Session["storageNameId"] : "-1");
            ViewData[DataGlobals.REFERENCE_STORAGE_SHOP_NAME] = storageNameList;

            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            if (!(idStorage != null && idStorage != "-1"))
            {
                foreach (var item in storageNames){
                    idStorage = item.Id.ToString();
                    break;
                }                
            }

            int id = System.Int32.Parse(idStorage);
            if (date == null) date = DateTime.Today.ToString("dd.MM.yy");
            DateTime dt = DateTime.ParseExact(date + " 00:00:00", DataGlobals.DATE_TIME_FORMAT, null);
            if (dateEnd == null) dateEnd = DateTime.Today.ToString("dd.MM.yy");
            DateTime dtEnd = DateTime.ParseExact(dateEnd + " 23:59:59", DataGlobals.DATE_TIME_FORMAT, null);
            Session.Add("dateString", date);
            Session.Add("dateEndString", dateEnd);
            Session.Add("idStorage", idStorage);
            queryParams.Add("OrganizationId", int.Parse(getCurrentEnterpriseId()));
            queryParams.Add("StorageNameId", id);
            queryParams.Add("[>=]OperDate", dt);
            queryParams.Add("[<=]OperDate", dtEnd);
            if (OperTypes != null)
            {
                if (OperTypes > 0)
                {
                    queryParams.Add("OperTypeId", OperTypes);
                }
            }
            if (Shops != null)
            {
                queryParams.Add("ShopId", (int)Shops);
            }
            if (Nomenclature != null)
            {
                queryParams.Add("NomenclatureId", (int)Nomenclature);
            }
            orderParams.Add("OperDate", ASC);
            IList<OperationSimple> operations = operationSimpleRepository.GetByLikeCriteria(queryParams, orderParams);
            return View(viewName, operations);
        }

        [GridAction]
        public ActionResult _Filtering(string filter, string dateString, string dateEndString, string idStorage, int? operType, int? Shops, int? Nomenclature)
        {
            string[] filterParams = new string[0];
            if (filter!=null)
                filterParams = filter.Split(new string[] { "~and~" }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            int id = System.Int32.Parse(idStorage);
            DateTime dt = DateTime.ParseExact(dateString + " 00:00:00", DataGlobals.DATE_TIME_FORMAT, null);
            DateTime dtEnd = DateTime.ParseExact(dateEndString + " 23:59:59", DataGlobals.DATE_TIME_FORMAT, null);
            queryParams.Add("OrganizationId", int.Parse(getCurrentEnterpriseId()));
            queryParams.Add("StorageNameId", id);
            queryParams.Add("[>=]OperDate", dt);
            queryParams.Add("[<=]OperDate", dtEnd);
            if (operType != null)
            {
                if (operType > 0)
                {
                    queryParams.Add("OperTypeId", operType);
                }                                    
            }
            if (Shops != null)
            {
                queryParams.Add("ShopId", (int)Shops);
            }
            if (Nomenclature != null)
            {
                queryParams.Add("NomenclatureId", (int)Nomenclature);
            }

            for (int i = 0; i < filterParams.Length; i++) {
                if (filterParams[i].StartsWith("DocNumber"))
                {
                    string paramValue = "";
                    Match m = Regex.Match(filterParams[i], @"'(?<value>.*)'$");
                    if (m.Success)
                    {
                        paramValue = m.Groups["value"].Value;
                        if (filterParams[i].IndexOf("~eq~") > 0)
                        {
                            queryParams.Add("[=]DocNumber", paramValue);
                        }
                        if (filterParams[i].IndexOf("~nq~") > 0)
                        {
                            queryParams.Add("[!=]DocNumber", paramValue);
                        }

                        if (filterParams[i].IndexOf("~startswith~") > 0)
                        {
                            queryParams.Add("DocNumber", paramValue);
                        }
                        if (filterParams[i].IndexOf("~endswith~") > 0)
                        {
                            queryParams.Add("[^]DocNumber", paramValue);
                        }
                        if (filterParams[i].IndexOf("~substringof~") > 0)
                        {
                            queryParams.Add("[*]DocNumber", paramValue);
                        }
                    }
                }
                if (filterParams[i].StartsWith("Fio"))
                {
                    string paramValue = "";
                    Match m = Regex.Match(filterParams[i], @"'(?<value>.*)'$");
                    if (m.Success)
                    {
                        paramValue = m.Groups["value"].Value;
                        if (filterParams[i].IndexOf("~eq~") > 0)
                        {
                            queryParams.Add("[=]Fio", paramValue);
                        }
                        if (filterParams[i].IndexOf("~nq~") > 0)
                        {
                            queryParams.Add("[!=]Fio", paramValue);
                        }

                        if (filterParams[i].IndexOf("~startswith~") > 0)
                        {
                            queryParams.Add("Fio", paramValue);
                        }
                        if (filterParams[i].IndexOf("~endswith~") > 0)
                        {
                            queryParams.Add("[^]Fio", paramValue);
                        }
                        if (filterParams[i].IndexOf("~substringof~") > 0)
                        {
                            queryParams.Add("[*]Fio", paramValue);
                        }
                    }
                }
            }
            orderParams.Add("OperDate", ASC);
            IList<OperationSimple> operations = operationSimpleRepository.GetByLikeCriteria(queryParams, orderParams);
            return View(new GridModel<OperationSimple> { Data = operations }); 
        }

        [GridAction]
        [Transaction]
        public ActionResult Select(string dateString, string dateEndString, string idStorage, int operType, int? Shops, int? Nomenclature)
        {
            IList<Operation> newList = new List<Operation>();
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            Dictionary<string, object> orderParams = new Dictionary<string, object>();

            if (!(idStorage != null && idStorage != "-1"))
            {
                string idStorageC = (string)Session["idStorage"];
                if (idStorageC != null) idStorage = idStorageC;
            }

            if (idStorage != null)
            {
                if (idStorage != "-1") Session.Add("idStorage", idStorage);
                //HttpContext.Cache.Insert("idStorage", idStorage);
                if (idStorage.Length > 0)
                {
                    List<Storage> model = new List<Storage>();
                    int id = System.Int32.Parse(idStorage);
                    queryParams.Add("Storage.StorageName.Id", id);


                    if (dateString != null)
                    {
                        //HttpContext.Cache.Insert("dateString", dateString);
                        Session.Add("dateString", dateString);
                        Session.Add("dateEndString", dateEndString);
                        Session.Add("operType", operType);
                        Session.Add("Shops", Shops);
                        Session.Add("Nomenclature", Nomenclature);
                        queryParams.Add("Organization.Id", int.Parse(getCurrentEnterpriseId()));
                        //IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
                        //string format = "dd.MM.yyyy";
                        //DateTime dt = DateTime.MinValue;
                        DateTime dt = DateTime.ParseExact(dateString + " 00:00:00", DataGlobals.DATE_TIME_FORMAT, null);
                        DateTime dtEnd = DateTime.ParseExact(dateEndString + " 23:59:59", DataGlobals.DATE_TIME_FORMAT, null);
                        queryParams.Add("[>=]OperDate", dt);
                        queryParams.Add("[<=]OperDate", dtEnd);
                        if (operType > 0) {
                            queryParams.Add("OperTypeId", operType);
                        }
                        if (Shops != null)
                        {
                            queryParams.Add("ShopId", (int)Shops);
                        }
                        if (Nomenclature!= null)
                        {
                            queryParams.Add("NomenclatureId", (int)Nomenclature);
                        }
                        orderParams.Add("OperDate", ASC);
                        IList<Operation> operations = operationRepository.GetByLikeCriteria(queryParams, orderParams);
                        //var operations = operationRepository.GetAll();
                        //var rows = GetRows<Operation>(operations);
                        List<string> excludeProperty = new List<string>();
                        excludeProperty.Add("Organization");
                        excludeProperty.Add("DocType");
                        excludeProperty.Add("WorkerWorkplace.Organization.NormaOrganization");
                        excludeProperty.Add("WorkerWorkplace.Organization.BeginDate");
                        excludeProperty.Add("WorkerWorkplace.Organization.RootOrganization");
                        excludeProperty.Add("WorkerWorkplace.Organization.Parent");
                        excludeProperty.Add("WorkerWorkplace.Worker.NomBodyPartSizes.NomBodyPart");
                        excludeProperty.Add("WorkerWorkplace.Worker.WorkerCategory");
                        excludeProperty.Add("WorkerWorkplace.Worker.Sex");
                        excludeProperty.Add("Storage.StorageName");
                        excludeProperty.Add("Storage.Nomenclature.Unit");
                        excludeProperty.Add("Storage.Nomenclature.Sex");
                        excludeProperty.Add("Storage.Nomenclature.NomGroup");
                        foreach (Operation current in operations)
                        {
                            Operation oper = rebuildOperation(current, excludeProperty);
                            //if (newList.Count < 2000)
                            newList.Add(oper);
                        }


                    }
                    //return View(new GridModel(operations));
                }
            }

            return View(new GridModel(newList));
        }

        [HttpPost]
        public ActionResult _GetNomenclatures(string text)
        {
            /*
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("Organization.Id", int.Parse(getCurrentEnterpriseId()));
            int code = -1;
            if (int.TryParse(text.Substring(2), out code))
                queryParams.Add("ExternalCode", text);
            else
                queryParams.Add("Name", text);
            IList<Nomenclature> nomens = nomenRepository.GetByLikeCriteria(queryParams);
            */
            IList<Nomenclature> nomens = nomenRepository.GetNomenclature(text , getCurrentEnterpriseId());

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
