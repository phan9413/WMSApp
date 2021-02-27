using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMSApp.Models.Login;
using WMSApp.Models.Setup.Group;
using WMSApp.ViewModels.Setting.AppGroup;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Setting.AppGroup
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppGroupUsrListView : ContentPage
    {
        public AppGroupUsrListView(zwaUserGroup group, List<zwaUser> users)
        {
            InitializeComponent();
            BindingContext = new AppGroupUsrListVM(Navigation, group, users);
        }
    }
}