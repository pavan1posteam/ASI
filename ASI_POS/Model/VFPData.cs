using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ASI_POS.Model
{
    [XmlRoot("VFPData")]
    public class VFPData
    {
        [XmlElement("pmttable")]
        public List<PmtTable> PmtTables { get; set; } = null;
        [XmlElement("custable")]
        public List<custable> CusTables { get; set; } = null;
        [XmlElement("ordtable")]
        public List<ordtable> OrdTables { get; set; } = null;
    }
    public class custable
    {
        public string custid { get; set; }
        public string email { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string street { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string home { get; set; }
        public string taxcode { get; set; } = null;
        public string loyaltyno { get; set; } = null;
        public string gender { get; set; }
        public string dob { get; set; }
        public string AddressFirstName { get; set; }
        public string AddressLastName { get; set; }
        public string AddressContact { get; set; }
    }
    public class PmtTable
    {
        public string transid { get; set; }
        public string orderid { get; set; }
        public string custid { get; set; }
        public DateTime? orddate { get; set; }
        public decimal posamount { get; set; }
        public decimal discamount { get; set; }
        public decimal taxamount { get; set; }
        public decimal? taxamount2 { get; set; } = null;
        public decimal? taxamount3 { get; set; } = null;
        public decimal? taxamount4 { get; set; } = null;
        public decimal? taxamount5 { get; set; } = null;
        public decimal shipamount { get; set; }
        public decimal tipamount { get; set; }
        public decimal servicefeeamount { get; set; }
        public decimal totalamt { get; set; }
        public string shipto { get; set; }
        public string payby { get; set; }
        public string cardnumber { get; set; }
        public string expdate { get; set; } = null;
        public string taxcode { get; set; } = null;
        public string shipper { get; set; }
        public string loyaltyno { get; set; } = null;
        public decimal shippingcost { get; set; }
        public string zipcodeextension { get; set; } = null;
    }
    
    public class ordtable
    {
        public string orderid { get; set; }
        public string custid { get; set; }
        public string sku { get; set; }
        public DateTime? orddate { get; set; }
        public int qty { get; set; }
        public string descript { get; set; }
        public decimal price { get; set; }
        public decimal offerprice { get; set; }
        public string loyaltyno { get; set; } = null;
        public string emailid { get; set; }
        public string phoneno { get; set; }
        public int pack { get; set; }
        public decimal deposit { get; set; }
    }
}
