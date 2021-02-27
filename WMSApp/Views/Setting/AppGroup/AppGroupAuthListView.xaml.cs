using WMSApp.Models.SAP;
using WMSApp.Models.Setup.Group;
using WMSApp.ViewModels.Setting.AppGroup;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Setting.AppGroup
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppGroupAuthListView : ContentPage
    {
        public AppGroupAuthListView(OADM_Ex curCompany, zwaUserGroup group)
        {
            InitializeComponent();
            BindingContext = new AppGroupAuthListVM(Navigation, curCompany, group);
        }
    }
}