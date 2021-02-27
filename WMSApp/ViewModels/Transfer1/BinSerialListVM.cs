using Acr.UserDialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Views.Share;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Transfer1
{
    /// <summary>
    /// Show list of the available bin and serial for user selection
    /// </summary>
    public class BinSerialListVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Command CmdDone { get; set; }
        public Command CmdCancel { get; set; }
        public Command CmdSearchVisible { get; set; }
        public Command CmdStartScan { get; set; }
        public Command CmdSelectAll { get; set; }

        string whsCode;
        public string WhsCode
        {
            get => whsCode;
            set
            {
                if (whsCode == value) return;
                whsCode = value;
                OnPropertyChanged(nameof(WhsCode));
            }
        }

        OSBQ_Ex selectedItem;
        public OSBQ_Ex SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;

                HandlerItemSelected(selectedItem);

                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        int neededQty;
        public int NeededQty
        {
            get => neededQty;
            set
            {
                if (neededQty == value) return;
                neededQty = value;
                OnPropertyChanged(nameof(NeededQty));
            }
        }

        string remainDisplay;
        public string RemainDisplay
        {
            get => remainDisplay;
            set
            {
                if (remainDisplay == value) return;
                remainDisplay = value;
                OnPropertyChanged(nameof(RemainDisplay));
            }
        }

        string allocatedDisplay;
        public string AllocatedDisplay
        {
            get => allocatedDisplay;
            set
            {
                if (allocatedDisplay == value) return;
                allocatedDisplay = value;
                OnPropertyChanged(nameof(AllocatedDisplay));
            }
        }


        public string SearchQuery
        {
            set => HandlerSearchQuery(value);
        }

        List<OSBQ_Ex> itemsSource;
        public ObservableCollection<OSBQ_Ex> ItemsSource { get; set; }

        string itemCode;
        public string ItemCode
        {
            get => itemCode;
            set
            {
                if (itemCode == value) return;
                itemCode = value;
                OnPropertyChanged(nameof(ItemCode));
            }
        }

        string itemName;
        public string ItemName
        {
            get => itemName;
            set
            {
                if (itemName == value) return;
                itemName = value;
                OnPropertyChanged(nameof(ItemName));
            }
        }

        bool isRemainQtyVisible;
        public bool IsRemainQtyVisible
        {
            get => isRemainQtyVisible;
            set
            {
                if (isRemainQtyVisible == value) return;
                isRemainQtyVisible = value;
                OnPropertyChanged(nameof(IsRemainQtyVisible));
            }
        }

        /// <summary>
        /// private declaration
        /// </summary>
        INavigation Navigation;
        string ReturnAddress;

        readonly string ReturnCameraAddrs = "20201018T1512_startScanner";
        int remaining, allocated;
        List<string> selectedSerials;

        /// <summary>
        /// Constructor
        /// </summary>
        public BinSerialListVM(INavigation navigation, string returnAddr,
            string itemCod, string itemNam, string whsCode, int neededQty, List<string> selectedSerial)
        {
            Navigation = navigation;
            ReturnAddress = returnAddr;
            WhsCode = whsCode;
            ItemCode = itemCod;
            ItemName = itemNam;

            IsRemainQtyVisible = (neededQty > 0);
            NeededQty = neededQty;
            remaining = neededQty;
            selectedSerials = selectedSerial;

            InitCmd();
            LoadServerData();
        }

        /// <summary>
        /// Calculate remaining
        /// </summary>
        void CalculateRemain()
        {
            allocated = itemsSource.Count(x => x.IsChecked);
            remaining = neededQty - allocated;
            RemainDisplay = $"{remaining:N} remain";
            AllocatedDisplay = $"Allocated {allocated:N}";
        }

        /// <summary>
        /// Load list of bin and serial number from server
        /// </summary>
        async void LoadServerData()
        {
            try
            {
                // checking with server on the serial number exsting in the warehouse
                UserDialogs.Instance.ShowLoading("A moment ...");
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "GetBinSerialAccumulators",
                    TransferItemCode = ItemCode,
                    TransferWhsCode = WhsCode
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Transfer1");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    DisplayMessage(content);
                    return;
                }

                var repliedCio = JsonConvert.DeserializeObject<Cio>(content);
                if (repliedCio == null)
                {
                    DisplayMessage("Error getting server message to process, please try again later. Thanks");
                    return;
                }

                if (repliedCio.TransferBinContentSerial == null)
                {
                    DisplayMessage($"{ItemCode}\n{itemName} Quantity falls into negative inventory, in warehouse {whsCode}");
                    MessagingCenter.Send<List<OSBQ_Ex>>(new List<OSBQ_Ex>(), ReturnAddress);
                    PopScreen();
                    return;
                }

                if (repliedCio.TransferBinContentSerial.Length == 0)
                {
                    DisplayMessage($"{ItemCode}\n{itemName} Quantity falls into negative inventory, in warehouse {whsCode}");
                    MessagingCenter.Send<List<OSBQ_Ex>>(new List<OSBQ_Ex>(), ReturnAddress);
                    PopScreen();
                    return;
                }

                var BinSerialList = repliedCio.TransferBinContentSerial;
                itemsSource = new List<OSBQ_Ex>();

                if (selectedSerials == null)
                {
                    itemsSource.AddRange(BinSerialList);
                    RefreshListView();
                    CalculateRemain();
                    return;
                }

                if (selectedSerials.Count == 0)
                {
                    itemsSource.AddRange(BinSerialList);
                    RefreshListView();
                    CalculateRemain();
                    return;
                }

                // compare selected list and bin serial list 
                // to prevent the serial being select twice     

                var binserials = new List<OSBQ_Ex>(BinSerialList);
                binserials = binserials.Where(x => !selectedSerials.Contains(x.DistNumber)).ToList();
                itemsSource.AddRange(binserials);

                // Show message on zero qty in the warehouse
                if (itemsSource.Count == 0)
                {
                    DisplayMessage($"Item {ItemCode} having zero qty in warehouse {WhsCode}," +
                        $"Or available serial(s) being selected / picked from by other line in same request doc, " +
                        $"please try other warehouse, Thanks");
                    Cancel();
                    return;
                }

                RefreshListView();
                CalculateRemain();
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Initial command linking
        /// </summary>
        void InitCmd()
        {
            CmdDone = new Command(Save);
            CmdCancel = new Command(Cancel);
            CmdSearchVisible = new Command<SearchBar>((SearchBar sb) => HandlerSearchBarVisible(sb));
            CmdStartScan = new Command(HandlerStartCameraScan);
            CmdSelectAll = new Command<string>((string param) => CheckItems(param));
        }

        /// <summary>
        /// Select all or uncheck all based on pass in parameter
        /// </summary>
        void CheckItems(string param)
        {
            if (itemsSource == null) return;
            if (itemsSource.Count == 0) return;

            if (param.Contains("SelectAll"))
            {
                itemsSource.ForEach(x => x.IsChecked = true);
                CalculateRemain();
                return;
            }

            itemsSource.ForEach(x => x.IsChecked = false);
            CalculateRemain();
        }

        /// <summary>
        /// Handler search bar visible and focus
        /// </summary>
        /// <param name="sb"></param>
        void HandlerSearchBarVisible(SearchBar sb)
        {
            if (sb == null) return;
            sb.IsVisible = !sb.IsVisible;
            if (sb.IsVisible)
            {
                sb.Placeholder = "Enter Serial# or Bin#";
                sb.Focus();
            }
        }

        /// <summary>
        /// handler start camera scan
        /// </summary>
        void HandlerStartCameraScan()
        {
            MessagingCenter.Subscribe<string>(this, ReturnCameraAddrs, (string code) =>
            {
                MessagingCenter.Unsubscribe<string>(this, ReturnCameraAddrs);
                if (string.IsNullOrWhiteSpace(code)) return;
                if (code.ToLower().Equals("cancel")) return;
                if (itemsSource == null) return;
                if (itemsSource.Count == 0) return;

                var lowercase = code.ToLower();
                var isFound = itemsSource
                    .Where(x => x.DistNumber != null &&  x.DistNumber.ToLower().Equals(lowercase)).FirstOrDefault();
                
                if (isFound == null)
                {
                    DisplayToastShortMessage($"Scan in code {code} no found in existing list, please try again. Thanks");
                    return;
                }

                HandlerItemSelected(isFound); // make the serial ticked
            });

            // start a scanner 
            Navigation.PushAsync(new CameraScanView(ReturnCameraAddrs, "Scan Serial#"));
        }

        /// <summary>
        /// Check and uncheck the selected item from the list
        /// </summary>
        /// <param name="selected"></param>
        void HandlerItemSelected(OSBQ_Ex selected)
        {
            try
            {
                if (selected == null) return;
                if (itemsSource == null) return;
                if (itemsSource.Count == 0) return;

                if (selected.IsChecked)
                {
                    int locId0 = itemsSource.IndexOf(selected);
                    if (locId0 < 0) return;
                    itemsSource[locId0].IsChecked = !itemsSource[locId0].IsChecked;
                    CalculateRemain();
                    return; // uncheck the check
                }

                // remove the control of the needed qty
                //var checkedCount = itemsSource.Count(Xamarin => Xamarin.IsChecked);
                //if (NeededQty <= checkedCount) // <--- control the remaining
                //{
                //    DisplayMessage($"The needed qty {NeededQty:N} was fulfilled, please tap DONE, and complete the step");
                //    return;
                //}

                int locId = itemsSource.IndexOf(selected);
                if (locId < 0) return;
                itemsSource[locId].IsChecked = !itemsSource[locId].IsChecked;
                CalculateRemain();
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Handle the search query from user screen
        /// </summary>
        /// <param name="query"></param>
        void HandlerSearchQuery(string query)
        {
            try
            {
                if (itemsSource == null) return;
                if (itemsSource.Count == 0) return;
                if (string.IsNullOrWhiteSpace(query))
                {
                    RefreshListView();
                    return;
                }

                string lowerCaseQuery = query.ToLower();
                var filter = itemsSource.Where(x =>
                                        x.BinCode.ToLower().Contains(lowerCaseQuery) ||
                                        x.DistNumber.ToLower().Contains(lowerCaseQuery)).ToList();

                if (filter == null)
                {
                    RefreshListView();
                    return;
                }

                if (filter.Count == 0)
                {
                    RefreshListView();
                    return;
                }

                ItemsSource = new ObservableCollection<OSBQ_Ex>(filter);
                OnPropertyChanged(nameof(ItemsSource));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Refresh the list view
        /// </summary>
        void RefreshListView()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<OSBQ_Ex>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// Send back the selected list to the caller
        /// </summary>
        void Save()
        {
            try
            {
                if (itemsSource == null) return;
                if (itemsSource.Count == 0) return;

                var selectedlist = itemsSource.Where(x => x.IsChecked).ToList();
                MessagingCenter.Send<List<OSBQ_Ex>>(selectedlist, ReturnAddress);
                PopScreen();
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Handle when user tap on cancel
        /// </summary>
        async void Cancel()
        {
            try
            {
                if (itemsSource == null)
                {
                    PopScreen();
                    return;
                }

                if (itemsSource.Count == 0)
                {
                    PopScreen();
                    return;
                }

                var selectedlist = itemsSource.Where(x => x.IsChecked).ToList();
                if (selectedlist == null)
                {
                    PopScreen();
                    return;
                }

                if (selectedlist.Count == 0)
                {
                    PopScreen();
                    return;
                }

                bool confirmLeave = await new Dialog().DisplayAlert("Confirm leaving?",
                    $"There are selected item in list ({selectedlist.Count} items), " +
                    $"leaving screen without save, will lost the selected data.", "Leave", "Cancel");

                if (confirmLeave) PopScreen();

            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Close the screen
        /// </summary>
        void PopScreen() => Navigation.PopAsync();

        /// <summary>
        /// Display message onto the screen
        /// </summary>
        /// <param name="message"></param>
        async void DisplayMessage(string message) => await new Dialog().DisplayAlert("Alert", message, "Ok");

        void DisplayToastShortMessage(string message) => DependencyService.Get<IToastMessage>()?.ShortAlert(message);

        void DisplayToastLongMessage(string message) => DependencyService.Get<IToastMessage>()?.LongAlert(message);
    }
}
