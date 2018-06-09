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
    public class SignDocumet : Entity
    {
        public SignDocumet()
        {
        }

        public SignDocumet(int id)
        {
            this.Id = id;
        }

        //[ScaffoldColumn(false)]
       // public override int Id { get; protected set; }

        [Required]
        [DisplayName("Организация")]
        public virtual int OrganizationId { get; set; }
        
        [DisplayName("Цex")]
        public virtual int ShopId { get; set; }
        [DisplayName("Подразделение")]
        public virtual Organization Unit { get; set; }
        
        [DisplayName("№ Склада")]
        public virtual int StorageNameId { get; set; }

        [DisplayName("Тип документа")]
        public virtual SignDocTypes SignDocType { get; set; }
        [DisplayName("Код документа")]
        public virtual string CodeDocumetn { get; set; }


        [DisplayName("Тип подписи")]
        public virtual SignTypes SignType { get; set; }
        [DisplayName("Код подписи")]
        public virtual string CodeSign { get; set; }
        [DisplayName("Наименование подписи")]
        public virtual string NameSign { get; set; }
        [DisplayName("Работник")]
        public virtual  Worker Worker { get; set; }
        [DisplayName("Таб. №")]
        public virtual int? Tabn { get; set; }
        [DisplayName("ФИО")]
        public virtual string Fio { get; set; }
        [DisplayName("Порядок")]
        public virtual int? Ord { get; set; }
        [DisplayName("Текст распоряжения")]
        public virtual string Value { get; set; }
        [DisplayName("Должность")]
        public virtual string WorkPlaceName { get; set; }
        
    }
}
