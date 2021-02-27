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
using Xamarin.Forms;

namespace WMSApp.ViewModels.Transfer
{
    public class SimpleBinListVM : IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string pName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pName));
        public void Dispose() { }// => GC.Collect();

        public Command CmdScanToBinCode { get; set; }
        public Command CmdSave { get; set; }
        public Command CmdClose { get; set; }

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

        FTS_vw_IMApp_ItemWhsBin selectedItem;
        public FTS_vw_IMApp_ItemWhsBin SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;
                CaptureBinToQty(selectedItem);
                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        List<FTS_vw_IMApp_ItemWhsBin> itemsSource;
        public ObservableCollection<FTS_vw_IMApp_ItemWhsBin> ItemsSource { get; set; }

        public Command CmdSerachBarVisible { get; set; }

        public string SearchText
        {
            set => HandlerSearch(value);
        }

        decimal needQty;
        public decimal NeedQty // based on selected from bin qty
        {
            get => needQty;
            set
            {
                if (needQty == value) return;
                needQty = value;
                OnPropertyChanged(nameof(NeedQty));
            }
        }
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

        INavigation Navigation;
        int fromWhsAbsEntry { get; set; }
        string returnAddress { get; set; }
        decimal sumAllocated { get; set; }
        decimal openRemain { get; set; }

        public SimpleBinListVM(INavigation navigation, string title, string whsCode, decimal fromBinNeedQty, int fromWhsAbs, string retAddress)
        {
            Navigation = navigation;
            Title = $"{title} {whsCode}";
            returnAddress = retAddress;
            NeedQty = openRemain = fromBinNeedQty;
            fromWhsAbsEntry = fromWhsAbs;

            LoadWarehouseBin(whsCode);
            InitCmd();
        }

        /// <summary>
        /// Capture Bin To Qty
        /// </summary>
        async void CaptureBinToQty(FTS_vw_IMApp_ItemWhsBin selected)
        {
            try
            {
                // prepare the init value 
                //var initValue = (openRemain <= selected.OnHandQty) ? openRemain : selected.OnHandQty;
                // save into list view 
                // then compile when save
                if (itemsSource == null) return;
                int locId = itemsSource.IndexOf(selected);
                if (locId < 0)
                {
                    DisplayToast("Can not locate the item in list, please try again later. Thanks");
                    return;
                }

                // second click to reset the zero
                if (itemsSource[locId].TransferQty > 0)
                {
                    itemsSource[locId].TransferQty = 0;
                    //itemsSource[locId].BinAbs = -1; // from warehouse bin
                    //itemsSource[locId].ToBinAbs = -1; // to warehouse bin
                    itemsSource[locId].IsChecked = false;
                    itemsSource[locId].SelectedColr = Color.White;
                    RecountRemain();
                    return;
                }

                if (openRemain == 0)
                {
                    DisplayToast("Needed qty are fulfilled, please SAVE (at top right of the screen) for futher processing, on tap selected BIN to reselect other. Thanks");
                    return;
                }

                string inputQty = await new Dialog()
                       .DisplayPromptAsync($"Input Transfer-To Bin: {selected.BinCode}, @<{selected.WhsCode}>"
                       , "", "OK", "Cancel", null, -1, Keyboard.Default, $"{openRemain:N}");

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

                if (openRemain < result)
                {
                    DisplayAlert("Insufficient Bin Qty");
                    return;
                }

                itemsSource[locId].TransferQty = result;
                itemsSource[locId].BinAbs = fromWhsAbsEntry; // from warehouse bin               
                itemsSource[locId].IsChecked = true;
                itemsSource[locId].SelectedColr = Color.Wheat;
                RecountRemain();
            }
            catch (Exception excep)
            {
                Console.WriteLine($"{excep}");
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// ON screen display the from bin need qty 
        /// with selected to bin qty
        /// </summary>
        void RecountRemain()
        {
            if (itemsSource == null) return;

            // update the sum qty 
            sumAllocated = itemsSource.Where(x => x.IsChecked == true).Sum(x => x.TransferQty);
            openRemain = needQty - sumAllocated;
            SelectedSumQty = $"Allocated {sumAllocated:N} / {openRemain:N} remain";
        }

        /// <summary>
        /// Init command 
        /// </summary>
        void InitCmd()
        {
            CmdSerachBarVisible = new Command<SearchBar>((SearchBar sb) =>
            {
                sb.IsVisible = !sb.IsVisible;
                if (sb.IsVisible) sb.Focus();
            });

            CmdScanToBinCode = new Command(LaunchScanner);
            CmdSave = new Command(Save);
            CmdClose = new Command(() => Navigation.PopAsync());
        }

        /// <summary>
        /// Send the selected to warehouse bin to from Bin screen 
        /// </summary>
        void Save()
        {
            try
            {
                if (itemsSource == null) return;
                if (openRemain > 0)
                {
                    DisplayToast($"There are still remain Qty {openRemain}");
                    return;
                }

                var selectList = itemsSource.Where(x => x.IsChecked == true && x.TransferQty > 0).ToList();
                if (selectList == null)
                {
                    DisplayToast("There is no [TO] bin selected, please select at least on [TO] Bin, and save. Thanks");
                    return;
                }

                MessagingCenter.Send(selectList, returnAddress); // send the list to [FROM] bin
                Navigation.PopAsync(); // close the screen
            }
            catch (Exception excep)
            {
                Console.WriteLine($"{excep}");
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Launch Scanner 
        /// </summary>
        async void LaunchScanner()
        {
            try
            {
                string returnAddress = "LaunchScanScreenToBinCode";
                MessagingCenter.Subscribe<string>(this, returnAddress, (string binCode) =>
                {
                    MessagingCenter.Unsubscribe<string>(this, returnAddress);
                    if (string.IsNullOrWhiteSpace(binCode)) return;
                    if (binCode.Equals("cancel")) return;
                    if (itemsSource == null)
                    {
                        DisplayToast("There is no bin configured, please try again later.");
                        return;
                    }

                    var foundBin = itemsSource.Where(x => x.BinCode.Equals(binCode)).FirstOrDefault();
                    if (foundBin == null)
                    {
                        DisplayToast($"The scanned in {binCode} no found, please try again later.");
                        return;
                    }

                    SelectedItem = foundBin;
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
        /// Handle the search text
        /// </summary>
        /// <param name="query"></param>
        void HandlerSearch(string query)
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

                var lowercase = query.ToLower();
                var filterList = ItemsSource.Where(x =>
                    x.BinCode.ToLower().Contains(lowercase)).ToList();

                if (filterList == null)
                {
                    RefreshListView();
                    return;
                }

                if (filterList.Count == 0)
                {
                    RefreshListView();
                    return;
                }

                ItemsSource = new ObservableCollection<FTS_vw_IMApp_ItemWhsBin>(filterList);
                OnPropertyChanged(nameof(ItemsSource));
            }
            catch (Exception excep)
            {
                Console.WriteLine($"{excep}");
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Load warehouse bin details 
        /// </summary>
        async void LoadWarehouseBin(string WarehouseCode)
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
                    request = "GetWarehouseBins",
                    QueryWhs = WarehouseCode
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
                    if (repliedCio.DtoBins == null) return;

                    itemsSource = new List<FTS_vw_IMApp_ItemWhsBin>();
                    foreach (var bin in repliedCio.DtoBins) // convert bin to app inner app object 
                    {
                        if (bin.AbsEntry == fromWhsAbsEntry) continue; // to prevent same wharecode
                        itemsSource.Add(new FTS_vw_IMApp_ItemWhsBin
                        {
                            BinCode = bin.BinCode,
                            WhsCode = bin.WhsCode,
                            TransferQty = 0,
                            BinAbs = bin.AbsEntry,
                            ToBinAbs = bin.AbsEntry,
                            IsChecked = false,
                            BatchSerial = string.Empty
                        });
                    }

                    if (itemsSource.Count == 0)
                    {
                        DisplayToast("There is no [TO] bin in selection list, " +
                            "please select different [FROM] bin, the [From] and [Bin] can not be same value. Thanks");
                        return;
                    }

                    RefreshListView();
                    RecountRemain();
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

        void RefreshListView()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<FTS_vw_IMApp_ItemWhsBin>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
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
