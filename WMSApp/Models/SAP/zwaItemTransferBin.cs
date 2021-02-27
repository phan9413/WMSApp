using System;
using System.Collections.Generic;
using System.Text;

namespace WMSApp.Models.SAP
{
    public class zwaItemTransferBin
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string ItemCode { get; set; }
        public decimal Quantity { get; set; }
        public string BinCode { get; set; }
        public int FromBinAbsEntry { get; set; }
        public int ToBinAbsEntry { get; set; }
        public string BatchNumber { get; set; }
        public string SerialNumber { get; set; }
        public string TransType { get; set; }
        public DateTime TransDateTime { get; set; }
        public Guid LineGuid { get; set; }

    }
}
