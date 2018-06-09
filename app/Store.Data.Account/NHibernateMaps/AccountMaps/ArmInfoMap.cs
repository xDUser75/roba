using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using Store.Core.Account;

namespace Store.Data.Account.NHibernateMaps.AccountMaps
{
    public class ArmInfoMap : ClassMap<ArmInfo>
    {
        public ArmInfoMap()
        {
            ReadOnly();
            Table("acs_awp");
            Id(x => x.Id, "narm").GeneratedBy.Assigned();
            Map(x => x.Name).Column("nmar");
        }
    }
}
