using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using Store.Core.Account;
using Store.Core.RepositoryInterfaces;
using Microsoft.Practices.ServiceLocation;
using System.Collections.Specialized;
using System.Configuration;
using SharpArch.Web.NHibernate;
using Store.Data;
using System.Web;

namespace Store.ApplicationServices.AccountServices
{
   public class UserGroupProvider : RoleProvider
   {
       private string pApplicationName;
       IGroupRepository _repository;

       public UserGroupProvider()
           : this(ServiceLocator.Current.GetInstance<IGroupRepository>()) 
       {
            
       }
       public UserGroupProvider(IGroupRepository repository) 
        {
            _repository = repository;
        }
       public override void Initialize(string name, NameValueCollection config)
       {
           // Initialize the abstract base class.
           base.Initialize(name, config);
           //pApplicationName = config["applicationName"];
           pApplicationName = ConfigurationManager.AppSettings["applicationName"];
       }
       public override string ApplicationName
       {
           get { return pApplicationName; }
           set { pApplicationName = value; }
       }
       public override void AddUsersToRoles(string[] usernames, string[] roleNames)
       {
            throw new NotImplementedException();
        }
       public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
       {
           throw new NotImplementedException();
       }
       public override void CreateRole(string roleName)
       {
           throw new NotImplementedException();
       }
       public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
       {
           throw new NotImplementedException();
       }
       public override bool RoleExists(string roleName)
       {
           throw new NotImplementedException();
       }
       public override bool IsUserInRole(string username, string roleName)
       {
           /*
           Dictionary<string, object> queryParams = new Dictionary<string, object>();
           queryParams.Add("UserName", username);
           queryParams.Add("ArmId", pApplicationName);
           //User user = _repository.FindOne(queryParams);
           User user = null;
           if (user != null)
               return user.IsInRole(roleName);
           else
               return false;
            */
           throw new NotImplementedException();
       }

       [Transaction(DataGlobals.ACCOUNT_DB_FACTORY_KEY)]
       public override string[] GetRolesForUser(string username)
       {
           Dictionary<string, object> queryParams = new Dictionary<string, object>();
           string auth_user_name = (string)HttpContext.Current.Session["auth_user_name"];
           if (auth_user_name == null) auth_user_name = "0\\0";
           String[] userInfo = auth_user_name.Split(new char[] { '\\' });
           int ui=0;

           //org = org + "";

           ///*
           //queryParams.Add("UserInfo.UserName", userInfo[1]);
           //*/
//           _repository.SaveDebugInfo("GetRolesForUser username", username);
           int.TryParse(userInfo[0], out ui);
//           queryParams.Add("UserId", int.Parse(userInfo[0]));
           queryParams.Add("UserId", ui);
           queryParams.Add("ArmId", userInfo[1]);
           IList<Group> userRoles = _repository.GetByCriteria(queryParams);

           IList<string> roles = new List<string>();
           foreach (Group curRole in userRoles)
           {
               if (curRole.Description != null)
                   roles.Add(curRole.Description);
               foreach (Access right in curRole.Accesses)
                   if (right.Description != null)
                       roles.Add(right.Description);
           }
           
           //ISet<Role> userRoles = (ISet)_repository.FindAll(queryParams);
           /*
           string[] roles = new string[user.Role.Rights.Count + 1];
           roles[0] = user.Role.Description;
           int idx = 0;
           foreach (Right right in user.Role.Rights)
               roles[++idx] = right.Description;
           return roles;
            */
           //string[] roles = new string[userRoles.Count+1];
           //int idx = 0;
           /*
           foreach (Role curRole in userRoles)
           {
               roles[idx++] = curRole.Description;
           }
            */
           //roles[idx] = "admin";
           //roles[idx+1] = "view";
           return roles.ToArray<string>();

       }
       public override string[] GetUsersInRole(string roleName)
       {
           throw new NotImplementedException();
       }
       public override string[] FindUsersInRole(string roleName, string usernameToMatch)
       {
           throw new NotImplementedException();
       }
       public override string[] GetAllRoles()
       {
           throw new NotImplementedException();
       }
   }
}
