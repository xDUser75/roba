using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Core.DomainModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace Store.Core
{
    [Serializable()]
    public class StorageName : Entity
    {
        public StorageName()
        {
        }

        public StorageName(int id)
        {
            this.Id = id;
        }

        [ScaffoldColumn(false)]
        public override int Id { get; protected set; }

        [Required]
        [DisplayName("Организация")]
        public virtual Organization Organization { get; set; }
        
        [DisplayName("№ Цexa")]
        public virtual int OrgTreeId { get; set; }
        
        [DisplayName("№ Склада")]
        public virtual string StorageNumber { get; set; }
        
        [DisplayName("Наименование")]
        public virtual string Name { get; set; }
        
        [DisplayName("Активный/Не активный")]
        public virtual bool IsActive { get; set; }
        [DisplayName("Салон/Не салон")]
        public virtual bool IsSalon { get; set; }

        public virtual int? Plant { get; set; }

        public virtual string StorageIn { get; set; }
        public virtual string StorageOut { get; set; }
        public virtual string StornoIn { get; set; }
        public virtual string StornoOut { get; set; }

        public virtual string Externalcode { get; set; }
        public virtual string AreaCode { get; set; }
        
        //public override bool Equals(object obj) {
        //    if (obj.GetType() != typeof(StorageName))
        //        return false;
        //    if (this.Id == ((StorageName)obj).Id)
        //        return true;
        //    return false;
        //}

        public virtual string StorageNameInfo
        {
            get
            {
                return this.StorageNumber+" - " + this.Name;
            }
        }
    }
}
