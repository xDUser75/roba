using System.Web.Mvc;
using System.Web.Routing;
using Store.Data;
using Store.Core;
using System.Collections;
using System.Collections.Generic;
using Store.Core.Account;
using System.Configuration;
using System;
using System.Xml;
using Store.Core.Utils;



namespace Store.Web.Controllers
{
    static class Extensions
    {
        public static bool In<T>(this T item, params T[] items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            for (int i = 0; i < items.Length; i++)
            {
                if (Object.Equals(item, items[i]))
                    return true;
            }
            return false;
        }
    }

    public class ViewedController : Controller
    {
        public const bool ASC = true;
        public const bool DESC = false;
        protected static string viewName;
        public DateTime Null_Date = new DateTime(1, 1, 1, 0, 0, 0, 0);

        public ViewedController() { }

        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
 //           System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ru-Ru");

            viewName = getViewName(ControllerContext);
            //string idOrg = Request.Params["_idOrg"];
            //string cachIdOrg = (string)Session["_idOrg"];
            //List<SelectListItem> enterprise = (List<SelectListItem>)Session[DataGlobals.REFERENCE_ENTERPRICE];
            //if (cachIdOrg != idOrg)
            //{
            //    if (idOrg != null)
            //    {
            //        cachIdOrg = idOrg;
            //        Session["_idOrg"] = cachIdOrg;
            //    }
            //}

//  Этот вариант рабочий (загрузка организаций из справочника), но надо тестировать на производительность. 
//  Как ведет себя OrganizationRepository если его создавать явным образом в теле этого метода?
            //if (enterprise == null || enterprise.Count == 0)
            //{
            //    enterprise = new List<SelectListItem>();
            //    OrganizationRepository organizationRepository = new OrganizationRepository();
            //    User user = (User)Session[DataGlobals.ACCOUNT_KEY];
            //    if (user == null)
            //        RedirectToRoute("~/LoginAccount/LogOn");
            //    IList<Organization> list = organizationRepository.GetEnterprises(user);
            //    int rowCount = 0;
            //    foreach (var item in list)
            //    {
            //        rowCount = rowCount + 1;
            //        if (rowCount == 1)
            //        {
            //            if (cachIdOrg == null)
            //            {
            //                cachIdOrg = item.Id.ToString();
            //                Session["_idOrg"] = cachIdOrg;
            //            }
            //        }
            //        enterprise.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString(), Selected = (item.Id.ToString() == cachIdOrg) });
            //    }

            //}
            //foreach (SelectListItem item in enterprise)
            //{
            //    if (item.Value == cachIdOrg)
            //        enterprise[enterprise.IndexOf(item)].Selected = true;
            //    else
            //        enterprise[enterprise.IndexOf(item)].Selected = false;
            //}

            //ControllerContext.HttpContext.Session[DataGlobals.REFERENCE_ENTERPRICE] = enterprise;
            //ControllerContext.Controller.ViewData["_EnterpriceSelectValue"] = cachIdOrg;
            //ControllerContext.Controller.ViewData[DataGlobals.REFERENCE_ENTERPRICE] = enterprise;
        }

        public string getCurrentEnterpriseId() {
            //if (Session["_idOrg"] != null)
            //    RedirectToAction("LogOn", "LoginAccount");
            return (string) Session["_idOrg"];
        }

        public int getIntCurrentEnterpriseId()
        {
            string org = getCurrentEnterpriseId();
            int res = -1;
            int.TryParse(org, out res);
            return res;
        }


        protected string getOrgByArmId()
        {
            string armId = ((Store.Core.Account.User)Session[DataGlobals.ACCOUNT_KEY]).ArmId;
            XmlNodeList list = ApplicationConfig.getNodeList("/Configuration/Organization[@armId=" + armId + "]");
            if (list == null) return null;
            if (list.Count == 0) return null;
            XmlNode node = list[0];
            //if (armId == DataGlobals.ARM_ID_OZSMK)
            //    return DataGlobals.ORG_ID_OZSMK;
            //if (armId == DataGlobals.ARM_ID_EVRAZTECHNIKA)
            //    return DataGlobals.ORG_ID_EVRAZTECHNIKA;
            //if (armId == DataGlobals.ARM_ID_VGOK)
            //    return DataGlobals.ORG_ID_VGOK;

            return node.Attributes["id"].Value;
        }


        private static bool isExclude(List<string> excludeProperty, String PropertyName)
        {
            if (excludeProperty == null) return false;
            for (int i = 0; i < excludeProperty.Count; i++) { 
                if (PropertyName.ToLower()==excludeProperty[i].ToLower()) return true;
            }
            return false;
        }

        private static List<string> rebuildExcludeProperty(List<string> excludeProperty, String PropertyName)
        {
            if (excludeProperty == null) return null;
            List<string> list = new List<string>();
            for (int i = 0; i < excludeProperty.Count; i++)
            {
                if (excludeProperty[i].StartsWith(PropertyName, true, null))
                { 
                    string val=excludeProperty[i];
                    string v = val.Substring(PropertyName.Length + 1);
                    if ((v != null) && (v.Length > 0))
                    {
                        list.Add(v);
                    }
                };
            }
            return list;
        }

        private static string getViewName(ControllerContext controllerContext)
        {
            string controllerName = controllerContext.Controller.GetType().Name;
            string viewName = controllerName.Substring(0, controllerName.IndexOf("Controller"));
            return viewName;

        }

        public static IList<NormaContent> reorderNormaContents(IList<NormaContent> normaContents, bool isWinter)
        {
            List<NormaContent> nc = new List<NormaContent>(normaContents);
            nc.Sort();
            normaContents = nc;

            NormaContent item = null;
            for (int i = 0, j = 0; j < normaContents.Count; i++)
            {
                item = normaContents[i];
                if (item.IsActive == false)
                {
                    normaContents.Remove(item);
                    i--;
                    continue;
                }
                if (item.InShop || (isWinter == false && item.NomGroup.IsWinter))
                {
                    normaContents.Remove(item);
                    i--;
                    continue;
                }
                if (item.NomGroup.NomBodyPart.Id == DataGlobals.SIZ_SIZE_ID)
                {
                    normaContents.Remove(item);
                    normaContents.Add(item);
                    i--;
                }
                j++;
            }
            return normaContents;
        }

        public static IList<WorkerCardContent> reorderWorkerCardContents(IList<WorkerCardContent> workerCardContents)
        {
            List<WorkerCardContent> wcc = new List<WorkerCardContent>(workerCardContents);
            wcc.Sort();
            workerCardContents = wcc;

            WorkerCardContent item = null;
            for (int i = 0, j = 0; j < workerCardContents.Count; i++)
            {
                item = workerCardContents[i];
                if (item.Storage.Nomenclature.NomBodyPart.Id == DataGlobals.SIZ_SIZE_ID)
                {
                    workerCardContents.Remove(item);
                    workerCardContents.Add(item);
                    i--;
                }
                j++;
            }
            return workerCardContents;
        }


        public static NomGroupPrice rebuildNomGroupPrice(NomGroupPrice inNomGroupPrice)
        {
            return rebuildNomGroupPrice(inNomGroupPrice, null);
        }

        public static NomGroupPrice rebuildNomGroupPrice(NomGroupPrice inNomGroupPrice, List<string> excludeProperty)
        {
            NomGroupPrice st = new NomGroupPrice(inNomGroupPrice.Id);
            st.OrganizationId = inNomGroupPrice.OrganizationId;
            st.Price = inNomGroupPrice.Price;
            st.ExternalCode = inNomGroupPrice.ExternalCode;
            if (!isExclude(excludeProperty, "NomGroup"))
            {
                st.NomGroup = rebuildNomGroup(inNomGroupPrice.NomGroup);
            }

            return st;
        }

        public static Certificate rebuildCertificate(Certificate inCertificate)
        {
            return rebuildCertificate(inCertificate, null);
        }

        public static Certificate rebuildCertificate(Certificate inCertificate, List<string> excludeProperty)
        {
            Certificate cert = new Certificate(inCertificate.Id);
            if (!isExclude(excludeProperty, "Organization"))
            {
                cert.Organization = rebuildOrganizationSimple(inCertificate.Organization);
            }
            cert.DocNum = inCertificate.DocNum;
            cert.DocDate = inCertificate.DocDate;
            cert.Descr = inCertificate.Descr;
            cert.Result = new Result(inCertificate.Result.Id);
            cert.Result.Name = inCertificate.Result.Name;
            cert.Result.Color = inCertificate.Result.Color;
            return cert;
        }


        public static TestRegister rebuildTestRegister(TestRegister inTestRegister)
        {
            return rebuildTestRegister(inTestRegister, null);
        }

        public static TestRegister rebuildTestRegister(TestRegister inTestRegister, List<string> excludeProperty)
        {
            TestRegister testReg = new TestRegister(inTestRegister.Id);
            testReg.TestDate = inTestRegister.TestDate;
            testReg.Model = inTestRegister.Model;
            if (!isExclude(excludeProperty, "Provaider"))
            {
                testReg.Provaider = new Provaider(inTestRegister.Provaider.Id);
                testReg.Provaider.Name = inTestRegister.Provaider.Name;
                testReg.Provaider.City = inTestRegister.Provaider.City;
            }
            if (!isExclude(excludeProperty, "NomGroup"))
            {
                testReg.NomGroup = rebuildNomGroup(inTestRegister.NomGroup,rebuildExcludeProperty(excludeProperty, "NomGroup"));
            }
            return testReg;
        }

        public static Subscription rebuildSubscription(Subscription inSubscription)
        {
            return rebuildSubscription(inSubscription, null);
        }

        public static Subscription rebuildSubscription(Subscription inSubscription, List<string> excludeProperty)
        {
            Subscription st = new Subscription(inSubscription.Id);
            if (!isExclude(excludeProperty, "Organization"))
            {
                st.Organization = rebuildOrganization(inSubscription.Organization);
            }


            if (inSubscription.Worker1 != null)
            {
                st.Tabn1 = inSubscription.Worker1.TabN;
                st.Worker1 = rebuildWorker(inSubscription.Worker1,rebuildExcludeProperty(excludeProperty, "Worker1"));
            }

            if (inSubscription.Worker2 != null)
            {
                st.Tabn2 = inSubscription.Worker2.TabN;
                st.Worker2 = rebuildWorker(inSubscription.Worker2, rebuildExcludeProperty(excludeProperty, "Worker2"));
            }
            if (inSubscription.Worker3 != null)
            {
                st.Tabn3 = inSubscription.Worker3.TabN;
                st.Worker3 = rebuildWorker(inSubscription.Worker3, rebuildExcludeProperty(excludeProperty, "Worker3"));
            }
            return st;
        }
        public static SignDocumet rebuildSignDocumet(SignDocumet inSignDocumet)
        {
            return rebuildSignDocumet(inSignDocumet, null);

        }

        public static SignDocumet rebuildSignDocumet(SignDocumet inSignDocumet, List<string> excludeProperty)
        {
            SignDocumet st = new SignDocumet(inSignDocumet.Id);
            st.OrganizationId = inSignDocumet.OrganizationId;
            st.ShopId = inSignDocumet.ShopId;

            if (!isExclude(excludeProperty, "Unit"))
            {
                st.Unit = rebuildOrganization(inSignDocumet.Unit);
            }
            st.StorageNameId = inSignDocumet.StorageNameId;
            if (inSignDocumet.Worker != null)
            {
                Sex s = null;
                    if (inSignDocumet.Worker.Sex != null)
                    {
                        s = new Sex(inSignDocumet.Worker.Sex.Id);
                        s.Name = inSignDocumet.Worker.Sex.Name;
                    }
                    st.Worker = new Worker(inSignDocumet.Worker.Id, inSignDocumet.Worker.TabN, inSignDocumet.Worker.Fio, s, inSignDocumet.Worker.IsActive);
            }
             /*
            if (inSignDocumet.Worker != null)
            {
                st.Worker = rebuildWorker(inSignDocumet.Worker);
            }
           */
            if (inSignDocumet.SignDocType != null)
            {
                st.SignDocType = rebuildSignDocTypes(inSignDocumet.SignDocType);
            }
            if (inSignDocumet.SignType != null)
            {
                st.SignType = rebuildSignTypes(inSignDocumet.SignType);
            }

            st.Tabn = inSignDocumet.Tabn;
            st.Fio = inSignDocumet.Fio;
            st.Ord = inSignDocumet.Ord;
            st.CodeDocumetn = inSignDocumet.CodeDocumetn;
            st.CodeSign = inSignDocumet.CodeSign;
            st.NameSign = inSignDocumet.NameSign;
            st.Value = inSignDocumet.Value;
            st.WorkPlaceName = inSignDocumet.WorkPlaceName;
            return st;
        }

        public static SignDocTypes rebuildSignDocTypes(SignDocTypes inSignDocTypes)
        {
            SignDocTypes st = new SignDocTypes(inSignDocTypes.Id);

            st.Code = inSignDocTypes.Code;
            st.Name = inSignDocTypes.Name;
            return st;
        }

        public static SignTypes rebuildSignTypes(SignTypes inSignTypes)
        {
            SignTypes st = new SignTypes(inSignTypes.Id);

            st.Code = inSignTypes.Code;
            st.Name = inSignTypes.Name;
            return st;
        }


        public static Storage rebuildStorage(Storage inStorage)
        {
            return rebuildStorage(inStorage, null);
        }

        public static Storage rebuildStorage(Storage inStorage, List<string> excludeProperty)
        {
            Storage st = new Storage(inStorage.Id);
            //StorageName sn = new StorageName(inStorage.StorageName.Id);
            //sn.Name = inStorage.StorageName.Name;
            //sn.StorageNumber = inStorage.StorageName.StorageNumber;
            //st.StorageName = sn;
            if (!isExclude(excludeProperty, "StorageName"))
            {
                st.StorageName = rebuildStorageName(inStorage.StorageName, rebuildExcludeProperty(excludeProperty, "StorageName"));
            }

     
            if (inStorage.Nomenclature != null)
            {
                Nomenclature nomenclature = new Nomenclature(inStorage.Nomenclature.Id);
                nomenclature.Name = inStorage.Nomenclature.Name;
                nomenclature.ExternalCode = inStorage.Nomenclature.ExternalCode;
                if (!isExclude(excludeProperty, "Nomenclature.Unit"))
                {

                    Unit unit = new Unit(inStorage.Nomenclature.Unit.Id, inStorage.Nomenclature.Unit.Name);
                    nomenclature.Unit = unit;
                }
                if (!isExclude(excludeProperty, "Nomenclature.Sex"))
                {

                    Sex sex = new Sex(inStorage.Nomenclature.Sex.Id, inStorage.Nomenclature.Sex.Name);
                    nomenclature.Sex = sex;
                }

                if (!isExclude(excludeProperty, "Nomenclature.NomGroup"))
                {
                    nomenclature.NomGroup = new NomGroup(inStorage.Nomenclature.NomGroup.Id, inStorage.Nomenclature.NomGroup.Name);
                }
                st.Nomenclature = nomenclature;
            }
            if (!isExclude(excludeProperty, "Nomenclature.Growth"))
            {
                st.Growth = rebuildNomBodyPartSize(inStorage.Growth, rebuildExcludeProperty(excludeProperty, "Nomenclature.Growth"));
            }
            st.Price = inStorage.Price;
            st.Quantity = inStorage.Quantity;
            if (!isExclude(excludeProperty, "Nomenclature.NomBodyPartSize"))
            {
                st.NomBodyPartSize = rebuildNomBodyPartSize(inStorage.NomBodyPartSize, rebuildExcludeProperty(excludeProperty, "Nomenclature.NomBodyPartSize"));
            }
            st.Wear = inStorage.Wear;
            return st;
        }


        public static Nomenclature rebuildNomenclature(Nomenclature inNomenclature)
        {
            return rebuildNomenclature(inNomenclature, null);
        }

        public static Nomenclature rebuildNomenclature(Nomenclature inNomenclature, List<string> excludeProperty)
        {
            Nomenclature nomenclature = null;
            if (inNomenclature != null)
            {
                nomenclature = new Nomenclature(inNomenclature.Id);
                nomenclature.IsActive = inNomenclature.IsActive;
                nomenclature.Enabled = inNomenclature.Enabled;
                nomenclature.Name = inNomenclature.Name;
                nomenclature.ExternalCode = inNomenclature.ExternalCode;

                if (!isExclude(excludeProperty, "StartDate"))
                {
                    nomenclature.StartDate = inNomenclature.StartDate;
                }
                if (!isExclude(excludeProperty, "FinishDate"))
                {
                    nomenclature.FinishDate = inNomenclature.FinishDate;
                }

                if (!isExclude(excludeProperty, "Organization"))
                {
                    nomenclature.Organization = rebuildOrganizationSimple(inNomenclature.Organization, rebuildExcludeProperty(excludeProperty, "Organization"));
                }

                if (!isExclude(excludeProperty, "NomBodyPart"))
                {
                    nomenclature.NomBodyPart = rebuildNomBodyPart(inNomenclature.NomBodyPart);
                }
                if (!isExclude(excludeProperty, "NomBodyPartSize"))
                {
                    nomenclature.NomBodyPartSize = rebuildNomBodyPartSize(inNomenclature.NomBodyPartSize);
                }
                if (!isExclude(excludeProperty, "Growth"))
                {
                    nomenclature.Growth = rebuildNomBodyPartSize(inNomenclature.Growth);
                }

                if (!isExclude(excludeProperty, "NomGroup"))
                {
                    nomenclature.NomGroup = rebuildNomGroup(inNomenclature.NomGroup);
                }

                if (!isExclude(excludeProperty, "Unit"))
                {
                    Unit unit = new Unit(inNomenclature.Unit.Id, inNomenclature.Unit.Name);
                    nomenclature.Unit = unit;
                }
                if (!isExclude(excludeProperty, "Sex"))
                {
                    if (inNomenclature.Sex != null)
                    {
                        Sex sex = new Sex(inNomenclature.Sex.Id, inNomenclature.Sex.Name);
                        nomenclature.Sex = sex;
                    }
                }

            }
            return nomenclature;
        }


        public static MatPersonOnHands rebuildMatPersonOnHand(MatPersonOnHands inMatPersonOnHands)
        {
            return rebuildMatPersonOnHand(inMatPersonOnHands, null);
        }

        public static MatPersonOnHands rebuildMatPersonOnHand(MatPersonOnHands inMatPersonOnHands, List<string> excludeProperty)
        {
            MatPersonOnHands person = null;
            if (inMatPersonOnHands != null)
            {
                person = new MatPersonOnHands();
                if (!isExclude(excludeProperty, "MatPersonCardHead"))
                {
                    person.MatPersonCardHead = rebuildMatPerson(inMatPersonOnHands.MatPersonCardHead, rebuildExcludeProperty(excludeProperty, "MatPersonCardHead"));
                }

                if (!isExclude(excludeProperty, "Nomenclature"))
                {
                    person.Nomenclature = rebuildNomenclature(inMatPersonOnHands.Nomenclature, rebuildExcludeProperty(excludeProperty, "Nomenclature"));
                }
                person.Wear = inMatPersonOnHands.Wear;
                person.Quantity = inMatPersonOnHands.Quantity;
                person.LastDocNumber = inMatPersonOnHands.LastDocNumber;
                person.LastDocDate = inMatPersonOnHands.LastDocDate;
                person.LastOperTypeId = inMatPersonOnHands.LastOperTypeId;
            }
            return person;
        }


        public static MatPersonCardHead rebuildMatPerson(MatPersonCardHead inMatPerson) {
            return rebuildMatPerson(inMatPerson, null);
        }


        public static MatPersonCardHead rebuildMatPerson(MatPersonCardHead inMatPerson, List<string> excludeProperty)
        {
            MatPersonCardHead person = null;
            if (inMatPerson != null) {
                person = new MatPersonCardHead(inMatPerson.Id);
                if (!isExclude(excludeProperty, "StorageName"))
                {
                    person.StorageName = rebuildStorageName(inMatPerson.StorageName, rebuildExcludeProperty(excludeProperty, "StorageName"));
                }
                if (!isExclude(excludeProperty, "Organization"))
                {
                    person.Organization = rebuildOrganizationSimple(inMatPerson.Organization, rebuildExcludeProperty(excludeProperty, "Organization"));
                }
                if (!isExclude(excludeProperty, "Worker"))
                {
                    person.Worker = rebuildWorker(inMatPerson.Worker, rebuildExcludeProperty(excludeProperty, "Worker"));
                }
                person.IsActive = inMatPerson.IsActive;
                if (!isExclude(excludeProperty, "Department"))
                {
                    person.Department = rebuildOrganizationSimple(inMatPerson.Department, rebuildExcludeProperty(excludeProperty, "Department"));
                }
            
            }
            return person;
        }


        public static Remaind rebuildRemain(Remaind inRemain)
        {
            return rebuildRemain(inRemain, null);
        }

        public static RemaindEx rebuildRemain(Remaind inRemaind, List<string> excludeProperty)
        {
            RemaindEx r = null;
            if (inRemaind != null)
            {
                r = new RemaindEx(inRemaind.Id);
                if (!isExclude(excludeProperty, "RemaindDate"))
                {
                    r.RemaindDate = inRemaind.RemaindDate;
                }
                if (!isExclude(excludeProperty, "Storage"))
                {
                    r.Storage = rebuildStorage(inRemaind.Storage, rebuildExcludeProperty(excludeProperty, "Storage"));
                }
                if (!isExclude(excludeProperty, "StorageName"))
                {
                    r.StorageName = rebuildStorageName(inRemaind.StorageName, rebuildExcludeProperty(excludeProperty, "StorageName"));
                }

                if (!isExclude(excludeProperty, "Nomenclature"))
                {
                    r.Nomenclature = rebuildNomenclature(inRemaind.Nomenclature, rebuildExcludeProperty(excludeProperty, "Nomenclature"));
                }

                if (!isExclude(excludeProperty, "Growth"))
                {
                    r.Growth = rebuildNomBodyPartSize(inRemaind.Growth, rebuildExcludeProperty(excludeProperty, "Growth"));
                }

                if (!isExclude(excludeProperty, "NomBodyPartSize"))
                {
                    r.NomBodyPartSize = rebuildNomBodyPartSize(inRemaind.NomBodyPartSize, rebuildExcludeProperty(excludeProperty, "NomBodyPartSize"));
                }
                r.Wear = inRemaind.Wear;
                r.Quantity = inRemaind.Quantity;
            }
            return r;
        }


        public static RemaindExternal rebuildRemaindExternal(RemaindExternal inRemain)
        {
            return rebuildRemaindExternal(inRemain, null);
        }

        public static RemaindExternal rebuildRemaindExternal(RemaindExternal inRemaind, List<string> excludeProperty)
        {
            RemaindExternal r = null;
            if (inRemaind != null)
            {
                r = new RemaindExternal(inRemaind.Id);
                if (!isExclude(excludeProperty, "RemaindDate"))
                {
                    r.RemaindDate = inRemaind.RemaindDate;
                }
                if (!isExclude(excludeProperty, "Storage"))
                {
                    r.Storage = rebuildStorage(inRemaind.Storage, rebuildExcludeProperty(excludeProperty, "Storage"));
                }
                if (!isExclude(excludeProperty, "StorageName"))
                {
                    r.StorageName = rebuildStorageName(inRemaind.StorageName, rebuildExcludeProperty(excludeProperty, "StorageName"));
                }

                if (!isExclude(excludeProperty, "Nomenclature"))
                {
                    r.Nomenclature = rebuildNomenclature(inRemaind.Nomenclature, rebuildExcludeProperty(excludeProperty, "Nomenclature"));
                }

                if (!isExclude(excludeProperty, "Growth"))
                {
                    r.Growth = rebuildNomBodyPartSize(inRemaind.Growth, rebuildExcludeProperty(excludeProperty, "Growth"));
                }

                if (!isExclude(excludeProperty, "NomBodyPartSize"))
                {
                    r.NomBodyPartSize = rebuildNomBodyPartSize(inRemaind.NomBodyPartSize, rebuildExcludeProperty(excludeProperty, "NomBodyPartSize"));
                }
                r.Wear = inRemaind.Wear;
                r.Quantity = inRemaind.Quantity;
                r.QuantityIn = inRemaind.QuantityIn;
                r.QuantityOut = inRemaind.QuantityOut;
            }
            return r;
        }

        public static StorageName rebuildStorageName(StorageName inStorageName)
        {
            return rebuildStorageName(inStorageName, null);
        }
        public static StorageName rebuildStorageName(StorageName inStorageName, List<string> excludeProperty)
        {
            StorageName sn = null;
            if (inStorageName != null)
            {
                sn = new StorageName(inStorageName.Id);
                sn.Name = inStorageName.Name;
                sn.StorageNumber = inStorageName.StorageNumber;
            }
            return sn;
        }

        public static Organization rebuildOrganization(Organization inOrganization, List<string> excludeProperty)
        {
            Organization outOrganization = null;
            try
            {
                outOrganization = new Organization(inOrganization.Id);
                outOrganization.Pid = inOrganization.Pid;
                outOrganization.Name = inOrganization.Name;
                outOrganization.ShortName = inOrganization.ShortName;
                outOrganization.ShopNumber = inOrganization.ShopNumber;
                outOrganization.Short = inOrganization.Short;
                outOrganization.Mvz = inOrganization.Mvz;
                outOrganization.MvzName = inOrganization.MvzName;
                outOrganization.IsWorkPlace = inOrganization.IsWorkPlace;
                outOrganization.IsActive = inOrganization.IsActive;
                if (inOrganization.Parent.Id != 0) 
                    outOrganization.Parent = rebuildOrganization(inOrganization.Parent);
                if (inOrganization.NormaOrganization != null)
                    outOrganization.NormaOrganization = inOrganization.NormaOrganization.rebuild();
                if (inOrganization.StorageName != null)
                    if (!isExclude(excludeProperty, "StorageName"))
                    {
                        outOrganization.StorageName = rebuildStorageName(inOrganization.StorageName, rebuildExcludeProperty(excludeProperty, "StorageName"));
                    }
            }
            catch { }
            return outOrganization;
        }

        public static Organization rebuildOrganization(Organization inOrganization)
        {
            return rebuildOrganization(inOrganization, null);
        }

        public static Organization rebuildOrganizationSimple(Organization inOrganization) {
            return rebuildOrganizationSimple(inOrganization, null);
        }

        public static Organization rebuildOrganizationSimple(Organization inOrganization, List<string> excludeProperty)
        {
            Organization outOrganization = null;
            outOrganization = new Organization(inOrganization.Id);
            outOrganization.Pid = inOrganization.Pid;
            if (!isExclude(excludeProperty, "Name"))
            {

                outOrganization.Name = inOrganization.Name;
            }
            if (!isExclude(excludeProperty, "ShortName"))
            {
                outOrganization.ShortName = inOrganization.ShortName;
            }
            outOrganization.ShopNumber = inOrganization.ShopNumber;
            outOrganization.Mvz = inOrganization.Mvz;
            outOrganization.MvzName = inOrganization.MvzName;
            return outOrganization;
        }

        public static NomBodyPartSize rebuildNomBodyPartSize(NomBodyPartSize inNomBodyPartSize) {
            return rebuildNomBodyPartSize(inNomBodyPartSize, null);
        }
        
        public static NomBodyPartSize rebuildNomBodyPartSize(NomBodyPartSize inNomBodyPartSize, List<string> excludeProperty)
        {
            try
            {

                if (inNomBodyPartSize != null)
                {
                    NomBodyPartSize nbps = new NomBodyPartSize(inNomBodyPartSize.Id);
                    nbps.SizeNumber = inNomBodyPartSize.SizeNumber;
                    nbps.IsActive = inNomBodyPartSize.IsActive;
                    if (!isExclude(excludeProperty, "NomBodyPart"))
                    {
                        nbps.NomBodyPart = rebuildNomBodyPart(inNomBodyPartSize.NomBodyPart);
                    }
                    return nbps;
                }
            }
            catch { }
            return null;
        }

        public static Worker rebuildWorker(Worker inWorker) { 
            return rebuildWorker(inWorker, null);
        }

        public static Worker rebuildWorker(Worker inWorker, List<string> excludeProperty)
        {
            Sex s = null;
            if (!isExclude(excludeProperty, "Sex"))
            {
                if (inWorker.Sex != null)
                {
                    s = new Sex(inWorker.Sex.Id);
                    s.Name = inWorker.Sex.Name;
                }
            }
            Worker w = new Worker(inWorker.Id, inWorker.TabN, inWorker.Fio, s, inWorker.IsActive);
            w.WorkDate = inWorker.WorkDate;
            w.ChildcareBegin = inWorker.ChildcareBegin;
            w.ChildcareEnd =  inWorker.ChildcareEnd;
            w.IsTabu = inWorker.IsTabu;
            if (!w.IsActive)
                w.DateZ = inWorker.DateZ;
            w.BeginDate = inWorker.BeginDate;
            if (!isExclude(excludeProperty, "nomBodyPartSizes"))
            {
                IList<NomBodyPartSize> nomBodyPartSizes = new List<NomBodyPartSize>();
                foreach (var item in inWorker.NomBodyPartSizes)
                {
                    NomBodyPartSize nomBodyPartSize = rebuildNomBodyPartSize(item);
                    nomBodyPartSizes.Add(nomBodyPartSize);
                }
                w.NomBodyPartSizes = nomBodyPartSizes;
            }
            if (!isExclude(excludeProperty, "WorkerCategory"))
            {
                if (inWorker.WorkerCategory != null)
                {
                    if (!isExclude(excludeProperty, "WorkerCategory"))
                    {
                        WorkerCategory wc = new WorkerCategory(inWorker.WorkerCategory.Id, inWorker.WorkerCategory.Name);
                        w.WorkerCategory = wc;
                    }
                }
            }

            if (!isExclude(excludeProperty, "WorkerGroup"))
            {
                if (inWorker.WorkerGroup != null)
                {
                    if (!isExclude(excludeProperty, "WorkerGroup"))
                    {
                        WorkerGroup wg = new WorkerGroup(inWorker.WorkerGroup.Id, inWorker.WorkerGroup.Name);
                        w.WorkerGroup = wg;
                    }
                }
            }
            
            return w;
        }

        public static WorkerWorkplace rebuildWorkerWorkplace(WorkerWorkplace inWorkerWorkplace)
        {
            if (inWorkerWorkplace == null) return null;
            WorkerWorkplace outWorkerWorkplace = new WorkerWorkplace(inWorkerWorkplace.Id);
            outWorkerWorkplace.Worker = rebuildWorker(inWorkerWorkplace.Worker);
            outWorkerWorkplace.Organization = rebuildOrganization(inWorkerWorkplace.Organization);
            outWorkerWorkplace.RootOrganization = inWorkerWorkplace.RootOrganization;
            outWorkerWorkplace.IsActive = inWorkerWorkplace.IsActive;
            outWorkerWorkplace.BeginDate = inWorkerWorkplace.BeginDate;
            return outWorkerWorkplace;
        }

        public static WorkerWorkplace rebuildWorkerWorkplaceSimple(WorkerWorkplace inWorkerWorkplace){
            return rebuildWorkerWorkplaceSimple(inWorkerWorkplace, null);
        }

        public static WorkerWorkplace rebuildWorkerWorkplaceSimple(WorkerWorkplace inWorkerWorkplace, List<string> excludeProperty)
        {
            WorkerWorkplace outWorkerWorkplace = new WorkerWorkplace(inWorkerWorkplace.Id);
            if (!isExclude(excludeProperty, "Worker"))
            {
                outWorkerWorkplace.Worker = rebuildWorker(inWorkerWorkplace.Worker, rebuildExcludeProperty(excludeProperty, "Worker"));
            }
            if (!isExclude(excludeProperty, "Organization"))
            {
                outWorkerWorkplace.Organization = rebuildOrganizationSimple(inWorkerWorkplace.Organization, rebuildExcludeProperty(excludeProperty, "Organization"));
            }
            outWorkerWorkplace.IsActive = inWorkerWorkplace.IsActive;
            outWorkerWorkplace.BeginDate = inWorkerWorkplace.BeginDate;
            return outWorkerWorkplace;
        }


        public static WorkerSize rebuildWorkerSize(WorkerSize inWorkerSize)
        {
            NomBodyPartSize nbs = rebuildNomBodyPartSize(inWorkerSize.NomBodyPartSize);
            Worker wk = rebuildWorker(inWorkerSize.Worker);
            WorkerSize ws = new WorkerSize(inWorkerSize.Id, wk, nbs, inWorkerSize.IsActive);
            return ws;
        }

        public static Operation rebuildOperation(Operation inOperation) {
            return rebuildOperation(inOperation, null);
        }
        public static Operation rebuildOperation (Operation inOperation, List<string> excludeProperty)
        {
            Operation outOperation = null;
            if (inOperation != null)
            {
                outOperation = new Operation(inOperation.Id);
                outOperation.DocNumber = inOperation.DocNumber;
                outOperation.DocDate = inOperation.DocDate;
                outOperation.Note = inOperation.Note;
                outOperation.Wear = inOperation.Wear;
                if (outOperation.Wear == null)
                {
                    outOperation.Wear = inOperation.Storage.Wear;
                }

                outOperation.Quantity = inOperation.Quantity;
                outOperation.OperDate = inOperation.OperDate;
                outOperation.Partner = rebuildStorageName(inOperation.Partner);
                if (!isExclude(excludeProperty, "StorageName"))
                {
                    outOperation.StorageName = rebuildStorageName(inOperation.StorageName, rebuildExcludeProperty(excludeProperty, "StorageName"));
                }
                OperType ot = new OperType(inOperation.OperType.Id, inOperation.OperType.Name);
                outOperation.OperType = ot;
                if (!isExclude(excludeProperty, "Storage"))
                {
                    outOperation.Storage = rebuildStorage(inOperation.Storage, rebuildExcludeProperty(excludeProperty, "Storage"));
                }
                if (!isExclude(excludeProperty, "DocType"))
                {

                    if (inOperation.DocType != null)
                    {
                        DocType doct = new DocType(inOperation.DocType.Id, inOperation.DocType.Name);
                        outOperation.DocType = doct;
                    }
                }
                if (!isExclude(excludeProperty, "WorkerWorkplace"))
                {
                    WorkerWorkplace wwp = new WorkerWorkplace();
                    Organization org = new Organization();
                    Worker worker = new Worker();
                    wwp.Organization = org;
                    wwp.Worker = worker;
                    if (inOperation.WorkerWorkplace != null)
                    {
                        wwp = rebuildWorkerWorkplaceSimple(inOperation.WorkerWorkplace, rebuildExcludeProperty(excludeProperty, "WorkerWorkplace"));
                    }
                    outOperation.WorkerWorkplace = wwp;
                }
                if (inOperation.Motiv != null)
                {
                    Motiv m = new Motiv(inOperation.Motiv.Id, inOperation.Motiv.Name);
                    outOperation.Motiv = m;
                }
            }
            return outOperation;
        }

        public static Norma rebuildNorma(Norma inNorma)
        {

            Norma norma = new Norma(inNorma.Id, inNorma.Name, rebuildOrganization(inNorma.Organization));
            norma.IsApproved = inNorma.IsApproved;
            norma.IsActive = inNorma.IsActive;
            norma.NormaComment = inNorma.NormaComment;
            //IList<NormaContent> normaContents = new List<NormaContent>();
            //foreach (var item in inNorma.NormaContents)
            //{
            //    NormaContent normaContent = rebuildNormaContent(item);
            //    normaContents.Add(normaContent);
            //}
            //norma.NormaContents = normaContents;

            return norma;
        }

        public static NormaContent rebuildNormaContent(NormaContent inNormaContent)
        {
            NormaContent normaContent = new NormaContent(inNormaContent.Id, inNormaContent.Quantity, inNormaContent.UsePeriod);
            normaContent.NormaId = inNormaContent.NormaId;
            normaContent.IsActive = inNormaContent.IsActive;
//            if (inNormaContent.QuantityTON != 0)
                normaContent.QuantityTON = inNormaContent.QuantityTON;
//            else
//                normaContent.QuantityTON = inNormaContent.Quantity;
            NomGroup nomGroup = rebuildNomGroup(inNormaContent.NomGroup);
            normaContent.NomGroup = nomGroup;
            normaContent.InShop = inNormaContent.InShop;
            List<NormaNomGroup> normaNomGroups = new List<NormaNomGroup>();
            foreach (var item in inNormaContent.NormaNomGroups)
            {
                NormaNomGroup normaNomGroup = rebuildNormaNomGroup(item);
                normaNomGroups.Add(normaNomGroup);
            }
            normaContent.NormaNomGroups = normaNomGroups;
            string normaContentInfo = "";
            foreach (var item in normaContent.NormaNomGroups)
            {
                if (item.IsBase==false)
                    normaContentInfo = normaContentInfo + item.NomGroup.Name+"; ";
            }
            normaContent.NormaContentInfo = normaContentInfo;
            return normaContent;
        }

        public static NomBodyPart rebuildNomBodyPart(NomBodyPart inNomBodyPart)
        {
            NomBodyPart nb = new NomBodyPart(inNomBodyPart.Id, inNomBodyPart.Name);
            return nb;
        }

        public static NomGroup rebuildNomGroup(NomGroup inNomGroup)
        {

            return rebuildNomGroup(inNomGroup, null);
        }

        public static NomGroup rebuildNomGroup(NomGroup inNomGroup, List<string> excludeProperty)
        {
            NomGroup nomGroup = new NomGroup(inNomGroup.Id, inNomGroup.Name);
            nomGroup.IsWinter = inNomGroup.IsWinter;
            nomGroup.ExternalCode = inNomGroup.ExternalCode;
            nomGroup.IsActive = inNomGroup.IsActive;
            if (inNomGroup.Organization != null)
            {
                if (!isExclude(excludeProperty, "Organization"))
                {
                    nomGroup.Organization = rebuildOrganizationSimple(inNomGroup.Organization, rebuildExcludeProperty(excludeProperty, "Organization"));
                }
            }
            if (inNomGroup.Organization != null)
            {
                if (!isExclude(excludeProperty, "NomBodyPart"))
                {
                    nomGroup.NomBodyPart = rebuildNomBodyPart(inNomGroup.NomBodyPart);
                }
            }
            nomGroup.NameOT = inNomGroup.NameOT;
            return nomGroup;
        }
        public static NormaNomGroup rebuildNormaNomGroup(NormaNomGroup inNormaNomGroup)
        {
            NormaNomGroup normaNomGroup = new NormaNomGroup(inNormaNomGroup.Id);
            normaNomGroup.NomGroup = rebuildNomGroup(inNormaNomGroup.NomGroup);
            normaNomGroup.IsBase = inNormaNomGroup.IsBase;
            NormaContent nc = new NormaContent(inNormaNomGroup.NormaContent.Id);
            normaNomGroup.NormaContent = nc;
            return normaNomGroup;
        }
        public static NormaOrganization rebuildNormaOrganization(NormaOrganization inNormaOrganization)
        {
            NormaOrganization normaOrganization = new NormaOrganization(inNormaOrganization.Id);
            normaOrganization.Norma = rebuildNorma(inNormaOrganization.Norma);
            normaOrganization.Organization = rebuildOrganization(inNormaOrganization.Organization);
            normaOrganization.IsActive = inNormaOrganization.IsActive;
            return normaOrganization;
        }

        public static NormaOrganizationSimple rebuildNormaOrganizationSimple(NormaOrganizationSimple inNormaOrganization)
        {
            NormaOrganizationSimple normaOrganization = new NormaOrganizationSimple(inNormaOrganization.Id);
            OrganizationRepository organizationRepository = new OrganizationRepository();
            normaOrganization.Norma = rebuildNorma(inNormaOrganization.Norma);
            normaOrganization.OrganizationId = inNormaOrganization.OrganizationId;
            Organization organization = organizationRepository.Get(inNormaOrganization.OrganizationId);
            normaOrganization.OrganizationFullName = organization.FullName;
            normaOrganization.Sort = organization.Short;
            normaOrganization.IsActive = inNormaOrganization.IsActive;
            return normaOrganization;
        }

        // пересчет остатков за период

        public static void rebuildRemaind(CriteriaRepository<Operation> operationRepository, CriteriaRepository<Remaind> remainsRepository, Storage storage, DateTime beginDate, DateTime endDate, DateTime remaindDate)
        {
            rebuildRemaind(operationRepository, remainsRepository, storage, beginDate, endDate,remaindDate, true );
        }

        public static void rebuildRemaind(CriteriaRepository<Operation> operationRepository, CriteriaRepository<Remaind> remainsRepository, Storage storage, DateTime beginDate, DateTime endDate, DateTime remaindDate, bool saveZeroCount)
        {
            Dictionary<string, object> query = new Dictionary<string, object>();
            int beginQuantity = 0;

            // ищем остатки за предыдущий период
            query.Add("Storage", storage);
            query.Add("RemaindDate", beginDate);
            IList<Remaind> beginRemainds = remainsRepository.FindAll(query);
            if (beginRemainds.Count > 0)
                beginQuantity = beginRemainds[0].Quantity;

            // ищем операции по позиции склада за период
            query.Clear();
            query.Add("Storage", storage);
            query.Add("[>=]OperDate", beginDate);
            query.Add("[<=]OperDate", endDate);
            IList<Operation> operations = operationRepository.GetByLikeCriteria(query);

            // бежим по операциям и пересчитываем остаток
            foreach (var item in operations)
            {
                if (item.OperType != null)
                {
                    if (item.OperType.Id == DataGlobals.OPERATION_STORAGE_IN ||
                        item.OperType.Id == DataGlobals.OPERATION_WORKER_RETURN ||
                        item.OperType.Id == DataGlobals.OPERATION_STORAGE_SAP_IN ||
                        item.OperType.Id == DataGlobals.OPERATION_MOL_STORAGE_IN
                        )
                    {
                        beginQuantity += item.Quantity;
                    }
                    else if (item.OperType.Id == DataGlobals.OPERATION_STORAGE_OUT ||
                        item.OperType.Id == DataGlobals.OPERATION_WORKER_IN ||
                        item.OperType.Id == DataGlobals.OPERATION_STORAGE_WEAR_OUT ||
                        item.OperType.Id == DataGlobals.OPERATION_MOL_STORAGE_OUT 
                        )
                    {
                        // .DocNumber="0" данные на руках, загруженые в систему, не должны отражаться на состоянии склада
                        if (item.DocNumber!="0")
                            beginQuantity -= item.Quantity;
                    }
                }
            }
            if ((saveZeroCount) || (saveZeroCount == false && beginQuantity != 0))
            {

                //переписываем пересчитанный остаток
                query.Clear();
                query.Add("Storage", storage);
                query.Add("RemaindDate", remaindDate);
                IList<Remaind> endRemainds = remainsRepository.FindAll(query);
                DateTime currentDate = DateTime.Now;
                if (endRemainds.Count > 0)
                {
                    endRemainds[0].Quantity = beginQuantity;
                    endRemainds[0].ActualDate = currentDate;
                    remainsRepository.SaveOrUpdate(endRemainds[0]);
                }
                else
                {
                    Remaind remaind = new Remaind();
                    remaind.NomBodyPartSize = storage.NomBodyPartSize;
                    remaind.Nomenclature = storage.Nomenclature;
                    remaind.Quantity = beginQuantity;
                    remaind.RemaindDate = remaindDate;
                    remaind.Storage = storage;
                    remaind.StorageName = storage.StorageName;
                    remaind.Wear = int.Parse(storage.Wear);
                    remaind.Growth = storage.Growth;
                    remaind.ActualDate = currentDate;
                    remainsRepository.SaveOrUpdate(remaind);
                }
            }
        }

        public static string getConfigParamValue(CriteriaRepository<Config> configRepository, string paramName, string idOrg)
        {
            int idEnterprise = System.Int32.Parse(idOrg);

            Dictionary<string, object> query = new Dictionary<string, object>();
            string paramValue = null;
            query.Add("OrganizationId", idEnterprise);
            query.Add("ParamName", paramName);
            Config config = configRepository.FindOne(query);

            if (config != null)
                paramValue = config.ParamValue;

            return paramValue;
        }


    
    }


}
