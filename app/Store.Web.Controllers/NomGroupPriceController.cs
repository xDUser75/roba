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
    public class NomGroupPriceController : ViewedController
    {
        private readonly NomGroupPriceRepository nomGroupPriceRepository;
        private readonly CriteriaRepository<PeriodPrice> periodPriceRepository;
        private readonly NomGroupRepository nomGroupRepository;
        private readonly OrganizationRepository organizationRepository;

        public NomGroupPriceController(
                                        NomGroupPriceRepository nomGroupPriceRepository,
                                        CriteriaRepository<PeriodPrice> periodPriceRepository,
                                        NomGroupRepository nomGroupRepository,
                                        OrganizationRepository organizationRepository
                                      )
        {
            Check.Require(nomGroupPriceRepository != null, "nomGroupPriceRepository may not be null");
            this.nomGroupPriceRepository = nomGroupPriceRepository;
            this.periodPriceRepository = periodPriceRepository;
            this.nomGroupRepository = nomGroupRepository;
            this.organizationRepository = organizationRepository;
        }


        [Transaction]
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_NOM_GROUP_PRICE_UPLOAD))]
        public ActionResult Index()
        {
            IList<PeriodPrice> storageNames = periodPriceRepository.GetAll();
            SelectList storageNameList = new SelectList(storageNames, "Id", "Name");
            ViewData[DataGlobals.REFERENCE_PERIOD_PRICE] = storageNameList;
            return View(viewName);
        }

        [GridAction]
        public ActionResult _GetNomGroupPrice(int priceId, int? nomGropId) {
            Dictionary<string, object> query = new Dictionary<string, object>();
            query.Add("OrganizationId", int.Parse(getCurrentEnterpriseId()));
            query.Add("PeriodPrice.Id", priceId);
            if (nomGropId.HasValue) {
                if (nomGropId > 0)
                {
                    query.Add("NomGroup.Id", nomGropId);
                }
            }
            IList<NomGroupPrice> list =  nomGroupPriceRepository.GetByLikeCriteria(query);
            IList<NomGroupPrice> model = new List<NomGroupPrice>();
            foreach (var item in list)
            {
                NomGroupPrice ng = rebuildNomGroupPrice(item);
                model.Add(ng);
            };

            return View(new GridModel(model));
        }

        [HttpPost]
        public ActionResult _GetNomGroup(string text)
        {
            /*
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("Organization.Id", int.Parse(getCurrentEnterpriseId()));
            int code = -1;
            if (int.TryParse(text, out code))
                queryParams.Add("ExternalCode", text);
            else
                queryParams.Add("Name", text);

            IList<NomGroup> nomGroups = nomGroupRepository.GetByLikeCriteria(queryParams);
            */
            IList<NomGroup> nomGroups = nomGroupRepository.GetNomGroup(text, getCurrentEnterpriseId());

            Dictionary<string, string> list = new Dictionary<string, string>();
            foreach (var item in nomGroups)
            {
                list.Add(item.Id.ToString(), item.ExternalCode + " - " + item.Name);
            }

            return new JsonResult
            {
                Data = new SelectList(list, "Key", "Value")
            };
        }

        [Transaction]
        [AcceptVerbs(HttpVerbs.Post)]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NOM_GROUP_PRICE_UPLOAD))]
        public ActionResult Save(int selectType, int PriceLoadList, IEnumerable<HttpPostedFileBase> attachments)
        {
            HttpResponseBase response = ControllerContext.HttpContext.Response;
            response.ContentType = "text/html";
            response.Write("<html><body>");
            if (attachments == null)
            {
                response.Write("Файл для загрузки не выбран!");
            }
            else
            {
                int idOrg = int.Parse(getCurrentEnterpriseId());
                Organization currentOrg = organizationRepository.Get(idOrg);
                PeriodPrice periodPrice = periodPriceRepository.Get(PriceLoadList);
                if (selectType == 1) nomGroupPriceRepository.TruncNomGroupPrice(idOrg, PriceLoadList);
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
                            response.Write("Загрузка данных производится из листа '" + workSheetNames.Trim(new[] { '$' }) + "'<br/>");
                            response.Flush();
                            if (!table.Columns.Contains("Цена"))
                            {
                                response.Write("Файл содержит не коррекные данные ('Цена').<br/>");
                                response.Flush();
                                isError = true;
                            }
                            if (!table.Columns.Contains("Код группы"))
                            {
                                response.Write("Файл содержит не коррекные данные ('Код группы').<br/>");
                                response.Flush();
                                isError = true;
                            }

                            int colPrice = table.Columns.IndexOf("Цена");
                            int colExternalCode = table.Columns.IndexOf("Код группы");

                            if (!isError)
                            {
                                DataRow[] result = table.Select();
                                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                                foreach (DataRow row in result) // Loop over the rows.
                                {
                                    if (row[colExternalCode] != System.DBNull.Value)
                                    {
                                        string rowExternalCode = Convert.ToString(row[colExternalCode]);
                                        try
                                        {
                                            string rowPriceString = Convert.ToString(row[colPrice]);
                                            double rowPrice = Convert.ToDouble(rowPriceString);
                                            queryParams.Clear();
                                            queryParams.Add("Organization", currentOrg);
                                            queryParams.Add("ExternalCode", rowExternalCode);
                                            NomGroup ng = nomGroupRepository.FindOne(queryParams);
                                            if (ng == null)
                                            {
                                                response.Write("В справочнике не найдена группа номенклатур с кодом " + rowExternalCode + ".<br/>");
                                                response.Flush();
                                            }
                                            else
                                            {
                                                NomGroupPrice nPrice = null;
                                                queryParams.Clear();
                                                queryParams.Add("OrganizationId", currentOrg.Id);
                                                queryParams.Add("NomGroup", ng);
                                                queryParams.Add("PeriodPrice", periodPrice);
                                                nPrice = nomGroupPriceRepository.FindOne(queryParams);
                                                if (nPrice == null)
                                                {
                                                    nPrice = new NomGroupPrice();
                                                    nPrice.NomGroup = ng;
                                                    nPrice.OrganizationId = currentOrg.Id;
                                                    nPrice.PeriodPrice = periodPrice;
                                                    nPrice.ExternalCode = ng.ExternalCode;
                                                }

                                                nPrice.Price = rowPrice;
                                                nomGroupPriceRepository.DbContext.BeginTransaction();
                                                try
                                                {
                                                    nomGroupPriceRepository.SaveOrUpdate(nPrice);
                                                    nomGroupPriceRepository.DbContext.CommitTransaction();
                                                }
                                                catch (Exception e)
                                                {
                                                    nomGroupPriceRepository.DbContext.RollbackTransaction();
                                                    response.Write("Ошибка при сохранении данных в БД:<br/>");
                                                    response.Write(e.Message);
                                                    response.Write("<br/>");
                                                    if (e.InnerException != null)
                                                    {
                                                        response.Write(e.InnerException.Message);
                                                        response.Write("<br/>");
                                                    }
                                                    response.Flush();
                                                }
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            response.Write("Ошибка определения цены для номклатуры " + rowExternalCode + "<br/>");
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
            Response.Write("</html></body>");
            Response.Flush();
            return null; ;
        }

    }
}
