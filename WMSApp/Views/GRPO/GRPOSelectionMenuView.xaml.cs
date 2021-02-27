using WMSApp.ViewModels.GRPO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.GRPO
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GRPOSelectionMenuView : ContentPage
    {
        public GRPOSelectionMenuView()
        {
            InitializeComponent();
            BindingContext = new GRPOSelectionMenuVM(Navigation);

            
        }
    }
}