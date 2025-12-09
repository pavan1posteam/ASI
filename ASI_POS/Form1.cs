using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using ASI_POS.Model;
using System.Globalization;

namespace ASI_POS
{
    public partial class Form1 : Form
    {
        int StoreId;
        DataTable dtResult = new DataTable();
        DataTable dtfullname = new DataTable();
        clsSettings settings = new clsSettings();
        GenerateCSV generateCSV = new GenerateCSV();
        private static string Argsprams { get; set; }
        public static Form1 Instance;
        string pathProduct1 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Upload");
        private static readonly System.Collections.Concurrent.ConcurrentQueue<string> _pendingStatus = new System.Collections.Concurrent.ConcurrentQueue<string>();
        private bool _isLoaded = false;
        public Form1(string[] args)
        {
            InitializeComponent();
            Instance = this;
            if (args.Length > 0)
            {
                Argsprams = args[0];
            }
            else
            {
                Argsprams = "";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _isLoaded = true;

            while (_pendingStatus.TryDequeue(out var msg))
            {
                try { ShowStatus(msg); } catch {  }
            }
            string pathProduct = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config");
            if (!Directory.Exists(pathProduct))
            {
                Directory.CreateDirectory("config");
            }
            string pathProduct1 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Upload");
            if (!Directory.Exists(pathProduct1))
            {
                Directory.CreateDirectory("Upload");

            }
            if (!File.Exists("config\\dbsettings.txt"))
            {
                FileStream fs;
                fs = System.IO.File.Open("config\\dbsettings.txt", FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
                fs.Close();
            }
            if (!File.Exists("config\\ftpsettings.txt"))
            {
                FileStream fs;
                fs = System.IO.File.Open(@"config\\ftpsettings.txt", FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
                fs.Close();
            }
            if (!File.Exists("config\\cat.txt"))
            {
                FileStream fs;
                fs = System.IO.File.Open(@"config\\cat.txt", FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
                fs.Close();
            }
            if (!File.Exists("config\\others.txt"))
            {
                FileStream fs;
                fs = System.IO.File.Open(@"config\\others.txt", FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
                fs.Close();
            }
            if (Argsprams != "")
            {
                Uploading();
                Environment.Exit(0);
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            Form2 frmSettings = new Form2();
            frmSettings.ShowDialog();
            Cursor.Current = Cursors.WaitCursor;
            Cursor.Current = Cursors.Default;
        }
        //public void ShowStatus(string str)
        //{
        //    var item = str;
        //    listBox1.Items.Add(item);
        //    listBox1.Refresh();
        //}

        public void ShowStatus(string str)
        {
            if (string.IsNullOrEmpty(str)) return;

            if (listBox1 == null || listBox1.IsDisposed || !_isLoaded)
            {
                _pendingStatus.Enqueue(str);
                return;
            }

            if (this.InvokeRequired)
            {
                try
                {
                    this.BeginInvoke(new Action<string>(ShowStatus), str);
                }
                catch
                {
                    _pendingStatus.Enqueue(str);
                }
                return;
            }
            try
            {
                listBox1.Items.Add(str);
                listBox1.TopIndex = listBox1.Items.Count - 1;
                listBox1.Refresh();
            }
            catch
            {
                _pendingStatus.Enqueue(str);
            }
        }


        private void btnUpload_Click(object sender, EventArgs e)
        {
            Uploading();
        }
        private void Uploading()
        {
            settings.LoadSettings();
            if (new FileInfo(@"config\dbsettings.txt").Length != 0 && new FileInfo(@"config\ftpsettings.txt").Length != 0) 
            {
                ShowStatus("Connecting to Database");
                string jsoncats;
                var FileStream = new FileStream(@"config\cat.txt", FileMode.Open, FileAccess.Read);
                using (var StreamReader = new StreamReader(FileStream, Encoding.UTF8))
                {
                    jsoncats = StreamReader.ReadToEnd();
                    StreamReader.Close();
                }
                FileStream.Close();
                clsCategories[] clscat;
                List<clscategory> cats = new List<clscategory>();
                if (!string.IsNullOrEmpty(jsoncats))
                {
                    
                    clscat = JsonConvert.DeserializeObject<clsCategories[]>(jsoncats);
                    string strcats = "";
                    List<string> catlist = new List<string>();
                    foreach (clsCategories cat in clscat)
                    {
                        clscategory c = new clscategory();
                        if (cat.Sel == 1)
                        {
                            if (strcats.Length > 0)
                            {
                                strcats += "," + cat.ID;
                            }
                            else
                            {
                                strcats += cat.ID;
                            }
                            catlist.Add(cat.ID);
                        }
                        c.catid = cat.ID;
                        c.catname = cat.Depart;
                        c.taxlevel = cat.Taxlevel;
                        cats.Add(c);
                    }
                    string servername = System.Environment.MachineName.ToString();
                    settings.LoadSettings();
                    string ConnectionString = settings.ConnectionString;
                    StoreId = Convert.ToInt32(settings.StoreId);
                    string taxcode = settings.TaxCode;
                    decimal markupPrice = settings.MarkUpPrice;
                    int AsiId = Convert.ToInt32(settings.Asi_Store_Id);
                    int QtyperPack = Convert.ToInt32(settings.QtyperPack);
                    string Inet_Value = settings.InvetValue;
                    string PriceLevels = settings.PrcLevels;
                    var values = settings.Stat.Split(',').Select(x => x.Trim()).Where(x => int.TryParse(x, out _)).Select(x => $"'{x}'");   
                    string stat = string.Join(",", values);

                    CreateStructure();
                    CreateFullNameStructure();
                    #region ASI_POS
                    string liqtbl = "SELECT " + StoreId + " as Storeid,u.UPC as Upc,s.BACK as qty, s.FLOOR as qty2, u.LEVEL as InetValue, p.LEVEL as prclvl,i.DEPOS as depcode, ";
                    liqtbl += "i.SKU as Sku, i.PACK as pack,i.SNAME as Uom,";
                    liqtbl += "i.NAME as StoreProductName,i.NAME as StoreDescription, i.cat as icat,p.Onsale as Discountable,";
                    liqtbl += "p.PRICE as Price,p.PROMO as promcode, 0 as sprice,p.SALE as sprce1,'' as Start, '' as end, p.DCODE as DiscountCode,'' as altupc1,";
                    liqtbl += "'' as altupc2,'' as altupc3,'' as altupc4,'' as altupc5,i.VINTAGE as vintage,  i.ACOST as Cost,i.TYPENAME as pcat,'' as pcat1,'' as pcat2,'' as country, '' as region  ";
                    liqtbl += "FROM ((inv i left join upc u on i.SKU = u.SKU) left join stk s on i.SKU = s.SKU) left join prc p on i.SKU = p.SKU  ";
                    liqtbl += "where s.STAT in ("+ stat + ") and p.STORE =  " + AsiId + " and s.STORE = " + AsiId + " ";// + strStock + strcats
                    
                    string depositquery = "Select DEPOS as depcode, UNIT as depositvalue from DEP";
                    string saledatequery = "Select  PROMO as promocode, START as sdate, STOP as edate from slh";
                    string taxquery = "Select  CODE as taxcode, RATE as taxrate, cat as tcat, level as taxlevel from txc ";


                    string noupcproducts = "Select " + StoreId + " as Storeid,i.SKU as Sku, s.BACK as qty, s.FLOOR as qty2, p.LEVEL as prclvl,i.DEPOS as depcode, ";
                    noupcproducts += "i.PACK as pack,i.SNAME as Uom,";
                    noupcproducts += "i.NAME as StoreProductName,i.NAME as StoreDescription,i.cat as icat,p.Onsale as Discountable,";
                    noupcproducts += "p.PRICE as Price,p.PROMO as promcode, 0 as sprice,p.SALE as sprce1,'' as Start, '' as end,p.DCODE as DiscountCode,  '' as altupc1,";
                    noupcproducts += "'' as altupc2,'' as altupc3,'' as altupc4,'' as altupc5,i.VINTAGE as vintage, i.ACOST as Cost,i.TYPENAME as pcat,'' as pcat1,'' as pcat2,'' as country, '' as region  ";
                    noupcproducts += "FROM ((inv i  left join stk s on i.SKU = s.SKU) left join prc p on i.SKU = p.SKU ) ";
                    noupcproducts += "where s.STAT in (" + stat + ") and p.STORE =  " + AsiId + " and s.STORE = " + AsiId + " ";// + strStock + strcats
                    noupcproducts += " and i.sku not in (Select SKU from upc)";
                    #endregion

                    #region 12715
                    //string liqtbl = "SELECT " + StoreId + " as Storeid,u.UPC as Upc,s.BACK as qty, s.FLOOR as qty2, u.LEVEL as InetValue, p.LEVEL as prclvl,i.DEPOS as depcode, ";
                    //liqtbl += "i.SKU as Sku, i.PACK as pack,i.SNAME as Uom,";
                    //liqtbl += "i.NAME as StoreProductName,i.memo as StoreDescription,";
                    //liqtbl += "p.PRICE as Price,p.PROMO as promcode, 0 as sprice,p.SALE as sprce1,'' as Start, '' as end, " + taxrate + " as Tax, '' as altupc1,";
                    //liqtbl += "'' as altupc2,'' as altupc3,'' as altupc4,'' as altupc5,i.Freeform1 as vintage, i.ACOST as Cost,i.TYPENAME as pcat,'' as pcat1,'' as pcat2,'' as country, '' as region  ";
                    //liqtbl += "FROM ((inv i left join upc u on i.SKU = u.SKU) left join stk s on i.SKU = s.SKU) left join prc p on i.SKU = p.SKU  ";
                    //liqtbl += "where s.STAT = '2' and p.STORE =  " + AsiId + " and s.STORE = " + AsiId + "" + strStock;

                    //string depositquery = "Select DEPOS as depcode, UNIT as depositvalue from DEP";
                    //string saledatequery = "Select  PROMO as promocode, START as sdate, STOP as edate from slh";

                    //string noupcproducts = "Select " + StoreId + " as Storeid,i.SKU as Sku, s.BACK as qty, s.FLOOR as qty2, p.LEVEL as prclvl,i.DEPOS as depcode, ";
                    //noupcproducts += "i.PACK as pack,i.SNAME as Uom,";
                    //noupcproducts += "i.NAME as StoreProductName,i.memo as StoreDescription,";
                    //noupcproducts += "p.PRICE as Price,p.PROMO as promcode, 0 as sprice,p.SALE as sprce1,'' as Start, '' as end, " + taxrate + " as Tax, '' as altupc1,";
                    //noupcproducts += "'' as altupc2,'' as altupc3,'' as altupc4,'' as altupc5,i.Freeform1 as vintage, i.ACOST as Cost,i.TYPENAME as pcat,'' as pcat1,'' as pcat2,'' as country, '' as region  ";
                    //noupcproducts += "FROM ((inv i  left join stk s on i.SKU = s.SKU) left join prc p on i.SKU = p.SKU ) ";
                    //noupcproducts += "where s.STAT = '2' and p.STORE =  " + AsiId + " and s.STORE = " + AsiId + "" + strStock;
                    //noupcproducts += " and i.sku not in (Select SKU from upc)";
                    #endregion
                    DataTable dtLiqcode = new DataTable();
                    DataTable dtdepositcode = new DataTable();
                    DataTable dtsdatecode = new DataTable();
                    DataTable dtnoupc = new DataTable();
                    DataTable dttaxcode = new DataTable();

                    using (OleDbConnection con = new OleDbConnection(ConnectionString))
                    {
                        using (OleDbCommand cmd = new OleDbCommand(liqtbl, con))
                        {
                            using (OleDbDataAdapter adp = new OleDbDataAdapter(cmd))
                            {
                                adp.Fill(dtLiqcode);
                                ShowStatus("Retrieving Data");
                            }
                        }
                        using (OleDbCommand cmd = new OleDbCommand(depositquery, con))
                        {
                            using (OleDbDataAdapter adp1 = new OleDbDataAdapter(cmd))
                            {
                                adp1.Fill(dtdepositcode);
                            }
                        }
                        using (OleDbCommand cmd = new OleDbCommand(saledatequery, con))
                        {
                            using (OleDbDataAdapter adp2 = new OleDbDataAdapter(cmd))
                            {
                                adp2.Fill(dtsdatecode);
                            }
                        }
                        using (OleDbCommand cmd = new OleDbCommand(taxquery, con))
                        {
                            using (OleDbDataAdapter adp2 = new OleDbDataAdapter(cmd))
                            {
                                adp2.Fill(dttaxcode);
                            }
                        }
                        if (settings.InclNoUpcProducts)
                        {
                            using (OleDbCommand cmd = new OleDbCommand(noupcproducts, con))
                            {
                                using (OleDbDataAdapter adp2 = new OleDbDataAdapter(cmd))
                                {
                                    adp2.Fill(dtnoupc);
                                }
                            }
                        }
                    }
                    #region Product File
                    DataTable finalResult = new DataTable();
                    finalResult.Columns.Add("StoreId", typeof(string));
                    finalResult.Columns.Add("Upc", typeof(string));
                    finalResult.Columns.Add("Qty", typeof(int));
                    finalResult.Columns.Add("Sku", typeof(string));
                    finalResult.Columns.Add("Uom", typeof(string));
                    finalResult.Columns.Add("Pack", typeof(int));
                    finalResult.Columns.Add("StoreProductName", typeof(string));
                    finalResult.Columns.Add("StoreDescription", typeof(string));
                    finalResult.Columns.Add("Price", typeof(decimal));
                    finalResult.Columns.Add("Sprice", typeof(string));
                    finalResult.Columns.Add("Start", typeof(string));
                    finalResult.Columns.Add("End", typeof(string));
                    finalResult.Columns.Add("Tax", typeof(decimal));
                    if (StoreId == 10001)
                        finalResult.Columns.Add("TaxLevel", typeof(int));
                    finalResult.Columns.Add("Altupc1", typeof(string));
                    finalResult.Columns.Add("Altupc2", typeof(string));
                    finalResult.Columns.Add("Altupc3", typeof(string));
                    finalResult.Columns.Add("Altupc4", typeof(string));
                    finalResult.Columns.Add("Altupc5", typeof(string));
                    finalResult.Columns.Add("Vintage", typeof(string));
                    finalResult.Columns.Add("Cost", typeof(string));
                    finalResult.Columns.Add("Deposit", typeof(string));
                    if (settings.AddDiscountable)
                        finalResult.Columns.Add("Discountable", typeof(int));
                    if (StoreId == 10128)
                        finalResult.Columns.Add("DiscountCode", typeof(string));
                    #endregion
                    #region full name 
                    DataTable fullResult = new DataTable();
                    fullResult.Columns.Add("pname", typeof(string));
                    fullResult.Columns.Add("pdesc", typeof(string));
                    fullResult.Columns.Add("Upc", typeof(string));
                    fullResult.Columns.Add("Sku", typeof(string));
                    fullResult.Columns.Add("Price", typeof(decimal));
                    fullResult.Columns.Add("Uom", typeof(string));
                    fullResult.Columns.Add("Pack", typeof(int));
                    fullResult.Columns.Add("pcat", typeof(string));
                    fullResult.Columns.Add("pcat1", typeof(string));
                    fullResult.Columns.Add("pcat2", typeof(string));
                    fullResult.Columns.Add("country", typeof(string));
                    fullResult.Columns.Add("region", typeof(string));
                    #endregion
                    var taxLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (DataRow tr in dttaxcode.Rows)
                    {
                        string tcat = tr["tcat"]?.ToString()?.Trim() ?? "";
                        string rate = tr["taxrate"]?.ToString()?.Trim() ?? "";

                        if (!string.IsNullOrEmpty(tcat) && !taxLookup.ContainsKey(tcat))
                            taxLookup[tcat] = rate;
                    }
                    Dictionary<string, DataRow> skuLookup = new Dictionary<string, DataRow>(StringComparer.OrdinalIgnoreCase);

                    foreach (DataRow dr in dtLiqcode.Rows)
                    {
                        var rawUpc = dr.GetString("Upc");
                        if (string.IsNullOrEmpty(rawUpc) || !Regex.IsMatch(rawUpc, @"^[0-9]+$")) continue;
                        var upc = "#" + rawUpc;

                        string INETVALUE = dr["InetValue"].ToString();
                        string prcLevel = dr["prclvl"].ToString();
                        string sku = dr["Sku"].ToString().Trim();

                        if (skuLookup.TryGetValue(sku, out DataRow existingRow))
                        {
                            bool alreadyExists = false;

                            string existingUpcMain = existingRow["Upc"]?.ToString() ?? "";
                            if (existingUpcMain.Equals(upc, StringComparison.OrdinalIgnoreCase))
                                alreadyExists = true;
                            else
                            {
                                for (int i = 1; i <= 5; i++)
                                {
                                    string altCol = "Altupc" + i;
                                    string altValue = existingRow[altCol]?.ToString() ?? "";
                                    if (altValue.Equals(upc, StringComparison.OrdinalIgnoreCase))
                                    {
                                        alreadyExists = true;
                                        break;
                                    }
                                }
                            }

                            if (!alreadyExists)
                            {
                                for (int i = 1; i <= 5; i++)
                                {
                                    string altCol = "Altupc" + i;
                                    if (string.IsNullOrWhiteSpace(existingRow[altCol]?.ToString()))
                                    {
                                        existingRow[altCol] = upc;
                                        break;
                                    }
                                }
                            }
                            continue;
                        }

                        DataRow newRow = finalResult.NewRow();
                        DataRow fullrow = fullResult.NewRow();

                        newRow["StoreId"] = dr["StoreId"];
                        newRow["Upc"] = upc;
                        var pqty = Convert.ToInt32(dr["qty"]);
                        if (settings.IncludeFloor)
                            pqty += Convert.ToInt32(dr["qty2"]);
                        newRow["Qty"] = pqty;
                        newRow["Sku"] = "#" + sku;
                        newRow["Uom"] = dr["Uom"];
                        newRow["Pack"] = 1;
                        string pcat = (dr["pcat"]?.ToString() ?? "").ToUpper().Trim();
                        newRow["Pack"] = 1;

                        bool isBeer = pcat.Contains("BEER");
                        bool isWine = pcat.Contains("WINE");
                        bool isLiquor = pcat.Contains("LIQUOR");
                        if (settings.AllQtyperPack)
                        {
                            int pack = 1;
                            if (!int.TryParse(dr["pack"]?.ToString(), out pack) || pack <= 0) { }
                            else
                            {
                                int qtypack = pqty / pack;
                                pqty = qtypack;
                                newRow["Pack"] = pack;
                                newRow["Qty"] = qtypack;
                            }
                        }
                        else if (QtyperPack == 1 && isBeer && !isWine && !isLiquor)
                        {
                            int pack = 1;
                            if (!int.TryParse(dr["pack"]?.ToString(), out pack) || pack <= 0) { }
                            else
                            {
                                int qtypack = pqty / pack;
                                pqty = qtypack;
                                newRow["Pack"] = pack;
                                newRow["Qty"] = qtypack;
                            }
                        }
                        if (settings.StockedItems == 1 && pqty < 0)
                        {
                            continue;
                        }
                        newRow["StoreProductName"] = dr["StoreProductName"];
                        string productDesc = dr["StoreDescription"].ToString();
                        productDesc = productDesc.Replace("\r", " ").Replace("\n", " ").Replace("\"", "")  .Trim();

                        if (string.IsNullOrEmpty(productDesc))
                            productDesc = dr["StoreProductName"].ToString();
                        newRow["StoreDescription"] = productDesc;
                        decimal price = Convert.ToDecimal(dr["Price"]);
                        newRow["Price"] = price + price * (markupPrice / 100);
                        newRow["Sprice"] = dr["Sprice"];
                        newRow["Start"] = dr["Start"];
                        newRow["End"] = dr["End"];
                        var invcat = dr["icat"]?.ToString()?.Trim();
                        newRow["Tax"] = ComputeTaxRateForInv(dr, taxcode, dttaxcode, cats);
                        var match = cats.FirstOrDefault(x => x.catid.Equals(invcat));
                        if (match != null && StoreId == 10001)
                        {
                            newRow["TaxLevel"] = match.taxlevel;
                        }
                        newRow["Altupc1"] = dr["Altupc1"];
                        newRow["Altupc2"] = dr["Altupc2"];
                        newRow["Altupc3"] = dr["Altupc3"];
                        newRow["Altupc4"] = dr["Altupc4"];
                        newRow["Altupc5"] = dr["Altupc5"];
                        string vintage_value = dr["vintage"].ToString();
                        newRow["Vintage"] = Regex.IsMatch(vintage_value, @"^\d{4,}")? dr["vintage"]: "";
                        if (settings.AddDiscountable)
                        {
                            var chk = dr["Discountable"];
                            if (chk.Equals("F"))
                                newRow["Discountable"] = 0;
                            else
                                newRow["Discountable"] = 1;
                        }
                        if (StoreId == 10128 && settings.AddDiscountable)
                        {
                            newRow["DiscountCode"] = dr["DiscountCode"];
                            string code = dr["DiscountCode"].ToString();
                            newRow["Discountable"] = string.IsNullOrWhiteSpace(code) ? 0 : 1;
                        }
                        newRow["Cost"] = dr["Cost"];
                        newRow["Deposit"] = "0";

                        fullrow["pname"] = dr["StoreProductName"];
                        fullrow["pdesc"] = productDesc;
                        fullrow["Upc"] = dr["Upc"];
                        fullrow["Sku"] = dr["Sku"];
                        fullrow["Price"] = dr["Price"];
                        fullrow["Uom"] = dr["Uom"];
                        fullrow["Pack"] = newRow["Pack"];
                        fullrow["pcat"] = dr["pcat"];
                        fullrow["pcat1"] = dr["pcat1"];
                        fullrow["pcat2"] = dr["pcat2"];
                        fullrow["country"] = dr["country"];
                        fullrow["region"] = dr["region"];

                        string depostcode = dr["depcode"].ToString();
                        foreach (DataRow drdep in dtdepositcode.Rows)
                        {
                            if (depostcode.Equals(drdep["depcode"].ToString()))
                            {
                                decimal val = Convert.ToDecimal(drdep["depositvalue"].ToString());
                                int pack = Convert.ToInt32(dr["pack"].ToString());
                                newRow["Deposit"] = val * pack;
                                break;
                            }
                        }
                        string prmcode = dr["promcode"].ToString();
                        foreach (DataRow drprom in dtsdatecode.Rows)
                        {
                            if (prmcode.Equals(drprom["promocode"].ToString()))
                            {
                                newRow["Sprice"] = dr["sprce1"];
                                decimal spricecheck = Convert.ToDecimal(newRow["Sprice"].ToString());
                                if(spricecheck > 0)
                                {
                                    if (!drprom["sdate"].ToString().Contains("1899"))
                                        newRow["Start"] = drprom["sdate"];
                                    newRow["End"] = drprom["edate"];
                                    if (drprom["edate"].ToString().Contains("1899"))
                                    {
                                        newRow["End"] = "/ /";
                                    }
                                }
                                break;
                            }
                        }

                        if (Inet_Value.Contains(INETVALUE) && PriceLevels.Contains(prcLevel) && INETVALUE.Equals(prcLevel) && pqty >= 0)
                        {
                            if (catlist.Contains(invcat))
                            {
                                fullResult.Rows.Add(fullrow);
                                finalResult.Rows.Add(newRow);

                                skuLookup[sku] = newRow;
                            }
                            
                        }
                    }
                    if (settings.InclNoUpcProducts)//Include No UPCs Products
                    {
                        HashSet<string> existingSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        HashSet<string> existingUpcs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                        foreach (DataRow r in finalResult.Rows)
                        {
                            existingSkus.Add(r["Sku"].ToString().Trim());
                            existingUpcs.Add(r["Upc"].ToString().Trim());
                        }

                        foreach (DataRow dr in dtnoupc.Rows)
                        {

                            DataRow newRow = finalResult.NewRow();
                            DataRow fullrow = fullResult.NewRow();

                            string pcatValue = dr["pcat"].ToString().Trim();
                            string prcLevel = dr["prclvl"].ToString();
                            string sku = dr["Sku"].ToString().Trim();
                            if (string.IsNullOrEmpty(sku))
                                continue;

                            string generatedUpc = dr["StoreId"] + "99" + sku;
                            if (existingUpcs.Contains(generatedUpc))
                                continue;
                            if (existingSkus.Contains("#" + sku))
                                continue;
                            
                            newRow["StoreId"] = dr["StoreId"];
                            newRow["Sku"] = "#" + sku;
                            newRow["Upc"] = generatedUpc;
                            var pqty = Convert.ToInt32(dr["qty"]);
                            if (settings.IncludeFloor)
                                pqty += Convert.ToInt32(dr["qty2"]);
                            newRow["Qty"] = pqty;

                            newRow["Uom"] = dr["Uom"];
                            newRow["Pack"] = dr["pack"];
                            string pcat = (dr["pcat"]?.ToString() ?? "").ToUpper().Trim();
                            newRow["Pack"] = 1;

                            bool isBeer = pcat.Contains("BEER");
                            bool isWine = pcat.Contains("WINE");
                            bool isLiquor = pcat.Contains("LIQUOR");
                            if (settings.AllQtyperPack)
                            {
                                int pack = 1;
                                if (!int.TryParse(dr["pack"]?.ToString(), out pack) || pack <= 0) { }
                                else
                                {
                                    int qtypack = pqty / pack;
                                    pqty = qtypack;
                                    newRow["Pack"] = pack;
                                    newRow["Qty"] = qtypack;
                                }
                            }
                            else if (QtyperPack == 1 && isBeer && !isWine && !isLiquor)
                            {
                                int pack = 1;
                                if (!int.TryParse(dr["pack"]?.ToString(), out pack) || pack <= 0) { }
                                else
                                {
                                    int qtypack = pqty / pack;
                                    pqty = qtypack;
                                    newRow["Pack"] = pack;
                                    newRow["Qty"] = qtypack;
                                }
                            }
                            if (settings.StockedItems == 1 && pqty < 0)
                            {
                                continue;
                            }
                            newRow["StoreProductName"] = dr["StoreProductName"];
                            string productDesc = dr["StoreDescription"].ToString();
                            productDesc = productDesc.Replace("\r", " ").Replace("\n", " ").Replace("\"", "").Trim();

                            if (string.IsNullOrEmpty(productDesc))
                                productDesc = dr["StoreProductName"].ToString();
                            newRow["StoreDescription"] = productDesc;

                            newRow["Price"] = dr["Price"];
                            newRow["Sprice"] = dr["Sprice"];
                            newRow["Start"] = dr["Start"];
                            newRow["End"] = dr["End"];
                            var invcat = dr["icat"]?.ToString()?.Trim();
                            newRow["Tax"] = ComputeTaxRateForInv(dr, taxcode, dttaxcode, cats);
                            var match = cats.FirstOrDefault(x => x.catid.Equals(invcat));
                            if (match != null)
                            {
                                newRow["TaxLevel"] = match.taxlevel;
                            }
                            newRow["Altupc1"] = dr["Altupc1"];
                            newRow["Altupc2"] = dr["Altupc2"];
                            newRow["Altupc3"] = dr["Altupc3"];
                            newRow["Altupc4"] = dr["Altupc4"];
                            newRow["Altupc5"] = dr["Altupc5"];
                            string vintage_value = dr["vintage"].ToString();
                            newRow["Vintage"] = Regex.IsMatch(vintage_value, @"^\d{4,}") ? dr["vintage"] : "";
                            newRow["Cost"] = dr["Cost"];
                            newRow["Deposit"] = "0";
                            if (settings.AddDiscountable)
                            {
                                var chk = dr["Discountable"];
                                if (chk.Equals("F"))
                                    newRow["Discountable"] = 0;
                                else
                                    newRow["Discountable"] = 1;
                            }
                            if (StoreId == 10128 && settings.AddDiscountable)
                            {
                                newRow["DiscountCode"] = dr["DiscountCode"];
                                string code = dr["DiscountCode"].ToString();
                                newRow["Discountable"] = string.IsNullOrWhiteSpace(code) ? 0 : 1;
                            }
                            fullrow["pname"] = dr["StoreProductName"];
                            fullrow["pdesc"] = productDesc;
                            fullrow["Upc"] = newRow["Upc"];
                            fullrow["Sku"] = dr["Sku"];
                            fullrow["Price"] = dr["Price"];
                            fullrow["Uom"] = dr["Uom"];
                            fullrow["Pack"] = dr["pack"];
                            fullrow["pcat"] = dr["pcat"];
                            fullrow["pcat1"] = dr["pcat1"];
                            fullrow["pcat2"] = dr["pcat2"];
                            fullrow["country"] = dr["country"];
                            fullrow["region"] = dr["region"];

                            string depostcode = dr["depcode"].ToString();
                            foreach (DataRow drdep in dtdepositcode.Rows)
                            {
                                if (depostcode.Equals(drdep["depcode"].ToString()))
                                {
                                    decimal val = Convert.ToDecimal(drdep["depositvalue"].ToString());
                                    int pack = Convert.ToInt32(dr["pack"].ToString());
                                    newRow["Deposit"] = val * pack;
                                    break;
                                }
                            }
                            string prmcode = dr["promcode"].ToString();
                            foreach (DataRow drprom in dtsdatecode.Rows)
                            {
                                if (prmcode.Equals(drprom["promocode"].ToString()))
                                {
                                    newRow["Sprice"] = dr["sprce1"];
                                    decimal spricecheck = Convert.ToDecimal(newRow["Sprice"].ToString());
                                    if (spricecheck > 0)
                                    {
                                        if (!drprom["sdate"].ToString().Contains("1899"))
                                            newRow["Start"] = drprom["sdate"];
                                        newRow["End"] = drprom["edate"];
                                        if (drprom["edate"].ToString().Contains("1899"))
                                        {
                                            newRow["End"] = "/ /";
                                        }

                                    }
                                    break;
                                }
                            }
                            if (catlist.Contains(invcat))
                            {
                                fullResult.Rows.Add(fullrow);
                                finalResult.Rows.Add(newRow);

                                existingSkus.Add("#" + sku);
                                existingUpcs.Add(generatedUpc);
                            }
                            
                        }
                    }

                    ShowStatus("Generating csv file");
                    string filename = generateCSV.GenerateCSVFile(finalResult);
                    ShowStatus("Connecting to FTP");
                    Upload("Upload//" + filename);
                    ShowStatus("Uploading " + filename);
                    if (File.Exists("Upload//" + filename))
                    {
                        if (File.Exists("Upload//" + filename))
                        {
                            File.Delete("Upload//" + filename);
                        }
                    }
                    filename = generateCSV.GenerateCSVFile(fullResult);
                    ShowStatus("Uploading " + filename);
                    Upload("Upload//" + filename);
                    ShowStatus("Inventory Upload completed");
                    if (File.Exists("Upload//" + filename))
                    {
                        if (File.Exists("Upload//" + filename))
                        {
                            File.Delete("Upload//" + filename);
                        }
                    }
                    ShowStatus("Disconnected from FTP");
                }
                else
                {
                    ShowStatus("Please do all the setting !");
                }

            }
            else
            {
                MessageBox.Show("All the Setting are required !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        public decimal ComputeTaxRateForInv(DataRow drProduct, string ecTaxCode, DataTable dtTxc, List<clscategory> cats)
        {
            decimal taxRate = 0m;
            string invcat = drProduct["icat"]?.ToString()?.Trim() ?? "";
            string cTaxLevel = "0";
            var match = cats.FirstOrDefault(x => x.catid.Equals(invcat, StringComparison.OrdinalIgnoreCase));
            if (match != null)
                cTaxLevel = match.taxlevel.ToString().Trim();
            var txcRows = dtTxc.AsEnumerable()
                .Where(r => string.Equals((r["taxcode"]?.ToString() ?? "").Trim(), (ecTaxCode ?? "").Trim(), StringComparison.OrdinalIgnoreCase));

            var row = txcRows.FirstOrDefault(r => string.Equals((r["taxlevel"]?.ToString() ?? "").Trim(), cTaxLevel.Trim(), StringComparison.OrdinalIgnoreCase));
            if (row != null)
            {
                decimal parsed;
                if (decimal.TryParse(row["taxrate"]?.ToString(), out parsed))
                    taxRate = parsed;
            }
            return taxRate;
        }

        private void CreateStructure()
        {
            DataColumn col1 = new DataColumn("storeid");
            dtResult.Columns.Add(col1);
            DataColumn col2 = new DataColumn("upc");
            dtResult.Columns.Add(col2);
            DataColumn col3 = new DataColumn("qty", typeof(double));
            dtResult.Columns.Add(col3);
            DataColumn col4 = new DataColumn("sku");
            dtResult.Columns.Add(col4);
            DataColumn col5 = new DataColumn("pack");
            dtResult.Columns.Add(col5);
            DataColumn col6 = new DataColumn("uom");
            dtResult.Columns.Add(col6);
            DataColumn col7 = new DataColumn("StoreProductName");
            dtResult.Columns.Add(col7);
            DataColumn col8 = new DataColumn("Storedescription");
            dtResult.Columns.Add(col8);
            DataColumn col9 = new DataColumn("price", typeof(double));
            dtResult.Columns.Add(col9);
            DataColumn col10 = new DataColumn("sprice", typeof(double));
            dtResult.Columns.Add(col10);
            DataColumn col11 = new DataColumn("start");
            dtResult.Columns.Add(col11);
            DataColumn col12 = new DataColumn("end");
            dtResult.Columns.Add(col12);
            DataColumn col13 = new DataColumn("tax");
            dtResult.Columns.Add(col13);
            DataColumn col14 = new DataColumn("altupc1");
            dtResult.Columns.Add(col14);
            DataColumn col15 = new DataColumn("altupc2");
            dtResult.Columns.Add(col15);
            DataColumn col16 = new DataColumn("altupc3");
            dtResult.Columns.Add(col16);
            DataColumn col17 = new DataColumn("altupc4");
            dtResult.Columns.Add(col17);
            DataColumn col18 = new DataColumn("altupc5");
            dtResult.Columns.Add(col18);
            DataColumn col19 = new DataColumn("Deposit", typeof(double));
            dtResult.Columns.Add(col19);
            if (StoreId == 11174)   
            {
                DataColumn col20 = new DataColumn("Pcat");
                dtResult.Columns.Add(col20);
            }
        }
        private void CreateFullNameStructure()
        {
            DataColumn col1 = new DataColumn("pname");
            dtfullname.Columns.Add(col1);
            DataColumn col2 = new DataColumn("pdesc");
            dtfullname.Columns.Add(col2);
            DataColumn col3 = new DataColumn("upc");
            dtfullname.Columns.Add(col3);
            DataColumn col4 = new DataColumn("sku");
            dtfullname.Columns.Add(col4);
            DataColumn col5 = new DataColumn("pack");
            dtfullname.Columns.Add(col5);
            DataColumn col6 = new DataColumn("price", typeof(double));
            dtfullname.Columns.Add(col6);
            DataColumn col7 = new DataColumn("uom");
            dtfullname.Columns.Add(col7);
            DataColumn col8 = new DataColumn("pcat");
            dtfullname.Columns.Add(col8);
            DataColumn col9 = new DataColumn("pcat1");
            dtfullname.Columns.Add(col9);
            DataColumn col10 = new DataColumn("pcat2");
            dtfullname.Columns.Add(col10);
            DataColumn col11 = new DataColumn("country");
            dtfullname.Columns.Add(col11);
            DataColumn col12 = new DataColumn("region");
            dtfullname.Columns.Add(col12);
        }
        private void Upload(string filename)
        {
            string ftpServerIP = settings.FtpServer;
            string ftpUserID = settings.FtpUserName;
            string ftpPassword = settings.FtpPassword;
            FileInfo fileInf = new FileInfo(filename);
            string uri = "ftp://" + ftpServerIP + "/Test/Upload/" + fileInf.Name;
            FtpWebRequest reqFTP;
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(
                      "ftp://" + ftpServerIP + "/" + settings.FtpUpFolder + "/" + fileInf.Name));
            reqFTP.Credentials = new NetworkCredential(ftpUserID,
                                                       ftpPassword);

            reqFTP.KeepAlive = false;

            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;

            reqFTP.UseBinary = true;

            reqFTP.ContentLength = fileInf.Length;

            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;

            FileStream fs = fileInf.OpenRead();
            try
            {
                Stream strm = reqFTP.GetRequestStream();

                contentLen = fs.Read(buff, 0, buffLength);

                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }
                strm.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Upload Error");
            }
        }

        //private void timer1_Tick(object sender, EventArgs e)
        //{
        //    timer1.Enabled = false;
        //    if (Argsprams != "")
        //    {
        //        btnUpload.PerformClick();
        //        Environment.Exit(Environment.ExitCode);
        //    }
        //}

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btndownload_Click(object sender, EventArgs e)// Download
        {
            DownloadLogic download = new DownloadLogic();
            download.DownloadAllXmlFilesFromFtp();
        }
    }
    public static class DataRowExtensions
    {
        public static string GetString(this DataRow r, string col) =>
            r.Table.Columns.Contains(col) && r[col] != DBNull.Value ? r[col].ToString().Trim() : "";

        public static int? GetInt(this DataRow r, string col)
        {
            var s = r.GetString(col);
            if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) return v;
            return null;
        }

        public static decimal? GetDecimal(this DataRow r, string col)
        {
            var s = r.GetString(col);
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) return v;
            return null;
        }
    }

}

