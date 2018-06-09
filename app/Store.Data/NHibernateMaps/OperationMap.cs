using Store.Core;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;

namespace Store.Data.NHibernateMaps.AccountMaps
{
    public class OperationMap : IAutoMappingOverride<Operation>
    {
        public void Override(AutoMapping<Operation> mapping)
        {
            //mapping.References(x => x.OperType).Not.LazyLoad();
            //mapping.References(x => x.DocType).Not.LazyLoad();
            mapping.References(x => x.Partner, "partnerId");
            //mapping.References(x => x.WorkerWorkplace).Not.LazyLoad();
        }
    }
}
