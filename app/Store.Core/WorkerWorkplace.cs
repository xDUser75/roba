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
    public class WorkerWorkplace : Entity
    {
        public WorkerWorkplace()
        {
        }
        public WorkerWorkplace(int id, Worker worker, Organization organization)
        {
            this.Id = id;
            this.Worker = worker;
            this.Organization = organization;
        }
        public WorkerWorkplace(int id)
        {
            this.Id = id;
        }
        //[ScaffoldColumn(false)]
        //public override int Id { get; protected set; }
        //[Required]
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Required")]
        //[DisplayName("Организация")]
        //[UIHint("organizationTemplate")]
        public virtual Organization Organization { get; set; }

        //public virtual NormaOrganization NormaOrganization { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Required")]
        //[DisplayName("Работник")]
        //[UIHint("workerTemplate")]
        public virtual Worker Worker { get; set; }
        [DataType("Date")]
        public virtual DateTime BeginDate { get; set; }

        public virtual int RootOrganization {get; set;}
        
        [DefaultValue(true)]
        public virtual bool IsActive { get; set; }
        public virtual string WorkplaceInfo
        {
            get
            {
                if (this.Worker != null && this.Organization != null)
                    return this.Worker.TabN + " - " + this.Worker.Fio + " - " + "[" + this.Organization.ShopNumber + "] "
                        + this.Organization.ShopName + " - " + this.Organization.Name + (this.IsActive ? "" : " - [НЕ АКТИВНА]");
                // с красной подсветкой, если НЕ АКТИВНА
                //return this.IsActive ? "" : "<span style=\"color:red\"><b>" + this.Worker.TabN + " - " + this.Worker.Fio + " - " + "[" + this.Organization.ShopNumber + "] "
                //    + this.Organization.ShopName + " - " + this.Organization.Name + (this.IsActive ? "" : " - [НЕ АКТИВНА]") + (this.IsActive ? "" : "</b></span>");
                else
                    return "";
            }
        }
        public virtual string WorkerTabn
        {
            get
            {
                if (this.Worker != null)
                    return ""+this.Worker.TabN;
                return "";
            }
        }

        public virtual string StorageNumber
        {
            get
            {
                if (this.Organization!=null && this.Organization.StorageName != null)
                    return "" + this.Organization.StorageName.StorageNumber;
                return "";
            }
        }

        public virtual string StorageId
        {
            get
            {
                if (this.Organization != null && this.Organization.StorageName != null)
                    return "" + this.Organization.StorageName.Id;
                return "";
            }
        }

        public virtual string WorkplaceShortName
        {
            get
            {
                if (this.Organization != null )
                    return "" + this.Organization.Short + " " + this.Organization.ShortName;
                return "";
            }
        }
        public virtual string MVZ
        {
            get
            {
                if (this.Organization != null && this.Organization.Parent != null)
                    if( this.Organization.Mvz != null)
                        return "" + this.Organization.Mvz + " " + this.Organization.MvzName;
                    else
                        if (this.Organization.Parent != null)
                            return "" + this.Organization.Parent.Mvz + " " + this.Organization.Parent.MvzName;
                return "";
            }
        }


    }
}
