
using WMSApp.Views.DeliveryOrder;
using WMSApp.Views.DO;
using Xamarin.Forms;

namespace WMSApp.ViewModels.DO
{
    public class DOSelectionMenuVM 
    {
        public Command CmdCancel { get; set; }
        public Command CmdShowSOList { get; set; }
        public Command CmdProcessNoSo { get; set; }
        public Command CmdProcessPickList { get; set; }

        INavigation Navigation { get; set; } = null;
        /// <summary>
        /// Constructor
        /// </summary>
        public DOSelectionMenuVM(INavigation navigation)
        {
            Navigation = navigation;
            InitCmd();
        }

        /// <summary>
        /// Init command link
        /// </summary>
        void InitCmd()
        {
            CmdCancel = new Command(() =>
            {
                Navigation.PopAsync();
            });

            CmdShowSOList = new Command(() =>
            {                
                Navigation.PushAsync(new SoDocListView("OPEN"));
            });

            CmdProcessNoSo = new Command(() =>
            { 
                Navigation.PushAsync(new DoWithNoSoView());
            });

            CmdProcessPickList = new Command(() =>
            {
                //Navigation.PushAsync(new GrpoWithNoPoView());
            });

        }
    }
}

