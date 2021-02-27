using WMSApp.ViewModels.Login;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace WMSApp.Views.Login
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChngWebApiConnView : ContentPage
    {
        public ChngWebApiConnView()
        {
            InitializeComponent();
            BindingContext = new ChngWebApiConnVM(Navigation);
        }
    }
}