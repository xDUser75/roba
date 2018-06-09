using System.Web.Mvc;
using SharpArch.Core;
using Store.Core;
using Telerik.Web.Mvc;
using System.Collections.Generic;
using Store.Data;
using SharpArch.Web.NHibernate;

namespace Store.Web.Controllers
{
    [HandleError]
    public class SubscriptionController : ViewedController
    {
        private readonly CriteriaRepository<Worker> workerRepository;
        private readonly CriteriaRepository<Subscription> subscriptionRepository;        
        private readonly OrganizationRepository organizationRepository;

        public SubscriptionController(CriteriaRepository<Worker> workerRepository,
                                 CriteriaRepository<Subscription> subscriptionRepository,
                                 OrganizationRepository organizationRepository)
        {
            Check.Require(subscriptionRepository != null, "subscriptionRepository may not be null");
            this.workerRepository = workerRepository;
            this.subscriptionRepository = subscriptionRepository;
            this.organizationRepository = organizationRepository;
        }


        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_SUBSCRIPTION_VIEW + ", " + DataGlobals.ROLE_SUBSCRIPTION_EDIT))]
        public ActionResult Index()
        {            
            return View(viewName);
        }

        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_SUBSCRIPTION_EDIT))]
        public ActionResult _GetShop(string text)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            Dictionary<string, object> orderParams = new Dictionary<string, object>();

            IList<Organization> shops = organizationRepository.GetActiveShops(getCurrentEnterpriseId(), text);
            return new JsonResult
            {
                Data = new SelectList(shops, "Id", "ShopInfo")
            };
        }

        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_SUBSCRIPTION_EDIT))]
        public ActionResult _GetWorker(string text)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            Dictionary<string, object> orderParams = new Dictionary<string, object>();

            IList<Worker> workers = null;
            int tabn = -1;
            if (int.TryParse(text, out tabn))
                queryParams.Add("TabN", tabn);
            else
                queryParams.Add("Fio", text);
            queryParams.Add("RootOrganization", getIntCurrentEnterpriseId());
            orderParams.Add("Fio", ASC);
            workers = workerRepository.GetByLikeCriteria(queryParams, orderParams);
            return new JsonResult
            {
                Data = new SelectList(workers, "Id", "WorkerInfo")
            };
        }

        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_SUBSCRIPTION_EDIT))]
        public ActionResult _addOrEditRecord(int id, int orgId, int? WorkerId1, int? WorkerId2, int? WorkerId3)
        {
            Subscription subscription = subscriptionRepository.Get(id);
            //Если вставляется новая запись, то пытаемся найти запись с выбранным цехом
            if (subscription == null) {
                Organization selectOrganization = organizationRepository.Get(orgId);
                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                queryParams.Add("Organization", selectOrganization);
                IList<Subscription> list = subscriptionRepository.GetByCriteria(queryParams);
                if (list.Count > 0)
                {
                    subscription = list[0];
                }
                else
                //Если записи не нашлось в базе, то создаем новую
                {
                    subscription = new Subscription();
                    subscription.Organization = selectOrganization;
                } 
            }
            if (WorkerId1.HasValue) subscription.Worker1 = workerRepository.Get(WorkerId1.Value);
            if (WorkerId2.HasValue) subscription.Worker2 = workerRepository.Get(WorkerId2.Value);
            if (WorkerId3.HasValue) subscription.Worker3 = workerRepository.Get(WorkerId3.Value);
            subscription.updateTabN();
            // сохраняем изменения
            subscriptionRepository.SaveOrUpdate(subscription);
            return null;
        }

        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_SUBSCRIPTION_EDIT))]
        public ActionResult _delRecord(int id) {
            Subscription subscription = subscriptionRepository.Get(id);
            subscriptionRepository.Delete(subscription);
            return null;
        }


        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_SUBSCRIPTION_VIEW + ", " + DataGlobals.ROLE_SUBSCRIPTION_EDIT))]
        public ActionResult _SelectSubscription(string param)
        {
            IList<Subscription> list = new List<Subscription>();

            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("Organization.OrgId", getIntCurrentEnterpriseId());

            //            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            //            orderParams.Add("Fio", ASC);

            IList<Subscription> subscriptions = subscriptionRepository.GetByLikeCriteria(queryParams/*, orderParams*/);
            List<string> excludeProperty = new List<string>();
            excludeProperty.Add("Worker1.Organization");
            excludeProperty.Add("Worker1.Sex");
            excludeProperty.Add("Worker1.nomBodyPartSizes");
            excludeProperty.Add("Worker1.WorkerCategory");
            excludeProperty.Add("Worker1.WorkerGroup");
            
            excludeProperty.Add("Worker2.Organization");
            excludeProperty.Add("Worker2.Sex");
            excludeProperty.Add("Worker2.nomBodyPartSizes");
            excludeProperty.Add("Worker2.WorkerCategory");
            excludeProperty.Add("Worker2.WorkerGroup");

            excludeProperty.Add("Worker3.Organization");
            excludeProperty.Add("Worker3.Sex");
            excludeProperty.Add("Worker3.nomBodyPartSizes");
            excludeProperty.Add("Worker3.WorkerCategory");
            excludeProperty.Add("Worker3.WorkerGroup");
            foreach (Subscription item in subscriptions)
            {
                Subscription newItem = rebuildSubscription(item, excludeProperty);
                list.Add(newItem);
            }
            return View(new GridModel(list));
        }
    }
}
