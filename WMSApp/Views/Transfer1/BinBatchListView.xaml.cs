using WMSApp.ViewModels.Transfer1;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Transfer1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BinBatchListView : ContentPage
    {
        public BinBatchListView(string returnAddress, string itemCod, string itemNam, string WhsCod, decimal neededQty)
        {
            InitializeComponent();
            BindingContext = new BinBatchListVM(Navigation, returnAddress, itemCod, itemNam,  WhsCod, neededQty);
        }
    }
}