using Store.Core;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;

namespace Store.Data.NHibernateMaps.AccountMaps
{
    public class NormaContentMap : IAutoMappingOverride<NormaContent>
    {
        public void Override(AutoMapping<NormaContent> mapping)
        {
            mapping.HasManyToMany(x => x.NormaNomGroups)
                .Inverse()
                .Cascade.All()
                .ParentKeyColumn("NormaContentId")
                .ChildKeyColumn("NomGroupId")
                .Table("NormaNomgroups");
        }
    }
}
