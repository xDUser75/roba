using System.Web.Mvc;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using SharpArch.Core.PersistenceSupport;
using Store.Core;
using Telerik.Web.Mvc;
using System.Collections.Generic;
using Store.Data;
using System;
using System.Data;

namespace Store.Web.Controllers
{
    [HandleError]
    public class ExportPlanController : ViewedController
    {
        private readonly PLAN_SAPRepository planSAPRepository;
        private readonly OrganizationRepository organizationRepository;

        public ExportPlanController(PLAN_SAPRepository planSAPRepository,
                                OrganizationRepository organizationRepository)
        {
            Check.Require(planSAPRepository != null, "PlanSAPRepository may not be null");
            this.planSAPRepository = planSAPRepository;
            this.organizationRepository = organizationRepository;
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN))]
        public ActionResult Index()
        {
            return View(viewName);
        }

        [HttpPost]
        public ActionResult _GetShops()
        {
            string idOrg = getCurrentEnterpriseId();
            int idEnterprise = System.Int32.Parse(idOrg);

            IList<Organization> shops = organizationRepository.GetShopsByEnterprise(idEnterprise);

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new SelectList(shops, "Id", "ShopInfo")
            };
        }



        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN))]
        public ActionResult _Select(string DateN,string DateEnd,int CexId)
        {
            Response.ClearHeaders();
            Response.AddHeader("content-disposition", string.Format("attachment; filename={0}", "PlanWorkerCloth.csv"));
            Response.ClearContent();
            Response.ContentEncoding = System.Text.Encoding.GetEncoding(866);
            Response.ContentType = "application/octet-stream";
//            Response.Write("dt;groupid;groupname;growth;nomid;nomname;plandata;quantitynorma;sex;sexid;shopname;shopnumber;sizenumber;storagename;storagenameid;unit;unit_eng;\n");

            DateTime dtN=DateTime.Parse(DateN);
            DateTime dtK = DateTime.Parse(DateEnd);
            string date_plan = planSAPRepository.CreateTempData(int.Parse(getCurrentEnterpriseId()), dtN, dtK, CexId, Response);
            //if ((date_plan.Length > 0) && (date_plan != "null"))
            //{
            //    Dictionary<string, object> query = new Dictionary<string, object>();
            //    query.Add("dt", date_plan);
            //    IList<PLAN_SAP> rows = planSAPRepository.GetByCriteria(query);
            //    foreach (var item in rows)
            //    {
            //        Response.Write(item.dt);
            //        Response.Write(";");
            //        Response.Write(item.groupid);
            //        Response.Write(";");
            //        Response.Write(item.groupname);
            //        Response.Write(";");
            //        Response.Write(item.growth);
            //        Response.Write(";");
            //        Response.Write(item.nomid);
            //        Response.Write(";");
            //        Response.Write(item.nomname);
            //        Response.Write(";");
            //        Response.Write(item.plandata);
            //        Response.Write(";");
            //        Response.Write(item.quantitynorma);
            //        Response.Write(";");
            //        Response.Write(item.sex);
            //        Response.Write(";");
            //        Response.Write(item.sexid);
            //        Response.Write(";");
            //        Response.Write(item.shopname);
            //        Response.Write(";");
            //        Response.Write(item.shopnumber);
            //        Response.Write(";");
            //        Response.Write(item.sizenumber);
            //        Response.Write(";");
            //        Response.Write(item.storagename);
            //        Response.Write(";");
            //        Response.Write(item.storagenameid);
            //        Response.Write(";");
            //        Response.Write(item.unit);
            //        Response.Write(";");
            //        Response.Write(item.unit_eng);
            //        Response.Write(";\n");
            //    }
            if ((date_plan.Length > 0) && (date_plan != "null"))
            {
                planSAPRepository.ClearTempData(date_plan);
            }
            //}
            Response.Flush();
            return null;
        }
    }
}
