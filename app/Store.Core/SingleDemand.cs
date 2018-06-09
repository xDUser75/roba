using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Core.DomainModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Store.Core;

namespace Store.Core
{
    [Serializable()]
    public class SingleDemand : Entity
    {
        public SingleDemand()
        {
        }

        public SingleDemand(int Id)
        {
            this.Id = Id;
        }

        public virtual DocType Doctype { get; set; }
        [DisplayName("Номер документа")]
        public virtual string DoctNumber { get; set; }
        [DataType(DataType.DateTime)]
        public virtual DateTime DocDate { get; set; }
        public virtual WorkerWorkplace WorkerWorkplace { get; set; }

    }
}
