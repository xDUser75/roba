using System;
using System.Web.Mvc;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using SharpArch.Core.PersistenceSupport;
using Store.Core;
using Telerik.Web.Mvc;
using System.Collections.Generic;
using Store.Data;
using Telerik.Web.Mvc.Extensions;
using Store.Core.RepositoryInterfaces;

namespace Store.Web.Controllers
{
    [HandleError]
    public class NomenclaturesController : ViewedController
    {
        private readonly CriteriaRepository<Organization> organizationRepository;
        private readonly CriteriaRepository<Nomenclature> nomenclatureRepository;
        private readonly CriteriaRepository<Unit> unitRepository;
        private readonly CriteriaRepository<Sex> sexRepository;
        private readonly CriteriaRepository<NomBodyPart> nombodypartRepository;
        private readonly CriteriaRepository<NomGroup> nomgroupRepository;
        private readonly CriteriaRepository<NomBodyPartSize> nombodypartsizeRepository;

        public NomenclaturesController(CriteriaRepository<Nomenclature> nomenclatureRepository,
                                       CriteriaRepository<Organization> organizationRepository,
                                       CriteriaRepository<Unit> unitRepository,
                                       CriteriaRepository<Sex> sexRepository,
                                       CriteriaRepository<NomBodyPart> nombodypartRepository,
                                       CriteriaRepository<NomBodyPartSize> nombodypartsizeRepository,
                                       CriteriaRepository<NomGroup> nomgroupRepository
                                        )
        {
            this.organizationRepository = organizationRepository;
            this.nomenclatureRepository = nomenclatureRepository;
            this.unitRepository = unitRepository;
            this.sexRepository = sexRepository;
            this.nombodypartRepository = nombodypartRepository;
            this.nomgroupRepository = nomgroupRepository;
            this.nombodypartsizeRepository = nombodypartsizeRepository;
        }

        private void getSessionSettings(){
            //Установить организацию
            //TempData["Organization.Id"] = 50004352; //ЗСМК
            //Session["Organization.Id"] = int.Parse(ViewData["_EnterpriceSelectValue"] as string);
            Session["Organization.Id"] = int.Parse(getCurrentEnterpriseId());
        }

        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_NOMENCLATURE_EDIT + ", " + DataGlobals.ROLE_NOMENCLATURE_VIEW))]
        public ActionResult Index()
        {
            //Заполним все списки
            getSessionSettings();
            //Запросим справочник секса ;)
            ViewData[DataGlobals.REFERENCE_SEX] = sexRepository.GetAll();
            IList<NomBodyPart> nomBodyParts = nombodypartRepository.GetAll();
            IList<NomBodyPart> newNomBodyParts = new List<NomBodyPart>();
            foreach (var nomBodyPart in nomBodyParts)
            {
                if (nomBodyPart.Id != DataGlobals.GROWTH_SIZE_ID)
                    newNomBodyParts.Add(nomBodyPart);
            };
            ViewData[DataGlobals.REFERENCE_NOM_BODY_PART] = newNomBodyParts;
            
            
            //Запросим список единиц измерения для списка по организации
            Dictionary<string,object> queryparams = new Dictionary<string, object>();
            queryparams.Add("Organization.Id", Session["Organization.Id"]);
            ViewData[DataGlobals.REFERENCE_UNIT] = unitRepository.GetByCriteria(queryparams);
            ViewData[DataGlobals.REFERENCE_NOMGROUPS] = nomgroupRepository.GetByCriteria(queryparams); ;
            queryparams.Clear();
            queryparams.Add("[!=]NomBodyPart.Id", DataGlobals.GROWTH_SIZE_ID);
            queryparams.Add("IsActive", true);
            ViewData[DataGlobals.REFERENCE_NOM_BODY_PART_SIZE] = nombodypartsizeRepository.GetByCriteria(queryparams);
            queryparams.Clear();
            queryparams.Add("NomBodyPart.Id", DataGlobals.GROWTH_SIZE_ID);
            queryparams.Add("IsActive", true);
            ViewData[DataGlobals.REFERENCE_NOM_BODY_PART_SIZE_GROWTH] = nombodypartsizeRepository.GetByCriteria(queryparams);
            return View(viewName);
        }

        [GridAction]
        public ActionResult Nomenclature_Select()
        {
            getSessionSettings();
            IList<Nomenclature> model = new List<Nomenclature>();
            Dictionary<string, object> queryparams = new Dictionary<string, object>();
            queryparams.Add("Organization.Id", Session["Organization.Id"]);
            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            orderParams.Add("Name", ASC);
            IEnumerable<Nomenclature> nom = nomenclatureRepository.GetByCriteria(queryparams, orderParams);
            foreach (Nomenclature item in nom)
            {

                    //Nomenclature n = new Nomenclature(item.Id);
                    //n.Name = item.Name;
                    //n.StartDate = item.StartDate;
                    //n.IsActive = item.IsActive;
                    //n.ExternalCode = item.ExternalCode;
                    //n.FinishDate = item.FinishDate;

                    //if (item.Organization != null)
                    //    n.Organization = new Organization(item.Organization.Id, item.Organization.Name);
                    //if (item.Unit != null)
                    //    n.Unit = new Unit(item.Unit.Id, item.Unit.Name);
                    //if (item.Sex != null)
                    //    n.Sex = new Sex(item.Sex.Id, item.Sex.Name);
                    //if (item.NomBodyPart != null)
                    //    n.NomBodyPart = new NomBodyPart(item.NomBodyPart.Id, item.NomBodyPart.Name);
                    //if (item.NomGroup != null)
                    //    n.NomGroup = new NomGroup(item.NomGroup.Id, item.NomGroup.Name);

                    model.Add(rebuildNomenclature(item));
            };

            return View(new GridModel(model));
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NOMENCLATURE_EDIT))]
        public ActionResult Nomenclature_Insert()
        {
            Nomenclature nomenclature = new Nomenclature();
            if (TryUpdateModel<Nomenclature>(nomenclature, null, null, new[] { "Organization" }))
            {

//                nomenclature.NomGroup = new NomGroup(int.Parse(groupID));
                nomenclature.Organization = new Organization((int)Session["Organization.Id"]);
                nomenclatureRepository.SaveOrUpdate(nomenclature);
            }
            return Nomenclature_Select();
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NOMENCLATURE_EDIT))]
        public ActionResult Nomenclature_Update(string id)
        {
            Nomenclature nomenclature = nomenclatureRepository.Get(int.Parse(id));
            if (TryUpdateModel<Nomenclature>(nomenclature, null, null, new[] { "Organization" }))
            {
                nomenclatureRepository.SaveOrUpdate(nomenclature);
            }
            return Nomenclature_Select();
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NOMENCLATURE_EDIT))]
        public ActionResult Nomenclature_Delete(string id)
        {
            Nomenclature obj = nomenclatureRepository.Get(int.Parse(id));
            nomenclatureRepository.Delete(obj);
            return Nomenclature_Select();
        }

    }
}
