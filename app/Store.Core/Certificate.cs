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
    public class Certificate : Entity
    {
        public Certificate() { }
        public Certificate(int id)
        {
            this.Id = id;
        }

        [DisplayName("Цех")]
        public virtual Organization Organization { get; set; }
        public virtual TestRegister TestRegister { get; set; }
        [DisplayName("Номер документа")]
        public virtual string DocNum { get; set; }
        [DisplayName("Дата документа")]
        public virtual DateTime DocDate { get; set; }
        public virtual string DocDateString
        {
            get
            {
                return this.DocDate.ToString("dd.MM.yyyy");
            }
        }

        [DisplayName("Примечание")]
        public virtual String Descr { get; set; }
        [DisplayName("Результат")]
        public virtual Result Result { get; set; }
        
        public virtual string Color {
            get 
            {
                if (this.Result == null) return "";
                else return this.Result.Color;
            }
        }
    }
}
