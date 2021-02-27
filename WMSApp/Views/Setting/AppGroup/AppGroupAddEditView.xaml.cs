using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMSApp.Models.Setup.Group;
using WMSApp.ViewModels.Setting.AppGroup;
using WMSApp.ViewModels.Setting.AppUserGroup;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Setting.AppGroup
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppGroupAddEditView : ContentPage
    {
        /// <summary>
        /// for modified the exsting group
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="group"></param>
        public AppGroupAddEditView(AppGroupListVM caller, zwaUserGroup group)
        {
            InitializeComponent();
            BindingContext = new AppGroupAddEditVM(Navigation, caller, group);
        }


        /// <summary>
        /// For Add in the new group
        /// </summary>
        /// <param name="caller"></param>
        public AppGroupAddEditView(AppGroupListVM caller)
        {
            InitializeComponent();
            BindingContext = new AppGroupAddEditVM(Navigation, caller);
        }
    }
}