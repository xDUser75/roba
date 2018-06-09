using System;
using System.Collections.Generic;
using Store.Core;
using Store.Core.RepositoryInterfaces;
using SharpArch.Data.NHibernate;
using System.Data;
using System.Linq;
using System.Configuration;
using Store.Core.External.Interfaсe;

namespace Store.Data.NHibernateMaps
{

    public class VGOKDataRepository : NHibernateRepositoryWithTypedId<VGOKData, string>, IExternalLoaderInvoice, IExportConsumption
    {
        private string DbPapamPrefix
        {
            get
            {
                Type connectionType = this.Session.Connection.GetType();
                if (connectionType.Name.Contains("Oracle"))
                    return ":";
                else return "@";
            }
        }

        //private static int SharpToGalDate(string dt)
        //{
        //    return SharpToGalDate(DateTime.ParseExact(dt, "dd.MM.yyyy", new System.Globalization.CultureInfo("ru-RU", true)));
        //}

        private static int SharpToGalDate(DateTime dt)
        {
            string binaryDay = Convert.ToString(dt.Day, 2);
            binaryDay = binaryDay.PadLeft(8, '0');
            string binaryMonth = Convert.ToString(dt.Month, 2);
            binaryMonth = binaryMonth.PadLeft(8, '0');
            string binaryYear = Convert.ToString(dt.Year, 2);
            binaryYear = binaryYear.PadLeft(16, '0');
            return Convert.ToInt32(binaryYear + binaryMonth + binaryDay, 2);
        }

        private static DateTime GalToSharpDate(int value)
        {
            if (value == 0) return new DateTime(1, 1, 1, 0, 0, 0, 0);
            string binValue = Convert.ToString(value, 2);
            binValue = binValue.PadLeft(32, '0');
            int year = Convert.ToInt32(binValue.Substring(0, 16), 2);
            int month = Convert.ToInt32(binValue.Substring(16, 8), 2);
            int day = Convert.ToInt32(binValue.Substring(24, 8), 2);
            DateTime retDate = new DateTime(year, month, day);
            return retDate;
        }

        public List<COMING_SAP> GetInvoice(Organization currentOrganization, StorageName currentStorage, ICriteriaRepository<Nomenclature> nomenRepository, ICriteriaRepository<NomGroup> nomGroupRepository,
                                   int DocTypeId, string docNumber, int docYear, string docDate, out string Message)
        {
            List<COMING_SAP> model = new List<COMING_SAP>();
            IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
            Dictionary<string, Object> param = new Dictionary<string, Object>();
            Message = "OK";
            string paramPrefix = DbPapamPrefix;
            int rowCount = 0;
            IDbConnection dbConnection = this.Session.Connection;
            if (dbConnection.State == ConnectionState.Closed)
                dbConnection.Open();
            using (var cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = "select * from COMING_SAPS where DocNumber=" + paramPrefix + "DocNumber and GalDocDate>=" + paramPrefix + "DocDate1 and GalDocDate<=" + paramPrefix + "DocDate2 and StorageNameExternalCode=" + paramPrefix + "StorageCode" + (docDate != "" ? " and GalDocDate=" + paramPrefix + "DocDate" : "");
                cmd.CommandType = CommandType.Text;

                var inval = cmd.CreateParameter();
                inval.ParameterName = "DocNumber";
                inval.DbType = DbType.String;
                inval.Value = docNumber;
                cmd.Parameters.Add(inval);

                inval = cmd.CreateParameter();
                inval.ParameterName = "DocDate1";
                inval.DbType = DbType.Int32;
                inval.Value = SharpToGalDate(DateTime.ParseExact("01.01." + docYear, "dd.MM.yyyy", culture));
                cmd.Parameters.Add(inval);

                inval = cmd.CreateParameter();
                inval.ParameterName = "DocDate2";
                inval.DbType = DbType.Int32;
                inval.Value = SharpToGalDate(DateTime.ParseExact("31.12." + docYear, "dd.MM.yyyy", culture));
                cmd.Parameters.Add(inval);

                inval = cmd.CreateParameter();
                inval.ParameterName = "StorageCode";
                inval.DbType = DbType.String;
                inval.Value = currentStorage.Externalcode;
                cmd.Parameters.Add(inval);

                if (docDate != "")
                {
                    inval = cmd.CreateParameter();
                    inval.ParameterName = "DocDate";
                    inval.DbType = DbType.Int32;
                    inval.Value = SharpToGalDate(DateTime.ParseExact(docDate, "dd.MM.yyyy", culture));
                    cmd.Parameters.Add(inval);
                }

                try
                {
                    using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while ((reader.IsClosed == false) && reader.Read())
                        {
                            rowCount++;
                            COMING_SAP item = new COMING_SAP();
                            item.DocNumber = Convert.ToString(reader["DOCNUMBER"]);
                            item.DocDate = GalToSharpDate(Convert.ToInt32(reader["GALDOCDATE"])).ToString(DataGlobals.DATE_FORMAT_FULL_YEAR);
                            item.StorageNameExternalCode = Convert.ToString(reader["STORAGENAMEEXTERNALCODE"]);
                            item.StorageName = Convert.ToString(reader["STORAGENAME"]);
                            item.ExternalCode = Convert.ToString(reader["EXTERNALCODE"]);
                            item.MaterialId = Convert.ToString(reader["MATERIALID"]);
                            item.MATERIAL = Convert.ToString(reader["MATERIAL"]);
                            item.SAPNomGroupId = Convert.ToString(reader["SAPNOMGROUPID"]);
                            item.SAPNomGroupName = Convert.ToString(reader["SAPNOMGROUPNAME"]);
                            item.QUANTITY = Convert.ToInt32(reader["QUANTITY"]);
                            item.UnitExternalCode = Convert.ToString(reader["UNITEXTERNALCODE"]);
                            item.UnitName = Convert.ToString(reader["UNITNAME"]);
                            item.SizeName = Convert.ToString(reader["SIZENAME"]);
                            item.GrowthName = Convert.ToString(reader["GROWTHNAME"]);
                            item.DocTypeId = DocTypeId;
                            model.Add(item);
                        }
                    }
                }
                catch (Exception e)
                {
                    Message = e.Message;
                }
            }

            if (model.GroupBy(g => g.DocNumber, g => g.DocDate).Count() > 1)
            {
                Message = "С номером " + model[0].DocNumber + " несколько документов!!! Укажите точную дату документа";
                model.Clear();
            }
            else
            {
                param.Clear();
                param.Add("Organization.Id", currentOrganization.Id);
                param.Add("IsActive", true);
                foreach (var item in model)
                {
                    param["ExternalCode"] = item.ExternalCode;
                    
                    IList<Nomenclature> nomenclatures = nomenRepository.GetByLikeCriteria(param);
                    if (nomenclatures.Count != 0)
                    {
                        Nomenclature nomenclature = nomenclatures[0];
                        item.MaterialId = nomenclature.ExternalCode;
                        if (nomenclature.Growth != null)
                        {
                            item.GrowthId = nomenclature.Growth.Id;
                            item.GrowthName = nomenclature.Growth.SizeNumber;
                        }
                        item.IsWinter = nomenclature.NomGroup.IsWinter;
                        item.MATERIAL = nomenclature.Name;
                        item.NomBodyPartId = nomenclature.NomBodyPart.Id;
                        item.NomBodyPartName = nomenclature.NomBodyPart.Name;
                        item.NomGroupId = nomenclature.NomGroup.Id;
                        item.NomGroupName = nomenclature.NomGroup.Name;
                        item.SexId = nomenclature.Sex.Id;
                        item.SexName = nomenclature.Sex.Name;
                        if (nomenclature.NomBodyPartSize != null)
                        {
                            item.SizeId = nomenclature.NomBodyPartSize.Id;
                            item.SizeName = nomenclature.NomBodyPartSize.SizeNumber;
                        }
                        item.UnitId = nomenclature.Unit.Id;
                        item.UnitName = nomenclature.Unit.Name;
                    }
                }
            }
            return model;
        }
/*
        public List<COMING_SAP> GetInvoice1(Organization currentOrganization, StorageName currentStorage, ICriteriaRepository<Nomenclature> nomenRepository, ICriteriaRepository<NomGroup> nomGroupRepository, 
                                           int DocTypeId, string docNumber, int docYear, string docDate, out string Message)
        {
            IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);
            CriteriaRepository<COMING_SAP> comingRepository = new CriteriaRepository<COMING_SAP>();
            List<COMING_SAP> model = new List<COMING_SAP>();
            Dictionary<string, Object> param = new Dictionary<string, Object>();
            param.Add("DocNumber", docNumber);
            param.Add("[>=]GalDocDate", Store.Core.Utils.ConvertTypes.SharpToGalDate(DateTime.ParseExact("01.01." + docYear, "dd.MM.yyyy", culture)));
            param.Add("[<=]GalDocDate", Store.Core.Utils.ConvertTypes.SharpToGalDate(DateTime.ParseExact("31.12." + docYear, "dd.MM.yyyy", culture)));
            param.Add("StorageNameExternalCode", currentStorage.Externalcode);
            if (docDate!="")
                param.Add("GalDocDate", Store.Core.Utils.ConvertTypes.SharpToGalDate(DateTime.ParseExact(docDate, "dd.MM.yyyy", culture)));

            Message = "OK";
            try
            {
                model = (List<COMING_SAP>)comingRepository.GetByCriteria(param);
                string docD="";
                foreach (var doc in model)
                {
                    if (docD!="" && docD!=doc.DocDate)
                        Message = "С номером "+doc.DocNumber+" несколько документов!!! Укажите точную дату документа";
                    docD = doc.DocDate;
                }
                if (Message == "OK")
                {
                    foreach (var incoming in model)
                    {
                        incoming.DocTypeId = DocTypeId;
                        param.Clear();
                        param.Add("Organization.Id", currentOrganization.Id);
                        param.Add("ExternalCode", incoming.ExternalCode);
                        param.Add("IsActive", true);

                        IList<Nomenclature> nomenclatures = nomenRepository.GetByLikeCriteria(param);
                        if (nomenclatures.Count != 0)
                        {
                            Nomenclature nomenclature = nomenclatures[0];
                            //                        incoming.MaterialId = nomenclature.Id.ToString();
                            incoming.MaterialId = nomenclature.ExternalCode;
                            if (nomenclature.Growth != null)
                            {
                                incoming.GrowthId = nomenclature.Growth.Id;
                                incoming.GrowthName = nomenclature.Growth.SizeNumber;
                            }
                            incoming.IsWinter = nomenclature.NomGroup.IsWinter;
                            incoming.MATERIAL = nomenclature.Name;
                            incoming.NomBodyPartId = nomenclature.NomBodyPart.Id;
                            incoming.NomBodyPartName = nomenclature.NomBodyPart.Name;
                            incoming.NomGroupId = nomenclature.NomGroup.Id;
                            incoming.NomGroupName = nomenclature.NomGroup.Name;
                            incoming.SexId = nomenclature.Sex.Id;
                            incoming.SexName = nomenclature.Sex.Name;
                            if (nomenclature.NomBodyPartSize != null)
                            {
                                incoming.SizeId = nomenclature.NomBodyPartSize.Id;
                                incoming.SizeName = nomenclature.NomBodyPartSize.SizeNumber;
                            }
                            incoming.UnitId = nomenclature.Unit.Id;
                            incoming.UnitName = nomenclature.Unit.Name;
                        }
                    }
                }
            }
            catch (Exception e) 
            {
                Message = e.ToString();
            }
            return model;
        }
*/
        private void ExportData(Dictionary<string, string> parameters, string sessionId)
        {

            Session.BeginTransaction();
            Session.CreateSQLQuery("begin SAP_ORGANIZATION.test; end; ").ExecuteUpdate();
            Session.Flush();
        }

        public string ExportConsumption(int currenOrganization, DateTime dateN, DateTime dateEnd, int ceh, int operTypeId, int storageId, string uchastokId, int? paramSplit,int? paramTabN, string nameNakl, string param1, int param2)
        {
            string errorStr = null;
            Session.Transaction.Begin();
            try
            {
                var conn = Session.Connection;
                
                IDbCommand cmd = conn.CreateCommand();
                bool isDebug = bool.Parse(ConfigurationManager.AppSettings["isDebug"]);
                if (isDebug) cmd.CommandText = "gal.cloth_at_zsmk.Insert_Akt_2_Galaxy@vgoktest";
                else cmd.CommandText = "gal.cloth_at_zsmk.Insert_Akt_2_Galaxy@vgok";
                cmd.CommandType = CommandType.StoredProcedure;


                var parameter = cmd.CreateParameter();
                parameter.ParameterName = "@nameNakl";
                parameter.Value = nameNakl;
                cmd.Parameters.Add(parameter);

                parameter = cmd.CreateParameter();
                parameter.ParameterName = "@paramOrganization";
                parameter.Value = currenOrganization;
                cmd.Parameters.Add(parameter);

                parameter = cmd.CreateParameter();
                parameter.ParameterName = "@dateN";
                parameter.Value = dateN;
                cmd.Parameters.Add(parameter);
                parameter = cmd.CreateParameter();
                parameter.ParameterName = "@dateEnd";
                parameter.Value = dateEnd;
                cmd.Parameters.Add(parameter);

                parameter = cmd.CreateParameter();
                parameter.ParameterName = "@ceh";
                parameter.Value = ceh;
                cmd.Parameters.Add(parameter);

                parameter = cmd.CreateParameter();
                parameter.ParameterName = "@operTypeId";
                parameter.Value = operTypeId;
                cmd.Parameters.Add(parameter);

                parameter = cmd.CreateParameter();
                parameter.ParameterName = "@paramStorage";
                parameter.Value = storageId;
                cmd.Parameters.Add(parameter);

                parameter = cmd.CreateParameter();
                parameter.ParameterName = "@paramUchastokId";
                parameter.Value = uchastokId;
                cmd.Parameters.Add(parameter);

                parameter = cmd.CreateParameter();
                parameter.ParameterName = "@paramSplit";
                parameter.Value = paramSplit;
                cmd.Parameters.Add(parameter);

                parameter = cmd.CreateParameter();
                parameter.ParameterName = "@paramTabN";
                parameter.Value = paramTabN;
                cmd.Parameters.Add(parameter);

                cmd.ExecuteNonQuery();
                Session.Flush();
                Session.Transaction.Commit();
            }
            catch (Exception e)
            {
                Session.Transaction.Rollback();
                errorStr = e.Message;
            }
            return errorStr;
        }

    }
}
