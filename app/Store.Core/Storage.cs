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
    //[HasSelfValidation]
    [Serializable()]
    public class Storage : Entity
    {
        public Storage()
        {
        }

        public Storage(int id)
        {
            this.Id = id;
        }

        public override int Id { get; protected set; }

        [DisplayName("Склад")]
        public virtual StorageName StorageName { get; set; }

        [DisplayName("Номенклатура")]
        [UIHint("NomTemplate")]
        public virtual Nomenclature Nomenclature { get; set; }
        
        [DisplayName("Цена")]
        [ScriptIgnore]
        public virtual double Price { get; set; }
        
        [DisplayName("Кол-во")]
        [Range(0, 10000, ErrorMessage="Кол-во должно быть больше 0")]
        public virtual int Quantity { get; set; }
        
        [DisplayName("Износ")]
        public virtual string Wear { get; set; }

        [DisplayName("Размер")]
        public virtual NomBodyPartSize NomBodyPartSize{ get; set; }

        [DisplayName("Рост")]
        public virtual NomBodyPartSize Growth { get; set; }

        public virtual string StorageInfo
        {
            get
            {
                return Nomenclature != null && Nomenclature.Name != null ? "[" + Nomenclature.ExternalCode + "] " + Nomenclature.Name + " (" + (NomBodyPartSize != null ? NomBodyPartSize.SizeNumber : "...") + " " + (Growth != null ? Growth.SizeNumber : "...") + " " + (Wear.Equals("100")?Wear:"<span style=\"color:red\"><b>"+Wear+"</b></span>") + /*" " + Price +*/ " " + Quantity + ")" : "";
            }
        }

        public virtual string NomenclatureInfo
        {
            get
            {
                return Nomenclature != null && Nomenclature.Name != null ? "[" + Nomenclature.ExternalCode + "] " + Nomenclature.Name + " (" + (NomBodyPartSize != null ? NomBodyPartSize.SizeNumber : "...") + " " + (Growth != null ? Growth.SizeNumber : "...") + " " +  Wear  + /*" " + Price +*/ " " + Quantity + ")" : "";
            }
        }


    }
}
