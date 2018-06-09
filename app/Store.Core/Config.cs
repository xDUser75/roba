using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Core.DomainModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Store.Core
{
    [Serializable()]
    public class Config : Entity
    {
        //public override int Id { get; protected set; }
        public virtual string ParamName { get; set; }
        public virtual string ParamValue { get; set; }
        public virtual string Description { get; set; }
        public virtual int OrganizationId { get; set; }

    }
}
