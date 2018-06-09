using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Core.DomainModel;


namespace Store.Core.Account
{
   [Serializable()]
    public class AccessCompositeId : ValueObject
    {
        //[DomainSignature]
        public virtual int RoleId { get; set; }
        public virtual string ArmId { get; set; }
        public virtual int ModuleId { get; set; }
        public virtual int AccessId { get; set; }
    }

    //public class Access : EntityWithTypedId<AccessCompositeId>, IHasAssignedId<AccessCompositeId>
   [Serializable()]
   public class Access : EntityWithTypedId<AccessCompositeId>
    //public class Access : Entity
    {
        //public Right() { }
        /*
        public Right(int rightId, string armId, int roleId, string description)
        {
            RightId = rightId;
            ArmId = armId;
            RoleId = roleId;
            Description = description;
        }
         */
        public virtual int AccessId { get; set; }
        public virtual int ModuleId { get; set; }
        public virtual string RoleId { get; set; }
        public virtual string ArmId { get; set; }
        public virtual string Description { get; set; }
        public virtual Group Group { get; set; }
        /*
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var t = obj as Right;
            if (t == null)
                return false;
            if (this.RightId == t.RightId)
                return true;
            return false;

        }
         */
        //public virtual void SetAssignedIdTo(AccessCompositeId assignedId)
        //{
        //    this.Id = assignedId;
        //}
    }
}
