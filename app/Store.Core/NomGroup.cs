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
    public class NomGroup : Entity, IComparable<NomGroup>
    {
        public NomGroup(int Id, string Name, Organization Organization, NomBodyPart NomBodyPart, bool IsActive, string ExternalCode)
        {
            this.Id = Id;
            this.Name = Name;
            this.Organization = Organization;
            this.NomBodyPart = NomBodyPart;
            this.IsActive = IsActive;
            this.ExternalCode = ExternalCode;
            //NomgroupNomenclature = new List<NomgroupNomenclature>();
        }

        public NomGroup(int Id, string Name, NomBodyPart NomBodyPart)
        {
            this.Id = Id;
            this.Name = Name;
            this.NomBodyPart = NomBodyPart;
            //NomgroupNomenclature = new List<NomgroupNomenclature>();
        }

        public NomGroup(int Id, string Name)
        {
            this.Id = Id;
            this.Name = Name;
            //NomgroupNomenclature = new List<NomgroupNomenclature>();
        }
        public NomGroup(int Id)
        {
            this.Id = Id;
            //NomgroupNomenclature = new List<NomgroupNomenclature>();
        }

        public NomGroup()
        {
            //NomgroupNomenclature = new List<NomgroupNomenclature>();
        }
        
        public virtual int CompareTo(NomGroup obj)
        {
            return this.Name.CompareTo(obj.Name);
        }

       //[ScaffoldColumn(false)]
        //[Required]
        [DisplayName("Организация")]
        public virtual Organization Organization { get; set; }
        [DisplayName("Часть тела")]
        //[UIHint("NomBodyPartTemplate1"), Required]
        public virtual NomBodyPart NomBodyPart { get; set; }       
        [DisplayName("Наименование")]
        //[UIHint("nomGroupTemplate")]
        public virtual string Name { get; set; }
        [DisplayName("Активный/Не активный")]
        public virtual bool IsActive { get; set; }
        [DisplayName("Зимняя одежда")]
        public virtual bool IsWinter { get; set; }
        //public virtual IList<NomgroupNomenclature> NomgroupNomenclature { get; set; }
        public virtual IList<Nomenclature> Nomenclatures { get; set; }
        [DisplayName("Код BC")]
        public virtual string ExternalCode { get; set; }
        [DisplayName("Наименование охраны труда")]
        public virtual string NameOT { get; set; }

    }

}
