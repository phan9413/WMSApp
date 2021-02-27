using DbClass;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.GIGR;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.GoodsTrans
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GoodsTransScanView : ContentPage
    {
        string DefaultWhs, TransType;
        bool IsScannerBusy = false;
        bool IsFollowMajorWhs;
        bool IsQtyCancel, IsWarheouseCancel = false;
        OWHS SelectedWhs;

        /// <summary>
        /// Constuctor
        /// </summary>
        /// <param name="defaultWarehouse"></param>
        /// <param name="transType"></param>
        /// <param name="isFollowMajorWhs"></param>
        /// <param name="selectedWhs"></param>
        public GoodsTransScanView(string defaultWarehouse, string transType, bool isFollowMajorWhs, OWHS selectedWhs)
        {
            InitializeComponent();
            //DefaultWhs = defaultWarehouse;
            //IsFollowMajorWhs = isFollowMajorWhs;
            //TransType = transType;
            //SelectedWhs = selectedWhs;

            //var options = new ZXing.Mobile.MobileBarcodeScanningOptions();
            //options.PossibleFormats = new List<ZXing.BarcodeFormat>() { ZXing.BarcodeFormat.All_1D, ZXing.BarcodeFormat.QR_CODE };
            //scanner.Options = options;
        }

        protected override void OnAppearing()
        {
            //base.OnAppearing();
            
            //scanner.IsEnabled = true;
            //scanner.IsScanning = true;
        }

        protected override void OnDisappearing()
        {
            //base.OnDisappearing();
            //scanner.IsEnabled = false;
            //scanner.IsScanning = false;
        }

        private void scanner_OnScanResult(ZXing.Result result)
        {

        }

        //async void HandlerScanItem(string result)
        //{
        //    // prepare the ask server             
        //    var cioRequest = new Cio() // load the data from web server 
        //    {
        //        sap_logon_name = App.waUser,
        //        sap_logon_pw = App.waPw,
        //        token = App.waToken,
        //        phoneRegID = App.waToken,
        //        request = "GetItem",
        //        QueryItemCode = result
        //    };

        //    string content, lastErrMessage;
        //    bool isSuccess = false;
        //    using (var httpClient = new HttpClientWapi())
        //    {
        //        content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Items");
        //        isSuccess = httpClient.isSuccessStatusCode;
        //        lastErrMessage = httpClient.lastErrorDesc;
        //    }

        //    if (!isSuccess)
        //    {
        //        ShowMessage($"Item code: {result} no found, please try again");
        //        IsScannerBusy = false; // continue scan
        //    }


        //    var replied = JsonConvert.DeserializeObject<Cio>(content);
        //    if (replied == null)
        //    {
        //        IsScannerBusy = false; // continue scan
        //        ShowMessage($"Item code: {result} no found, please try again");
        //        return;
        //    }

        //    if (replied.Item == null)
        //    {
        //        IsScannerBusy = false; // continue scan
        //        ShowMessage($"Item code: {result} no found, please try again");
        //        return;
        //    }

        //    var foundItem = new OITM_Ex
        //    {
        //        Item = replied.Item,
        //        TransType = TransType,
        //        Navigation = Navigation
        //    };

        //    // check the warehouse                 
        //    // -------------------------------------------------------------
        //    string itemWhs = DefaultWhs;
        //    if (IsFollowMajorWhs)
        //    {
        //        if (SelectedWhs.BinActivat.Equals("Y"))
        //        {
        //            PromptQhsBinItemQtyEntries(foundItem);
        //            IsScannerBusy = false; // continue scan
        //            return;
        //        }
        //        goto CaptureItemQty;
        //    }
        //    else if (!IsFollowMajorWhs)
        //    {
        //        // if not follow default warehouse
        //        // prompt user to select a warehouse
        //        // if selected warehouse is bin activated, then prompt bin item qty entry
        //        // else continue only enter the qty
        //        itemWhs = await CaptureWhs();
        //        if (!IsFollowMajorWhs && SelectedWhs.BinActivat.Equals("Y"))
        //        {
        //            PromptQhsBinItemQtyEntries(foundItem);
        //            IsScannerBusy = false; // continue scan
        //            return;
        //        }
        //        goto CaptureItemQty;
        //    }

        //// capture the quantity
        //// -------------------------------------------------------------
        //CaptureItemQty:

        //    string qty = await CaptureItemReceiptQty(foundItem);
        //    if (IsQtyCancel)
        //    {
        //        IsQtyCancel = false;
        //        IsScannerBusy = false; // continue scan
        //        return;
        //    }

        //    if (qty.Length == 0)
        //    {
        //        IsScannerBusy = false; // continue scan
        //        ShowMessage($"Invalid qty for Item code: {result}, please try again");
        //        return;
        //    }

        //    // update into the object
        //    foundItem.TransQty = Convert.ToDecimal(qty);
        //    foundItem.ItemWhsCode = itemWhs;
        //    foundItem.TransType = TransType;

        //    MessagingCenter.Send(foundItem, "UpdateScanItem");
        //    ShowToast($"Item code {result} add into list");
        //    IsScannerBusy = false; // continue scan
        //    return;
        //}

        /// <summary>
        /// Promtp bin item qty entries screen
        /// </summary>
        //void PromptQhsBinItemQtyEntries(OITM_Ex selected)
        //{
        //    const string address = "ScanItemSelectWhsBin";
        //    selected.TransType = TransType;
        //    MessagingCenter.Subscribe<OITM_Ex>(this, address, (OITM_Ex ItemWithBins) =>
        //    {
        //        MessagingCenter.Unsubscribe<OITM_Ex>(this, address);
        //        ItemWithBins.TransType = TransType;

        //        //AddItem(ItemWithBins); // sent back to VM to addItem
        //        return;
        //    });

        //    selected.PromptWarehouseSelectionAsync(SelectedWhs, address);
        //}

        /// <summary>
        /// Prompt dialog to capture the Qty
        /// </summary>
        /// <param name="selectedPOLine"></param>
        //async Task<string> CaptureItemReceiptQty(OITM_Ex selected)
        //{
        //    string receiptQty = await new Dialog().DisplayPromptAsync(
        //        $"Please input {selected.ItemCodeDisplay}",
        //        $"{selected.ItemNameDisplay}",
        //        "OK",
        //        "Cancel",
        //        "",
        //        -1,
        //        Keyboard.Numeric,
        //        "1");

        //    if (receiptQty.ToLower().Equals("cancel"))
        //    {
        //        IsScannerBusy = false; // continue scan
        //        IsQtyCancel = true;
        //        return string.Empty;
        //    }

        //    if (receiptQty.Equals(""))
        //    {
        //        IsScannerBusy = false; // continue scan
        //        return string.Empty;
        //    }

        //    var isNumeric = Decimal.TryParse(receiptQty, out decimal m_receiptQty);
        //    if (!isNumeric) // check numeric
        //    {
        //        //ShowAlert("Invalid entry of the receipt quantity, please try again. Thanks [x0]");
        //        IsScannerBusy = false; // continue scan
        //        return string.Empty;
        //    }

        //    if (m_receiptQty.Equals(0)) // check zero, if zero assume as reset receipt qty
        //    {
        //        //ShowAlert("Invalid entry of the receipt quantity zero, zero is not allowed, please try again. Thanks [x1]");

        //        IsScannerBusy = false; // continue scan
        //        return string.Empty;
        //    }

        //    // else           
        //    if (m_receiptQty < 0) // check negative
        //    {
        //        ShowMessage($"The receive quantity ({m_receiptQty:N}) must be numeric and positve, " +
        //            $"negative are no allowance.\nPlease try again later. Thanks [x2]");
        //        IsScannerBusy = false; // continue scan
        //        return string.Empty;
        //    }

        //    IsScannerBusy = false; // continue scan
        //    return m_receiptQty.ToString();
        //}

        /// <summary>
        /// Use to capture the warehouse for each item (if any)
        /// </summary>
        /// <returns></returns>
        //async Task<string> CaptureWhs()
        //{
        //    if (App.Warehouses == null) return string.Empty;
        //    var whsList = App.Warehouses.Select(x => x.WhsCode).Distinct().ToArray();
        //    var selectedWhs = await new Dialog().DisplayActionSheet($"Select {TransType} warehouse", "Cancel", null, whsList);

        //    if (string.IsNullOrWhiteSpace(selectedWhs))
        //    {
        //        return DefaultWhs;
        //    }

        //    if (selectedWhs.ToLower().Equals("cancel")) /// if then rxit the screen
        //    {
        //        IsWarheouseCancel = true;
        //        return DefaultWhs;
        //    }
        //    return selectedWhs;
        //}

        //void ShowMessage(string message) => DisplayAlert("Alert", message, "OK");        

        //void ShowToast(string message)
        //{
        //    var toast = DependencyService.Get<IToastMessage>();
        //    toast?.ShortAlert(message);
        //}

        //private void scanner_OnScanResult(ZXing.Result result)
        //{
        //    if (IsScannerBusy) return;
        //    IsScannerBusy = true;

        //    if (result == null)
        //    {
        //        IsScannerBusy = false; // continue scan
        //        return;
        //    }

        //    if (String.IsNullOrWhiteSpace(result.ToString()))
        //    {
        //        IsScannerBusy = false; // continue scan
        //        return;
        //    }

        //    Device.BeginInvokeOnMainThread(() =>
        //    {
        //        HandlerScanItem(result.ToString());
        //    });
        //}

        private void scannerOverlay_FlashButtonClicked(Button sender, EventArgs e)
        {
            scanner.ToggleTorch();
        }
        private void btnCancel_Clicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

    }
}