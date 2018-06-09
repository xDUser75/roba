using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Core.DomainModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;

namespace Store.Core
{
    [Serializable()]
    public class DocumentNumber : Entity
    {
        public virtual int OperationId{ get; set; }
        public virtual int DocNumber { get; set; }
        public virtual int Year { get; set; }
        public virtual int OrganizationId { get; set; }
    }
}
