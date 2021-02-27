using Acr.UserDialogs;
using DbClass;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Models.Share;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Share
{
    public class ItemWarehouseVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string pName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pName));
        public ObservableCollection<CommonStockInfo> ItemsSource { get; set; }
        public Command CmdVisibleSearch { get; set; }

        List<CommonStockInfo> itemsSource { get; set; }

        CommonStockInfo selectedItem;

        public CommonStockInfo SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                // Handle selected item 

                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public string SearchText { set => HandleSearch(value); }

        INavigation Navigation;
        OITM SelectedOITM;
        OWHS SelectedOWHS;
        bool IsWhsBinActivated = false;
        bool IsItemSerialManage = false;
        bool IsItemBatchManage = false;
        string ReturnAddress = string.Empty;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="warehouse"></param>
        public ItemWarehouseVM(INavigation navigation, OITM item, OWHS warehouse, string returnAddr)
        {
            SelectedOWHS = warehouse;
            SelectedOITM = item;

            IsItemBatchManage = item.ManSerNum.Equals("Y"); // SelectedOITM.ManSerNum.Equals("Y")
            IsItemBatchManage = item.ManBtchNum.Equals("Y"); // SelectedOITM.ManSerNum.Equals("Y")

            IsWhsBinActivated = SelectedOWHS.BinActivat.Equals("Y");
            Navigation = navigation;

            ReturnAddress = returnAddr;

            InitCmd();
            Init();
        }

        /// <summary>
        /// Handle the searchText
        /// </summary>
        void HandleSearch(string query)
        {
            try
            {
                if (itemsSource == null) return;
                if (string.IsNullOrWhiteSpace(query))
                {
                    RefreshListView();
                    return;
                }

                var lowerCaseQuery = query.ToLower();
                var filter = itemsSource.Where(x => 
                        x.ItemCode.Equals(lowerCaseQuery) ||
                        x.itemName.Equals(lowerCaseQuery) ||
                        x.DistNumber.Equals(lowerCaseQuery) ||
                        x.BinCode.Equals(lowerCaseQuery) ||
                        x.WhsCode.Equals(lowerCaseQuery)
                        ).ToList();

                if (filter == null)
                {
                    RefreshListView();
                    return;
                }

                ItemsSource = new ObservableCollection<CommonStockInfo>(filter);
                OnPropertyChanged(nameof(ItemsSource));
            }
            catch (Exception excep)
            {
                Console.WriteLine($"{excep}");
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Initial the command
        /// </summary>
        void InitCmd()
        {
            CmdVisibleSearch = new Command<SearchBar>((SearchBar searchbar) =>
            {
                searchbar.IsVisible = !searchbar.IsVisible;
                if (searchbar.IsVisible) searchbar.Focus();
            });
        }

        /// <summary>
        /// Initial check 
        /// </summary>
        void Init()
        {
            var actionName = string.Empty;
            if (IsWhsBinActivated && IsItemBatchManage) // bin with batch
            {
                actionName = "GetItemWhsBinBatch";
                GetItemWhsBinServer(actionName, SelectedOITM.ItemCode, SelectedOWHS.WhsCode);
                return;
            }

            if (IsWhsBinActivated && IsItemSerialManage)  // bin with serial
            {
                actionName = "GetItemWhsBinSerial";
                GetItemWhsBinServer(actionName, SelectedOITM.ItemCode, SelectedOWHS.WhsCode);
                return;
            }

            if (IsWhsBinActivated) // handle bin only
            {
                actionName = "GetItemWhsBin";
                GetItemWhsBinServer(actionName, SelectedOITM.ItemCode, SelectedOWHS.WhsCode);
                return;
            }

            if (IsItemBatchManage) // no bin batch only
            {
                // get list of listed avaialble batch 
                actionName = "GetItemWhsBinBatch";
                GetItemWhsBinServer(actionName, SelectedOITM.ItemCode, SelectedOWHS.WhsCode);
                return;
            }

            if (IsItemSerialManage) // no bin , serial only
            {
                actionName = "GetItemWhsBinSerial";
                GetItemWhsBinServer(actionName, SelectedOITM.ItemCode, SelectedOWHS.WhsCode);
                return;
            }

            // handle no qty to qty
            // prompt qty entry and 

        }

        /// <summary>
        /// Load from server
        /// </summary>
        async void GetItemWhsBinServer(string requestAction, string itemCode, string warehouse)
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
                    request = requestAction,
                    QueryItemCode = itemCode,
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
                    if (repliedCio.CommonStockInfos == null) return;

                    itemsSource = new List<CommonStockInfo>(repliedCio.CommonStockInfos);
                    RefreshListView();
                    return;
                }

                //ELSE
                var bRequest = JsonConvert.DeserializeObject<BRequest>(content);
                if (bRequest == null)
                {
                    DisplayAlert(lastErrMessage + "\n" + content);
                    return;
                }
                
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
        /// Rebind the listview data
        /// </summary>
        void RefreshListView()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<CommonStockInfo>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// Show alert message
        /// </summary>
        /// <param name="message"></param>
        async void DisplayAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "Ok");

    }
}
