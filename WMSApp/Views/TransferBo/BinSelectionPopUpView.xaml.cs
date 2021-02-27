using Rg.Plugins.Popup.Pages;
using Xamarin.Forms.Xaml;

namespace WMSApp.ViewModels.Share
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BinSelectionPopUpView : PopupPage
    {
        public BinSelectionPopUpView(string distNumber, string whsCode, string returnAddr)
        {
            InitializeComponent();
            BindingContext = new BinSelectionPopUpVM(Navigation, distNumber, whsCode, returnAddr);
        }
    }
}