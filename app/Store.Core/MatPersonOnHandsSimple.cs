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
    public class MatPersonOnHandsSimple : Entity
    {

        public MatPersonOnHandsSimple() { }
        public MatPersonOnHandsSimple(int id)
        {
            this.Id = id;
        }

        [DisplayName("Номенклатура")]
        public virtual String Nomenclature { get; set; }
        [DisplayName("Код ВС")]
        public virtual String ExternalCode { get; set; }
        public virtual int NomenclatureId { get; set; }
        [DisplayName("На руках")]
        [DataType("Integer")]
        public virtual int Quantity { get; set; }
        public virtual string Wear { get; set; }
        [DisplayName("Кол-во")]
        [DataType("Integer")]
        public virtual int WorkQuantity { get; set; }
        public virtual string LastDocNumber { get; set; }
        public virtual DateTime LastDocDate { get; set; }
        public virtual string LastOperTypeId { get; set; }

    }
}
