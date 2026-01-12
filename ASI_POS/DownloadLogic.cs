using ASI_POS.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace ASI_POS
{
    public class DownloadLogic
    {
        clsSettings settings = new clsSettings();
        public FtpDownloadResult DownloadAllXmlFilesFromFtp()
        {
            var result = new FtpDownloadResult();
            int xmlcount = 0;
            try
            {
                settings.LoadSettings();
                #region Test Region
                //string tablename = "stk";
                //string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{tablename}_columns.txt");
                //ExportdatatypeToFile(outputPath, tablename);
                //SafeShowStatus($"Exported {tablename} columns datatype to: " + outputPath);
                //ksUpdate update = new ksUpdate();
                //int orderid = 301995127;
                //DateTime orddate = DateTime.Now.AddDays(-18);
                //inv itn = new inv
                //{
                //    SKU = "15791",
                //    NAME = "Name",
                //    cat = "Category",
                //    Sdate = DateTime.Now.AddYears(-1)
                //};
                //jnl jnl = new jnl
                //{
                //    rflag = 0,
                //    qty = 1
                //};
                //update.UpdateInv(orderid, itn, jnl, orddate);
                #endregion

                string ftpServerIP = settings.FtpServer;
                string ftpUserID = settings.FtpUserName;
                string ftpPassword = settings.FtpPassword;
                string ftpFolder = settings.FtpDownFolder?.Trim() ?? "";

                string listUri = string.IsNullOrEmpty(ftpFolder)? $"ftp://{ftpServerIP}/": $"ftp://{ftpServerIP}/{ftpFolder}/";

                SafeShowStatus("Connecting to FTP", 3);

                System.Net.ServicePointManager.DefaultConnectionLimit = Math.Max(System.Net.ServicePointManager.DefaultConnectionLimit, 100);

                var reqList = (FtpWebRequest)WebRequest.Create(new Uri(listUri));
                reqList.Method = WebRequestMethods.Ftp.ListDirectory;
                reqList.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqList.UsePassive = true;
                reqList.UseBinary = true;
                reqList.KeepAlive = false;
                reqList.Timeout = 120000;

                List<string> allFiles = new List<string>();
                using (var resp = (FtpWebResponse)reqList.GetResponse())
                using (var rs = resp.GetResponseStream())
                using (var sr = new StreamReader(rs))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                            allFiles.Add(line.Trim());
                    }
                }

                if (allFiles.Count == 0)
                {
                    SafeShowStatus("0 Files found", 2);
                    return result;
                }
                var xmlFiles = allFiles
                    .Where(n => n != null && n.Length > 4 && n.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                xmlcount = xmlFiles.Count;
                if (xmlFiles.Count == 0)
                {
                    SafeShowStatus("0 Files Found!", 2);
                    return result;
                }
                string downloadFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Upload/OrderPending");
                Directory.CreateDirectory(downloadFolder);

                SafeShowStatus($"{xmlFiles.Count} Files Found!", 1);

                foreach (var fileName in xmlFiles)
                {
                    fileName.Trim();
                    string fileUri = string.IsNullOrEmpty(ftpFolder)? $"ftp://{ftpServerIP}/{fileName}": $"ftp://{ftpServerIP}/{ftpFolder}/{fileName}";
                    string localTemp = Path.Combine(downloadFolder, fileName + ".tmp");
                    string localFinal = Path.Combine(downloadFolder, fileName);

                    bool success = false;
                    Exception lastEx = null;

                    for (int i = 1; i <= 3 && !success; i++)
                    {
                        try
                        {
                            SafeShowStatus($"File: {fileName}");

                            var req = (FtpWebRequest)WebRequest.Create(new Uri(fileUri));
                            req.Method = WebRequestMethods.Ftp.DownloadFile;
                            req.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                            req.UseBinary = true;
                            req.UsePassive = true;
                            req.KeepAlive = false;
                            req.Timeout = 120000;

                            req.ReadWriteTimeout = 300000;

                            using (var resp = (FtpWebResponse)req.GetResponse())
                            using (var rs = resp.GetResponseStream())
                            using (var fs = new FileStream(localTemp, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                byte[] buffer = new byte[8192];
                                int read;
                                while ((read = rs.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    fs.Write(buffer, 0, read);
                                }
                                fs.Flush(true);
                            }
                            var fi = new FileInfo(localTemp);
                            if (!fi.Exists || fi.Length == 0)
                                throw new IOException("Downloaded file is empty or missing after transfer.");

                            if (File.Exists(localFinal)) File.Delete(localFinal);
                            File.Move(localTemp, localFinal);
                            result.DownloadedFiles.Add(localFinal);
                            SafeShowStatus($"File: {fileName} Downloaded");
                            success = true;
                            
                        }
                        catch (WebException wex)
                        {
                            lastEx = wex;
                            SafeShowStatus($"i {i} failed for {fileName}: {wex.Message}", 2);
                            try { if (File.Exists(localTemp)) File.Delete(localTemp); } catch { }
                            System.Threading.Thread.Sleep(500 * i);
                        }
                        catch (Exception ex)
                        {
                            lastEx = ex;
                            SafeShowStatus($"i {i} error for {fileName}: {ex.Message}", 2);
                            try { if (File.Exists(localTemp)) File.Delete(localTemp); } catch { }
                            System.Threading.Thread.Sleep(500 * i);
                        }
                    }
                    try
                    {
                        var reqDel = (FtpWebRequest)WebRequest.Create(new Uri(fileUri));
                        reqDel.Method = WebRequestMethods.Ftp.DeleteFile;
                        reqDel.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                        reqDel.UseBinary = true;
                        reqDel.UsePassive = true;
                        reqDel.KeepAlive = false;
                        reqDel.Timeout = 120000;
                        SafeShowStatus("Deleting file from FTP: " + fileName);
                        using (var delResp = (FtpWebResponse)reqDel.GetResponse())
                        {

                        }
                    }
                    catch (WebException wexDel)
                    {
                        SafeShowStatus($"Warning: Could Not Delete FTP File {fileName}: {wexDel.Message}");
                    }
                    if (!success)
                    {
                        result.FailedFiles.Add(fileName);
                        SafeShowStatus($"Failed to download after {3} is: {fileName}. Last error: {lastEx?.Message}", 2);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                SafeShowStatus("FTP operation failed: " + ex.Message, 2);
                result.FatalError = ex;
                return result;
            }
            finally
            {
                //if(xmlcount > 0)
                {
                    ProcessDownloadedXmlFiles();
                    SafeShowStatus("All Done", 1);
                }
                SafeShowStatus("Disconnected from FTP", 3);
            }
        }
        public XmlProcessResult ProcessDownloadedXmlFiles()
        {
            var result = new XmlProcessResult();
            string downloadFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Upload/OrderPending");
            var files = Directory.GetFiles(downloadFolder, "*.xml", SearchOption.TopDirectoryOnly)
                     .Select(f => new
                     {
                         FullPath = f,
                         Name = Path.GetFileName(f),
                         Prefix = Regex.Match(Path.GetFileName(f), @"^[A-Z]+", RegexOptions.IgnoreCase).Value.ToUpper()
                     }).ToList();
            var cusFiles = files.Where(f => f.Prefix == "CUS").ToList();
            bool flag = false;
            foreach (var f in cusFiles)
            {
                try
                {
                    SafeShowStatus("Processing file: " + f.Name);
                    var xmlContent = File.ReadAllText(f.FullPath);
                    Customers customers = new Customers();
                    flag = customers.customerupdate(f.Name, xmlContent);
                    
                    if (flag)
                    {
                        MoveToArchive(f.Name);
                        flag = false;
                    }
                }
                catch (Exception ex)
                {
                    flag = false;
                    SafeShowStatus($"Processing failed {f.Name}: {ex.Message}", 2);
                }
            }
            SafeShowStatus("Customer Update Completed", 1);
            var ordFiles = files.Where(f => f.Prefix == "ORD").ToList();
            var pmtFiles = files.Where(f => f.Prefix == "PMT").ToList();
            var ordLookup = ordFiles.ToDictionary(f => Regex.Replace(f.Name, @"^(ORD)_?", "", RegexOptions.IgnoreCase), f => f);
            var pmtLookup = pmtFiles.ToDictionary(f => Regex.Replace(f.Name, @"^(PMT)_?", "", RegexOptions.IgnoreCase), f => f);
            foreach (var key in ordLookup.Keys)
            {
                if (!pmtLookup.TryGetValue(key, out var pmt))
                {
                    SafeShowStatus($"PMT File Missing for ORD file: {ordLookup[key].Name}", 2);
                    continue;
                }
                string ordXml = File.ReadAllText(ordLookup[key].FullPath);
                string pmtXml = File.ReadAllText(pmt.FullPath);
                Match regexMatch = Regex.Match(pmtXml, @"<orderid>(?<Result>\d+)</orderid>");
                string orderid = regexMatch.Groups["Result"]?.Value;
                SafeShowStatus($"Processing Order Id: #{orderid}", 3);

                updateOrders orders = new updateOrders();
                flag = orders.updateorder(ordLookup[key].Name, ordXml, pmt.Name, pmtXml);
                if (flag)
                {
                    MoveToArchive(ordLookup[key].Name);
                    MoveToArchive(pmt.Name);
                    flag = false;
                }

            }
            return result;
        }
        public void MoveToArchive(string fileName)
        {
            string downloadFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Upload/OrderPending");
            string transferTo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Archive");
            if (!Directory.Exists(transferTo))
                Directory.CreateDirectory(transferTo);
            string destinationPath = Path.Combine(transferTo, Path.GetFileName(fileName));
            if (File.Exists(downloadFolder + "/" + fileName))
            {
                if (File.Exists(destinationPath))
                    File.Delete(destinationPath);
                File.Move(downloadFolder + "/" + fileName, destinationPath);
            }

        }
        private void SafeShowStatus(string msg, int c =0)
        {
            if (Form1.Instance != null)
            {
                try { Form1.Instance.ShowStatus(msg, c); } catch { }
            }
            else
            {
                try { File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ErrorLogs.log"), DateTime.Now + " " + msg + Environment.NewLine); } catch { }
            }
        }
        public void Install()
        {
            SafeShowStatus("Installation Mode!!!");
            settings.LoadSettings();
            string columnname = "webcustid";
            bool flag = false;
            using (var conn = new OleDbConnection(settings.ConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT {columnname} FROM cus WHERE 1=0";
                try
                {
                    cmd.ExecuteReader();
                    flag = true;
                }
                catch
                {
                    flag = false;
                }
            }
            if (!flag)
            {
                using (var conn = new OleDbConnection(settings.ConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = $"ALTER TABLE cus ADD COLUMN {columnname} I(4)";
                    cmd.ExecuteNonQuery();
                    SafeShowStatus($"Added Column Name {columnname} In CUS Table.", 1);
                }
            }
            else
            {
                SafeShowStatus($"Column: {columnname} Is Already Exists.", 3);
            }
            try
            {
                bool catflag = false;
                using (var conn = new OleDbConnection(settings.ConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();

                    cmd.CommandText = @"SELECT COUNT(*) FROM cat c WHERE ALLTRIM(c.cat) == ?";
                    cmd.Parameters.Add(new OleDbParameter
                    {
                        OleDbType = OleDbType.VarChar,
                        Value = settings.ServiceFee.Trim()
                    });

                    int exists = Convert.ToInt32(cmd.ExecuteScalar());

                    if (exists > 0)
                        SafeShowStatus("BC SERVICE FEE Already Exists In CAT Table.", 3);
                    else
                        catflag = true;
                }
                if (catflag)
                {
                    using (var conn = new OleDbConnection(settings.ConnectionString))
                    {
                        conn.Open();
                        bool surflag = false;
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = $"UPDATE cat SET sur = ?";
                            cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = true });
                            int rows = cmd.ExecuteNonQuery();
                            surflag = true;
                        }
                        if (surflag)
                        {
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = $"INSERT INTO cat(Cat, Name, Seq, Lspace, Code,cost, Taxlevel, Sur, Disc, Dbcr, Cflag, Income, Cog, Inventory, Discount, Who, Tstamp, Wsgroup, Wscod, Wsgallons, Catnum, Fson, Fsfactor, Datacap, Sent, Belowcost, Gosent, Cost_plus)" +
                                  $"VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = settings.ServiceFee });
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "BC SERVICE FEE" });//Name
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "0" + settings.ServiceFee });//seq
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "0" });//Lspace
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "X" });//Code
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = 0 });//cost
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = 0 });//TaxLevel
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false });//Sur
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false });//Disc
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = 1 });//Dbcr
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false });//Cflag
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "06" + settings.ServiceFee });//Income
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//cog
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//inventory
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//discount
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//who
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = DateTime.Now });//tstamp
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//Wsgroup
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//Wscod
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false });//Wsgallons
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = 0 });//Catnum
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false });//Fson
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = 0 });//Fsfactor
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//Datacap
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false });//Sent
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false });//Belowcost
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = true });//Gosent
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = 0 });//Cost_plus
                                cmd.ExecuteNonQuery();
                                SafeShowStatus($"Added BC SERVICE FEE in CAT Table.", 3);
                            }
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = $"INSERT INTO gla(Sequence, Type, Glaccount, Short, Descript,Balance, Lastcheck, Who, Tstamp, Qbooks, Qbooksac, Skipstat, Sent)" +
                                  $"VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?)";
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "06" + settings.ServiceFee });//Sequence
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "L" });//type
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "06" + settings.ServiceFee });//Glaccount
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "BC SER CHG" });//short
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "BC SERVICE CHARGE" });//Descript
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = 0 });//Balance
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//Lastcheck
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//Who
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = DateTime.Now });//tstamp
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false });//Qbooks
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//Qbooksac
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false });//Skipstat
                                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false });//Sent
                                cmd.ExecuteNonQuery();
                                SafeShowStatus($"Added BC SERVICE FEE in GLA Table.", 3);
                            }
                        }
                    }
                }
                SafeShowStatus("Installation Done!!!", 1);
                SafeShowStatus("Change The Required Fields In Settings Tab Before Running!", 3);
            }
            catch (Exception ex)
            {
                SafeShowStatus($"Error: {ex.Message}", 2);
                SafeShowStatus("Installation Failed!", 2);
            }
        }
        #region test region
        public void ExportdatatypeToFile(string outputFilePath, string tablename = "inv")
        {
            settings.LoadSettings();

            if (string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new InvalidOperationException("ConnectionString not configured.");

            using (var conn = new OleDbConnection(settings.ConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();

                cmd.CommandText = $"SELECT * FROM {tablename} WHERE 1=0";

                using (var reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly))
                {
                    var schema = reader.GetSchemaTable();
                    if (schema == null)
                        throw new InvalidOperationException($"Failed to read schema for {tablename} table.");

                    using (var sw = new StreamWriter(outputFilePath, false))
                    {
                        sw.WriteLine("================================");
                        sw.WriteLine("ColumnName | DataType | ColumnSize");
                        sw.WriteLine("--------------------------------");

                        foreach (DataRow row in schema.Rows)
                        {
                            string colName = row["ColumnName"].ToString();
                            string dataType = row["DataType"]?.ToString() ?? "Unknown";
                            string colSize = row.Table.Columns.Contains("ColumnSize")
                                ? row["ColumnSize"]?.ToString()
                                : "N/A";

                            sw.WriteLine($"{colName} | {dataType} | {colSize}");
                        }
                    }
                }
            }
        }
        #endregion
    }
    public class FtpDownloadResult
    {
        public List<string> DownloadedFiles { get; } = new List<string>();
        public List<string> FailedFiles { get; } = new List<string>();
        public Exception FatalError { get; set; } = null;
    }
    public class XmlProcessResult
    {
        public List<string> ProcessedFiles { get; } = new List<string>();
        public List<string> FailedFiles { get; } = new List<string>();
        public Dictionary<string, Exception> Failures { get; } = new Dictionary<string, Exception>();
    }
}
