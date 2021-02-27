using Acr.UserDialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.Inventory;
using WMSApp.Views.Share;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Inventory
{
    public class InventoryVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string pName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pName));

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

        public Command CmdScanItemCode { get; set; }
        public Command CmdShowBinContent { get; set; }
        public Command CmdInputItemCode { get; set; }

        DateTime startDate;
        public DateTime StartDate
        {
            get => startDate;
            set
            {
                if (startDate == value) return;
                startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        DateTime endDate;
        public DateTime EndDate
        {
            get => endDate;
            set
            {
                if (endDate == value) return;
                endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        BinContent selectedItem_BinContent;
        public BinContent SelectedItem_BinContent
        {
            get => selectedItem_BinContent;
            set
            {
                if (selectedItem_BinContent == value) return;
                selectedItem_BinContent = value;
                OnPropertyChanged(nameof(SelectedItem_BinContent));
            }
        }

        List<BinContent> itemsSource_BinContent;
        public ObservableCollection<BinContent> ItemsSource_BinContent { get; set; }

        StockTransactionLog selectedItem_StockTranLog;
        public StockTransactionLog SelectedItem_StockTranLog
        {
            get => selectedItem_StockTranLog;
            set
            {
                if (selectedItem_StockTranLog == value) return;
                selectedItem_StockTranLog = value;
                OnPropertyChanged(nameof(SelectedItem_StockTranLog));
            }
        }

        List<StockTransactionLog> itemsSource_StockTranLogs;
        public ObservableCollection<StockTransactionLog> ItemsSource_StockTranLogs { get; set; }


        bool isBinContentVisible;

        public bool IsBinContentVisible
        {
            get => isBinContentVisible; 
            set
            {
                if (isBinContentVisible == value) return;
                isBinContentVisible = value;
                OnPropertyChanged(nameof(IsBinContentVisible));
            }
        }

        bool isStockTransLogVisible;
        public bool IsStockTransLogVisible
        {
            get => isStockTransLogVisible;
            set
            {
                if (isStockTransLogVisible == value) return;
                isStockTransLogVisible = value;
                OnPropertyChanged(nameof(IsStockTransLogVisible));
            }
        }

        INavigation Navigation;
        /// <summary>
        /// The constructor
        /// </summary>
        public InventoryVM(INavigation navigation)
        {
            Navigation = navigation;

            StartDate =  new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // set first day of the month 
            EndDate = DateTime.Now;

            IsBinContentVisible = true;
            IsStockTransLogVisible = false;

           // StartTimer(); // start a task to delay to start camera
            InitCmd();
        }

        /// <summary>
        /// Start the camera scanner
        /// </summary>
        void InitCmd ()
        {
            CmdScanItemCode = new Command(StartScanner);
            CmdInputItemCode = new Command(CaptureItemCode);
            CmdShowBinContent = new Command<string>( (string tapType) => 
            {
                switch (tapType)
                {
                    case "BinContent":
                        {
                            IsBinContentVisible = true;
                            IsStockTransLogVisible = false;
                            break;
                        }
                    case "StockTransLog":
                        {
                            IsBinContentVisible = false;
                            IsStockTransLogVisible = true;
                            break;
                        }
                }
            });
        }

        /// <summary>
        /// Start Timer
        /// </summary>
        void StartTimer()
        {
            // to launch the scanner after constructor
            Device.StartTimer(TimeSpan.FromSeconds(1.2), () =>
            {
                StartScanner();
                return false; // return true to continue
            });
        }

        /// <summary>
        /// Start the scanner when button press
        /// </summary>
        void StartScanner()
        {
            try
            {
                var returnAddress = "InventoryVM_GetScanner";
                MessagingCenter.Subscribe<string>(this, returnAddress, (string itemCode) =>
                {
                    if (string.IsNullOrWhiteSpace(itemCode))
                    {
                        DisplayToast("No item code scan or not recognised input, please try again. Thanks");                        
                        return;
                    }

                    if (itemCode.ToLower().Equals("cancel"))return;                    

                    // load the item from server
                    ItemCode = itemCode;
                    LoadItemCodeFromServer(itemCode);
                });

                Navigation.PushAsync(new CameraScanView(returnAddress));
            }
            catch (Exception excep)
            {
                LogException(excep.ToString(), excep.Message);
            }
        }

        /// <summary>
        /// Show dialog to capture the item code
        /// </summary>
        async void CaptureItemCode ()
        {
            try
            {
                string inputItemCode = await new Dialog().DisplayPromptAsync("Input Item Code", "", "Ok", "Cancel");
                if (string.IsNullOrWhiteSpace(inputItemCode))
                {
                    DisplayToast("Invalid Item code input, please try again");
                    return;
                }

                if (inputItemCode.ToLower().Equals("cancel"))
                {                    
                    return;
                }

                ItemCode = inputItemCode;
                LoadItemCodeFromServer(inputItemCode);
            }
            catch (Exception excep)
            {
                LogException(excep.ToString(), excep.Message);
            }
        }


        /// <summary>
        /// Load the item coce detail from the server
        /// </summary>
        async void LoadItemCodeFromServer(string itemCode)
        {
            try
            {
                //public BinContent[] dtoBinContents { get; set; }
                //public StockTransactionLog[] dtoStockTransLogs { get; set; }
                //public DateTime QueryStartDate { get; set; }
                //public DateTime QueryEndDate { get; set; }

                UserDialogs.Instance.ShowLoading("A moment please...");
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "GetBinContentList",
                    QueryItemCode = itemCode,
                    QueryStartDate = startDate,
                    QueryEndDate = endDate
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Items");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {                                      
                    DisplayAlert($"{content}");
                    return;
                }

                var retBag = JsonConvert.DeserializeObject<Cio>(content);
                if (retBag == null)
                {
                    DisplayToast("Error reading server information, please tray again later");
                    return;
                }

                if (retBag.dtoBinContents == null && retBag.dtoStockTransLogs  == null)
                {
                    DisplayAlert($"There is no item bin information found for {itemCode}, please try again later,Thanks.");
                    return;
                }

                if (retBag.dtoBinContents.Count() == 0 && retBag.dtoStockTransLogs.Count() == 0)
                {
                    DisplayAlert($"There is no item bin information found for {itemCode}, please try again later,Thanks.");
                    return;
                }

                // bin content
                if (retBag.dtoBinContents == null)
                {
                    DisplayToast("Error reading server information (Bin content), please tray again later");
                    return;
                }

                itemsSource_BinContent = new List<BinContent>();
                itemsSource_BinContent.AddRange(retBag.dtoBinContents);

                // stock trans log
                if (retBag.dtoStockTransLogs == null)
                {
                    DisplayToast("Error reading server information (Bin content), please tray again later");
                    return;
                }

                itemsSource_StockTranLogs = new List<StockTransactionLog>();
                itemsSource_StockTranLogs.AddRange(retBag.dtoStockTransLogs);
                ResetListView();
            }
            catch (Exception excep)
            {
                LogException(excep.ToString(), excep.Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Refresh the list
        /// </summary>
        void ResetListView()
        {
            if (itemsSource_BinContent != null)
            {
                ItemsSource_BinContent = new ObservableCollection<BinContent>(itemsSource_BinContent);
                OnPropertyChanged(nameof(ItemsSource_BinContent));
            }

            if (itemsSource_StockTranLogs != null)
            {
                ItemsSource_StockTranLogs = new ObservableCollection<StockTransactionLog>(itemsSource_StockTranLogs);
                OnPropertyChanged(nameof(ItemsSource_StockTranLogs));
            }
        }

        /// <summary>
        /// Simplified the log exception message
        /// </summary>
        /// <param name="longMessage"></param>
        /// <param name="shortMessage"></param>
        void LogException (string longMessage, string shortMessage)
        {
            Console.WriteLine(longMessage);
            DisplayAlert(shortMessage);
        }

        async void DisplayAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "OK");
        void DisplayToast(string message) => DependencyService.Get<IToastMessage>()?.ShortAlert(message);
    }
}
