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
    public class NomBodyPartSize : Entity
    {
        public NomBodyPartSize()
        {
        }

        public NomBodyPartSize(int id)
        {
            this.Id = id;
        }

        public virtual NomBodyPart NomBodyPart { get; set; }

        [DisplayName("Размер")]
        public virtual string SizeNumber { get; set; }
        [DisplayName("Активный/Не активный")]
        public virtual bool IsActive { get; set; }
        public virtual string SizeString { get; set; }

    }


}
