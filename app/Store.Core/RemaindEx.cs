using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Core.DomainModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace Store.Core
{
    [Serializable()]
    public class RemaindEx : Remaind
    {
        public RemaindEx(int id) :base(id) { }
        public RemaindEx() { }
        
        public virtual int RemaindId { get; set; }
        [DisplayName("Кол-во B/C")]
        public virtual int QuantityE { get; set; }
    }
}
