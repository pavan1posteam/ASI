using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI_POS.Model
{
    class GenerateCSV
    {
        clsSettings setting = new clsSettings();
        public string GenerateCSVFile(DataTable dt, bool flag = false)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                int count = 1;
                int totalColumns = dt.Columns.Count;
                foreach (DataColumn dr in dt.Columns)
                {
                    sb.Append(dr.ColumnName);

                    if (count != totalColumns)
                    {
                        sb.Append(",");
                    }

                    count++;
                }

                sb.AppendLine();

                #region
                string value = String.Empty;
                foreach (DataRow dr in dt.Rows)
                {
                    for (int x = 0; x < totalColumns; x++)
                    {
                        value = dr[x].ToString();

                        if (value.Contains(",") || value.Contains("\""))
                        {
                            value = '"' + value.Replace("\"", "\"\"") + '"';
                        }

                        sb.Append(value);

                        if (x != (totalColumns - 1))
                        {
                            sb.Append(",");
                        }
                    }

                    sb.AppendLine();
                }
                #endregion
                if (flag)
                {
                    string filename = "FrequentSP" + setting.StoreId + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv";

                    File.WriteAllText("Upload\\" + filename, sb.ToString());

                    return filename;
                }
                else if (dt.Columns.Count > 13)
                {
                    string filename = "Product" + setting.StoreId + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv";

                    File.WriteAllText("Upload\\" + filename, sb.ToString());

                    return filename;
                }
                else
                {
                    string filename = "Fullname" + setting.StoreId + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv";
                    File.WriteAllText("Upload\\" + filename, sb.ToString());
                    return filename;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return "";
        }
    }
}
