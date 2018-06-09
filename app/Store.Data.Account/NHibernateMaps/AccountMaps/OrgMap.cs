using FluentNHibernate.Mapping;
using Store.Core.Account;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;

namespace Store.Data.Account.NHibernateMaps.AccountMaps
{
    public class OrgMap : ClassMap<Org>
    {
        public OrgMap()
        {
            ReadOnly();
            Table("adm_group_objects");
            CompositeId<OrgCompositeId>(x => x.Id)
                .KeyProperty(x => x.ArmId, "id_arm")
                .KeyProperty(x => x.GroupId, "id_group")
                .KeyProperty(x => x.OrgId, "id_object");
            //Map(x => x.OrgId).Column("id_object");
            References(x => x.Group)
                .Columns(new string[] { "id_arm", "id_group" }).Not.LazyLoad();
        }
    }

}
