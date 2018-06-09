using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace Store.Core.Utils
{
    public class Reflection
    {
        private static Object CreateBinding(Assembly a, string objectName)
        {
            Object obj = null;
            try
            {
                Type objectType = a.GetType(objectName, false, true);
                obj = Activator.CreateInstance(objectType);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return obj;
        }

        public static Object LoadClassObject(string assemblyName, string objectName) 
        {
            if (assemblyName == null) return null;
            Object obj = null;
            Assembly ass = null;
            try
            {
                ass = Assembly.Load(assemblyName);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
            if (ass != null)
                obj = CreateBinding(ass, objectName);
            return obj;
        }

    }
}
