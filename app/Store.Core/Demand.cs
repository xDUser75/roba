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
    public class Demand : Entity
    {
       public Demand()
       {
       }
       public Demand(int Id)
       {
           this.Id = Id;
       }
       public Demand(int Id, NomGroup NomGroup, int Quantity, int UsePeriod)
       {
           this.Id = Id;
           this.NomGroup = NomGroup;
           this.Quantity = Quantity;
           this.UsePeriod = UsePeriod;
       }
        
        [ScaffoldColumn(false)]
        public virtual int SingleDemandsId { get; set; }
        [Required]
        [DisplayName("Группа номенклатуры")]
        [UIHint("nomGroupTemplate")]
        public virtual NomGroup NomGroup { get; set; }
        [DisplayName("По норме")]
        [DataType("Integer")]
        public virtual int Quantity { get; set; }
        [DisplayName("Период использования")]
        public virtual int UsePeriod { get; set; }

    }
}
