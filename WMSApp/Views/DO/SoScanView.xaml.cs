using System;
using System.Collections.Generic;
using System.Linq;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.SAP;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.DeliveryOrder
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SoScanView : ContentPage
    {
        List<RDR1_Ex> ValidationList;
        //string SelectedMajorWhs= string.Empty;
        //bool IsFollowMajorWh  = false;
        static bool IsScannerBusy = false;

        public SoScanView(List<RDR1_Ex> list) //, string selectedMajorWh, bool isFollowMajorWhs)
        {
            InitializeComponent();
            ValidationList = list;
            //SelectedMajorWhs = selectedMajorWh;
            //IsFollowMajorWh = isFollowMajorWhs;

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

        void HandlerScanItem(string result)
        {
            var itemFound = ValidationList.Where(x => x.ItemCode.Equals(result)).FirstOrDefault();
            if (itemFound == null)
            {
                DisplayAlert("Item no found", "Please try again", "OK");
                IsScannerBusy = false; // continue scan
                return;
            }

            // capture the qty
            CaptureItemExitQty(itemFound);
        }

        /// <summary>
        /// Prompt dialog to capture the Qty
        /// </summary>
        /// <param name="selectedPOLine"></param>
        async void CaptureItemExitQty(RDR1_Ex selectedSOLine)
        {
            string receiptQty = await new Dialog().DisplayPromptAsync(
                $"Please input {selectedSOLine.ItemCodeDisplay}",
                $"{selectedSOLine.ItemNameDisplay}\nOpen Qty: {selectedSOLine.OpenQty:N}",
                "OK",
                "Cancel",
                "",
                -1,
                Keyboard.Numeric,
                $"{selectedSOLine.OpenQty:N}");

            // settle the warehouse

            // 20200617T
            // get the selected whs for this item
            // capture the warehouse
            //string warehouse = SelectedMajorWhs;
            //if (!IsFollowMajorWh) // if follow the presented warehouse by pass the warehouse selection
            //{
            //    if (App.Warehouses != null)
            //    {
            //        var whsList = App.Warehouses.Select(x => x.WhsCode).Distinct().ToArray();
            //        warehouse = await new Dialog().DisplayActionSheet("Select an exit warehouse", "Cancel", "", whsList);
            //        if (warehouse.Equals("cancel"))
            //        {
            //            warehouse = SelectedMajorWhs;
            //        }
            //    }
            //}

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
                .Where(x => x.LineNum.Equals(selectedSOLine.LineNum)).FirstOrDefault());

            if (locId < 0)
            {
                ShowAlert("Unable to local the item in list, please try again. Thanks [x1]");
                IsScannerBusy = false; // continue scan
                return;
            }

            if (m_receiptQty.Equals(0)) // check zero, if zero assume as reset receipt qty
            {
                //ShowAlert("Invalid entry of the receipt quantity zero, zero is not allowed, please try again. Thanks [x1]");
                ValidationList[locId].ExitQuantity = m_receiptQty; // assume as reset to the receipt
                //ValidationList[locId].ItemWhsCode = warehouse;
                ShowToast($"Exit qty {m_receiptQty} for Item code {ValidationList[locId].ItemCodeDisplay} updated");
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

            if (ValidationList[locId].OpenQty < m_receiptQty) // check open qty
            {
                ShowAlert($"The receive quantity ({m_receiptQty:N}) must be equal or " +
                    $"smaller than the <= Open qty ({ValidationList[locId].OpenQty:N}).\nPlease try again later. Thanks [x3]");
                IsScannerBusy = false; // continue scan
                return;
            }

            // update into the list in memory
            ValidationList[locId].ExitQuantity = m_receiptQty;
           // ValidationList[locId].ItemWhsCode = warehouse;

            ShowToast($"Exit qty {m_receiptQty} for Item code {ValidationList[locId].ItemCodeDisplay} updated");
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
            MessagingCenter.Send(ValidationList, "SoScannedUpdatedList");
            Navigation.PopAsync();
        }
    }
}