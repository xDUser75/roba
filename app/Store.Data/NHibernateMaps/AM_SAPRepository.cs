using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Store.Core;
using SharpArch.Core.PersistenceSupport.NHibernate;
using Store.Core.RepositoryInterfaces;
using NHibernate;
using SharpArch.Data.NHibernate;
using NHibernate.Criterion;
using SharpArch.Web.NHibernate;
using Store.Data.NHibernateMaps;
using SAP.Middleware.Connector;
using System.Globalization;
using Store.Core.External.Interfaсe;
using Store.Core.Utils;
using System.Configuration;

namespace Store.Data.NHibernateMaps
{
    // Стандартное движение между складами SAP. Класс можно использовать для различных организаций и SAPов
    // используем для ЗСМК и ЕВРАЗРУДЫ
    public class AM_SAPRepository : CriteriaRepository<AM_SAP>, IExternalLoaderInvoice, IExportConsumption
    {
        //const string INCOMING_RFC = "SAP_INCOMING";

        private string[] getMaterialName(RfcDestination destination, string MaterialId)
        {
            string[] material = new string[2];
            IRfcFunction function = null;
            try
            {
                function = destination.Repository.CreateFunction("BAPI_MATERIAL_GET_DETAIL");
                function.SetValue("material", MaterialId);
                function.Invoke(destination);
            }
            catch (RfcBaseException e)
            {
                Console.WriteLine(e.ToString());
            }
            IRfcStructure retData = (IRfcStructure)function.GetValue("material_general_data");
            material[0] = (string)retData.GetValue("MATL_DESC");
            material[1] = (string)retData.GetValue("MATL_GROUP");
            return material;
        }

        private string getNomenclatureGroupName(RfcDestination destination, string groupId)
        {
            string groupName = "";
            IRfcFunction function = null;
            try
            {
                function = destination.Repository.CreateFunction("RFC_MERCHANDISE_GROUP_READ");
                IRfcTable table = function.GetTable("PI_MGDATA");
                table.Append();
                table.CurrentRow.SetValue("MATKL", groupId);
                table.CurrentRow.SetValue("SPART", "");
                table.CurrentRow.SetValue("LREF3", "");
                table.CurrentRow.SetValue("WWGDA", "");
                table.CurrentRow.SetValue("WWGPA", "");
                table.CurrentRow.SetValue("ABTNR", "");
                table.CurrentRow.SetValue("BEGRU", "");
                table.CurrentRow.SetValue("SPRAS", "");
                table.CurrentRow.SetValue("WGBEZ", "");
                table.CurrentRow.SetValue("WGBEZ60", "");
                table.CurrentRow.SetValue("FLDELETE", "");
                function.Invoke(destination);
            }
            catch (RfcBaseException e)
            {
                Console.WriteLine(e.ToString());
            }
            IRfcTable oTable = function.GetTable("po_mgdata");
            for (int i = 0; i < oTable.RowCount; i++)
            {
                oTable.CurrentIndex = i;
                groupName = oTable.GetString("WGBEZ60");
            }
            return groupName;
        }

        public List<COMING_SAP> GetInvoice(Organization currentOrganization, StorageName currentStorage, ICriteriaRepository<Nomenclature> nomenRepository, ICriteriaRepository<NomGroup> nomGroupRepository, int DocTypeId, string docNumber, int docYear, string docDate, out string Message)
        {
            String INCOMING_RFC = ConfigurationManager.AppSettings["SAP_ERP_" + currentOrganization.Id];
            RfcDestination destination = RfcDestinationManager.GetDestination(INCOMING_RFC);
            IRfcFunction function = null;
            List<COMING_SAP> list = new List<COMING_SAP>();
//            string docDate = "01.01." + docYear;
            docDate = "01.01." + docYear;

            if (currentStorage == null) currentStorage = new StorageName();
            try
            {
                function = destination.Repository.CreateFunction("BAPI_GOODSMVT_GETDETAIL");
                
                function.SetValue("MATERIALDOCUMENT", docNumber);
                function.SetValue("MATDOCUMENTYEAR", docYear);
                function.Invoke(destination);
            }
            catch (RfcBaseException e)
            {
                Message = e.StackTrace;
                //                Console.WriteLine(e.ToString());
            }
            Dictionary<string, Nomenclature> MaterialName = new Dictionary<string, Nomenclature>();
            Dictionary<string, string> NomenclatureGroupName = new Dictionary<string, string>();
            Message = "OK";
            IRfcStructure tableHeader = function.GetStructure("GOODSMVT_HEADER");
            if (tableHeader.Count > 0)
            {
                if (tableHeader.GetString("DOC_DATE") != "0000-00-00")
                {
                    docDate = tableHeader.GetString("DOC_DATE");
                    try
                    {
                        docDate = DateTime.ParseExact(docDate, "yyyy-MM-dd", new System.Globalization.CultureInfo("ru-RU", true)).ToString(DataGlobals.DATE_FORMAT_FULL_YEAR);
                    }
                    catch
                    {
                    }
                }
            }
            IRfcTable table = function.GetTable("GOODSMVT_ITEMS");
            if (table.RowCount == 0)
            {
                IRfcTable retTable = function.GetTable("RETURN");
                retTable.CurrentIndex = 0;
                Message = retTable.GetString("MESSAGE");
            }
            else
            {
                for (int i = 0; i < table.RowCount; i++)
                {
                    table.CurrentIndex = i;
                    if ((table.GetString("PLANT") == "" + currentStorage.Plant) && (table.GetString("STGE_LOC") == "" + currentStorage.Externalcode))
                    {
                        //if (getKnownMove(table.GetString("MOVE_TYPE"), currentStorage) != MoveType.Unknown)
                        //{
                        COMING_SAP incomingSap = new COMING_SAP(i + 1);
                        incomingSap.DocTypeId = DocTypeId;
                        incomingSap.DocNumber = docNumber;
                        //incomingSap.DocDate = "01.01." + docYear;
                        incomingSap.DocDate = docDate;
                        incomingSap.MaterialId = table.GetString("MATERIAL").TrimStart('0');
                        Nomenclature defValue = null;
                        if (MaterialName.TryGetValue(incomingSap.MaterialId, out defValue))
                        {
                            incomingSap.MATERIAL = defValue.Name;
                            incomingSap.ExternalCode = defValue.ExternalCode;
                            if (defValue.Growth != null)
                            {
                                incomingSap.GrowthId = defValue.Growth.Id;
                                incomingSap.GrowthName = defValue.Growth.SizeNumber;
                            }
                            if (defValue.NomBodyPartSize != null)
                            {
                                incomingSap.SizeId = defValue.NomBodyPartSize.Id;
                                incomingSap.SizeName = defValue.NomBodyPartSize.SizeNumber;
                            }
                            if (defValue.NomGroup != null)
                            {
                                incomingSap.NomGroupId = defValue.NomGroup.Id;
                                incomingSap.NomGroupName = defValue.NomGroup.Name;
                                incomingSap.IsWinter = defValue.NomGroup.IsWinter;
                            }
                            if (defValue.Sex != null)
                            {
                                incomingSap.SexId = defValue.Sex.Id;
                                incomingSap.SexName = defValue.Sex.Name;
                            }
                            if (defValue.Unit != null)
                            {
                                incomingSap.UnitId = defValue.Unit.Id;
                                incomingSap.UnitName = defValue.Unit.Name;
                            }
                            if (defValue.NomBodyPart != null)
                            {
                                incomingSap.NomBodyPartId = defValue.NomBodyPart.Id;
                                incomingSap.NomBodyPartName = defValue.NomBodyPart.Name;
                            }
                        }
                        else
                        {
                            Dictionary<string, object> queryParams = new Dictionary<string, object>();
                            queryParams.Add("Organization", currentOrganization);
                            queryParams.Add("ExternalCode", incomingSap.MaterialId.TrimStart('0'));
                            queryParams.Add("IsActive", true);
                            //                                Nomenclature currentNomenclature = nomenRepository.FindOne(queryParams);
                            IList<Nomenclature> currentNomenclatures = nomenRepository.GetByLikeCriteria(queryParams);
                            Nomenclature currentNomenclature = null;
                            if (currentNomenclatures.Count > 0)
                                currentNomenclature = currentNomenclatures[0];
                            if (currentNomenclature != null)
                            {
                                // TODO: Требуется обновить группу номенклатуры из SAP?
                                defValue = currentNomenclature;
                                incomingSap.MATERIAL = defValue.Name;
                                incomingSap.ExternalCode = defValue.ExternalCode;
                                if (defValue.NomGroup != null)
                                {
                                    incomingSap.NomGroupId = defValue.NomGroup.Id;
                                    incomingSap.NomGroupName = defValue.NomGroup.Name;
                                    incomingSap.IsWinter = defValue.NomGroup.IsWinter;
                                }
                                if (defValue.Sex != null)
                                {
                                    incomingSap.SexId = defValue.Sex.Id;
                                    incomingSap.SexName = defValue.Sex.Name;
                                }
                                if (defValue.Unit != null)
                                {
                                    incomingSap.UnitId = defValue.Unit.Id;
                                    incomingSap.UnitName = defValue.Unit.Name;
                                }
                                if (defValue.NomBodyPart != null)
                                {
                                    incomingSap.NomBodyPartId = defValue.NomBodyPart.Id;
                                    incomingSap.NomBodyPartName = defValue.NomBodyPart.Name;
                                }
                            }
                            else
                            {
                                //Группу из сапа пока не запрашиваю!!!!
                                /*                                defValue = getMaterialName(destination, incomingSap.MaterialId);
                                                                if (NomenclatureGroupName.TryGetValue(incomingSap.NomrnclatureGroupId, out defStringValue))
                                                                {
                                                                    incomingSap.NomrnclatureGroupName = defStringValue;
                                                                }
                                                                else
                                                                {
                                                                    defStringValue = getNomenclatureGroupName(destination, incomingSap.NomrnclatureGroupId);
                                                                    incomingSap.NomrnclatureGroupName = defStringValue;
                                                                    NomenclatureGroupName.Add(incomingSap.NomrnclatureGroupId, defStringValue);
                                                                }*/
                                if (Message == "OK") Message = "";
                                defValue = new Nomenclature(int.Parse(incomingSap.MaterialId));
                                String[] strArray = getMaterialName(destination, table.GetString("MATERIAL"));
                                defValue.Name = strArray[0];
                                defValue.ExternalCode = strArray[1].TrimStart('0');
                                Message = Message + "Номенклатура: [" + incomingSap.MaterialId + "]" + defValue.Name + "не найдена.\n";
                                incomingSap.MATERIAL = defValue.Name;
                                incomingSap.ExternalCode = incomingSap.MaterialId;
                                queryParams.Clear();
                                queryParams.Add("Organization", currentOrganization);
                                queryParams.Add("ExternalCode", strArray[1].TrimStart('0'));
                                NomGroup nGroup = nomGroupRepository.FindOne(queryParams);
                                if (nGroup != null)
                                {
                                    incomingSap.NomGroupId = nGroup.Id;
                                    incomingSap.NomGroupName = nGroup.Name;
                                    incomingSap.IsWinter = nGroup.IsWinter;
                                    incomingSap.NomBodyPartId = nGroup.NomBodyPart.Id;
                                    incomingSap.NomBodyPartName = nGroup.NomBodyPart.Name;
                                }
                                incomingSap.SAPNomGroupId = strArray[1].TrimStart('0');
                                incomingSap.SAPNomGroupName = getNomenclatureGroupName(destination, strArray[1]);
                                defValue.NomGroup = nGroup;
                            }
                            if (defValue != null)
                            {
                                MaterialName.Add(incomingSap.MaterialId, defValue);
                            }
                        }

                        incomingSap.QUANTITY = (int)Math.Truncate(table.GetDouble("ENTRY_QNT"));
                        incomingSap.UOM = table.GetString("ENTRY_UOM");
                        incomingSap.LC = table.GetString("AMOUNT_LC");
                        incomingSap.SV = table.GetString("AMOUNT_SV");
                        incomingSap.MoveType = table.GetString("MOVE_TYPE");
                        //NomenclatureInfo nInfo = getMaterialDetail(destination, incomingSap.MaterialId);
                        list.Add(incomingSap);
                        //}
                        //else {
                        //    Message = table.GetString("MOVE_TYPE") + " вид движения не поддерживается!";
                        //}
                    }
                }
            }
            return list;
        }

        public string ExportConsumption(int currenOrganization, DateTime dateN, DateTime dateEnd, int ceh, int operTypeId, int storageId, string uchastokId, int? paramSplit, int? paramTabN, string nameNakl, string param1, int param2)
        {
            return null;
        }
    }

}
