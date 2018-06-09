using Store.Core;
using Store.Core.RepositoryInterfaces;
using System.Collections.Generic;
using System;

namespace Store.Data
{
    public class SexRepository : CriteriaRepository<Sex>
    {
        public IList<Sex> GetRowForWorkers()
        {
            IList<Sex> rows = Session.CreateQuery("from Sex S where S.Id < 3").List<Sex>();
            return rows;
        }


        public IList<Sex> GetRowForStore()
        {
            return GetAll();
        }

    }
}
