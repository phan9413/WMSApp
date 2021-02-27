using System;
using WMSApp.Models.GRPO;
using WMSApp.ViewModels.Share;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Share
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CollectBatchesView : ContentPage
    {
        public CollectBatchesView(string returnAddr)
        {
            InitializeComponent();
            BindingContext = new CollectBatchesVM(Navigation, returnAddr);
        }

        /// <summary>
        ///  Use to collectio bath list only
        /// </summary>
        /// <param name="returnAddr"></param>
        /// <param name="isCollectioBatchOnly"></param>
        public CollectBatchesView(string returnAddr, bool isCollectioBatchOnly)
        {
            InitializeComponent();
            BindingContext = new CollectBatchesVM(Navigation, returnAddr, isCollectioBatchOnly);
        }

        async void OnDeleteSwipeItemInvoked(object sender, EventArgs e)
        {
            SwipeItem swipeItem = (SwipeItem)sender;
            if (swipeItem == null) return;
            Batch selected = swipeItem.CommandParameter as Batch;

            if (selected == null) return;
            
            bool isComfirmRemove = await DisplayAlert("Confirm removed?",
                $"Remove {selected.DistNumber}, {selected.Qty} ?", "Yes", "Cancel");

            if (!isComfirmRemove) return;
            ((CollectBatchesVM)BindingContext).DeleteItem(selected);
        }

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ((CollectBatchesVM)BindingContext).Cancel();
            });
            return true;
        }
    }
}