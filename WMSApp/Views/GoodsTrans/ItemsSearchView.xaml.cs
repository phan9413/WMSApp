using WMSApp.ViewModels.GIGR;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.GIGR
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemsSearchView : ContentPage
    {
        public ItemsSearchView()
        {
            InitializeComponent();
            BindingContext = new ItemsSearchVM(Navigation);
        }
    }
}