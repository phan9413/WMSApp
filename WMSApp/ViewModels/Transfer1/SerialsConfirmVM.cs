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
    public class SerialsConfirmVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Command CmdSave { get; set; }
        public Command CmdCancel { get; set; }
        public Command CmdSearchBarVisible { get; set; }
        public Command CmdScanSerial { get; set; }
        public Command CmdSelect { get; set; }

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
            set
            {
                HandlerSearchQuery(value);
            }
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

        readonly string _ScanSerialAddr = "20201021T1524_ScanSerialA";
        INavigation Navigation { get; set; } = null;
        //int currentLineDocEntry { get; set; } = -1;        
        string returnAddress { get; set; } = string.Empty;
        Guid currentLineDocGuid { get; set; } = default;
        Guid currentLineDocLineGuid { get; set; } = default;

        /// <summary>
        /// Constructor
        /// </summary>
        public SerialsConfirmVM(INavigation navigation, Guid headGuid, Guid lineGuid,
            string itemCod, string itemNam, string whsCode, string returnAddr)
        {
            Navigation = navigation;
            currentLineDocGuid = headGuid;
            currentLineDocLineGuid = lineGuid;

            WhsCode = whsCode;
            returnAddress = returnAddr;

            ItemCode = itemCod;
            ItemName = itemNam;

            LoadSerials();
            InitCmd();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SerialsConfirmVM(INavigation navigation, List<string> serials,
            string itemCod, string itemNam, string whsCode, string returnAddr)
        {
            Navigation = navigation;

            WhsCode = whsCode;
            returnAddress = returnAddr;

            ItemCode = itemCod;
            ItemName = itemNam;

            //LoadSerials();
            itemsSource = new List<zwaTransferDocDetailsBin>();
            serials.ForEach(x => itemsSource.Add(new zwaTransferDocDetailsBin
            {
                ItemCode = itemCod,
                Serial = x,
                Qty = 1
            }));

            RefreshListView();
            InitCmd();
        }

        /// <summary>
        /// Init command
        /// </summary>
        void InitCmd()
        {
            CmdSearchBarVisible = new Command<SearchBar>((SearchBar sb) =>
            {
                sb.IsVisible = !sb.IsVisible;
                if (sb.IsVisible)
                {
                    sb.Focus();
                    sb.Placeholder = "Input Serial# to search";
                }
            });

            CmdSave = new Command(Save);
            CmdCancel = new Command(Close);
            CmdScanSerial = new Command(ScanSerial);
            CmdSelect = new Command<string>((string param) =>
            {
                if (itemsSource == null) return;
                if (itemsSource.Count == 0) return;

                if (param.Equals("Select"))
                {
                    itemsSource.ForEach(x => x.IsChecked = true);
                    return;
                }

                // set each ischeck to false
                itemsSource.ForEach(x => { x.IsChecked = false; x.Reset(); });
            });
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

                if (itemsSource[locId].IsChecked)
                {
                    itemsSource[locId].IsChecked = false;
                    return;
                }

                itemsSource[locId].IsChecked = true;
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

                var lowerCase = query.ToLower();
                var filter = itemsSource.Where(x => x.Serial != null && x.Serial.ToLower().Contains(lowerCase)).ToList();

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
        void ScanSerial()
        {
            try
            {
                MessagingCenter.Subscribe<string>(this, _ScanSerialAddr, (string serial) =>
                {
                    MessagingCenter.Unsubscribe<string>(this, _ScanSerialAddr);

                    if (string.IsNullOrWhiteSpace(serial)) return;
                    if (serial.ToLower().Equals("cancel")) return;

                    // handler capture serial number
                    //HandlerSearchQuery(serial);

                    if (itemsSource == null)
                    {
                        DisplayToast("Item list is empty, search can not perform"); return;
                    }

                    if (itemsSource.Count == 0)
                    {
                        DisplayToast("Item list is empty, search can not perform"); return;
                    }

                    var lowerCase = serial.ToLower();
                    var found = itemsSource.Where(x => x.Serial != null && x.Serial.ToLower().Equals(lowerCase)).FirstOrDefault();
                    if (found == null)
                    {
                        DisplayToast($"Scan in code {serial} not found in the list");
                        return;
                    }

                    int locid = itemsSource.IndexOf(found);
                    if (locid >= 0)
                    {
                        HandlerItemSelected(itemsSource[locid]);
                        return;
                    }
                });

                Navigation.PushAsync(new CameraScanView(_ScanSerialAddr, "Scan Serial#"));
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
            try
            {
                // <List<zwaTransferDocDetailsBin>>
                if (itemsSource == null) Close();
                if (itemsSource.Count == 0) Close();

                var checkList = itemsSource.Where(x => x.IsChecked).ToList();
                if (checkList == null)
                {
                    DisplayMessage("Some serial(s) not being allocated the bin, Please allocate all serial(s) to some bins, Thanks.");
                    return;
                }

                if (checkList.Count == 0)
                {
                    DisplayMessage("Some serial(s) not being allocated the bin, Please allocate all serial(s) to some bins, Thanks.");
                    return;
                }

                // zwaTransferDocDetailsBin
                MessagingCenter.Send(checkList, returnAddress); /// return check serial only
                Close();
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
        async void LoadSerials()
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
                    request = "GetSerialsList",
                    TransferDocRequestGuid = currentLineDocGuid,
                    TransferDocRequestGuidLine = currentLineDocLineGuid
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

        /// <summary>
        /// display toast
        /// </summary>
        /// <param name="message"></param>
        void DisplayToast(string message) => DependencyService.Get<IToastMessage>()?.ShortAlert(message);
    }
}
