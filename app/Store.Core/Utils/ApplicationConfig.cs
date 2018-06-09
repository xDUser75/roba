using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace Store.Core.Utils
{
    public class ApplicationConfig
    {
        private static string XML_FILE = "ApplicationConfig.xml";

        public static string ReadVariable(string xPath) 
        {
            // Создаем экземпляр класса
            XmlDocument xmlDoc = new XmlDocument();
            // Загружаем XML-документ из файла
            var physicalFilePath = Path.Combine(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, XML_FILE);
            xmlDoc.Load(physicalFilePath);
            XmlNodeList list = xmlDoc.SelectNodes(xPath);
            if (list == null) return null;
            if (list.Count == 0) return null;
            return list[0].InnerText;
        }

        public static XmlNodeList getNodeList(string xPath)
        {
            // Создаем экземпляр класса
            XmlDocument xmlDoc = new XmlDocument();
            // Загружаем XML-документ из файла
            var physicalFilePath = Path.Combine(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, XML_FILE);
            xmlDoc.Load(physicalFilePath);
            XmlNodeList list = xmlDoc.SelectNodes(xPath);
            return list;
        }

        public static Dictionary<string,string> getAllOrganizationArmId()
        {
            // Создаем экземпляр класса
            XmlDocument xmlDoc = new XmlDocument();
            // Загружаем XML-документ из файла
            var physicalFilePath = Path.Combine(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, XML_FILE);
            xmlDoc.Load(physicalFilePath);
            XmlNodeList list = xmlDoc.SelectNodes("/Configuration/Organization");
            Dictionary<string, string> orgList = new Dictionary<string, string>();
            foreach (XmlNode item in list)
            {
                orgList.Add(item.Attributes["armId"].Value, item.Attributes["name"].Value);
            }
            return orgList;
        }

        public static int WriteVariable(string xPath, string value)
        {
            throw new NotImplementedException();
        }

        public static string getInterfaceNameToLoadOrganization(string idOrganozation)
        {
            // Создаем экземпляр класса
            XmlDocument xmlDoc = new XmlDocument();
            // Загружаем XML-документ из файла
            var physicalFilePath = Path.Combine(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, XML_FILE);
            xmlDoc.Load(physicalFilePath);
            XmlNodeList list = xmlDoc.SelectNodes("/Configuration/Organization[@id=" + idOrganozation + "]/InterfaceLoadOrganization");
            if (list == null) return null;
            if (list.Count == 0) return null;
            return list[0].InnerText;
        }


        public static string getInterfaceNameToLoadNomenclature(string idOrganozation)
        {
            // Создаем экземпляр класса
            XmlDocument xmlDoc = new XmlDocument();
            // Загружаем XML-документ из файла
            var physicalFilePath = Path.Combine(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, XML_FILE);
            xmlDoc.Load(physicalFilePath);
            XmlNodeList list = xmlDoc.SelectNodes("/Configuration/Organization[@id=" + idOrganozation + "]/InterfaceLoadNomenclature");
            if (list == null) return null;
            if (list.Count == 0) return null;
            return list[0].InnerText;
        }

    }
}
