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
    public class NomGroupPrice : Entity
    {
        public NomGroupPrice()
        {
        }

        public NomGroupPrice(int Id) {
            this.Id = Id;
        }

        [DisplayName("Организация")]
        public virtual int OrganizationId { get; set; }
        [DisplayName("Группа")]
        public virtual NomGroup NomGroup { get; set; }
        [DisplayName("Период")]
        public virtual PeriodPrice PeriodPrice { get; set; }
        [DisplayName("Цена")]
        public virtual double Price { get; set; }
        [DisplayName("Код В/С")]
        public virtual string ExternalCode { get; set; }
    }
}
