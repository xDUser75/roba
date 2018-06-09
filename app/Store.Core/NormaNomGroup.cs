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
    public class NormaNomGroup : Entity
    {
        public NormaNomGroup(int Id)
        {
            this.Id = Id;
        }

        public NormaNomGroup()
        {
        }

        [Required]
        [DisplayName("Группа номенклатуры")]
        // Сделали, что только Активные по просьбе ЗСМК. ВГОК пока все группы активные.
        //[UIHint("nomGroupActiveTemplate")]
        [UIHint("nomGroupTemplate")]
        public virtual NomGroup NomGroup { get; set; }

        [DisplayName("Основная/Замена")]
        public virtual bool IsBase { get; set; }
        //public virtual int NormaContentId { get; set; }
        public virtual NormaContent NormaContent { get; set; }


    }

}
