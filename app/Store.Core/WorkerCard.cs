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
    public class WorkerCard : Entity
    {
        public WorkerCard()
        {
        }

        public WorkerCard(int id)
        {
            this.Id = id;
        }

        [ScaffoldColumn(false)]
        public override int Id { get; protected set; }

        public virtual WorkerWorkplace WorkerWorkplace { get; set; }

        //public virtual NormaContent NormaContent { get; set; }
        //public virtual NormaOrganization NormaOrganization { get; set; }
        //public virtual IList<NormaContent> NormaContents { get; set; }
        public virtual Storage Storage { get; set; }
        public virtual Operation Operation { get; set; }
        [DisplayName("На руках")]
        [DataType("Integer")]
        public virtual int Quantity { get; set; }

    }
}
