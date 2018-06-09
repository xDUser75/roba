using Store.Core;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;
using FluentNHibernate.Mapping;

namespace Store.Data.NHibernateMaps
{
    public class MatPersonOnHandsMap :  IAutoMappingOverride<MatPersonOnHands>
    {
        public void Override(AutoMapping<MatPersonOnHands> mapping)
        {
            mapping.ReadOnly();
            mapping.Table("view_matperson_on_hand");
        }
    }

}
