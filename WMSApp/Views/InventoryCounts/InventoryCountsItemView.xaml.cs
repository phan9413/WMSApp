using WMSApp.Models.SAP;
using WMSApp.ViewModels.InventoryCounts;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.InventoryCounts
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InventoryCountsItemView : ContentPage
    {
        public InventoryCountsItemView(OINC_Ex selected)
        {
            InitializeComponent();
            BindingContext = new InventoryCountsItemVM(Navigation, selected);
        }
    }
}