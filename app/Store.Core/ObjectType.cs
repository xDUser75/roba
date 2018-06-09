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
    public class ObjectType : Entity
    {
        [ScaffoldColumn(false)]
        public override int Id { get; protected set; }
        [Required]
        [DisplayName("Наименование")]
        public virtual string Name { get; set; }
        [DisplayName("имя таблицы")]
        public virtual string TableName { get; set; }

    }
}
