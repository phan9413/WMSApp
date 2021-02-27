using Acr.UserDialogs;
using DbClass;
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
using WMSApp.Views.Transfer1;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Transfer1
{
    public class ItemToBinVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region View binding        
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Command CmdSave { get; set; }
        public Command CmdCancel { get; set; }
        public Command CmdSearchBarVisible { get; set; }
        public Command CmdScanItem { get; set; }
        public Command CmdPickBins { get; set; }

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

        public string SearchQuery
        {
            set => HandlerSearchQuery(value);
        }

        zwaTransferDocDetailsBin selectedItem;
        public zwaTransferDocDetailsBin SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;

                HandlerItemSelected(value);

                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
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

        List<zwaTransferDocDetailsBin> itemsSource;
        public ObservableCollection<zwaTransferDocDetailsBin> ItemsSource { get; set; }
        #endregion

        #region private declaration        
        INavigation Navigation { get; set; } = null;
        Guid headerGuid { get; set; } = default;
        Guid lineGuid { get; set; } = default;
        string returnAddress { get; set; } = string.Empty;

        bool fromWhsVisible = false;
        List<string> Serials { get; set; } // for cater the GRN 

        readonly string _ScanItemAddr = "_ScanItemAddr_20201114T1322";
        readonly string _PickBin = "_PickBin_20201114T1322";

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public ItemToBinVM(INavigation navigation,
            Guid headGuid,
            Guid linGuid,
            string itemCod,
            string itemNam,
            string whsCode,
            string returnAddr)
        {
            Navigation = navigation;
            headerGuid = headGuid;
            lineGuid = linGuid;

            WhsCode = whsCode;
            returnAddress = returnAddr;
            ItemCode = itemCod;
            ItemName = itemNam;

            fromWhsVisible = true;

            LoadItemsBin();
            InitCmd();
        }


        /// <summary>
        /// Resue the same screen to full fill serial allocation to bin 
        /// for GRN process
        /// </summary>
        /// <param name="navigation"></param>
        /// <param name="serials"></param>
        /// <param name="itemCod"></param>
        /// <param name="itemNam"></param>
        /// <param name="whsCode"></param>
        /// <param name="returnAddr"></param>
        public ItemToBinVM(INavigation navigation,
           string itemCod,
           string itemNam,
           string whsCode,
           string returnAddr)
        {
            Navigation = navigation;
            WhsCode = whsCode;
            returnAddress = returnAddr;
            ItemCode = itemCod;
            ItemName = itemNam;
            InitCmd();
        }

        /// <summary>
        /// 
        /// </summary>
        void InitCmd()
        {
            CmdSearchBarVisible = new Command<SearchBar>((SearchBar sb) =>
            {
                sb.IsVisible = !sb.IsVisible;
                if (sb.IsVisible)
                {
                    sb.Focus();
                    sb.Placeholder = "Input Item Code# to search";
                }
            });

            CmdSave = new Command(Save);
            CmdCancel = new Command(Close);
            CmdScanItem = new Command(ScanItem);
            //CmdPickBins = new Command(SelectBin);
        }

        /// <summary>
        /// Launch screen for user to select entry 
        /// </summary>
        void SelectBin(zwaTransferDocDetailsBin selected)
        {
            if (itemsSource == null) return;
            if (itemsSource.Count == 0) return;

            MessagingCenter.Subscribe<List<OBIN_Ex>>(this, _PickBin, (List<OBIN_Ex> bins) =>
            {
                MessagingCenter.Unsubscribe<List<OBIN_Ex>>(this, _PickBin);
                // update the screen and kept until all item number fulfill
                // loop thru the record and update the bin information

                // perfrom update and show the select bin + qty onto the screen, and send back to caller 

                selected.SelectedBins = bins;
                int locid = itemsSource.IndexOf(selected);
                if (locid >=0)
                {
                    itemsSource[locid].IsChecked = false;
                    itemsSource[locid].ReceiptColor = Color.LightGreen;
                    
                    var showlist = String.Join("\n", bins.Select(x => $"{ x.BinQty:N} -> {x.oBIN.BinCode}").ToList());
                    itemsSource[locid].SelectedBinCode = "\n"+showlist;
                }

                //    itemsSource[x].SelectedBinCode = bins.BinCode;
                //    itemsSource[x].SelectedBin = bins;
                //    itemsSource[x].Direction = "TO";
                //    itemsSource[x].IsChecked = false;
                //    itemsSource[x].ManufacturerSerialNumber = string.Empty;
                //    itemsSource[x].InternalSerialNumber = string.Empty;
                //    itemsSource[x].ReceiptColor = Color.LightGreen;
                //}
            });
            
            var fromBinList = itemsSource.Select (x=>x.BinCode).Distinct().ToList();
            Navigation.PushAsync(new ItemToBinSelectView(whsCode, _PickBin, ItemCode, ItemName, selected.Qty, fromBinList));
        }

        /// <summary>
        /// Close the screen
        /// </summary>
        void Close() => Navigation.PopAsync();

        /// <summary>
        /// Check check te item
        /// </summary>
        /// <param name="selected"></param>
        void HandlerItemSelected(zwaTransferDocDetailsBin selected)
        {
            try
            {
                if (selected == null) return;
                if (itemsSource == null) return;
                if (itemsSource.Count == 0) return;

                int locId = itemsSource.IndexOf(selected);
                if (locId < 0) return;

                if (itemsSource[locId].SelectedBinCode != null && itemsSource[locId].SelectedBinCode.Length > 0)
                {
                    itemsSource[locId].IsChecked = false;
                    itemsSource[locId].Reset();
                    return;
                }

                if (itemsSource[locId].IsChecked)
                {
                    itemsSource[locId].IsChecked = false;
                    itemsSource[locId].Reset();
                }
                else
                {
                    itemsSource[locId].IsChecked = true;
                }

                SelectBin(selected); // launch the bin selection
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Filter the list based on the enter item
        /// </summary>
        /// <param name="query"></param>
        void HandlerSearchQuery(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    RefreshListView();
                    return;
                }

                if (itemsSource == null) return;
                if (itemsSource.Count == 0) return;

                var tolower = query.ToLower();
                var filter = itemsSource
                    .Where(x => x.Serial != null &&  x.Serial.ToLower().Contains(tolower)).ToList();
                
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

                ItemsSource = new ObservableCollection<zwaTransferDocDetailsBin>(filter);
                OnPropertyChanged(nameof(ItemsSource));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Scan serial 
        /// </summary>
        void ScanItem()
        {
            try
            {
                MessagingCenter.Subscribe<string>(this, _ScanItemAddr, (string item) =>
                {
                    MessagingCenter.Unsubscribe<string>(this, _ScanItemAddr);

                    if (string.IsNullOrWhiteSpace(item)) return;
                    if (item.ToLower().Equals("cancel")) return;

                    var foundItem = itemsSource.Where(x => x.ItemCode.ToLower().Equals(item.ToLower())).FirstOrDefault();
                    if (foundItem == null)
                    {
                        DisplayToast($"Input {item} no found. Please try again, Thanks");
                        return;
                    }

                    HandlerItemSelected(foundItem);

                    // handler capture serial number
                    //HandlerSearchQuery(serial);
                });

                Navigation.PushAsync(new CameraScanView(_ScanItemAddr, "Scan Item Code#"));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Return the serial list back to the caller
        /// </summary>
        void Save()
        {
            // <List<zwaTransferDocDetailsBin>>
            if (itemsSource == null) Close();
            if (itemsSource.Count == 0) Close();

            //var checkList = itemsSource.Where(x => x.SelectedBins != null).ToList();
            List<OBIN_Ex> checkList = new List<OBIN_Ex>();
            itemsSource.ForEach(x =>
            {
                if (x.SelectedBins != null)
                {
                    checkList.AddRange(x.SelectedBins);
                }
            });
            
            if (checkList == null)
            {
                DisplayMessage("Some serial(s) not being allocated the bin, Please allocate all serial(s) to some bins, Thanks [N].");
                return;
            }

            if (checkList.Count == 0)
            {
                DisplayMessage("Some serial(s) not being allocated the bin, Please allocate all serial(s) to some bins, Thanks.[Z]");
                return;
            }

            MessagingCenter.Send(checkList, returnAddress); // only return the set bin
            Close();
        }

        /// <summary>
        /// Load the serial string list to fill tye screen
        /// </summary>
        void SetupSerialList()
        {
            try
            {
                if (Serials == null)
                {
                    DisplayMessage("The serial setup list is empty, " +
                        "bin allocation setup can not process, please try again later, Thanks [N]");
                    return;
                }

                if (Serials.Count == 0)
                {
                    DisplayMessage("The serial setup list is empty, " +
                        "bin allocation setup can not process, please try again later, Thanks [Z]");
                    return;
                }

                itemsSource = new List<zwaTransferDocDetailsBin>();
                Serials.ForEach(x =>
                {
                    itemsSource.Add(new zwaTransferDocDetailsBin
                    {
                        ItemCode = this.itemCode,
                        Serial = x,
                        Batch = string.Empty,
                        InternalSerialNumber = string.Empty,
                        ManufacturerSerialNumber = string.Empty,
                        BinAbs = -1,
                        SnBMDAbs = -1,
                        Qty = 1,
                        WhsCode = this.whsCode,
                        Direction = "TO"
                    });
                });

                RefreshListView();
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Load list of serial for user to pick the destination warehouse
        /// </summary>
        async void LoadItemsBin()
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
                    request = "GetItemList", //<--- get list of the from bin for item
                    TransferDocRequestGuid = headerGuid,
                    TransferDocRequestGuidLine = lineGuid
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

                // handler 
                Cio repliedBag = JsonConvert.DeserializeObject<Cio>(content);
                if (repliedBag == null)
                {
                    DisplayMessage("Some error reading the server replied, please try again later, Thanks.");
                    return;
                }

                if (repliedBag.TransferDocDetailsBins == null)
                {
                    DisplayMessage("Some error reading the server replied, please try again later [01], Thanks.");
                    return;
                }

                itemsSource = new List<zwaTransferDocDetailsBin>(repliedBag.TransferDocDetailsBins);
                itemsSource.ForEach(x => x.FromWhsVisible = this.fromWhsVisible);

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
        /// Refresh list view
        /// </summary>
        void RefreshListView()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<zwaTransferDocDetailsBin>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// Display message onto user screen
        /// </summary>
        /// <param name="message"></param>
        async void DisplayMessage(string message) => await new Dialog().DisplayAlert("Alert", message, "Ok");

        void DisplayToast(string message) => DependencyService.Get<IToastMessage>()?.ShortAlert(message);
    }
}