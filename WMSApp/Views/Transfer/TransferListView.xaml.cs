using WMSApp.ViewModels.Transfer;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Transfer
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TransferListView : ContentPage
    {
        TransferListVM ViewModel;
        public TransferListView(string transType)
        {
            InitializeComponent();
            ViewModel = new TransferListVM(Navigation);
            BindingContext = ViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();           
            ViewModel?.LoadTransferRequest();
        }
    }
}