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
    public class WorkerSize : Entity
    {
        public WorkerSize()
        {
        }

        public WorkerSize(int id, Worker worker, NomBodyPartSize nomBodyPartSize, bool isActive)
        {
            this.Id = id;
            this.Worker = worker;
            this.NomBodyPartSize = nomBodyPartSize;
            this.IsActive = isActive;
        }

        public WorkerSize(int id)
        {
            this.Id = id;
        }

        [Required]
        public virtual Worker Worker { get; set; }
//        [DisplayName("Наименование")]
//        [UIHint("NomBodyPartTemplate"), Required]                
//        public virtual NomBodyPart NomBodyPart { get; set; }
//         [DisplayName("Размер")]
//        public virtual int SizeNum { get; set; }
         [DisplayName("Активен")]
        public virtual bool IsActive { get; set; }
         [DisplayName("Размеры Работника")]
         public virtual NomBodyPartSize NomBodyPartSize { get; set; }
        [DisplayName("Тип размера")]
         public virtual NomBodyPart NomBodyPart { get; set; }

    }
}
