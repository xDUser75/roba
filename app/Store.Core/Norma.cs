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
    public class Norma : Entity
    {
        public Norma()
        {
        }

        public Norma(int Id, string Name, Organization Organization)
        {
            this.Id = Id;
            this.Organization = Organization;
            this.Name = Name;
        }
        public Norma(int Id)
        {
            this.Id = Id;
        }
//        [Required]
        [DisplayName("Организация")]
        [UIHint("organizationTemplate")]
        public virtual Organization Organization { get; set; }
        [DisplayName("Наименование")]
        public virtual string Name { get; set; }
        [DisplayName("Содержание нормы")]
        public virtual IList<NormaContent> NormaContents { get; set; }
        [DisplayName("Активный/Не активный")]
        public virtual bool IsActive { get; set; }
        [DisplayName("Комментарии")]
        public virtual string NormaComment { get; set; }
        [DisplayName("Утверждена")]
        public virtual bool IsApproved { get; set; }

        public virtual Norma rebuild()
        {
            Norma outNorma = new Norma(this.Id);
            outNorma.Name = this.Name;
            return outNorma;
        }
    }
}
