using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Core.DomainModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Store.Core
{
    //public class AM_SAPCompositeId : ValueObject
    //{
    //    public virtual string OBJID { get; set; }
    //}
    [Serializable()]
    public class PLAN_SAP : Entity
    {
        public virtual string plandata { get; set; }
        public virtual string shopnumber { get; set; }
        public virtual string shopname { get; set; }
        public virtual int nomid { get; set; }
        public virtual string nomname { get; set; }
        public virtual int quantitynorma { get; set; }
        public virtual string unit { get; set; }
        public virtual string unit_eng { get; set; }
        public virtual string dt { get; set; }
        public virtual int groupid { get; set; }
        public virtual string groupname { get; set; }
        public virtual int sizenumber { get; set; }
        public virtual int growth { get; set; }
        public virtual int sexid { get; set; }
        public virtual string sex { get; set; }
        public virtual int storagenameid { get; set; }
        public virtual string storagename { get; set; }
    }

}
