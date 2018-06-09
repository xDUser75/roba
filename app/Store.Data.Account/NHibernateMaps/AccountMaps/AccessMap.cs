using FluentNHibernate.Mapping;
using Store.Core.Account;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;

namespace Store.Data.Account.NHibernateMaps.AccountMaps
{
    public class AccessMap : ClassMap<Store.Core.Account.Access>
    {
        public AccessMap()
        {
            ReadOnly();
            Table("adm_group_access");
            //CompositeId()
            //    .KeyProperty(x => x.ArmId, "id_arm")
            //    .KeyProperty(x => x.RoleId, "id_group");
                //.KeyProperty(x => x.ModuleId, "id_module");
                //.KeyProperty(x => x.AccessId, "id_access");
            ///*
            CompositeId<AccessCompositeId>(x => x.Id)
                .KeyProperty(x => x.ArmId, "id_arm")
                .KeyProperty(x => x.RoleId, "id_group")
                .KeyProperty(x => x.ModuleId, "id_module")
                .KeyProperty(x => x.AccessId, "id_access");
            //*/
            //Id(x => x.Id).Column("id_module");
            //Map(x => x.RightId).Column("RightId");
            Map(x => x.Description);
            References(x => x.Group)
                .Columns(new string[] { "id_arm", "id_group" }).Not.LazyLoad();
            //.Columns(new string[] { "id" });
            //.Column("id_group")
            //.Column("id_arm");
        }
    }

    //public class AccessMap : IAutoMappingOverride<Store.Core.Account.Access>
    //{
    //    public void Override(AutoMapping<Store.Core.Account.Access> mapping)
    //    {
    //        mapping.ReadOnly();
    //        mapping.Table("adm_group_access");
    //        //mapping.Id(x => x.Id).Column("id_module");
    //        mapping.CompositeId()
    //            .KeyProperty(x => x.ArmId, "id_arm")
    //            .KeyProperty(x => x.RoleId, "id_group")
    //            .KeyProperty(x => x.ModuleId, "id_module")
    //            .KeyProperty(x => x.AccessId, "id_access");
    //        //mapping.CompositeId<AccessCompositeId>(x => x.Id)
    //        //    .KeyProperty(x => x.ArmId, "id_arm")
    //        //    .KeyProperty(x => x.RoleId, "id_group")
    //        //    .KeyProperty(x => x.ModuleId, "id_module")
    //        //    .KeyProperty(x => x.AccessId, "id_access");
    //        mapping.Map(x => x.Description);
    //        mapping.References(x => x.Role)
    //            .Columns(new string[] { "id_arm", "id_group" });
    //    }
    //}
}
