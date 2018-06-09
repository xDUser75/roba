using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Store.Core.Account;
using Store.ApplicationServices.AccountServices;
using Store.Data;
using Store.Core.RepositoryInterfaces;
using Store.Core.Utils;

namespace Store.Web.Controllers
{
    public class LoginAccountController : Controller
    {
        UserMemberProvider provider = (UserMemberProvider)Membership.Provider;
        private readonly IUserRepository userRepository;

        public LoginAccountController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }
        ///*
        public ActionResult LogOnForce()
        {
            return LogOn(true);
        }
        // */

        public ActionResult LogOn(bool? force)
        {
            //FormsAuthentication.SignOut();
            SelectList armSelectList = null;
            if (force == null)
            {
                string logonUser = HttpContext.Request.LogonUserIdentity.Name;
                //logonUser = "SIB\\prilepin_vs";
                //logonUser = null;

                if (!string.IsNullOrEmpty(logonUser)) {
                    Dictionary<string, object> queryParams = new Dictionary<string, object>();
                    Dictionary<string, string> list = ApplicationConfig.getAllOrganizationArmId();
                    string armCSV = "'0',";
                    foreach (var item in list)
                    {
                        armCSV = armCSV + ",'" + item.Key + "'";
                    }

                    queryParams.Add("UserInfo.UserName", logonUser.Split('\\')[1]);
                    queryParams.Add("[in]ArmId", armCSV);
                    IList<User> users = userRepository.GetByCriteriaIgnoreCase(queryParams);

                    armSelectList = new SelectList(users, "ArmInfo.Id", "ArmInfo.Name");

                    if (users.Count == 1)
                        return ValidateUser(logonUser, "Ра$$w0rd", users[0].ArmId, null);
                }
                force = true;
            }

            armSelectList = setDefaultArmSelectList();

            //SelectList organizationList = new SelectList(organizationRepository.GetEnterprises(null), "Id", "Name");
            ViewData[DataGlobals.REFERENCE_ENTERPRICE] = armSelectList;
            ViewData["logonForce"] = force;
            return View();
        }


        public ActionResult AccessDenied()
        {
            return View("Denied");
        }

        public ActionResult ValidateUser(string userName, string password, string _EnterpriceList, string returnUrl, bool force = false)
        {
            if (!ValidateLogOn(userName, password, _EnterpriceList))
            {
                if (!force)
                {
                    return View();
                }
                else 
                { 
                    return AccessDenied();
                }
            }
            User user = provider.User;

            user.UserInfo.DomainName = HttpContext.Request.LogonUserIdentity.Name;

            FormsAuthentication.SetAuthCookie(user.UserInfo.UserName, false);

            HttpContext.Session[DataGlobals.ACCOUNT_KEY] = user;
            HttpContext.User = user;

            if (!String.IsNullOrEmpty(returnUrl) && returnUrl != "/")
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult LogOn(string userName, string password, string _EnterpriceList, string returnUrl)
        {
            SelectList armSelectList = setDefaultArmSelectList();

            ViewData[DataGlobals.REFERENCE_ENTERPRICE] = armSelectList;
            ViewData["logonForce"] = true;

            if (String.IsNullOrEmpty(userName))
            {
                userName = HttpContext.Request.LogonUserIdentity.Name;
                password = "Ра$$w0rd";
            }
            return ValidateUser(userName, password, _EnterpriceList, returnUrl);
        }
        [Authorize]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            //return RedirectToAction("LogOn", "LoginAccount", new { force = true });
            return LogOnForce();
        }
        private bool ValidateLogOn(string userName, string password, string _EnterpriceList)
        {
            if (String.IsNullOrEmpty(userName))
            {
                //ModelState.AddModelError("username", "You must specify a username.");
                ModelState.AddModelError("username", "Введите пользователя.");
            }
            if (String.IsNullOrEmpty(password))
            {
                //ModelState.AddModelError("password", "You must specify a password.");
                ModelState.AddModelError("password", "Введите пароль.");
            }
            if (!provider.ValidateUser(userName+"\\"+_EnterpriceList, password))
            {
                //ModelState.AddModelError("_FORM", "The username or password provided is incorrect.");
                ModelState.AddModelError("_FORM", "Нет доступа.");
            }
            return ModelState.IsValid;
        }

        private SelectList setDefaultArmSelectList()
        {
            IList<ArmInfo> armList = new List<ArmInfo>();
            Dictionary<string, string> list = ApplicationConfig.getAllOrganizationArmId();
            foreach (var item in list) {
                armList.Add(new ArmInfo(item.Key, item.Value));
            }
            return new SelectList(armList, "Id", "Name");
        }
    }

}
