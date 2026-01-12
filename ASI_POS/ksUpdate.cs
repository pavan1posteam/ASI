using ASI_POS.Model;
using ASI_POS.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;
namespace ASI_POS
{
    public class ksUpdate
    {
        updateOrders updateOrders;
        clsSettings settings = new clsSettings();
        private object ToDbValue(string s) => string.IsNullOrWhiteSpace(s) ? "" : s.Trim();
        public ksUpdate() 
        {
            updateOrders updateOrder = new updateOrders();
            updateOrders = updateOrder;
        }
        public bool UpInv(int orderId, List<ordtable> orddetails, PmtTable pmt)
        {
            settings.LoadSettings();
            if (settings.Mobile_Cashier == "9999")
                return false;
            var orderedSkus = orddetails.Select(o => o.sku);
            List<inv> inventoryinfo = updateOrders.getInvInfo(orderedSkus);
            var stkinfo = updateOrders.getStkInfo(orderedSkus);
            var jnlsaleinfo = getJnlInfo(orderId);
            foreach (ordtable ord in orddetails)
            {
                int sku = Convert.ToInt32(ord.sku);
                if(inventoryinfo.Any(i => i.SKU.Equals(ord.sku)) && jnlsaleinfo.Any(i => i.sku.Equals(sku)))
                {
                    var invinfo = inventoryinfo.FirstOrDefault(i => i.SKU.Equals(ord.sku));
                    var jnlinfo = jnlsaleinfo.FirstOrDefault(i => i.sku.Equals(sku));
                    var stk = stkinfo.FirstOrDefault(i => i.SKU.Equals(sku));
                    bool flag = UpdateInv(orderId, invinfo, jnlinfo, ord.orddate);
                    int uqty = jnlinfo.qty * jnlinfo.pack;
                    if (flag)
                    {
                        bool iskit = false;
                        flag = upStk(stk, jnlinfo, ord, uqty, iskit, orderId);
                    }
                    if (flag)
                    {
                        var jnhBool = getJnhInfo(orderId).Any(i => i.taxcode.ToUpper().Equals("EXPCAT"));
                        if (jnhBool)
                            continue;
                        else
                        {
                            if(ord.offerprice > 0) 
                                ord.price = ord.offerprice;
                            InsertHSt(Convert.ToInt32(ord.sku), uqty, ord.price, jnlinfo.cost, jnlinfo.pack, jnlinfo.prclevel);
                            InsertHstsum(Convert.ToInt32(ord.sku), uqty, ord.price, jnlinfo.cost);
                        }
                    }
                }
                else
                {
                    SafeShowStatus($"Ordered Product SKU not Found in Inventory Table!!!", 2);
                    SafeShowStatus($"Skipping {ord.sku} -> {ord.descript}", 2);
                    continue;
                }
            }
            return true;
        }
        private bool upStk(stk stkinfo,jnl jnlinfo, ordtable ord, int uqty, bool iskit, int orderId)
        {
            settings.LoadSettings();
            using (var conn = new OleDbConnection(settings.ConnectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    var stk = LoadStockForUpdate(Convert.ToInt32(ord.sku), Convert.ToInt32(settings.webstore), conn, tx);
                    if (stk == null)
                    {
                        tx.Rollback();
                        return false;
                    }
                    if (!LockStkRow(stk.SKU, stk.Store, conn, tx))
                    {
                        tx.Rollback();
                        return false;
                    }
                    InsertSTKLOG(stk, Convert.ToInt32(ord.sku), orderId, uqty, iskit, conn, tx);
                    ApplyStockChange(stk, uqty, jnlinfo.rflag, jnlinfo.location, iskit);
                    UpdateStock(stk, conn, tx);
                    tx.Commit();
                }
            }
            return true;
        }
        public bool UpdateInv(int orderId, inv item, jnl jnlinfo, DateTime? saleDate)
        {
            if (item == null) return false;
            if (DateTime.Now > saleDate && (jnlinfo.rflag == 0 || jnlinfo.rflag == 8) && jnlinfo.qty > 0)
            {
                if (string.IsNullOrWhiteSpace(settings.serverpath))
                    throw new ArgumentException("dbfFolder required", nameof(settings.serverpath));

                string sql = @"UPDATE inv SET sdate = ? WHERE sku = ?";

                try
                {
                    using (var conn = new OleDbConnection(settings.ConnectionString))
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = sql;

                        cmd.Parameters.Add(new OleDbParameter("p1", saleDate));
                        cmd.Parameters.Add(new OleDbParameter("p2", Convert.ToInt32(item.SKU)));

                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
                catch (OleDbException ex)
                {
                    SafeShowStatus($"INV update failed for SKU {item.SKU}: {ex.Message}", 2);
                    return false;
                }
            }
            else
                return false;
        }
        private bool InsertSTKLOG(stk stkinfo,int sku,int saleNumber,int qty,bool isKit,OleDbConnection conn,OleDbTransaction tx)
        {
            string sql = @"INSERT INTO stklog (
            sku, store, floor, back, shipped, kits,
            amount, number, type, whochange, cdate,
            stat, mtd_units, weeks, sdate,
            mtd_dol, mtd_prof, ytd_units, ytd_dol, ytd_prof,
            acost, lcost, pvend, lvend, pdate,
            smin, sord, sweeks, freeze_w,
            shelf, rshelf, sloc, bloc,
            lstore, who, tstamp, inet,
            depos, skipstat, mincost
        )
        VALUES (
            ?,?,?,?,?, ?,?,?,?,?, ?,
            ?,?,?,?,?, ?,?,?,?,?, ?,
            ?,?,?,?,?, ?,?,?,
            ?,?,?,?,?, ?,?,?,
            ?,?
        )";

            using (var cmd = new OleDbCommand(sql, conn, tx))
            {
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = sku });//sku
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = settings.webstore });//store
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = stkinfo.Floor });//floor
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = stkinfo.Back });//back
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = stkinfo.Shipped });//shipped
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = stkinfo.Kits });//kits
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = -qty });//amount
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = saleNumber });//number
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = isKit ? "K" : "S" });//type
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "WEB" });//whochange
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = DateTime.Now });//cdate
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = stkinfo.Stat });//stat
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = stkinfo.MTD_UNITS });//mtd_units
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = stkinfo.Weeks });//weeks
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = stkinfo.SDate ?? DateTime.Now });//sdate
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = stkinfo.MTD_DOL });//mtd_dol
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = stkinfo.MTD_PROF });//mtd_prof
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = stkinfo.YTD_UNITS });//ytd_units
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = stkinfo.YTD_DOL });//ytd_dol
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = stkinfo.YTD_PROF });//ytd_prof
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = stkinfo.ACOST });//acost
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = stkinfo.LCOST });//lcost
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = stkinfo.PVEND ?? "" });//pvend
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = stkinfo.LVEND ?? "" });//lvend
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = stkinfo.PDate ?? DateTime.Now });//pdate
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = stkinfo.SMin });//smin
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = stkinfo.SOrd });//sord
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = stkinfo.SWeeks });//sweeks
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = stkinfo.Freeze_W });//freeze_w
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = stkinfo.Shelf });//shelf
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = stkinfo.RShelf });//rshelf
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//sloc
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//bloc
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = settings.webstore });//lstore
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "WEB" });//who
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.DBTimeStamp, Value = DateTime.Now });//tstamp
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = stkinfo.INET });//inet
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = stkinfo.Depos ?? "" });//depos
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false });//skipstat
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = stkinfo.MinCost });//mincost
                return cmd.ExecuteNonQuery() == 1;
            }
        }
        private stk LoadStockForUpdate(int sku,int store,OleDbConnection conn,OleDbTransaction tx)
        {
            string sql = @"SELECT * FROM stk WHERE sku = ? AND store = ?";

            using (var cmd = new OleDbCommand(sql, conn, tx))
            {
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = sku });
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = store });
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new stk
                    {
                        SKU = Convert.ToInt32(reader["sku"]),
                        Store = Convert.ToInt32(reader["store"]),
                        Floor = Convert.ToInt32(reader["floor"]),
                        Back = Convert.ToInt32(reader["back"]),
                        Shipped = Convert.ToInt32(reader["shipped"]),
                        Kits = Convert.ToInt32(reader["kits"]),
                        Stat = reader["stat"].ToString().Trim(),
                        ACOST = Convert.ToDecimal(reader["acost"]),
                        LCOST = Convert.ToDecimal(reader["lcost"]),
                        SDate = (DateTime?)reader["sdate"]
                    };
                }
            }
        }
        private void ApplyStockChange(stk s, int uqty, int rflag, string location, bool isKit)
        {
            if (rflag > 1 && rflag < 5)
                return;

            switch (rflag)
            {
                case 5: // ship
                    s.Shipped += uqty;
                    AdjustLocation(s, -uqty, location, isKit);
                    break;

                case 6: // unship
                    s.Shipped -= uqty;
                    AdjustLocation(s, uqty, location, isKit);
                    break;

                case 8: // ship + sell
                    s.Shipped -= uqty;
                    break;

                default: // normal sale / return
                    AdjustLocation(s, -uqty, location, isKit);
                    break;
            }

            NormalizeStock(s);
        }
        private void AdjustLocation(stk s, int delta, string location, bool isKit)
        {
            if (isKit)
                s.Kits += delta;
            else if (string.Equals(location, "F", StringComparison.OrdinalIgnoreCase))
                s.Floor += delta;
            else
                s.Back += delta;
        }
        private void NormalizeStock(stk s)
        {
            if (s.Shipped < 0) { s.Back += s.Shipped; s.Shipped = 0; }
            if (s.Floor < 0) { s.Back += s.Floor; s.Floor = 0; }
            if (s.Kits < 0) { s.Back += s.Kits; s.Kits = 0; }

            if (s.Stat == "9" && s.Back < 0)
                s.Back = 0;
        }
        private void UpdateStock(stk s,OleDbConnection conn,OleDbTransaction tx)
        {
            settings.LoadSettings();
            string sql = @"
        UPDATE stk SET
            floor   = ?,
            back    = ?,
            shipped = ?,
            kits    = ?,
            sdate   = ?,
            lstore  = ?,
            who     = ?,
            tstamp  = ?,
            sent    = ?
        WHERE sku = ? AND store = ?";

            using (var cmd = new OleDbCommand(sql, conn, tx))
            {
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = s.Floor });//floor
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = s.Back });//back
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = s.Shipped });//shipped
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = s.Kits });//kits
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = DateTime.Now });//sdate
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = Convert.ToInt32(settings.webstore) });//lstore
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "WEB" });//who
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.DBTimeStamp, Value = DateTime.Now });//tstamp
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false });//sent
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = s.SKU });//sku
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = Convert.ToInt32(settings.webstore) });//store

                cmd.ExecuteNonQuery();
            }   
        }
        public bool InsertHSt(int sku, int qty, decimal price, decimal cost,int pack, string prclevel)
        {
            int lvl1qty = 0, lvl2qty = 0, lvl3qty = 0, lvl4qty = 0;
            decimal lvl1price = 0, lvl2price = 0, lvl3price = 0, lvl4price = 0;
            decimal lvl1cost = 0, lvl2cost = 0, lvl3cost = 0, lvl4cost = 0;
            switch (prclevel.Trim())
            {
                case "2":
                    lvl2qty = qty; lvl2price = price; lvl2cost = cost;
                    break;
                case "3":
                    lvl3qty = qty; lvl3price = price; lvl3cost = cost;
                    break;
                case "4":
                    lvl4qty = qty; lvl4price = price; lvl4cost = cost;
                    break;
                default:
                    lvl1qty = qty; lvl1price = price; lvl1cost = cost;
                    break;
            }
            settings.LoadSettings();
            string sql = @"INSERT INTO Hst
            (sku, date, Edate, Qty,Price, Cost, promo, store, pack,who, tstamp, lvl1qty, lvl1price,lvl1cost,lvl2qty, lvl2price,lvl2cost,lvl3qty, lvl3price,lvl3cost,lvl4qty, lvl4price,lvl4cost, sent)
            VALUES(?, ?, ?, ?,?, ?, ?, ?, ?,?, ?, ?, ?,?, ?, ?, ?,?, ?, ?, ?,?, ?, ?)";
            try
            {
                using (var conn = new OleDbConnection(settings.ConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = sql;
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = sku });//sku
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = DateTime.Now });//date
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = DateTime.Now });//Edate
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = qty });//qty
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = price });//price
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = cost });//cost
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "WEB" });//promo
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = settings.webstore });//store
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = pack });//pack
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "web" });//who
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.DBTimeStamp, Value = DateTime.Now });//tstamp
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = lvl1qty });//lvl1qty
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = lvl1price });//lvl1price
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = lvl1cost });//lvl1cost
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = lvl2qty });//lvl2qty
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = lvl2price });//lvl2price
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = lvl2cost });//lvl2cost
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = lvl3qty });//lvl3qty
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = lvl3price });//lvl3price
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = lvl3cost });//lvl3cost
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = lvl4qty });//lvl4qty
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = lvl4price });//lvl4price
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = lvl4cost });//lvl4cost
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false });//sent

                    return cmd.ExecuteNonQuery() == 1;
                }
            }
            catch (Exception ex) 
            {
                SafeShowStatus($"Insert HST Table failed for SKU: {sku} -> {ex.Message} ");
            }
            
            return false;
        }
        public bool InsertHstsum(int sku, int qty, decimal price, decimal cost)
        {
            int janqty = 0, febqty = 0, marqty = 0, aprqty = 0, mayqty = 0, junqty = 0, julqty = 0, augqty = 0, sepqty = 0, octqty = 0, novqty = 0, decqty = 0;
            decimal janprice = 0, febprice = 0, marprice = 0, aprprice = 0, mayprice = 0, junprice = 0, julprice = 0, augprice = 0, sepprice = 0, octprice = 0, novprice = 0, decprice = 0;
            decimal jancost = 0, febcost = 0, marcost = 0, aprcost = 0, maycost = 0, juncost = 0, julcost = 0, augcost = 0, sepcost = 0, octcost = 0, novcost = 0, deccost = 0;
            string year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.Month.ToString();
            switch (month)
            {
                case "1": janqty = qty; janprice = price; jancost = cost; break;
                case "2": febqty = qty; febprice = price; febcost = cost; break;
                case "3": marqty = qty; marprice = price; marcost = cost; break;
                case "4": aprqty = qty; aprprice = price; aprcost = cost; break;
                case "5": mayqty = qty; mayprice = price; maycost = cost; break;
                case "6": junqty = qty; junprice = price; juncost = cost; break;
                case "7": julqty = qty; julprice = price; julcost = cost; break;
                case "8": augqty = qty; augprice = price; augcost = cost; break;
                case "9": sepqty = qty; sepprice = price; sepcost = cost; break;
                case "10": octqty = qty; octprice = price; octcost = cost; break;
                case "11": novqty = qty; novprice = price; novcost = cost; break;
                case "12": decqty = qty; decprice = price; deccost = cost; break;
            }
            settings.LoadSettings();
            string sql = @"INSERT INTO hstsum
            (sku, store, year, janqty, janprice, jancost,febqty, febprice, febcost,marqty, marprice, marcost,aprqty, aprprice, aprcost,mayqty, mayprice, maycost, junqty, junprice, juncost,julqty,julprice, julcost,augqty, augprice, augcost,sepqty, sepprice, sepcost,octqty, octprice, octcost,novqty,novprice, novcost,decqty,decprice, deccost, who, tstamp, sent)
            VALUES(?, ?, ?, ?,?, ?, ?, ?, ?,?, ?, ?, ?,?, ?, ?, ?,?, ?, ?, ?,?, ?, ?,?, ?, ?, ?,?, ?, ?, ?, ?,?, ?, ?,?,?,?,?,?,?)";
            try
            {
                using (var conn = new OleDbConnection(settings.ConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = sql;
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = sku });//sku
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = settings.webstore });//store
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = year });//year
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = janqty });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = janprice });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = jancost });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = febqty });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = febprice });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = febcost });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = marqty });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = marprice });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = marcost });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = aprqty });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = aprprice });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = aprcost });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = mayqty });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = mayprice });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = maycost });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = junqty });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = junprice });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = juncost });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = julqty });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = julprice });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = julcost });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = augqty });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = augprice });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = augcost });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = sepqty });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = sepprice });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = sepcost });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = octqty });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = octprice });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = octcost });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = novqty });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = novprice });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = novcost });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = decqty });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = decprice });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = deccost });
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "web" });//who
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.DBTimeStamp, Value = DateTime.Now });//tstamp
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false });//sent

                    return cmd.ExecuteNonQuery() == 1;
                }
            }
            catch (Exception ex)
            {
                SafeShowStatus($"Insert Hstsum Table failed for SKU: {sku} -> {ex.Message} ");
            }
            return false;
        }
        private bool LockStkRow(int sku, int store, OleDbConnection conn, OleDbTransaction tx)
        {
            using (var cmd = new OleDbCommand(
                "UPDATE stk SET sku = sku WHERE sku = ? AND store = ?", conn, tx))
            {
                cmd.Parameters.Add(new OleDbParameter { Value = sku });
                cmd.Parameters.Add(new OleDbParameter { Value = store });
                return cmd.ExecuteNonQuery() == 1;
            }
        }
        public List<jnl> getJnlInfo(int sale)
        {
            settings.LoadSettings();
            if (sale == 0)
                return new List<jnl>();
            if (string.IsNullOrWhiteSpace(settings.serverpath))
                throw new ArgumentException("dbfFolder required", nameof(settings.serverpath));

            if (!System.IO.Directory.Exists(settings.serverpath))
                throw new System.IO.DirectoryNotFoundException($"DBF folder not found: {settings.serverpath}");

            var jnlinfo = new List<jnl>();

            try
            {
                using (var conn = new OleDbConnection(settings.ConnectionString))
                {
                    conn.Open();
                    string sql = $"SELECT * FROM jnl WHERE sku > 0 and sale = ?";

                    using (var cmd = new OleDbCommand(sql, conn))
                    {
                        cmd.Parameters.Add(
                                new OleDbParameter { OleDbType = OleDbType.Integer, Value = sale }
                            );
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var c = new jnl();

                                string GetString(int idx1)
                                {
                                    if (idx1 < 0) return null;
                                    if (reader.IsDBNull(idx1)) return null;
                                    return reader.GetValue(idx1)?.ToString().Trim();
                                }

                                int GetOrdinalOrMinus(string name)
                                {
                                    try { return reader.GetOrdinal(name); }
                                    catch (IndexOutOfRangeException) { return -1; }
                                }
                                int idx;
                                idx = GetOrdinalOrMinus("store"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int wcid)) c.store = wcid; }
                                idx = GetOrdinalOrMinus("sale"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int wcid)) c.sale = wcid; }
                                idx = GetOrdinalOrMinus("line"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int wcid)) c.line = wcid; }
                                idx = GetOrdinalOrMinus("qty"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int wcid)) c.qty = wcid; }
                                idx = GetOrdinalOrMinus("pack"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int wcid)) c.pack = wcid; }
                                idx = GetOrdinalOrMinus("sku"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int wcid)) c.sku = wcid; }
                                idx = GetOrdinalOrMinus("descript"); if (idx >= 0) c.descript = GetString(idx);
                                idx = GetOrdinalOrMinus("price"); if (idx >= 0) { var v = GetString(idx); if (decimal.TryParse(v, out decimal wcid)) c.price = wcid; }
                                idx = GetOrdinalOrMinus("cost"); if (idx >= 0) { var v = GetString(idx); if (decimal.TryParse(v, out decimal wcid)) c.cost = wcid; }
                                idx = GetOrdinalOrMinus("discount"); if (idx >= 0) { var v = GetString(idx); if (decimal.TryParse(v, out decimal wcid)) c.discount = wcid; }
                                idx = GetOrdinalOrMinus("dclass"); if (idx >= 0) c.dclass = GetString(idx);
                                idx = GetOrdinalOrMinus("promo"); if (idx >= 0) c.promo = GetString(idx);
                                idx = GetOrdinalOrMinus("cat"); if (idx >= 0) c.cat = GetString(idx);
                                idx = GetOrdinalOrMinus("location"); if (idx >= 0) c.location = GetString(idx);
                                idx = GetOrdinalOrMinus("rflag"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int wcid)) c.rflag = wcid; }
                                idx = GetOrdinalOrMinus("upc"); if (idx >= 0) c.upc = GetString(idx);
                                idx = GetOrdinalOrMinus("boss"); if (idx >= 0) c.boss = GetString(idx);
                                idx = GetOrdinalOrMinus("memo"); if (idx >= 0) c.memo = GetString(idx);
                                idx = GetOrdinalOrMinus("date"); if (idx >= 0) { var v = GetString(idx); if (DateTime.TryParse(v, out DateTime wcid)) c.date = wcid; }
                                idx = GetOrdinalOrMinus("prclevel"); if (idx >= 0) c.prclevel = GetString(idx);
                                idx = GetOrdinalOrMinus("fspoints"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int wcid)) c.fspoints = wcid; }
                                idx = GetOrdinalOrMinus("rtnqty"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int wcid)) c.rtnqty = wcid; }
                                jnlinfo.Add(c);
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (OleDbException ex)
            {
                SafeShowStatus($"Failed: {ex.Message} ", 2);
            }
            catch (Exception ex)
            {
                SafeShowStatus($"Failed: {ex.Message} ", 2);
            }

            return jnlinfo;
        }
        public List<jnh> getJnhInfo(int sale)
        {
            settings.LoadSettings();
            if (sale == 0)
                return new List<jnh>();
            if (string.IsNullOrWhiteSpace(settings.serverpath))
                throw new ArgumentException("dbfFolder required", nameof(settings.serverpath));

            if (!System.IO.Directory.Exists(settings.serverpath))
                throw new System.IO.DirectoryNotFoundException($"DBF folder not found: {settings.serverpath}");

            var jnhinfo = new List<jnh>();

            try
            {
                using (var conn = new OleDbConnection(settings.ConnectionString))
                {
                    conn.Open();
                    string sql = $"SELECT * FROM jnh WHERE sale = ?";

                    using (var cmd = new OleDbCommand(sql, conn))
                    {
                        cmd.Parameters.Add(
                                new OleDbParameter { OleDbType = OleDbType.Integer, Value = sale }
                            );
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var c = new jnh();

                                string GetString(int idx1)
                                {
                                    if (idx1 < 0) return null;
                                    if (reader.IsDBNull(idx1)) return null;
                                    return reader.GetValue(idx1)?.ToString().Trim();
                                }

                                int GetOrdinalOrMinus(string name)
                                {
                                    try { return reader.GetOrdinal(name); }
                                    catch (IndexOutOfRangeException) { return -1; }
                                }
                                int idx;
                                idx = GetOrdinalOrMinus("date"); if (idx >= 0) { var v = GetString(idx); if (DateTime.TryParse(v, out DateTime wcid)) c.date = wcid; }
                                idx = GetOrdinalOrMinus("store"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int wcid)) c.store = wcid; }
                                idx = GetOrdinalOrMinus("register"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int wcid)) c.register = wcid; }
                                idx = GetOrdinalOrMinus("cashier"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int wcid)) c.cashier = wcid; }
                                idx = GetOrdinalOrMinus("sale"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int wcid)) c.sale = wcid; }
                                idx = GetOrdinalOrMinus("customer"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int wcid)) c.customer = wcid; }
                                idx = GetOrdinalOrMinus("order"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int wcid)) c.order = wcid; }
                                idx = GetOrdinalOrMinus("taxcode"); if (idx >= 0) c.taxcode = GetString(idx);
                                idx = GetOrdinalOrMinus("total"); if (idx >= 0) { var v = GetString(idx); if (decimal.TryParse(v, out decimal wcid)) c.total = wcid; }
                                idx = GetOrdinalOrMinus("receipts"); if (idx >= 0) { var v = GetString(idx); if (decimal.TryParse(v, out decimal wcid)) c.receipts = wcid; }
                                idx = GetOrdinalOrMinus("date"); if (idx >= 0) { var v = GetString(idx); if (DateTime.TryParse(v, out DateTime wcid)) c.date = wcid; }
                                idx = GetOrdinalOrMinus("memo"); if (idx >= 0) c.memo = GetString(idx);
                                idx = GetOrdinalOrMinus("signature"); if (idx >= 0) c.signature = GetString(idx);
                                idx = GetOrdinalOrMinus("reference"); if (idx >= 0) c.reference = GetString(idx);
                                idx = GetOrdinalOrMinus("ackrefno"); if (idx >= 0) c.ackrefno = GetString(idx);
                                idx = GetOrdinalOrMinus("voided"); if (idx >= 0) { var v = GetString(idx); if (bool.TryParse(v, out bool wcid)) c.voided = wcid; }
                                jnhinfo.Add(c);
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (OleDbException ex)
            {
                SafeShowStatus($"Failed: {ex.Message} ", 2);
            }
            catch (Exception ex)
            {
                SafeShowStatus($"Failed: {ex.Message} ", 2);
            }

            return jnhinfo;
        }
        private void SafeShowStatus(string msg, int c = 0)
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
    }
}
