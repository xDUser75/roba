using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Core.DomainModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Store.Core
{
    [Serializable]
    public class WorkerGroup : Entity
    {
        public WorkerGroup() { }
        public WorkerGroup(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        [ScaffoldColumn(false)]
        public override int Id { get; protected set; }
        
        [Required]
        [DisplayName("Наименование")]
        public virtual string Name { get; set; }
        
    }
}
