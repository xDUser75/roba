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
    public class SignDocTypes : Entity
    {
        public SignDocTypes()
        {
        }

        public SignDocTypes(int id)
        {
            this.Id = id;
        }

        //[ScaffoldColumn(false)]
        //public override int Id { get; protected set; }

        [Required]
        [DisplayName("Код типа документа")]
        public virtual string Code { get; set; }

        [DisplayName("Наименование типа документа")]
        public virtual string Name { get; set; }

        [DisplayName("Наименование типа документа")]
        public virtual string Info
        {
            get
            {
                return this.Code + " - " + this.Name;
            }
        }
    }
}
