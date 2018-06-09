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
    public class Remaind : Entity
    {
        public Remaind() { }
        public Remaind(int id) { this.Id = id; }

        [DisplayName("Дата")]
        public virtual DateTime RemaindDate { get; set; }
        public virtual StorageName StorageName { get; set; }
        public virtual Storage Storage { get; set; }
        public virtual Nomenclature Nomenclature { get; set; }
        [DisplayName("Кол-во")]
        public virtual int Quantity { get; set; }
        [DisplayName("Износ")]
        public virtual int Wear { get; set; }
        [DisplayName("Размер")]
        public virtual NomBodyPartSize NomBodyPartSize { get; set; }
        [DisplayName("Рост")]
        public virtual NomBodyPartSize Growth { get; set; }
        [ScriptIgnore]
        public virtual DateTime ActualDate { get; set; }
        [DisplayName("Рост")]
        public virtual String StringWear {
            get{
                return this.Wear == 100 ? "новая" : this.Wear == 50 ? "б/у" : this.Wear == 0 ? "утиль" : ""+this.Wear;
            }
        }

    }
}
