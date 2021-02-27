using WMSApp.ViewModel.Warehouse;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.View.Warehouse
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WarehouseListV : ContentPage
    {
        public WarehouseListV()
        {
            InitializeComponent();
            BindingContext = new WarehouseListVM(Navigation);
        }
    }
}