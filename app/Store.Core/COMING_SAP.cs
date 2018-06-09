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
    public class COMING_SAP : Entity
    {
        public COMING_SAP() { }
        public COMING_SAP(int id)
        {
            this.Id = id;
        }

        [ScaffoldColumn(false)]
        public override int Id { get; protected set; }
        public virtual int DocTypeId { get; set; }
        public virtual string DocNumber { get; set; }
        public virtual string DocDate { get; set; }
        //{
        //    get
        //    { return Utils.ConvertTypes.GalToSharpDate(GalDocDate).ToString("dd.MM.yyyy"); }
        //}
        public virtual string MaterialId { get; set; }
        [Required]
        [DisplayName("Номенклатура")]        
        public virtual string MATERIAL { get; set; }
        public virtual string ExternalCode { get; set; }
        [DisplayName("Кол-во")]        
        public virtual int? QUANTITY { get; set; }
        public virtual string UOM { get; set; }
        public virtual string LC { get; set; }
        public virtual string SV { get; set; }

        [DisplayName("Пол")]
        [UIHint("SapSexTemplate"), Required]
        public virtual int? SexId { get; set; }
        public virtual string SexName { get; set; }
        [Required]
        [DisplayName("Ед. изм.")]
        public virtual int? UnitId { get; set; }
        public virtual string UnitExternalCode { get; set; }
        public virtual string UnitName { get; set; }
        [Required]
        [DisplayName("Группа")]
        public virtual int NomGroupId { get; set; }
        public virtual string NomGroupName { get; set; }
        public virtual string SAPNomGroupId { get; set; }
        public virtual string SAPNomGroupName { get; set; }
        [Required]
        [DisplayName("Часть тела")]
        public virtual int? NomBodyPartId { get; set; }
        public virtual string NomBodyPartName { get; set; }
        [DisplayName("Зима")]
        public virtual bool? IsWinter { get; set; }
        public virtual string MoveType { get; set; }
        public virtual int? SizeId { get; set; }
        public virtual string SizeName { get; set; }
        public virtual int? GrowthId { get; set; }
        public virtual string GrowthName { get; set; }
        public virtual string StorageNameExternalCode { get; set; }
        public virtual string StorageName { get; set; }
        //public virtual int GalDocDate { get; set; }
        public virtual double Price { get; set; }


   }
}
