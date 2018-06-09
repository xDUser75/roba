using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Core.DomainModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Store.Core.Properties;

namespace Store.Core
{
    [Serializable()]
    public class Subscription : Entity
    {
        public Subscription()
        {
        }
        public Subscription(int id, Worker worker1, Organization organization)
        {
            this.Id = id;
            this.Worker1 = worker1;
            this.Organization = organization;
        }
        public Subscription(int id)
        {
            this.Id = id;
        }
        [DisplayName("Подразделение")]
        public virtual Organization Organization { get; set; }
        [DisplayName("Подразделение")]
        public virtual string OrganizationInfo { 
            get {
                return this.Organization.Name;
            }
        }
        

        [DisplayName("Таб. № руководителя подразделения")]
        public virtual int Tabn1 { get; set; }
        [DisplayName("Таб № ответсвенного в цехе за нормы")]
        public virtual int Tabn2 { get; set; }
        [DisplayName("Таб № ответственного за выходной документ")]
        public virtual int Tabn3 { get; set; }

        public virtual int WorkerInfo1Id
        {
            get
            {
                if (this.Worker1 == null)
                {
                    return -1;
                }
                else
                {
                    return this.Worker1.Id;
                }
            }
        }
        public virtual int WorkerInfo2Id
        {
            get
            {
                if (this.Worker2 == null)
                {
                    return -1;
                }
                else
                {
                    return this.Worker2.Id;
                }
            }
        }
        public virtual int WorkerInfo3Id
        {
            get
            {
                if (this.Worker3 == null)
                {
                    return -1;
                }
                else
                {
                    return this.Worker3.Id;
                }
            }
        }
        [DisplayName("Руководитель подразделения")]
        [UIHint("workerTemplate")]
        public virtual Worker Worker1 { get; set; }
        [DisplayName("Руководитель подразделения")]
        public virtual string WorkerInfo1
        {
            get
            {
                if (this.Worker1 == null)
                {
                    return "";
                }
                else {
                    return this.Worker1.WorkerInfo; 
                }
            }
        }

        [DisplayName("Ответсвенный в цехе за нормы")]
        [UIHint("workerTemplate")]
        public virtual Worker Worker2 { get; set; }
        [DisplayName("Ответсвенный в цехе за нормы")]
        public virtual string WorkerInfo2
        {
            get
            {
                if (this.Worker2 == null)
                {
                    return "";
                }
                else
                {
                    return this.Worker2.WorkerInfo;
                }
            }
        }
        [DisplayName("Ответственный за выходной документ")]
        [UIHint("workerTemplate")]
        public virtual Worker Worker3 { get; set; }
        [DisplayName("Ответственный за выходной документ")]
        public virtual string WorkerInfo3
        {
            get
            {
                if (this.Worker3 == null)
                {
                    return "";
                }
                else
                {
                    return this.Worker3.WorkerInfo;
                }
            }
        }

        public virtual void updateTabN()
        {
            if (this.Worker1 == null)
            {
                this.Tabn1 = -1;
            }
            else 
            {
                this.Tabn1 = this.Worker1.TabN;
            }

            if (this.Worker2 == null)
            {
                this.Tabn2 = -1;
            }
            else
            {
                this.Tabn2 = this.Worker2.TabN;
            }

            if (this.Worker3 == null)
            {
                this.Tabn3 = -1;
            }
            else
            {
                this.Tabn3 = this.Worker3.TabN;
            }
        }
        
    }
}
