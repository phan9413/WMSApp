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
    public class SerialListVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Command CmdDone { get; set; }
        public Command CmdCancel { get; set; }
        public Command CmdSearchVisible { get; set; }
        public Command CmdStartScan { get; set; }
        public Command CmdAutoSelect { get; set; }
        public Command CmdAutoCancel { get; set; }

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

        OSRQ_Ex selectedItem;
        public OSRQ_Ex SelectedItem
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

        public string SearchQuery
        {
            set => HandlerSearchQuery(value);
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

        List<OSRQ_Ex> itemsSource;
        public ObservableCollection<OSRQ_Ex> ItemsSource { get; set; }

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
        string returnAddress;
        
        readonly string returnCameraAddrs = "20201018T1529_startScanner";
        int controlLimit;
        int neededQty;
        int remainingQty;
        int allocatedQty;
        List<string> selectedSerial;

        /// <summary>
        /// Constructor
        /// </summary>
        public SerialListVM(INavigation navigation, string returnAddr, string itemCod,  string itemNam,
            string whsCode, int ctrlLmt,List<string> selectedSerials)
        {
            Navigation = navigation;
            returnAddress = returnAddr;
            WhsCode = whsCode;
            ItemCode = itemCod;
            ItemName = itemNam;

            controlLimit = ctrlLmt;

            NeededQtyDisplay = $"Needed Qty {ctrlLmt:N}";
            neededQty = ctrlLmt;
            remainingQty = ctrlLmt;
            allocatedQty = 0;

            selectedSerial = selectedSerials;
            IsRemainQtyVisible = (neededQty > 0);

            InitCmd();
            LoadServerData();
        }

        /// <summary>
        /// Calcualte the remain
        /// </summary>
        void CalculateRemaining()
        {
            if (itemsSource == null)
            {
                allocatedQty = 0;
                remainingQty = neededQty;
                
                AllocatedDisplay = $"Allocated {allocatedQty:N}";
                RemainDisplay = $"{remainingQty:N} Remain";
                return;
            }

            if (itemsSource.Count == 0)
            {
                allocatedQty = 0;
                remainingQty = neededQty;

                AllocatedDisplay = $"Allocated {allocatedQty:N}";
                RemainDisplay = $"{remainingQty:N} Remain";
                return;
            }

            allocatedQty = itemsSource.Where(x => x.IsChecked).Count();
            remainingQty = neededQty - allocatedQty;

            AllocatedDisplay = $"Allocated {allocatedQty:N}";
            RemainDisplay = $"{remainingQty:N} Remain";
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
                    request = "GetSerialInWhs",
                    TransferItemCode = itemCode,
                    TransferWhsCode = whsCode
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

                if (repliedCio.TransferContentSerial == null)
                {
                    DisplayMessage($"{ItemCode}\n{itemName} Quantity falls into negative inventory, in warehouse {whsCode}");
                    MessagingCenter.Send<List<OSRQ_Ex>>(new List<OSRQ_Ex>(), this.returnAddress);
                    PopScreen();
                    return;
                }

                if (repliedCio.TransferContentSerial.Length == 0)
                {
                    DisplayMessage($"{ItemCode}\n{itemName} Quantity falls into negative inventory, in warehouse {whsCode}");
                    MessagingCenter.Send<List<OSRQ_Ex>>(new List<OSRQ_Ex>(), returnAddress);
                    PopScreen();
                    return;
                }

                var SerialList = repliedCio.TransferContentSerial;
                itemsSource = new List<OSRQ_Ex>();
                if (selectedSerial == null)
                {
                    SerialList.ForEach(x => x.IsChecked = false);                    
                    itemsSource.AddRange(SerialList);

                    // check needed qty can be full full or not 
                    if (neededQty > itemsSource.Count)
                    {
                        DisplayMessage($"Item {itemCode}\nInsufficient needed quantity {neededQty:N} " +
                            $"from warehouse {whsCode}, available qty {itemsSource.Count:N}, please select other warehouse and try again, Thanks");
                        MessagingCenter.Send<List<OSRQ_Ex>>(new List<OSRQ_Ex>(), returnAddress);
                        PopScreen();
                        return;
                    }
                    CalculateRemaining();
                    RefreshListView();
                    return;
                }

                if (selectedSerial.Count == 0)
                {
                    SerialList.ForEach(x => x.IsChecked = false);                    
                    itemsSource.AddRange(SerialList);

                    // check needed qty can be full full or not 
                    if (neededQty > itemsSource.Count)
                    {
                        DisplayMessage($"Item {itemCode}\nInsufficient needed quantity {neededQty:N} " +
                            $"from warehouse {whsCode}, available qty {itemsSource.Count:N}, please select other warehouse and try again, Thanks");
                        MessagingCenter.Send<List<OSRQ_Ex>>(new List<OSRQ_Ex>(), returnAddress);
                        PopScreen();
                        return;
                    }
                    CalculateRemaining();
                    RefreshListView();
                    return;
                }

                // loop each selected serial and add into the item source
                // prevent duplicated serial number
                itemsSource.AddRange(repliedCio.TransferContentSerial);
                itemsSource = itemsSource.Where(x => !selectedSerial.Contains(x.DistNumber)).ToList();
                itemsSource.ForEach(x => x.IsChecked = false);

                // check needed qty can be full full or not 
                if (neededQty > itemsSource.Count)
                {
                    DisplayMessage($"Item {itemCode}\nInsufficient needed quantity {neededQty:N} " +
                        $"from warehouse {whsCode}, available qty {itemsSource.Count:N}, " +
                        $"please select other warehouse and try again, Thanks");
                    MessagingCenter.Send<List<OSRQ_Ex>>(new List<OSRQ_Ex>(), returnAddress);
                    PopScreen();
                    return;
                }

                // reset the list to false
                itemsSource.ForEach(x => x.IsChecked = false);

                CalculateRemaining();
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
            CmdAutoSelect = new Command<bool>((bool bvalue) => AutoSelect(bvalue));
        }

        /// <summary>
        /// Auto select
        /// </summary>
        void AutoSelect(bool bvalue)
        {
            if (neededQty == 0) return;
            if (itemsSource == null) return;
            if (itemsSource.Count == 0) return;

            // uncheck all item in the list
            itemsSource.ForEach(x => { x.IsChecked = false; x.CellColor = Color.White; });

            // check based on the first to needed qty
            int checkQty = (neededQty <= ItemsSource.Count) ? neededQty : ItemsSource.Count;
            for (int x = 0; x < checkQty; x++)
            {
                if (bvalue)
                {
                    itemsSource[x].IsChecked = true;
                    itemsSource[x].CellColor = Color.LightGreen;
                }
                else
                {
                    itemsSource[x].IsChecked = false;
                    itemsSource[x].CellColor = Color.White;
                }
                //if (bvalue) SelectItemCell(itemsSource[x], bvalue);
                //else SelectItemCell(itemsSource[x], false);
            }
            CalculateRemaining();
        }

        /// <summary>
        /// Handler search bar visible and focus
        /// </summary>
        /// <param name="sb"></param>
        void HandlerSearchBarVisible(SearchBar sb)
        {
            if (sb == null) return;
            sb.IsVisible = !sb.IsVisible;
            if (sb.IsVisible) sb.Focus();
        }

        /// <summary>
        /// handler start camera scan
        /// </summary>
        void HandlerStartCameraScan()
        {
            MessagingCenter.Subscribe<string>(this, returnCameraAddrs, (string code) =>
            {
                MessagingCenter.Unsubscribe<string>(this, returnCameraAddrs);
                if (string.IsNullOrWhiteSpace(code)) return;
                if (code.ToLower().Equals("code")) return;
                if (itemsSource == null) return;
                if (itemsSource.Count == 0) return;

                var lowercase = code.ToLower();
                var isFound = itemsSource
                    .Where(x => x.DistNumber != null && x.DistNumber.ToLower().Equals(lowercase)).FirstOrDefault();

                if (isFound == null)
                {
                    DisplayToastShortMessage($"Scan in code {code} no found in existing list. Please try again, Thanks");
                    return;
                }
                HandlerItemSelected(isFound); // make the serial ticked
            });

            Navigation.PushAsync(new CameraScanView(returnCameraAddrs, "Scan Serial#")); // start a scanner 
        }

        /// <summary>
        /// Check and uncheck the selected item from the list
        /// </summary>
        /// <param name="selected"></param>
        void HandlerItemSelected(OSRQ_Ex selected)
        {
            try
            {
                if (selected == null) return;
                if (itemsSource == null) return;
                if (itemsSource.Count == 0) return;

                if (selected.IsChecked) // if checked tyhen un checked 
                {
                    SelectItemCell(selected, false);
                    return;
                }

                // remove the limit control of transfer
                //var checkedCount = itemsSource.Count(x => x.IsChecked);
                //if (controlLimit <= checkedCount) // <--- control the remaining
                //{
                //    DisplayMessage($"The needed qty {controlLimit:N} was fulfilled, please tap DONE, and complete the step.");
                //    return;
                //}

                SelectItemCell(selected, true);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Select item cell
        /// </summary>
        /// <param name="locId"></param>
        void SelectItemCell(OSRQ_Ex selected, bool isSetTrue)
        {
            if (itemsSource == null) return;
            if (itemsSource.Count == 0) return;

            int locId = itemsSource.IndexOf(selected);
            if (locId < 0) return;

            if (isSetTrue)
            {
                itemsSource[locId].IsChecked = true;
                itemsSource[locId].CellColor = Color.LightGreen;
                CalculateRemaining();
                return;
            }

            itemsSource[locId].IsChecked = false;
            itemsSource[locId].CellColor = Color.White;
            CalculateRemaining(); 
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
                var filter = itemsSource.Where(x => x.DistNumber.ToLower().Contains(lowerCaseQuery)).ToList();

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

                ItemsSource = new ObservableCollection<OSRQ_Ex>(filter);
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
            ItemsSource = new ObservableCollection<OSRQ_Ex>(itemsSource);
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
                MessagingCenter.Send<List<OSRQ_Ex>>(selectedlist, returnAddress);
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

