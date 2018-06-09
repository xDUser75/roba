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
    public class MatPersonCardContent : Entity, IComparable<MatPersonCardContent>
    {
        public MatPersonCardContent()
        {
        }

        public MatPersonCardContent(int id)
        {
            this.Id = id;
        }

        public virtual int CompareTo(MatPersonCardContent obj)
        {
            return this.Storage.Nomenclature.Name.CompareTo(obj.Storage.Nomenclature.Name);
        }

        [ScaffoldColumn(false)]
        public override int Id { get; protected set; }
        public virtual Operation Operation { get; set; }
        [DisplayName("На руках")]
        [DataType("Integer")]
        public virtual int Quantity { get; set; }
        public virtual Storage Storage { get; set; }
        public virtual OperType OperType { get; set; }
        public virtual MatPersonCardHead MatPersonCardHead { get; set; }
        [DisplayName("Выдача")]
        [DataType("Date")]
        public virtual DateTime OperDate { get; set; }
        [DisplayName("Износ")]
        public virtual string Wear { get; set; }

    }
}
