using FluentNHibernate.Mapping;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

using Store.Core;

namespace Store.Data.NHibernateMaps.AccountMaps
{

    public class NormaMapping : IAutoMappingOverride<Norma>
    {
        public void Override(AutoMapping<Norma> mapping)
        {
            //mapping.HasMany(x => x.NormaContents).KeyColumns.Add("NormaId");
            // пытался сделать сортировку прямо в маппинге
            //mapping.HasMany(x => x.NormaContents).OrderBy("NomGroup.Name");
        }
    }
}
