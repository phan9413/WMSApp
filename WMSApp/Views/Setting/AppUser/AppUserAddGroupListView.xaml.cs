using WMSApp.ViewModels.Setting.AppUser;
using WMSApp.ViewModels.Setting.AppUserGroup;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Setting.AppUserGroup
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppUserAddGroupListView : ContentPage
    {
        public AppUserAddGroupListView(AppUserAddVM source, string curUser,
            string curGroupName, string curCompanyName)
        {
            InitializeComponent();
            BindingContext = new AppUserAddGroupListVM(Navigation, source, curUser, curGroupName, curCompanyName);

        }
    }
}