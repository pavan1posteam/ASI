using ASI_POS.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ASI_POS
{
    class updateOrders
    {
        clsSettings settings = new clsSettings();
        public bool updateorder(string ordFileName, string ordContent, string pmtFileName, string pmtContent)
        {
            settings.LoadSettings();
            XmlSerializer serializer = new XmlSerializer(typeof(VFPData));
            VFPData Ord_Data;
            VFPData Pmt_Data;
            try
            {
                using (StringReader sr = new StringReader(ordContent))
                {
                    Ord_Data = (VFPData)serializer.Deserialize(sr);
                }

                using (StringReader sr = new StringReader(pmtContent))
                {
                    Pmt_Data = (VFPData)serializer.Deserialize(sr);
                }
            }
            catch (Exception ex)
            {
                SafeShowStatus($"Deserialize Failed for Update Orders: {ex.Message}", 2);
                return false;
            }
            if (Ord_Data?.OrdTables == null || Ord_Data.OrdTables.Count <= 0)
                return false;
            if (Pmt_Data?.PmtTables == null || Pmt_Data.PmtTables.Count <= 0)
                return false;
            var orderLines = Ord_Data.OrdTables;
            var paymentRow = Pmt_Data.PmtTables[0];
            int orderId = GenerateAndValidateOrderId(orderLines);
            if (orderId <= 0)
                return false;
            cus cusinfo = ResolveCustomer(Ord_Data, Pmt_Data);
            if (cusinfo == null)
                return false;
            List<inv> inventoryinfo = getInvInfo();
            List<stk> stkinfo = getStkInfo();
            List<txc> txcinfo = getTxcInfo();
            txc taxtable = txcinfo.FirstOrDefault(i => i.Code.Equals(settings.TaxCode));
            if (orderId != -1)
            {
                updateOHDTable(orderId, cusinfo, paymentRow);
                updateJNHTable(orderId, cusinfo, paymentRow);
                InsertOddAndJnl_ftItems(orderId, cusinfo, paymentRow, orderLines, inventoryinfo, stkinfo, taxtable);

            }
            return false;
        }
        private bool updateOHDTable(int orderId, cus c, PmtTable pmt)
        {
            if (OrderExists(orderId))
                return false;
            settings.LoadSettings();
            string sql = @"INSERT INTO ohd
(order, store, customer, shipto,contact, phone, status, orddate, promdate,shipdate, invdate, agedate, whosold,whoorder, whoship, whoinvoice, terms, shipvia,taxcode, total, transact, printmemo,
 lreg, lstore, who, tstamp,shipmemo, memo, creditcard, expire, cvv,sent)
VALUES(?, ?, ?, ?,?, ?, ?, ?, ?,?, ?, ?, ?,?, ?, ?, ?, ?,?, ?, ?, ?,?, ?, ?, ?,?, ?, ?, ?, ?,?)";
            try
            {
                using (var conn = new OleDbConnection(settings.ConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = sql;
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = orderId });//order
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = Convert.ToInt32(settings.webstore)});//store
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = c.Customer});//customer
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = 0 });//shipto
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//contact
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = c.Phone ?? "" }); // phone
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = pmt.payby == "PAYATSTORE" ? "8": "5"});//Status
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = pmt.orddate });//orddate
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = DateTime.Now });//promdate
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = pmt.orddate });//shipdate 
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = pmt.orddate });//invdate
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = pmt.orddate }); // agedate
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" }); // whosold
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });// whoorder
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });// whoship
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" }); // whoinvoice
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" }); // terms
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" }); // shipvia
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = string.IsNullOrEmpty(pmt.taxcode) ? settings.TaxCode : pmt.taxcode }); // taxcode
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = pmt.totalamt }); // total
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = 0 });//transact
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" }); // printmemo
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = Convert.ToInt32(settings.Mobile_Register) });//lreg
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = c.Lstore }); // lstore
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "web" }); // who
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.DBTimeStamp, Value = DateTime.Now }); // tstamp
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = pmt.shipto }); // shipmemo
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" }); // memo
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = pmt.cardnumber });//creditcard
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" }); // expire
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" }); // cvv
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false }); // sent
                    return cmd.ExecuteNonQuery() == 1;
                }
            }
            catch (Exception ex)
            {
                SafeShowStatus($"Error while updating OHD table: {ex.Message}", 2);
                return false;
            }

        }
        private bool updateJNHTable(int orderId, cus c, PmtTable pmt)
        {
            if (OrderExists(orderId, "JNH"))
                return false;
            settings.LoadSettings();
            string jnhcomments = $"Shipping Address: {pmt.shipto}. PAID BY : {pmt.payby}";
            string sql = @"INSERT INTO JNH
            (date, store, register, cashier,sale, customer, order, taxcode, total,receipts, tstamp, memo, signature,reference, ackrefno, voided)
            VALUES(?, ?, ?, ?,?, ?, ?, ?, ?,?, ?, ?, ?,?, ?, ?)";
            try
            {
                using (var conn = new OleDbConnection(settings.ConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = sql;
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = DateTime.Now });//date
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = Convert.ToInt32(settings.webstore) });//store
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = Convert.ToInt32(settings.Mobile_Register) });//register
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = Convert.ToInt32(settings.Mobile_Cashier) });//cashier
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = orderId });//sale
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = c.Customer });//customer
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = orderId });//order
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = string.IsNullOrEmpty(pmt.taxcode) ? settings.TaxCode : pmt.taxcode }); // taxcode
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = pmt.totalamt }); // total
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = 0m });//receipts
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.DBTimeStamp, Value = DateTime.Now }); // tstamp
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = jnhcomments });// memo
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });// signature
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });// reference
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });// ackrefno
                    cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false }); // voided
                    return cmd.ExecuteNonQuery() == 1;
                }
            }
            catch (Exception ex)
            {
                SafeShowStatus($"Error while updating OHD table: {ex.Message}", 2);
                return false;
            }

        }
        private bool InsertOddAndJnl_ftItems(int orderId, cus c, PmtTable pmt, List<ordtable> orddetails, List<inv> inventoryinfo, List<stk> stkinfo, txc taxtable)
        {
            settings.LoadSettings();
            int storeId = Convert.ToInt32(settings.webstore);
            int registerId = Convert.ToInt32(settings.Mobile_Register);
            var findingofferprice = orddetails.FindAll(j=>j.offerprice>0);
            int sumqty = findingofferprice.Sum(i => i.qty);
            int allsumqty = orddetails.Sum(k => k.qty);
            decimal taxlvl1cost = 0;
            decimal tcost = 0;
            bool Discount = false;
            string cat = "";
            using (var conn = new OleDbConnection(settings.ConnectionString))
            {
                conn.Open();
                int line = 1;
                foreach(ordtable ord in orddetails)
                {
                    decimal price;
                    bool Isofferprod = false;
                    decimal dis = 0;
                    decimal prodCost = 0;
                    var stk = stkinfo.FirstOrDefault(t => !string.IsNullOrEmpty(t.SKU) && t.SKU.Trim() == ord.sku);
                    var itemInfo = inventoryinfo.FirstOrDefault(t => !string.IsNullOrEmpty(t.SKU) && t.SKU.Trim() == ord.sku);
                    if (stk != null)
                    {
                        prodCost = stk.ACOST > 0 ? stk.ACOST / itemInfo.pack : stk.LCOST / itemInfo.pack;
                        cat = itemInfo != null ? itemInfo.cat : "";
                    } 
                    else if (itemInfo != null)
                    {
                        prodCost = itemInfo.ACOST > 0 ? itemInfo.ACOST / itemInfo.pack : itemInfo.LCOST / itemInfo.pack;
                        cat = itemInfo.cat;
                    }
                        
                    prodCost = Math.Round(prodCost * (ord.qty * ord.pack), 2, MidpointRounding.AwayFromZero);
                    if (ord.offerprice > 0)
                    {
                        price = ord.qty * ord.offerprice;
                        Isofferprod = true;
                        if (Isofferprod)
                        {
                            if (ord.offerprice == 0 && sumqty > 0)
                            {
                                dis = (pmt.discamount / sumqty) * ord.qty;
                                Discount = true;
                            }  
                            else
                                dis = 0;
                        }
                        else
                        {
                            dis = ((ord.price * ord.qty) / 100) * ((pmt.discamount / pmt.posamount) * 100);
                            Discount = true;
                        }
                    }
                    else
                        price = ord.qty * ord.price;
                    if (ord.deposit > 0)
                        tcost += (ord.price * ord.qty) + Math.Round(ord.deposit * ord.qty, 2);
                    else
                        tcost += (ord.price * ord.qty);
                    taxlvl1cost = taxlvl1cost + (price * ord.qty);
                    using (var tx = conn.BeginTransaction())
                    {
                        try
                        {
                            /* INSERT INTO ODD  */
                            using (var cmdOdd = conn.CreateCommand())
                            {
                                cmdOdd.Transaction = tx;

                                cmdOdd.CommandText = @"INSERT INTO odd (order, store, line, sku, status, customer, orddate, agedate, qty, pack, descript, price, cost, discount, promo, dflag,
                         dclass, damount, taxlevel, surcharge, cat, location, dml, cpack, onsale,freeze, memo, bqty, extax, prclevel)
                        VALUES (?, ?, ?, ?, ?, ?,?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = orderId });//order
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = storeId });//store
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = line });          // line
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = ord.sku });          // sku
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = pmt.payby == "PAYATSTORE" ? "8" : "5" });        // status
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = c.Customer });//customer
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = ord.orddate });//orddate
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = DateTime.Now });//agedate
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = ord.qty });          // qty
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = ord.pack });          // pack
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = ord.descript });//descript
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = price });//price
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = prodCost });//cost
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = dis });//discount
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//promo
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = 0 });//dflag
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//dclass
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = 0m });//damount
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = 1 });//taxlevel
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false });//surcharge
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = cat });//cat
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//location
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = 0 });//dml
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = 0 });//cpack
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false });//onsale
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false });//freeze
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//memo
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = 0 });//bqtu
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = 0m });//extax
                                cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//prclevel
                                cmdOdd.ExecuteNonQuery();
                            }

                            /* INSERT INTO JNL  */
                            using (var cmdJnl = conn.CreateCommand())
                            {
                                cmdJnl.Transaction = tx;

                                cmdJnl.CommandText = @"INSERT INTO jnl (store, sale, line, qty, pack, sku, descript, price, cost, discount, dclass, promo, cat,
                         location, rflag, upc, boss, memo, date, prclevel, fspoints, rtnqty) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ? , ?, ?, ?, ?, ?)";

                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = storeId });//store
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = orderId });//sale
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = line });//line
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = ord.qty }); // qty
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = ord.pack });//pack
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = Convert.ToInt32(ord.sku) });//sku
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = ord.descript });//descript
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = price });//price
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = prodCost });//cost
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = dis });//discount
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//dclass
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//promo
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = cat });//cat
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//location
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = 0m });//rflag
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//upc
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//boss
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//memo
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = DateTime.Now });//date
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//prclevel
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = 0 });//fspoints
                                cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = 0 });//rtnqty
                                cmdJnl.ExecuteNonQuery();
                            }

                            tx.Commit();
                        }
                        catch (Exception ex)
                        {
                            tx.Rollback();
                            SafeShowStatus("ODD/JNL insert failed: " + ex.Message, 2);
                        }
                    }
                    line++;
                    if (ord.deposit > 0)
                    {
                        line = updateAnything(orderId, c, pmt, ord, line, "CONTAINER DEPOSITS", Math.Round(ord.deposit * ord.qty, 2),"90");
                    }
                }
                line = updateAnything(orderId, c, pmt, orddetails[0], 900, $"{allsumqty} ITEMS SUBTOTAL", tcost, "", true);
                line = updateAnything(orderId, c, pmt, orddetails[0], line, "SHIPPING AMOUNT", pmt.shipamount, settings.ShippingCat);//195
                line = updateAnything(orderId, c, pmt, orddetails[0], line, "SERVICE FEE AMOUNT", pmt.servicefeeamount, settings.ServiceFee);
                line = updateAnything(orderId, c, pmt, orddetails[0], line, "TIP AMOUNT", pmt.tipamount, settings.TipCat);//197
                if(Discount && pmt.discamount>0)
                    line = updateAnything(orderId, c, pmt, orddetails[0], line, "DISCOUNT", -pmt.discamount, settings.DiscountCat, true);//63
                if (pmt.taxamount > 0)
                {
                    updateAnything(orderId, c, pmt, orddetails[0], 941, $"{taxtable.Descript} TAX ON {taxlvl1cost}", pmt.taxamount, taxtable.Cat, true);
                }
                //if(paymentRow.taxamount2 != null || paymentRow.taxamount2 > 0)
                //{

                //}
                //if (paymentRow.taxamount3 != null || paymentRow.taxamount3 > 0)
                //{

                //}
                //if (paymentRow.taxamount4 != null || paymentRow.taxamount4 > 0)
                //{

                //}
                //if (paymentRow.taxamount5 != null || paymentRow.taxamount5 > 0)
                //{

                //}
                updateAnything(orderId, c, pmt, orddetails[0], 950, $"ID # {settings.Mobile_Cashier} TOTAL", pmt.totalamt, "", true);
                string paybycat = "";
                if (Regex.IsMatch(pmt.payby, @"^V", RegexOptions.IgnoreCase))
                    paybycat = settings.visa;
                else if (Regex.IsMatch(pmt.payby, @"^A", RegexOptions.IgnoreCase))
                    paybycat = settings.amex;
                else if (Regex.IsMatch(pmt.payby, @"^M", RegexOptions.IgnoreCase))
                    paybycat = settings.mastercard;
                else if (Regex.IsMatch(pmt.payby, @"^D", RegexOptions.IgnoreCase))
                    paybycat = settings.discover;
                else 
                    paybycat = settings.generic;
                updateAnything(orderId, c, pmt, orddetails[0], 980, $"{pmt.payby}", pmt.totalamt, paybycat, true);//CardType
                updateAnything(orderId, c, pmt, orddetails[0], 1021, $"CARD # {pmt.cardnumber}", 0, "", true);
                string shipto = (pmt.shipto ?? "").Trim().Split(';')[0].ToUpper();
                updateAnything(orderId, c, pmt, orddetails[0], 1022, $"{shipto}", 0, "", true);
            }
            return false;
        }
        public int updateAnything(int orderId, cus c, PmtTable pmt, ordtable ord, int line = 1, string Descript = "", decimal cost = 0, string cat = "", bool qtypack = false)
        {
            settings.LoadSettings();
            int storeId = Convert.ToInt32(settings.webstore);
            int qty = 1;
            int pack = 1;
            if (qtypack)
            {
                qty = 0;
                pack = 0;
            }
            using (var conn = new OleDbConnection(settings.ConnectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        /* INSERT INTO ODD  */
                        using (var cmdOdd = conn.CreateCommand())
                        {
                            cmdOdd.Transaction = tx;

                            cmdOdd.CommandText = @"INSERT INTO odd (order, store, line, sku, status, customer, orddate, agedate, qty, pack, descript, price, cost, discount, promo, dflag,
                         dclass, damount, taxlevel, surcharge, cat, location, dml, cpack, onsale,freeze, memo, bqty, extax, prclevel)
                        VALUES (?, ?, ?, ?, ?, ?,?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = orderId });//order
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = storeId });//store
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = line });          // line
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = 0 });          // sku
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = pmt.payby == "PAYATSTORE" ? "8" : "5" });        // status
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = c.Customer });//customer
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = ord.orddate });//orddate
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = DateTime.Now });//agedate
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = qty });          // qty
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = pack });          // pack
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = Descript });//descript
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = cost });//price
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = 0 });//cost
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = 0 });//discount
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//promo
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = 0 });//dflag
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//dclass
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = 0m });//damount
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = 0 });//taxlevel
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false });//surcharge
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = cat });//cat
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//location
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = 0 });//dml
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = 0 });//cpack
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false });//onsale
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Boolean, Value = false });//freeze
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//memo
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = 0 });//bqtu
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = 0m });//extax
                            cmdOdd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//prclevel
                            cmdOdd.ExecuteNonQuery();
                        }

                        /* INSERT INTO JNL  */
                        using (var cmdJnl = conn.CreateCommand())
                        {
                            cmdJnl.Transaction = tx;

                            cmdJnl.CommandText = @"INSERT INTO jnl (store, sale, line, qty, pack, sku, descript, price, cost, discount, dclass, promo, cat,
                         location, rflag, upc, boss, memo, date, prclevel, fspoints, rtnqty) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ? , ?, ?, ?, ?, ?)";

                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = storeId });//store
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = orderId });//sale
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = line });//line
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = qty }); // qty
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = pack });//pack
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = 0 });//sku
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = Descript });//descript
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = cost });//price
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = 0 });//cost
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = 0 });//discount
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//dclass
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//promo
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = cat });//cat
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//location
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = 0m });//rflag
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//upc
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//boss
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//memo
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = DateTime.Now });//date
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = "" });//prclevel
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Numeric, Value = 0 });//fspoints
                            cmdJnl.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = 0 });//rtnqty
                            cmdJnl.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        SafeShowStatus($"ODD/JNL insert failed for {Descript}: " + ex.Message, 2);
                    }
                }
            }
            return ++line;
        }
        private void UpdateInventory(List<ordtable> orderLines)
        {
            using (var conn = new OleDbConnection(settings.ConnectionString))
            {
                conn.Open();
                foreach (var line in orderLines)
                {
                    int deductQty = line.qty * (line.pack <= 0 ? 1 : line.pack);
                    using (var cmd = conn.CreateCommand())
                    {
                        try
                        {
                            cmd.CommandText = @" UPDATE stk SET Back = Back - ? WHERE ALLTRIM(sku) == ?";
                            cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = deductQty });
                            cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarChar, Value = line.sku.Trim() });
                            int rows = cmd.ExecuteNonQuery();
                            if (rows == 0)
                            {
                                SafeShowStatus($"WARNING: SKU not found in inventory: {line.sku}", 2);
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
        }
        private int GenerateAndValidateOrderId(List<ordtable> ordTables)
        {
            if (ordTables == null || ordTables.Count == 0)
                throw new Exception("ORD XML contains no orders");

            int orderId = (Convert.ToInt32(settings.Mobile_Register) * 1_000_000) + int.Parse(ordTables[0].orderid);

            if (OrderExists(orderId))
            {
                SafeShowStatus($"Order {orderId} already exists. Skipping.", 3);
                return -1; 
            }
            return orderId;
        }
        private bool OrderExists(int orderId, string tablename = "ohd")
        {
            using (var conn = new OleDbConnection(settings.ConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $@"SELECT COUNT(*) FROM {tablename} WHERE order = ?";
                cmd.Parameters.Add(new OleDbParameter
                {
                    OleDbType = OleDbType.Integer,
                    Value = orderId
                });
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
        private cus ResolveCustomer(VFPData ord, VFPData pmt)
        {
            if (ord == null || pmt == null)
                return null;

            var customers = new Customers().getCustomersInfo();
            if (customers == null || customers.Count == 0)
                return null;
            if (int.TryParse(pmt.PmtTables[0]?.custid, out int webCustId) && webCustId > 0)
            {
                var match = customers.FirstOrDefault(c => c.Webcustid == webCustId);
                if (match != null)
                    return match;
            }
            var loyalty = ord.OrdTables[0]?.loyaltyno?.Trim();
            if (!string.IsNullOrEmpty(loyalty))
            {
                var match = customers.FirstOrDefault(c => c.Clubcard == loyalty);
                if (match != null)
                    return match;
            }
            var email = ord.OrdTables[0]?.emailid?.Trim();
            if (!string.IsNullOrEmpty(email))
            {
                var match = customers.FirstOrDefault(c => string.Equals(c.Email, email, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                    return match;
            }
            var phone = ord.OrdTables[0]?.phoneno;
            if (!string.IsNullOrWhiteSpace(phone))
            {
                phone = new Customers().NormalizePhone(phone);
                var match = customers.FirstOrDefault(c => new Customers().NormalizePhone(c.Phone) == phone);
                if (match != null)
                    return match;
            }
            ordtable newcusinfo = new ordtable
            {
                emailid = ord.OrdTables[0].emailid,
                phoneno = ord.OrdTables[0].phoneno
            };
            return CreateCustomerFromOrder(newcusinfo, webCustId);
        }
        private cus CreateCustomerFromOrder(ordtable ord, int webCustId)
        {
            var newcusinfo = new cus
            {
                Firstname = "",
                Lastname = "",
                Email = ord.emailid?.Trim() ?? "",
                Phone = new Customers().NormalizePhone(ord.phoneno),
                Webcustid = webCustId,
                Store = settings.webstore,
                Status = "WEB",
                Taxcode = settings.TaxCode,
                Fson = true,
                Startdate = DateTime.Now,
                Lstore = settings.webstore
            };

            bool ok = new Customers().InsertCustomer(newcusinfo);
            if (!ok)
                throw new Exception("Failed to create customer from order.");

            return newcusinfo;
        }
        public List<inv> getInvInfo()
        {
            settings.LoadSettings();
            if (string.IsNullOrWhiteSpace(settings.serverpath))
                throw new ArgumentException("dbfFolder required", nameof(settings.serverpath));

            if (!System.IO.Directory.Exists(settings.serverpath))
                throw new System.IO.DirectoryNotFoundException($"DBF folder not found: {settings.serverpath}");

            var inventory = new List<inv>();

            try
            {
                using (var conn = new OleDbConnection(settings.ConnectionString))
                {
                    conn.Open();
                    string sql = "SELECT * FROM inv";

                    using (var cmd = new OleDbCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var c = new inv();

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
                            idx = GetOrdinalOrMinus("SKU"); if (idx >= 0) c.SKU = GetString(idx);
                            idx = GetOrdinalOrMinus("NAME"); if (idx >= 0) c.NAME = GetString(idx);
                            idx = GetOrdinalOrMinus("cat"); if (idx >= 0) c.cat = GetString(idx);
                            idx = GetOrdinalOrMinus("Onsale"); if (idx >= 0) c.Onsale = GetString(idx);
                            idx = GetOrdinalOrMinus("pack"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int wcid)) c.pack = wcid; }
                            idx = GetOrdinalOrMinus("LCOST"); if (idx >= 0) { var v = GetString(idx); if (decimal.TryParse(v, out decimal wcid)) c.LCOST = wcid; }
                            idx = GetOrdinalOrMinus("ACOST"); if (idx >= 0) { var v = GetString(idx); if (decimal.TryParse(v, out decimal wcid)) c.ACOST = wcid; }
                            idx = GetOrdinalOrMinus("fsfactor"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int wcid)) c.fsfactor = wcid; }
                            idx = GetOrdinalOrMinus("fson"); if (idx >= 0) { var v = GetString(idx); if (bool.TryParse(v, out bool wcid)) c.fson = wcid; }
                            inventory.Add(c);
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

            return inventory;
        }
        public List<stk> getStkInfo()
        {
            settings.LoadSettings();
            if (string.IsNullOrWhiteSpace(settings.serverpath))
                throw new ArgumentException("dbfFolder required", nameof(settings.serverpath));

            if (!System.IO.Directory.Exists(settings.serverpath))
                throw new System.IO.DirectoryNotFoundException($"DBF folder not found: {settings.serverpath}");

            var inventory = new List<stk>();

            try
            {
                using (var conn = new OleDbConnection(settings.ConnectionString))
                {
                    conn.Open();
                    string sql = "SELECT * FROM stk";

                    using (var cmd = new OleDbCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var c = new stk();

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
                            idx = GetOrdinalOrMinus("SKU"); if (idx >= 0) c.SKU = GetString(idx);
                            idx = GetOrdinalOrMinus("Store"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int wcid)) c.Store = wcid; }
                            idx = GetOrdinalOrMinus("ACOST"); if (idx >= 0) { var v = GetString(idx); if (decimal.TryParse(v, out decimal wcid)) c.ACOST = wcid; }
                            idx = GetOrdinalOrMinus("LCOST"); if (idx >= 0) { var v = GetString(idx); if (decimal.TryParse(v, out decimal wcid)) c.LCOST = wcid; }
                            inventory.Add(c);
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

            return inventory;
        }
        public List<txc> getTxcInfo()
        {
            settings.LoadSettings();
            if (string.IsNullOrWhiteSpace(settings.serverpath))
                throw new ArgumentException("dbfFolder required", nameof(settings.serverpath));

            if (!System.IO.Directory.Exists(settings.serverpath))
                throw new System.IO.DirectoryNotFoundException($"DBF folder not found: {settings.serverpath}");
            var inventory = new List<txc>();
            try
            {
                using (var conn = new OleDbConnection(settings.ConnectionString))
                {
                    conn.Open();
                    string sql = "SELECT * FROM txc";

                    using (var cmd = new OleDbCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var c = new txc();
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
                            idx = GetOrdinalOrMinus("Code"); if (idx >= 0) c.Code = GetString(idx);
                            idx = GetOrdinalOrMinus("Descript"); if (idx >= 0) c.Descript = GetString(idx);
                            idx = GetOrdinalOrMinus("Cat"); if (idx >= 0) c.Cat = GetString(idx);
                            idx = GetOrdinalOrMinus("Level"); if (idx >= 0) { var v = GetString(idx); if (int.TryParse(v, out int wcid)) c.Level = wcid; }
                            idx = GetOrdinalOrMinus("Rate"); if (idx >= 0) { var v = GetString(idx); if (decimal.TryParse(v, out decimal wcid)) c.Rate = wcid; }
                            inventory.Add(c);
                        }
                    }
                    conn.Close();
                }
            }
            catch (OleDbException ex)
            {
                SafeShowStatus($"Failed: {ex.Message} ",2);
            }
            catch (Exception ex)
            {
                SafeShowStatus($"Failed: {ex.Message} ",2);
            }

            return inventory;
        }
        private void SafeShowStatus(string msg, int c=0)
        {
            if (Form1.Instance != null)
            {
                try { Form1.Instance.ShowStatus(msg, c); } catch { }
            }
            else
            {
                try { File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download.log"), DateTime.Now + " " + msg + Environment.NewLine); } catch { }
            }
        }
    }
}
