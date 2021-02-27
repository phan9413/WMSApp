using System.Collections.Generic;
using System.Threading;
using WMSApp.Models.SAP;
using WMSApp.ViewModels.Share;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Share
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CaptureTextView : ContentPage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pageTitle"></param>
        /// <param name="repliedMcAddress"></param>
        public CaptureTextView(string pageTitle, string dataType, string repliedMcAddress, 
            List<OBIN_Ex> bins, decimal controlQty, bool isCheckQty)
        {
            InitializeComponent();
            BindingContext = new CaptureTextVM(Navigation, pageTitle, dataType, repliedMcAddress, bins, controlQty, isCheckQty);
            NavigationPage.SetHasBackButton(this, false); // remove the back button from page
        }

        /// <summary>
        /// set text box focus ready for handheld
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            Thread.Sleep(300);
            //lbHandHeldValue.Focus(); // hide for no support hand held
        }

        /// <summary>
        /// Control the hardware button from the phone
        /// </summary>
        /// <returns></returns>
        protected override bool OnBackButtonPressed()
        {
            ((CaptureTextVM)BindingContext).SaveAndClose();
            return true; // retain the screen
        }

        /// <summary>
        /// handle when scanner button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// Temporary hide for no support hand held scanning
        //private void TapGestureRecognizer_Tapped(object sender, System.EventArgs e)
        //{
        //    lbHandHeldValue.Focus();
        //}

        /// <summary>
        /// Handle handheld scanning
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// Temporary hide for no support hand held scanning
        //private void lbHandHeldValue_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    if (string.IsNullOrWhiteSpace(lbHandHeldValue.Text)) return;
        //    ((CaptureTextVM)BindingContext).HandlerCaptureResult(lbHandHeldValue.Text);
        //    lbHandHeldValue.Text = string.Empty;
        //}


        private async void OnDeleteSwipeItemInvoked(object sender, System.EventArgs e)
        {
            SwipeItem swipeItem = (SwipeItem)sender;
            if (swipeItem == null) return;
            OBIN_Ex selected = (OBIN_Ex)swipeItem.CommandParameter;

            if (selected == null) return;

            bool isComfirmRemove = await DisplayAlert("Confirm removed?", 
                $"Remove {selected.oBIN.BinCode} --> Receipted {selected.BinQty}?", "Yes", "Cancel");
            if (!isComfirmRemove) return;

            ((CaptureTextVM)BindingContext).RemoveBinListObject(selected);
        
            
        
        }
    }
}