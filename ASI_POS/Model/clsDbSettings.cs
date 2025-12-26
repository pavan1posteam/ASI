using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI_POS.Model
{
    class clsDbSettings
    {
        public string selectpath { set; get; }
        public string UpFolder { get; set; }
        public string DownFolder { get; set; }
        public string service_fee { get; set; }
        public string TaxCode { get; set; }
        public string shipCat { get; set; }
        public string tipCat { get; set; }
        public string discountCat { get; set; }
        public string visa { get; set; }
        public string amex { get; set; }
        public string mastercard { get; set; }
        public string discover { get; set; }
        public string generic { get; set; }
    }

    public class clscategory
    {
        public string catid { get; set; }
        public string catname { get; set; }
        public int taxlevel { get; set; }
    }
}
