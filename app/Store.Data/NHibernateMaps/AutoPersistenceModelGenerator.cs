using System;
using System.Linq;
using FluentNHibernate.Automapping;
using FluentNHibernate.Conventions;
using Store.Core;
using Store.Data.NHibernateMaps.Conventions;
using SharpArch.Core.DomainModel;
using SharpArch.Data.NHibernate.FluentNHibernate;

namespace Store.Data.NHibernateMaps
{

    public class AutoPersistenceModelGenerator : IAutoPersistenceModelGenerator
    {

        #region IAutoPersistenceModelGenerator Members

        public AutoPersistenceModel Generate()
        {
            return AutoMap.AssemblyOf<Worker>(new AutomappingConfiguration())
                .Conventions.Setup(GetConventions())
                .IgnoreBase<Entity>()
                .IgnoreBase(typeof(EntityWithTypedId<>))
                .UseOverridesFromAssemblyOf<AutoPersistenceModelGenerator>();
        }

        #endregion

        private Action<IConventionFinder> GetConventions()
        {
            return c =>
            {
                c.Add<Store.Data.NHibernateMaps.Conventions.ForeignKeyConvention>();
                c.Add<Store.Data.NHibernateMaps.Conventions.HasManyConvention>();
                c.Add<Store.Data.NHibernateMaps.Conventions.HasManyToManyConvention>();
                c.Add<Store.Data.NHibernateMaps.Conventions.ManyToManyTableNameConvention>();
                c.Add<Store.Data.NHibernateMaps.Conventions.PrimaryKeyConvention>();
                c.Add<Store.Data.NHibernateMaps.Conventions.ReferenceConvention>();
                c.Add<Store.Data.NHibernateMaps.Conventions.TableNameConvention>();
            };
        }
    }
}
