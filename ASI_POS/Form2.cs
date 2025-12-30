using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using ASI_POS.Model;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace ASI_POS
{
    public partial class Form2 : Form
    {
        DataTable dtCat;
        clsSettings settings = new clsSettings();
        public event Action SettingsUpdated;
        List<clsCategories> categories = new List<clsCategories>();
        private static readonly byte[] Key =
        Encoding.UTF8.GetBytes("Bottlecapps-Secret-Key!!"); // 32 bytes

        private static readonly byte[] IV =
            Encoding.UTF8.GetBytes("16ByteInitVector"); // 16 bytes
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            AddHeaderCheckBox();
            HeaderCheckBox.MouseClick += new MouseEventHandler(HeaderCheckBox_MouseClick);
            if (File.Exists(@"data.enc"))
            {
                byte[] encrypted = File.ReadAllBytes("data.enc");
                string json2 = Decrypt(encrypted);
                var apps = JsonConvert.DeserializeObject<List<AppSettings>>(json2);
                AppSettings app = apps[0];
                settings.LoadSettings();
                textpath.Text = app.Db.selectpath;
                txtStoreID.Text = app.Ftp.StoreId;
                txtFTPpwd.Text = app.Ftp.FtpPassword;
                txtFTPserver.Text = app.Ftp.Server;
                txtFTPuid.Text = app.Ftp.FtpUserName;
                txtUPFolder.Text = app.Db.UpFolder;
                txtdownloadpath.Text = app.Db.DownFolder;
                txtasistoreid.Text = app.Ftp.Asi_StoreId;
                txtInetValue.Text = app.Other.Inet_Value;
                txtPrcLvl.Text = app.Other.PLevels;
                txttaxcode.Text = app.Db.TaxCode;
                txtstat.Text = app.Other.Statvalue;
                if (app.Other.StockedItems == 0)
                {
                    chkStoked.Checked = false;
                }
                else
                {
                    chkStoked.Checked = true;
                }
                if (app.Other.QtyPack == 0)
                {
                    chkqtypack.Checked = false;
                }
                else
                {
                    chkqtypack.Checked = true;
                }
                if (app.Other.NoUpcProducts)
                {
                    chkNoUpc.Checked = true;
                }
                else
                {
                    chkNoUpc.Checked = false;
                }
                chkdiscountable.Checked = app.Other.chkDiscountable;
                chkfloor.Checked = app.Other.chkfloor;
                chkallqtyperpack.Checked = app.Other.AllQtyPack;
                chkupcustomerfiles.Checked = app.Other.uploadfilestoftp;
                chkclubcardno.Checked = app.Other.updatecclubcardnos;
                txtwebstore.Text = app.Ftp.mobilestore;
                textMarkUp.Text = app.Other.MarkUpPrice.ToString();
                txtservicefee.Text = app.Db.service_fee;
                txtmobileregister.Text = app.Other.mobileregister;
                txtmobilecashier.Text = app.Other.mobilecashier;
                txtShippingCat.Text = app.Db.shipCat;
                txtTipCat.Text = app.Db.tipCat;
                txtDiscountCat.Text = app.Db.discountCat;
                txtVisa.Text = app.Db.visa;
                txtAmex.Text = app.Db.amex;
                txtMc.Text = app.Db.mastercard;
                txtDiscover.Text = app.Db.discover;
                txtGeneric.Text = app.Db.generic;
                chkfrequent.Checked = app.Other.enablefrequentFile;
                numericUpload.Value = app.Other.uploadminute;
                numericDownload.Value = app.Other.downloadminute;
                chkuploadfilesftp.Checked = app.Other.uploadfilestoftp;
                chkdownloadfilesftp.Checked = app.Other.downloadfilestoftp;
                loadCats(app.Categories);
            }

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textpath.Text = fbd.SelectedPath;
                }
            }
        }

        private void btnDbSave_Click(object sender, EventArgs e)
        {
            btnFTPSave_Click(sender, e);
        }

        private void btnFTPSave_Click(object sender, EventArgs e)
        {
            clsDbSettings clsdb = new clsDbSettings();
            clsdb.selectpath = textpath.Text;
            clsdb.UpFolder = txtUPFolder.Text;
            clsdb.DownFolder = txtdownloadpath.Text;
            clsdb.TaxCode = txttaxcode.Text;
            clsdb.service_fee = txtservicefee.Text;
            clsdb.shipCat = txtShippingCat.Text;
            clsdb.tipCat = txtTipCat.Text;
            clsdb.discountCat = txtDiscountCat.Text;
            clsdb.visa = txtVisa.Text;
            clsdb.amex = txtAmex.Text;
            clsdb.mastercard = txtMc.Text;
            clsdb.discover = txtDiscover.Text;
            clsdb.generic = txtGeneric.Text;


            clsFtpSettings clsftp = new clsFtpSettings();
            clsftp.StoreId = txtStoreID.Text;
            clsftp.Server = txtFTPserver.Text;
            clsftp.FtpUserName = txtFTPuid.Text;
            clsftp.FtpPassword = txtFTPpwd.Text;
            clsftp.Asi_StoreId = txtasistoreid.Text;
            clsftp.mobilestore = txtwebstore.Text;

            clsOthers others = new clsOthers();
            others.MarkUpPrice = Convert.ToDecimal(textMarkUp.Text);
            others.Inet_Value = txtInetValue.Text;
            others.NoUpcProducts = chkNoUpc.Checked;
            others.PLevels = txtPrcLvl.Text;
            others.StockedItems = (int)chkStoked.CheckState;
            others.QtyPack = (int)chkqtypack.CheckState;
            others.chkDiscountable = chkdiscountable.Checked;
            others.chkfloor = chkfloor.Checked;
            others.AllQtyPack = chkallqtyperpack.Checked;
            others.Statvalue = txtstat.Text;
            others.updatecustomerfiles = chkupcustomerfiles.Checked;
            others.updatecclubcardnos = chkclubcardno.Checked;
            others.mobileregister = txtmobileregister.Text;
            others.mobilecashier = txtmobilecashier.Text;
            others.enablefrequentFile = chkfrequent.Checked;
            others.uploadminute = (int)numericUpload.Value;
            others.downloadminute = (int)numericDownload.Value;
            others.uploadfilestoftp = chkuploadfilesftp.Checked;
            others.downloadfilestoftp = chkdownloadfilesftp.Checked;

            List<AppSettings> app = new List<AppSettings>();
            AppSettings appSettings = new AppSettings();
            appSettings.Db = clsdb;
            appSettings.Ftp = clsftp;
            appSettings.Other = others;

            appSettings.Categories = GetCategoriesFromGrid();
            app.Add(appSettings);
            string json2 = JsonConvert.SerializeObject(app);
            byte[] encrypted = Encrypt(json2);
            File.WriteAllBytes("data.enc", encrypted);
            settings.LoadSettings();
        }
        private static byte[] Encrypt(string plainText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                using (var encryptor = aes.CreateEncryptor())
                {
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            using (var sw = new StreamWriter(cs))
                            {
                                sw.Write(plainText);
                                sw.Close();
                                return ms.ToArray();
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt(byte[] cipher)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                using (var decryptor = aes.CreateDecryptor())
                {
                    using (var ms = new MemoryStream(cipher))
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (var sr = new StreamReader(cs))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }
        private void btnCatsave_Click(object sender, EventArgs e)
        {
            btnFTPSave_Click(sender, e);
        }
        private void loadCats(List<clsCategories> categories)
        {
            var savedCats = categories;

            dtCat = new DataTable();
            dataGridView1.AutoGenerateColumns = false;

            using (var con = new OleDbConnection(settings.ConnectionString))
            using (var cmd = new OleDbCommand("SELECT DISTINCT 0 as sel, CAT as ID, Name, Taxlevel FROM cat", con))
            using (var adp = new OleDbDataAdapter(cmd))
            {
                adp.Fill(dtCat);
            }

            // apply saved selections
            foreach (var saved in savedCats.Where(c => c.Sel == 1))
            {
                var row = dtCat.AsEnumerable()
                               .FirstOrDefault(r => r["Name"].ToString() == saved.Depart);

                if (row != null)
                    row["sel"] = 1;
            }

            dataGridView1.ColumnCount = 3;

            //Add Columns
            dataGridView1.Columns[0].Name = "sel";
            dataGridView1.Columns[0].HeaderText = "Select";
            dataGridView1.Columns[0].DataPropertyName = "sel";

            dataGridView1.Columns[1].Name = "CAT_ID";
            dataGridView1.Columns[1].HeaderText = "ID";
            dataGridView1.Columns[1].DataPropertyName = "ID";

            dataGridView1.Columns[2].Name = "Description";
            dataGridView1.Columns[2].HeaderText = "Name";
            dataGridView1.Columns[2].DataPropertyName = "Name";
            dataGridView1.Columns[2].Width = 400;
            dataGridView1.DataSource = dtCat.DefaultView;

            dataGridView1.DataSource = dtCat.DefaultView;
        }
        private List<clsCategories> GetCategoriesFromGrid()
        {
            var list = new List<clsCategories>();

            if (dtCat == null) return list;

            foreach (DataRow row in dtCat.Rows)
            {
                list.Add(new clsCategories
                {
                    Sel = row["sel"] == DBNull.Value ? 0 : Convert.ToInt32(row["sel"]),
                    ID = row["ID"]?.ToString() ?? "0",
                    Depart = row["Name"]?.ToString() ?? "",
                    Taxlevel = row["Taxlevel"] == DBNull.Value ? 0 : Convert.ToInt32(row["Taxlevel"])
                });
            }

            return list;
        }
        private void btnOtherSave_Click(object sender, EventArgs e)
        {
            btnFTPSave_Click(sender, e);
            SettingsUpdated?.Invoke();

        }
        CheckBox HeaderCheckBox = null;
        bool IsHeaderCheckBoxClicked = false;
        private void AddHeaderCheckBox()
        {
            HeaderCheckBox = new CheckBox();
            HeaderCheckBox.Size = new Size(15, 15);
            this.dataGridView1.Controls.Add(HeaderCheckBox);
        }
        private void HeaderCheckBoxClick(CheckBox HCheckBox)
        {
            IsHeaderCheckBoxClicked = true;
            foreach (DataGridViewRow dgvr in dataGridView1.Rows)
            {
                ((DataGridViewCheckBoxCell)dgvr.Cells["sel"]).Value = HCheckBox.Checked;
                dataGridView1.RefreshEdit();
                IsHeaderCheckBoxClicked = false;
            }
        }
        private void HeaderCheckBox_MouseClick(object sender, MouseEventArgs e)
        {
            HeaderCheckBoxClick((CheckBox)sender);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private void textpath_TextChanged(object sender, EventArgs e)
        {

        }

        private void chkStoked_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button5_Click(object sender, EventArgs e)// Save cat
        {
            btnDbSave_Click(sender, e);
        }
    }
}
