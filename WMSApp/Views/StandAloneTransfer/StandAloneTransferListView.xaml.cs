using WMSApp.ViewModels.StandAloneTransfer;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.StandAloneTransfer
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StandAloneTransferListView : ContentPage
    {
        public StandAloneTransferListView()
        {
            InitializeComponent();
            BindingContext = new StandAloneTransferListVM(Navigation);
        }
    }
}