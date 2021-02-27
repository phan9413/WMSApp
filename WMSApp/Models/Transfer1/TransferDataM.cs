using DbClass;

namespace WMSApp.Models.Transfer1
{
    public class TransferDataM
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemManagedBy { get; set; }
        public string FromWhsCode { get; set; }
        public string BinActivated { get; set; }
        public decimal RequestedQty { get; set; }

        // for item orig infor
        public OITM Item { get; set; }

        // for line guid
        public string LineGuid { get; set; }
        public string HeaderGuid {get; set;}
    }
}
