using FluentNHibernate.Mapping;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

using Store.Core;

namespace Store.Data.NHibernateMaps
{

        //public class NormaContentMapping : IAutoMappingOverride<NormaContent>
        //{
        //    public void Override(AutoMapping<NormaContent> mapping)
        //    {
        //        mapping.HasMany(x => x.NormaNomGroups).KeyColumns.Add("NormaContentId");
                
        //    }
        //}

        public class NormaContentMap : ClassMap<NormaContent>
        {
            public NormaContentMap()
            {
//                ReadOnly();
                Table("normacontents");
                Id(x => x.Id).Column("Id");
                Map(x => x.NormaId).Column("normaid");
                Map(x => x.UsePeriod).Column("UsePeriod");
                Map(x => x.Quantity).Column("Quantity");
                Map(x => x.QuantityTON).Column("QuantityTON");
                Map(x => x.InShop).Column("InShop");
                Map(x => x.IsActive).Column("IsActive");
                References(x => x.NomGroup).Column("nomgroupId");
                HasMany(x => x.NormaNomGroups).KeyColumns.Add("NormaContentId");
            }
        }  
    
}
