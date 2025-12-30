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
        public string FtpDownFolder { get; set; }
        public int StockedItems { get; set; }
        public string webstore { get; set; }
        public string ServiceFee { get; set; }
        public bool InclNoUpcProducts { get; set; }
        public bool AddDiscountable { get; set; }
        public bool IncludeFloor { get; set; }
        public bool AllQtyperPack { get; set; }
        public int QtyperPack { get; set; }
        public string TaxCode { get; set; }
        public string serverpath { set; get; }
        public decimal MarkUpPrice { get; set; }
        public string Asi_Store_Id { get; set; }
        public string InvetValue { get; set; }
        public string PrcLevels { get; set; }
        public string Stat { get; set; }
        public bool updateCustomerFiles { get; set; }
        public bool updateClubcardNo { get; set; }
        public string Mobile_Register { get; set; }
        public string Mobile_Cashier { get; set; }
        public string ShippingCat { get; set; }
        public string TipCat { get; set; }
        public string DiscountCat { get; set; }
        public string visa { get; set; }
        public string amex { get; set; }
        public string mastercard { get; set; }
        public string discover { get; set; }
        public string generic { get; set; }
        public bool FrequentFile { get; set; }
        public int UploadTime { get; set; }
        public int DownloadTime { get; set; }
        public bool UploadFilesToFTP { get; set; }
        public bool DownloadFilesToFTP { get; set; }
        public void LoadSettings()
        {
            if (File.Exists(@"data.enc"))
            {
                byte[] encrypted = File.ReadAllBytes("data.enc");
                string json = Form2.Decrypt(encrypted);
                var apps = JsonConvert.DeserializeObject<List<AppSettings>>(json);
                AppSettings app = apps[0];
                clsDbSettings clsdb = app.Db;
                clsFtpSettings clsFTP = app.Ftp;
                clsOthers others = app.Other;
                serverpath = clsdb.selectpath;
                ConnectionString = String.Format("Provider=VFPOLEDB;Data Source={0};Collating Sequence=machine;Mode=Share Deny None;", serverpath);
                FtpUpFolder = clsdb.UpFolder;
                FtpDownFolder = clsdb.DownFolder;
                TaxCode = clsdb.TaxCode;
                ServiceFee = clsdb.service_fee;
                ShippingCat = clsdb.shipCat;
                TipCat = clsdb.tipCat;
                DiscountCat = clsdb.discountCat;
                visa = clsdb.visa;
                amex = clsdb.amex;
                mastercard = clsdb.mastercard;
                discover = clsdb.discover;
                generic = clsdb.generic;
                StoreId = clsFTP.StoreId;
                FtpServer = clsFTP.Server;
                FtpUserName = clsFTP.FtpUserName;
                FtpPassword = clsFTP.FtpPassword;
                Asi_Store_Id = clsFTP.Asi_StoreId;
                webstore = clsFTP.mobilestore;
                MarkUpPrice = others.MarkUpPrice;
                StockedItems = others.StockedItems;
                InvetValue = others.Inet_Value;
                QtyperPack = others.QtyPack;
                InclNoUpcProducts = others.NoUpcProducts;
                PrcLevels = others.PLevels;
                AddDiscountable = others.chkDiscountable;
                IncludeFloor = others.chkfloor;
                AllQtyperPack = others.AllQtyPack;
                Stat = others.Statvalue;
                updateCustomerFiles = others.updatecustomerfiles;
                updateClubcardNo = others.updatecclubcardnos;
                Mobile_Register = others.mobileregister;
                Mobile_Cashier = others.mobilecashier;
                FrequentFile = others.enablefrequentFile;
                UploadTime = others.uploadminute;
                DownloadTime = others.downloadminute;
                UploadFilesToFTP = others.uploadfilestoftp;
                DownloadFilesToFTP = others.downloadfilestoftp;
            }

        }
    }
}
