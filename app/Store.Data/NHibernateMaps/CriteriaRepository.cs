using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Data.NHibernate;
using Store.Core.Account;
using Store.Core.RepositoryInterfaces;
using NHibernate.Criterion;
using System.Collections;
using SharpArch.Core.PersistenceSupport;
using Store.Core;
using System.Collections.ObjectModel;

namespace Store.Data
{

    public class CriteriaRepository<T> : Repository<T>, ICriteriaRepository<T>
    {
        public IList<T> GetByCriteria(Dictionary<string, object> queryParams)
        {
            return GetByCriteria(queryParams, null);
        }

        private ICollection GetStringCollection(string value){
            String[] val = value.ToString().Split(',');
            Collection<String> retValue = new Collection<String>();
            for (int i = 0; i < val.Length; i++) {
                retValue.Add(val[i].Replace("'",""));
             }
            return retValue;
        }

        private ICollection GetIntCollection(string value)
        {
            String[] val = value.ToString().Split(',');
            Collection<int> retValue = new Collection<int>();
            for (int i = 0; i < val.Length; i++)
            {
                if (val.Length>0)
                    retValue.Add(int.Parse(val[i]));
            }
            return retValue;
        }

        /*
         Для числовых полей доступны следующие условия:
         []  - null
         [!] - не null
         [>] - больше
         [<] - меньше
         [!=] - не равно
         [>=] - больше или равно
         [<=] - меньше или равно
         [in] - принадлежит множеству
         [!in]- не принадлежит множеству
         */
        public IList<T> GetByCriteria(Dictionary<string, object> queryParams, Dictionary<string, object> orderParams = null)
        {
            string alias;
            IList aliases = new List<string>();
            NHibernate.ICriteria criteria = Session.CreateCriteria(typeof(T));
            foreach (KeyValuePair<string, object> kvp in queryParams)
            {
                string kvpKey = kvp.Key;
                string param = null;
                if (kvpKey.StartsWith("["))
                {
                    param = kvpKey.Substring(0, kvpKey.IndexOf("]")+1);
                    //Убираем спец. символы
                    if ((param == "[!]") || (param == "[]") || (param == "[!=]") || (param == "[>]") || (param == "[<]") || (param == "[>=]") || (param == "[<=]") || (param == "[in]") || (param == "[!in]"))
                    {
                        kvpKey = kvpKey.Substring(kvpKey.IndexOf("]") + 1);
                    }
                }
                if (kvpKey.Contains("."))
                {
                    alias = kvpKey.Substring(0, kvpKey.IndexOf("."));
                    if (!aliases.Contains(alias))
                    {
                        aliases.Add(alias);
                        criteria.CreateAlias(alias, alias);
                    }
                }
                if (param == null)
                    criteria.Add(Restrictions.Eq(kvpKey, kvp.Value));
                else
                {
                    if (param == "[]")
                        criteria.Add(Restrictions.IsNull(kvpKey));
                    else
                        if (param == "[!]")
                            criteria.Add(Restrictions.Not(Restrictions.IsNull(kvpKey)));
                        else
                            if (param == "[!=]")
                                criteria.Add(Restrictions.Not(Restrictions.Eq(kvpKey, kvp.Value)));
                            else
                                if (param == "[>]")
                                    criteria.Add(Restrictions.Gt(kvpKey, kvp.Value));
                                else
                                    if (param == "[in]")
                                    {
                                        ICollection collection;
                                        String val=(string)kvp.Value;
                                        if (val.Contains("'")) collection = GetStringCollection((string)kvp.Value);
                                            else collection = GetIntCollection((string)kvp.Value);
                                        criteria.Add(Restrictions.In(kvpKey, collection));
                                    }
                                    else
                                        if (param == "[!in]")
                                        {
                                            ICollection collection;
                                            String val = (string)kvp.Value;
                                            if (val.Contains("'")) collection = GetStringCollection((string)kvp.Value);
                                            else collection = GetIntCollection((string)kvp.Value);
                                            criteria.Add(Restrictions.Not(Restrictions.In(kvpKey, collection)));
                                        }
                                        else
                                            if (param == "[<]")
                                                criteria.Add(Restrictions.Lt(kvpKey, kvp.Value));
                                            else
                                                if (param == "[>=]")
                                                    criteria.Add(Restrictions.Ge(kvpKey, kvp.Value));
                                                else
                                                    if (param == "[<=]")
                                                        criteria.Add(Restrictions.Le(kvpKey, kvp.Value));
                                                    else
                                                        criteria.Add(Restrictions.Eq(kvpKey, kvp.Value));
                }
            }
            if (orderParams!=null){
                foreach (KeyValuePair<string, object> ovp in orderParams)
                {
                    if (ovp.Key.Contains("."))
                    {
                        alias = ovp.Key.Substring(0, ovp.Key.IndexOf("."));
                        if (!aliases.Contains(alias))
                        {
                            aliases.Add(alias);
                            criteria.CreateAlias(alias, alias);
                        }
                    }
                    criteria.AddOrder(new Order(ovp.Key, (bool)ovp.Value));
                }
            }
            return criteria.List<T>();
        }
        public IList<T> GetByCriteriaIgnoreCase(Dictionary<string, object> queryParams)
        {
            return GetByCriteriaIgnoreCase(queryParams, null);
        }

        public IList<T> GetByCriteriaIgnoreCase(Dictionary<string, object> queryParams, Dictionary<string, object> orderParams = null)
        {
            string alias;
            IList aliases = new List<string>();
            NHibernate.ICriteria criteria = Session.CreateCriteria(typeof(T));
            foreach (KeyValuePair<string, object> kvp in queryParams)
            {
                string kvpKey = kvp.Key;
                string param = null;
                if (kvpKey.StartsWith("["))
                {
                    param = kvpKey.Substring(0, kvpKey.IndexOf("]") + 1);
                    //Убираем спец. символы
                    if ((param == "[!]") || (param == "[]") || (param == "[!=]") || (param == "[>]") || (param == "[<]") || (param == "[>=]") || (param == "[<=]") || (param == "[in]") || (param == "[!in]"))
                    {
                        kvpKey = kvpKey.Substring(kvpKey.IndexOf("]") + 1);
                    }
                }
                if (kvpKey.Contains("."))
                {
                    alias = kvpKey.Substring(0, kvpKey.IndexOf("."));
                    if (!aliases.Contains(alias))
                    {
                        aliases.Add(alias);
                        criteria.CreateAlias(alias, alias);
                    }
                }
                if (param == null)
                    criteria.Add(Restrictions.Eq(kvpKey, kvp.Value).IgnoreCase());
                else
                {
                    if (param == "[]")
                        criteria.Add(Restrictions.IsNull(kvpKey));
                    else
                        if (param == "[!]")
                            criteria.Add(Restrictions.Not(Restrictions.IsNull(kvpKey)));
                        else
                            if (param == "[!=]")
                                criteria.Add(Restrictions.Not(Restrictions.Eq(kvpKey, kvp.Value).IgnoreCase()));
                            else
                                if (param == "[>]")
                                    criteria.Add(Restrictions.Gt(kvpKey, kvp.Value).IgnoreCase());
                                else
                                    if (param == "[in]")
                                    {
                                        ICollection collection;
                                        String val = (string)kvp.Value;
                                        if (val.Contains("'")) collection = GetStringCollection((string)kvp.Value);
                                        else collection = GetIntCollection((string)kvp.Value);
                                        criteria.Add(Restrictions.In(kvpKey, collection));
                                    }
                                    else
                                        if (param == "[!in]")
                                        {
                                            ICollection collection;
                                            String val = (string)kvp.Value;
                                            if (val.Contains("'")) collection = GetStringCollection((string)kvp.Value);
                                            else collection = GetIntCollection((string)kvp.Value);
                                            criteria.Add(Restrictions.Not(Restrictions.In(kvpKey, collection)));
                                        }
                                        else
                                            if (param == "[<]")
                                                criteria.Add(Restrictions.Lt(kvpKey, kvp.Value).IgnoreCase());
                                            else
                                                if (param == "[>=]")
                                                    criteria.Add(Restrictions.Ge(kvpKey, kvp.Value).IgnoreCase());
                                                else
                                                    if (param == "[<=]")
                                                        criteria.Add(Restrictions.Le(kvpKey, kvp.Value).IgnoreCase());
                                                    else
                                                        criteria.Add(Restrictions.Eq(kvpKey, kvp.Value).IgnoreCase());
                }
            }
            if (orderParams != null)
            {
                foreach (KeyValuePair<string, object> ovp in orderParams)
                {
                    if (ovp.Key.Contains("."))
                    {
                        alias = ovp.Key.Substring(0, ovp.Key.IndexOf("."));
                        if (!aliases.Contains(alias))
                        {
                            aliases.Add(alias);
                            criteria.CreateAlias(alias, alias);
                        }
                    }
                    criteria.AddOrder(new Order(ovp.Key, (bool)ovp.Value));
                }
            }
            return criteria.List<T>();
        }


        public IList<T> GetByLikeCriteria(Dictionary<string, object> queryParams)
        {
            return GetByLikeCriteria(queryParams,null);
        }

        /*
         Для числовых полей доступны следующие условия:
         []  - null
         [!] - не null
         [>] - больше
         [<] - меньше
         [!=] - не равно 
         [>=] - больше или равно
         [<=] - меньше или равно
        Для символьных полей доступны следующие условия: 
         []  - null
         [!] - не null
         [!=] - не равно
         [=] - равно
         [^] - оканчивается
         [*] - вхождение
         */
        public IList<T> GetByLikeCriteria(Dictionary<string, object> queryParams, Dictionary<string, object> orderParams = null)
        {
            string alias;
            IList aliases = new List<string>();
            NHibernate.ICriteria criteria = Session.CreateCriteria(typeof(T));
            foreach (KeyValuePair<string, object> kvp in queryParams)
            {
                string kvpKey = kvp.Key;
                string kvpKey1 = "", kvpKey2 = "";
                string param = null;
                if (kvpKey.StartsWith("["))
                {
                    param = kvpKey.Substring(0, kvpKey.IndexOf("]")+1);
                    //Убираем спец. символы
                    if ((param == "[!]") || (param == "[]") || (param == "[^]") || (param == "[!=]") || (param == "[=]") || (param == "[>]") || (param == "[<]") || (param == "[>=]") || (param == "[<=]") || (param == "[in]") || (param == "[!in]") || (param == "[*]"))
                    {
                        kvpKey = kvpKey.Substring(kvpKey.IndexOf("]") + 1);
                    }
                }

                if (kvpKey.Contains("."))
                {
                    alias = kvpKey.Substring(0, kvpKey.IndexOf("."));
                    if (!aliases.Contains(alias))
                    {
                        aliases.Add(alias);
                        criteria.CreateAlias(alias, alias);
                    }
                }
                if (kvp.Value.GetType() == typeof(string))
                {
                    if (param == null)
                        criteria.Add(Restrictions.InsensitiveLike(kvpKey, (string)kvp.Value, MatchMode.Start));
                    else
                    {
                        if (param == "[]")
                            criteria.Add(Restrictions.IsNull(kvpKey));
                        else if (param == "[!]")
                            criteria.Add(Restrictions.Not(Restrictions.IsNull(kvpKey)));
                        else if (param == "[!=]")
                            criteria.Add(Restrictions.Not(Restrictions.InsensitiveLike(kvpKey, (string)kvp.Value, MatchMode.Start)));
                        else if (param == "[^]")
                            criteria.Add(Restrictions.InsensitiveLike(kvpKey, (string)kvp.Value, MatchMode.End));
                        else if (param == "[*]")
                            criteria.Add(Restrictions.InsensitiveLike(kvpKey, (string)kvp.Value, MatchMode.Anywhere));
                        else if (param == "[=]")
                                criteria.Add(Restrictions.Eq(kvpKey, (string)kvp.Value));
                        else
                        if (param == "[in]")
                        {
                            ICollection collection;
                            String val = (string)kvp.Value;
                            if (val.Contains("'")) collection = GetStringCollection((string)kvp.Value);
                            else collection = GetIntCollection((string)kvp.Value);
                            criteria.Add(Restrictions.In(kvpKey, collection));
                        }
                        else
                            if (param == "[!in]")
                            {
                                ICollection collection;
                                String val = (string)kvp.Value;
                                if (val.Contains("'")) collection = GetStringCollection((string)kvp.Value);
                                else collection = GetIntCollection((string)kvp.Value);
                                criteria.Add(Restrictions.Not(Restrictions.In(kvpKey, collection)));
                            }
                        else 
                            criteria.Add(Restrictions.InsensitiveLike(kvpKey, (string)kvp.Value, MatchMode.Start));
                    }
                }
                else if (kvp.Value.GetType() == typeof(DateTime))
                {
                    if (param == null)
                        criteria.Add(Restrictions.Between(kvpKey, kvp.Value, ((DateTime)kvp.Value).AddDays(1)));
                    else
                    {
                        if (param == "[]")
                            criteria.Add(Restrictions.IsNull(kvpKey));
                        else if (param == "[!]")
                            criteria.Add(Restrictions.Not(Restrictions.IsNull(kvpKey)));
                        else if (param == "[!=]")
                            criteria.Add(Restrictions.Not(Restrictions.Eq(kvpKey, kvp.Value)));
                        else if (param == "[>]")
                            criteria.Add(Restrictions.Gt(kvpKey, kvp.Value));
                        else if (param == "[<]")
                            criteria.Add(Restrictions.Lt(kvpKey, kvp.Value));
                        else if (param == "[>=]")
                            criteria.Add(Restrictions.Ge(kvpKey, kvp.Value));
                        else if (param == "[<=]")
                            criteria.Add(Restrictions.Le(kvpKey, kvp.Value));
                        else
                            criteria.Add(Restrictions.Between(kvpKey, kvp.Value, ((DateTime)kvp.Value).AddDays(1)));
                    }
                }
                else
                {
                    if (param == null)
                        criteria.Add(Restrictions.Eq(kvpKey, kvp.Value));
                    else
                    {
                        if (param == "[]")
                            criteria.Add(Restrictions.IsNull(kvpKey));
                        else if (param == "[!]")
                            criteria.Add(Restrictions.Not(Restrictions.IsNull(kvpKey)));
                        else if (param == "[!=]")
                            criteria.Add(Restrictions.Not(Restrictions.Eq(kvpKey, kvp.Value)));
                        else if (param == "[>]")
                            criteria.Add(Restrictions.Gt(kvpKey, kvp.Value));
                        else if (param == "[<]")
                            criteria.Add(Restrictions.Lt(kvpKey, kvp.Value));
                        else if (param == "[>=]")
                            criteria.Add(Restrictions.Ge(kvpKey, kvp.Value));
                        else if (param == "[<=]")
                            criteria.Add(Restrictions.Le(kvpKey, kvp.Value));
                        else
                            if (param == "[in]")
                            {
                                ICollection collection;
                                String val = (string)kvp.Value;
                                if (val.Contains("'")) collection = GetStringCollection((string)kvp.Value);
                                else collection = GetIntCollection((string)kvp.Value);
                                criteria.Add(Restrictions.In(kvpKey, collection));
                            }
                            else
                                if (param == "[!in]")
                                {
                                    ICollection collection;
                                    String val = (string)kvp.Value;
                                    if (val.Contains("'")) collection = GetStringCollection((string)kvp.Value);
                                    else collection = GetIntCollection((string)kvp.Value);
                                    criteria.Add(Restrictions.Not(Restrictions.In(kvpKey, collection)));
                                }
                                else
                            criteria.Add(Restrictions.Eq(kvpKey, kvp.Value));
                    }
                }
            }
            if (orderParams != null)
            {
                foreach (KeyValuePair<string, object> ovp in orderParams)
                {
                    if (ovp.Key.Contains("."))
                    {
                        alias = (string)ovp.Key.Substring(0, ovp.Key.IndexOf("."));
                        if (!aliases.Contains(alias))
                        {
                            aliases.Add(alias);
                            criteria.CreateAlias(alias, alias);
                        }
                    }
                    criteria.AddOrder(new Order((string)ovp.Key, (bool)ovp.Value));
                }
            }
            return criteria.List<T>();
        }
    }
}

