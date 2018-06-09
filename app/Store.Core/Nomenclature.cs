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
    public class Nomenclature : Entity, IComparable<Nomenclature>
    {
        public Nomenclature()
        {
        }

        public Nomenclature(int Id)
        {
            this.Id = Id;
        }

        public virtual int CompareTo(Nomenclature obj)
        {
            return this.Name.CompareTo(obj.Name);
        }

        //[ScaffoldColumn(false)]
        //public override int Id { get; protected set; }

        [DisplayName("Организация")]
        public virtual Organization Organization { get; set; }

        [DisplayName("Наименование")]
        public virtual string Name { get; set; }
        
        [DisplayName("Единица измерения")]
        public virtual Unit Unit { get; set; }
        
        [DisplayName("Дата ввода в эксплуатацию")]
        [DataType(DataType.Date)]
        public virtual DateTime StartDate { get; set; }
        
        [DisplayName("Дата вывода из эксплуатации")]
        [DataType("Date")]
        public virtual DateTime FinishDate { get; set; }
        
        [DisplayName("Активный/Не активный")]
        public virtual bool IsActive { get; set; }

        [DisplayName("Закуп")]
        public virtual bool Enabled { get; set; }
        
        [DisplayName("Код ВС")]
        public virtual string ExternalCode { get; set; }
        
        [DisplayName("Пол")]
        public virtual Sex Sex { get; set; }
        //[UIHint("NomBodyPartTemplate1"), Required]
        
        public virtual NomBodyPart NomBodyPart { get; set; }
        public virtual NomBodyPartSize NomBodyPartSize { get; set; }
        public virtual NomBodyPartSize Growth { get; set; }
        //[UIHint("NomGroupTemplate"), Required]
        public virtual NomGroup NomGroup { get; set; }
    }
}
