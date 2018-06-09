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
    public class PeriodPrice : Entity
    {
        [DisplayName("Наименование")]
        public virtual string Name { get; set; }
        [DisplayName("Дата от")]
        public virtual DateTime DTN { get; set; }
        [DisplayName("Дата до")]
        public virtual DateTime DTK { get; set; }
        [DisplayName("Месяцев")]
        public virtual int Month { get; set; }
    }
}
