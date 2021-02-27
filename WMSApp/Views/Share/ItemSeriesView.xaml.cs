using Rg.Plugins.Popup.Pages;
using WMSApp.ViewModels.Share;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Share
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemSeriesView : PopupPage
    {
        public ItemSeriesView(string returnAddr, string itemCode, string warehouse, decimal neededQty)
        {
            InitializeComponent();
            BindingContext = new ItemSeriesVM(Navigation, returnAddr, itemCode, warehouse, neededQty);
        }
    }
}