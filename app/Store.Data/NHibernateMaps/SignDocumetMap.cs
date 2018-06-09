using Store.Core;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;

namespace Store.Data.NHibernateMaps.AccountMaps
{
    public class SignDocumetMap : IAutoMappingOverride<SignDocumet>
    {
        public void Override(AutoMapping<SignDocumet> mapping)
        {
            mapping.Map(x => x.ShopId).Column("shopid");
            mapping.References(x => x.Unit).Column("orgtreeid");
        }
    }
}
