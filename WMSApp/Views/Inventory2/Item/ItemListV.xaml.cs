using WMSApp.ViewModel.Item;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.View.Item
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemListV : ContentPage
    {
        public ItemListV()
        {
            InitializeComponent();
            BindingContext = new ItemListVM(Navigation);
        }
    }
}