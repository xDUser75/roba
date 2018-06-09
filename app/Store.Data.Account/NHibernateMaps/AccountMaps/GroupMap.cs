using FluentNHibernate.Mapping;
using Store.Core.Account;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;

namespace Store.Data.Account.NHibernateMaps.AccountMaps
{
    public class GroupMap : ClassMap<Group>
    {
        public GroupMap()
        {
            ReadOnly();
            Table("adm_user_group");
            //CompositeId()
            //    .KeyProperty(x => x.ArmId, "id_arm")
            //    .KeyProperty(x => x.GroupId, "id_group");
            CompositeId<GroupCompositeId>(x => x.Id)
                .KeyProperty(x => x.ArmId, "id_arm")
                .KeyProperty(x => x.RoleId, "id_group");
            Map(x => x.ArmId).Column("id_arm");
            Map(x => x.UserId).Column("kdus");
            Map(x => x.GroupId).Column("id_group");
            HasMany(x => x.Accesses)
                .KeyColumns.Add(new string[] { "id_arm", "id_group" }).Not.LazyLoad();
            HasMany(x => x.Orgs)
                .KeyColumns.Add(new string[] { "id_arm", "id_group" }).Not.LazyLoad();
            References(x => x.User)
                .Columns(new string[] { "id_arm", "kdus" }).Not.LazyLoad();
            Join("adm_groups", j =>
            {
                //j.Optional();
                j.KeyColumn(new string[] { "id_arm", "id_group" });
                j.Map(x => x.Description);
            });
        }
    }

    //public class GroupMap : IAutoMappingOverride<Group>
    //{
    //    public void Override(AutoMapping<Group> mapping)
    //    {
    //        mapping.ReadOnly();
    //        mapping.Table("adm_user_group");
    //        //mapping.Id(x => x.Id).Column("id_arm");
    //        mapping.CompositeId()
    //            .KeyProperty(x => x.ArmId, "id_arm")
    //            .KeyProperty(x => x.RoleId, "id_group");
    //        //mapping.CompositeId<GroupCompositeId>(x => x.Id)
    //        //    .KeyProperty(x => x.ArmId, "id_arm")
    //        //    .KeyProperty(x => x.RoleId, "id_group");
    //        //mapping.Map(x => x.ArmId).Column("id_arm");
    //        mapping.Map(x => x.UserId).Column("kdus");
    //        mapping.HasMany(x => x.Rights)
    //            .KeyColumns.Add(new string[] { "id_arm", "id_group" });
    //        mapping.Join("adm_groups", j =>
    //        {
    //            //j.Optional();
    //            j.KeyColumn(new string[] { "id_arm", "id_group" });
    //            j.Map(x => x.Description);
    //        });
    //    }
    //}
}
