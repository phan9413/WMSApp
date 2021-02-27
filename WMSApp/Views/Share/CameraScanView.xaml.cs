using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Share
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CameraScanView : ContentPage
    {
        string repliedAddress;
        string bottomText;
        bool IsScannerBusy = false;

        public CameraScanView(string mcRepliedAddress, string intruction = "") //, bool isPlaySound = false, bool isVibrate = false)
        {
            bottomText = intruction;
            InitializeComponent();
            repliedAddress = mcRepliedAddress;

            var options = new ZXing.Mobile.MobileBarcodeScanningOptions();
            options.PossibleFormats = new List<ZXing.BarcodeFormat>() { ZXing.BarcodeFormat.All_1D, ZXing.BarcodeFormat.QR_CODE };
            scanner.Options = options;
        }

        /// <summary>
        /// Init the camara scanning
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            scanner.IsScanning = true;
            IsScannerBusy = false;
            scannerOverlay.BottomText = bottomText;
        }

        /// <summary>
        /// Event return from camera scan
        /// </summary>
        /// <param name="result"></param>
        private void scanner_OnScanResult(ZXing.Result result)
        {
            if (IsScannerBusy) return;
            IsScannerBusy = true;

            if (result == null)
            {
                IsScannerBusy = false; // continue scan
                return;
            }

            if (result == null)
            {
                IsScannerBusy = false; // continue scan
                return;
            }

            if (string.IsNullOrWhiteSpace(result.ToString()))
            {
                IsScannerBusy = false; // continue scan
                return;
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                HandlerScanItem(result.ToString());
            });
        }

        void HandlerScanItem (string result)
        {
            MessagingCenter.Send<string>(result, repliedAddress);
            Navigation.PopAsync();
        }

        private void btnCancel_Clicked(object sender, EventArgs e)
        {
            try
            {
#if (DEBUG)
                //MessagingCenter.Send("0100-2240", repliedAddress);
                //Navigation.PopAsync();
#endif

                MessagingCenter.Send("cancel", repliedAddress);
                Navigation.PopAsync();
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
            }
        }

        private void scannerOverlay_FlashButtonClicked(Button sender, EventArgs e)
        {
            scanner.ToggleTorch();
        }

        /// <summary>
        /// Allow manual entry
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        //{
        //    if (EntryManualInput == null) return;
        //    if (string.IsNullOrWhiteSpace(EntryManualInput.Text)) return;
        //    HandlerScanItem(EntryManualInput.Text);
        //}

        private void torchLight_Clicked(object sender, EventArgs e)
        {
            scanner.ToggleTorch();
        }

        private void btnSubmit_Tapped(object sender, EventArgs e)
        {
            if (EntryManualInput == null) return;
            if (string.IsNullOrWhiteSpace(EntryManualInput.Text)) return;
            HandlerScanItem(EntryManualInput.Text);
        }
    }
}