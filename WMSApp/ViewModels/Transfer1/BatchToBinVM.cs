using Acr.UserDialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.GRPO;
using WMSApp.Models.SAP;
using WMSApp.Models.StandAloneTransfer;
using WMSApp.Views.Share;
using WMSApp.Views.Transfer1;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Transfer1
{
    public class BatchToBinVM : INotifyPropertyChanged
    {
        #region view property
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Command CmdSave { get; set; }
        public Command CmdCancel { get; set; }
        public Command CmdSearchBarVisible { get; set; }
        public Command CmdScanBatch { get; set; }

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

        zwaTransferDocDetailsBin curSelectedItem;
        zwaTransferDocDetailsBin selectedItem;
        public zwaTransferDocDetailsBin SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;
                curSelectedItem = value;
                HandlerItemSelected(value);

                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        List<zwaTransferDocDetailsBin> itemsSource;
        public ObservableCollection<zwaTransferDocDetailsBin> ItemsSource { get; set; }
        #endregion

        #region Private declaration
        /// <summary>
        /// Private declaration
        /// </summary>
        INavigation Navigation { get; set; } = null;
        Guid currentDocGuid { get; set; } = default;
        Guid currentDocGuidLine { get; set; } = default;
        string returnCallerAddress { get; set; } = string.Empty;
        string itemCode { get; set; } = string.Empty;
        string itemName { get; set; } = string.Empty;
        TransferLine standAloneLine { get; set; } = null; // for stand alone line

        List<Batch> batchs;
        bool fromWhsVisible { get; set; } = false;

        readonly string _ScanBatchAddr = "20201021T1524_ScanBatch";
        readonly string _PickBin = "20201021T1524_PickBatchEntryBin";
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public BatchToBinVM(
            INavigation navigation, Guid head, Guid line, string itemCod,
            string itemNam, string whsCod, string callerAddr)
        {
            Navigation = navigation;
            currentDocGuidLine = line;
            currentDocGuid = head;

            itemName = itemNam;
            itemCode = itemCod;

            WhsCode = whsCod;
            returnCallerAddress = callerAddr;
            fromWhsVisible = true;
            LoadBatchBin();
            InitCmd();
        }

        /// <summary>
        /// Allow the GRPO 
        /// </summary>
        /// <param name="navigation"></param>
        /// <param name="batches"></param>
        /// <param name="itemCod"></param>
        /// <param name="itemNam"></param>
        /// <param name="whsCod"></param>
        /// <param name="callerAddr"></param>
        public BatchToBinVM(
            INavigation navigation, List<Batch> batches, string itemCod,
            string itemNam, string whsCod, string callerAddr)
        {
            Navigation = navigation;
            itemName = itemNam;
            itemCode = itemCod;
            WhsCode = whsCod;
            batchs = batches;
            fromWhsVisible = false;

            returnCallerAddress = callerAddr;
            ParepareBatchBin();
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
                    sb.Placeholder = "Input Batch# to search";
                }
            });

            CmdSave = new Command(Save);
            CmdCancel = new Command(Close);
            CmdScanBatch = new Command(ScanBatch);
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
                if (curSelectedItem == null) return;

                MessagingCenter.Subscribe(this, _PickBin, (List<OBIN_Ex> selectedBins) =>
                {
                    MessagingCenter.Unsubscribe<List<OBIN_Ex>>(this, _PickBin);
                    var lines = itemsSource.Where(x => x.Bins != null).ToList();

                    if (lines == null)
                    {
                        int locid1 = itemsSource.IndexOf(curSelectedItem);
                        if (locid1 < 0) return;
                        itemsSource[locid1].Bins = selectedBins;
                        return;
                    }

                    // check the whole doc from batch line 
                    foreach (var bin in selectedBins)
                    {
                        foreach (var line in lines)
                        {
                            if (line.BinAbs.Equals(bin.oBIN.AbsEntry) && line.Batch.Equals(bin.BatchNumber))
                            {
                                DisplayMessage($"The allocated bin [TO] and bin [FROM] {bin.oBIN.BinCode} found duplicated in other [FROM] line, "  +
                                    $"the bin [TO] and bin [FROM] can no be same for the batch# {bin.BatchNumber}," +
                                    $"Please select different Bin [TO] Thanks.");
                                return;
                            }
                        }
                    }

                    int locid = itemsSource.IndexOf(curSelectedItem);
                    if (locid < 0) return;
                    itemsSource[locid].Bins = selectedBins;
                });

                Navigation.PushAsync(
                    new BatchToBinSelectView(
                        Navigation, this.whsCode, _PickBin, itemCode, itemName, 
                        selected.Batch, selected.Qty, selected.BinAbs));
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
                    .Where(x => x.Batch != null &&  x.Batch.ToLower().Contains(tolower)).ToList();

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
        /// Scan batch 
        /// </summary>
        void ScanBatch()
        {
            try
            {
                MessagingCenter.Subscribe<string>(this, _ScanBatchAddr, (string batch) =>
                {
                    MessagingCenter.Unsubscribe<string>(this, _ScanBatchAddr);

                    if (string.IsNullOrWhiteSpace(batch)) return;
                    if (batch.ToLower().Equals("cancel")) return;

                    if (itemsSource == null) return;

                    var found = itemsSource.Where(x => x.Batch.ToLower().Equals(batch.ToLower())).FirstOrDefault();
                    if (found == null)
                    {
                        DisplayToast($"Input {batch} no found. Please try again, Thanks.");
                        return;
                    }

                    HandlerItemSelected(found);
                });

                Navigation.PushAsync(new CameraScanView(_ScanBatchAddr, "Scan Batch#"));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Return the batch list back to the caller
        /// </summary>
        void Save()
        {
            try
            {
                // <List<zwaTransferDocDetailsBin>>
                if (itemsSource == null) Close();
                if (itemsSource.Count == 0) Close();
                var checkList = itemsSource.Where(x => x.Bins != null).ToList();
                MessagingCenter.Send(checkList, returnCallerAddress);
                Close();
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Load list of batch for user to pick the destination warehouse
        /// </summary>
        async void ParepareBatchBin()
        {
            try
            {
                // checking with server on the batched number exsting in the warehouse
                UserDialogs.Instance.ShowLoading("A moment ...");
                if (batchs == null) return;
                if (batchs.Count == 0) return;
                itemsSource = new List<zwaTransferDocDetailsBin>();

                batchs.ForEach(x => itemsSource.Add(
                            new zwaTransferDocDetailsBin
                            {
                                ItemCode = itemCode,
                                Qty = x.Qty,
                                Serial = string.Empty,
                                Batch = x.DistNumber,
                                InternalSerialNumber = string.Empty,
                                ManufacturerSerialNumber = string.Empty,
                                BinAbs = -1,
                                SnBMDAbs = -1,
                                WhsCode = whsCode,
                                Direction = "TO", 
                                BatchProp = x,
                                FromWhsVisible = this.fromWhsVisible
                            }));

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
        /// Load list of batch for user to pick the destination warehouse
        /// </summary>
        async void LoadBatchBin()
        {
            try
            {
                // checking with server on the batched number exsting in the warehouse
                UserDialogs.Instance.ShowLoading("A moment ...");
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "GetBatchsList",
                    TransferDocRequestGuid = currentDocGuid,
                    TransferDocRequestGuidLine = currentDocGuidLine
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

        /// <summary>
        /// Display toast onto the screen
        /// </summary>
        /// <param name="message"></param>
        void DisplayToast(string message) => DependencyService.Get<IToastMessage>()?.ShortAlert(message);
    }
}
