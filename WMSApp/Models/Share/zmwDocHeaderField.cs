using System;
using System.Collections.Generic;
using System.Text;

namespace WMSApp.Models.Share
{
    public class zmwDocHeaderField
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string DocSeries { get; set; }
        public int Series { get; set; }
        public string Ref2 { get; set; }
        public string Comments { get; set; }
        public string JrnlMemo { get; set; }
        public string NumAtCard { get; set; }
    }
}
