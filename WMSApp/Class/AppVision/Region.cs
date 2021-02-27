using System;
using System.Collections.Generic;
using System.Text;

namespace WMSApp.Class.AppVision
{
    public class Region
    {
        public string boundingBox { get; set; }
        public List<Line> lines { get; set; }
    }
}
