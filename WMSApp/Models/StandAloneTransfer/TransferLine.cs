using DbClass;
using System;
using System.Collections.Generic;
using System.Text;
using WMSApp.Models.SAP;
using WMSApp.Models.Transfer1;

namespace WMSApp.Models.StandAloneTransfer
{
    public class TransferLine
    {
        public string ItemCode { get; set; }
        public string  ItemName { get; set; }
        public string  DistNumber { get; set; }
        public List<OBIN_Ex> Bins { get; set; } // reserved for later used
        public  string FromWhsCode { get; set; }
        public string ToWhsCode { get; set; }
        public decimal FromTransQty { get; set; }
        public decimal ToTransQty { get; set; }

        public OITM Item { get; set; }
        public List<TransferItemDetailBinM> Lines { get; set; }
    }
}
