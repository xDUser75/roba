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
    public class Unit : Entity, IComparable<Unit>
    {
        [ScaffoldColumn(false)]
        public override int Id { get; protected set; }
        [Required]
        [DisplayName("Организация")]
        public virtual Organization Organization { get; set; }
        [DisplayName("Наименование")]
        public virtual string Name { get; set; }
        public virtual string ExternalCode { get; set; }
        public Unit()
        {
        }
        public Unit(int Id, string Name)
        {
            this.Id = Id;
            this.Name = Name;
        }

        public virtual int CompareTo(Unit obj)
        {
            return this.Name.CompareTo(obj.Name);
        }

    }
}
