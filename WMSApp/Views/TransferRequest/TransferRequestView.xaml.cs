using DbClass;
using WMSApp.Models.SAP;
using WMSApp.ViewModels.TransferRequest;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.TransferRequest
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TransferRequestView : ContentPage
    {
        public TransferRequestView()
        {
            InitializeComponent();
            BindingContext = new TransferRequestVM(Navigation);
        }

        /// <summary>
        /// Handler swipe item when user swipe remote item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwipeItem_RemoveItem(object sender, System.EventArgs e)
        {
            var swipeItem = sender as SwipeItem;
            if (swipeItem == null) return;

            var item = (WTQ1_Ex)swipeItem.CommandParameter;
            if (item == null) return;

            ((TransferRequestVM)BindingContext).RemoveItemFromList(item);
        }

        /// <summary>
        /// Handle user change line warehouse from and to warehouse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwipeItem_EditItemFromWarehouse(object sender, System.EventArgs e)
        {
            var swipeItem = sender as SwipeItem;
            if (swipeItem == null) return;

            var item = (WTQ1_Ex)swipeItem.CommandParameter;
            if (item == null) return;

            ((TransferRequestVM)BindingContext).EditFromWarehouse(item);
        }

        /// <summary>
        /// Edit to warehouse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwipeItem_EditItemToWarehouse(object sender, System.EventArgs e)
        {
            var swipeItem = sender as SwipeItem;
            if (swipeItem == null) return;

            var item = (WTQ1_Ex)swipeItem.CommandParameter;
            if (item == null) return;

            ((TransferRequestVM)BindingContext).EditToWarehouse(item);
        }
    }
}