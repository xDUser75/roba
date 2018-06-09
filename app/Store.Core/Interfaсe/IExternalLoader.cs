using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpArch.Data.NHibernate;
using SharpArch.Core.PersistenceSupport.NHibernate;
using Store.Core.RepositoryInterfaces;

namespace Store.Core.External.Interfaсe
{
    public interface IExternalLoaderOrganization 
    {
        string LoadOrganization(string shopId, string shopNumber, string organizationId, string sessionId, string childCare);
    }

    public interface IExternalLoaderNomenclature
    {
        string LoadNomenclature(string organizationId, string sessionId);
    }

    public interface IExternalLoaderInvoice
    {
        List<COMING_SAP> GetInvoice(Organization currentOrganization, StorageName currentStorage, ICriteriaRepository<Nomenclature> nomenRepository, ICriteriaRepository<NomGroup> nomGroupRepository, int DocTypeId, string docNumber, int docYear, string docDate, out string Message);
    }

    public interface IExternalExportData
    {
        void ExportData(string shopId, string shopNumber, string organizationId, string sessionId);
    }

    public interface IExportConsumption
    {
        string ExportConsumption(int currenOrganization, DateTime dateN, DateTime dateEnd, int ceh, int operTypeId, int storageId, string uchastokId, int? paramSplit, int? paramTabN, string nameNakl, string param1, int param2);
    }

}
