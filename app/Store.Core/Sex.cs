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
    public class Sex : Entity, IComparable<Sex>
    {
        public Sex()
        {
        }

        public Sex(int id)
        {
            this.Id = id;
        }

        public Sex(int Id, string Name)
        {
            this.Id = Id;
            this.Name = Name;
        }
        //[ScaffoldColumn(false)]
        //public override int Id { get; protected set; }

        [DisplayName("Пол")]
        public virtual string Name { get; set; }

        public virtual int CompareTo(Sex obj)
        {
            return this.Name.CompareTo(obj.Name);
        }

    }
}
