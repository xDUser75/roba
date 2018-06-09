using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Security;
using Store.Core.Account;
using Store.Core.RepositoryInterfaces;
using Microsoft.Practices.ServiceLocation;
using System.Collections.Specialized;
using System.Configuration;
using SharpArch.Web.NHibernate;
using Store.Data;
using SharpArch.Core.PersistenceSupport;
using System.Web;
using System.IO;

namespace Store.ApplicationServices.AccountServices
{
    public class UserMemberProvider : MembershipProvider
    {
        #region Unimplemented MembershipProvider Methods
        private string pApplicationName;

        public override void Initialize(string name, NameValueCollection config)
        {
            // Initialize the abstract base class.
            base.Initialize(name, config);
            pApplicationName = ConfigurationManager.AppSettings["applicationName"];
            //pApplicationName = config["applicationName"];
        }
        public override string ApplicationName
        {
            get { return pApplicationName; }
            set { pApplicationName = value; }
        }
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }
      
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }
        public override bool EnablePasswordReset
        {
            get { throw new NotImplementedException(); }
        }
        public override bool EnablePasswordRetrieval
        {
            get { throw new NotImplementedException(); }
        }
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }
        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }
        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            throw new NotImplementedException();
        }
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }
        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }
        public override int MaxInvalidPasswordAttempts
        {
            get { throw new NotImplementedException(); }
        }
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new NotImplementedException(); }
        }
        public override int MinRequiredPasswordLength
        {
            get { throw new NotImplementedException(); }
        }
        public override int PasswordAttemptWindow
        {
            get { throw new NotImplementedException(); }
        }
        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new NotImplementedException(); }
        }
        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(); }
        }
        public override bool RequiresQuestionAndAnswer
        {
            get { throw new NotImplementedException(); }
        }
        public override bool RequiresUniqueEmail
        {
            get { throw new NotImplementedException(); }
        }
        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }
        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }
        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }
        
        #endregion
        IUserRepository _repository;
        //IGroupRepository _repositoryGroup;
        public UserMemberProvider()
            //: this(ServiceLocator.Current.GetInstance<IUserRepository>(), ServiceLocator.Current.GetInstance<IGroupRepository>())
            : this(ServiceLocator.Current.GetInstance<IUserRepository>())
            //: this(ServiceLocator.Current.GetInstance<CriteriaRepository<User>>())
        {
        }
        //public UserMemberProvider(IUserRepository repository, IGroupRepository repositoryGroup)
        public UserMemberProvider(IUserRepository repository)
        {
            _repository = repository;
            //_repositoryGroup = repositoryGroup;
        }
        public User User
        //public IList<User> Users
        {
            get;
            private set;
        }
        public User CreateUser(string fullName, string passWord, string email)
        {
            return (null);
        }
        
        [Transaction(DataGlobals.ACCOUNT_DB_FACTORY_KEY)]
        public override bool ValidateUser(string username, string password)
        {
            if(string.IsNullOrEmpty(password.Trim())) return false;

            String[] userInfo = username.Split(new char[]{'\\'});
            
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            ///*
            if (userInfo.Length > 2)
            {
                //queryParams.Add("UserInfo.Domain", userInfo[0].ToUpper());
                queryParams.Add("UserInfo.UserName", userInfo[1]);
                queryParams.Add("ArmId", userInfo[2]);
            }
            else
            {
                //int userId;
                //if (int.TryParse(userInfo[0], out userId))
                //    queryParams.Add("UserId", userId);
                //else
                //    return false;
                    queryParams.Add("UserInfo.UserId", userInfo[0]);

                queryParams.Add("UserInfo.Password", password);
                queryParams.Add("ArmId", userInfo[1]);
            }
             //*/
            //queryParams.Add("[in]ArmId", pApplicationName);
            //string hash = EncryptPassword(password);
            //User user = _repository.GetByUserName(username);
            //User user = _repository.FindOne(queryParams);

            IList<User> users = _repository.GetByCriteriaIgnoreCase(queryParams);
            if (users == null || users.Count == 0) return false;
            //if (user.Password == hash)
            else
            {
                users[0].UserInfo.UserName = "" + users[0].UserInfo.UserId + "\\" + queryParams["ArmId"];
                HttpContext.Current.Session["auth_user_name"] = "" + users[0].UserInfo.UserId + "\\" + queryParams["ArmId"]; 
                User = users[0];
                _repository.SaveDebugInfo("UserMemberProvider User.UserInfo.UserName", User.UserInfo.UserName);
                //Users = users;
                return true;
            }

            //return false;
        }
        /// <summary>
        /// Procuses an MD5 hash string of the password
        /// </summary>
        /// <param name="password">password to hash</param>
        /// <returns>MD5 Hash string</returns>
        protected string EncryptPassword(string password)
        {
            //we use codepage 1252 because that is what sql server uses
            byte[] pwdBytes = Encoding.GetEncoding(1252).GetBytes(password);
            byte[] hashBytes = System.Security.Cryptography.MD5.Create().ComputeHash(pwdBytes);
            return Encoding.GetEncoding(1252).GetString(hashBytes);
        }
    }
}
