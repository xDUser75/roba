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
    public class WorkerCardHead : Entity
    {
        public WorkerCardHead()
        {
        }

        public WorkerCardHead(int id)
        {
            this.Id = id;
        }

        [ScaffoldColumn(false)]
        public override int Id { get; protected set; }

        public virtual WorkerWorkplace WorkerWorkplace { get; set; }

        //public virtual NormaOrganization NormaOrganization { get; set; }

        public virtual IList<WorkerCardContent> WorkerCardContents {get; set; }

        public virtual bool removeWorkerCardContent(WorkerCardContent wcc)
        {
            return WorkerCardContents.Remove(wcc);
        }

        public virtual IList<WorkerCardContent> getActiveWorkerCardContent()
        {
            IList<WorkerCardContent> activeList=new List<WorkerCardContent>() ;
            foreach (WorkerCardContent item in WorkerCardContents){
                if (item.Quantity > 0) {
                    activeList.Add(item);
                }
            }
            return activeList;
        }
    }
}
