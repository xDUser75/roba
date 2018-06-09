using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Core.DomainModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;

namespace Store.Core
{
    [Serializable()]
    public class Result : Entity
    {
        public Result(){}
        public Result(int id){
            this.Id = id;
        }

        [DisplayName("Наименование")]
        public virtual string Name{ get; set; }

        [DisplayName("Цвет")]
        public virtual string Color{ get; set; }
    }
}
