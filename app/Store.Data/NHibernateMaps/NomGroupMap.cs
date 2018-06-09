using Store.Core;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;

namespace Store.Data.NHibernateMaps.AccountMaps
{
    public class NomGroupMap :  IAutoMappingOverride<NomGroup>
    {
        public void Override(AutoMapping<NomGroup> mapping)
        {
            mapping.References(x => x.NomBodyPart)
                .Column("nombodypartid");
  
        }
    }
}
