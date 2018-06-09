using Store.Core;
using Store.Core.RepositoryInterfaces;
using System.Collections.Generic;
using System;
using Store.Core.Account;
using NHibernate;
using System.Collections;
using NHibernate.Criterion;

namespace Store.Data
{
    public class OperationRepository : CriteriaRepository<Operation>
    {
        public string GetNextDocNumber(int operType, string organizationId)
        {
            String SEQ_NAME = "DocNum_"+organizationId + "_" + operType + "_SEQ";
            return Session.CreateSQLQuery("Select " + SEQ_NAME + ".nextval from dual").UniqueResult().ToString();

            //return Session.CreateSQLQuery("begin  utils.getnextdocnumber(:idOrg, :idType); end;")
            //    .SetInt32("idType", operType)
            //    .SetInt32("idOrg", int.Parse(organizationId))
            //    .UniqueResult().ToString();
        }

        public string GetNextDocNumberOld(int operType, string organizationId)
        {
            string sql = "select nvl2(a.num, a.num+1, 1) from (select max(to_number(docnumber)) num from operations where OrganizationId=" + organizationId + " and opertypeid=" + operType + " and to_char(docdate, 'rrrr')=to_char(sysdate, 'rrrr')) a";
            return Session.CreateSQLQuery(sql).UniqueResult().ToString();
        }

        /*
        public string GetNextDocNumber(int operType, string organizationId)
        {
            int currentYear = DateTime.Now.Year;
            DocumentNumber docNumber = null;

            using (ITransaction transaction = Session.BeginTransaction(System.Data.IsolationLevel.Unspecified))
            {
                string sql = "select docnumber from documentnumbers where OrganizationId=" + organizationId + " and OPERATIONID=" + operType + " and year=" + currentYear+" for update";
//                string sql = "from DocumentNumber where OrganizationId=" + organizationId + " and OPERATIONID=" + operType + " and year=" + currentYear;// +" for update";

//            IList<DocumentNumber> docNumbers = Session.CreateQuery(sql).List<DocumentNumber>();
            string docN = Session.CreateSQLQuery(sql).UniqueResult().ToString();
                int doc=0;
                if (docN != null)
                    doc = int.Parse(docN);

                docNumber = new DocumentNumber();
                docNumber.OperationId = operType;
                docNumber.Year = currentYear;
                docNumber.OrganizationId = int.Parse(organizationId);
                if (doc == 0)
            {
                docNumber.DocNumber = 1;
            }
            else
            {
                    docNumber.DocNumber = doc + 1;
            }
            Session.SaveOrUpdate(docNumber);
            Session.Transaction.Commit();
            }

            return docNumber.DocNumber.ToString();
        }

*/
    }
}
