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
    public class TestRegisterController : ViewedController
    {
        private readonly CriteriaRepository<TestRegister> testRegisterRepository;
        private readonly CriteriaRepository<TestRegisterSimple> testRegisterSimpleRepository;
        private readonly OrganizationRepository organizationRepository;
        private readonly CriteriaRepository<Result> resultRepository;
        private readonly CriteriaRepository<Provaider> provaiderRepository;
        private readonly CriteriaRepository<Certificate> certificateRepository;
        private readonly CriteriaRepository<NomGroup> nomGroupRepository;


        public TestRegisterController(CriteriaRepository<TestRegister> testRegisterRepository,
                            CriteriaRepository<TestRegisterSimple> testRegisterSimpleRepository,
                            OrganizationRepository organizationRepository,
                            CriteriaRepository<Result> resultRepository,
                            CriteriaRepository<Provaider> provaiderRepository,
                            CriteriaRepository<Certificate> certificateRepository,
                            CriteriaRepository<NomGroup> nomGroupRepository)
        {
            Check.Require(testRegisterRepository != null, "testRegisterRepository may not be null");
            this.testRegisterRepository = testRegisterRepository;
            this.testRegisterSimpleRepository = testRegisterSimpleRepository;
            this.organizationRepository = organizationRepository;
            this.resultRepository = resultRepository;
            this.provaiderRepository = provaiderRepository;
            this.certificateRepository = certificateRepository;
            this.nomGroupRepository = nomGroupRepository;
        }

        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_TEST_REGISTER_VIEW + ", " + DataGlobals.ROLE_TEST_REGISTER_EDIT))]
        public ActionResult Index()
        {
            ViewData[DataGlobals.REFERENCE_RESULT] = resultRepository.GetAll();            
            return View(viewName);
        }

        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_TEST_REGISTER_EDIT))]
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

        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_TEST_REGISTER_EDIT + ", " + DataGlobals.ROLE_TEST_REGISTER_VIEW + ", " + DataGlobals.ROLE_VIEW_ALL))]
        public ActionResult _SelectCerticicates(int id)
        {
            IList<Certificate> list = new List<Certificate>();
            if (id > 0)
            {
                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                Dictionary<string, object> orderParams = new Dictionary<string, object>();
                TestRegister testRegister = testRegisterRepository.Get(id);
                IList<Certificate> certificates = null;
                queryParams.Add("TestRegister", testRegister);
                orderParams.Add("DocDate", ASC);
                certificates = certificateRepository.GetByLikeCriteria(queryParams, orderParams);
                foreach (var item in certificates)
                {
                    Certificate cert = rebuildCertificate(item);
                    list.Add(cert);
                }
            }
            return View(new GridModel(list));
        }

        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_TEST_REGISTER_EDIT + ", " + DataGlobals.ROLE_TEST_REGISTER_VIEW + ", " + DataGlobals.ROLE_VIEW_ALL))]
        public ActionResult _SelectTestRegister()
        {
            IList<TestRegisterSimple> list = new List<TestRegisterSimple>();
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            queryParams.Add("OrganizationId", getIntCurrentEnterpriseId());
            orderParams.Add("TestDate", ASC);
            list = testRegisterSimpleRepository.GetByLikeCriteria(queryParams, orderParams);
            return View(new GridModel(list));
        }

        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_TEST_REGISTER_EDIT))]
        public ActionResult _addOrEditTestRegister(int id, string testDate, int provaiderId, int nomGroupId,string model)
        {
            TestRegister testReg = testRegisterRepository.Get(id);
            //Если вставляется новая запись, то пытаемся найти запись с выбранным цехом
            if (testReg == null)
            {
                testReg = new TestRegister();
                testReg.OrganizationId = getIntCurrentEnterpriseId();
            }
            testReg.TestDate = System.DateTime.Parse(testDate);
            testReg.Provaider = provaiderRepository.Get(provaiderId);
            testReg.NomGroup = nomGroupRepository.Get(nomGroupId);
            testReg.Model = model;
            // сохраняем изменения
            testRegisterRepository.SaveOrUpdate(testReg);
            return null;
        }

        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_TEST_REGISTER_EDIT))]
        public ActionResult _addOrEditCertificat(int id,int pid, int orgId, string DocNum, string DocDate, int ResultId, string Descr)
        {
            Certificate cert = certificateRepository.Get(id);
            //Если вставляется новая запись, то пытаемся найти запись с выбранным цехом
            if (cert == null)
            {
                cert = new Certificate();
                cert.TestRegister = testRegisterRepository.Get(pid);
            }
            // сохраняем изменения
            if (cert.TestRegister == null) {
                ModelState.AddModelError("", "Не найдена запись по тестированию номенклатуры!");
            }
            else
            {
                cert.DocNum = DocNum;
                cert.DocDate = System.DateTime.Parse(DocDate);
                cert.Descr = Descr;
                cert.Result = resultRepository.Get(ResultId);
                cert.Organization = organizationRepository.Get(orgId);
                certificateRepository.SaveOrUpdate(cert);
            }
            return null;
        }

        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_TEST_REGISTER_EDIT))]
        public ActionResult _GetProviders(string text)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("Name",text);
            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            orderParams.Add("Name", ASC);
            IList<Provaider> provaiders = provaiderRepository.GetByLikeCriteria(queryParams, orderParams);
            return new JsonResult
            {
                Data = new SelectList(provaiders, "Id", "ProvaiderInfo")
            };
        }


        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_TEST_REGISTER_EDIT))]
        public ActionResult _GetNomGroups(string text)
        {
            Organization currentOrg = organizationRepository.Get(getIntCurrentEnterpriseId());
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("Organization", currentOrg);
            queryParams.Add("Name", text);
            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            orderParams.Add("Name", ASC);
            IList<NomGroup> provaiders = nomGroupRepository.GetByLikeCriteria(queryParams, orderParams);
            return new JsonResult
            {
                Data = new SelectList(provaiders, "Id", "Name")
            };
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_TEST_REGISTER_EDIT))]
        public ActionResult _Provaider_Insert()
        {
            Provaider provaider = new Provaider();
            if (TryUpdateModel<Provaider>(provaider))
            {
                provaider.OrganizationId = getIntCurrentEnterpriseId();
                provaiderRepository.SaveOrUpdate(provaider);
            }
            return _Provaider_Select();
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_TEST_REGISTER_EDIT))]
        public ActionResult _Provaider_Update(string id)
        {
            Provaider provaider = provaiderRepository.Get(int.Parse(id));
            if (TryUpdateModel<Provaider>(provaider))
            {
                provaiderRepository.SaveOrUpdate(provaider);
            }
            return _Provaider_Select();
        }

        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_TEST_REGISTER_EDIT))]
        public ActionResult _Provaider_Select()
        {            
            Dictionary<string, object> queryparams = new Dictionary<string, object>();
            queryparams.Add("OrganizationId", getIntCurrentEnterpriseId());
            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            orderParams.Add("Name", ASC);
            IList<Provaider> model = provaiderRepository.GetByCriteria(queryparams, orderParams);
            return View(new GridModel(model));
        }

        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_TEST_REGISTER_EDIT))]
        public ActionResult _delTestRegisterRecord(TestRegister testRegister)
        {
            IList<Certificate> certificates = null;
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("TestRegister", testRegister);
            certificates = certificateRepository.GetByLikeCriteria(queryParams);
            foreach (var item in certificates)
            {
                certificateRepository.Delete(item);
            }
            testRegisterRepository.Delete(testRegister);
            return null;
        }

        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_TEST_REGISTER_EDIT))]
        public ActionResult _delCertificateRecord(Certificate certificate)
        {
            certificateRepository.Delete(certificate);
            return null;
        }
    }
}
