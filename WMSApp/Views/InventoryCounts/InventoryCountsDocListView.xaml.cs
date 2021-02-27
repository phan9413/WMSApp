using WMSApp.ViewModels.InventoryCounts;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.InventoryCounts
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InventoryCountsDocListView : ContentPage
    {
        public InventoryCountsDocListView(string pageType)
        {
            InitializeComponent();
            BindingContext = new InventoryCountsDocListVM(Navigation, pageType);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ((InventoryCountsDocListVM)BindingContext).GetInvtryCountDoc();
        }
    }
}