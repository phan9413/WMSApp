using System;
using WMSApp.ViewModels.Share;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Share
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CollectSerialsView : ContentPage
    {
        public CollectSerialsView(string callerAddress)
        {
            InitializeComponent();
            BindingContext = new CollectSerialsVM(Navigation, callerAddress);
        }

        private async void OnDeleteSwipeItemInvoked(object sender, EventArgs e)
        {
            SwipeItem swipeItem = (SwipeItem)sender;
            if (swipeItem == null) return;
            string selected = (string)swipeItem.CommandParameter;

            if (selected == null) return;
            if (string.IsNullOrWhiteSpace(selected)) return;

            bool isComfirmRemove = await DisplayAlert("Confirm removed?",
                $"Remove {selected} ?", "Yes", "Cancel");

            if (!isComfirmRemove) return;
            ((CollectSerialsVM)BindingContext).DeleteItem(selected);
        }

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ((CollectSerialsVM)BindingContext).Cancel();
            });
            return true;
        }
    }
}