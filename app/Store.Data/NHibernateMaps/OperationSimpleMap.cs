using FluentNHibernate.Mapping;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;
using Store.Core;

namespace Store.Data.NHibernateMaps
{
    public class OperationSimpleMap : IAutoMappingOverride<OperationSimple>
    {
        public void Override(AutoMapping<OperationSimple> mapping)
        {
            mapping.ReadOnly();
            mapping.Table("OperationSimples");
        }
    }
}
