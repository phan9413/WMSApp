using System.Collections.Generic;
using WMSApp.ClassObj.Item;
using WMSApp.ClassObj.Warehouse;

namespace WMSApp.ClassObj
{
    public class WarehouseDTO
    {
        public string Request { get; set; }
        public string SAPID { get; set; }
        public string SAPPassword { get; set; }
        public string Token { get; set; }
        public string SelectedWarehouse { get; set; }
        public string SelectedItem { get; set; }
        public List<OWHSObj> oWHSs { get; set; }
        public AvailableItemObj[] AvailableItemObjs { get; set; }
        public List<ItemObj> ItemWhsResult { get; set; }
    }
}
