using Store.Core;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;

namespace Store.Data.NHibernateMaps.AccountMaps
{
    public class StoragesMap :  IAutoMappingOverride<Storage>
    {
        public void Override(AutoMapping<Storage> mapping)
        {
            mapping.References(x => x.Growth)
                .Column("growthId").NotFound.Ignore(); 
        }
    }
}
