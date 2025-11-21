
namespace ASI_POS
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            this.btnExit = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnDbSave = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.textpath = new System.Windows.Forms.TextBox();
            this.labelPath = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnFTPSave = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.txtasistoreid = new System.Windows.Forms.TextBox();
            this.lblTaxalchl = new System.Windows.Forms.Label();
            this.txtTaxrate = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtStoreID = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtUPFolder = new System.Windows.Forms.TextBox();
            this.txtFTPserver = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtFTPpwd = new System.Windows.Forms.TextBox();
            this.txtFTPuid = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btnCatsave = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.btnOtherSave = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.txtInetValue = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPrcLvl = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkqtypack = new System.Windows.Forms.CheckBox();
            this.chkNoUpc = new System.Windows.Forms.CheckBox();
            this.chkStoked = new System.Windows.Forms.CheckBox();
            this.textMarkUp = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.sel = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tabPage4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnExit
            // 
            this.btnExit.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExit.Image = ((System.Drawing.Image)(resources.GetObject("btnExit.Image")));
            this.btnExit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExit.Location = new System.Drawing.Point(468, 458);
            this.btnExit.Margin = new System.Windows.Forms.Padding(4);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(140, 42);
            this.btnExit.TabIndex = 10;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(16, 4);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(603, 450);
            this.tabControl1.TabIndex = 9;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btnDbSave);
            this.tabPage1.Controls.Add(this.panel1);
            this.tabPage1.Location = new System.Drawing.Point(4, 33);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage1.Size = new System.Drawing.Size(595, 464);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Database Settings";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnDbSave
            // 
            this.btnDbSave.Image = ((System.Drawing.Image)(resources.GetObject("btnDbSave.Image")));
            this.btnDbSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDbSave.Location = new System.Drawing.Point(353, 308);
            this.btnDbSave.Margin = new System.Windows.Forms.Padding(4);
            this.btnDbSave.Name = "btnDbSave";
            this.btnDbSave.Size = new System.Drawing.Size(211, 50);
            this.btnDbSave.TabIndex = 10;
            this.btnDbSave.Text = "Save DB Settings";
            this.btnDbSave.UseVisualStyleBackColor = true;
            this.btnDbSave.Click += new System.EventHandler(this.btnDbSave_Click);
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.btnBrowse);
            this.panel1.Controls.Add(this.textpath);
            this.panel1.Controls.Add(this.labelPath);
            this.panel1.Location = new System.Drawing.Point(11, 31);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(553, 240);
            this.panel1.TabIndex = 1;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(443, 78);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(85, 31);
            this.btnBrowse.TabIndex = 78;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // textpath
            // 
            this.textpath.Location = new System.Drawing.Point(21, 113);
            this.textpath.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.textpath.Name = "textpath";
            this.textpath.Size = new System.Drawing.Size(505, 32);
            this.textpath.TabIndex = 77;
            this.textpath.TextChanged += new System.EventHandler(this.textpath_TextChanged);
            // 
            // labelPath
            // 
            this.labelPath.AutoSize = true;
            this.labelPath.Location = new System.Drawing.Point(17, 85);
            this.labelPath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPath.Name = "labelPath";
            this.labelPath.Size = new System.Drawing.Size(204, 24);
            this.labelPath.TabIndex = 76;
            this.labelPath.Text = "Select Data Folder Path";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnFTPSave);
            this.tabPage2.Controls.Add(this.panel2);
            this.tabPage2.Location = new System.Drawing.Point(4, 33);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage2.Size = new System.Drawing.Size(595, 464);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "FTP";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnFTPSave
            // 
            this.btnFTPSave.Image = ((System.Drawing.Image)(resources.GetObject("btnFTPSave.Image")));
            this.btnFTPSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnFTPSave.Location = new System.Drawing.Point(347, 338);
            this.btnFTPSave.Margin = new System.Windows.Forms.Padding(4);
            this.btnFTPSave.Name = "btnFTPSave";
            this.btnFTPSave.Size = new System.Drawing.Size(225, 52);
            this.btnFTPSave.TabIndex = 11;
            this.btnFTPSave.Text = "Save FTP Settings";
            this.btnFTPSave.UseVisualStyleBackColor = true;
            this.btnFTPSave.Click += new System.EventHandler(this.btnFTPSave_Click);
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.txtasistoreid);
            this.panel2.Controls.Add(this.lblTaxalchl);
            this.panel2.Controls.Add(this.txtTaxrate);
            this.panel2.Controls.Add(this.label10);
            this.panel2.Controls.Add(this.txtStoreID);
            this.panel2.Controls.Add(this.label9);
            this.panel2.Controls.Add(this.txtUPFolder);
            this.panel2.Controls.Add(this.txtFTPserver);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.label7);
            this.panel2.Controls.Add(this.txtFTPpwd);
            this.panel2.Controls.Add(this.txtFTPuid);
            this.panel2.Controls.Add(this.label8);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Location = new System.Drawing.Point(8, 8);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(564, 322);
            this.panel2.TabIndex = 2;
            // 
            // txtasistoreid
            // 
            this.txtasistoreid.Location = new System.Drawing.Point(389, 238);
            this.txtasistoreid.Name = "txtasistoreid";
            this.txtasistoreid.Size = new System.Drawing.Size(100, 32);
            this.txtasistoreid.TabIndex = 22;
            // 
            // lblTaxalchl
            // 
            this.lblTaxalchl.AutoSize = true;
            this.lblTaxalchl.Location = new System.Drawing.Point(37, 238);
            this.lblTaxalchl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTaxalchl.Name = "lblTaxalchl";
            this.lblTaxalchl.Size = new System.Drawing.Size(85, 24);
            this.lblTaxalchl.TabIndex = 20;
            this.lblTaxalchl.Text = "Tax_Rate";
            // 
            // txtTaxrate
            // 
            this.txtTaxrate.Location = new System.Drawing.Point(151, 236);
            this.txtTaxrate.Margin = new System.Windows.Forms.Padding(4);
            this.txtTaxrate.Name = "txtTaxrate";
            this.txtTaxrate.Size = new System.Drawing.Size(103, 32);
            this.txtTaxrate.TabIndex = 19;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(27, 187);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(0, 24);
            this.label10.TabIndex = 14;
            // 
            // txtStoreID
            // 
            this.txtStoreID.Location = new System.Drawing.Point(180, 17);
            this.txtStoreID.Margin = new System.Windows.Forms.Padding(4);
            this.txtStoreID.Name = "txtStoreID";
            this.txtStoreID.Size = new System.Drawing.Size(309, 32);
            this.txtStoreID.TabIndex = 13;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(37, 22);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(76, 24);
            this.label9.TabIndex = 12;
            this.label9.Text = "Store ID";
            // 
            // txtUPFolder
            // 
            this.txtUPFolder.Location = new System.Drawing.Point(180, 167);
            this.txtUPFolder.Margin = new System.Windows.Forms.Padding(4);
            this.txtUPFolder.Name = "txtUPFolder";
            this.txtUPFolder.Size = new System.Drawing.Size(309, 32);
            this.txtUPFolder.TabIndex = 11;
            // 
            // txtFTPserver
            // 
            this.txtFTPserver.Location = new System.Drawing.Point(180, 52);
            this.txtFTPserver.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPserver.Name = "txtFTPserver";
            this.txtFTPserver.Size = new System.Drawing.Size(309, 32);
            this.txtFTPserver.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(37, 171);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 24);
            this.label5.TabIndex = 8;
            this.label5.Text = "Folder";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(37, 133);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(89, 24);
            this.label6.TabIndex = 5;
            this.label6.Text = "Password";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(37, 91);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(102, 24);
            this.label7.TabIndex = 4;
            this.label7.Text = "User Name";
            // 
            // txtFTPpwd
            // 
            this.txtFTPpwd.Location = new System.Drawing.Point(180, 129);
            this.txtFTPpwd.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPpwd.Name = "txtFTPpwd";
            this.txtFTPpwd.PasswordChar = '*';
            this.txtFTPpwd.Size = new System.Drawing.Size(309, 32);
            this.txtFTPpwd.TabIndex = 3;
            // 
            // txtFTPuid
            // 
            this.txtFTPuid.Location = new System.Drawing.Point(180, 89);
            this.txtFTPuid.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPuid.Name = "txtFTPuid";
            this.txtFTPuid.Size = new System.Drawing.Size(309, 32);
            this.txtFTPuid.TabIndex = 2;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(37, 57);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(62, 24);
            this.label8.TabIndex = 0;
            this.label8.Text = "Server";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(268, 244);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 24);
            this.label1.TabIndex = 21;
            this.label1.Text = "ASI_StoreID";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.btnCatsave);
            this.tabPage3.Controls.Add(this.dataGridView1);
            this.tabPage3.Location = new System.Drawing.Point(4, 33);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(595, 413);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Categories";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // btnCatsave
            // 
            this.btnCatsave.Image = ((System.Drawing.Image)(resources.GetObject("btnCatsave.Image")));
            this.btnCatsave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCatsave.Location = new System.Drawing.Point(369, 354);
            this.btnCatsave.Margin = new System.Windows.Forms.Padding(4);
            this.btnCatsave.Name = "btnCatsave";
            this.btnCatsave.Size = new System.Drawing.Size(213, 49);
            this.btnCatsave.TabIndex = 3;
            this.btnCatsave.Text = "Save Categories";
            this.btnCatsave.UseVisualStyleBackColor = true;
            this.btnCatsave.Click += new System.EventHandler(this.btnCatsave_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.sel,
            this.ID,
            this.Description});
            this.dataGridView1.Location = new System.Drawing.Point(9, 14);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(4);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.Size = new System.Drawing.Size(573, 336);
            this.dataGridView1.TabIndex = 2;
            this.dataGridView1.VirtualMode = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.btnOtherSave);
            this.tabPage4.Controls.Add(this.panel3);
            this.tabPage4.Location = new System.Drawing.Point(4, 33);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage4.Size = new System.Drawing.Size(595, 464);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Others";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // btnOtherSave
            // 
            this.btnOtherSave.BackColor = System.Drawing.Color.LightGray;
            this.btnOtherSave.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOtherSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOtherSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOtherSave.Location = new System.Drawing.Point(436, 345);
            this.btnOtherSave.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnOtherSave.Name = "btnOtherSave";
            this.btnOtherSave.Size = new System.Drawing.Size(129, 47);
            this.btnOtherSave.TabIndex = 13;
            this.btnOtherSave.Text = "Save";
            this.btnOtherSave.UseVisualStyleBackColor = false;
            this.btnOtherSave.Click += new System.EventHandler(this.btnOtherSave_Click);
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.txtInetValue);
            this.panel3.Controls.Add(this.label3);
            this.panel3.Controls.Add(this.txtPrcLvl);
            this.panel3.Controls.Add(this.label4);
            this.panel3.Controls.Add(this.chkqtypack);
            this.panel3.Controls.Add(this.chkNoUpc);
            this.panel3.Controls.Add(this.chkStoked);
            this.panel3.Controls.Add(this.textMarkUp);
            this.panel3.Controls.Add(this.label12);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Location = new System.Drawing.Point(23, 22);
            this.panel3.Margin = new System.Windows.Forms.Padding(4);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(542, 308);
            this.panel3.TabIndex = 12;
            // 
            // txtInetValue
            // 
            this.txtInetValue.Location = new System.Drawing.Point(157, 131);
            this.txtInetValue.Name = "txtInetValue";
            this.txtInetValue.Size = new System.Drawing.Size(100, 32);
            this.txtInetValue.TabIndex = 32;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 131);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(99, 24);
            this.label3.TabIndex = 31;
            this.label3.Text = "Inet_Value";
            // 
            // txtPrcLvl
            // 
            this.txtPrcLvl.Location = new System.Drawing.Point(157, 76);
            this.txtPrcLvl.Name = "txtPrcLvl";
            this.txtPrcLvl.Size = new System.Drawing.Size(100, 32);
            this.txtPrcLvl.TabIndex = 30;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(27, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(105, 24);
            this.label4.TabIndex = 29;
            this.label4.Text = "Price Levels";
            // 
            // chkqtypack
            // 
            this.chkqtypack.AutoSize = true;
            this.chkqtypack.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkqtypack.Location = new System.Drawing.Point(313, 89);
            this.chkqtypack.Name = "chkqtypack";
            this.chkqtypack.Size = new System.Drawing.Size(106, 28);
            this.chkqtypack.TabIndex = 28;
            this.chkqtypack.Text = "Qty/Pack";
            this.chkqtypack.UseVisualStyleBackColor = true;
            // 
            // chkNoUpc
            // 
            this.chkNoUpc.AutoSize = true;
            this.chkNoUpc.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkNoUpc.Location = new System.Drawing.Point(313, 55);
            this.chkNoUpc.Name = "chkNoUpc";
            this.chkNoUpc.Size = new System.Drawing.Size(207, 28);
            this.chkNoUpc.TabIndex = 27;
            this.chkNoUpc.Text = "Incl. NoUPC Products";
            this.chkNoUpc.UseVisualStyleBackColor = true;
            // 
            // chkStoked
            // 
            this.chkStoked.AutoSize = true;
            this.chkStoked.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkStoked.Location = new System.Drawing.Point(313, 20);
            this.chkStoked.Margin = new System.Windows.Forms.Padding(4);
            this.chkStoked.Name = "chkStoked";
            this.chkStoked.Size = new System.Drawing.Size(190, 28);
            this.chkStoked.TabIndex = 17;
            this.chkStoked.Text = "Stocked Items Only";
            this.chkStoked.UseVisualStyleBackColor = true;
            // 
            // textMarkUp
            // 
            this.textMarkUp.Location = new System.Drawing.Point(157, 18);
            this.textMarkUp.Margin = new System.Windows.Forms.Padding(4);
            this.textMarkUp.Name = "textMarkUp";
            this.textMarkUp.Size = new System.Drawing.Size(132, 32);
            this.textMarkUp.TabIndex = 16;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(27, 18);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(122, 24);
            this.label12.TabIndex = 15;
            this.label12.Text = "MarkUp Price";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 187);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 24);
            this.label2.TabIndex = 14;
            // 
            // sel
            // 
            this.sel.FalseValue = "0";
            this.sel.HeaderText = "Select";
            this.sel.MinimumWidth = 6;
            this.sel.Name = "sel";
            this.sel.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.sel.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.sel.TrueValue = "1";
            this.sel.Width = 125;
            // 
            // ID
            // 
            this.ID.FillWeight = 50F;
            this.ID.HeaderText = "ID";
            this.ID.MinimumWidth = 6;
            this.ID.Name = "ID";
            this.ID.ReadOnly = true;
            this.ID.Width = 50;
            // 
            // Description
            // 
            this.Description.HeaderText = "Name";
            this.Description.MinimumWidth = 10;
            this.Description.Name = "Description";
            this.Description.ReadOnly = true;
            this.Description.Width = 900;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(621, 509);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form2";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tabPage4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button btnDbSave;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox textpath;
        private System.Windows.Forms.Label labelPath;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button btnFTPSave;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblTaxalchl;
        private System.Windows.Forms.TextBox txtTaxrate;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtStoreID;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtUPFolder;
        private System.Windows.Forms.TextBox txtFTPserver;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtFTPpwd;
        private System.Windows.Forms.TextBox txtFTPuid;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button btnCatsave;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Button btnOtherSave;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TextBox textMarkUp;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TextBox txtasistoreid;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkNoUpc;
        private System.Windows.Forms.CheckBox chkStoked;
        private System.Windows.Forms.TextBox txtInetValue;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPrcLvl;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkqtypack;
        private System.Windows.Forms.DataGridViewCheckBoxColumn sel;
        private System.Windows.Forms.DataGridViewTextBoxColumn ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Description;
    }
}