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
    //public class AM_SAPMapping : ClassMap<AM_SAP>
    //{
    //    public void AM_SAPMap()
    //    {
    //        Table("AM_SAPS");
    //        Id(x => x.OBJID);
    //        Map(x => x.BEGDA);
    //        Map(x => x.ENDDA);
    //        Map(x => x.LEV_HIE);
    //        Map(x => x.OTYPE);
    //        Map(x => x.PRIOX);
    //        Map(x => x.PROZT);
    //        Map(x => x.SBEGDA);
    //        Map(x => x.SCLAS);
    //        Map(x => x.SENDDA);
    //        Map(x => x.SHORT);
    //        Map(x => x.SOBID);
    //        Map(x => x.STEXT);
    //    }
    //}

//    public class AM_SAPMapping : ClassMap<AM_SAP>
//    {
//       public void AM_SAPMap()
//        {
//            ReadOnly();
//            Table("AM_SAPS");
//            //Id(x => x.Id).Column("OBJID");
//            CompositeId<AM_SAPCompositeId>(x => x.Id)
//                .KeyProperty(x => x.OBJID, "OBJID");
////            Map(x => x.OBJID).Column("OBJID");
//            Map(x => x.BEGDA).Column("BEGDA");
//            Map(x => x.ENDDA).Column("ENDDA");
//            Map(x => x.LEV_HIE).Column("LEV_HIE");
//            Map(x => x.OTYPE).Column("OTYPE");
//            Map(x => x.PRIOX).Column("PRIOX");
//            Map(x => x.PROZT).Column("PROZT");
//            Map(x => x.SBEGDA).Column("SBEGDA");
//            Map(x => x.SCLAS).Column("SCLAS");
//            Map(x => x.SENDDA).Column("SENDDA");
//            Map(x => x.SHORT).Column("SHORT");
//            Map(x => x.SOBID).Column("SOBID");
//            Map(x => x.SOBID).Column("SOBID");

//        }
    
//}    
    
    public class AM_SAPMapping : IAutoMappingOverride<AM_SAP>
    {
        public void Override(AutoMapping<AM_SAP> mapping)
        {
//            mapping.Not.LazyLoad();
            mapping.Id(x => x.Id, "ID").GeneratedBy.Increment();
        }
    }


}
