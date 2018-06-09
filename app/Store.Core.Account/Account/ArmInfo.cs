using System;

namespace Store.Core.Account
{
    [Serializable()]
    public class ArmInfo
    {
        protected ArmInfo() { }
        public ArmInfo(string Id, string Name)
        {
            this.Id = Id;
            this.Name = Name;
        }

        public virtual string Id { get; set; }
        public virtual string Name { get; set; }
    }
}
