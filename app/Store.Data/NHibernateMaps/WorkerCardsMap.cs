using Store.Core;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;
using FluentNHibernate.Mapping;

namespace Store.Data.NHibernateMaps
{
    public class WorkerCardsMap : IAutoMappingOverride<WorkerCard>
    {
        public void Override(AutoMapping<WorkerCard> mapping)
        {
            
            //mapping.Table("NormaContents");
            //mapping.HasOne(x => x.
            //References(x => x.Worker).Not.LazyLoad().Fetch.Join();
            //References(x => x.Organization, "orgtreeid").Not.LazyLoad().Fetch.Join();
            //mapping.References(x => x.NormaContent);
            //mapping.References(x => x.WorkerCard, "normaid");
            //mapping.References(x => x.Operation);
            //mapping.HasMany(hm => hm.NormaContents).KeyColumn("normaid");
            //mapping.Join("NormaContents", j => j.Inverse().KeyColumn("normaid").Map(x => x.NormaContent));
        }
    }
}
