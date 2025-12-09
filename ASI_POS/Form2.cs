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

namespace ASI_POS
{
    public partial class Form2 : Form
    {
        DataTable dtCat;
        clsSettings settings = new clsSettings();
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            AddHeaderCheckBox();
            HeaderCheckBox.MouseClick += new MouseEventHandler(HeaderCheckBox_MouseClick);
            if (File.Exists("config//dbsettings.txt") && File.Exists("config//ftpsettings.txt") && File.Exists("config//others.txt"))
            {
                settings.LoadSettings();

                textpath.Text = settings.serverpath;

                txtStoreID.Text = settings.StoreId;
                txtFTPpwd.Text = settings.FtpPassword;
                txtFTPserver.Text = settings.FtpServer;
                txtFTPuid.Text = settings.FtpUserName;
                txtUPFolder.Text = settings.FtpUpFolder;
                txtdownloadpath.Text = settings.FtpDownFolder;
                txtasistoreid.Text = settings.Asi_Store_Id;
                txtInetValue.Text = settings.InvetValue;
                txtPrcLvl.Text = settings.PrcLevels;
                txttaxcode.Text = settings.TaxCode;
                txtstat.Text = settings.Stat;
                if (settings.StockedItems == 0)
                {
                    chkStoked.Checked = false;
                }
                else
                {
                    chkStoked.Checked = true;
                }
                if (settings.QtyperPack == 0)
                {
                    chkqtypack.Checked = false;
                }
                else
                {
                    chkqtypack.Checked = true;
                }
                if (settings.InclNoUpcProducts)
                {
                    chkNoUpc.Checked = true;
                }
                else
                {
                    chkNoUpc.Checked = false;
                }
                chkdiscountable.Checked = settings.AddDiscountable;
                chkfloor.Checked = settings.IncludeFloor;
                chkallqtyperpack.Checked = settings.AllQtyperPack;
                textMarkUp.Text = settings.MarkUpPrice.ToString();
            }
            if (File.Exists("config//cat.txt"))
            {
                loadCats();
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
            clsDbSettings clsdb = new clsDbSettings();
            clsdb.selectpath = textpath.Text;
            clsdb.UpFolder = txtUPFolder.Text;
            clsdb.DownFolder = txtdownloadpath.Text;
            try
            {
                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                using (StreamWriter sw = new StreamWriter(@"config\dbsettings.txt"))
                using (Newtonsoft.Json.JsonTextWriter writer = new Newtonsoft.Json.JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, clsdb);
                    sw.Close();
                    writer.Close();
                }
                MessageBox.Show("Saved Sucessfully!!", "Connection Status");
            }
            catch (Exception)
            {
                MessageBox.Show("Connection Failure, Please check Database settings", "Connection Status");
            }
            //loadCats();
            settings.LoadSettings();
        }

        private void btnFTPSave_Click(object sender, EventArgs e)
        {
            if (txtStoreID.Text.Trim().Length == 0 || txtFTPserver.Text.Trim().Length == 0 || txtFTPuid.Text.Trim().Length == 0 || txtFTPpwd.Text.Trim().Length == 0 || txtUPFolder.Text.Trim().Length == 0 || txttaxcode.Text.Trim().Length == 0)
            {
                MessageBox.Show("All fields are mandatory !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                clsFtpSettings clsftp = new clsFtpSettings();
                clsftp.StoreId = txtStoreID.Text;
                clsftp.Server = txtFTPserver.Text;
                clsftp.FtpUserName = txtFTPuid.Text;
                clsftp.FtpPassword = txtFTPpwd.Text;
                clsftp.TaxCode = txttaxcode.Text;
                clsftp.Asi_StoreId = txtasistoreid.Text;
                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                using (StreamWriter sw = new StreamWriter(@"config\ftpsettings.txt"))
                using (Newtonsoft.Json.JsonTextWriter writer = new Newtonsoft.Json.JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, clsftp);
                    sw.Close();
                    writer.Close();
                }
            }
            MessageBox.Show("Saved Sucessfully!!", "Ftp Setting", MessageBoxButtons.OK, MessageBoxIcon.Information);
            settings.LoadSettings();
        }

        private void btnCatsave_Click(object sender, EventArgs e)
        {
            var query = from r in dtCat.AsEnumerable()
                        select new
                        {
                            sel = r.IsNull("sel") ? 0 : Convert.ToInt32(r["sel"]),
                            ID = r.IsNull("ID") ? "0" : r.Field<string>("ID"),
                            Depart = r.Field<string>("Name"),
                            Taxlevel = r.IsNull("Taxlevel") ? 0 : Convert.ToInt32(r["Taxlevel"])
                        };
            if (query.Count() == 0)
            {
                MessageBox.Show(" Select Categories ", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                using (StreamWriter sw = new StreamWriter(@"config\cat.txt"))
                using (Newtonsoft.Json.JsonTextWriter writer = new Newtonsoft.Json.JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, query);
                    sw.Close();
                    writer.Close();
                }
            }
            MessageBox.Show("Saved Sucessfully!!", "Category", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void loadCats()
        {
            string jsoncats;
            var fileStream = new FileStream(@"config\cat.txt", FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                jsoncats = streamReader.ReadToEnd();
            }
            clsCategories[] clscat = JsonConvert.DeserializeObject<clsCategories[]>(jsoncats);
            OleDbConnection con;
            try
            {
                dtCat = new System.Data.DataTable();
                dataGridView1.AutoGenerateColumns = false;
                string servername = System.Environment.MachineName.ToString();
                string connectionstring = settings.ConnectionString;
                con = new OleDbConnection(connectionstring);

                OleDbCommand cmd = new OleDbCommand("SELECT DISTINCT 0 as sel,CAT as ID, Name, Taxlevel FROM cat ", con);
                con.Open();
                cmd.ExecuteNonQuery();
                OleDbDataAdapter adp = new OleDbDataAdapter(cmd);
                adp.Fill(dtCat);

                if (clscat != null)
                {
                    foreach (var itm in clscat)
                    {
                        if(itm.Sel == 1)
                        {
                            string depart = itm.Depart.ToString().Replace("'", "''");
                            DataRow row = dtCat.Select($"Name='{depart}'").FirstOrDefault();

                            if (row != null)
                            {
                                row["sel"] = 1;
                            }
                        }
                    }
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
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
            }
        }

        private void btnOtherSave_Click(object sender, EventArgs e)
        {
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
            JsonSerializer serializer = new JsonSerializer();
            using (StreamWriter sw = new StreamWriter(@"config\others.txt"))
            using (JsonTextWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, others);
                sw.Close();
                writer.Close();
            }
            MessageBox.Show("Saved Successfully !!", "Others", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                ((DataGridViewCheckBoxCell)dgvr.Cells["Sel"]).Value = HCheckBox.Checked;
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
    }
}
