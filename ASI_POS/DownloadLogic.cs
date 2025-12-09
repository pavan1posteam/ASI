using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
                    //fileprocessing();
                }
                SafeShowStatus("Disconnected from FTP");
            }
        }

        public XmlProcessResult ProcessDownloadedXmlFiles(
            string downloadFolder,
            IEnumerable<FileTypeHandlerMapping> mappings,
            bool moveOnSuccess = true,
            bool moveOnFailure = true)
        {
            var result = new XmlProcessResult();

            if (string.IsNullOrWhiteSpace(downloadFolder) || !Directory.Exists(downloadFolder))
            {
                SafeShowStatus($"Download folder missing: {downloadFolder}");
                return result;
            }

            // ensure processed/failed directories exist
            string processedDir = Path.Combine(downloadFolder, "Processed");
            string failedDir = Path.Combine(downloadFolder, "Failed");
            Directory.CreateDirectory(processedDir);
            Directory.CreateDirectory(failedDir);

            var xmlFiles = Directory.GetFiles(downloadFolder, "*.xml", SearchOption.TopDirectoryOnly)
                                    .OrderBy(f => f) 
                                    .ToList();

            foreach (var fullPath in xmlFiles)
            {
                string fileName = Path.GetFileName(fullPath);
                SafeShowStatus("Processing file: " + fileName);

                try
                {
                    // find mapping by filename pattern
                    var mapping = mappings.FirstOrDefault(m => m.IsMatch(fileName));
                    if (mapping == null)
                    {
                        SafeShowStatus($"No handler mapping found for {fileName} - moving to Failed");
                        result.FailedFiles.Add(fileName);
                        result.Failures[fileName] = new InvalidOperationException("No mapping");
                        if (moveOnFailure) MoveToFolderAtomic(fullPath, failedDir);
                        continue;
                    }

                    // Deserialize XML into the target type. Supports either T (single object) or List<T>.
                    object deserialized = DeserializeXml(fullPath, mapping.TargetType);

                    if (deserialized == null)
                    {
                        throw new InvalidOperationException("Deserialization returned null");
                    }

                    // call the handler delegate
                    mapping.Handler(deserialized);

                    // mark success
                    result.ProcessedFiles.Add(fileName);
                    SafeShowStatus($"Processed: {fileName}");

                    if (moveOnSuccess) MoveToFolderAtomic(fullPath, processedDir);
                }
                catch (Exception ex)
                {
                    SafeShowStatus($"Processing failed for {fileName}: {ex.Message}");
                    result.FailedFiles.Add(fileName);
                    result.Failures[fileName] = ex;

                    // move to failed folder so we don't retry broken files
                    try { if (moveOnFailure) MoveToFolderAtomic(fullPath, failedDir); } catch { /* swallow */ }
                }
            }

            return result;
        }

        /// <summary>
        /// Mapping entry: filename pattern -> target CLR type -> handler delegate
        /// </summary>
        public class FileTypeHandlerMapping
        {
            /// <summary>
            /// If this regex or substring matches the filename, this mapping is used.
            /// </summary>
            public Func<string, bool> IsMatch { get; set; }

            /// <summary>
            /// System.Type to which XML will be deserialized (e.g. typeof(List&lt;MyOrder&gt;) or typeof(MyOrdersWrapper))
            /// </summary>
            public Type TargetType { get; set; }

            /// <summary>
            /// Action to call with deserialized object. The object will be of TargetType.
            /// </summary>
            public Action<object> Handler { get; set; }

            public FileTypeHandlerMapping() { }
        }

        /// <summary>
        /// Deserialize XML file into given Type using XmlSerializer.
        /// If xml contains a wrapper element with a list, pass typeof(List&lt;T&gt;).
        /// </summary>
        private object DeserializeXml(string filePath, Type targetType)
        {
            // read file into memory first (so file can be moved safely)
            string xml;
            using (var sr = new StreamReader(filePath))
            {
                xml = sr.ReadToEnd();
            }

            var serializer = new XmlSerializer(targetType);
            using (var reader = new StringReader(xml))
            {
                return serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Move file to destination folder by copying to temp file and then moving atomically.
        /// </summary>
        private void MoveToFolderAtomic(string sourceFileFullPath, string destinationFolder)
        {
            string fileName = Path.GetFileName(sourceFileFullPath);
            string destTemp = Path.Combine(destinationFolder, fileName + ".tmp");
            string destFinal = Path.Combine(destinationFolder, fileName);

            // ensure destination temp removed
            if (File.Exists(destTemp)) File.Delete(destTemp);
            // copy then move
            File.Copy(sourceFileFullPath, destTemp);
            if (File.Exists(destFinal)) File.Delete(destFinal);
            File.Move(destTemp, destFinal);

            // remove original
            File.Delete(sourceFileFullPath);
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
