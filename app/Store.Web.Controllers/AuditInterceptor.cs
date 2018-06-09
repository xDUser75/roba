using System;
using NHibernate;
using NHibernate.Type;
using System.Web;
using Store.Core.Account;
using SharpArch.Data.NHibernate;
using Store.Data;
using System.Text;
using System.Collections;
using Store.Core;

namespace Store.Web.Controllers
{
    public class AuditInterceptor : EmptyInterceptor
    {
        private static object lastId = null;

        public override bool OnSave(object entity, object id, object[] state, string[] propertyNames, IType[] types)
        {
            //IPrincipal user = HttpContext.Current.User;
            if (entity.GetType() != typeof(AM_SAP))
            {
                User user = ((User)HttpContext.Current.Session[DataGlobals.ACCOUNT_KEY]);
                ISession sessionNew = NHibernateSession.CurrentFor("nhibernate.current_session").SessionFactory.OpenSession();

                string[] str = getValueString(id, state, propertyNames);
                string tableName = Inflector.Net.Inflector.Pluralize(entity.GetType().Name);
                //sessionOld.CreateSQLQuery("begin pck_audit.saveAuditInfo(?,null,?,?,pck_audit.ACT_INSERT,pck_audit.getIDA(null),"
                sessionNew.CreateSQLQuery("begin pck_audit.saveAuditInfo(?,null,?,?,pck_audit.ACT_INSERT,?,"
                  + "col_name_set_t(" + str[0] + "),"
                  + "null,"
                  + "col_val_set_t(" + str[1] + ")"
                  + "); end;")
                  .SetString(0, user.UserInfo.UserId + " " + user.UserInfo.DomainName)
                  .SetInt32(1, int.Parse(user.ArmId))
                  .SetString(2, tableName)
                  .SetInt32(3, int.Parse(id.ToString()))
                    //.SetString(3, columnString)
                    //.SetString(4, valueString)
                  .ExecuteUpdate();
                sessionNew.Close();
            }
            return false;
        }

        public override void OnDelete(object entity, object id, object[] state, string[] propertyNames, IType[] types)
        {
            if (entity.GetType() != typeof(AM_SAP))
            {
                User user = ((User)HttpContext.Current.Session[DataGlobals.ACCOUNT_KEY]);
                ISession sessionNew = NHibernateSession.CurrentFor("nhibernate.current_session").SessionFactory.OpenSession();

                string[] str = getValueString(id, state, propertyNames);
                string tableName = Inflector.Net.Inflector.Pluralize(entity.GetType().Name);
                
                sessionNew.CreateSQLQuery("begin pck_audit.saveAuditInfo(?,null,?,?,pck_audit.ACT_DELETE,?,"
                  + "col_name_set_t(" + str[0] + "),"
                  + "col_val_set_t(" + str[1] + "),"
                  + "null"
                  + "); end;")
                  .SetString(0, user.UserInfo.UserId + " " + user.UserInfo.DomainName)
                  .SetInt32(1, int.Parse(user.ArmId))
                  .SetString(2, tableName)
                  .SetInt32(3, int.Parse(id.ToString()))
                  .ExecuteUpdate();
                 
                sessionNew.Close();
                
            }
        }

        public override bool OnFlushDirty(object entity, object id, object[] currentState, object[] previousState, string[] propertyNames, IType[] types)
        {
           // return false;
            if (entity.GetType() != typeof(AM_SAP))
            {
                // запоминаем ID последнего объекта
                // в контексте одной сессии БД экземпляры объекта будут равны
                // в новой сессии объект с темже ID не будет равен lastId
                if (lastId != id)
                {
                    lastId = id;
                    User user = ((User)HttpContext.Current.Session[DataGlobals.ACCOUNT_KEY]);
                    ISession sessionNew = NHibernateSession.CurrentFor("nhibernate.current_session").SessionFactory.OpenSession();

                    //sessionOld.BeginTransaction();
                    //Object objectOld = sessionOld.Get(entity.GetType(), id);
                    //sessionOld.Transaction.Commit();

                    //if (objectOld != null)
                    //{
                    //string[] str = getValueString(entity, propertyNames);
                    string[] str = getValueString(id, currentState, propertyNames);
                    string tableName = Inflector.Net.Inflector.Pluralize(entity.GetType().Name);
                    string[] strOld = getValueString(id, previousState, propertyNames);
                    
                    sessionNew.CreateSQLQuery("begin pck_audit.saveAuditInfo(?,null,?,?,pck_audit.ACT_UPDATE,?,"
                          + "col_name_set_t(" + str[0] + "),"
                          + "col_val_set_t(" + strOld[1] + "),"
                          + "col_val_set_t(" + str[1] + ")"
                          + "); end;")
                          .SetString(0, user.UserInfo.UserId + " " + user.UserInfo.DomainName)
                          .SetInt32(1, int.Parse(user.ArmId))
                          .SetString(2, tableName)
                          .SetInt32(3, int.Parse(id.ToString()))
                          .ExecuteUpdate();
               
                    // sessionNew.Close();
                    ////}
                    //return true;
                }
            }
            return false;
        }

        private string[] getValueString(object id, object[] state, string[] propertyNames)
        {
            string[] str = new string[2];
            StringBuilder sb = new StringBuilder();
            StringBuilder sb1 = new StringBuilder();

            //sb.Append("'ID',");
            //sb1.Append("'" + id.ToString() + "',");
            
            for (int i = 0; i < propertyNames.Length; i++)
            {
                
                Object obj = state[i];
                if (!(obj is IList))
                {
                    if (obj != null && obj.GetType().GetProperty("Id") != null)
                    {
                        obj = obj.GetType().GetProperty("Id").GetValue(obj, null);
                        sb.Append("'" + propertyNames[i] + "ID',");
                    }
                    else
                        sb.Append("'" + propertyNames[i] + "',");
                    sb1.Append("'" + obj + "',");
                }
                 
            }

            sb.Remove(sb.Length - 1, 1);
            sb1.Remove(sb1.Length - 1, 1);
            str[0] = sb.ToString();
            str[1] = sb1.ToString();
                 
            return str;
        }
    }
}
