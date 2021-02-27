using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.SAP;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.InventoryCounts
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InventoryCountsItemScanView : ContentPage
    {
        List<INC1_Ex> ItemList;
        bool IsScannerBusy = false;

        public InventoryCountsItemScanView(List<INC1_Ex> sourceList)
        {
            InitializeComponent();
            ItemList = sourceList;
            var options = new ZXing.Mobile.MobileBarcodeScanningOptions();
            options.PossibleFormats = new List<ZXing.BarcodeFormat>() { ZXing.BarcodeFormat.All_1D, ZXing.BarcodeFormat.QR_CODE };
            scanner.Options = options;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            scanner.IsScanning = true;
        }

        private void scannerOverlay_FlashButtonClicked(Button sender, EventArgs e)
        {
            scanner.ToggleTorch();
        }

        private void btnCancel_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(ItemList, "InvenCountScannedList");
            Navigation.PopAsync();
        }

        void ShowToast(string message)
        {
            var itoast = DependencyService.Get<IToastMessage>();
            itoast?.ShortAlert(message);
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

            if (String.IsNullOrWhiteSpace(result.ToString()))
            {
                IsScannerBusy = false; // continue scan
                return;
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                HandlerScanItem(result.ToString());
            });
        }

        /// <summary>
        /// Handle the item code
        /// </summary>
        /// <param name="itemCode"></param>
        void HandlerScanItem(string itemCode)
        {
            try
            {
                var foundItem = ItemList.Where(x => x.ItemCodeDisplay == itemCode).FirstOrDefault();
                if (foundItem == null)
                {
                    ShowAlert($"The item code {itemCode} no found in the list, please try others. Thanks");
                    IsScannerBusy = false;
                    return;
                }

                CaptureItemReceiptQty(foundItem);
            }
            catch (Exception excep)
            {
                ShowAlert(excep.ToString());
            }
        }

        /// <summary>
        /// Prompt dialog to capture the Qty
        /// </summary>
        /// <param name="selectedPOLine"></param>
        async void CaptureItemReceiptQty(INC1_Ex selectedLine)
        {
            string receiptQty = await new Dialog().DisplayPromptAsync(
                $"Please input {selectedLine.ItemCodeDisplay}",
                $"{selectedLine.ItemDescDisplay} counted Qty",
                "OK",
                "Cancel",
                "",
                -1,
                Keyboard.Numeric,
                "");

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
                IsScannerBusy = false; // continue scan
                return;
            }

            int locId = ItemList.IndexOf(selectedLine);
            if (locId < 0)
            {
                ShowAlert("Unable to local the item in list, please try again. Thanks [x1]");
                IsScannerBusy = false; // continue scan
                return;
            }

            if (m_receiptQty.Equals(0)) // check zero, if zero assume as reset receipt qty
            {
                //ShowAlert("Invalid entry of the receipt quantity zero, zero is not allowed, please try again. Thanks [x1]");
                ItemList[locId].CountedQty = m_receiptQty; // assume as reset to the receipt     

                ShowToast($"counted qty {m_receiptQty} for Item code {ItemList[locId].ItemCodeDisplay} updated.");
                IsScannerBusy = false; // continue scan
                return;
            }

            // else           
            if (m_receiptQty < 0) // check negative
            {
                ShowAlert($"The counted quantity ({m_receiptQty:N}) must be numeric and positve, " +
                    $"negative are no allowance.\nPlease try again later. Thanks [x2]");
                IsScannerBusy = false; // continue scan
                return;
            }

            // update into the list in memory
            ItemList[locId].CountedQty = m_receiptQty;
            ShowToast($"Counted qty {m_receiptQty} for Item code {ItemList[locId].ItemCodeDisplay} updated");
            IsScannerBusy = false; // continue scan
        }

        /// <summary>
        /// Show the alert message on to the user screen
        /// </summary>
        /// <param name="message"></param>
        void ShowAlert(string message)
        {
            _ = new Dialog().DisplayAlert("Alert", message, "Ok");
        }


    }
}