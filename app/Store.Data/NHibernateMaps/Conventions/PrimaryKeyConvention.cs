using FluentNHibernate.Conventions;
using FluentNHibernate.Mapping;
using Store.Core;
 
namespace Store.Data.NHibernateMaps.Conventions
{
    public class PrimaryKeyConvention : IIdConvention
    {
        public void Apply(FluentNHibernate.Conventions.Instances.IIdentityInstance instance)
        {
            instance.Column("Id");
            instance.UnsavedValue("0");
            //instance.GeneratedBy.HiLo("1000");
            //instance.GeneratedBy.Assigned();
            //string seqName = instance.EntityType.Name.ToUpper() + "S_SEQ";
            if (instance.EntityType == typeof(Operation))
            {
                instance.GeneratedBy.Sequence("OPERATIONS_SEQ");
            }
            else
                if (instance.EntityType == typeof(WorkerCardHead)) 
                {
                    instance.GeneratedBy.Sequence("WORKERCARDHEADS_SEQ");
                }
                else
                    if (instance.EntityType == typeof(WorkerCardContent))
                    {
                        instance.GeneratedBy.Sequence("WORKERCARDCONTENTS_SEQ");
                    }
                    else
                        if (instance.EntityType == typeof(Storage))
                        {
                            instance.GeneratedBy.Sequence("STORAGES_SEQ");
                        }
                        else
                            if (instance.EntityType == typeof(Norma))
                            {
                                instance.GeneratedBy.Sequence("NORMAS_SEQ");
                            }
                            else
                                if (instance.EntityType == typeof(NormaContent))
                                {
                                    instance.GeneratedBy.Sequence("NORMACONTENTS_SEQ");
                                }
                                else
                                    if (instance.EntityType == typeof(NormaNomGroup))
                                    {
                                        instance.GeneratedBy.Sequence("NORMANOMGROUPS_SEQ");
                                    }
                                    else
                                        if (instance.EntityType == typeof(Nomenclature))
                                        {
                                            instance.GeneratedBy.Sequence("NOMENCLATURES_SEQ");
                                        }
                                        else
                                            if (instance.EntityType == typeof(NormaOrganization))
                                            {
                                                instance.GeneratedBy.Sequence("NORMAORGANIZATIONS_SEQ");
                                            }
                                            else
                                                if (instance.EntityType == typeof(NormaOrganizationSimple))
                                                {
                                                    instance.GeneratedBy.Sequence("NORMAORGANIZATIONS_SEQ");
                                                }
                                            else
                                                if (instance.EntityType == typeof(WorkerSize))
                                                {
                                                    instance.GeneratedBy.Sequence("WORKERSIZES_SEQ");
                                                }

                                                else
                                                    if (instance.EntityType == typeof(MatPersonCardHead))
                                                    {
                                                        instance.GeneratedBy.Sequence("MATPERSONCARDHEADS_SEQ");
                                                    }

                                                    else
                                                        if (instance.EntityType == typeof(MatPersonCardContent))
                                                        {
                                                            instance.GeneratedBy.Sequence("MATPERSONCARDCONTENTS_SEQ");
                                                        }

                                                        
                                                        else
                                                            if (instance.EntityType == typeof(Remaind))
                                                            {
                                                                instance.GeneratedBy.Sequence("REMAINDS_SEQ");
                                                            }
                                                            else
                                                                
                                                                if (instance.EntityType == typeof(SignDocumet))
                                                                {
                                                                    instance.GeneratedBy.Sequence("SIGNDOCUMETS_SEQ");
                                                                }
                                                                else

                                                    {
                                                        instance.GeneratedBy.Increment();
                                                    }
        }
    }
}
