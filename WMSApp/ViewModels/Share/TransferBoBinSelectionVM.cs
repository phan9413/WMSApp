using Acr.UserDialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.Share;
using WMSApp.Views.Share;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Share
{
    public class TransferBoBinSelectionVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Command CmdSearchVisible { get; set; }
        public Command CmdUpdate { get; set; }
        public Command CmdStartScanner { get; set; }

        CommonStockInfo selectedItem;
        public CommonStockInfo SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;

                if (itemsSource != null && itemsSource.Count > 0)
                {
                    if (selectedItem.IsChecked) goto Reset;
                    if (remainingQty <= 0)
                    {
                        DisplayToast("Needed Qty fulfilled. Please tap update to complete the action. Thanks");
                        goto RefreshScreen;
                    }

                Reset:
                    int locId = itemsSource.IndexOf(selectedItem);
                    itemsSource[locId].IsChecked = !itemsSource[locId].IsChecked;
                    CalculateSelectedQty();
                }

            // handler the selected item
            RefreshScreen:

                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        List<CommonStockInfo> itemsSource;
        public ObservableCollection<CommonStockInfo> ItemsSource { get; set; }
        bool searchBarIsVisble;
        public bool SearchBarIsVisble
        {
            get => searchBarIsVisble;
            set
            {
                if (searchBarIsVisble == value) return;
                searchBarIsVisble = value;
                OnPropertyChanged(nameof(SearchBarIsVisble));
            }
        }

        public string SearchText { set => HandlerSearch(value); }

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

        decimal neededQty;
        public decimal NeededQty
        {
            get => neededQty;
            set
            {
                if (neededQty == value) return;
                neededQty = value;
                remainingQty = value;
                OnPropertyChanged(nameof(NeededQty));
            }
        }
        string pageTitle;
        public string PageTitle
        {
            get => pageTitle;
            set
            {
                if (pageTitle == value) return;
                pageTitle = value;
                OnPropertyChanged(PageTitle);
            }
        }

        decimal remainingQty;
        string ReturnAddr;
        readonly string WebApiAddress = "Transfer";

        INavigation Navigation;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="returnAddr"></param>
        public TransferBoBinSelectionVM(INavigation navigation, List<CommonStockInfo> details,
            string returnAddr, string itemCode, string warehouse, decimal neededQty, string pgTitle, string request)
        {
            SearchBarIsVisble = false;
            ReturnAddr = returnAddr;
            NeededQty = neededQty;
            NeededQtyDisplay = $"Selected 0 / {NeededQty}";
            Navigation = navigation;
            itemsSource = details;
            PageTitle = pgTitle;
            LoadDataFromSvr(request, itemCode, warehouse);

            InitCmd();
        }

        /// <summary>
        /// Refresh the screen qty
        /// </summary>
        void CalculateSelectedQty()
        {
            if (itemsSource == null) return;
            if (itemsSource.Count == 0) return;

            var count = itemsSource.Count(x => x.IsChecked == true);
            remainingQty = neededQty - count;
            NeededQtyDisplay = $"Selected {count:N} / {remainingQty:N} Remaining";
        }

        /// <summary>
        /// Init command
        /// </summary>
        void InitCmd()
        {
            CmdSearchVisible = new Command<SearchBar>((SearchBar searchBar) =>
            {
                searchBar.IsVisible = !searchBar.IsVisible;
                if (searchBar.IsVisible) searchBar.Focus();
            });

            CmdStartScanner = new Command(StartCamera);

            //CmdAutoSelect = new Command()
            CmdUpdate = new Command(Update);
        }

        /// <summary>
        /// Start camera capture barcode
        /// </summary>
        async void StartCamera()
        {
            var returnAddrs = "20201005T1801_startCamera";
            MessagingCenter.Subscribe<string>(this, returnAddrs, (string barcode) =>
            {
                MessagingCenter.Unsubscribe<string>(this, returnAddrs);
                HandlerSearch(barcode);
                return;
            });

            await Navigation.PushAsync(new CameraScanView(returnAddrs));
        }

        /// <summary>
        /// Return the selected List
        /// </summary>
        void Update()
        {
            if (itemsSource == null)
            {
                PopScreen();
                return;
            }

            List<CommonStockInfo> returnList = itemsSource.Where(x => x.IsChecked).ToList();
            if (returnList == null)
            {
                PopScreen();
                return;
            }

            if (returnList.Count == 0)
            {
                PopScreen();
                return;
            }

            if (remainingQty != 0)
            {
                DisplayAlert($"There is remaining {remainingQty}, please select more or tap back to return.");
                return;
            }

            MessagingCenter.Send(returnList, ReturnAddr);
            PopScreen();
        }

        /// <summary>
        /// Pop the screeen
        /// </summary>
        void PopScreen() => Navigation.PopAsync();

        /// <summary>
        /// Handle the query search
        /// </summary>
        /// <param name="query"></param>
        void HandlerSearch(string query)
        {
            try
            {
                if (itemsSource == null) return;

                if (string.IsNullOrWhiteSpace(query))
                {
                    RefreshListView();
                    return;
                }

                var lowercaseQuery = query.ToLower();
                var filter = itemsSource.Where(x => x.DistNumber.ToLower().Contains(query)).ToList();
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

                ItemsSource = new ObservableCollection<CommonStockInfo>(filter);
                OnPropertyChanged(nameof(ItemsSource));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Load warehouse stock details from the server
        /// </summary>
        /// <param name="request"></param>
        /// <param name="itemCode"></param>
        /// <param name="warehouse"></param>
        async void LoadDataFromSvr(string request, string itemCode, string warehouse)
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
                    request = request,
                    QueryItemCode = itemCode,
                    QueryItemWhsCode = warehouse
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, WebApiAddress);
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
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Refresh the list view binding
        /// </summary>
        void RefreshListView()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<CommonStockInfo>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        async void DisplayAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "OK");

        void DisplayToast(string message) => DependencyService.Get<IToastMessage>()?.ShortAlert(message);
    }
}
