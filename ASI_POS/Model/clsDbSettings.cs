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

    }

    public class clscategory
    {
        public string catid { get; set; }
        public string catname { get; set; }
        public int taxlevel { get; set; }
    }
}
