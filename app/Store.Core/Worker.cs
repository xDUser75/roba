using System.ComponentModel.DataAnnotations;
using SharpArch.Core.DomainModel;
using System.Collections.Generic;
using System;
using System.ComponentModel;

namespace Store.Core
{
    [Serializable()]
    public class Worker : Entity, IComparable<Worker>
    {
        public Worker()
        {
        }
        public Worker(int id, int tabN, string fio, Sex sex, bool isActive)
        {
            this.Id = id;
            this.TabN = tabN;
            this.Fio = fio;
            this.Sex = sex;
            this.IsActive = isActive;
        }
        //[ScaffoldColumn(false)]
        //public override int Id { get; protected set; }
        [Required]
        public virtual int TabN { get; set; }

        //[UIHint("workerTemplate"), Required]
        public virtual string Fio { get; set; }
        [UIHint("SexTemplate"), Required]
        public virtual Sex Sex { get; set; }
        public virtual bool IsActive { get; set; }

        //public virtual IList<Organization> Workplaces { get; set; }
        public virtual int RootOrganization { get; set; }

        //public virtual IList<WorkerSize> WorkerSizes { get; set; }

        public virtual IList<NomBodyPartSize> NomBodyPartSizes { get; set; }
        
        [DataType("Date")]
        public virtual DateTime BeginDate { get; set; }

        public virtual WorkerCategory WorkerCategory { get; set; }
        public virtual WorkerGroup WorkerGroup { get; set; }

        [DisplayName("Дата приема на работу")]
        [DataType("Date")]
        public virtual DateTime WorkDate { get; set; }

        // Дата последнего обновления, в случае не активной записи является датой увольнения
        [DisplayName("Дата увольнения")]
        [DataType("Date")]
        public virtual DateTime? DateZ { get; set; }
        public virtual bool IsTabu { get; set; }

        [DisplayName("Дата начала декретного отпуска")]
        [DataType("Date")]
        public virtual DateTime? ChildcareBegin { get; set; }
        [DisplayName("Дата окончания декретного отпуска")]
        [DataType("Date")]
        public virtual DateTime? ChildcareEnd { get; set; }


        public virtual string Growth
        {
            get
            {
                if (NomBodyPartSizes != null)
                {
                    foreach (var item in NomBodyPartSizes)
                    {
                        // DataGlobals.GROWTH_SIZE_ID = 1;      Код типа размера Рост
                        if (1 == item.NomBodyPart.Id)
                            return item.SizeNumber;
                    }
                }
                return "";
            }
        }

        public virtual string WorkerInfo
        {
            get
            {
                return this.TabN + " - " + this.Fio;
            }
        }

        public override string ToString()
        {
            //return base.ToString();
            return "[id="+this.Id+", fio="+this.Fio+"]";
        }
        
        public virtual int CompareTo(Worker obj)
        {
            return this.Fio.CompareTo(obj.Fio);
        }
    }
}
