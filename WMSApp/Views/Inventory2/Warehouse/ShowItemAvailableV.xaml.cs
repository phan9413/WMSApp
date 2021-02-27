using WMSApp.ClassObj.Warehouse;
using WMSApp.ViewModel.Warehouse;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.View.Warehouse
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShowItemAvailableV : ContentPage
    {
        public ShowItemAvailableV(OWHSObj SelectedWarehouse)
        {
            InitializeComponent();
            BindingContext = new ShowItemAvailableVM(Navigation, SelectedWarehouse);
        }
    }
}