using System;
using System.Data.OleDb;
using System.Data;
using System.Collections.Generic;

namespace Store.Core.Utils
{
    public class ExcelReader
    {
        public string[] workSheetNames = new string[] { };   //List of Worksheet names 
        private string connectionString;

        public ExcelReader(string fileName):this(fileName, false)
        {
        }

        public ExcelReader(string fileName, bool headerExsist)
        {
            string header = "NO";
            if (headerExsist) header = "YES";
            string[] splitByDots = fileName.Split(new char[1] { '.' });
            this.connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + (char)34 + fileName + (char)34 + ";Extended Properties=" + (char)34 + "Excel 8.0;IMEX=1;HDR=" + header + ";" + (char)34;
            //Excel 97-2003 file
            //if (splitByDots[splitByDots.Length - 1] == "xlsx")
            //    this.connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + (char)34 + fileName + (char)34 + ";Extended Properties=" + (char)34 + "Excel 8.0;HDR=NO;" + (char)34;
            //else
            //    //read a 97-2003 file
            //    connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + (char)34 + fileName + (char)34 + ";Extended Properties=" + (char)34 + "Excel 8.0;" + (char)34;
            OleDbConnection con;
            con = new OleDbConnection(connectionString);
            con.Open();

            //get all the available sheets
            System.Data.DataTable dataSet = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

            //get the number of sheets in the file
            IList<string> list = new List<string>();            
            int i = 0;
            foreach (DataRow row in dataSet.Rows)
            {
                //insert the sheet's name in the current element of the array
                //and remove the $ sign at the end
                //workSheetNames[i] = row["TABLE_NAME"].ToString().Trim(new[] { '$' });
                string rowName = row["TABLE_NAME"].ToString();
                rowName = rowName.Trim(new char[] {'\''});
                if (rowName.EndsWith("$"))
                {
                    list.Add(row["TABLE_NAME"].ToString());
                }
                i++;
            }
            workSheetNames = new String[list.Count];
            list.CopyTo(workSheetNames,0);
            if (con != null)
            {
                con.Close();
                con.Dispose();
            }

            if (dataSet != null)
                dataSet.Dispose();
        }
        /// <summary>
        /// Returns the contents of the sheet
        /// </summary>
        /// <param name="worksheetName">The sheet's name in a string</param>
        /// <returns>A DataTable containing the data</returns>
        public System.Data.DataTable GetWorksheet(string worksheetName)
        {
            OleDbConnection con = new System.Data.OleDb.OleDbConnection(connectionString);
            OleDbDataAdapter cmd = new System.Data.OleDb.OleDbDataAdapter(
                "select * from [" + worksheetName + "]", con);

            con.Open();
            System.Data.DataSet excelDataSet = new DataSet();
            cmd.Fill(excelDataSet);
            con.Close();
            if (excelDataSet.Tables.Count > 0)
                return excelDataSet.Tables[0];
            else return null;
        }

    }
}
