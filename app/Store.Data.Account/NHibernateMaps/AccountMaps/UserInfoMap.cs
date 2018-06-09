using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using Store.Core.Account;

namespace Store.Data.Account.NHibernateMaps.AccountMaps
{
    public class UserInfoMap : ClassMap<UserInfo>
    {
        public UserInfoMap()
        {
            ReadOnly();
            Table("acs_user");
            Id(x => x.UserId, "kdus").GeneratedBy.Assigned();
            Map(x => x.UserName).Column("dmnm");
            Map(x => x.Password).Column("pswd");
            Map(x => x.FullName).Column("fiou");
            //Map(x => x.Domain).Column("dmnu");
            Map(x => x.DomainName).Column("dmnm");
        }
    }
}
