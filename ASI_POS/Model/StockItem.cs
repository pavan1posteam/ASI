using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI_POS.Model
{
    public class StockItem
    { 
        public int Sku { get; set; } 
        public int Store { get; set; }
        public decimal Floor { get; set; }
        public decimal Back { get; set; }
        public decimal Shipped { get; set; }
        public decimal Kits { get; set; }
    }
    public class InventoryItem 
    { 
        public int Sku { get; set; }
        public int Pack { get; set; }
        public decimal ACost { get; set; }
        public decimal LCost { get; set; }
        public int Stat { get; set; }
    }
    public class JournalLine 
    { 
        public int Sku { get; set; }
        public int Qty { get; set; }
        public int Pack { get; set; }
        public int RFlag { get; set; }
        public string Location { get; set; }
    }
}
