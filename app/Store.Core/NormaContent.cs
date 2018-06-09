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
    public class NormaContent : Entity, IComparable<NormaContent>
    {
       public NormaContent()
       {
       }
       public NormaContent(int Id)
       {
           this.Id = Id;
       }
       public NormaContent(int Id, int Quantity, int UsePeriod)
       {
           this.Id = Id;
           this.Quantity = Quantity;
           this.UsePeriod = UsePeriod;
       }

       public virtual int CompareTo(NormaContent obj)
       {
           return this.NomGroup.Name.CompareTo(obj.NomGroup.Name);
       }

       [ScaffoldColumn(false)]
        public virtual int NormaId { get; set; }

        [Required]
        [DisplayName("Группа номенклатуры")]
        [UIHint("nomGroupActiveTemplate")]
        public virtual NomGroup NomGroup {get; set;}
        [DisplayName("По норме")]
        [DataType("Integer")]
        public virtual int Quantity { get; set; }
        [DisplayName("По ТОН")]
        [DataType("Integer")]
        public virtual int QuantityTON { get; set; }

        [DisplayName("Период использования")]
        [DataType("Integer")]
        public virtual int UsePeriod { get; set; }
        public virtual IList<NormaNomGroup> NormaNomGroups{ get; set; }
        public virtual bool InShop { get; set; }

        public virtual string NormaContentInfo { get; set;}
        public virtual bool IsApproved { get; set; }
        [DisplayName("Активный/Не активный")]
        public virtual bool IsActive { get; set; }

    }
}
