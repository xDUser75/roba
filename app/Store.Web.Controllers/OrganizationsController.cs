using System.Web.Mvc;
using System.Collections;
using System.Linq;
using System;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using SharpArch.Core.PersistenceSupport;
using Store.Core;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.UI;
using System.Collections.Generic;
using Store.Core.RepositoryInterfaces;
using Store.Data;




namespace Store.Web.Controllers
{
    [HandleError]
    public class OrganizationsController : ViewedController
    {
        private readonly OrganizationRepository organizationRepository;
        private readonly CriteriaRepository<Norma> normaRepository;

        public OrganizationsController(OrganizationRepository organizationRepository,
                                       CriteriaRepository<Norma> normaRepository)
        {
            Check.Require(organizationRepository != null, "organizationRepository may not be null");

            this.organizationRepository = organizationRepository;
            this.normaRepository = normaRepository;
        }

         
        public ActionResult ShowTree()
        {
            Organization organization = new Organization();
            string idOrg = getCurrentEnterpriseId();
            int org = 0;
            if (idOrg != null)
                org = int.Parse(idOrg);

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("Pid", org);
            parameters.Add("IsActive", true);
            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            orderParams.Add("Short", ASC);
            IList<Organization> organizations = organizationRepository.GetByLikeCriteria(parameters, orderParams);
            //organization = organizationRepository.Get(org);
            //IList<Organization> organizations= new List<Organization>();
            //    organizations.Add(organization);
                return View("TreeOrganizations", organizations);

        }
      

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxLoading(TreeViewItem node)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            int pid = int.Parse(node.Value);
            parameters.Add("Pid", pid);
            parameters.Add("IsActive", true);
            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            orderParams.Add("IsWorkPlace", DESC);
            orderParams.Add("Short", ASC);
            
            IList<Organization> organizations = organizationRepository.GetByLikeCriteria(parameters, orderParams);
            IList<TreeViewItem> nodes = new List<TreeViewItem>();
            /*
            int len = 0;
            foreach (Organization organization in organizations) 
            {
                if (organization.Name.Length > len)
                    len = organization.Name.Length;
            }
            */

            foreach (Organization organization in organizations)
            {
                TreeViewItem node1 = new TreeViewItem();
                TreeViewItem norma = null;
                //string s = new string('_', len + 1 - organization.Name.Length);
                
                node1.Checkable = false;
                node1.Template.Html= "<img src=../Content/Images/pawn_glass_red.gif />";
                node1.Text = organization.Short + (organization.Mvz != null ? " МВЗ: " + organization.Mvz : "") + " " + organization.Name;
                //+(organization.Mvz != null ? " (" + organization.MvzName + ")" : "");
                node1.Value = organization.Id.ToString();
                 node1.LoadOnDemand = organization.Childs.Count > 0;
                 if (organization.Id== organization.ShopId)
                     node1.Text = organization.ShopNumber + (organization.Mvz != null ? " МВЗ: " + organization.Mvz : "") + " " + organization.Name;
                //+(organization.Mvz != null ? " " + organization.MvzName + ")" : "");
                 if (organization.IsWorkPlace)
                 {
                     if (organization.NormaOrganization == null)
                     {
// его нет в реквесте
//                         if (Request.Params.Get("hideHeader") == "true")
                         node1.Checkable = true;
                         node1.ImageUrl = "../Content/Images/pawn_glass_red.gif";
                     }
                     else
                     {
                         node1.ImageUrl = "../Content/Images/pawn_glass_green.gif";
                         node1.Text = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + node1.Text;
                         norma = new TreeViewItem();
                         norma.ImageHtmlAttributes.Add("ID", "img" + organization.Id + "|" + organization.NormaOrganization.Norma.Id);
                         if (organization.NormaOrganization.Norma.IsApproved)
                         {
                             norma.SpriteCssClasses = "Approved-show";
                         }
                         else
                             norma.SpriteCssClasses = "Approved-hide";
                             norma.Checkable = false;
                             norma.Text = norma.Text + "Норма: " + organization.NormaOrganization.Norma.Name;
                             norma.Value = organization.NormaOrganization.Norma.Id.ToString();

                             node1.Items.Add(norma);
//                           node1.Expanded = true;
                     }   


                 }
                 node1.Enabled = true;
                 nodes.Add(node1);
                 //if (norma!=null)
                 //   nodes.Add(norma);


            }
            return new JsonResult { Data = nodes };
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [Transaction]
        public ActionResult _UpdateAllSelectedNormas(int rootId, bool status)
        {
            List<Organization> list = new List<Organization>();
            getAllNormaTree(rootId, ref list);
            foreach(var item in list){
                UpdateNorma(item, status);
            }
            return null;
        }

        private void getAllNormaTree(int rootId, ref List<Organization> list)
        {
            Organization organization = organizationRepository.Get(rootId);
            foreach (var item in organization.Childs) 
            {
                if (item.IsWorkPlace)
                {
                    list.Add(item);
                }
                else
                {
                    if (item.Childs.Count > 0)
                    {
                        getAllNormaTree(item.Id, ref list);
                    }
                }
            }
        
        }

        private void UpdateNorma(Organization org, bool status)
        {
            Dictionary<string, object> query = new Dictionary<string, object>();
            query.Add("Organization", org);
            NormaOrganization norma = org.NormaOrganization;            
            if (norma != null)
            {
                norma.Norma.IsApproved = status;
                normaRepository.SaveOrUpdate(norma.Norma);
            }
        }
    }
}
