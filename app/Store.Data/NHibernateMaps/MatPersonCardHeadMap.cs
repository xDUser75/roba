using Store.Core;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;
using FluentNHibernate.Mapping;

namespace Store.Data.NHibernateMaps
{
    public class MatPersonCardHeadMap : IAutoMappingOverride<MatPersonCardHead>
    {
        public void Override(AutoMapping<MatPersonCardHead> mapping)
        {
            mapping.References(x => x.Department, "orgtreeid");
        }
    }
}
