using WMSApp.ViewModels.SimplifiedMain;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.SimplifiedMain
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SimplifyMainMenuView : ContentPage
    {
        public SimplifyMainMenuView()
        {
            InitializeComponent();
            BindingContext = new SimplifyMainMenuVM(Navigation, this);
        }
    }
}