using Store.Core;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;
using FluentNHibernate.Mapping;

namespace Store.Data.NHibernateMaps
{
    //public class NormaOrganizationSimpleMap : IAutoMappingOverride<NormaOrganizationSimpleMap>
    //{
    //    public void Override(AutoMapping<NormaOrganizationSimple> mapping)
    //    {

    //        mapping.Table("NormaOrganizations");
    //        mapping.Id(x => x.OrganizationId, "organizationId");            
    //    }
    //}

    public class NormaOrganizationSimpleMap : ClassMap<NormaOrganizationSimple>
    {
        public NormaOrganizationSimpleMap()
        {
            ReadOnly();
            Table("NormaOrganizations");
            Id(x => x.Id).Column("Id");
            Map(x => x.IsActive).Column("isactive");
            Map(x => x.OrganizationId).Column("organizationid");

            References(x => x.Norma)
                .Column("normaid");

        }
    }

}
