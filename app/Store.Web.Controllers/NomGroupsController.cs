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
    public class NomGroupsController : ViewedController
    {
        private readonly CriteriaRepository<NomBodyPart> nomBodyPartRepository;
        private readonly CriteriaRepository<Organization> organizationRepository;
        private readonly CriteriaRepository<NomGroup> nomGroupRepository;
        private readonly CriteriaRepository<Nomenclature> nomenclatureRepository;

        public NomGroupsController(CriteriaRepository<NomGroup> nomGroupRepository,
                                       CriteriaRepository<NomBodyPart> nomBodyPartRepository,
                                       CriteriaRepository<Organization> organizationRepository,
                                       CriteriaRepository<Nomenclature> nomenclatureRepository
                                        )
        {
            Check.Require(nomGroupRepository != null, "nomGroupRepository may not be null");
            this.nomBodyPartRepository = nomBodyPartRepository;
            this.organizationRepository = organizationRepository;
            this.nomGroupRepository = nomGroupRepository;
            this.nomenclatureRepository = nomenclatureRepository;
        }

        private void getSessionSettings(){
            //Установить организацию
            //TempData["Organization.Id"] = 50004352; //ЗСМК
            Session["Organization.Id"] = this.getCurrentEnterpriseId();
        }

        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " 
            + DataGlobals.ROLE_NOMGROUP_EDIT + ", " + DataGlobals.ROLE_NOMGROUP_VIEW + ","
            + DataGlobals.ROLE_NOMENCLATURE_EDIT + ", " + DataGlobals.ROLE_NOMENCLATURE_VIEW))]
        public ActionResult Index()
        {
            //Заполним все списки
            getSessionSettings();
            //Запросим части тела для списка
            IList<NomBodyPart> nomBodyParts = nomBodyPartRepository.GetAll();

            IList<NomBodyPart> newNomBodyParts = new List<NomBodyPart>();
            foreach (var nomBodyPart in nomBodyParts)
            {
                if (nomBodyPart.Id != DataGlobals.GROWTH_SIZE_ID)
                    newNomBodyParts.Add(nomBodyPart);
            };
             ViewData[DataGlobals.REFERENCE_NOM_BODY_PART] =  newNomBodyParts;
            return View(viewName);
        }
        
        # region Grid NomGroup Action

        [GridAction]
        public ActionResult NomGroup_Select()
        {
            //Получим настройки
            getSessionSettings();
            IList<NomGroup> model = new List<NomGroup>();
            Dictionary<string, object> queryparams = new Dictionary<string, object>();
            //queryparams.Add("Organization.Id",Session["Organization.Id"]);
            queryparams.Add("Organization.Id", int.Parse(this.getCurrentEnterpriseId()));
            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            orderParams.Add("Name", ASC);
            IEnumerable<NomGroup> nomgroup = nomGroupRepository.GetByCriteria(queryparams, orderParams);
            foreach (var item in nomgroup)
            {

                //NomBodyPart nb = null;
                //Organization org = null;
                //if (item.NomBodyPart != null)
                //    nb = new NomBodyPart(item.NomBodyPart.Id, item.NomBodyPart.Name);
                //if (item.Organization != null)
                //    org = new Organization(item.Organization.Id, item.Organization.Name);
                //NomGroup ng = new NomGroup(item.Id,item.Name, org, nb, item.IsActive,item.ExternalCode);
                //ng.IsWinter = item.IsWinter;
                
                NomGroup ng = rebuildNomGroup(item);
                model.Add(ng);
            };
             return View(new GridModel(model));
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NOMGROUP_EDIT))]
        public ActionResult NomGroup_Insert()
        {
            NomGroup nomGroup = new NomGroup();
            if (TryUpdateModel<NomGroup>(nomGroup, null, null, new[] { "Organization" }))
            {
                //Заполним поля по умолчанию
                //Organization org = organizationRepository.Get((int)Session["Organization.Id"]);
                Organization org = organizationRepository.Get(int.Parse(this.getCurrentEnterpriseId()));
                nomGroup.Organization = new Organization(org.Id, org.Name);
                nomGroupRepository.SaveOrUpdate(nomGroup);
            }
            return NomGroup_Select();
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NOMGROUP_EDIT))]
        public ActionResult NomGroup_Update(string id)
        {
            NomGroup nomGroup = nomGroupRepository.Get(int.Parse(id));
            if (TryUpdateModel<NomGroup>(nomGroup, null, null, new[] { "Organization" }))
            {
                nomGroupRepository.SaveOrUpdate(nomGroup);
            }
            return NomGroup_Select();
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NOMGROUP_EDIT))]
        public ActionResult NomGroup_Delete(string id)
        {
            NomGroup nomGroup = nomGroupRepository.Get(int.Parse(id));
            nomGroupRepository.Delete(nomGroup);
            return NomGroup_Select();
        }
        
        # endregion

        # region Grid Nomeclature Action

        [GridAction]
        public ActionResult Nomenclature_Select(string groupID)
        {
            getSessionSettings();
            groupID = groupID ?? "-1";
            Session["NomGroup.Id"] = groupID;
            IList<Nomenclature> model = new List<Nomenclature>();
            NomGroup group = nomGroupRepository.Get(int.Parse(groupID));
            if (group != null)
            {
                foreach (Nomenclature item in group.Nomenclatures)
                {
                     model.Add(rebuildNomenclature(item));
                };
            }

            return View(new GridModel(model));
        }


        # endregion

    }

}

