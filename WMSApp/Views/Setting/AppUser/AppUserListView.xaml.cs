using WMSApp.ViewModels.Setting.AppUser;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace WMSApp.Models.Setting.AppUser
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppUserListView : ContentPage
    {
        public AppUserListView()
        {
            InitializeComponent();
            BindingContext = new AppUserListVM(Navigation);
        }
    }
}