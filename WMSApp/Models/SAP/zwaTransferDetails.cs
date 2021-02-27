using System;
using System.Collections.Generic;
using System.Text;

namespace WMSApp.Models.SAP
{
    public class zwaTransferDetails
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string ItemCode { get; set; }
        public int FromBinAbs { get; set; }
        public int ToBinAbs { get; set; }
        public string FromWhsCode { get; set; }
        public string ToWhsCode { get; set; }
        public string BatchNo { get; set; }
        public string SerialNo { get; set; }
        public decimal TransferQty { get; set; }
        public DateTime TransDate { get; set; }

        // add on 
   
        public int SourceDocNum { get; set; }
        public int SourceDocEntry { get; set; }
        public int SourceDocBaseType { get; set; }
        public int SourceBaseEntry { get; set; }
        public int SourceBaseLine { get; set; }

        public Guid LineGuid { get; set; }
    }
}
