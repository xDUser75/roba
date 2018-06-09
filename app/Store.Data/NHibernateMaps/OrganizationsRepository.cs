using Store.Core;
using Store.Core.RepositoryInterfaces;
using System.Collections.Generic;
using System;
using Store.Core.Account;

namespace Store.Data
{
    public class OrganizationRepository : CriteriaRepository<Organization>
    {
        public IList<Organization> GetShops(String orgId)
        {
            if (orgId == null) {
                orgId = "-1";
            }
            IList<Organization> shops = Session.CreateQuery("from Organization O where O.Id = O.ShopId and isactive=1 and OrganizationId=" + orgId + " Order by ShortName").List<Organization>();
            return shops;
        }

        public IList<Organization> GetActiveShops(String orgId, string ShopNumber)
        {
            if (orgId == null)
            {
                orgId = "-1";
            }
            IList<Organization> shops = Session.CreateQuery("from Organization O where O.Id = O.ShopId and isactive=1 and OrganizationId=" + orgId + " and lower(ShopNumber) like '" + ShopNumber.ToLower() + "%' Order by ShortName").List<Organization>();
            return shops;
        }


        public IList<Organization> GetShops()
        {
            IList<Organization> shops = Session.CreateQuery("from Organization O where O.Id = O.ShopId and isactive=1 Order by ShortName").List<Organization>();
            return shops;
        }

        public IList<Organization> GetShopsByEnterprise(int idEnterprise)
        {
            IList<Organization> shops = Session.CreateQuery("from Organization O where O.Id = O.ShopId and isactive=1 and organizationid=" + idEnterprise + " Order by Short").List<Organization>();
            return shops;
        }
        public IList<Organization> GetUnitByShop(int idEnterprise, int idShop)
        {
            IList<Organization> units = Session.CreateQuery("from Organization O where O.Pid = "+ idShop +" and isactive=1 and organizationid=" + idEnterprise + " Order by Short").List<Organization>();
            return units;
        }

        
        public IList<Organization> GetAllShopsByEnterprise(int idEnterprise)
        {
            IList<Organization> shops = Session.CreateQuery("from Organization O where O.Id = O.ShopId and organizationid=" + idEnterprise + " Order by Short").List<Organization>();
            return shops;
        }

        public IList<Organization> GetOrganizationsByEnterprise(int idEnterprise)
        {
            IList<Organization> organizations = Session.CreateQuery("from Organization O where isactive=1 and organizationid=" + idEnterprise).List<Organization>();
            return organizations;
        }


        public IList<Organization> GetEnterprises(User user)
        {
            string param = "";
            if (user != null) {
                if (user.Organizations.Count > 0) {
                    param = "and O.Id in (";
                    foreach (var item in user.Organizations){
                        param = param + item+", ";
                    }
                    param = param.Substring(0,param.Length-2) + ")";
                }
            }
            //else param = "and O.Id in (-1)";
            IList<Organization> shops = Session.CreateQuery("from Organization O where O.Pid = 0 " + param).List<Organization>();
            return shops;
        }

    }
}
