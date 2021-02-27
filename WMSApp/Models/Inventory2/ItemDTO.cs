using System;
using System.Collections.Generic;
using System.Text;
using WMSApp.ClassObj.Item;

namespace WMSApp.ClassObj
{
    class ItemDTO
    {
        public string Request { get; set; }
        public string SAPID { get; set; }
        public string SAPPassword { get; set; }
        public string Token { get; set; }
        public string SelectedItem { get; set; }
        public List<OITMObj> oITMs { get; set; }
        public ItemObj[] ItemWhsResult { get; set; }
    }
}
