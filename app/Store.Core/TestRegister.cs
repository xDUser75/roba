using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Core.DomainModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;

namespace Store.Core
{
    [Serializable()]
    public class TestRegister : Entity
    {
        public TestRegister() { }
        public TestRegister(int id) 
        {
            this.Id = id;
        }
        public virtual int OrganizationId { get; set; }

        [DisplayName("Поставщик")]
        public virtual Provaider Provaider { get; set; }

        [DisplayName("Группа номенклатур")]
        public virtual NomGroup NomGroup { get; set; }

        public virtual DateTime TestDate { get; set; }
        [DisplayName("Номенклатура")]
        public virtual String Model { get; set; }

    }
}
