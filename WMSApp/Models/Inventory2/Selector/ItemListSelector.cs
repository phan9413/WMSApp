using WMSApp.ClassObj.Item;
using Xamarin.Forms;

namespace Selector
{
    public class ItemListSelector : DataTemplateSelector
    {
        public DataTemplate QueryNonManageItem { get; set; }
        public DataTemplate QueryNonManageItemWithBin { get; set; }
        public DataTemplate QuerySerialItemWithoutBin { get; set; }
        public DataTemplate QuerySerialItemWithBin { get; set; }
        public DataTemplate QueryBatchItemWithoutBin { get; set; }
        public DataTemplate QueryBatchItemWithBin { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            switch (((ItemObj)item).request)
            {
                case "QueryNonManageItem":
                    return QueryNonManageItem;
                case "QueryNonManageItemWithBin":
                    return QueryNonManageItemWithBin;
                case "QuerySerialItemWithoutBin":
                    return QuerySerialItemWithoutBin;
                case "QuerySerialItemWithBin":
                    return QuerySerialItemWithBin;
                case "QueryBatchItemWithoutBin":
                    return QueryBatchItemWithoutBin;
                case "QueryBatchItemWithBin":
                    return QueryBatchItemWithBin;
            }
            return null;

        }
    }
}
