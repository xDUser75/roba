using System.Web.Mvc;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using SharpArch.Core.PersistenceSupport;
using Store.Core;
using Telerik.Web.Mvc;
using System.Collections.Generic;
using Store.Data;
using Store.Core.RepositoryInterfaces;

namespace Store.Web.Controllers
{
    [HandleError]
    public class NormasController : ViewedController
    {
        private readonly CriteriaRepository<Norma> normaRepository;
        private readonly CriteriaRepository<NormaContent> normaContentRepository;
        private readonly OrganizationRepository organizationRepository;
        private readonly CriteriaRepository<NomGroup> nomGroupRepository;
        private readonly CriteriaRepository<NormaOrganization> normaOrganizationRepository;
        private readonly CriteriaRepository<NormaOrganizationSimple> normaOrganizationSimpleRepository;
        private readonly CriteriaRepository<NormaNomGroup> normaNomGroupRepository;
        private readonly CriteriaRepository<WorkerCardHead> workerCardHeadRepository;
        private readonly CriteriaRepository<WorkerCardContent> workerCardContentRepository;
        private readonly CriteriaRepository<WorkerWorkplace> workerWorkplaceRepository;

        public NormasController(CriteriaRepository<Norma> normaRepository,
            CriteriaRepository<NormaContent> normaContentRepository,
            OrganizationRepository organizationRepository,
            CriteriaRepository<NomGroup> nomGroupRepository,
            CriteriaRepository<NormaOrganization> normaOrganizationRepository,
            CriteriaRepository<NormaOrganizationSimple> normaOrganizationSimpleRepository,
            CriteriaRepository<NormaNomGroup> normaNomGroupRepository,
            CriteriaRepository<WorkerCardHead> workerCardHeadRepository,
            CriteriaRepository<WorkerCardContent> workerCardContentRepository,
            CriteriaRepository<WorkerWorkplace> workerWorkplaceRepository)
        {
            Check.Require(normaRepository != null, "normaRepository may not be null");
            this.normaRepository = normaRepository;
            this.normaContentRepository = normaContentRepository;
            this.organizationRepository = organizationRepository;
            this.nomGroupRepository = nomGroupRepository;
            this.normaOrganizationRepository = normaOrganizationRepository;
            this.normaOrganizationSimpleRepository = normaOrganizationSimpleRepository;
            this.normaNomGroupRepository = normaNomGroupRepository;
            this.workerCardHeadRepository = workerCardHeadRepository;
            this.workerCardContentRepository = workerCardContentRepository;
            this.workerWorkplaceRepository = workerWorkplaceRepository;

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
                Data = new SelectList(shops, "ShopNumber", "ShopInfo")
            };
        }

        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_NORMA_EDIT + ", " + DataGlobals.ROLE_NORMA_VIEW + ", " + DataGlobals.ROLE_NORMA_APPROVED ))]
        public ActionResult Index()
        {
            return View(viewName);

        }

        [HttpPost]
        public ActionResult _GetNomGroupsActive(string text)
        {
            return _GetNomGroups(text, true);
        }

        [HttpPost]
        public ActionResult _GetNomGroups(string text, bool? isActive)
        {
            string idOrg = getCurrentEnterpriseId();
            int idEnterprise = System.Int32.Parse(idOrg);

            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("Organization.Id", idEnterprise);
            queryParams.Add("Name", text);
            if ((isActive.HasValue)&&(isActive.Value==true))
                queryParams.Add("IsActive", true);

            IList<NomGroup> nomGroups = nomGroupRepository.GetByLikeCriteria(queryParams);

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new SelectList(nomGroups, "Id", "Name")
            };
        }

        
// SELECT
        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_NORMA_EDIT + ", " + DataGlobals.ROLE_NORMA_VIEW + ", " + DataGlobals.ROLE_NORMA_APPROVED ))]
        public ActionResult _SelectionClientSide_Normas(string shopNumber, string shopInfo)
        {

            Session.Remove("NormaId");
            Session.Remove("IsApproved");
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            if (shopNumber == null)
            {
                if (Session["shopNumber"] != null)
                    shopNumber = Session["shopNumber"].ToString();
            }


            string idOrg = getCurrentEnterpriseId();
            int idOrganization = System.Int32.Parse(idOrg);
            List<Norma> model = new List<Norma>();
            if (idOrg != null && idOrg.Length > 0)
            {
                if (shopNumber != null)
                {
                    Session.Add("shopNumber", shopNumber);
                    Session.Add("shopInfo", shopInfo);
                    queryParams.Add("Name", shopNumber);
                }
                queryParams.Add("Organization.Id", idOrganization);
                queryParams.Add("IsActive", true);
                Dictionary<string, object> orderParams = new Dictionary<string, object>();
                orderParams.Add("Name", ASC);
                IEnumerable<Norma> normas = normaRepository.GetByLikeCriteria(queryParams, orderParams);
                foreach (var item in normas)
                {
                    Norma norma = rebuildNorma(item);
                    model.Add(norma);
                };


                return View(viewName, new GridModel<Norma>
                {
                    Data = model
                });
            }

            return View(viewName, new GridModel<Norma>
            {
                Data = model
            });
        }

        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_NORMA_EDIT + ", " + DataGlobals.ROLE_NORMA_VIEW + ", " + DataGlobals.ROLE_NORMA_APPROVED))]
        public ActionResult _Selection_Normas(string Id)
        {
                List<Norma> model = new List<Norma>();
                if ((Id != null) && (Id != "") )
                {
                    Norma norma = normaRepository.Get(int.Parse(Id));
                    Norma newNorma = rebuildNorma(norma);
                    model.Add(newNorma);
                }

                return View(viewName, new GridModel<Norma>
                {
                    Data = model
                });
        }


        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_NORMA_EDIT + ", " + DataGlobals.ROLE_NORMA_VIEW + ", " + DataGlobals.ROLE_NORMA_APPROVED ))]
        public ActionResult _SelectionClientSide_NormaContents(string id)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            List<NormaContent> model = new List<NormaContent>();

            int idNorma = 0;
            if (id == null)
            {
                if (Session["NormaId"] != null)
                    idNorma = int.Parse(Session["NormaId"].ToString());
            }
            else
                idNorma = int.Parse(id);
            if (idNorma != 0)
            {
                Norma norma = normaRepository.Get(idNorma);
                Session.Add("NormaId", idNorma);
                queryParams.Add("NormaId", idNorma);
                queryParams.Add("IsActive", true);
                Dictionary<string, object> orderParams = new Dictionary<string, object>();
                orderParams.Add("NomGroup.IsWinter", ASC);
                orderParams.Add("NomGroup.NomBodyPart.Id", ASC);
                orderParams.Add("NomGroup.Name", ASC);
                IEnumerable<NormaContent> normaContents = normaContentRepository.GetByLikeCriteria(queryParams, orderParams);
                foreach (var item in normaContents)
                {
                    NormaContent nc = rebuildNormaContent(item);
                    nc.IsApproved = norma.IsApproved;

                    model.Add(nc);
                }
            }

            return View(new GridModel<NormaContent>
            {
                Data = model
            });
        }

        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_NORMA_EDIT + ", " + DataGlobals.ROLE_NORMA_VIEW + ", " + DataGlobals.ROLE_NORMA_APPROVED + ", "+DataGlobals.ROLE_WORKER_CARD_EDIT))]
        public ActionResult _Selection_NormaNomGroups(string NormaContentId)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            List<NormaNomGroup> model = new List<NormaNomGroup>();
            int idNormaContent = 0;
            if (NormaContentId == null)
            {
                if (Session["NormaId"] != null)
                {
                    if (Session["NormaContentId"] != null)
                        idNormaContent = int.Parse(Session["NormaContentId"].ToString());
                }
            }
            else
                idNormaContent = int.Parse(NormaContentId);
            if (idNormaContent != 0)
            {
                Session.Add("NormaContentId", idNormaContent);
                queryParams.Add("NormaContent.Id", idNormaContent);
                Dictionary<string, object> orderParams = new Dictionary<string, object>();
                orderParams.Add("NomGroup.Name", ASC); 

                IEnumerable<NormaNomGroup> normaNomGroups = normaNomGroupRepository.GetByLikeCriteria(queryParams, orderParams);
                foreach (var item in normaNomGroups)
                {
                    if (item.IsBase != true)
                    {
                        NormaNomGroup nng = rebuildNormaNomGroup(item);
                        model.Add(nng);
                    }
                }
            }

            return View(new GridModel<NormaNomGroup>
            {
                Data = model
            });
        }


        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_NORMA_EDIT + ", " + DataGlobals.ROLE_NORMA_VIEW + ", " + DataGlobals.ROLE_NORMA_APPROVED ))]
        public ActionResult _Selection_NormaOrganizations(string Id)
        {
            List<NormaOrganizationSimple> model = new List<NormaOrganizationSimple>();
            int idNorma = 0;
            if (Id == null)
            {
                if (Session["NormaId"] != null)
                    idNorma = int.Parse(Session["NormaId"].ToString());
            }
            else
                idNorma = int.Parse(Id);
            if (idNorma != 0)
            {
                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                queryParams.Add("Norma.Id", idNorma);
                queryParams.Add("IsActive", true);
                Dictionary<string, object> orderParams = new Dictionary<string, object>();
                orderParams.Add("id", DESC);
                IEnumerable<NormaOrganizationSimple> workplaces = normaOrganizationSimpleRepository.GetByLikeCriteria(queryParams, orderParams);
                foreach (var item in workplaces)
                {
                    NormaOrganizationSimple no = rebuildNormaOrganizationSimple(item);
                    model.Add(no);
                }
            }

            return View("Normas",new GridModel<NormaOrganizationSimple>
            {
                Data = model
            });

        }

        private ActionResult getAllAndView()
        {
            IList<Norma> normas = normaRepository.GetAll();
            return View(new GridModel(normas));
        }


// Insert
        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NORMA_EDIT + ", " + DataGlobals.ROLE_NORMA_APPROVED ))]
        public ActionResult Insert()
        {
            string idOrg = getCurrentEnterpriseId();
            string shopNumber = Session["shopNumber"].ToString();
            
            int idOrganization = System.Int32.Parse(idOrg);
            Organization organization = organizationRepository.Get(idOrganization);
            Norma norma = new Norma();
            if (TryUpdateModel<Norma>(norma, null, null, new string[] { "Organization" }))
            {
                norma.Organization = organization;
                norma.IsActive = true;
                normaRepository.SaveOrUpdate(norma);
            }
            return _SelectionClientSide_Normas(shopNumber, Session["shopInfo"].ToString());
        }

        public void ValidateContent(int NormaId, NormaContent normaContent, bool isControlNomgroup)
        {
            if (isControlNomgroup == true)
            {
                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                queryParams.Add("NormaId", NormaId);
                queryParams.Add("IsActive", true);
                IList<NormaContent> normaContents = normaContentRepository.FindAll(queryParams);
                foreach (var item in normaContents)
                {
                    foreach (var nng in item.NormaNomGroups)
                    {
                        if (nng.NomGroup.Id == normaContent.NomGroup.Id)
                            ModelState.AddModelError("NomGroup", "Группа  " + normaContent.NomGroup.Name + " уже добавлена в норму");

                    }

                }
            }
            if (normaContent.Quantity == 0)
                ModelState.AddModelError("Quantity", "Количество не может быть 0");

            if (normaContent.InShop == false && normaContent.UsePeriod == 0)
                ModelState.AddModelError("UsePeriod ", "Период использования для СИЗ, выдаваемых в салоне, не должен быть 0");

        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NORMA_EDIT + ", " + DataGlobals.ROLE_NORMA_APPROVED ))]
        public ActionResult InsertContent()
        {
            int NormaId = int.Parse(Session["NormaId"].ToString());
            NormaContent normaContent = new NormaContent();
            IList<NormaNomGroup> normaNomGroups = new List<NormaNomGroup>();

            TryUpdateModel(normaContent);
            {
                
                ValidateContent(NormaId, normaContent , true);

                if (ModelState.IsValid)
                {
                    normaContent.NormaId = NormaId;
                    NormaNomGroup normaNomGroup = new NormaNomGroup();
                    normaNomGroup.NomGroup = normaContent.NomGroup;
                    normaNomGroup.IsBase = true;
                    normaNomGroup.NormaContent = normaContent;
                    normaNomGroups.Add(normaNomGroup);
                    normaContent.NormaNomGroups = normaNomGroups;
                    normaContent.IsActive = true;
                    
                    
                    normaContentRepository.SaveOrUpdate(normaContent);
                    ChangeContentIns(normaNomGroup);
                   
                }
            }

            return _SelectionClientSide_NormaContents(NormaId.ToString());
        }

        public void ChangeContentIns(NormaNomGroup normaNomgroup)
        {
                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                queryParams.Add("NormaId", normaNomgroup.NormaContent.NormaId);
//                queryParams.Add("IsActive", false);
//                queryParams.Add("InShop", false);
                IList<NormaContent> normaContents = normaContentRepository.FindAll(queryParams);
                foreach (var item in normaContents)
                {
                            queryParams.Clear();
                            queryParams.Add("NormaContent.Id", item.Id);
                            IList<WorkerCardContent> workerCardContents = workerCardContentRepository.FindAll(queryParams);
                            foreach (var wcc in workerCardContents)
                            {
                                if (wcc.Storage.Nomenclature.NomGroup.Id == normaNomgroup.NomGroup.Id)
                                {
                                    wcc.NormaContent = normaNomgroup.NormaContent;
                                    workerCardContentRepository.SaveOrUpdate(wcc);
                                }
                            }


                }

        }

        public void ChangeContentDel(NormaContent normaContent)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("NormaId", normaContent.NormaId);
            queryParams.Add("IsActive", true);
            IList<NormaContent> normaContents = normaContentRepository.FindAll(queryParams);
            foreach (var item in normaContents)
            {
                foreach (var nng in item.NormaNomGroups)
                {
                        queryParams.Clear();
                        queryParams.Add("NormaContent.Id", normaContent.Id);
                        IList<WorkerCardContent> workerCardContents = workerCardContentRepository.FindAll(queryParams);
                        foreach (var wcc in workerCardContents)
                        {
                            if (wcc.Storage.Nomenclature.NomGroup.Id == nng.NomGroup.Id)
                            {
                                wcc.NormaContent = nng.NormaContent;
                                workerCardContentRepository.SaveOrUpdate(wcc);
                            }
                        }

                    }
            }

        }

        public void ChangeNorma(NormaOrganizationSimple normaOrganization)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("Organization.Id", normaOrganization.OrganizationId);
            queryParams.Add("IsActive", true);
            IList<WorkerWorkplace> wwps = workerWorkplaceRepository.FindAll(queryParams);
            foreach (var item in wwps)
            {
                queryParams.Clear();
                queryParams.Add("WorkerWorkplace.Id", item.Id);
                IList<WorkerCardHead> workerCardHeads = workerCardHeadRepository.FindAll(queryParams);
                foreach (var wch in workerCardHeads)
                {
                    foreach (var wcc in wch.getActiveWorkerCardContent())
                    {
                        foreach (var nc in normaOrganization.Norma.NormaContents)
                        {
                            if (nc.IsActive == true)
                            {
                                foreach (var nng in nc.NormaNomGroups)
                                {
                                    if (wcc.Storage.Nomenclature.NomGroup.Id == nng.NomGroup.Id)
                                    {
                                        wcc.NormaContent = nng.NormaContent;
                                        workerCardContentRepository.SaveOrUpdate(wcc);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                }

            }
        }


        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NORMA_EDIT + ", " + DataGlobals.ROLE_NORMA_APPROVED ))]
        public ActionResult InsertNormaOrganization(string OrgString, string NormaId4Tree)
        {
            int NormaId = 0;
            bool flag;
            if (NormaId4Tree != null)
                NormaId = int.Parse(NormaId4Tree);
            string s = Request.Params["OrgString"];
            Norma norma = normaRepository.Get(NormaId);
            string[] t;
            if (OrgString != null)
            {
                t = OrgString.Split(',');
                for (int i = 0; i < t.Length; i++)
                {
                    flag = true;
                    if (t[i] != null && t[i] != "")
                    {
                        NormaOrganizationSimple no = new NormaOrganizationSimple();
                        no.Norma = norma;
                        no.OrganizationId = int.Parse(t[i]);
                        no.IsActive = true;
                        TryUpdateModel(no);
                        {
                            Dictionary<string, object> queryParams = new Dictionary<string, object>();
                            queryParams.Add("Organization.Id", no.OrganizationId);
                            queryParams.Add("IsActive", true);
                            IList<NormaOrganization> existNormaOrganizations = normaOrganizationRepository.GetByLikeCriteria(queryParams);
                            if (existNormaOrganizations.Count > 0)
                            {
                                ModelState.AddModelError("Norma", organizationRepository.Get(no.OrganizationId).Name + " добавлен в норму " + existNormaOrganizations[0].Norma.Name);
                                flag = false;
                            }

                            if (flag)
                            {
                                normaOrganizationSimpleRepository.SaveOrUpdate(no);
                                ChangeNorma(no);
                            }
                        }
                    }
                }
            }
           // return View(viewName);
            return _Selection_NormaOrganizations(NormaId4Tree);
        }

        public void ValidateNormaNomGroup(int NormaContentId, NormaNomGroup normaNomGroup)
        {

            NormaContent normaContent = normaContentRepository.Get(NormaContentId);
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("NormaId", normaContent.NormaId);
            queryParams.Add("IsActive", true);
            IList<NormaContent> normaContents = normaContentRepository.FindAll(queryParams);
            foreach (var nc in normaContents)
            {
                queryParams.Clear();
                queryParams.Add("NormaContent", nc);
                IList<NormaNomGroup> normaNomGroups = normaNomGroupRepository.FindAll(queryParams);
                foreach (var nng in normaNomGroups)
                {
                    if (nng.NomGroup.Id == normaNomGroup.NomGroup.Id)
                        ModelState.AddModelError(nng.NomGroup.Id.ToString(), "Группа  " + normaNomGroup.NomGroup.Name + " уже добавлена в норму");

                }
            }
            
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NORMA_EDIT + ", " + DataGlobals.ROLE_NORMA_APPROVED + ", " + DataGlobals.ROLE_WORKER_CARD_EDIT))]
        public ActionResult InsertNormaNomGroup()
        {
            string NormaContentId = null;
            int NormaId = 0;
            if (Session["NormaContentId"] != null)
                NormaContentId = Session["NormaContentId"].ToString();
            if (Session["NormaId"] != null)
                NormaId = int.Parse(Session["NormaId"].ToString());
            NormaNomGroup normaNomGroup = new NormaNomGroup();
            if (TryUpdateModel<NormaNomGroup>(normaNomGroup))
            {
                ValidateNormaNomGroup(int.Parse(NormaContentId), normaNomGroup);

                    normaNomGroup.IsBase = false;
                    normaNomGroup.NormaContent = normaContentRepository.Get(int.Parse(NormaContentId));
                    if (ModelState.IsValid)
                    {
                        normaNomGroupRepository.SaveOrUpdate(normaNomGroup);
                        ChangeContentIns(normaNomGroup);
                    }
            }
            return _Selection_NormaNomGroups(NormaContentId);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NORMA_EDIT + ", " + DataGlobals.ROLE_NORMA_APPROVED ))]
        public ActionResult Copy(string Id)
        {

            Norma norma = normaRepository.Get(int.Parse(Id));
            Norma newNorma = new Norma();
            newNorma.Organization = norma.Organization;
            newNorma.Name = norma.Name + " (Копия)";
            newNorma.IsActive = norma.IsActive;
            newNorma.NormaComment = norma.NormaComment;
            normaRepository.SaveOrUpdate(newNorma);

            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("NormaId", int.Parse(Id));
            queryParams.Add("IsActive", true);
            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            orderParams.Add("Id", ASC);
            IEnumerable<NormaContent> normaContents = normaContentRepository.GetByCriteria(queryParams, orderParams);

            foreach (var item in normaContents)
            {
                NormaContent nc = new NormaContent();
                nc.Quantity = item.Quantity;
                nc.QuantityTON = item.QuantityTON;
                nc.UsePeriod = item.UsePeriod;
                nc.IsActive = item.IsActive;
                nc.NormaId = newNorma.Id;
                nc.NomGroup = item.NomGroup;
                nc.InShop = item.InShop;
                normaContentRepository.SaveOrUpdate(nc);
                foreach (var nng in item.NormaNomGroups)
                {
                    NomGroup nomGroup = new NomGroup(nng.NomGroup.Id, nng.NomGroup.Name);
                    NomBodyPart nb = new NomBodyPart(nng.NomGroup.NomBodyPart.Id, nng.NomGroup.NomBodyPart.Name);
                    Organization o = rebuildOrganization(nng.NomGroup.Organization);
                    nomGroup.Organization = o;
                    nomGroup.NomBodyPart = nb;
                    NormaNomGroup ng = new NormaNomGroup();
                    ng.NomGroup = nomGroup;
                    ng.IsBase = nng.IsBase;
                    ng.NormaContent = nc;

                    normaNomGroupRepository.SaveOrUpdate(ng);
                }

            };
            return View(viewName);
//            return _SelectionClientSide_Normas();
        }
        
// Save

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NORMA_EDIT + ", " + DataGlobals.ROLE_NORMA_APPROVED ))]
        public ActionResult Save(string Id)
        {
            string idOrg = getCurrentEnterpriseId();
            string shopNumber = Session["shopNumber"].ToString();

            int idOrganization = System.Int32.Parse(idOrg);
            Organization organization = organizationRepository.Get(idOrganization);
            //Norma norma = new Norma();
            Norma norma = normaRepository.Get(int.Parse(Id));

//            if (TryUpdateModel<Norma>(norma, null, null, new string[] { "Organization" }))
            if (TryUpdateModel<Norma>(norma))

            {
                norma.Organization = organization;
                normaRepository.SaveOrUpdate(norma);
            }

            return _SelectionClientSide_Normas(shopNumber, Session["shopInfo"].ToString());
        }
        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NORMA_APPROVED))]
        public ActionResult ApprovedNorma()
        {
            int NormaId = int.Parse(Session["NormaId"].ToString());

            Norma norma = normaRepository.Get(NormaId);
                norma.IsApproved = true;
                normaRepository.SaveOrUpdate(norma);
                return null;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NORMA_APPROVED))]
        public ActionResult UnApprovedNorma()
        {
            int NormaId = int.Parse(Session["NormaId"].ToString());

            Norma norma = normaRepository.Get(NormaId);
            norma.IsApproved = false;
            normaRepository.SaveOrUpdate(norma);
            return null;
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NORMA_EDIT + ", " + DataGlobals.ROLE_NORMA_APPROVED ))]
        public ActionResult SaveContent(string Id)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("Id", int.Parse(Id));
            NormaContent newNormaContent = normaContentRepository.FindOne(queryParams);
            IList<NormaNomGroup> newNormaNomGroups = new List<NormaNomGroup>();
            NormaContent normaContent = new NormaContent();
            newNormaNomGroups = newNormaContent.NormaNomGroups;
            if (TryUpdateModel<NormaContent>(normaContent, null, new string[] { "NomGroup", "Quantity", "UsePeriod", "InShop", "IsActive", "QuantityTON" }, new string[] { "NormaNomGroups" }))
            {

                if (newNormaContent.NomGroup.Id == normaContent.NomGroup.Id) 
                    ValidateContent(newNormaContent.NormaId, normaContent, false);
                else 
                    ValidateContent(newNormaContent.NormaId, normaContent, true);


                if (ModelState.IsValid)
                {
                    foreach (NormaNomGroup nng in newNormaNomGroups)
                    {
                        if (nng.IsBase)
                            newNormaNomGroups[newNormaNomGroups.IndexOf(nng)].NomGroup = normaContent.NomGroup;
                    }
                    newNormaContent.NomGroup = normaContent.NomGroup;
                    newNormaContent.Quantity = normaContent.Quantity;
                    newNormaContent.UsePeriod = normaContent.UsePeriod;
                    newNormaContent.IsActive = normaContent.IsActive;
                    newNormaContent.InShop = normaContent.InShop;
                    newNormaContent.QuantityTON = normaContent.QuantityTON;

                    normaContentRepository.SaveOrUpdate(newNormaContent);
                    //ChangeContentIns(newNormaContent.NomGroup);


                }
            }
            return _SelectionClientSide_NormaContents(newNormaContent.NormaId.ToString());
        }

        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NORMA_EDIT + ", " + DataGlobals.ROLE_NORMA_APPROVED + ", " + DataGlobals.ROLE_WORKER_CARD_EDIT))]
        public ActionResult SaveNormaNomGroup(string id)
        {
            string NormaContentId = null;
            if (Session["NormaContentId"] != null)
                NormaContentId = Session["NormaContentId"].ToString();
//            NormaNomGroup normaNomGroup = new NormaNomGroup();
            NormaNomGroup newNormaNomGroup = normaNomGroupRepository.Get(int.Parse(id));
            NormaNomGroup normaNomGroup = new NormaNomGroup();
            normaNomGroup.NomGroup = newNormaNomGroup.NomGroup;
            NormaContent normaContent = normaContentRepository.Get(int.Parse(NormaContentId));


            if (TryUpdateModel<NormaNomGroup>(normaNomGroup, null, new string[] { "NomGroup" }, new string[] { "NormaContent" }))

            {
                if (normaNomGroup.NomGroup.Id != newNormaNomGroup.NomGroup.Id) 
                    ValidateNormaNomGroup(normaContent.Id, normaNomGroup);

                if (ModelState.IsValid)
                {
                    newNormaNomGroup.NormaContent = normaContent;
                    newNormaNomGroup.NomGroup = normaNomGroup.NomGroup;
                    normaNomGroupRepository.SaveOrUpdate(newNormaNomGroup);
                    ChangeContentIns(newNormaNomGroup);
                }
            }

            return _Selection_NormaNomGroups(NormaContentId);
        }


//Delete
        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NORMA_EDIT + ", " + DataGlobals.ROLE_NORMA_APPROVED ))]
        public ActionResult Delete()
        {
            string idOrg = getCurrentEnterpriseId();
            string shopNumber = Session["shopNumber"].ToString();

            int idOrganization = System.Int32.Parse(idOrg);
            Organization organization = organizationRepository.Get(idOrganization);
            IList<WorkerCardContent> workerCardContents=null;
            Norma norma = new Norma();
            if (TryUpdateModel<Norma>(norma))
            {
                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                norma = normaRepository.Get(norma.Id);

                queryParams.Clear();
                queryParams.Add("Norma.Id", norma.Id);
                string flag = "";
                IEnumerable<NormaOrganization> workplaces = normaOrganizationRepository.FindAll(queryParams);
                foreach (var workplace in workplaces)
                {
                    normaOrganizationRepository.Delete(workplace);
                }
                NormaContent normaContent=null;

                    for (int i = 0; i < norma.NormaContents.Count; i++)
                    {
                        normaContent = norma.NormaContents[i];
                        queryParams.Clear();
                        queryParams.Add("NormaContent", normaContent);
                        workerCardContents = workerCardContentRepository.GetByLikeCriteria(queryParams);
                        if (workerCardContents.Count > 0)
                        {
                            flag = "1";
                            normaContent.IsActive = false;
                        }
                        else
                        {
                            norma.NormaContents.Remove(normaContent);
                            normaContentRepository.Delete(normaContent);
                            i--;
                        }
                    }
                    if (flag == "1")
                    {
                        //normaContents = newNormaContents;
                        //norma.NormaContents = normaContents;
                        norma.Organization = organization;
                        norma.IsActive = false;
                        normaRepository.SaveOrUpdate(norma);
                    }
                    else
                        normaRepository.Delete(norma);

            }
            //HttpContext.Cache.Remove("NormaId");
            Session.Remove("NormaId");
            return _SelectionClientSide_Normas(shopNumber, Session["shopInfo"].ToString());
        }

        [GridAction]
        [AcceptVerbs(HttpVerbs.Post)]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NORMA_EDIT + ", " + DataGlobals.ROLE_NORMA_APPROVED ))]
        public ActionResult DeleteContent(int Id)
        {
            NormaContent normaContent = normaContentRepository.Get(Id);
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("NormaContent", normaContent);
            IList<WorkerCardContent> workerCardContents = workerCardContentRepository.FindAll(queryParams);
            if (workerCardContents.Count == 0)
                normaContentRepository.Delete(normaContent);
            else {
                normaContent.IsActive = false;
                normaContentRepository.SaveOrUpdate(normaContent);
            }

            ChangeContentDel(normaContent);
            return _SelectionClientSide_NormaContents(normaContent.NormaId.ToString());
        }


        //[GridAction]
        //[AcceptVerbs(HttpVerbs.Post)]
        //[Transaction]
        //[Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NORMA_EDIT))]
        //public ActionResult DeleteNormaOrganization(NormaOrganization normaOrganization)
        //{
        //    normaOrganization = normaOrganizationRepository.Get(normaOrganization.Id);
        //    normaOrganizationRepository.Delete(normaOrganization);
        //    return _Selection_NormaOrganizations(normaOrganization.Norma.Id.ToString());
        //}


        [GridAction]
        [AcceptVerbs(HttpVerbs.Post)]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NORMA_EDIT + ", " + DataGlobals.ROLE_NORMA_APPROVED ))]
        public ActionResult DeleteNormaOrganization(NormaOrganizationSimple normaOrganization)
        {
            NormaOrganization newNormaOrganization = normaOrganizationRepository.Get(normaOrganization.Id);
            normaOrganizationRepository.Delete(newNormaOrganization);
            return _Selection_NormaOrganizations(newNormaOrganization.Norma.Id.ToString());
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NORMA_EDIT + ", " + DataGlobals.ROLE_NORMA_APPROVED))]
        public void DeleteNormaFromWorkerWorkplace(int normId, int workerWorkPlaceId)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("Norma.Id", normId);
            queryParams.Add("Organization.Id", workerWorkPlaceId);
            NormaOrganization normaOrganization = null;
            IList<NormaOrganization> NormaOrganizations = normaOrganizationRepository.GetByLikeCriteria(queryParams);
            if (NormaOrganizations.Count > 0) {
                normaOrganization = NormaOrganizations[0];            
            }
            if (normaOrganization != null) {
                normaOrganizationRepository.Delete(normaOrganization);
            }
        }

        [GridAction]
        [AcceptVerbs(HttpVerbs.Post)]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_NORMA_EDIT + ", " + DataGlobals.ROLE_NORMA_APPROVED + ", " + DataGlobals.ROLE_WORKER_CARD_EDIT))]
        public ActionResult DeleteNormaNomGroup(int Id)
        {
            NormaNomGroup normaNomGroup = normaNomGroupRepository.Get(Id);
            string NormaContentId = null;
            //if (HttpContext.Cache.Get("NormaContentId") != null)
            //    NormaContentId = HttpContext.Cache.Get("NormaContentId").ToString();
            if (Session["NormaContentId"] != null)
                NormaContentId = Session["NormaContentId"].ToString();
            normaNomGroupRepository.Delete(normaNomGroup);
            return _Selection_NormaNomGroups(NormaContentId);
        }



    }
}
