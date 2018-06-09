using Store.Core;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;
using FluentNHibernate.Mapping;

namespace Store.Data.NHibernateMaps
{
    public class TestRegisterSimpleMap : ClassMap<TestRegisterSimple>
    {
        public TestRegisterSimpleMap()
        {
            ReadOnly();
            Table("VIEW_TEST_REGISTERS");
            Id(x => x.Id).Column("Id");
            Map(x => x.OrganizationId).Column("OrganizationId");
            Map(x => x.City).Column("City");
            Map(x => x.Color).Column("Color");
            Map(x => x.Model).Column("Model");
            Map(x => x.NomGroup).Column("NomGroup");
            Map(x => x.NomGroupId).Column("NomGroupId");
            Map(x => x.ProvaiderId).Column("ProvaiderId");
            Map(x => x.Provaider).Column("Provaider");
            Map(x => x.Producer).Column("Producer");
            Map(x => x.TestDate).Column("TestDate");
        }
    }
}
