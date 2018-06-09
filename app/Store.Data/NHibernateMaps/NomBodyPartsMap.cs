using Store.Core;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;

namespace Store.Data.NHibernateMaps.AccountMaps
{
    public class NomBodyPartsMap : IAutoMappingOverride<NomBodyPart>
    {
        public void Override(AutoMapping<NomBodyPart> mapping)
        {
            mapping.Id(x => x.Id);
            mapping.Map(x => x.Name)
                .Column("name");
            //mapping.Map(x => x.EditName)
            //    .Column("name");
        }
    }
}
