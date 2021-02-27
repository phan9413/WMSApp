using Rg.Plugins.Popup.Extensions;
using System.ComponentModel;
using WMSApp.Views.StandAloneTransfer;
using Xamarin.Forms;

namespace WMSApp.ViewModels.StandAloneTransfer
{
    public class StansAloneTransferMenuVM : INotifyPropertyChanged
    {
        public Command CmdCreateNewTransfer { get; set; }
        public Command CmdPickTransfer { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        INavigation Navigation;

        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        ///  the constructor
        /// </summary>
        /// <param name="navigation"></param>
        public StansAloneTransferMenuVM(INavigation navigation)
        {
            Navigation = navigation;
            InitCmds();
        }

        void InitCmds()
        {
            CmdCreateNewTransfer = new Command(() =>
            {
                Navigation.PopPopupAsync();
                Navigation.PushAsync(new StandAloneTransferLineFROMView());
                
            });

            CmdPickTransfer = new Command(() =>
            {
                Navigation.PopPopupAsync();
                Navigation.PushAsync(new StandAloneTransferListView());                
            });
        }
    }
}
