using System;
using System.Collections.Generic;
using System.Text;

namespace WMSApp.Models.SAP
{
    public class zwaGRPO
    {        
        public Guid Guid { get; set; }
        public string ItemCode { get; set; }
        public decimal Qty { get; set; }
        public string SourceCardCode { get; set; }
        public int SourceDocNum { get; set; }
        public int SourceDocEntry { get; set; }
        public int SourceDocBaseType { get; set; }
        public int SourceBaseEntry { get; set; }
        public int SourceBaseLine { get; set; }
        public string Warehouse { get; set; } //add in 20200634T1834
        public string SourceDocType { get; set; }
        public Guid LineGuid { get; set; }
    }
}
