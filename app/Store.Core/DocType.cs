﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Core.DomainModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Store.Core
{
    [Serializable()]
    public class DocType : Entity
    {
        [ScaffoldColumn(false)]
        public override int Id { get; protected set; }
        [DisplayName("Наименование")]
        public virtual string Name { get; set; }

        public DocType()
        {
        }
        public DocType(int Id, string Name)
        {
            this.Id = Id;
            this.Name = Name;
        }
    }
}
