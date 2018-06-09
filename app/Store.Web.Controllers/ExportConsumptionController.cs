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
using Store.Core.Utils;
using Store.Core.External.Interfaсe;

namespace Store.Web.Controllers
{
    [HandleError]
    public class ExportConsumptionController : ViewedController
    {
        private readonly PLAN_SAPRepository planSAPRepository;
        private readonly OrganizationRepository organizationRepository;

        public ExportConsumptionController(PLAN_SAPRepository planSAPRepository,
                                OrganizationRepository organizationRepository)
        {
            Check.Require(planSAPRepository != null, "PlanSAPRepository may not be null");
            this.planSAPRepository = planSAPRepository;
            this.organizationRepository = organizationRepository;
        }


        public ActionResult Index()
        {
            return View(viewName);
        }


        [Transaction]
        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_EXPORT_DATA))]
        [HttpPost]
        public ActionResult _Export(int paramOrganization, string dateN,string  dateEnd,int ceh,int operTypeId, int paramStorage,string paramUchastokId, int? paramSplit, int? paramTabN, string nameNakl)
        {
            IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
            DateTime dtN = DateTime.ParseExact(dateN, DataGlobals.DATE_FORMAT_FULL_YEAR, culture);
            DateTime dtK = DateTime.ParseExact(dateEnd, DataGlobals.DATE_FORMAT_FULL_YEAR, culture);
            string assemblyName = ApplicationConfig.ReadVariable("/Configuration/Organization[@id=" + paramOrganization + "]/InterfaceLoadInvoice/AssemblyName");
            string className = ApplicationConfig.ReadVariable("/Configuration/Organization[@id=" + paramOrganization + "]/InterfaceLoadInvoice/ClassName");
            IExportConsumption loader = (IExportConsumption)Store.Core.Utils.Reflection.LoadClassObject(assemblyName, className);
            if (loader != null)
            {
                string result = loader.ExportConsumption(paramOrganization, dtN, dtK, ceh, operTypeId, paramStorage, paramUchastokId, paramSplit, paramTabN, nameNakl, null, 0);
                if (result != null)
                {
                    ModelState.AddModelError("", result);
                }
            }
            else
            {
                ModelState.AddModelError("", "Сборка с интерфейсом IExportConsumption не найдена. Обратитесь к разработчикам.");
            }
            return View(new GridModel(new List<Object>()));
        }
    }
}
