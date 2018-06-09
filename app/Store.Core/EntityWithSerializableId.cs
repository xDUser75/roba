using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Core.DomainModel;

namespace Store.Core
{
    [Serializable()]
    public class EntityWithSerializableId : Entity
    {
        public virtual int ItemId
        {
            get { return Id; }
            set { ; }
        }
    }
}
