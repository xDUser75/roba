using Store.Core;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;

namespace Store.Data.NHibernateMaps.AccountMaps
{
    public class WorkerWorkplacesMap1 :  IAutoMappingOverride<WorkerWorkplace>
    {
        public void Override(AutoMapping<WorkerWorkplace> mapping)
        {
            mapping.References(x => x.Worker);
            mapping.References(x => x.Organization, "orgtreeid");
            //mapping.Join("Organizations", j => j.Inverse().KeyColumn("orgtreeid").Map(x => x.Organization));
        }
    }
}
