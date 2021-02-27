using WMSApp.ViewModels.DO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.DO
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DOSelectionMenuView : ContentPage
    {
        public DOSelectionMenuView()
        {
            InitializeComponent();
            BindingContext = new DOSelectionMenuVM(Navigation);
        }
    }
}