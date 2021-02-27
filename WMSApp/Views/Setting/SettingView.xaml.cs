using WMSApp.ViewModels.Setting;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Setting
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingView : ContentPage
    {
        public SettingView()
        {
            InitializeComponent();
            BindingContext = new SettingVM(Navigation);
        }
    }
}