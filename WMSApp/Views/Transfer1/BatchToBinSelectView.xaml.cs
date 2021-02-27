using WMSApp.ViewModels.Transfer1;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Transfer1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BatchToBinSelectView : ContentPage
    {
        public BatchToBinSelectView(INavigation navigation, 
            string whsCode, string retAddrs, string itemCode, string itemName, string batchNumber, decimal batchQty, int fromBinAbs)
        {
            InitializeComponent();
            BindingContext = new BatchToBinSelectVM(navigation, whsCode, retAddrs, itemCode, 
                itemName, batchNumber, batchQty, fromBinAbs);
        }
    }
}