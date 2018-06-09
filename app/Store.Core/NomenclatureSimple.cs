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
    public class NomenclatureSimple : Entity
    {
        public NomenclatureSimple()
        {
        }

        public NomenclatureSimple(int Id)
        {
            this.Id = Id;
        }
        public virtual void setId(int value) {
            this.Id = value;
        }

        [DisplayName("Номенклатура")]
        public virtual string Name { get; set; }
        public virtual int NameId { get; set; }

        [DisplayName("Номенклатура")]
        public virtual string GroupName { get; set; }
        public virtual int GroupId { get; set; }

        [DisplayName("Кол-во")]
        public virtual int Quantity { get; set; }

        public virtual int? SizeId { get; set; }
        [DisplayName("Размер")]
        public virtual int? SizeName { get; set; }
        public virtual int? GrowthId { get; set; }
        [DisplayName("Рост")]
        public virtual int? GrowthName { get; set; }
        public virtual int? NomBodyPartId { get; set; }
        public virtual string DocNumber { get; set; }
        [DisplayName("Износ")]
        public virtual string Wear { get; set; }
        public virtual string WearId { get; set; }
        public virtual int OperType { get; set; }
        public virtual int StorageNameId { get; set; }
        public virtual string StorageNumber { get; set; }
        public virtual string StartDateStr { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime OperDate { get; set; }
        public virtual string OperDateStr { get; set; }
        public virtual int? NormaContentId { get; set; }
        public virtual string NormaContentName { get; set; }
        public virtual int? WorkerCardHeadId { get; set; }
    }
}
