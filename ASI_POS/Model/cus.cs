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
    }
    public class stk
    {
        public string SKU { get; set; }
        public int Store { get; set; }
        public decimal ACOST { get; set; }
        public decimal LCOST { get; set; }
    }
    public class txc
    {
        public string Code { get; set; }
        public int Level { get; set; }
        public string Descript { get; set; }
        public string Cat { get; set; }
        public decimal Rate { get; set; }
    }
}
