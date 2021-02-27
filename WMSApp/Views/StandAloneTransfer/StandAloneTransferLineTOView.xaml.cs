using WMSApp.Class;
using WMSApp.ViewModels.StandAloneTransfer;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.StandAloneTransfer
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StandAloneTransferLineTOView : ContentPage
    {
        public StandAloneTransferLineTOView(zwaHoldRequest selected, string direction)
        {
            InitializeComponent();
            BindingContext = new StandAloneTransferLineTOVM(Navigation, selected, direction);
        }
    }
}