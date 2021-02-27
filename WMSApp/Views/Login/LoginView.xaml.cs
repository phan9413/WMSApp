using WMSApp.ViewModels.Login;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace WMSApp.Views.Login
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginView : ContentPage
    {
        public LoginView()
        {
            InitializeComponent();
            BindingContext = new LoginVM(Navigation);
            MessagingCenter.Send<object>(_scanView, App._SNSendScannerCtrl);
        }

    }
}