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
    public class MoveStorageOutSimple : Entity
    {
        public MoveStorageOutSimple()
        {
        }

        public MoveStorageOutSimple(int Id)
        {
            this.Id = Id;
        }


        [DisplayName("Наименование группы")]
        public virtual string GroupName { get; set; }

        [DisplayName("Номенклатура")]
        public virtual string Nomenclature { get; set; }
        
        [DisplayName("Ед. изм.")]
        public virtual string Unit { get; set; }

        [DisplayName("Кол-во")]
        public virtual int Quantity { get; set; }

        [DisplayName("Списать")]
        public virtual int OutQuantity { get; set; }
        
    }
}
