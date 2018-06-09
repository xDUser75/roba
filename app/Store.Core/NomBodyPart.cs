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
    public class NomBodyPart : Entity, IComparable<NomBodyPart>
    {
        public NomBodyPart()
        {
        }

        public NomBodyPart(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
        //[RenderMode(RenderMode.DisplayModeOnly)] 
        //public override int Id { get; protected set; }
        [DisplayName("Наименование")]
        [DataType(DataType.Text)]
        [Required]
        public virtual string Name { get; set; }

        public virtual int CompareTo(NomBodyPart obj)
        {
            return this.Name.CompareTo(obj.Name);
        }

    }


}
