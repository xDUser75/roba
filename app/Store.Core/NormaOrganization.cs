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
    public class NormaOrganization : Entity
    {
        public NormaOrganization()
        {
        }
        public NormaOrganization(int Id)
        {
            this.Id = Id;
        }


        [ScaffoldColumn(false)]
        public override int Id { get; protected set; }
        [DisplayName("Норма")]
        public virtual Norma Norma { get; set; }
        [DisplayName("Рабочее место")]
        public virtual Organization Organization { get; set; }
        [DisplayName("Активный/Не активный")]
        public virtual bool IsActive { get; set; }

        public virtual NormaOrganization rebuild()
        {
            NormaOrganization outNormaOrganization = new NormaOrganization(this.Id);
            outNormaOrganization.Norma = this.Norma.rebuild();
            return outNormaOrganization;
        }
    }
}
