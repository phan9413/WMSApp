using DbClass;
using WMSApp.ViewModels.Share;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Share
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemWarehouseView : ContentPage
    {
        public ItemWarehouseView(OITM item, OWHS whs, string returnAddr, string pageTitle)
        {
            InitializeComponent();
            BindingContext = new ItemWarehouseVM(Navigation, item, whs, returnAddr);
            Title = pageTitle;
        }
    }
}