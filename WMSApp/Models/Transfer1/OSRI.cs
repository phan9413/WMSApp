using System;
using System.Collections.Generic;
using System.Text;

namespace WMSApp.Models.Transfer1
{
    public class OSRI
    {
        public string ItemCode { get; set; }
        public int SysSerial { get; set; }
        public string SuppSerial { get; set; }
        public string IntrSerial { get; set; }
        public string BatchId { get; set; }
        public DateTime ExpDate { get; set; }
        public DateTime PrdDate { get; set; }
        public DateTime InDate { get; set; }
        public DateTime GrntStart { get; set; }
        public DateTime GrntExp { get; set; }
        public string WhsCode { get; set; }
        public string Located { get; set; }
        public string Notes { get; set; }
        public decimal Quantity { get; set; }
        public int BaseType { get; set; }
        public int BaseEntry { get; set; }
        public int BaseNum { get; set; }
        public int BaseLinNum { get; set; }
        public DateTime CreateDate { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string ItemName { get; set; }
        public int Status { get; set; }
        public int Direction { get; set; }
        public string DataSource { get; set; }
        public short UserSign { get; set; }
        public string Transfered { get; set; }
        public short Instance { get; set; }
    }
}
