using System;
using WMSApp.Models.GRPO;

namespace WMSApp.Models.SAP
{
    public class zwaItemBin
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string ItemCode { get; set; }
        public decimal Quantity { get; set; }
        public string BinCode { get; set; }
        public int BinAbsEntry { get; set; }
        public string BatchNumber { get; set; }
        public string SerialNumber { get; set; }
        public string TransType { get; set; }
        public DateTime TransDateTime { get; set; }
        public string BatchAttr1 { get; set; }
        public string BatchAttr2 { get; set; }
        public DateTime BatchAdmissionDate { get; set; } = DateTime.Now;
        public DateTime BatchExpiredDate { get; set; } = DateTime.Now;

        public Guid LineGuid { get; set; }

    }
}
