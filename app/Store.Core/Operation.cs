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
    public class Operation : Entity
    {
        public Operation() { }
        public Operation(int id)
        {
            this.Id = id;
        }
        [ScaffoldColumn(false)]
        public override int Id { get; protected set; }

        [UIHint("operTypeTemplate"), Required]
        public virtual OperType OperType { get; set; }

        [DisplayName("Дата операции")]
        [DataType(DataType.Date)]
        public virtual DateTime OperDate{ get; set; }

        [DisplayName("Тип документа")]
        public virtual DocType DocType { get; set; }       

        [DisplayName("Дата документа")]
        [DataType(DataType.DateTime)]
        public virtual DateTime DocDate { get; set; }       
        
        [DisplayName("Номер документа")]
        public virtual string DocNumber{ get; set; }
        
        //[DisplayName("Тип объекта операции")]
        //public virtual ObjectType ObjectType { get; set; }
        //[DisplayName("Код объекта операции")]
        //public virtual int ObjectValue { get; set; }
        
        [DisplayName("Кол-во")]
        [DataType("Integer"), Required]
        public virtual int Quantity { get; set; }
        
        [DisplayName("Годность")]
        public virtual string Wear { get; set; }

        //[DisplayName("Основание")]
        [UIHint("motivTemplate"), Required]
        public virtual Motiv Motiv { get; set; }

        public virtual Storage Storage { get; set; }

        public virtual StorageName StorageName { get; set; }
        public virtual StorageName Partner { get; set; }
        public virtual string Note { get; set; }

        //[ScriptIgnore]
        [DisplayName("Организация")]
        public virtual Organization Organization { get; set; }

        //[ScriptIgnore]
        public virtual WorkerWorkplace WorkerWorkplace { get; set; }
        [ScriptIgnore]
        public virtual Operation RefOperation { get; set; }
        [DisplayName("Корпоративный")]
        public virtual bool IsCorporate { get; set; }
        public virtual Cause Cause { get; set; }
        [ScriptIgnore]
        public virtual Operation TransferOperation { get; set; }
        [ScriptIgnore]
        public virtual Operation GiveOperation { get; set; }

    }
}
