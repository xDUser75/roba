using Store.Core;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;
using FluentNHibernate.Mapping;

namespace Store.Data.NHibernateMaps
{
    public class WorkerWorkplaceMap : IAutoMappingOverride<WorkerWorkplace>
    {
        public void Override(AutoMapping<WorkerWorkplace> mapping)
        {
            //Table("workerworkplaces");
            //Id(x => x.Id).Column("Id");
            //References(x => x.Worker);
            mapping.References(x => x.Organization, "orgtreeid");
            mapping.Map(x => x.RootOrganization, "organizationId");
            //mapping.References(x => x.NormaOrganization, "orgtreeid");
            //References(x => x.Worker).Not.LazyLoad().Fetch.Join();
            //References(x => x.Organization, "orgtreeid").Not.LazyLoad().Fetch.Join();
            //mapping.Join("Organizations", j => j.Inverse().KeyColumn("orgtreeid").Map(x => x.Organization));
        }
    }
}
