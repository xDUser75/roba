using System.Web.Mvc;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using SharpArch.Core.PersistenceSupport;
using Store.Core;
using Telerik.Web.Mvc;
using System.Collections.Generic;
using Store.Data;
using Store.Data.NHibernateMaps;
using System.Web.Script.Serialization;
using System;
using System.Linq;
using System.Web;
using System.Xml.XPath;
using System.Xml;
using System.Text.RegularExpressions;
using NHibernate;
using Store.Core.RepositoryInterfaces;
using Store.Core.External.Interfaсe;
using Store.Core.Utils;
using System.Text;
using NHibernate.Criterion;


namespace Store.Web.Controllers
{
    [HandleError]
    public class StoragesController : ViewedController
    {
        private enum MoveType { Unknown = 0, StorageIn, StorageOut, StornoIn, StornoOut };
        private readonly OrganizationRepository organizationRepository;
        private readonly CriteriaRepository<Storage> storageRepository;
        private readonly StorageNameRepository storageNameRepository;
        private readonly CriteriaRepository<OperType> operTypeRepository;
        private readonly CriteriaRepository<DocType> docTypeRepository;
        private readonly CriteriaRepository<Motiv> motivRepository;
        private readonly CriteriaRepository<Operation> operationRepository;
        private readonly NomenclatureRepository nomenRepository;
        private readonly CriteriaRepository<NomGroup> nomGroupRepository;
        private readonly CriteriaRepository<NomBodyPartSize> nomBodyPartSizeRepository;
        private readonly NomBodyPartRepository nomBodyPartRepository;
        private readonly CriteriaRepository<Unit> unitRepository;
        private readonly SexRepository sexRepository;
        private readonly IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
        private readonly CriteriaRepository<COMING_SAP> comingRepository;
        
        public StoragesController(OrganizationRepository organizationRepository,
                                  CriteriaRepository<Storage> storageRepository,
                                  StorageNameRepository storageNameRepository,
                                  CriteriaRepository<OperType> operTypeRepository,
                                  CriteriaRepository<DocType> docTypeRepository,
                                  CriteriaRepository<Motiv> motivRepository,
                                  CriteriaRepository<Operation> operationRepository,
                                  NomenclatureRepository nomenRepository,
                                  CriteriaRepository<NomBodyPartSize> nomBodyPartSizeRepository,
                                  NomBodyPartRepository nomBodyPartRepository,
                                  CriteriaRepository<Unit> unitRepository,
                                  SexRepository sexRepository,
                                  CriteriaRepository<NomGroup> nomGroupRepository,
                                  CriteriaRepository<COMING_SAP> comingRepository)
        {
            Check.Require(storageRepository != null, "StorageRepository may not be null");

            this.storageRepository = storageRepository;
            this.organizationRepository = organizationRepository;
            this.storageNameRepository = storageNameRepository;
            this.operTypeRepository = operTypeRepository;
            this.docTypeRepository = docTypeRepository;
            this.motivRepository = motivRepository;
            this.operationRepository = operationRepository;
            this.nomenRepository = nomenRepository;
            this.nomBodyPartSizeRepository = nomBodyPartSizeRepository;
            this.nomBodyPartRepository = nomBodyPartRepository;
            this.unitRepository = unitRepository;
            this.sexRepository = sexRepository;
            this.nomGroupRepository = nomGroupRepository;
            this.comingRepository = comingRepository;
        }

        private bool isGoodMove(string moveType, string knownMove)
        {
            bool ret = false;
            if (knownMove != null)
            {
                string[] t = knownMove.Split(',');
                for (int i = 0; i < t.Length; i++)
                {
                    if (t[i] != moveType)
                    {
                        ret = true;
                    }
                }
            }
            return ret;
        }

        private MoveType getKnownMove(string moveType, StorageName currentStorage)
        {
            MoveType ret = MoveType.Unknown;
            if (isGoodMove(moveType, currentStorage.StorageIn)) ret = MoveType.StorageIn;
            if (isGoodMove(moveType, currentStorage.StorageOut)) ret = MoveType.StorageOut;
            if (isGoodMove(moveType, currentStorage.StornoIn)) ret = MoveType.StornoIn;
            if (isGoodMove(moveType, currentStorage.StornoOut)) ret = MoveType.StornoOut;
            return ret;
        }
        private string formatXMLTag(string val)
        {
			if (val==null) return "";
			string sb=val;
            sb = sb.Replace("&", "&amp;");
            sb = sb.Replace("\"", "&quot;");
            sb = sb.Replace(">", "&gt;");
            sb = sb.Replace("<", "&lt;");
			return sb;		 		
		}


        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_STORAGE_EDIT + ", " + DataGlobals.ROLE_STORAGE_VIEW))]
        public ActionResult Index()
        {
            if (Session["_idOrg"] == null) Session["_idOrg"] = getOrgByArmId();
            int curOrg = getIntCurrentEnterpriseId();
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            if (Request.Params["storageCode"]!=null){
                queryParams.Add("Organization.Id", curOrg);
                queryParams.Add("Externalcode", HttpUtility.UrlDecode(Request.Params["storageCode"]));
                try
                {
                    ViewData["storageId"] = storageNameRepository.FindOne(queryParams).Id;
                }
                catch (Exception) { }
            }

            ViewData["Senders"] = storageNameRepository.GetSenders();
            ViewData[DataGlobals.REFERENCE_SEX] = sexRepository.GetAll();
            ViewData[DataGlobals.REFERENCE_SHOP] = organizationRepository.GetShops(getCurrentEnterpriseId());
            ViewData[DataGlobals.REFERENCE_STORAGE_OPERATION] = ViewData[DataGlobals.REFERENCE_SHOP];
//            ViewData[DataGlobals.REFERENCE_STORAGE_OPERATION] = organizationRepository.GetShops(getCurrentEnterpriseId());
            queryParams.Clear();
            queryParams.Add("[!in]Id", DataGlobals.EXCLUDE_ID_INCOMING_STORAGE_DOCTYPE);
            ViewData[DataGlobals.REFERENCE_DOCUMENT_TYPE] = docTypeRepository.GetByCriteria(queryParams);
            ViewData[DataGlobals.REFERENCE_MOTIV] = motivRepository.GetAll();
            queryParams.Clear();
            queryParams.Add("Organization.Id", curOrg);
            ViewData[DataGlobals.REFERENCE_UNIT] = unitRepository.FindAll(queryParams);
//            ViewData[DataGlobals.REFERENCE_STORAGE_SHOP_NAME] = storageNameRepository.GetStorageShops(getCurrentEnterpriseId());
            Dictionary<string, object> query = new Dictionary<string, object>();
//            query.Add("OrgTreeId", int.Parse(getCurrentEnterpriseId()));
            query.Add("Organization.Id", curOrg);
            query.Add("[in]Id", ((Store.Core.Account.User)Session[DataGlobals.ACCOUNT_KEY]).ObjectsCSV);
            IList<StorageName> storageNames = storageNameRepository.GetByCriteria(query);

            SelectList storageNameList = new SelectList(storageNames, "Id", "Name", Session[DataGlobals.STORAGE_NAME_ID_LIST] != null ? (string)Session[DataGlobals.STORAGE_NAME_ID_LIST] : "-1");
            ViewData[DataGlobals.REFERENCE_STORAGE_SHOP_NAME] = storageNameList;
            if ((storageNames.Count > 0) && ((string)Session[DataGlobals.STORAGE_NAME_ID_LIST] == null))
            {
                Session[DataGlobals.STORAGE_NAME_ID_LIST] = storageNames[0].Id.ToString();
            }

            //SelectList storageNameList = new SelectList(storageNameRepository.GetStorageShops(getCurrentEnterpriseId()), "Id", "Name", Session["storageNameId"] != null ? (string)Session["storageNameId"] : "0");


            ViewData[DataGlobals.REFERENCE_NOM_BODY_PART] = nomBodyPartRepository.GetAllSizeNotIn(new int[] { DataGlobals.GROWTH_SIZE_ID });
            queryParams.Clear();
            queryParams.Add("NomBodyPart.Id", DataGlobals.GROWTH_SIZE_ID);
            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            orderParams.Add("SizeNumber", ASC);
            ViewData[DataGlobals.REFERENCE_NOM_BODY_PART_SIZE_GROWTH] = nomBodyPartSizeRepository.GetByCriteria(queryParams, orderParams);
            string storageName = "0";
            if (storageNames.Count>0) storageName = storageNames[0].Id.ToString();
            if ( DataGlobals.ORG_ID_EVRAZRUDA == curOrg.ToString())
                ViewData[DataGlobals.REFERENCE_STORAGE_NAME] = storageNameRepository.GetAllShops("" + getCurrentEnterpriseId(), storageName);
            else 
                ViewData[DataGlobals.REFERENCE_STORAGE_NAME] = storageNameRepository.GetShops(getCurrentEnterpriseId());

//            ViewData[DataGlobals.REFERENCE_STORAGE_NAME] = storageNameList;
            return View(viewName);
        }

        
        public ActionResult LoadInvoice(int armId) {
            if (Session[Store.Data.DataGlobals.ACCOUNT_KEY] == null)
            {
                string urlParam = Request.RawUrl.Substring(Request.RawUrl.IndexOf('?'));
              //  string urlParam = null;
              //  var dict = param.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
              //.Select(part => part.Split('='))
              //.ToDictionary(split => split[0], split => split[1]);
              //  if (Request.Params["storageCode"] != null) urlParam = urlParam + "storageCode=" + dict["storageCode"];
              //  if (Request.Params["docNumber"] != null)
              //  {
              //      if (urlParam != null) urlParam = urlParam + "&";
              //      urlParam = urlParam + "docNumber=" + dict["docNumber"];
              //  }
              //  if (Request.Params["docYear"] != null)
              //  {
              //      if (urlParam != null) urlParam = urlParam + "&";
              //      urlParam = urlParam + "docYear=" + Request.Params["docYear"];
              //  }
              //  if (urlParam != null) urlParam = "?" + urlParam;

                string logonUser = HttpContext.Request.LogonUserIdentity.Name;
                return RedirectToAction("ValidateUser", "LoginAccount", new { userName = logonUser, password = "Ра$$w0rd", _EnterpriceList = armId.ToString(), returnUrl = "/Storages" + (urlParam ??" ") , force = true});
            }
              
            //[Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_STORAGE_EDIT + ", " + DataGlobals.ROLE_STORAGE_VIEW))]
            return Index();
        }


        [HttpPost]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_STORAGE_EDIT + ", " + DataGlobals.ROLE_STORAGE_VIEW))]
        public ActionResult _LoadingStorage(string id)
        {
            IList<StorageName> DropDownList1 = null;
            if (id.Length > 0)
            {
                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                int orgId = System.Int32.Parse(id);
                queryParams.Add("Organization.Id", orgId);
                DropDownList1 = storageNameRepository.FindAll(queryParams);
            } else {
                DropDownList1 = new List<StorageName>();
            }
            return new JsonResult
            {
                Data = new SelectList(DropDownList1, "Id", "Name")
            };
        }

        [GridAction]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _SelectEmptyRow() {
            return new JsonResult
            {
                Data = new List<NomenclatureSimple>()
            };
        }

        [GridAction]
        [AcceptVerbs(HttpVerbs.Post)]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_STORAGE_EDIT + ", " + DataGlobals.ROLE_STORAGE_VIEW))]
        public ActionResult _SelectionClientSide_Storage(string idStorage)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();

            if (!(idStorage != null && idStorage != "-1"))
            {
                string idStorageC = (string)Session[DataGlobals.STORAGE_NAME_ID_LIST];
                //(string)HttpContext.Cache.Get("storageNameId");
                if (idStorageC != null) idStorage = idStorageC;
            }

            if (idStorage != null)
            {
                if (idStorage != "-1") Session[DataGlobals.STORAGE_NAME_ID_LIST] = idStorage;
                //HttpContext.Cache.Insert("idStorage", idStorage);
                if (idStorage.Length > 0)
                {
                    List<Storage> model = new List<Storage>();
                    int id = System.Int32.Parse(idStorage);
                    queryParams.Add("StorageName.Id", id);
                    queryParams.Add("[>]Quantity", 0);                    
                    Dictionary<string, object> orderParams = new Dictionary<string, object>();
                    orderParams.Add("Nomenclature.Name", ASC);
                    IEnumerable<Storage> storage = storageRepository.GetByCriteria(queryParams, orderParams);
                    foreach (var item in storage)
                    {
                            model.Add(rebuildStorage(item));
                    };
                    return View(new GridModel<Storage>
                    {
                        Data = model
                    });
                }
            }
            return View(viewName);
        }

        [GridAction]
        [AcceptVerbs(HttpVerbs.Post)]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN +  ", " + DataGlobals.ROLE_STORAGE_EDIT))]

        public ActionResult _OutNomenclature(int id, int storageId, string docNumber, string docDate, int DocTypeId, int Quantity)
        {
            Storage storageFrom = storageRepository.Get(id);
            IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
            string format = "dd.MM.yyyy";

            Operation operation = new Operation();
            operation.Quantity = Quantity;
            operation.OperType = operTypeRepository.Get(DataGlobals.OPERATION_STORAGE_OUT);
            operation.OperDate = DateTime.ParseExact(docDate, format, culture); 
            operation.DocType = docTypeRepository.Get(DocTypeId);
            operation.DocNumber = docNumber;
            operation.DocDate = DateTime.ParseExact(docDate, format, culture); 
            operation.Organization = organizationRepository.Get(storageFrom.StorageName.Organization.RootOrganization);
            operation.Storage = storageFrom;
            operation.Wear = storageFrom.Wear;
            operation.StorageName = storageFrom.StorageName;
            operation.Partner = storageNameRepository.Get(storageId);
            //Dictionary<string, object> queryParams = new Dictionary<string, object>();
            //queryParams.Add("StorageName.Id", storageId);
            //queryParams.Add("Nomenclature.Id", storageFrom.Nomenclature.Id);
            //if (storageFrom.Growth != null)
            //{
            //    queryParams.Add("Growth.Id", storageFrom.Growth.Id);
            //}
            //else {
            //    queryParams.Add("Growth", null);
            //}
            //queryParams.Add("NomBodyPartSize.Id", storageFrom.NomBodyPartSize.Id);
            //queryParams.Add("Wear", storageFrom.Wear);
////            Storage storageTo = storageRepository.FindOne(queryParams);
////            if (storageTo == null) {
//                storageTo = new Storage();
//                storageTo.NomBodyPartSize = storageFrom.NomBodyPartSize;
//                storageTo.Nomenclature = storageFrom.Nomenclature;
//                storageTo.Price = storageFrom.Price;
//                storageTo.Growth = storageFrom.Growth;
//                storageTo.Wear = storageFrom.Wear;
//                storageTo.StorageName = storageNameRepository.Get(storageId);

//            }
//            storageTo.Quantity = storageTo.Quantity + Quantity;


            storageFrom.Quantity = storageFrom.Quantity - Quantity;
//            storageRepository.SaveOrUpdate(storageTo);
            operationRepository.SaveOrUpdate(operation);
            storageRepository.SaveOrUpdate(storageFrom);
            return new JsonResult();
        }

        public void InNomenclature(int storageId, string docNumber, string docDate, int DocTypeId, int Quantity, int Growth, string Wear, int NomenclatureId, int? SizeId, int StorIdIncoming)
        {
            if (Wear == null) {
                Wear = "100";
            }

            if (Wear == "") {
                Wear = "100";
            }
            IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
            string format = "dd.MM.yyyy";
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("StorageName.Id", storageId);
            queryParams.Add("Nomenclature.Id", NomenclatureId);
            if (Growth >-1)
            {
                queryParams.Add("Growth.Id", Growth);
            }
            else {
                queryParams.Add("Growth", null);
            }
            if (SizeId > 0)
            {
                queryParams.Add("NomBodyPartSize.Id", SizeId);
            }
            else {
                queryParams.Add("NomBodyPartSize", null);
            }
            queryParams.Add("Wear", Wear);
//            queryParams.Add("Price", Price);
            Storage storageTo = storageRepository.FindOne(queryParams);
            if (storageTo == null) {
                storageTo = new Storage();
                if (SizeId == null)
                {
                    storageTo.NomBodyPartSize = null;
                }
                else {
                    storageTo.NomBodyPartSize = nomBodyPartSizeRepository.Get((int)SizeId);
                }
                storageTo.Nomenclature = nomenRepository.Get(NomenclatureId);
//                storageTo.Price = Price;
                if (Growth > -1){
                    storageTo.Growth =nomBodyPartSizeRepository.Get(Growth);
                }
                storageTo.Wear = Wear;
                storageTo.StorageName = storageNameRepository.Get(storageId);
            }
            storageTo.Quantity = storageTo.Quantity + Quantity;

            Operation operation = new Operation();
            operation.Quantity = Quantity;
            operation.OperType = operTypeRepository.Get(DataGlobals.OPERATION_STORAGE_IN);
            operation.OperDate = DateTime.Now;
            operation.DocType = docTypeRepository.Get(DocTypeId);
            operation.DocNumber = docNumber;
            operation.DocDate = DateTime.ParseExact(docDate, format, culture);
            operation.Organization = organizationRepository.Get(storageTo.StorageName.Organization.RootOrganization);
            operation.Storage = storageTo;
            operation.StorageName = storageTo.StorageName;
            operation.Partner = storageNameRepository.Get(StorIdIncoming);
            operationRepository.SaveOrUpdate(operation);
            storageRepository.SaveOrUpdate(storageTo);
        }

        [HttpPost]
        public ActionResult GetNomenclatures(string nomGroupId, string text)
        {

            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            int code = -1;
            string idOrg = getCurrentEnterpriseId();
            /*
            int idEnterprise = System.Int32.Parse(idOrg);
            queryParams.Add("Organization.Id", idEnterprise);
            if (int.TryParse(text, out code))
                queryParams.Add("ExternalCode", text);
            else
                queryParams.Add("Name", text);
             */
            //IList<Nomenclature> nomens = nomenRepository.GetByLikeCriteria(queryParams);
            IList<Nomenclature> nomens = nomenRepository.GetNomenclature( text, idOrg);

            Dictionary<string, string> list = new Dictionary<string, string>();
            foreach (var item in nomens)
            {
                list.Add(item.Id.ToString()+"|"+item.NomBodyPart.Id.ToString(),item.ExternalCode+" - "+item.Name);
            };

            return new JsonResult
            {
                Data = new SelectList(list, "Key", "Value")
            };
        }

        [GridAction]
        [HttpPost]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_STORAGE_EDIT))]
        public ActionResult _SelectionSAPData(int storageId, int docType, string document, int year, string docDate) {
            
            string message = "";
            Organization currentOrganization = organizationRepository.Get(int.Parse(getCurrentEnterpriseId()));
            StorageName storageName = storageNameRepository.Get(storageId);
            IList<COMING_SAP> model = new List<COMING_SAP>();
            Dictionary<string, Object> param = new Dictionary<string, Object>();
            if (document != "-1")
            {
                //docDate = DateTime.ParseExact(docDate, "yyyy-MM-dd", new System.Globalization.CultureInfo("ru-RU", true)).ToString(DataGlobals.DATE_FORMAT_FULL_YEAR)
                param.Add("OperType.Id", DataGlobals.OPERATION_STORAGE_IN);
                param.Add("DocNumber", document);
                param.Add("Organization.Id", currentOrganization.Id);
                param.Add("[>=]DocDate", DateTime.ParseExact("01.01." + year, "dd.MM.yyyy", culture));
                param.Add("[<=]DocDate", DateTime.ParseExact("31.12." + year, "dd.MM.yyyy", culture));
                param.Add("[]RefOperation","");
                param.Add("Storage.StorageName", storageName);
                if (docDate != "")
                    param.Add("DocDate", DateTime.ParseExact(docDate, DataGlobals.DATE_FORMAT_FULL_YEAR, null));

                IList<Operation> retList = operationRepository.GetByLikeCriteria(param);
                
                if (retList.Count > 0){
                    message = "Документ с номером " + document + " за " + year + " год уже обрабатывался!";
                    if (docDate == "" && getCurrentEnterpriseId() == DataGlobals.ORG_ID_VGOK) message += " Введите точную дату документа";
                } else
                {
                   param.Clear();
                    StorageName currentStorage = storageNameRepository.Get(storageId);
                    string assemblyName = ApplicationConfig.ReadVariable("/Configuration/Organization[@id=" + getCurrentEnterpriseId() + "]/InterfaceLoadInvoice/AssemblyName");
                    string className = ApplicationConfig.ReadVariable("/Configuration/Organization[@id=" + getCurrentEnterpriseId() + "]/InterfaceLoadInvoice/ClassName");
                    IExternalLoaderInvoice loader = (IExternalLoaderInvoice)Store.Core.Utils.Reflection.LoadClassObject(assemblyName, className);
                    if (loader != null)
                    {
                        model = loader.GetInvoice(currentOrganization, currentStorage, nomenRepository, nomGroupRepository, docType, document, year, docDate, out message);
                    }
                    /*
                    if (currentOrganization.Id == 3)
                    {
                        param.Clear();
                        param.Add("DocNumber", document);
                        param.Add("[>=]FullDocDate", DateTime.ParseExact("01.01." + year, "dd.MM.yyyy", culture));
                        param.Add("[<=]FullDocDate", DateTime.ParseExact("31.12." + year, "dd.MM.yyyy", culture));
                        model = comingRepository.GetByCriteria(param);
                        
                    }
                    else
                        model = CallRFC.getNomenclature(currentOrganization, currentStorage, nomenRepository, nomGroupRepository, docType, document, year, out message);
                    */
                }
            }
            else return View(new GridModel<COMING_SAP> { Data = model });
            HttpResponseBase Response = ControllerContext.HttpContext.Response;
            Response.ContentType = "text/xml";
            String retData = "<data><message>"+formatXMLTag(message)+"</message>";
            retData = retData + "<rows>";
            foreach (var item in model)
            {
                retData = retData + "<row>";
                retData = retData + "<Id>" + item.Id + "</Id>";
                retData = retData + "<DocNumber>" + formatXMLTag(item.DocNumber) + "</DocNumber>";
                retData = retData + "<DocDate>" + item.DocDate + "</DocDate>";
                //retData = retData + "<GalDocDate>" + item.GalDocDate + "</GalDocDate>";
                retData = retData + "<DocTypeId>" + item.DocTypeId + "</DocTypeId>";
                retData = retData + "<MaterialId>" + item.MaterialId + "</MaterialId>";
                if (item.MATERIAL==null)
                    retData = retData + "<Material> НОМЕНКЛАТУРА НЕ НАЙДЕНА В СПРАВОЧНИКЕ!!! </Material>";
                else
                    retData = retData + "<Material>" + formatXMLTag(item.MATERIAL) + "</Material>";
                retData = retData + "<ExternalCode>" + formatXMLTag(item.ExternalCode) + "</ExternalCode>";
                retData = retData + "<Quantity>" + item.QUANTITY + "</Quantity>";
                retData = retData + "<Price>" + item.Price + "</Price>";
                retData = retData + "<UOM>" + item.UOM + "</UOM>";
                retData = retData + "<LC>" + item.LC + "</LC>";
                retData = retData + "<SV>" + item.SV + "</SV>";
                retData = retData + "<IsWinter>" + item.IsWinter + "</IsWinter>";
                retData = retData + "<NomBodyPartId>" + item.NomBodyPartId + "</NomBodyPartId>";
                retData = retData + "<NomBodyPartName>" + formatXMLTag(item.NomBodyPartName) + "</NomBodyPartName>";
                retData = retData + "<NomGroupId>" + item.NomGroupId + "</NomGroupId>";
                retData = retData + "<NomGroupName>" + formatXMLTag(item.NomGroupName) + "</NomGroupName>";
                if (item.SAPNomGroupId == null) {
                    item.SAPNomGroupId = ""+item.NomGroupId;
                    item.SAPNomGroupName = item.NomGroupName;
                }
                retData = retData + "<SAPNomGroupId>" + item.SAPNomGroupId + "</SAPNomGroupId>";
                retData = retData + "<SAPNomGroupName>" + formatXMLTag(item.SAPNomGroupName) + "</SAPNomGroupName>";
                retData = retData + "<MoveType>" + item.MoveType + "</MoveType>";
//По названию номенклатуры пробуем определить размеры и рост
                string size="";
                string growth = "";
                if (item.SizeName != null) {
                    size = item.SizeName.ToString();
                }
                if (item.GrowthName != null)
                {
                    growth = item.GrowthName.ToString();
                }

                // Если размеры в справочнике номенклатур не определены
                if (size.Length == 0 &&  item.MATERIAL!=null )
                {
                    Match m = Regex.Match(item.MATERIAL, @"[\.\s_](?<size>\d{2})\s*$");
                    if (m.Success)
                    {
                        size = m.Groups["size"].Value;
                    }
                    else
                    {
                        m = Regex.Match(item.MATERIAL, @"[\.\s]\d{2,3}-(?<size>\d{2,3})/\d{3}-(?<growth>\d{3})\s*$");
                        if (m.Success)
                        {
                            size = m.Groups["size"].Value;
                            growth = m.Groups["growth"].Value;
                        }
                        else
                        {
                            m = Regex.Match(item.MATERIAL, @"[\.\s]\d{2}-(?<size>\d{2})\s*$");
                            if (m.Success)
                            {
                                size = m.Groups["size"].Value;
                            }
                            else
                            {
                                m = Regex.Match(item.MATERIAL, @"[_]\d{2,3}-(?<size>\d{2,3})\s\d{3}-(?<growth>\d{3})\s*$");
                                if (m.Success)
                                {
                                    size = m.Groups["size"].Value;
                                    growth = m.Groups["growth"].Value;
                                }
                            }
                        }
                    }
                    //Пробуем определить размеры и рост
                    //Рост:
                    if (growth.Length > 0)
                    {
                        param.Clear();
                        param.Add("NomBodyPart.Id", DataGlobals.GROWTH_SIZE_ID);
                        param.Add("SizeNumber", growth);
                        NomBodyPartSize nomBodyPartSize = nomBodyPartSizeRepository.FindOne(param);
                        if (nomBodyPartSize != null)
                        {
                            retData = retData + "<GrowthId>" + nomBodyPartSize.Id + "</GrowthId>";
                            retData = retData + "<GrowthName>" + formatXMLTag(nomBodyPartSize.SizeNumber) + "</GrowthName>";
                        }
                    }
                    //Размер
                    if (size.Length > 0)
                    {
                        param.Clear();
                        param.Add("NomBodyPart.Id", item.NomBodyPartId);
                        int outVal = -1;
                        if (int.TryParse(size, out outVal) == true)
                        {
                            outVal = int.Parse(size);
                            if (outVal >= 70)
                            {
                                size = "" + Math.Round((double)outVal / 2);
                            }
                        }
                        param.Add("SizeNumber", size);
                        NomBodyPartSize nomBodyPartSize = nomBodyPartSizeRepository.FindOne(param);
                        if (nomBodyPartSize != null)
                        {
                            retData = retData + "<SizeId>" + nomBodyPartSize.Id + "</SizeId>";
                            retData = retData + "<SizeName>" + formatXMLTag(nomBodyPartSize.SizeNumber) + "</SizeName>";
                        }
                    }
                }
                else {
                    retData = retData + "<SizeId>" + item.SizeId + "</SizeId>";
                    retData = retData + "<SizeName>" + formatXMLTag(size) + "</SizeName>";
                    retData = retData + "<GrowthId>" + item.GrowthId + "</GrowthId>";
                    retData = retData + "<GrowthName>" + formatXMLTag(growth) + "</GrowthName>";

                }
                if (item.SexId == null && item.MATERIAL != null)
                {
                    if (item.MATERIAL.IndexOf("муж.") > 0)
                    {
                        Sex sex = sexRepository.Get(1);
                        retData = retData + "<SexId>" + sex.Id + "</SexId>";
                        retData = retData + "<SexName>" + sex.Name + "</SexName>";
                    }
                    else if (item.MATERIAL.IndexOf("жен.") > 0)
                    {
                        Sex sex = sexRepository.Get(2);
                        retData = retData + "<SexId>" + sex.Id + "</SexId>";
                        retData = retData + "<SexName>" + sex.Name + "</SexName>";
                    }
                    param.Clear();
                    param.Add("Organization.Id", int.Parse(getCurrentEnterpriseId()));
                    param.Add("ExternalCode", item.UOM);
                    Unit unit = unitRepository.FindOne(param);
                    if (unit != null)
                    {
                        retData = retData + "<UnitId>" + unit.Id + "</UnitId>";
                        retData = retData + "<UnitName>" + formatXMLTag(unit.Name) + "</UnitName>";
                    }
                }
                else
                {
                    retData = retData + "<SexId>" + item.SexId + "</SexId>";
                    retData = retData + "<SexName>" + item.SexName + "</SexName>";
                    retData = retData + "<UnitId>" + item.UnitId + "</UnitId>";
                    retData = retData + "<UnitName>" + formatXMLTag(item.UnitName) + "</UnitName>";
                }

                retData = retData + "</row>";
            }
            retData = retData + "</rows></data>";
            Response.Write(retData);
            return null; 
        }

        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_STORAGE_EDIT))]
        public ActionResult _Select_NombodyPartSize(string id, string idSize)
        {
            IList<SelectListItem> model = new List<SelectListItem>();
            if (id.Length > 0)
            {
                int ID = System.Int32.Parse(id);
                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                queryParams.Add("NomBodyPart.Id", ID);
                queryParams.Add("IsActive", true);
                Dictionary<string, object> orderParams = new Dictionary<string, object>();
                orderParams.Add("SizeNumber", ASC);

                IEnumerable<NomBodyPartSize> nomBodySize = nomBodyPartSizeRepository.GetByCriteria(queryParams, orderParams);
                foreach (var item in nomBodySize)
                {
                    model.Add(new SelectListItem { Text = item.SizeNumber.ToString(), Value = item.Id.ToString(), Selected = (item.Id.ToString() == idSize) });
                };
            }

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction]
        [Transaction]
        public ActionResult _SaveSapData(int storageId, IEnumerable<COMING_SAP> inserted, IEnumerable<COMING_SAP> updated, IEnumerable<COMING_SAP> deleted)
        {
            IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
            Organization currentOrganization = organizationRepository.Get(int.Parse(getCurrentEnterpriseId()));
            StorageName currentStorageName = storageNameRepository.Get(storageId);
            HttpResponseBase Response = ControllerContext.HttpContext.Response;
            Response.ContentType = "text/html";

            Dictionary<string, object> queryParams = new Dictionary<string, object>(); 
            string document="";
            string year="";
            //string message="";
            if (updated != null)
            {
                foreach (var gridRow in updated)
                {
                    document = gridRow.DocNumber;
                    year = DateTime.ParseExact(gridRow.DocDate, DataGlobals.DATE_FORMAT_FULL_YEAR, culture).ToString().Substring(6,4);
                    break;
                }
                queryParams.Add("OperType.Id", DataGlobals.OPERATION_STORAGE_IN);
                queryParams.Add("DocNumber", document);
                queryParams.Add("Organization.Id", currentOrganization.Id);
                queryParams.Add("[>=]DocDate", DateTime.ParseExact("01.01." + year, "dd.MM.yyyy", culture));
                queryParams.Add("[<=]DocDate", DateTime.ParseExact("31.12." + year, "dd.MM.yyyy", culture));
                queryParams.Add("[]RefOperation", "");
                queryParams.Add("Storage.StorageName", currentStorageName);

                IList<Operation> retList = operationRepository.GetByLikeCriteria(queryParams);
                if (retList.Count > 0)
                {
//                    ModelState.AddModelError("", "Документ с номером " + document + " за " + year + " год уже обрабатывался!");
                    Response.Write("Документ с номером " + document + " за " + year + " год уже обрабатывался!");

                }
                else
                {
                    foreach (var gridRow in updated)
                    {
                        //Определяем вид движения
                        MoveType currentMoveType = getKnownMove(gridRow.MoveType, currentStorageName);
                        //if (currentMoveType != CallRFC.MoveType.Unknown)
                        //{

//                            Dictionary<string, object> queryParams = new Dictionary<string, object>();
                            queryParams.Clear();
                            queryParams.Add("Organization", currentOrganization);
                            queryParams.Add("ExternalCode", gridRow.MaterialId.TrimStart('0'));
                            queryParams.Add("IsActive", true);
//                            Nomenclature currentNomenclature = nomenRepository.FindOne(queryParams);
// Добавила Назарова, т.к. старые номенклатуры, которые на руках сопоставилис кодами SAP и теперь по 
// ExternalCode может быть несколько записей
                            IList<Nomenclature> currentNomenclatures = nomenRepository.GetByLikeCriteria(queryParams);
                            Nomenclature currentNomenclature = null;
                            if (currentNomenclatures.Count > 0)
                                currentNomenclature = currentNomenclatures[0];

                        if (currentNomenclature == null)
                            {
                                NomGroup currentNomeGroup = nomGroupRepository.Get((int)gridRow.NomGroupId);
                                if (currentNomeGroup == null)
                                {
                                    currentNomeGroup = new NomGroup();
                                    currentNomeGroup.IsWinter = (bool)gridRow.IsWinter;
                                    currentNomeGroup.Name = gridRow.NomGroupName;

                                    currentNomeGroup.NomBodyPart = nomBodyPartRepository.Get((int)gridRow.NomBodyPartId);
                                    currentNomeGroup.IsActive = true;
                                    currentNomeGroup.ExternalCode = gridRow.SAPNomGroupId;
                                    currentNomeGroup.Organization = currentOrganization;
                                    nomGroupRepository.SaveOrUpdate(currentNomeGroup);
                                }
                                currentNomenclature = new Nomenclature();
                                currentNomenclature.Organization = currentOrganization;
                                currentNomenclature.ExternalCode = gridRow.MaterialId.TrimStart('0');
                                if (gridRow.MATERIAL.StartsWith("[]"))
                                {
                                    gridRow.MATERIAL = gridRow.MATERIAL.Substring(2);
                                }
                                currentNomenclature.Name = gridRow.MATERIAL;
                                currentNomenclature.Sex = sexRepository.Get((int)gridRow.SexId);
                                currentNomenclature.Unit = unitRepository.Get((int)gridRow.UnitId);
                                currentNomenclature.NomBodyPart = nomBodyPartRepository.Get((int)gridRow.NomBodyPartId);
                                currentNomenclature.NomGroup = currentNomeGroup;
                                currentNomenclature.StartDate = DateTime.Now;
                                currentNomenclature.IsActive = true;
                                nomenRepository.SaveOrUpdate(currentNomenclature);
                            }
                            else
                            {
                                NomBodyPart nbp = nomBodyPartRepository.Get((int)gridRow.NomBodyPartId);
                                //Если пользователь изменил информацию о группе
                                if ((nbp.Id != currentNomenclature.NomGroup.NomBodyPart.Id) || (currentNomenclature.NomGroup.IsWinter != (bool)gridRow.IsWinter))
                                {
                                    currentNomenclature.NomGroup.IsWinter = (bool)gridRow.IsWinter;
                                    currentNomenclature.NomGroup.NomBodyPart = nbp;
                                    nomGroupRepository.SaveOrUpdate(currentNomenclature.NomGroup);
                                }

                            }

                            queryParams.Clear();
                            queryParams.Add("StorageName", currentStorageName);
                            queryParams.Add("Nomenclature", currentNomenclature);
                            //if (gridRow.GrowthId > -1)
                            //{
                            //    queryParams.Add("Growth.Id", (int)gridRow.GrowthId);
                            //}
                            //else
                            //{
                            //    queryParams.Add("[]Growth", 0);
                            //}
                            //if (DataGlobals.SIZ_SIZE_ID != gridRow.NomBodyPartId)
                            //{
                            //    queryParams.Add("NomBodyPartSize.Id", (int)gridRow.SizeId);
                            //}
                            //else
                            //{
                            //    queryParams.Add("[]NomBodyPartSize.Id", 0);
                            //}
                            queryParams.Add("Wear", "100");

                            Storage storageTo = null;

                            IList<Storage> storages = storageRepository.GetByLikeCriteria(queryParams);
                            foreach (var item in storages)
                            {
                                storageTo = item;
                            }
                            if (storageTo == null)
                            {
                                storageTo = new Storage();
                                if (DataGlobals.SIZ_SIZE_ID != gridRow.NomBodyPartId)
                                {
                                    storageTo.NomBodyPartSize = nomBodyPartSizeRepository.Get((int)gridRow.SizeId);
                                }
                                storageTo.Nomenclature = currentNomenclature;
                                //                storageTo.Price = Price;
                                if (gridRow.GrowthId > -1)
                                {
                                    storageTo.Growth = nomBodyPartSizeRepository.Get((int)gridRow.GrowthId);
                                }
                                storageTo.Wear = "100";
                                storageTo.StorageName = currentStorageName;
                            }
                            Operation operation = new Operation();
                            //switch (currentMoveType)
                            //{
                            //    case CallRFC.MoveType.StorageIn:
                            //        operation.OperType = operTypeRepository.Get(DataGlobals.OPERATION_STORAGE_IN);
                            //        storageTo.Quantity = storageTo.Quantity + (int)gridRow.QUANTITY;
                            //        break;
                            //    case CallRFC.MoveType.StorageOut:
                            //        operation.OperType = operTypeRepository.Get(DataGlobals.OPERATION_STORAGE_OUT);
                            //        storageTo.Quantity = storageTo.Quantity - (int)gridRow.QUANTITY;
                            //        if (storageTo.Quantity < 0)
                            //        {
                            //            throw new Exception("На складе не хватает номенклатуры:[" + storageTo.Nomenclature.ExternalCode + "] " + storageTo.Nomenclature.Name);
                            //        }
                            //        break;
                            //}

                            operation.OperType = operTypeRepository.Get(DataGlobals.OPERATION_STORAGE_IN);
                            storageTo.Price = ((storageTo.Price * storageTo.Quantity) + ((int)gridRow.Price * (int)gridRow.QUANTITY)) / (storageTo.Quantity + (int)gridRow.QUANTITY);
                            storageTo.Quantity = storageTo.Quantity + (int)gridRow.QUANTITY;
                            
                            storageRepository.SaveOrUpdate(storageTo);
                            operation.Quantity = (int)gridRow.QUANTITY;
                            
                            operation.DocType = docTypeRepository.Get(gridRow.DocTypeId);
                            operation.DocNumber = gridRow.DocNumber;
                            operation.DocDate = DateTime.ParseExact(gridRow.DocDate, DataGlobals.DATE_FORMAT_FULL_YEAR, culture);
//                            operation.OperDate = DateTime.Now;
                            operation.OperDate = operation.DocDate.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddSeconds(DateTime.Now.Second);
                            operation.Organization = currentOrganization;
                            operation.Wear = "100";
                            operation.Storage = storageTo;
                            operation.StorageName = currentStorageName;
                            operationRepository.SaveOrUpdate(operation);
                        }
                    //}
//                    transaction.Commit();
                }

            }
            return new JsonResult();
        }

        [HttpPost]
        public ActionResult _GetNomGroups(string text)
        {
           Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("Name", text);
            IList<NomGroup> nomens = nomGroupRepository.GetByLikeCriteria(queryParams);
            Dictionary<string, string> list = new Dictionary<string, string>();
            foreach (var item in nomens)
            {
                list.Add(item.NomBodyPart.Id.ToString() + "|" + item.NomBodyPart.Name + "|" + item.IsWinter + "|" + item.Id.ToString(), item.Name);
            };
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new SelectList(list, "Key", "Value")
            };
        }


        [GridAction]
        [Transaction]
        public ActionResult _SaveIncomingData(int storageId, int StorIdIncoming, int docTypeId, string docNumber, string docDate, IEnumerable<NomenclatureSimple> inserted, IEnumerable<NomenclatureSimple> updated, IEnumerable<NomenclatureSimple> deleted)
        {
            IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
            Organization currentOrganization = organizationRepository.Get(int.Parse(getCurrentEnterpriseId()));
            StorageName currentStorageName = storageNameRepository.Get(storageId);
            HttpResponseBase Response = ControllerContext.HttpContext.Response;
            Response.ContentType = "text/html";

            string format = "dd.MM.yyyy";
            if (updated != null)
            {
                operationRepository.DbContext.BeginTransaction();
                try
                {
                    foreach (var gridRow in updated)
                    {
                        gridRow.Wear = gridRow.WearId;
                        if (gridRow.Wear == null)
                        {
                            gridRow.Wear = "100";
                        }

                        if (gridRow.Wear == "")
                        {
                            gridRow.Wear = "100";
                        }

                        Dictionary<string, object> queryParams = new Dictionary<string, object>();
                        Dictionary<string, object> orderParams = new Dictionary<string, object>();

                        queryParams.Add("StorageName", currentStorageName);
                        queryParams.Add("Nomenclature.Id", gridRow.NameId);
                        //if (Growth > -1)
                        //{
                        //    queryParams.Add("Growth.Id", Growth);
                        //}
                        //else
                        //{
                        //    queryParams.Add("Growth", null);
                        //}
                        //if (SizeId > 0)
                        //{
                        //    queryParams.Add("NomBodyPartSize.Id", SizeId);
                        //}
                        //else
                        //{
                        //    queryParams.Add("NomBodyPartSize", null);
                        //}
                        queryParams.Add("Wear", gridRow.Wear);
                        //            queryParams.Add("Price", Price);
                        orderParams.Add("Id", ASC);

                        IList<Storage> storagesTo = storageRepository.GetByLikeCriteria(queryParams, orderParams);
                        Storage storageTo = null;
                        foreach (var storage in storagesTo)
                        {
                            storageTo = storage;
                            if (storage.Quantity > 0)
                                break;
                        }
                        
                        if (storageTo == null)
                        {
                            storageTo = new Storage();
                            if (gridRow.SizeId == null)
                            {
                                storageTo.NomBodyPartSize = null;
                            }
                            else
                            {
                                storageTo.NomBodyPartSize = nomBodyPartSizeRepository.Get((int)gridRow.SizeId);
                            }
                            storageTo.Nomenclature = nomenRepository.Get(gridRow.NameId);
                            //storageTo.Price = Price;
                            if (gridRow.GrowthId > 0)
                            {
                                storageTo.Growth = nomBodyPartSizeRepository.Get((int)gridRow.GrowthId);
                            }
                            storageTo.Wear = gridRow.Wear;
                            storageTo.StorageName = storageNameRepository.Get(storageId);
                        }
                        storageTo.Quantity = storageTo.Quantity + gridRow.Quantity;
                        storageRepository.SaveOrUpdate(storageTo);
                        Operation operation = new Operation();
                        operation.Quantity = gridRow.Quantity;
                        operation.OperType = operTypeRepository.Get(DataGlobals.OPERATION_STORAGE_IN);
                        operation.OperDate = DateTime.ParseExact(docDate, format, culture);
                        operation.DocType = docTypeRepository.Get(docTypeId);
                        operation.DocNumber = docNumber;
                        operation.DocDate = operation.OperDate;
                        operation.Organization = organizationRepository.Get(storageTo.StorageName.Organization.RootOrganization);
                        operation.Storage = storageTo;
                        operation.StorageName = storageTo.StorageName;
                        operation.Partner = storageNameRepository.Get(StorIdIncoming);
                        operationRepository.SaveOrUpdate(operation);
                    }
                    operationRepository.DbContext.CommitTransaction();
                }
                catch (Exception e)
                {
                    operationRepository.DbContext.RollbackTransaction();
                    Response.Write("При сохранении данных произошла ошибка:");
                    Response.Write("//n");
                    Response.Write(e.ToString());
                }
            }
            return null; ;
        }


    }
}
