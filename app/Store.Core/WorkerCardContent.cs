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
    public class WorkerCardContent : Entity, IComparable<WorkerCardContent>
    {
        public WorkerCardContent()
        {
        }

        public WorkerCardContent(int id)
        {
            this.Id = id;
        }

        public virtual int CompareTo(WorkerCardContent obj)
        {
            return this.Storage.Nomenclature.Name.CompareTo(obj.Storage.Nomenclature.Name);
        }

        [ScaffoldColumn(false)]
        public override int Id { get; protected set; }

        public virtual Storage Storage { get; set; }

        public virtual Operation Operation { get; set; }

        public virtual NormaContent NormaContent { get; set; }

        public virtual WorkerCardHead WorkerCardHead { get; set; }

        [DisplayName("На руках")]
        [DataType("Integer")]
        public virtual int Quantity { get; set; }

        [DisplayName("Выдача")]
        [DataType("Date")]
        public virtual DateTime StartDate { get; set; }
        public virtual Operation GiveOperation { get; set; }

        [DisplayName("Возврат")]
        [DataType("Date")]
        public virtual DateTime EndDate { get; set; }

        public virtual Operation OperReturn { get; set; }
        [DisplayName("Корпоративный")]
        public virtual bool IsCorporate { get; set; }
        public virtual int OperationId { get; set; }
        public virtual int WorkerCardContentId { get; set; }
        [DisplayName("Период использования")]
        [DataType("Integer")]
        public virtual int UsePeriod { get; set; }


    }
}
