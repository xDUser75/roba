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
    public class OrganizationId : ValueObject
    {
        public virtual int Id { get; set; }
    }


   //        public class Organization : EntityWithTypedId<OrganizationId>
        //public class Organization : EntityWithSerializableId
    [Serializable()]
    public class Organization : Entity, IComparable<Organization>
    {
        public Organization()
        {
        }
        public Organization(int id)
        {
            this.Id = id;
        }

            public Organization(int Id, string Name)
            {
                this.Id = Id;
                this.Name = Name;
            }

        [Required]
        [DisplayName("Id родителя")]
        public virtual int Pid { get; set; }
        [DisplayName("Краткое наименование")]
        public virtual string ShortName { get; set; }
        [DisplayName("Наименование")]
        [UIHint("organizationTemplate")]
        public virtual string Name { get; set; }
        
        [DisplayName("Активный/Не активный")]
        public virtual bool IsActive { get; set; }
        public virtual Organization Parent { get; set; }
        public virtual IList<Organization> Childs { get; set; }

        [DisplayName("Id цеха")]
        public virtual int ShopId { get; set; }
        [DisplayName("Id организации")]
        public virtual int OrgId { get; set; }
        [DisplayName("Код цеха")]
        public virtual string ShopNumber { get; set; }
        [ScriptIgnore]
        public virtual string Bukrs { get; set; }
        public virtual string Short { get; set; }
        public virtual bool IsWorkPlace { get; set; }
        public virtual string AreaCode { get; set; }
        public virtual string ExternalCode { get; set; }
        public virtual string ParentExternalCode { get; set; }

        [DisplayName("Дата начала действия структурной единицы")]
        [DataType("Date")]
        [ScriptIgnore]
        public virtual DateTime BeginDate { get; set; }

        public virtual NormaOrganization NormaOrganization { get; set; }
       

        public virtual string FullName
        {
            get
            {
                if (this.Parent == null || this.Parent.Id == 0 )
                {
                    return this.Name;
                } else {
                    return this.Parent.FullName + " >> " + this.Name;
                }
            }
        }

       public virtual int RootOrganization
       {
            get
            {
                int rootOrganization = -1;
                if (this.Parent != null && this.Parent.Id != 0)
                    rootOrganization = this.Parent.RootOrganization;
                else
                    rootOrganization = this.Id;
                return rootOrganization;
            }
       }

       public virtual string ShopName
       {
           get
           {
               if (this.Id == this.ShopId)
                   return this.Name;
               else if (Parent != null)
                   return this.Parent.ShopName;
               else
                   return "";
           }
       }
       public virtual string ShopInfo
       {
           get
           {
               if (this.Id == this.ShopId)
                   return this.ShopNumber+" - "+this.Name;
               else if (Parent != null)
                   return this.Parent.ShopInfo;
               else
                   return "";
           }
       }

       public virtual string OrganizationMvzName
       {
           get
           {
               if (this.Parent != null && this.Parent.Id != 0)
               {
                   if (this.IsWorkPlace == true)
                   {
                       if (this.Mvz != null)
                           return this.Mvz + "-" + this.MvzName;
                       else
                           if (this.Parent != null)
                               return this.Parent.Mvz + "-" + this.Parent.MvzName;
                           else
                               return "";
                   }
                   else
                       return this.Mvz + "-" + this.MvzName;
               }
               else
               {
                   return "";
               }
           }
       }

       public virtual int CompareTo(Organization obj)
       {
           return this.FullName.CompareTo(obj.FullName);
       }

       public virtual StorageName StorageName { get; set; }

       public virtual string Mvz { get; set; }
       public virtual string MvzName{ get; set; }
       


    }
}
