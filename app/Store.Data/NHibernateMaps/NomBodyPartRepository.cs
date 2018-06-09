using Store.Core;
using Store.Core.RepositoryInterfaces;
using System.Collections.Generic;
using System;
using Store.Core.Account;
using NHibernate.Criterion;

namespace Store.Data
{
    public class NomBodyPartRepository : CriteriaRepository<NomBodyPart>
    {

        public IList<NomBodyPart> GetAllSizeNotIn(int[] array)
        {
             NHibernate.ICriteria criteria = Session.CreateCriteria(typeof(NomBodyPart));
             criteria.Add(Expression.Not(Expression.In("Id", array)));
             return criteria.List<NomBodyPart>();
        }


        public IList<NomBodyPart> GetAllSizeIn(int[] array)
        {
            NHibernate.ICriteria criteria = Session.CreateCriteria(typeof(NomBodyPart));
            criteria.Add(Expression.In("Id", array));
            return criteria.List<NomBodyPart>();
        }

    }
}
