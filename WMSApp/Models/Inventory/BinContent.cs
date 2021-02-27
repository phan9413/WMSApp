using System;
using System.Collections.Generic;
using System.Text;

namespace WMSApp.Models.Inventory
{
    public class BinContent
    {
        public long Seq { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string BinCode { get; set; }
        public decimal Qty { get; set; }
        public string Freezed { get; set; }
    }
}
