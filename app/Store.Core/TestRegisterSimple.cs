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
    public class TestRegisterSimple : Entity
    {
        public virtual int OrganizationId { get; set; }
        public virtual int ProvaiderId { get; set; }
        [DisplayName("Поставщик")]
        public virtual string Provaider { get; set; }
        [DisplayName("Производитель")]
        public virtual string Producer { get; set; }
        [DisplayName("Город")]
        public virtual string City { get; set; }

        public virtual int NomGroupId { get; set; }
        [DisplayName("Группа номенклатур")]
        public virtual string NomGroup { get; set; }

        [DisplayName("Дата")]
        public virtual DateTime TestDate { get; set; }
        public virtual string TestDateString 
        {
            get 
            {
                return this.TestDate.ToString("dd.MM.yyyy");
            } 
        }
        [DisplayName("Номенклатура")]
        public virtual String Model { get; set; }

        public virtual String Color { get; set; }
        public virtual string ProvaiderInfo
        {
            get
            {
                return this.Provaider + " " + this.Producer;                            
            }
        }

    }
}
