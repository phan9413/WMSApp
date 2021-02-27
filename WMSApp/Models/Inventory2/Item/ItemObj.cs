using Newtonsoft.Json;

namespace WMSApp.ClassObj.Item
{
    public class ItemObj
    {
        //public string ItemCode { get; set; }
        //public string ItemName { get; set; }
        //public string WhsCode { get; set; }
        //public string DistNumber { get; set; }
        //public string BinCode { get; set; }
        //public decimal OnHandQty { get; set; }
        //public decimal Quantity { get; set; }
        //public decimal CommitQty { get; set; }
        //public decimal CountQty { get; set; }
        //public decimal OnHand { get; set; }
        //public decimal IsCommited { get; set; }
        //public decimal OnOrder { get; set; }
        //public string request { get; set; }

        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string WhsCode { get; set; }
        public string DistNumber { get; set; }
        public string BinCode { get; set; }
        public decimal OnHandQty { get; set; }
        public decimal OrderQty { get; set; }
        public decimal CommitQty { get; set; }
        public decimal CountQty { get; set; }
        public string request { get; set; }

        [JsonIgnore]
        public decimal Available
        {
            get => OnHandQty + OrderQty - CommitQty;
        }
    }
}
