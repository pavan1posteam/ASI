using ASI_POS.Model;
using System;
using System.Collections.Generic;
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
                string ftpServerIP = settings.FtpServer;
                string ftpUserID = settings.FtpUserName;
                string ftpPassword = settings.FtpPassword;
                string ftpFolder = settings.FtpDownFolder?.Trim() ?? "";

                string listUri = string.IsNullOrEmpty(ftpFolder)? $"ftp://{ftpServerIP}/": $"ftp://{ftpServerIP}/{ftpFolder}/";

                SafeShowStatus("Connecting to FTP");

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
                    SafeShowStatus("0 Files found");
                    return result;
                }
                var xmlFiles = allFiles
                    .Where(n => n != null && n.Length > 4 && n.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                xmlcount = xmlFiles.Count;
                if (xmlFiles.Count == 0)
                {
                    SafeShowStatus("0 Files found");
                    return result;
                }
                string downloadFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Upload/Download");
                Directory.CreateDirectory(downloadFolder);

                SafeShowStatus($"{xmlFiles.Count} Files Found");

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
                            SafeShowStatus($"Downloading: {fileName}");

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
                            SafeShowStatus($"Downloaded: {fileName}");
                            success = true;
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
                                SafeShowStatus($"Warning: could not delete remote file {fileName}: {wexDel.Message}");
                            }
                        }
                        catch (WebException wex)
                        {
                            lastEx = wex;
                            SafeShowStatus($"i {i} failed for {fileName}: {wex.Message}");
                            try { if (File.Exists(localTemp)) File.Delete(localTemp); } catch { }
                            System.Threading.Thread.Sleep(500 * i);
                        }
                        catch (Exception ex)
                        {
                            lastEx = ex;
                            SafeShowStatus($"i {i} error for {fileName}: {ex.Message}");
                            try { if (File.Exists(localTemp)) File.Delete(localTemp); } catch { }
                            System.Threading.Thread.Sleep(500 * i);
                        }
                    } 

                    if (!success)
                    {
                        result.FailedFiles.Add(fileName);
                        SafeShowStatus($"Failed to download after {3} is: {fileName}. Last error: {lastEx?.Message}");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                SafeShowStatus("FTP operation failed: " + ex.Message);
                result.FatalError = ex;
                return result;
            }
            finally
            {
                if(xmlcount > 0)
                {
                    ProcessDownloadedXmlFiles();
                }
                SafeShowStatus("Disconnected from FTP");
            }
        }
        public XmlProcessResult ProcessDownloadedXmlFiles()
        {
            var result = new XmlProcessResult();
            string downloadFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Upload/Download");

            var xmlFiles = Directory.GetFiles(downloadFolder, "*.xml", SearchOption.TopDirectoryOnly).OrderBy(f => f).ToList();

            foreach (var fullPath in xmlFiles)
            {
                string fileName = Path.GetFileName(fullPath);
                string xmlContent = File.ReadAllText(fullPath);
                SafeShowStatus("Processing file: " + fileName);
                try
                {
                    bool flag = false;
                    if (Regex.IsMatch(fileName, @"^CUS", RegexOptions.IgnoreCase))
                    {
                        Customers customers = new Customers();
                        flag = customers.customerupdate(fileName, xmlContent);
                    }
                    else if (Regex.IsMatch(fileName, @"^ORD", RegexOptions.IgnoreCase))
                    {

                    }
                    else if (Regex.IsMatch(fileName, @"^PMT", RegexOptions.IgnoreCase))
                    {

                    }
                    if (flag)
                    {
                        SafeShowStatus($"Processed file: {fileName}");
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

                }
                catch (Exception ex)
                {
                    SafeShowStatus($"Processing failed for {fileName}: {ex.Message}");
                    result.FailedFiles.Add(fileName);
                    result.Failures[fileName] = ex;
                }
            }
            return result;
        }
        private void SafeShowStatus(string msg)
        {
            if (Form1.Instance != null)
            {
                try { Form1.Instance.ShowStatus(msg); } catch { }
            }
            else
            {
                try { File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download.log"), DateTime.Now + " " + msg + Environment.NewLine); } catch { }
            }
        }
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
