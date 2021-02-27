using System;
using System.Collections.Generic;
using System.Linq;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.SAP;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.GRPO
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GrpoScanView : ContentPage
    {
        List<POR1_Ex> ValidationList;        
        static bool IsScannerBusy = false;
        //static string SelectedMajorWhs;
        //bool IsFollowMajorWhs;

        public GrpoScanView(List<POR1_Ex> list) //, string selectedMajorWhs, bool isFollowMajorWhs)
        {
            InitializeComponent();
            ValidationList = list;
            //SelectedMajorWhs = selectedMajorWhs;
            //IsFollowMajorWhs = isFollowMajorWhs;

            var options = new ZXing.Mobile.MobileBarcodeScanningOptions();
            options.PossibleFormats = new List<ZXing.BarcodeFormat>() { ZXing.BarcodeFormat.All_1D, ZXing.BarcodeFormat.QR_CODE };
            scanner.Options = options;            
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            scanner.IsScanning = true;
        }

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

            if (String.IsNullOrWhiteSpace(result.ToString()) )
            {
                IsScannerBusy = false; // continue scan
                return;
            }

            Device.BeginInvokeOnMainThread( () =>
            {
                HandlerScanItem(result.ToString());
            });
        }

        void HandlerScanItem(string result)
        {
            var itemFound = ValidationList.Where(x => x.POLine.ItemCode.Equals(result)).FirstOrDefault();
            if (itemFound == null)
            {
                DisplayAlert("Item no found", "Please try again", "OK");
                IsScannerBusy = false; // continue scan
                return;
            }

            // capture the qty
            CaptureItemReceiptQty(itemFound);            
        }

        /// <summary>
        /// Prompt dialog to capture the Qty
        /// </summary>
        /// <param name="selectedPOLine"></param>
        async void CaptureItemReceiptQty(POR1_Ex selectedPOLine)
        {
            string receiptQty = await new Dialog().DisplayPromptAsync(
                $"Please input {selectedPOLine.ItemCodeDisplay}",
                $"{selectedPOLine.ItemNameDisplay}\nOpen Qty: {selectedPOLine.POLine.OpenQty:N}",
                "OK",
                "Cancel",
                "",
                -1,
                Keyboard.Numeric,
                $"{selectedPOLine.POLine.OpenQty:N}");

            if (receiptQty.ToLower().Equals("cancel"))
            {
                IsScannerBusy = false; // continue scan
                return;
            }

            if (receiptQty.Equals(""))
            {
                IsScannerBusy = false; // continue scan
                return;
            }

            var isNumeric = Decimal.TryParse(receiptQty, out decimal m_receiptQty);
            if (!isNumeric) // check numeric
            {
                //ShowAlert("Invalid entry of the receipt quantity, please try again. Thanks [x0]");
                IsScannerBusy = false; // continue scan
                return;
            }

            int locId = ValidationList.IndexOf(ValidationList
                .Where(x => x.POLine.LineNum.Equals(selectedPOLine.POLine.LineNum)).FirstOrDefault());
            if (locId < 0)
            {
                ShowAlert("Unable to local the item in list, please try again. Thanks [x1]");
                IsScannerBusy = false; // continue scan
                return;
            }

            if (m_receiptQty.Equals(0)) // check zero, if zero assume as reset receipt qty
            {
                //ShowAlert("Invalid entry of the receipt quantity zero, zero is not allowed, please try again. Thanks [x1]");
                ValidationList[locId].ReceiptQuantity = m_receiptQty; // assume as reset to the receipt
                //ValidationList[locId].ItemWhsCode = warehouse;
                ShowToast($"Receipt qty {m_receiptQty} for Item code {ValidationList[locId].ItemCodeDisplay} updated");
                IsScannerBusy = false; // continue scan
                return;
            }

            // else           
            if (m_receiptQty < 0) // check negative
            {
                ShowAlert($"The receive quantity ({m_receiptQty:N}) must be numeric and positve, " +
                    $"negative are no allowance.\nPlease try again later. Thanks [x2]");
                IsScannerBusy = false; // continue scan
                return;
            }

            if (ValidationList[locId].POLine.OpenQty < m_receiptQty) // check open qty
            {
                ShowAlert($"The receive quantity ({m_receiptQty:N}) must be equal or " +
                    $"smaller than the <= Open qty ({ValidationList[locId].POLine.OpenQty:N}).\nPlease try again later. Thanks [x3]");
                IsScannerBusy = false; // continue scan
                return;
            }

            // update into the list in memory
            ValidationList[locId].ReceiptQuantity = m_receiptQty;
            //ValidationList[locId].ItemWhsCode = warehouse;

            ShowToast($"Receipt qty {m_receiptQty} for Item code {ValidationList[locId].ItemCodeDisplay} updated");
            IsScannerBusy = false; // continue scan
        }


        void ShowToast(string message)
        {
            var itoast = DependencyService.Get<IToastMessage>();
            itoast?.ShortAlert(message);
        }

        /// <summary>
        /// Show the alert message on to the user screen
        /// </summary>
        /// <param name="message"></param>
        void ShowAlert(string message)
        {
            _ = new Dialog().DisplayAlert("Alert", message, "Ok");
        }

        private void scannerOverlay_FlashButtonClicked(Button sender, EventArgs e)
        {
            scanner.ToggleTorch();
        }

        private void btnCancel_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(ValidationList, "ScannedUpdatedList");
            Navigation.PopAsync();            
        }
    }
}