using WMSApp.ViewModels.Transfer;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Transfer
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SimpleBinListView : ContentPage
    {
        public SimpleBinListView(string title, string whsCode, decimal fromBinNeedQty, int fromWhsAbs, string returnAddress)
        {
            InitializeComponent();
            BindingContext = new SimpleBinListVM(Navigation, title, whsCode, fromBinNeedQty, fromWhsAbs, returnAddress);
        }
    }
}