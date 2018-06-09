using FluentNHibernate.Mapping;
using Store.Core;

namespace Store.Data.NHibernateMaps.AccountMaps
{
    public class OrganizationsMap : ClassMap<Organization>
    {
        public OrganizationsMap()
        {
            ReadOnly();
            Table("organizations");
            //CompositeId<OrganizationId>(x => x.Id)
            //.KeyProperty(x => x.Id, "id");
            Id(x => x.Id).Column("Id");
            Map(x => x.Pid).Column("Pid");
            Map(x => x.Name).Column("Name");
            Map(x => x.ShortName).Column("ShortName");
            Map(x => x.ShopId).Column("ShopId");
            Map(x => x.OrgId).Column("OrganizationId");
            Map(x => x.Bukrs).Column("Bukrs");
            Map(x => x.ShopNumber).Column("ShopNumber");
            Map(x => x.Short).Column("Short");
            Map(x => x.IsActive).Column("isactive");
            Map(x => x.IsWorkPlace).Column("IsWorkPlace");
            Map(x => x.BeginDate).Column("BeginDate");
            Map(x => x.AreaCode).Column("AreaCode");
            Map(x => x.ExternalCode).Column("ExternalCode");
            Map(x => x.ParentExternalCode).Column("ParentExternalCode");
            Map(x => x.Mvz).Column("mvz");
            Map(x => x.MvzName).Column("mvzname");
            References(x => x.Parent)
                .Column("pid");
            References(x => x.StorageName)
                .Column("storagenameid");
            HasMany(x => x.Childs)
                .KeyColumns.Add("pid");
            HasOne(x => x.NormaOrganization).PropertyRef("Organization");
        }
    }
}
