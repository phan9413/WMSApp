using Acr.UserDialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.SAP;
using WMSApp.Views.Share;
using WMSApp.Views.Transfer;
using Xamarin.Forms;
namespace WMSApp.ViewModels.Transfer
{
    public class SelectBinListVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string pName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pName));

        #region Page view binding property
        public Command CmdSave { get; set; }
        public Command CmdCancel { get; set; }
        public Command CmdShowSearchBar { get; set; }
        public Command CmdScanFrmBinCode { get; set; }

        List<zwaTransferDetails> selectedItemWhsBin;

        FTS_vw_IMApp_ItemWhsBin selectedItem;
        public FTS_vw_IMApp_ItemWhsBin SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;

                HandlerSelectedItem(selectedItem);

                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        List<FTS_vw_IMApp_ItemWhsBin> itemsSource;
        public ObservableCollection<FTS_vw_IMApp_ItemWhsBin> ItemsSource { get; set; }

        string selectedSumQty;
        public string SelectedSumQty
        {
            get => selectedSumQty;
            set
            {
                if (selectedSumQty == value) return;
                selectedSumQty = value;
                OnPropertyChanged(nameof(SelectedSumQty));
            }
        }

        decimal needQty;
        public decimal NeedQty
        {
            get => needQty;
            set
            {
                if (needQty == value) return;
                needQty = value;
                OnPropertyChanged(nameof(NeedQty));
            }
        }

        public string SearchBarText
        {
            set => HandlerSerachText(value);
        }

        bool isVisibleSearchBar;
        public bool IsVisibleSearchBar
        {
            get => isVisibleSearchBar;
            set
            {
                if (isVisibleSearchBar == value) return;
                isVisibleSearchBar = value;
                OnPropertyChanged(nameof(IsVisibleSearchBar));
            }
        }

        string title;
        public string Title
        {
            get => title;
            set
            {
                if (title == value) return;
                title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        #endregion

        /// <summary>
        /// Private declaration
        /// </summary>
        INavigation Navigation;
        WTQ1_Ex currentLine;
        string returnAddres;
        bool isFrmWhsBinActivated;
        bool isToWhsBinActivated;
        decimal sumAllocated;
        decimal openRemain;
        int currentListViewId = -1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="navigation"></param>
        /// <param name="selectedLine"></param>
        /// <param name="returnAddress"></param>
        public SelectBinListVM(INavigation navigation, WTQ1_Ex selectedLine, string returnAddress, string title)
        {
            Navigation = navigation;
            currentLine = selectedLine;
            openRemain = NeedQty = currentLine.me.OpenQty;

            returnAddres = returnAddress;
            Title = title;
            InitCmd();

            isFrmWhsBinActivated = IsWhsBinActivated(currentLine.RequestFromWarehouse);
            if (isFrmWhsBinActivated)
            {
                GetItemWhsBinServer(currentLine.RequestFromWarehouse);
                return;
            }

            isToWhsBinActivated = IsWhsBinActivated(currentLine.RequestToWarehouse);
            if (isToWhsBinActivated)
            {
                GetItemWhsBinServer(currentLine.RequestFromWarehouse);
                return;
            }
        }

        /// <summary>
        /// return true or false when request line warehouse is bin activated 
        /// </summary>
        /// <param name="WarehouseCode"></param>
        /// <returns></returns>
        bool IsWhsBinActivated(string WarehouseCode)
        {
            if (App.Warehouses == null) return false;
            var requestFromWhs = App.Warehouses.Where(x => x.WhsCode.Equals(WarehouseCode)).FirstOrDefault();
            if (requestFromWhs == null) return false;
            return (requestFromWhs.BinActivat.Equals("Y"));
        }

        /// <summary>
        /// Init command
        /// </summary>
        void InitCmd()
        {
            CmdSave = new Command(Save);
            CmdCancel = new Command(Cancel);
            CmdShowSearchBar = new Command<SearchBar>((SearchBar searb) => HandlerSearhBarVisibilities(searb));
            CmdScanFrmBinCode = new Command(LaunchScanScreen);
        }

        /// <summary>
        /// Launch the scanner to scan bin code
        /// </summary>
        async void LaunchScanScreen()
        {
            try
            {
                string returnAddress = "LaunchScanScreenBinCode";
                MessagingCenter.Subscribe<string>(this, returnAddress, (string binCode) =>
                {
                    MessagingCenter.Unsubscribe<string>(this, returnAddress);
                    if (string.IsNullOrWhiteSpace(binCode)) return;
                    if (binCode.Equals("cancel")) return;

                    var selectedItem = itemsSource.Where(x => x.BinCode == binCode).FirstOrDefault();
                    if (selectedItem == null)
                    {
                        DisplayToast($"Invalid bin code scanned {binCode}, please try again later");
                        return;
                    }

                    HandlerSelectedItem(selectedItem);
                    return;
                });

                await Navigation.PushAsync(new CameraScanView(returnAddress));
            }
            catch (Exception excep)
            {
                Console.WriteLine($"{excep}");
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Handler SearhBar Visibilities on user view
        /// </summary>
        /// <param name="searchBar"></param>
        void HandlerSearhBarVisibilities(SearchBar searchBar)
        {
            searchBar.IsVisible = !searchBar.IsVisible;
            if (searchBar.IsVisible) searchBar.Focus();
        }

        /// <summary>
        /// Search Bin code
        /// </summary>
        /// <param name="query"></param>
        void HandlerSerachText(string query)
        {
            if (itemsSource == null) return;
            if (string.IsNullOrWhiteSpace(query))
            {
                RefreshListView();
                return;
            }

            var lowerCase = query.ToLower();
            var filterList = itemsSource.Where(x =>
                   x.BinCode.ToLower().Contains(lowerCase) ||
                   x.WhsCode.ToLower().Contains(lowerCase) ||
                   x.ItemCode.ToLower().Contains(lowerCase) ||
                   x.ItemName.ToLower().Contains(lowerCase)
                   //x.BatchSerial.ToLower().Contains(lowerCase)
                   ).ToList();

            if (filterList == null)
            {
                RefreshListView();
                return;
            }

            ItemsSource = new ObservableCollection<FTS_vw_IMApp_ItemWhsBin>(filterList);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// Load from server
        /// </summary>
        async void GetItemWhsBinServer(string warehouse)
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
                    request = "GetItemWhsBin",
                    QueryItemCode = currentLine.SelectedOITM.ItemCode,
                    QueryItemWhsCode = warehouse
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
                    var repliedCio = JsonConvert.DeserializeObject<Cio>(content);
                    if (repliedCio == null) return;
                    if (repliedCio.ItemWhsBinList == null) return;

                    itemsSource = new List<FTS_vw_IMApp_ItemWhsBin>(repliedCio.ItemWhsBinList);
                    RefreshListView();
                    return;
                }
                //ELSE
                var bRequest = JsonConvert.DeserializeObject<BRequest>(content);
                lastErrMessage = $"{lastErrMessage}\n{bRequest?.Message}";
                DisplayAlert($"{lastErrMessage}");
            }
            catch (Exception excep)
            {
                Console.WriteLine($"{excep}");
                DisplayAlert(excep.Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Refresh the user page view
        /// </summary>
        void RefreshListView()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<FTS_vw_IMApp_ItemWhsBin>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// Handler the bin cell tap by user
        /// </summary>
        /// <param name="selected"></param>
        async void HandlerSelectedItem(FTS_vw_IMApp_ItemWhsBin selected)
        {
            if (itemsSource == null) return;
            if (selected.TransferQty > 0) // reset to zero
            {
                int locId_ = itemsSource.IndexOf(selected);
                if (locId_ < 0) return;
                itemsSource[locId_].IsChecked = !itemsSource[locId_].IsChecked;
                openRemain += itemsSource[locId_].TransferQty;
                itemsSource[locId_].TransferQty = 0;
                itemsSource[locId_].SelectedColr = Color.White;
                RecountRemain();

                if (selectedItemWhsBin == null) return;
                if (selectedItemWhsBin.Count == 0) return;

                int id_ = selectedItemWhsBin.IndexOf(selectedItemWhsBin.Where(x => x.ItemCode == selected.ItemCode).FirstOrDefault());
                if (id_ >= 0)
                {
                    selectedItemWhsBin.RemoveAt(id_);
                }
                return;
            }

            if (selected.OnHandQty == 0)
            {
                DisplayToast("Insuficient Bin Qty for needed quantity");
                return;
            }

            if (openRemain == 0)
            {
                DisplayToast("Needed qty are fulfilled, please SAVE (at top right of the screen) for futher processing, on tap selected BIN to reselect other. Thanks");
                return;
            }

            // prepare the init value 
            var initValue = (openRemain <= selected.OnHandQty) ? openRemain : selected.OnHandQty;

            string inputQty = await new Dialog()
                .DisplayPromptAsync($"Input Transfer Qty for {selected.ItemCode}, <FROM> {currentLine.RequestFromWarehouse}>",
                        $"{selected.ItemName}", "OK", "Cancel", null, -1, Keyboard.Default, $"{initValue:N}");

            if (string.IsNullOrWhiteSpace(inputQty)) return;
            if (inputQty.ToLower().Equals("cancel")) return;

            var isNumeric = decimal.TryParse(inputQty, out decimal result);
            if (!isNumeric)
            {
                DisplayAlert("Please enter value numeric value");
                return;
            }

            if (result <= 0)
            {
                DisplayAlert("Please enter positive numeric value");
                return;
            }

            if (selected.OnHandQty < result)
            {
                DisplayAlert("Insufficient Bin Qty");
                return;
            }

            int locId = itemsSource.IndexOf(selected);
            currentListViewId = itemsSource.IndexOf(selected); // class golbal variable to indicate the current list view item

            if (locId < 0) return;
            itemsSource[locId].IsChecked = !itemsSource[locId].IsChecked;
            itemsSource[locId].TransferQty = result;
            itemsSource[locId].ToBinAbs = -1; // assume to warehouse is non in warehouse
            itemsSource[locId].SelectedColr = Color.Wheat;

            // check to warehouse is bin activated
            if (isToWhsBinActivated)
            {
                LaunchSelectToWarehouseBin(locId, result, itemsSource[locId].BinAbs);
            }

            RecountRemain();
        }

        void RecountRemain()
        {
            // update the sum qty 
            sumAllocated = itemsSource.Sum(x => x.TransferQty);
            openRemain = needQty - sumAllocated;
            SelectedSumQty = $"Allocated {sumAllocated:N} / {openRemain:N} remain";
        }

        /// <summary>
        /// Laucnh a screen to select the to warehouse bin code
        /// </summary>
        async void LaunchSelectToWarehouseBin(int locId, decimal neededQty, int fromWhsAbs)
        {
            var returnAddress = "LaunchSelectToWarehouseBin";
            MessagingCenter.Subscribe<List<FTS_vw_IMApp_ItemWhsBin>>(this, returnAddress, (List<FTS_vw_IMApp_ItemWhsBin> selectedToWhsBinList) =>
                {
                    MessagingCenter.Unsubscribe<FTS_vw_IMApp_ItemWhsBin>(this, returnAddress);

                    // update the 
                    //itemsSource[locId].ToBinAbs = selectedToWhsBin.AbsEntry;
                    // check filfull needed qty
                    //if (IsQtyFulfilled()) Save();                    
                    // else stay at the screen

                    // save int the from whs objec kept in code behind
                    //selectedToWhsBinList.ForEach(x => x.BinAbs = itemsSource[locId].BinAbs);
                    if (currentListViewId >= 0)
                    {
                        itemsSource[currentListViewId].ToWhsAbsList = selectedToWhsBinList;
                    }
                    else
                    {
                        DisplayToast("Error updating the bin info, please try again later. Thanks");
                    }

                    return;
                });


            await Navigation.PushAsync(
                new SimpleBinListView("To BIN", currentLine.RequestToWarehouse, neededQty, fromWhsAbs, returnAddress));
        }

        /// <summary>
        /// Check the need qty are fillfull and auto close ans save
        /// </summary>
        /// <returns></returns>
        //bool IsQtyFulfilled ()
        //{
        //    var sumQtyTransfer = itemsSource.Sum(x => x.TransferQty);
        //    return (needQty <= sumQtyTransfer);
        //}

        /// <summary>
        /// Create and save the bin lits from screen
        /// </summary>
        void CreateBinTransferDetails()
        {
            if (itemsSource == null) return;
            var binsList = itemsSource.Where(x => x.IsChecked == true && x.TransferQty > 0).ToList();
            if (selectedItemWhsBin == null) selectedItemWhsBin = new List<zwaTransferDetails>();

            foreach (var bin in binsList)
            {
                if (bin.ToWhsAbsList == null)
                {
                    selectedItemWhsBin.Add(new zwaTransferDetails
                    {
                        ItemCode = bin.ItemCode,
                        ToBinAbs = bin.ToBinAbs,
                        FromBinAbs = bin.BinAbs,
                        FromWhsCode = bin.WhsCode,
                        ToWhsCode = currentLine.RequestToWarehouse,
                        BatchNo = string.Empty,
                        SerialNo = string.Empty,
                        TransferQty = bin.TransferQty,
                        SourceDocNum = -2,
                        SourceDocEntry = currentLine.me.DocEntry,
                        SourceDocBaseType = Convert.ToInt32(currentLine.me.ObjType),
                        SourceBaseEntry = currentLine.me.DocEntry,
                        SourceBaseLine = currentLine.me.LineNum
                    });
                    continue;
                }

                // handle multiple bin to multiple bin
                foreach (var toBinList in bin.ToWhsAbsList)
                {
                    var newLine = new zwaTransferDetails
                    {
                        ItemCode = bin.ItemCode,
                        FromBinAbs = bin.BinAbs,
                        ToBinAbs = toBinList.ToBinAbs,
                        FromWhsCode = bin.WhsCode,

                        ToWhsCode = currentLine.RequestToWarehouse,
                        BatchNo = bin.BatchSerial,
                        SerialNo = bin.BatchSerial,
                        TransferQty = toBinList.TransferQty,
                        SourceDocNum = -2,
                        SourceDocEntry = currentLine.me.DocEntry,
                        SourceDocBaseType = Convert.ToInt32(currentLine.me.ObjType),
                        SourceBaseEntry = currentLine.me.DocEntry,
                        SourceBaseLine = currentLine.me.LineNum
                    };

                    selectedItemWhsBin.Add(newLine);
                }
            }
        }

        /// <summary>
        /// Save by sending the selected bin back to caller screen
        /// via messaging center
        /// </summary>
        async void Save()
        {
            CreateBinTransferDetails();
            MessagingCenter.Send(selectedItemWhsBin, returnAddres);

            DisplayToast("Bin information saved");
            await Navigation.PopAsync();
        }

        /// <summary>
        /// Close the view, and send null to caller.
        /// Caller site will cancel the messaging center subscribe
        /// </summary>
        async void Cancel()
        {
            MessagingCenter.Send(new List<zwaTransferDetails>(), returnAddres);
            await Navigation.PopAsync();
        }

        /// <summary>
        /// Display alart on screen
        /// </summary>
        /// <param name="message"></param>
        async void DisplayAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "Ok");

        /// <summary>
        /// Display toast
        /// </summary>
        /// <param name="message"></param>
        void DisplayToast(string message) => DependencyService.Get<IToastMessage>()?.ShortAlert(message);

    }
}
