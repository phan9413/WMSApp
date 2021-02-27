
using WMSApp.Views.GRPO;
using Xamarin.Forms;

namespace WMSApp.ViewModels.GRPO
{
    public class GRPOSelectionMenuVM
    {
        public Command CmdCancel { get; set; }
        public Command CmdShowPOList { get; set; }
        public Command CmdProcessNoPo { get; set; }

        INavigation Navigation { get; set; } = null;
        /// <summary>
        /// Constructor
        /// </summary>
        public GRPOSelectionMenuVM (INavigation navigation)
        {
            Navigation = navigation;
            InitCmd();
        }

        /// <summary>
        /// Init command link
        /// </summary>
        void InitCmd ()
        {
            CmdCancel = new Command(() =>
            {
                Navigation.PopAsync();
            });

            CmdShowPOList = new Command( () =>
            {
                Navigation.PushAsync(new GrpoDocListView("OPEN"));
            });

            CmdProcessNoPo = new Command(() =>
            {
                Navigation.PushAsync(new GrpoWithNoPoView());
            });

        }
    }
}
