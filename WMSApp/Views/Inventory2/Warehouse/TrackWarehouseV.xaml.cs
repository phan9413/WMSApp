using WMSApp.ClassObj.Warehouse;
using WMSApp.ViewModel.Warehouse;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.View.Warehouse
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TrackWarehouseV : ContentPage
    {
        public TrackWarehouseV(AvailableItemObj selectedItem,OWHSObj selectedWarehouse)
        {
            InitializeComponent();
            BindingContext = new TrackWarehouseVM(selectedItem,selectedWarehouse);
        }
    }
}