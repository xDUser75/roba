using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Core.DomainModel;


namespace Store.Core.Account
{
    [Serializable()]
    public class OrgCompositeId : ValueObject
    {
        public virtual int GroupId { get; set; }
        public virtual string ArmId { get; set; }
        public virtual int OrgId { get; set; }
    }

    [Serializable()]
    public class Org : EntityWithTypedId<OrgCompositeId>
    {
        public virtual Group Group { get; set; }
    }
}
