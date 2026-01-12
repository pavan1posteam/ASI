using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;
using System.Globalization;

namespace ASI_POS.Model
{
    public class cus
    {
        public int? Customer { get; set; }      
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int? Webcustid { get; set; }       
        public string Clubcard { get; set; }      
        public string Altid { get; set; }     
        public string Street1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Taxcode { get; set; }
        public string Store { get; set; }
        public string Lstore { get; set; }
        public string Status { get; set; }
        public DateTime? Startdate { get; set; }
        public bool? Fson { get; set; }          
    }
    public class inv
    {
        public string SKU { get; set; }
        public string NAME { get; set; }
        public string cat { get; set; }
        public int pack { get; set; }
        public decimal ACOST { get; set; }
        public decimal LCOST { get; set; }
        //public string Onsale { get; set; }
        public bool fson { get; set; }
        public int fsfactor { get; set; }
        public DateTime Sdate { get; set; }
    }
    public class stk
    {
        public int SKU { get; set; }
        public int Store { get; set; }
        public int Floor { get; set; }
        public int Back { get; set; }
        public int Shipped { get; set; }
        public int Kits { get; set; }
        public string Stat { get; set; }
        public int MTD_UNITS { get; set; }
        public decimal Weeks { get; set; }
        public DateTime? SDate { get; set; }
        public decimal MTD_DOL { get; set; }
        public decimal MTD_PROF { get; set; }
        public int YTD_UNITS { get; set; }
        public decimal YTD_DOL { get; set; }
        public decimal YTD_PROF { get; set; }
        public decimal ACOST { get; set; }
        public decimal LCOST { get; set; }
        public string PVEND { get; set; }
        public string LVEND { get; set; }
        public DateTime? PDate { get; set; }
        public int SMin { get; set; }
        public int SOrd { get; set; }
        public decimal SWeeks { get; set; }
        public bool Freeze_W { get; set; }
        public int Shelf { get; set; }
        public int RShelf { get; set; }
        public string SLoc { get; set; }
        public string BLoc { get; set; }
        public int LStore { get; set; }
        public string Who { get; set; }
        public DateTime? TStamp { get; set; }
        public decimal INET { get; set; }
        public string Depos { get; set; }
        public bool SkipStat { get; set; }
        public decimal MinCost { get; set; }
        public decimal Base { get; set; }
        public bool Sent { get; set; }
        public int Ware { get; set; }
        public string Vintage { get; set; }
        public bool GoSent { get; set; }
    }

    public class txc
    {
        public string Code { get; set; }
        public int Level { get; set; }
        public string Descript { get; set; }
        public string Cat { get; set; }
        public decimal Rate { get; set; }
    }
    public class jnl
    {
        public int store { get; set; }
        public int sale { get; set; }
        public int line { get; set; }
        public int qty { get; set; }
        public int pack { get; set; }
        public int sku { get; set; }
        public string descript { get; set; }
        public decimal price { get; set; }
        public decimal cost { get; set; }
        public decimal discount { get; set; }
        public string dclass { get; set; }
        public string promo { get; set; }
        public string cat { get; set; }
        public string location { get; set; }
        public int rflag { get; set; }
        public string upc { get; set; }
        public string boss { get; set; }
        public string memo { get; set; }
        public DateTime? date { get; set; }
        public string prclevel { get; set; }
        public int fspoints { get; set; }
        public int rtnqty { get; set; }
    }
    public class jnh
    {
        public DateTime? date { get; set; }
        public int store { get; set; }
        public int register { get; set; }
        public int cashier { get; set; }
        public int sale { get; set; }
        public int customer { get; set; }
        public int order { get; set; }
        public string taxcode { get; set; }
        public decimal total { get; set; }
        public decimal receipts { get; set; }
        public DateTime? tstamp { get; set; }
        public string memo { get; set; }
        public string signature { get; set; }
        public string reference { get; set; }
        public string ackrefno { get; set; }
        public bool voided { get; set; }
    }
}
