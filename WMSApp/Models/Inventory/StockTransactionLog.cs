using System;
using System.Collections.Generic;
using System.Text;

namespace WMSApp.Models.Inventory
{
    public class StockTransactionLog
    {
        public long Seq { get; set; }
        public string DocName { get; set; }
        public int Age { get; set; }
        public string DocNum { get; set; }
        public DateTime DocDate { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemGroup { get; set; }
        public string Warehouse { get; set; }
        public decimal Quantity { get; set; }
        public string BinCode { get; set; }
        public decimal BinQty { get; set; }
        public DateTime CreateDate { get; set; }
        public short DocTime { get; set; }
        public string User { get; set; }
    }
}
