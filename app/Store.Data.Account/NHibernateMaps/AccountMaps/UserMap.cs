using FluentNHibernate.Mapping;
using Store.Core.Account;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;

namespace Store.Data.Account.NHibernateMaps.AccountMaps
{

    public class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            Table("acs_access");
            ReadOnly();
            ///*
            CompositeId<UserCompositeId>(x => x.Id)
                .KeyProperty(x => x.ArmId, "narm")
                .KeyProperty(x => x.UserId, "kdus");
            //*/
            //Id(x => x.Id, "kdus");
            //Map(x => x.UserId).Column("kdus");
            Map(x => x.ArmId).Column("narm");
            References(x => x.UserInfo).Column("kdus");
            HasMany(x => x.Roles)
                .KeyColumns.Add(new string[] { "id_arm", "kdus" }).Not.LazyLoad();
            References(x => x.ArmInfo).Column("narm").Not.LazyLoad();
        }
    }

    //public class UserMap : IAutoMappingOverride<User>
    //{
    //    public void Override(AutoMapping<User> mapping)
    //    {
    //        mapping.ReadOnly();
    //        mapping.Table("acs_access");
    //        mapping.Id(x => x.Id, "kdus").GeneratedBy.Assigned();
    //        mapping.References(x => x.UserInfo).Column("kdus");
    //    }
    //}
}
