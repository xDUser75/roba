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
    public class NormaOrganizationSimple : Entity
    {
        public NormaOrganizationSimple()
        {
        }
        public NormaOrganizationSimple(int Id)
        {
            this.Id = Id;
        }


        [ScaffoldColumn(false)]
        public override int Id { get; protected set; }
        [DisplayName("Норма")]
        public virtual Norma Norma { get; set; }
        public virtual int OrganizationId{ get; set; }
        [DisplayName("Рабочее место")]
        public virtual string OrganizationFullName { get; set; }
        [DisplayName("Активный/Не активный")]
        public virtual bool IsActive { get; set; }

        [DisplayName("Шифр рабочего места")]
        public virtual string Sort{ get; set; }

    }
}
