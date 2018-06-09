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
    public class MatPersonOnHands : Entity, IComparable<MatPersonOnHands>
    {
        private  DateTime Null_Date = new DateTime(1, 1, 1, 0, 0, 0, 0);
        public virtual int CompareTo(MatPersonOnHands obj)
        {
//            return this.Storage.Nomenclature.Name.CompareTo(obj.Storage.Nomenclature.Name);
            return 0;
        }

        public virtual Nomenclature Nomenclature { get; set; }
        public virtual MatPersonCardHead MatPersonCardHead { get; set; }
        public virtual StorageName StorageName { get; set; }
        [DisplayName("Износ")]
        public virtual string Wear { get; set; }
        [DisplayName("На руках")]
        [DataType("Integer")]
        public virtual int Quantity { get; set; }

        public virtual string StorageInfo
        {
            get
            {
                return Nomenclature != null && Nomenclature.Name != null ? "[" + Nomenclature.ExternalCode + "] " + Nomenclature.Name + " (" + (Nomenclature.NomBodyPartSize != null ? Nomenclature.NomBodyPartSize.SizeNumber : "...") + " " + (Nomenclature.Growth != null ? Nomenclature.Growth.SizeNumber : "...") + " " + (Wear.Equals("100") ? Wear : "<span style=\"color:red\"><b>" + Wear + "</b></span>") + /*" " + Price +*/ " " + Quantity + ")" : "";
            }
        }

        public virtual string IdNomenclatureAndWear
        {
            get
            {
                return (Nomenclature != null?Nomenclature.Id.ToString():"") + '|' + Wear;
            }
        }

        public virtual string NomenclatureInfo
        {
            get
            {
                return Nomenclature != null && Nomenclature.Name != null ? "[" + Nomenclature.ExternalCode + "] " + Nomenclature.Name + " (" + (Nomenclature.NomBodyPartSize != null ? Nomenclature.NomBodyPartSize.SizeNumber : "...") + " " + (Nomenclature.Growth != null ? Nomenclature.Growth.SizeNumber : "...") + " " +  Wear + /*" " + Price +*/ " " + Quantity + ")" : "";
            }
        }

        public virtual string LastDocNumber { get; set; }
        public virtual string LastOperTypeId { get; set; }
        public virtual DateTime LastDocDate { get; set; }
        public virtual string LastDocDateAsString
        {
            get
            {
                return LastDocDate != Null_Date ? LastDocDate.ToString("dd.MM.yyyy"):"" ;
            }
        }


    }
}
