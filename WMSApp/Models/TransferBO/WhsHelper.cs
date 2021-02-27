using Acr.UserDialogs;
using DbClass;
using Newtonsoft.Json;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WMSApp.Class;
using WMSApp.Models.Share;
using WMSApp.ViewModels.Share;
using WMSApp.Views.Share;
using Xamarin.Forms;

namespace WMSApp.Models.TransferBO
{
    public class WhsHelper : IDisposable
    {
        public decimal NeededQty { get; set; }
        public OWHS FromOWhs { get; set; }
        public OWHS ToOWhs { get; set; }
        public OITM OItm { get; set; }
        public INavigation Navigation { get; set; }
        public List<CommonStockInfo> StockList { get; set; }
        public List<CommonStockInfo> SelectedStockList { get; set; }

        string PageTitle = "From warehouse";
        string Request = string.Empty;
        decimal TransferQty = -1;

        readonly string webApiAddress = "Transfer";
        bool isStockListLoop = false;

        /// <summary>
        /// The constructor
        /// </summary>
        public WhsHelper() { }

        /// <summary>
        /// Manage from warehouse procedure
        /// </summary>
        public void ManageFromWhs()
        {
            switch (FromOWhs.BinActivat)
            {
                case "Y":
                    {
                        if (OItm.ManBtchNum.Equals("Y"))
                        {
                            // Show available batch in all bin of this warehouse
                            Request = "GetItemWhsBinBatch";
                        }
                        else if (OItm.ManSerNum.Equals("Y")) // Show available serial in all bin of this warehouse
                        {

                            Request = "GetItemWhsBinSerial";
                        }
                        else
                        {
                            // show bin qty in each warehouse
                            Request = "GetItemWhsBin";
                        }

                        //PrompEntryQty


                        //BinSelectionPopUpView
                        var returnAddrs = "20201004T1853_WhsHelper";
                        MessagingCenter.Subscribe<List<CommonStockInfo>>(this, returnAddrs, (List<CommonStockInfo> selected) =>
                        {
                            MessagingCenter.Unsubscribe<List<CommonStockInfo>>(this, returnAddrs);

                            if (selected == null) return;
                            SelectedStockList = selected;
                            ManageToWhs();
                        });

                        Navigation.PushAsync(
                            new TransferBoItemSrBatView(StockList, returnAddrs, OItm.ItemCode, FromOWhs.WhsCode, NeededQty, PageTitle, Request));

                        return;
                    }
                default: // normal warehouse operation
                    {
                        // handler bin no activated warehouse
                        if (OItm.ManBtchNum.Equals("Y"))
                        {
                            // show availble batch in this warehouse
                            // GetItemWhsBatch
                            Request = "GetItemWhsBatch";

                        }
                        else if (OItm.ManSerNum.Equals("Y"))
                        {
                            // show availble Serial in this warehouse
                            Request = "GetItemWhsSerial";
                        }
                        else
                        {
                            // show availble Qty in this warehouse
                        }

                        PromptFromWhsEntryQty();
                        return;
                    }
            }

        }

        /// <summary>
        /// Check current warehouse item qty
        /// </summary>
        /// <param name="itemCode"></param>
        /// <param name="whsCode"></param>
        /// <param name="requestQty"></param>
        async void PromptFromWhsEntryQty()
        {
            var transferQty = await new Dialog()
                    .DisplayPromptAsync(
                        $"Input {OItm.ItemCode} transfer qty"
                        , ""
                        , "OK"
                        , "Cancel"
                        , ""
                        , -1, Keyboard.Default
                        , $"{this.NeededQty:N}");

            if (string.IsNullOrWhiteSpace(transferQty)) return;
            if (transferQty.ToLower().Equals("cancel")) return;

            var isNummeric = decimal.TryParse(transferQty, out decimal actualValue);
            if (actualValue <= 0) return;

            #region hide control of transfer qty            
            // hide for future activation
            //if (requestLine.RequestQuantity < actualValue)
            //{
            //    DisplayAlert($"Requested Qty: {requestLine.RequestQuantity}, Transfer Qty: {actualValue} is more than request");
            //    return;
            //}
            #endregion

            decimal onHand = await CheckItemCodeAndWarehouseQty(OItm.ItemCode, FromOWhs.WhsCode);
            if (onHand <= 0)
            {
                DisplayAlert($"Item code {this.OItm.ItemCode} " +
                    $"in request FROM {FromOWhs.WhsCode} (on hand {onHand:N}), " +
                    $"of quantity {this.NeededQty:N} was insufficient. (t0)");
                return;
            }

            if (onHand < actualValue)
            {
                DisplayAlert($"Item code {this.OItm.ItemCode} " +
                    $"in request of warehouse-> {FromOWhs.WhsCode} (on hand {onHand:N}), " +
                    $"of quantity {this.NeededQty:N} was insufficient. (t1)");
                return;
            }

            TransferQty = actualValue;
            LaunchTransferBoBinSelectionView();
        }

        /// <summary>
        /// Launch the screen for selection
        /// </summary>
        void LaunchTransferBoBinSelectionView()
        {
            string returnAddrs = "20201004T2219_WhsHelper";
            MessagingCenter.Subscribe<List<CommonStockInfo>>(this, returnAddrs, (List<CommonStockInfo> selected) =>
            {
                MessagingCenter.Unsubscribe<List<CommonStockInfo>>(this, returnAddrs);
                if (selected == null) return;
                SelectedStockList = selected;
                ManageToWhs();
            });

            Navigation.PushAsync(
                new TransferBoItemSrBatView(StockList, returnAddrs, OItm.ItemCode, FromOWhs.WhsCode, TransferQty, PageTitle, Request));
        }

        /// <summary>
        /// Use to check the item in the given warehouse
        /// </summary>
        /// <param name="itemCode"></param>
        /// <param name="warehouseCode"></param>
        /// <returns></returns>
        async Task<decimal> CheckItemCodeAndWarehouseQty(string itemCode, string warehouseCode)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("A moment ...");
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "CheckItemCodeAndWarehouseQty",
                    QueryItemCode = itemCode,
                    QueryItemWhsCode = warehouseCode
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Transfer");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (isSuccess)
                {
                    var bagReplied = JsonConvert.DeserializeObject<Cio>(content);
                    if (bagReplied == null) return 0;
                    if (bagReplied.oITW == null) return 0;

                    return bagReplied.oITW.OnHand;
                }

                //ELSE
                var bRequest = JsonConvert.DeserializeObject<BRequest>(content);
                lastErrMessage = $"{lastErrMessage}\n{bRequest?.Message}";
                DisplayAlert($"{lastErrMessage}");
                return 0;
            }
            catch (Exception excep)
            {
                Console.WriteLine($"{excep}");
                DisplayAlert(excep.Message);
                return 0;
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Handler to warehouse operation
        /// </summary>
        public void ManageToWhs()
        {
            if (ToOWhs.BinActivat.Equals("Y"))
            {
                // Show list of bin for parking the batch and serial
                // serial is serial trasfer
                // batch is qty transfer

                var returnAddrs = "20201005T2120_BinCode";
                MessagingCenter.Subscribe<List<CommonStockInfo>>(this, returnAddrs, (List<CommonStockInfo> selected) =>
                {
                    MessagingCenter.Unsubscribe<List<CommonStockInfo>>(this, returnAddrs);
                    SelectedStockList = selected;
                });

                return;
            }

            // handle non bin acitvated warehouse
            // normal warehouse
            if (SelectedStockList == null)
            {
                SelectedStockList.ForEach(x => x.ToWhs = this.ToOWhs.WhsCode);
            }
        }

        /// <summary>
        /// Display messag onto the screen
        /// </summary>
        /// <param name="message"></param>
        async void DisplayAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "OK");

        public void Dispose() {}// GC.Collect();
    }
}
