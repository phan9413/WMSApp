using WMSApp.ViewModels.Setting.AppUser;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using WMSApp.ViewModels.Setting;
using WMSApp.ViewModels.Setting.AppUserGroup;

namespace WMSApp.Views.Setting
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppChseCmpnyListView : ContentPage
    {
        /// <summary>
        /// Constructor for user
        /// </summary>
        /// <param name="caller"></param>
        public AppChseCmpnyListView(AppUserListVM caller)
        {
            InitializeComponent();
            BindingContext = new AppChseCmpnyListVM(Navigation, caller);
        }

        /// <summary>
        /// for group list display
        /// </summary>
        /// <param name="caller"></param>
        public AppChseCmpnyListView(AppGroupListVM caller)
        {
            InitializeComponent();
            BindingContext = new AppChseCmpnyListVM(Navigation, caller);
        }
    }
}