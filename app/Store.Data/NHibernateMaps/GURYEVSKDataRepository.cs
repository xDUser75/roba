using System;
using System.Collections.Generic;
using Store.Core;
using Store.Core.RepositoryInterfaces;
using SharpArch.Data.NHibernate;
using System.Data;
using System.Linq;
using Store.Core.External.Interfaсe;

namespace Store.Data.NHibernateMaps
{

    public class GURYEVSKDataRepository : NHibernateRepository<LoadData>, IExternalLoaderInvoice, IExportConsumption
    {
        private string DbPapamPrefix {
            get
            {
                Type connectionType = this.Session.Connection.GetType();
                if (connectionType.Name.Contains("Oracle"))
                    return ":";
                else return "@";
            }
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
                cmd.CommandText = "select * from \"1C_STORE_GUR\".invoice where DocNumber=" + paramPrefix + "DocNumber and InvoiceDate>=" + paramPrefix + "DocDate1 and InvoiceDate<=" + paramPrefix + "DocDate2 and StorageNameExternalCode=" + paramPrefix + "StorageCode" + (docDate != "" ? " and InvoiceDate=" + paramPrefix + "DocDate" : "");
                cmd.CommandType = CommandType.Text;

                var inval = cmd.CreateParameter();
                inval.ParameterName = "DocNumber";
                inval.DbType = DbType.String;
                inval.Value = docNumber;
                cmd.Parameters.Add(inval);

                inval = cmd.CreateParameter();
                inval.ParameterName = "DocDate1";
                inval.DbType = DbType.Date;
                inval.Value = DateTime.ParseExact("01.01." + docYear, "dd.MM.yyyy", culture);
                cmd.Parameters.Add(inval);

                inval = cmd.CreateParameter();
                inval.ParameterName = "DocDate2";
                inval.DbType = DbType.Date;
                inval.Value = DateTime.ParseExact("31.12." + docYear, "dd.MM.yyyy", culture);
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
                    inval.DbType = DbType.Date;
                    inval.Value = DateTime.ParseExact(docDate, "dd.MM.yyyy", culture);
                    cmd.Parameters.Add(inval);
                }

                try
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rowCount++;
                            COMING_SAP item = new COMING_SAP();
                            item.DocNumber = Convert.ToString(reader["DOCNUMBER"]);
                            if (reader["INVOICEDATE"] != null)
                            {
                                item.DocDate = ((DateTime)reader["INVOICEDATE"]).ToString(DataGlobals.DATE_FORMAT_FULL_YEAR);
                            }
                            else
                            {
                                Message = "В накладной " + docNumber + " отсутствует дата!";
                            }
                            item.StorageNameExternalCode = Convert.ToString(reader["STORAGENAMEEXTERNALCODE"]);
                            item.StorageName = Convert.ToString(reader["STORAGENAME"]);
                            item.ExternalCode = Convert.ToString(reader["EXTERNALCODE"]);
                            item.QUANTITY = Convert.ToInt32(reader["QUANTITY"]);

                            item.DocTypeId = DocTypeId;
                            model.Add(item);
                        }
                        reader.Close();
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
                foreach (var item in model)
                {
                    param["ExternalCode"] = item.ExternalCode;
                    //param.Add("IsActive", true);
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
                    else {
                        item.SexName = "";
                    }
                }
            }

            return model;
        }

        private void ExportData(Dictionary<string, string> parameters, string sessionId)
        {
            //Session.BeginTransaction();
            //Session.CreateSQLQuery("begin SAP_ORGANIZATION.test; end; ").ExecuteUpdate();
            //Session.Flush();
        }

        public string ExportConsumption(int currenOrganization, DateTime dateN, DateTime dateEnd, int ceh, int operTypeId, int storageId, string uchastokId, int? paramSplit,int? paramTabN, string nameNakl, string param1, int param2)
        {
            string errorStr = null;
            //Session.Transaction.Begin();
            //try
            //{
            //    var conn = Session.Connection;
                
            //    IDbCommand cmd = conn.CreateCommand();
            //    bool isDebug = bool.Parse(ConfigurationManager.AppSettings["isDebug"]);
            //    if (isDebug) cmd.CommandText = "gal.cloth_at_zsmk.Insert_Akt_2_Galaxy@vgoktest";
            //    else cmd.CommandText = "gal.cloth_at_zsmk.Insert_Akt_2_Galaxy@vgok";
            //    cmd.CommandType = CommandType.StoredProcedure;


            //    var parameter = cmd.CreateParameter();
            //    parameter.ParameterName = "@nameNakl";
            //    parameter.Value = nameNakl;
            //    cmd.Parameters.Add(parameter);

            //    parameter = cmd.CreateParameter();
            //    parameter.ParameterName = "@paramOrganization";
            //    parameter.Value = currenOrganization;
            //    cmd.Parameters.Add(parameter);

            //    parameter = cmd.CreateParameter();
            //    parameter.ParameterName = "@dateN";
            //    parameter.Value = dateN;
            //    cmd.Parameters.Add(parameter);
            //    parameter = cmd.CreateParameter();
            //    parameter.ParameterName = "@dateEnd";
            //    parameter.Value = dateEnd;
            //    cmd.Parameters.Add(parameter);

            //    parameter = cmd.CreateParameter();
            //    parameter.ParameterName = "@ceh";
            //    parameter.Value = ceh;
            //    cmd.Parameters.Add(parameter);

            //    parameter = cmd.CreateParameter();
            //    parameter.ParameterName = "@operTypeId";
            //    parameter.Value = operTypeId;
            //    cmd.Parameters.Add(parameter);

            //    parameter = cmd.CreateParameter();
            //    parameter.ParameterName = "@paramStorage";
            //    parameter.Value = storageId;
            //    cmd.Parameters.Add(parameter);

            //    parameter = cmd.CreateParameter();
            //    parameter.ParameterName = "@paramUchastokId";
            //    parameter.Value = uchastokId;
            //    cmd.Parameters.Add(parameter);

            //    parameter = cmd.CreateParameter();
            //    parameter.ParameterName = "@paramSplit";
            //    parameter.Value = paramSplit;
            //    cmd.Parameters.Add(parameter);

            //    parameter = cmd.CreateParameter();
            //    parameter.ParameterName = "@paramTabN";
            //    parameter.Value = paramTabN;
            //    cmd.Parameters.Add(parameter);

            //    cmd.ExecuteNonQuery();
            //    Session.Flush();
            //    Session.Transaction.Commit();
            //}
            //catch (Exception e)
            //{
            //    Session.Transaction.Rollback();
            //    errorStr = e.Message;
            //}
            return errorStr;
        }

    }
}
