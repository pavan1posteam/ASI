using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI_POS.Model
{
    class AppSettings
    {
        public clsDbSettings Db { get; set; } = new clsDbSettings();
        public clsFtpSettings Ftp { get; set; } = new clsFtpSettings();
        public clsOthers Other { get; set; } = new clsOthers();
        public List<clsCategories> Categories { get; set; } = new List<clsCategories>();
    }
}
