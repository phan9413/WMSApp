using WMSApp.ViewModels.Setting.AppGroup;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace WMSApp.Views.Setting.AppGroup
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppGroupUserSelectionListView : ContentPage
    {
        public AppGroupUserSelectionListView(AppGroupUsrListVM caller)
        {
            InitializeComponent();
            BindingContext = new AppGroupUserSelectionListVM(Navigation, caller);
        }
    }
}