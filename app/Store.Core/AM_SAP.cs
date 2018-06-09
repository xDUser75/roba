using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Core.DomainModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Store.Core
{
    //public class AM_SAPCompositeId : ValueObject
    //{
    //    public virtual string OBJID { get; set; }
    //}
    [Serializable()]
    public class AM_SAP : Entity
    {
        [ScaffoldColumn(false)]
        public override int Id { get; protected set; }

        [Required]
        public virtual string OBJID { get; set; }
        public virtual string OTYPE { get; set; }
        public virtual string SHORT { get; set; }
        public virtual string STEXT { get; set; }
        public virtual double PROZT { get; set; }
        public virtual DateTime BEGDA { get; set; }
        public virtual DateTime ENDDA { get; set; }
        public virtual string PRIOX { get; set; }
        public virtual string SCLAS { get; set; }
        public virtual string SOBID { get; set; }
        public virtual DateTime SBEGDA { get; set; }
        public virtual DateTime SENDDA { get; set; }
        public virtual int LEV_HIE { get; set; }
        public virtual string BUKRS { get; set; }
        public virtual string PERNR { get; set; }
        public virtual string R_01 { get; set; }
        public virtual string R_02 { get; set; }
        public virtual string R_03 { get; set; }
        public virtual string R_04 { get; set; }
        public virtual string R_05 { get; set; }
        public virtual string R_06 { get; set; }
        public virtual string R_07 { get; set; }
        public virtual string GESCH { get; set; }
        // категория сотрудникоа
        public virtual string PERSK { get; set; }
        // группа сотрудников
        public virtual string PERSG { get; set; }
        public virtual string SESSIONID { get; set; }
        public virtual DateTime DATP { get; set; }
        public virtual string STRINF_ID { get; set; }
        public virtual string VERB { get; set; }
        public virtual string SHOPNUMBER { get; set; }
        public virtual Boolean ISSHOP { get; set; }
        public virtual DateTime BEGDA_D { get; set; }
        public virtual DateTime ENDDA_D { get; set; }
        public virtual string MVZ { get; set; }
        public virtual string MVZ_NAME { get; set; }
    }


}
