using Acr.UserDialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.Transfer1;
using WMSApp.Views.Share;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace WMSApp.ViewModels.Transfer1
{
    public class BinBatchListVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;        
        protected void OnPropertyChanged(string name) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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

        OBBQ_Ex selectedItem;
        public OBBQ_Ex SelectedItem
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

        public string SearchQuery
        {
            set => HandlerSearchQuery(value);
        }

        List<OBBQ_Ex> itemsSource;
        public ObservableCollection<OBBQ_Ex> ItemsSource { get; set; }

        string neededQtyDisplay;
        public string NeededQtyDisplay
        {
            get => neededQtyDisplay;
            set
            {
                if (neededQtyDisplay == value) return;
                neededQtyDisplay = value;
                OnPropertyChanged(nameof(NeededQtyDisplay));
            }
        }

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
        readonly string ReturnCameraAddrs = "20201019T1512_startScanner";
        INavigation Navigation;
        string ReturnAddress;       
        decimal NeededQty, RemainingQty, AllocateQty;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public BinBatchListVM(
            INavigation navigation, string returnAddr, string itemCod, 
            string itemNam, string whsCode, decimal neededQty)
        {
            Navigation = navigation;
            ReturnAddress = returnAddr;
            WhsCode = whsCode;
            ItemCode = itemCod;
            ItemName = itemNam;

            NeededQty = neededQty;
            RemainingQty = neededQty;
            NeededQtyDisplay = $"Needed Qty {neededQty:n}";

            IsRemainQtyVisible = (neededQty > 0);

            InitCmd();
            LoadServerData();
        }

        /// <summary>
        /// Use to calculate the allocated Qty
        /// </summary>
        void CalculateAllocated()
        {
            if (itemsSource == null) return;
            if (itemsSource.Count == 0) return;

            AllocateQty = itemsSource.Sum(x => x.TransferBatchQty);
            RemainingQty = NeededQty - AllocateQty;            

            AllocatedDisplay = $"Allocated {AllocateQty:N}";
            RemainDisplay = $"{RemainingQty:N} Remain";
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
                    request = "GetBinBatchAccumulators",
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

                if (repliedCio.TransferBinContentBatch == null)
                {
                    DisplayMessage($"{ItemCode}\n{itemName} Quantity falls into negative inventory, in warehouse {whsCode}");
                    MessagingCenter.Send<List<OBBQ_Ex>>(new List<OBBQ_Ex>(), ReturnAddress);
                    PopScreen();
                    return;
                }

                if (repliedCio.TransferBinContentBatch.Length ==0)
                {
                    DisplayMessage($"{ItemCode}\n{itemName} Quantity falls into negative inventory, in warehouse {whsCode}");
                    MessagingCenter.Send<List<OBBQ_Ex>>(new List<OBBQ_Ex>(), ReturnAddress);
                    PopScreen();
                    return;
                }
               
                var BinBatchList = repliedCio.TransferBinContentBatch;
                BinBatchList.ForEach(x => x.IsChecked = false);

                if (itemsSource == null) itemsSource = new List<OBBQ_Ex>();
                itemsSource.AddRange(BinBatchList);

                CalculateAllocated();
                RefreshListView();
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
                return;
            }

            itemsSource.ForEach(x => x.IsChecked = false);
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
                sb.Placeholder = "Enter Batch# or Bin#";
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

                var tolower = code.ToLower();

                var isFound = itemsSource
                        .Where(x => x.DistNumber != null && x.DistNumber.ToLower().Equals(tolower)).FirstOrDefault();

                if (isFound == null)
                {
                    DisplayToastShortMessage($"Scan in code {code} no found in existing list, please try again. Thanks");
                    return;
                }

                HandlerItemSelected(isFound); // make the serial ticked
            });

            // start a scanner 
            Navigation.PushAsync(new CameraScanView(ReturnCameraAddrs, "Scan Batch#"));
        }

        /// <summary>
        /// Check and uncheck the selected item from the list
        /// </summary>
        /// <param name="selected"></param>
        async void HandlerItemSelected(OBBQ_Ex selected)
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
                    itemsSource[locId0].IsChecked = false;
                    itemsSource[locId0].TransferBatchQty = 0;
                    itemsSource[locId0].SelectedColor = Color.White;
                    return; // uncheck the check
                }

                decimal intialValue = selected.OnHandQty;

                //decimal intialValue = (RemainingQty >= selected.OnHandQty) ? selected.OnHandQty : RemainingQty;
                //intialValue = (RemainingQty < 0) ? selected.OnHandQty : 0;

                //if (intialValue == 0)
                //{
                //    DisplayMessage($"The needed qty {NeededQty:N} already fulfilled, " +
                //        "please uncheck some batch(es), or tap DONE to complete this step, Thanks");
                //    return;
                //}

                // prompt to capture qty
                string qty = await new Dialog().DisplayPromptAsync($"Input Item  {selected.ItemCode} transfer Qty, ",
                    $"Batch# {selected.DistNumber}, @ OnHand Qty {selected.OnHandQty:N}", "Ok", "Cancel"
                    , null
                    , -1
                    , Keyboard.Numeric
                    , $"{intialValue:N}");

                if (string.IsNullOrWhiteSpace(qty)) return;

                if (qty.ToLower().Equals("cancel")) return;
                bool isNumeric = Decimal.TryParse(qty, out decimal result);
                if (!isNumeric)
                {
                    DisplayMessage($"Invalid transfer qty input, Item {selected.ItemCode} @ Batch#{selected.DistNumber}, " +
                        $"Please try again later. Thanks");
                    return;
                }

                if (selected.OnHandQty < result)
                {
                    DisplayMessage($"Invalid transfer qty input, Item {selected.ItemCode} @ Batch#{selected.DistNumber}, " +
                       $"The batch On Hand {selected.OnHandQty:N} < inputed Qty {result:N}");
                    return;
                }

                if (result < 0)
                {
                    DisplayMessage($"Invalid transfer qty input, Item {selected.ItemCode} @ Batch#{selected.DistNumber}, " +
                       $"The inputed Qty {result:N} must be positive, Thanks.");
                    return;
                }

                // hide the checking for this control limit
                //decimal sumOfSelected = itemsSource.Sum(x => x.TransferBatchQty) + result;

                //if (sumOfSelected > NeededQty)
                //{
                //    DisplayMessage($"Invalid transfer qty input, Item {selected.ItemCode} @ Batch#{selected.DistNumber}, " +
                //        $"The selected qty {sumOfSelected:N} > {NeededQty:N}, " +
                //        $"Please try again later. Thanks");
                //    return;
                //}

                int locId = itemsSource.IndexOf(selected);
                if (locId < 0) return;
                itemsSource[locId].IsChecked = !itemsSource[locId].IsChecked;
                itemsSource[locId].TransferBatchQty = result;
                itemsSource[locId].SelectedColor = Color.Green;
                CalculateAllocated();
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

                ItemsSource = new ObservableCollection<OBBQ_Ex>(filter);
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
            ItemsSource = new ObservableCollection<OBBQ_Ex>(itemsSource);
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

                MessagingCenter.Send<List<OBBQ_Ex>>(selectedlist, ReturnAddress);
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
                if (itemsSource == null) return;
                if (itemsSource.Count == 0) return;

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
                    $"leaving screen without save, will lost the selected data.", 
                    "Leave", "Cancel");

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
        async void DisplayMessage(string message) => 
            await new Dialog().DisplayAlert("Alert", message, "Ok");

        /// <summary>
        /// Display short toast message
        /// </summary>
        /// <param name="message"></param>
        void DisplayToastShortMessage(string message) => 
            DependencyService.Get<IToastMessage>()?.ShortAlert(message);

        /// <summary>
        /// Diplay long toast message
        /// </summary>
        /// <param name="message"></param>
        void DisplayToastLongMessage(string message) => 
            DependencyService.Get<IToastMessage>()?.LongAlert(message);
    }
}

