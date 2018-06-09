using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Core.DomainModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;

namespace Store.Core
{
    //[KnownType(typeof(Operation))]
    [Serializable()]
    public class OperationSimple : Entity
    {
        public virtual string OperType { get; set; }

        [DisplayName("Дата операции")]
        [DataType(DataType.Date)]
        public virtual DateTime OperDate{ get; set; }

        [DisplayName("Тип документа")]
        public virtual string DocType { get; set; }

        [DisplayName("Цех")]
        public virtual string ShopNumber { get; set; }

        [DisplayName("Таб ФИО")]
        public virtual string Fio { get; set; }       

        [DisplayName("Дата документа")]
        [DataType(DataType.DateTime)]
        public virtual DateTime DocDate { get; set; }       
        
        [DisplayName("Номер документа")]
        public virtual string DocNumber{ get; set; }
        
        [DisplayName("Кол-во")]
        [DataType("Integer"), Required]
        public virtual int Quantity { get; set; }
        
        [DisplayName("Годность")]
        public virtual string Wear { get; set; }

        [DisplayName("Номенклатура")]
        public virtual string Nomenclature { get; set; }

        //[DisplayName("Основание")]
        public virtual string Motiv { get; set; }

        public virtual int OrganizationId { get; set; }
        public virtual int OperTypeId { get; set; }
        public virtual int ShopId { get; set; }
        public virtual int NomenclatureId { get; set; }
        public virtual int StorageNameId { get; set; }
        public virtual string StorageNumber { get; set; }
        public virtual int WorkerWorkPlaceId { get; set; }
        public virtual int WorkerId { get; set; }
        public virtual int MolId { get; set; }
        public virtual int RefOperationId { get; set; }
        public virtual int GiveOperationId { get; set; }

        public virtual String StringOperDate
        {
            get
            {
                return OperDate.ToString("dd.MM.yyyy");
            }
        }
    }
}
