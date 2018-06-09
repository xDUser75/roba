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
    //[KnownType(typeof(Operation))]
    [Serializable()]
    public class Message : Entity
    {
        public Message() {
            this.MessDate = DateTime.Now;
        }

        [DisplayName("Дата")]
        [DataType(DataType.Date)]
        public virtual DateTime MessDate{ get; set; }

        [DisplayName("Сообщение")]
        public virtual string MessageText{ get; set; }

        [DisplayName("Активный/Не активный")]
        public virtual bool IsActive { get; set; }

        [DisplayName("Организация")]
        public virtual int OrganizationId { get; set; }

    }
}
