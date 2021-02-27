using Acr.UserDialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.GRPO;
using WMSApp.Views.Share;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Transfer1
{
    /// <summary>
    /// Handle the no bin ... selection of batch / or for user to confirm the batch qty, and perform the received
    /// </summary>
    public class BatchesConfirmVM : INotifyPropertyChanged
    {
        #region Page property
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Command CmdSave { get; set; }
        public Command CmdCancel { get; set; }
        public Command CmdSearchBarVisible { get; set; }
        public Command CmdScanBatch { get; set; }
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

                HandlerItemSelected(selectedItem);

                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        List<zwaTransferDocDetailsBin> itemsSource;
        public ObservableCollection<zwaTransferDocDetailsBin> ItemsSource { get; set; }
        #endregion


        #region Private declaration
        readonly string _ScanBatchAddr = "20201021T1524_ScanBatchA";
        INavigation Navigation { get; set; } = null;
        Guid currentLineDocGuid { get; set; } = default;
        Guid currentLineDocGuidLine { get; set; } = default;
        string returnAddress { get; set; } = string.Empty;        
        //int currentLineDocEntry { get; set; } = -1;
        
        //Guid LineGuid = default;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public BatchesConfirmVM(INavigation navigation, Guid head, Guid line, string whsCod, string returnAddr)
        {
            Navigation = navigation;
            currentLineDocGuid = head;
            currentLineDocGuidLine = line;

            returnAddress = returnAddr;
            WhsCode = whsCod;            
            LoadBatches();
            InitCmd();
        }

        /// <summary>
        /// Handler the return batch to warehous allocation
        /// </summary>
        /// <param name="navigation"></param>
        /// <param name="whsCod"></param>
        /// <param name="returnAddr"></param>
        /// <param name="batches"></param>
        /// <param name="itemCode"></param>
        /// <param name="itemName"></param>
        public BatchesConfirmVM(INavigation navigation, string whsCod, string returnAddr, List<Batch> batches, string itemCode, string itemName)
        {
            Navigation = navigation;
            //currentLineDocGuid = head;
            //currentLineDocGuidLine = line;

            returnAddress = returnAddr;
            WhsCode = whsCod;

            itemsSource = new List<zwaTransferDocDetailsBin>();
            batches.ForEach(x => itemsSource.Add(new zwaTransferDocDetailsBin
            {
                ItemCode = itemCode, 
                ItemName = itemName, 
                Batch = x.DistNumber, 
                Qty = x.Qty
            }));

            RefreshListView();

            InitCmd();
        }

        /// <summary>
        /// Init command linking 
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
            CmdSelect = new Command<string>((string param) =>
            {
                if (itemsSource == null) return;
                if (itemsSource.Count == 0) return;

                if (param.Equals("Select"))
                {
                    itemsSource.ForEach(x => 
                    { 
                        x.IsChecked = true; 
                        x.ReceiptQty = x.Qty; 
                        x.ReceiptColor = Color.Green; 
                    });

                    return;
                }

                itemsSource.ForEach(x =>
                {
                    x.IsChecked = false;                                        
                    x.Reset();
                });
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
        async void HandlerItemSelected(zwaTransferDocDetailsBin selected)
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
                    itemsSource[locId].ReceiptQty = 0;                    
                    itemsSource[locId].ReceiptColor = Color.White;
                    return;
                }

                decimal receiptQty = await CaptureReceiptQty(selected);
                if (receiptQty == -1) return;
                if (receiptQty > selected.Qty)
                {
                    DisplayMessage($"The input quantity {receiptQty:N} is more than the picked batch {selected.Batch} quantity ({selected.Qty:N}), " +
                        $"please try again, Thanks.");
                    itemsSource[locId].IsChecked = false;
                    return;
                }

                itemsSource[locId].IsChecked = true;
                itemsSource[locId].ReceiptQty = receiptQty;                
                itemsSource[locId].ReceiptColor = Color.Green;
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Prompt to enter the receipt qty
        /// </summary>
        /// <returns></returns>
        async Task<decimal> CaptureReceiptQty(zwaTransferDocDetailsBin selected)
        {
            try
            {
                string receiptQty = await new Dialog().DisplayPromptAsync(
                    $"Input Item{selected.ItemCode} @ batch#{selected.Batch} Qty", 
                    "", "Ok", "Cancel", null, -1, Keyboard.Numeric, $"{selected.Qty:N}");

                if (string.IsNullOrWhiteSpace(receiptQty)) return -1;
                if (receiptQty.ToLower().Equals("cancel")) return -1;
                bool isNUmeric = decimal.TryParse(receiptQty, out decimal result);
                if (!isNUmeric) return -1;
                if (result < 0) return -1;

                return result;
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
                return -1;
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
                var filter = itemsSource.Where(x => x.Batch != null && x.Batch.ToLower().Contains(query)).ToList();
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
        /// Scan Batch 
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

                    var found = itemsSource.Where(x => x.Batch != null && x.Batch.ToLower().Equals(batch.ToLower())).FirstOrDefault();
                    if (found == null)
                    {
                        DisplayToast($"Input {batch} no found. Please try again, Thanks.");
                        return;
                    }

                    HandlerItemSelected(found);
                    // handler capture serial number
                    //HandlerSearchQuery(batch);

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
            // <List<zwaTransferDocDetailsBin>>
            if (itemsSource == null) Close();
            if (itemsSource.Count == 0) Close();

            var checkList = itemsSource.Where(x => x.IsChecked && x.ReceiptQty > 0).ToList();

            if (checkList == null)
            {
                DisplayMessage("Some batch(es) not being acknowledged, Please acknowledged all batch(es) qty, Thanks.");
                return;
            }

            // cooy the rceipt qty into qty for display
            checkList.ForEach(x => x.Qty = x.ReceiptQty);

            MessagingCenter.Send(checkList, returnAddress);
            Close();
        }

        /// <summary>
        /// Load list of batch for user to pick the destination warehouse
        /// </summary>
        async void LoadBatches()
        {
            try
            {
                // checking with server on the batch number exsting in the warehouse
                UserDialogs.Instance.ShowLoading("A moment ...");
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "GetBatchesList",                    
                    TransferDocRequestGuid = currentLineDocGuid,
                    TransferDocRequestGuidLine = currentLineDocGuidLine
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

                if (repliedBag.TransferDocDetailsBins.Length == 0)
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
        void DisplayToast(string message) => DependencyService.Get<IToastMessage>()?.ShortAlert(message);
    }
}
