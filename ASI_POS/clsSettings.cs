using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ASI_POS.Model;

namespace ASI_POS
{
    class clsSettings
    {
        public clsSettings()
        {
            LoadSettings();
        }
        public string ConnectionString = "";
        public string StoreId { get; set; }
        public string FtpServer { get; set; }
        public string FtpUserName { get; set; }
        public string FtpPassword { get; set; }
        public string FtpUpFolder { get; set; }
        public int StockedItems { get; set; }
        public bool InclNoUpcProducts { get; set; }
        public int QtyperPack { get; set; }
        public string Tax { get; set; }
        public string serverpath { set; get; }
        public decimal MarkUpPrice { get; set; }
        public string Asi_Store_Id { get; set; }
        public string InvetValue { get; set; }
        public string PrcLevels { get; set; }
        public void LoadSettings()
        {
            string json;
            if (File.Exists("config//dbsettings.txt") && File.Exists("config//ftpsettings.txt") && File.Exists("config//others.txt"))
            {
                var fileStream = new FileStream(@"config\dbsettings.txt", FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    json = streamReader.ReadToEnd();
                }
                clsDbSettings clsdb = JsonConvert.DeserializeObject<clsDbSettings>(json);

                if (clsdb != null)
                    serverpath = clsdb.selectpath;

                ConnectionString = String.Format("Provider=VFPOLEDB;Data Source={0};Collating Sequence=machine;Mode=Share Deny None;", serverpath);

                fileStream = new FileStream(@"config\ftpsettings.txt", FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    json = streamReader.ReadToEnd();
                }
                clsFtpSettings clsFTP = JsonConvert.DeserializeObject<clsFtpSettings>(json);
                if (clsFTP != null)
                {
                    StoreId = clsFTP.StoreId;
                    FtpServer = clsFTP.Server;
                    FtpUserName = clsFTP.FtpUserName;
                    FtpPassword = clsFTP.FtpPassword;
                    FtpUpFolder = clsFTP.UpFolder;
                    Tax = clsFTP.Tax;
                    Asi_Store_Id = clsFTP.Asi_StoreId;
                }
                // For Others Settings
                fileStream = new FileStream(@"config\others.txt", FileMode.Open, FileAccess.Read);
                using (var StreamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    json = StreamReader.ReadToEnd();
                }
                clsOthers others = JsonConvert.DeserializeObject<clsOthers>(json);
                if (others != null)
                {
                    MarkUpPrice = others.MarkUpPrice;
                    StockedItems = others.StockedItems;
                    InvetValue = others.Inet_Value;
                    QtyperPack = others.QtyPack;
                    InclNoUpcProducts = others.NoUpcProducts;
                    PrcLevels = others.PLevels;
                }
            }
        }
    }
}
