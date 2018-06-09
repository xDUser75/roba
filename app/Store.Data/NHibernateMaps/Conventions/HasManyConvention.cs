using FluentNHibernate.Conventions;
using FluentNHibernate.Mapping;

namespace Store.Data.NHibernateMaps.Conventions
{
    public class HasManyConvention : IHasManyConvention
    {
        public void Apply(FluentNHibernate.Conventions.Instances.IOneToManyCollectionInstance instance)
        {
            instance.Key.Column(instance.EntityType.Name + "Id");
            instance.Inverse();
            instance.Cascade.All();
        }
    }
}
