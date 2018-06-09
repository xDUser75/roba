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
    public class MatPersonCardHead : Entity
    {
        public MatPersonCardHead()
        {
        }

        public MatPersonCardHead(int id)
        {
            this.Id = id;
        }

        [ScaffoldColumn(false)]
        public override int Id { get; protected set; }

        public virtual Organization Organization { get; set; }

        public virtual StorageName StorageName { get; set; }

        public virtual Organization Department { get; set; }

        public virtual Worker Worker { get; set; }
        [DisplayName("Активный/Не активный")]
        public virtual bool IsActive { get; set; }
        public virtual IList<MatPersonCardContent> MatPersonCardContents { get; set; }

        public virtual string MatPersonIdAndStorageNameId
        {
            get
            {
                return Id+"|"+this.StorageName.Id;
            }
        }

        public virtual string MatPersonInfo
        {
            get
            {
                return (this.Worker != null ? this.Worker.WorkerInfo : "") + " (" + (this.StorageName != null ? this.StorageName.Name : "")+")";
            }
        }
        public virtual string MatPersonFullInfo
        {
            get
            {
                    return (this.Worker != null ? this.Worker.WorkerInfo : "") + " (" + (this.StorageName != null ? this.StorageName.Name : "")+") "
                        + (this.Department != null && this.Department.Mvz!=null? this.Department.Mvz + "-" + this.Department.MvzName : "");
            }
        }

       
    }
}
