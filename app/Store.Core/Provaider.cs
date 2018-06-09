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
    public class Provaider : Entity
    {

        public Provaider() { }
        public Provaider(int Id) 
        {
            this.Id = Id;
        }

        public virtual int OrganizationId { get; set; }

        [Required]
        [DisplayName("Наименование")]
        public virtual string Name{ get; set; }

        [Required]
        [DisplayName("Город")]
        public virtual string City{ get; set; }

        [Required]
        [DisplayName("Производитель")]
        public virtual string Producer { get; set; }

        public virtual string ProvaiderInfo 
        {
            get
            {
                return this.Name + " " + this.Producer;
            }
        }

    }
}
