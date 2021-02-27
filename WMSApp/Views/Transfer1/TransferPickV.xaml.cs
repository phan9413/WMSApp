using System;
using WMSApp.Models.Transfer1;
using WMSApp.ViewModels.Transfer1;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Transfer1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TransferPickV : ContentPage
    {        
        public TransferPickV(TransferDataM item, string callerAddress, string whsDirection)
        {
            InitializeComponent();            
            BindingContext = new TransferPickVM(Navigation, item, callerAddress, whsDirection);
        }

        /// <summary>
        /// Handler list view swipe operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async private void OnDeleteSwipeItemInvoked(object sender, EventArgs e)
        {
            SwipeItem swipeItem = (SwipeItem)sender;
            if (swipeItem == null) return;

            var param = swipeItem.CommandParameter as TransferItemDetailBinM;            
            if (param == null) return;

            bool confirmRemove = await DisplayAlert($"Remove item {param.ItemCode}?", 
                $"Dist. Number {param.CheckDistNum}?", 
                "Confirm", "Cancel");

            if (confirmRemove)
            {
                ((TransferPickVM)BindingContext).RemoveItem(param);
            }
        }
    }
}