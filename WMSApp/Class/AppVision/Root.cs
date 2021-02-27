using System;
using System.Collections.Generic;
using System.Text;

namespace WMSApp.Class.AppVision
{
    public class Root
    {
        public string language { get; set; }
        public double textAngle { get; set; }
        public string orientation { get; set; }
        public List<Region> regions { get; set; }
    }
}
