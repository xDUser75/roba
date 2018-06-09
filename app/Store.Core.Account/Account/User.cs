using System.Security.Principal;
using System.Collections.Generic;
using SharpArch.Core.DomainModel;
using System;


namespace Store.Core.Account
{
    [Serializable()]
    public class UserCompositeId : ValueObject
    {
        public virtual string ArmId { get; set; }
        public virtual int UserId { get; set; }
    }

    [Serializable()]
    public class User : EntityWithTypedId<UserCompositeId>, IPrincipal
    {
        /*
        protected User() { }
        public User(int userId, string userName, string fullName, string password)
        {
            UserId = userId;
            UserName = userName;
            FullName = fullName;
            Password = password;
        }
         */
        //public virtual int UserId { get; set; }
        public virtual string ArmId { get; set; }
        public virtual ArmInfo ArmInfo { get; set; }
        public virtual UserInfo UserInfo { get; set; }

        //public virtual Role Role { get; set; }

        //public virtual AccessApp AccessApp { get; set; }
        public virtual IList<Group> Roles { get; set; }
        
        public virtual IIdentity Identity { get; set; }

        public virtual IList<int> Organizations
        {
            get
            {
                IList<int> result = new List<int>();
                foreach (Group curRole in this.Roles)
                {
                    foreach (Org curOrg in curRole.Orgs)
                    {
                        result.Add(curOrg.Id.OrgId);
                    }
                }
                return result;
            }
        }

        public virtual string ObjectsCSV
        {
            get
            {
                string result = "";
                foreach (Group curRole in this.Roles)
                {
                    foreach (Org curOrg in curRole.Orgs)
                    {
                        result += "," +curOrg.Id.OrgId;
                    }
                }
                return result.Substring(1);
            }
        }
        /*
        public virtual bool IsInRole(string role)
        {
            if (Role.Description.ToLower() == role.ToLower())
                return true;
            foreach (Right right in Role.Rights)
            {
                if (right.Description.ToLower() == role.ToLower())
                    return true;
            }
            return false;
        }
        //*/
        ///*
        public virtual bool IsInRole(string role)
        {
            foreach (Group curRole in this.Roles)
            {
                if (curRole.Description.ToLower() == role.ToLower())
                    return true;
            }
            return false;
        }
        // */
    }
}
