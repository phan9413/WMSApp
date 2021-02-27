using WMSApp.Models.Login;
using WMSApp.ViewModels.Setting.AppUser;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Setting.AppUser
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppUserAddView : ContentPage
    {
        // Page constructor for existing user
        public AppUserAddView(AppUserListVM caller, zwaUser selected)
        {
            InitializeComponent();
            BindingContext = new AppUserAddVM(Navigation, caller, selected);
        }

        /// <summary>
        /// Page constructor for new user
        /// </summary>
        /// <param name="caller"></param>
        public AppUserAddView(AppUserListVM caller)
        {
            InitializeComponent();
            BindingContext = new AppUserAddVM(Navigation, caller);
        }
    }
}