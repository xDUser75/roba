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
    public class SignTypes : Entity
    {
        public SignTypes()
        {
        }

        public SignTypes(int id)
        {
            this.Id = id;
        }

       // [ScaffoldColumn(false)]
        //public override int Id { get; protected set; }

        [Required]
        [DisplayName("Код подписи")]
        public virtual string Code { get; set; }

        [DisplayName("Наименование подписи")]
        public virtual string Name { get; set; }

    }
}
