using System.ComponentModel.DataAnnotations;
using SharpArch.Core.DomainModel;
using System.ComponentModel;
using System;
using Store.Core.Properties;

namespace Store.Core
{
    [Serializable()]
    public class WorkerNorma
    {
        public WorkerNorma() {}
        //public WorkerNorma(NormaContent normaContent, WorkerCardContent workerCardContent, int quantityOnStorage)
        //public WorkerNorma(NormaContent normaContent, WorkerCardContent workerCardContent, int quantity)
        //public WorkerNorma(NomGroup nomGroup, WorkerCardContent workerCardContent, int normaQuantity)
        //{
        //    this.Id = nomGroup.Id;
        //    this.NomGroup = nomGroup;
        //    this.WorkerCardContent = workerCardContent;
        //    //this.QuantityOnStorage = quantityOnStorage;
        //    this.NormaQuantity = normaQuantity;
        //    //this.NormaQuantity = normaQuantity;
        //}
        public virtual int Id { get; set; }
        public virtual int NormaContentId { get; set; }
        public virtual string NormaContentName { get; set; }
        //public virtual NormaContent NormaContent { get; set; }
        //public virtual NomGroup NomGroup { get; set; }
        //public virtual int NormaQuantity {get; set;}
        //public virtual WorkerCardContent WorkerCardContent { get; set; }
        //[DisplayName("На складе")]
        //public virtual int QuantityOnStorage{ get; set; }
        //public virtual Storage Storage { get; set; }

        public virtual int StorageId { get; set; }
        public virtual string StorageNumber { get; set; }
        public virtual string StorageInfo { get; set; }
        
        // кол-во по норме
        public virtual int NormaQuantity { get; set; }

        // период по норме
        public virtual int NormaUsePeriod { get; set; }

        // выдать
        [DataType(DataType.Text, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "DataType")]
        public virtual int PutQuantity { get; set; }

        // на руках
        public virtual int PresentQuantity { get; set; }

        // дата получения
        public virtual DateTime? ReceptionDate { get; set; }

        // дата операции
        public virtual DateTime? OperDate { get; set; }
        // тип операции
        public virtual int OperTypeId { get; set; }
        // Id операции
        public virtual int OperationId { get; set; }
        // Id позиции карточки
       public virtual int WorkerCardContentId { get; set; }

        // номер документа
        public virtual string DocNumber { get; set; }

        // дата документа
        [DataType(DataType.Date)]
        public virtual DateTime DocDate { get; set; }

        // износ
        //[UIHint("WearTemplate")]
        public virtual string Wear { get; set; }

        // основание
        public virtual int MotivId { get; set; }

        // причина
        public virtual int? CauseId { get; set; }


        public virtual string ReceptionDateAsString
        {
            get
            {
                if (this.ReceptionDate != null)
                {
                    //if (format == null)
                    //    format = "dd.MM.yyyy";
                    return this.ReceptionDate.Value.ToString("dd.MM.yyyy");
                }
                return null;
            }
        }

        [DisplayName("Корпоративный")]
        public virtual bool IsCorporate { get; set; }

    }
}
