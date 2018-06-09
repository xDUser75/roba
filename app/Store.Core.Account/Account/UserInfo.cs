using System;

namespace Store.Core.Account
{
    [Serializable()]
    public class UserInfo
    {
        protected UserInfo() { }

        public virtual string UserId { get; set; }
        public virtual string UserName { get; set; }
        public virtual string UserShortName
        {
            get
            { 
                int k = FullName.IndexOf(" ");
                string fio = FullName.Substring(0, k);
                string io = FullName.Substring(k + 1);
                int m = io.IndexOf(" ");
                string i = io.Substring(m + 1);
                fio = fio +" " + io.Substring(0, 1) + ". "+i.Substring(0,1)+".";
                return fio;
            }
        }
        public virtual string FullName { get; set; }
        public virtual string Password { get; set; }
        //public virtual string Domain { get; set; }
        public virtual string DomainName { get; set; }
    }
}
