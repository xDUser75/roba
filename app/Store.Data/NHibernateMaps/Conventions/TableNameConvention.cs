using FluentNHibernate.Conventions;
using FluentNHibernate.Mapping;

namespace Store.Data.NHibernateMaps.Conventions
{
    public class TableNameConvention : IClassConvention
    {
        public void Apply(FluentNHibernate.Conventions.Instances.IClassInstance instance)
        {
            //if (instance.EntityType.Name.ToUpper().Contains("WORKER"))
            //    instance.Table("WORKERS");
            //else
            var a = instance.EntityType.Name;
                instance.Table(Inflector.Net.Inflector.Pluralize(instance.EntityType.Name));
        }
    }
}
