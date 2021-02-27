using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMSApp.ViewModels.Setting.AppUserGroup;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Setting.AppUserGroup
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppGroupListView : ContentPage
    {
        public AppGroupListView()
        {
            InitializeComponent();
            BindingContext = new AppGroupListVM(Navigation);
        }
    }
}