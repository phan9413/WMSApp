using WMSApp.Models.StandAloneTransfer;
using WMSApp.ViewModels.StandAloneTransfer;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace WMSApp.Views.StandAloneTransfer
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StandAloneTransferLineFROMView : ContentPage
    {
        public StandAloneTransferLineFROMView()
        {
            InitializeComponent();
            BindingContext = new StandAloneTransferLineFROMVM(Navigation);
        }

        /// <summary>
        /// handle the remove item from list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void OnDeleteSwipeItemInvoked(object sender, System.EventArgs e)
        {
            var swipeItem = (SwipeItem)sender;
            if (swipeItem == null) return;

            var param = swipeItem.CommandParameter as TransferLine;
            if (param == null) return;

            bool confirmRemove = 
                await DisplayAlert(
                $"Remove item {param.ItemCode}?", 
                "", 
                "Confirm", "Cancel");

            if (confirmRemove)
            {
                ((StandAloneTransferLineFROMVM)BindingContext).RemoveItem(param);
            }
        }
    }
}