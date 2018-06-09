using System;
using System.Collections;
using Store.Core;
using System.Collections.Generic;
using SharpArch.Core.DomainModel;

namespace Store.Core.Account
{
    [Serializable()]
    public class GroupCompositeId : ValueObject
    {
        public virtual int RoleId { get; set; }
        //public virtual int UserId { get; set; }
        public virtual string ArmId { get; set; }
    }

    //public class Group : EntityWithTypedId<GroupCompositeId>, IHasAssignedId<GroupCompositeId>
    [Serializable()]
    public class Group : EntityWithTypedId<GroupCompositeId>
    //public class Group : Entity
    {
        //public Role()
        //{
        //    Rights = new List<Right>();
        //}
        /*
        public Role(int userId, string armId, int roleId, string roleDescription)
        {
            UserId = userId;
            ArmId = armId;
            RoleId = roleId;
            Description = roleDescription;
        }
         //*/
        public virtual int GroupId { get; set; }
        public virtual int UserId { get; set; }
        public virtual string ArmId { get; set; }
        public virtual string Description { get; set; }
        public virtual IList<Access> Accesses { get; set; }
        public virtual IList<Org> Orgs { get; set; }
        public virtual User User { get; set; }
        //public virtual ISet<Right> Rights { get; set; }
        /*
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var t = obj as Role;
            if (t == null)
                return false;
            if (this.ArmId == t.ArmId && this.RoleId == t.RoleId)
                return true;
            return false;

        }
        */
        //public virtual void SetAssignedIdTo(GroupCompositeId assignedId)
        //{
        //    this.Id = assignedId;
        //}
    }
}

