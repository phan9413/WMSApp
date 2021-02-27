using WMSApp.ViewModels.Inventory;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.ItemMasterInfo
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InventoryView : ContentPage
    {
        public InventoryView()
        {
            InitializeComponent();
            BindingContext = new InventoryVM(Navigation);
        }
    }
}