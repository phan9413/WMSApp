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
using WMSApp.Views.Share;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Transfer1
{
    public class SerialToBinSelectVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

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

        public Command CmdScanBinCode { get; set; }
        public Command CmdSearchVisible { get; set; }
        public string SearchText
        {
            set => HandlerSearch(value);
        }

        OBIN selectedItemBin;
        public OBIN SelectedItemBin /// support 1 selected item
        {
            get => selectedItemBin;
            set
            {
                if (selectedItemBin == value) return;
                selectedItemBin = value;

                MessagingCenter.Send(selectedItemBin, returnAddress);

                selectedItemBin = null;
                OnPropertyChanged(nameof(SelectedItemBin));
                Navigation.PopAsync();
            }
        }

        List<OBIN> itemsSourceBin;
        public ObservableCollection<OBIN> ItemsSourceBin { get; set; }

        string returnAddress;
        
        INavigation Navigation;
        readonly string returnAddr = "20201022T0913_StartBinCodeScanner";
        List<zwaTransferDocDetailsBin> fromSerialsBin;

        /// <summary>
        /// Constructor
        /// </summary>
        public SerialToBinSelectVM (INavigation navigation, 
            string whsCode, string retAddrs, List<zwaTransferDocDetailsBin> serials)
        {
            Navigation = navigation;
            returnAddress = retAddrs;
            WhsCode = whsCode;
            fromSerialsBin = serials;
            LoadWarehouseBin();
            InitCmd();
        }

        /// <summary>
        /// Init the command
        /// </summary>
        void InitCmd()
        {
            CmdScanBinCode = new Command(StartScanner);
            CmdSearchVisible = new Command<SearchBar>((SearchBar sb) =>
            {
                sb.IsVisible = !sb.IsVisible; 
                if (sb.IsVisible)
                {
                    sb.Focus();
                    sb.Placeholder = "Input Bin code to search";
                }
            });
        }

        /// <summary>
        /// Start the scanner screen
        /// </summary>
        void StartScanner ()
        {
            try
            {
                MessagingCenter.Subscribe<string>(this, returnAddr, (string binCode) =>
                {
                    MessagingCenter.Unsubscribe<string>(this, returnAddr);

                    if (string.IsNullOrWhiteSpace(binCode)) return;
                    if (binCode.ToLower().Equals("cancel")) return;

                    var foundBin = itemsSourceBin.Where(x => x.BinCode != null && x.BinCode.ToLower().Equals(binCode)).FirstOrDefault();
                    if (foundBin == null)
                    {
                        DisplayToast($"Input {binCode} no found. Please try again, Thanks");
                        return;
                    }

                    SelectedItemBin = foundBin;
                });

                Navigation.PushAsync(new CameraScanView(returnAddress, "Scan Bin#"));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Handle the search 
        /// </summary>
        /// <param name="query"></param>
        void HandlerSearch(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    RefreshBinListView();
                    return;
                }

                if (itemsSourceBin == null)
                {
                    RefreshBinListView();
                    return;
                }

                if (itemsSourceBin.Count == 0)
                {
                    RefreshBinListView();
                    return;
                }

                var lowerCaseQuery = query.ToLower();
                var filter = itemsSourceBin.Where(x => x.BinCode != null && x.BinCode.ToLower().Contains(lowerCaseQuery)).ToList();
                if (filter == null)
                {
                    RefreshBinListView();
                    return;
                }
                if (filter.Count == 0)
                {
                    RefreshBinListView();
                    return;
                }

                // bind the filter list on the screen
                ItemsSourceBin = new ObservableCollection<OBIN>(filter);
                OnPropertyChanged(nameof(ItemsSourceBin));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Load the warehouse content bin
        /// </summary>
        async void LoadWarehouseBin ()
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
                    request = "GetWhsBins",
                    QueryWhs = whsCode
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

                Cio replied = JsonConvert.DeserializeObject<Cio>(content);
                if (replied == null)
                {
                    DisplayToast("Error getting information from server, please try again later. Thanks");
                    return;
                }

                if (replied.WarehouseBin == null)
                {
                    DisplayToast("Error getting information from server, please try again later [x1]. Thanks");
                    return;
                }

                if (replied.WarehouseBin.Length == 0)
                {
                    DisplayToast("Error getting information from server, please try again later [x2]. Thanks");
                    return;
                }

              //  itemsSourceBin = new List<OBIN>(replied.WarehouseBin);
                int[] fileterBins = fromSerialsBin.Select(x => x.BinAbs).Distinct().ToArray();

                itemsSourceBin = (from item in replied.WarehouseBin
                                  where !fileterBins.Contains(item.AbsEntry) select item).ToList();
                
                RefreshBinListView();
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
        /// Refresh the list view bin
        /// </summary>
        void RefreshBinListView ()
        {
            if (itemsSourceBin == null) return;
            if (itemsSourceBin.Count == 0) return;
            ItemsSourceBin = new ObservableCollection<OBIN>(itemsSourceBin);
            OnPropertyChanged(nameof(ItemsSourceBin));            
        }

        /// <summary>
        /// Display Alert message
        /// </summary>
        /// <param name="message"></param>
        async void DisplayMessage(string message) => await new Dialog().DisplayAlert("Alert", message, "OK");

        void DisplayToast(string message) => DependencyService.Get<IToastMessage>()?.ShortAlert(message);
    }
}
