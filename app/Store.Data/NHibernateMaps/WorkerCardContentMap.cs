using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Store.Core;
using FluentNHibernate.Mapping;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace Store.Data.NHibernateMaps
{
    public class WorkerCardContentMap : ClassMap<WorkerCardContent>
    {
        public WorkerCardContentMap()
        {
            Table("WorkerCardContents");
            Id(x => x.Id).Column("Id");
            References(x => x.NormaContent, "normacontentid");
            References(x => x.Operation, "operationid");
            References(x => x.OperReturn, "OPERRETURNID");
            Map(x => x.Quantity, "Quantity");
            Map(x => x.StartDate, "StartDate");
            Map(x => x.EndDate, "EndDate");
            Map(x => x.UsePeriod, "UsePeriod");
            References(x => x.Storage, "storageid");
            References(x => x.WorkerCardHead, "workercardheadid");
            Map(x => x.IsCorporate);
            References(x => x.GiveOperation, "giveoperationid");
        }
    }



}
