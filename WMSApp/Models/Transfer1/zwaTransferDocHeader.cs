using System;
namespace WMSApp.Class
{
    public class zwaTransferDocHeader
    {
        public int Id { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime TaxDate { get; set; }
        public string FromWhsCode { get; set; }
        public string ToWhsCode { get; set; }
        public string JrnlMemo { get; set; }
        public string Comments { get; set; }
        public Guid Guid { get; set; }
        public string DocNumber { get; set; }
        public string DocStatus { get; set; }
        public string LastErrorMessage { get; set; }
    }
}
