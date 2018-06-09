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
    public class UploadFilesController : ViewedController
    {
        private readonly StorageNameRepository storageNameRepository;
        private readonly RemainExternalRepository remaindExternalRepository;
        private readonly CriteriaRepository<Nomenclature> nomenclatureRepository;
        public UploadFilesController(RemainExternalRepository remaindExternalRepository,
                                    StorageNameRepository storageNameRepository,
                                    CriteriaRepository<Nomenclature> nomenclatureRepository)
        {
            Check.Require(remaindExternalRepository != null, "remaindExternalRepository may not be null");
            this.remaindExternalRepository = remaindExternalRepository;
            this.storageNameRepository = storageNameRepository;
            this.nomenclatureRepository = nomenclatureRepository;
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
            int.TryParse((string)Session["row_count"], out messRowCount);
            Session.Remove("row_count");
            if (messRowCount > 0) {
                for (int i = 0; i < messRowCount; i++) {
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
        public ActionResult Save(string date, int StorageList, IEnumerable<HttpPostedFileBase> attachments)
        {
            int messCount = 0;
            if (attachments == null)
            {
                Session.Add("messRow"+messCount.ToString(),"Файл для загрузки не выбран");
                messCount++;
            }
            else
            {                
                remaindExternalRepository.DbContext.BeginTransaction();
                // The Name of the Upload component is "attachments" 
                IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
                string format = "dd.MM.yyyy";
                DateTime paramDate = DateTime.ParseExact(date, format, culture);
                StorageName storageName = storageNameRepository.Get(StorageList);
                remaindExternalRepository.TruncRemainExternal(StorageList, date);
                foreach (var file in attachments)
                {
                    // Some browsers send file names with full path. This needs to be stripped.
                    var isError = false;
                    var fileName = Path.GetFileName(file.FileName);
                    Session.Add("messRow"+messCount.ToString(),"Обрабатывается файл " + fileName);
                    messCount++;
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
                            Session.Add("messRow"+messCount.ToString(),"Ошибка при охранении файла на сервере:");
                            messCount++;
                            Session.Add("messRow"+messCount.ToString(),e.Message);
                            messCount++;
                            isError = true;
                        }
                        System.Data.DataTable table = null;
                        if (!isError)
                        {
                            try
                            {
                                ExcelReader excelReader = new ExcelReader(physicalFilePath);
                                table = excelReader.GetWorksheet(excelReader.workSheetNames[0]);
                            }
                            catch (Exception e)
                            {
                                Session.Add("messRow"+messCount.ToString(),"Ошибка при открытии файла:");
                                messCount++;
                                Session.Add("messRow"+messCount.ToString(),e.Message);
                                messCount++;
                                isError = true;
                            }
                        }
                        if (table != null)
                        {
                            try
                            {
                                DataRow[] result = table.Select("F2 = '*' and F3="+storageName.Plant+" and F4 in (" + storageName.StorageNumber + ")");
                                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                                foreach (DataRow row in result) // Loop over the rows.
                                {
                                    if (row[4] != System.DBNull.Value)
                                    {
                                        queryParams.Clear();
                                        queryParams.Add("Organization", storageName.Organization);
                                        queryParams.Add("ExternalCode", (string)row[4]);
                                        Nomenclature nomenclature = nomenclatureRepository.FindOne(queryParams);
                                        RemaindExternal remExt = new RemaindExternal();
                                        remExt.ActualDate = DateTime.Now;
                                        remExt.RemaindDate = paramDate;
                                        remExt.StorageName = storageName;
                                        remExt.Nomenclature = nomenclature;
                                        remExt.Wear = 100;
                                        String wear=(string)row[4];
                                        if ((wear.ToUpper(System.Globalization.CultureInfo.CurrentCulture) == "БУ")||
                                            (wear.ToUpper(System.Globalization.CultureInfo.CurrentCulture) == "Б/У")){
                                            remExt.Wear = 50;
                                        }
                                        remExt.ExternalCode = (string)row[4];
                                        int cnt = 0;
                                        if (row[11] != System.DBNull.Value)
                                            int.TryParse((string)row[11], out cnt);
                                        remExt.QuantityIn = cnt;
                                        cnt = 0;
                                        if (row[12] != System.DBNull.Value)
                                            int.TryParse((string)row[12], out cnt);
                                        remExt.QuantityOut = cnt;
                                        cnt = 0;
                                        if (row[13] != System.DBNull.Value)
                                            int.TryParse((string)row[13], out cnt);
                                        remExt.Quantity = cnt;
                                        remaindExternalRepository.SaveOrUpdate(remExt);
                                    }
                                }
                                Session.Add("messRow"+messCount.ToString(),"Обработано " + result.Length + " записей.");
                                messCount++;
                            }
                            catch (Exception e)
                            {
                                Session.Add("messRow"+messCount.ToString(),"Ошибка сохранении данных в БД:");
                                messCount++;
                                Session.Add("messRow"+messCount.ToString(),e.Message);
                                messCount++;
                                isError = true;
                            }

                        }
                        if (!isError)
                        {
                            remaindExternalRepository.DbContext.CommitTransaction();
                            Session.Add("messRow"+messCount.ToString(),"Файл успешно обработан");
                            messCount++;
                        }
                        else
                        {
                            remaindExternalRepository.DbContext.RollbackTransaction();
                        }
                    }
                    finally
                    {
                        System.IO.File.Delete(physicalFilePath);
                    }
                }             
            }
            Session.Add("row_count", messCount.ToString());
            return RedirectToAction("Index");
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_REMAINS_EXTERNAL_UPLOAD + ", " + DataGlobals.ROLE_REMAINS_EXTERNAL_VIEW))]
        public ActionResult _SelectRemains(string idStorage, string date)
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
                    List<RemaindExternal> model = new List<RemaindExternal>();
                    int id = System.Int32.Parse(idStorage);
                    queryParams.Add("StorageName.Id", id);
                    DateTime dt = DateTime.ParseExact(date, DataGlobals.DATE_FORMAT_FULL_YEAR, null);
                    queryParams.Add("RemaindDate", dt);
                    Dictionary<string, object> orderParams = new Dictionary<string, object>();
                    orderParams.Add("Nomenclature.Name", ASC);
                    IEnumerable<RemaindExternal> remainds = remaindExternalRepository.GetByCriteria(queryParams, orderParams);
                    List<string> excludeProperty = new List<string>();
                    excludeProperty.Add("Storage");
                    excludeProperty.Add("StorageName");
                    excludeProperty.Add("NomBodyPartSize.NomBodyPart");
                    excludeProperty.Add("Growth.NomBodyPart");
                    excludeProperty.Add("Nomenclature.NomBodyPart");
                    excludeProperty.Add("Nomenclature.Organization");
                    excludeProperty.Add("Nomenclature.Unit");
                    excludeProperty.Add("Nomenclature.Sex");
                    excludeProperty.Add("Nomenclature.NomBodyPart");
                    excludeProperty.Add("Nomenclature.NomGroup");
                    foreach (var item in remainds)
                    {
                        model.Add(rebuildRemaindExternal(item, excludeProperty));
                    };
                    return View(new GridModel(model));
                }
            }
            return View(viewName);
        }

    }
}
