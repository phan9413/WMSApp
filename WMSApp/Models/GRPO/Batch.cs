using System;

namespace WMSApp.Models.GRPO
{
    public class Batch
    {
        public string DistNumber { get; set; }
        public string Attribute1 { get; set; }
        public string Attribute2 { get; set; }
        public DateTime Admissiondate { get; set; }
        public DateTime Expireddate { get; set; }
        public decimal Qty { get; set; }

        public bool IsShowOtherProperty { get; set; }

    }
}
