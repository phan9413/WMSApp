using System;
using System.Collections.Generic;
using System.Text;

namespace WMSApp.Models.SAP
{
    public class zwaInventoryRequestHead
    {
        public int Id { get; set; }
        public string FromWarehouse { get; set; }
        public string ToWarehouse { get; set; }
        public Guid Guid { get; set; }
        public DateTime TransDate { get; set; }
        public string DocNumber { get; set; }
        public string Remarks { get; set; }
    }
}
