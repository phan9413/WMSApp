using System;
using System.ComponentModel;
using Xamarin.Forms;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.Transfer1;
using WMSApp.Views.Share;
using Acr.UserDialogs;
using Newtonsoft.Json;
using DbClass;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using WMSApp.Views.Transfer1;

namespace WMSApp.ViewModels.Transfer1
{
    public class TransferPickVM : INotifyPropertyChanged
    {
        #region Property
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        bool statusBarVisible;
        public bool StatusBarVisible
        {
            get => statusBarVisible;
            set
            {
                if (statusBarVisible == value) return;
                statusBarVisible = value;
                OnPropertyChanged(nameof(StatusBarVisible));
            }
        }

        string statusBarText;
        public string StatusBarText
        {
            get => statusBarText;
            set
            {
                if (statusBarText == value) return;
                statusBarText = value;
                OnPropertyChanged(nameof(StatusBarText));
            }
        }

        Color statusBarTextColor;
        public Color StatusBarTextColor
        {
            get => statusBarTextColor;
            set
            {
                if (statusBarTextColor == value) return;
                statusBarTextColor = value;
                OnPropertyChanged(nameof(StatusBarTextColor));
            }
        }

        Color statusBarColor;
        public Color StatusBarColor
        {
            get => statusBarColor;
            set
            {
                if (statusBarColor == value) return;
                statusBarColor = value;
                OnPropertyChanged(nameof(StatusBarColor));
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

        string itemManagedBy;
        public string ItemManagedBy
        {
            get => itemManagedBy;
            set
            {
                if (itemManagedBy == value) return;
                itemManagedBy = value;
                OnPropertyChanged(nameof(ItemManagedBy));
            }
        }

        string fromWhsCode;
        public string FromWhsCode
        {
            get => fromWhsCode;
            set
            {
                if (fromWhsCode == value) return;
                fromWhsCode = value;
                OnPropertyChanged(nameof(FromWhsCode));
            }
        }

        string binActivated;
        public string BinActivated

        {
            get => binActivated;
            set
            {
                if (binActivated == value) return;
                binActivated = value;
                OnPropertyChanged(nameof(BinActivated));
            }
        }

        decimal requestedQty;
        public decimal RequestedQty
        {
            get => requestedQty;
            set
            {
                if (requestedQty == value) return;
                requestedQty = value;
                OnPropertyChanged(nameof(RequestedQty));
            }
        }

        bool itemDetailsIsExpanded;
        public bool ItemDetailsIsExpanded
        {
            get => itemDetailsIsExpanded;
            set
            {
                if (itemDetailsIsExpanded == value) return;
                itemDetailsIsExpanded = value;
                OnPropertyChanged(nameof(ItemDetailsIsExpanded));
            }
        }

        string selectedQty;
        public string SelectedQty
        {
            get => selectedQty;
            set
            {
                if (selectedQty == value) return;
                selectedQty = value;
                OnPropertyChanged(nameof(SelectedQty));
            }

        }

        string remainingQty;
        public string RemainingQty
        {
            get => remainingQty;
            set
            {
                if (remainingQty == value) return;
                remainingQty = value;
                OnPropertyChanged(nameof(RemainingQty));
            }
        }

        List<TransferItemDetailBinM> itemsSource;
        public ObservableCollection<TransferItemDetailBinM> ItemsSource { get; set; }

        TransferItemDetailBinM selectedItem;
        public TransferItemDetailBinM SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;

                // if not serial then collect qty
                if (string.IsNullOrWhiteSpace(selectedItem.Serial))
                {
                    ChangeQty(selectedItem);
                }


                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public Command CmdManualInput { get; set; }
        public Command CmdStartScanner { get; set; }
        public Command CmdSave { get; set; }
        public Command CmdCancel { get; set; }
        public Command CmdShowList { get; set; }
        #endregion

        /// <summary>
        /// Private declaration
        /// </summary>
        decimal remaining;
        decimal selected;
        string direction;
        //string InputInstrutionDesc = "Input Bin Code, Serial# or Batch# to add";

        OBIN selectedBinObj;
        OSRN selectSerialObj;
        OBTN selectBatchObj;

        List<OSBQ_Ex> selectedBinSerials; // kept line of selected bin and serials #
        readonly string _binSerialsReturnAddress = "20201018T1111_serialBin";

        List<OSRQ_Ex> selectedSerials; // kept line of selected bin and serials #
        readonly string _serialsReturnAddress = "20201018T1539_serial";

        INavigation Navigation;
        string returnCallerAddress;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="navigation"></param>
        public TransferPickVM(INavigation navigation, TransferDataM item, string callerAddress, string whsDirection)
        {
            Navigation = navigation;
            ItemCode = item.ItemCode;
            ItemName = item.ItemName;
            ItemManagedBy = item.ItemManagedBy;
            FromWhsCode = item.FromWhsCode;
            BinActivated = item.BinActivated;
            RequestedQty = item.RequestedQty;
            remaining = item.RequestedQty;
            direction = whsDirection;

            ItemDetailsIsExpanded = true;
            returnCallerAddress = callerAddress;
            CalculateRemainingQty();
            InitCmd();
            //StartTimerForCheck();
        }

        /// <summary>
        /// remove item from list
        /// </summary>
        /// <param name="removeItem"></param>
        public void RemoveItem(TransferItemDetailBinM removeItem)
        {
            if (itemsSource == null) return;
            if (itemsSource.Count == 0) return;
            itemsSource.Remove(removeItem);

            selected -= removeItem.Qty;

            RefreshListview();
            CalculateRemainingQty();
        }

        /// <summary>
        /// click line to get the qty
        /// </summary>
        /// <param name="selected"></param>
        void ChangeQty(TransferItemDetailBinM selected)
        {
            if (itemsSource == null) return;
            PromptCaptureQty();
        }

        /// <summary>
        /// initial start a timmer to initial check
        /// </summary>
        //void StartTimerForCheck()
        //{
        //    Device.StartTimer(new TimeSpan(150), () =>
        //    {
        //        // do something every 60 seconds
        //        Device.BeginInvokeOnMainThread(() =>
        //        {
        //            InitialCheck();
        //        });
        //        return false; // runs again, or false to stop
        //    });
        //}

        /// <summary>
        /// Perform a intial check on the item and warehouse property
        /// </summary>
        void InitialCheck()
        {
            try
            {
                switch (itemManagedBy)
                {
                    case "None":
                        {
                            if (binActivated.Equals("Yes"))
                            {
                                //PromptSelectOrScan("Bin");
                                PromptCodeScanner($"Scan Bin#");
                                return;
                            }
                            // ELSE
                            PromptCaptureQty();
                            break;
                        }
                    case "Serial":
                        {
                            if (binActivated.Equals("Yes"))
                            {
                                //PromptSelectOrScan("Bin");
                                PromptCodeScanner($"Scan Bin#");
                                return;
                            }
                            // ELSE 
                            // prompt scan bin code                            
                            //PromptSelectOrScan("Serial");
                            PromptCodeScanner($"Scan Serial#");
                            return;
                        }
                    case "Batch":
                        {
                            if (binActivated.Equals("Yes"))
                            {
                                //PromptSelectOrScan("Bin");
                                PromptCodeScanner($"Scan Bin#");
                                return;
                            }
                            // ELSE 
                            // prompt scan bin code                            
                            //PromptSelectOrScan("Batch");
                            PromptCodeScanner($"Scan Batch#");
                            return;
                        }
                }
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }
       
        /// <summary>
        /// Handler item with no bin none manage item
        /// </summary>
        async void PromptCaptureQty()
        {
            try
            {
                decimal qty = await CaptureQuantity($"Input {itemCode} transfer qty", "", requestedQty);
                if (qty == -1) return;
                if (qty == 0)
                {
                    DisplayMessage($"Item {itemCode} transfer qty is zero, please try again. Thanks");
                    return;
                }

                if (qty > requestedQty)
                {
                    DisplayMessage($"Item {itemCode} transfer qty {qty} is more than requested qty {requestedQty:N}, please try again. Thanks");
                    return;
                }

                // then add into the list
                var noneManagedItem = new TransferItemDetailBinM
                {
                    ItemCode = itemCode,
                    Qty = qty,
                    Serial = string.Empty,
                    Batch = string.Empty,
                    InternalSerialNumber = string.Empty,
                    ManufacturerSerialNumber = string.Empty,
                    BinAbs = -1,
                    SnBMDAbs = -1,
                    WhsCode = fromWhsCode,
                    Direction = direction,
                    BinCode = string.Empty
                };

                if (itemsSource == null) itemsSource = new List<TransferItemDetailBinM>();

                if (IsItemDuplicated(noneManagedItem, itemManagedBy))
                {
                    int locId = itemsSource.IndexOf(itemsSource
                        .Where(x => x.ItemCode.Equals(noneManagedItem.ItemCode)).FirstOrDefault()); // overwrite the qty for existing the item
                    if (locId >= 0)
                    {
                        selected -= itemsSource[locId].Qty;
                        itemsSource[locId].Qty = qty;
                        selected += qty; // increase the selected Qty 

                        RefreshListview();
                        CalculateRemainingQty();

                        // reset local bin 
                        selectedBinObj = null;
                    }
                    return;
                }

                AddItemToList(noneManagedItem);
                selected += qty; // increase the selected Qty 
                RefreshListview();
                CalculateRemainingQty();
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Initial command
        /// </summary>
        void InitCmd()
        {
            CmdManualInput = new Command(CaptureManualInput);
            CmdStartScanner = new Command(() =>
            {
                if (IsQtyFulfill()) return;
                InitialCheck();
            });

            CmdCancel = new Command(ConfirmQuit);
            CmdSave = new Command(Save);
            CmdShowList = new Command(Showlist);
        }

        /// <summary>
        /// Send the collected information back to the caller via messaging centre
        /// </summary>
        async void Save()
        {
            MessagingCenter.Send(itemsSource, returnCallerAddress);
            await Navigation.PopAsync();
        }

        /// <summary>
        /// Based on the current item setup show list of 
        /// Bin serial selection 
        /// Serial in this location 
        /// Batch bin selection 
        /// bacth in this location 
        /// none manage item in bin selection 
        /// none manage item in this location
        /// </summary>
        void Showlist()
        {
            try
            {
                switch (itemManagedBy)
                {
                    case "None":
                        {
                            if (binActivated.Equals("Yes"))
                            {
                                return;
                            }
                            // ELSE
                            PromptCaptureQty();
                            break;
                        }
                    case "Serial":
                        {
                            if (binActivated.Equals("Yes"))
                            {
                                HandlerBinSerialSelectionList();
                                return;
                            }
                            // ELSE 
                            // show list of serial in this warehouse
                            HandlerSerialSelectionList();
                            return;
                        }
                    case "Batch":
                        {
                            if (binActivated.Equals("Yes"))
                            {
                                // show list of batch in this warehouse bin                                
                                return;
                            }
                            // ELSE 
                            // show batches list in this warehouse
                            return;
                        }
                }
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Show list of bin + serial # for user to pick and select
        /// </summary>
        void HandlerBinSerialSelectionList()
        {
            // show list of serial in whs bin 
            MessagingCenter.Subscribe<List<OSBQ_Ex>>(this, _binSerialsReturnAddress,
               (List<OSBQ_Ex> binSerials) =>
               {
                   MessagingCenter.Unsubscribe<List<OSBQ_Ex>>(this, _binSerialsReturnAddress);
                   if (binSerials == null) return;
                   selectedBinSerials = binSerials;

                   // add in the serial # into the list


               });

            //Navigation.PushAsync(new BinSerialListView(_binSerialsReturnAddress, itemCode, fromWhsCode, (int)remaining));
        }

        /// <summary>
        ///  Show list of serial in this warehouse for user to select
        /// </summary>
        void HandlerSerialSelectionList()
        {
            // show list of serial in whs bin 
            MessagingCenter.Subscribe<List<OSRQ_Ex>>(this, _serialsReturnAddress,
               (List<OSRQ_Ex> serials) =>
               {
                   MessagingCenter.Unsubscribe<List<OSRQ_Ex>>(this, _serialsReturnAddress);
                   if (serials == null) return;
                   selectedSerials = serials;
               });

            //Navigation.PushAsync(new SerialListView(_serialsReturnAddress, itemCode, fromWhsCode));
        }

        /// <summary>
        /// to confirm quite screen
        /// </summary>
        async void ConfirmQuit()
        {
            if (itemsSource != null && itemsSource.Count > 0)
            {
                bool confirmLeave = await new Dialog().DisplayAlert("Leave without save?",
                    "There are item in the list no save, are you sure to leave?", "Confirm", "Cancel");
                if (confirmLeave)
                {
                    await Navigation.PopAsync();
                    return;
                }
                return;
            }
            await Navigation.PopAsync();
        }

        // check is qty fulfilled
        bool IsQtyFulfill()
        {
            if (itemsSource != null && itemsSource.Count > 0)
            {
                var sum = itemsSource.Sum(x => x.Qty);
                if (sum == requestedQty)
                {
                    DisplayMessage($"Line requested qty-> {requestedQty:N} fulfilled, please save and process the next line, Thanks");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Prompt dialog for capture the serial, item code or batch number / bin
        /// </summary>
        async void CaptureManualInput()
        {
            try
            {
                if (IsQtyFulfill()) return;

                string capture = await new Dialog().DisplayPromptAsync(
                    "Manul Input", "Item Code, Bin Code, Serial# or Batch# to add",
                    "OK",
                    "Cancel",
                    "",
                    -1,
                    Keyboard.Default,
                    $"{itemCode}");

                if (string.IsNullOrWhiteSpace(capture))
                {
                    DisplayToastShort("Please enter valid value, Thanks");
                    return;
                }

                if (capture.ToLower().Equals("cancel")) return;

                ProcessCapture(capture);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Calculate the remaining qty
        /// </summary>
        void CalculateRemainingQty()
        {
            remaining = requestedQty - selected;
            SelectedQty = $"Selected {selected:N}";
            RemainingQty = $"{remaining:N} Remaining";
        }

        /// <summary>
        ///  to used by scan in serial , batch and those warehouse with bin activated
        /// </summary>
        void PromptCodeScanner(string subTitle)
        {
            var returnAddress = "20201011T2000_StartScanner";
            MessagingCenter.Subscribe(this, returnAddress, (string code) =>
            {
                MessagingCenter.Unsubscribe<string>(this, returnAddress);
                if (string.IsNullOrWhiteSpace(code))
                {
                    DisplayToastShort("Invalid code input, please try again later. Thanks");
                    return;
                }

                if (code.ToLower().Equals("cancel")) return;

                ProcessCapture(code);
            });

            Navigation.PushAsync(new CameraScanView(returnAddress, subTitle));
        }

        /// <summary>
        /// Support scan data
        /// Send data to server to get item
        /// </summary>
        /// <param name="capture"></param>
        async void ProcessCapture(string capture)
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
                    request = "QueryCode",
                    TransferItemCode = itemCode,
                    TransferQueryCode = capture,
                    TransferWhsCode = fromWhsCode
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
                    DisplayMessage("Error contacting server replied, please try again later");
                    return;
                }

                if (repliedCio.TransferFoundBin != null)
                {
                    selectedBinObj = repliedCio.TransferFoundBin;
                    HandlerBinCodeCapture();
                    return;
                }

                if (repliedCio.TransferFoundSerial != null)
                {
                    if (repliedCio.TransferFoundItem == null)
                    {
                        DisplayMessage($"There is error reading the server replied info, please again later. Thanks");
                        return;
                    }

                    if (!itemCode.Equals(repliedCio.TransferFoundItem.ItemCode))
                    {
                        DisplayMessage($"Serial# {capture} found, " +
                            $"but it does not belong to requested item -> {itemCode}, please try others serial# again. Thanks");
                        return;
                    }

                    if (repliedCio.TransferFoundSerial.Status != null && repliedCio.TransferFoundSerial.Status.Equals("1"))
                    {
                        DisplayMessage($"Serial# {capture} found, " +
                           $"but the serial is unavailable, please try others serial# again. Thanks");
                        return;
                    }

                    if (repliedCio.TransferFoundSerial.Status != null && repliedCio.TransferFoundSerial.Status.Equals("2"))
                    {
                        DisplayMessage($"Serial# {capture} found, " +
                            $"but the serial is allocated, please try others serial# again. Thanks");
                        return;
                    }

                    // check serial code exit in the warehouse 
                    CheckSerialExistInWhs(repliedCio.TransferFoundSerial);
                    return;
                }

                if (repliedCio.TransferFoundBatch != null)
                {
                    if (repliedCio.TransferFoundItem == null)
                    {
                        DisplayMessage($"There is error reading the server replied info, please again later. Thanks");
                        return;
                    }

                    if (!itemCode.Equals(repliedCio.TransferFoundItem.ItemCode))
                    {
                        DisplayMessage($"Batch# {capture} found, " +
                            $"but it does not belong to requested item -> {itemCode}, please try others batch# again. Thanks");
                        return;
                    }

                    selectBatchObj = repliedCio.TransferFoundBatch;
                    HandlerBatchCapture();
                    return;
                }

                if (repliedCio.TransferFoundItem != null)
                {
                    InitialCheck();
                    return;
                }
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
        /// Query server to check the serial exit in the warehouse
        /// </summary>
        async void CheckSerialExistInWhs(OSRN serial)
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
                    request = "CheckItemSerialExistInWhs",
                    TransferQueryCode = itemCode,
                    TransferWhsCode = fromWhsCode,
                    TransSerialCode = serial.DistNumber
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
                // check 
                var repliedCio = JsonConvert.DeserializeObject<Cio>(content);
                if (repliedCio == null)
                {
                    DisplayMessage($"Error reading information from server for item {itemCode}, Serial# {serial.DistNumber}," +
                        $"Please try again later. Thanks");
                    return;
                }

                OSRI osriObj = repliedCio.TransferOSRI;
                if (osriObj == null)
                {
                    DisplayMessage($"Item {itemCode}, Serial# {serial.DistNumber} not exist in warehouse {fromWhsCode}, " +
                        $"Or the serial # is unavailable / allocated by other transaction. " +
                        $"Please try other serial#. Thanks");
                    return;
                }

                if (osriObj.Status.Equals("1"))
                {
                    DisplayMessage($"Item {itemCode}, Serial# {serial.DistNumber} exist in warehouse {fromWhsCode}," +
                        $"But the serial was unavailable, " +
                        $"Please try other serial#. Thanks");
                    return;
                }

                if (string.IsNullOrWhiteSpace(serial.Status))
                {
                    // proceed the serial number
                    selectSerialObj = serial;
                    HandlerSerialCapture();
                    return;
                }

                // When checking done all done
                if (serial.Status.Equals("0"))
                {
                    selectSerialObj = serial;
                    HandlerSerialCapture();
                    return;
                }
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
        /// Handle batch code input
        /// </summary>
        async void HandlerBatchCapture()
        {
            try
            {
                // prompt scan bin code first
                if (binActivated.Equals("Yes"))
                {
                    PromptCodeScanner("Scan Bin#");
                    return;
                }

                // else add in the batch to list 
                // then add into the list
                // need to capature the batch actual transfer qty
                if (selectBatchObj == null)
                {
                    DisplayMessage($"Internal error reading batch #, please try again. Thanks");
                    return;
                }

                // if not null
                if (!string.IsNullOrWhiteSpace(selectBatchObj.Status) && selectBatchObj.Status.Equals("1"))
                {
                    DisplayMessage($"Item {itemCode}, Batch# {selectBatchObj.DistNumber} not exist in warehouse {fromWhsCode}, " +
                        $"Or the batch# is Not Accessible." +
                        $"Please try other batch#. Thanks");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(selectBatchObj.Status) && selectBatchObj.Status.Equals("2"))
                {
                    DisplayMessage($"Item {itemCode}, Batch# {selectBatchObj.DistNumber} not exist in warehouse {fromWhsCode}, " +
                        $"Or the batch# is locked." +
                        $"Please try other batch#. Thanks");
                    return;
                }

                // need to query server on the batch, whs and item 
                CheckBatchQty(selectBatchObj);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Check batch Qty in specific warehouse
        /// </summary>
        async void CheckBatchQty(OBTN batchMaster)
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
                    request = "CheckItemBatchExistInWhs",
                    TransferQueryCode = itemCode,
                    TransferWhsCode = fromWhsCode,
                    TransBatchAbs = batchMaster.AbsEntry
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
                    DisplayMessage($"Error reading information from server for item {itemCode}, Batch# {batchMaster.DistNumber}," +
                      $"Please try again later. Thanks");
                    return;
                }

                if (repliedCio.TransferBatch == null)
                {
                    DisplayMessage($"Item {itemCode}, Batch# {batchMaster.DistNumber} not exist in warehouse {fromWhsCode}, " +
                         $"Please try other batch#. Thanks");
                    return;
                }

                OBTQ obtqObj = repliedCio.TransferBatch;
                if (obtqObj == null)
                {
                    DisplayMessage($"Item {itemCode}, Batch# {batchMaster.DistNumber} not exist in warehouse {fromWhsCode}, " +
                       $"Please try other batch#. Thanks");
                    return;
                }

                if (obtqObj.Quantity == 0)
                {
                    DisplayMessage($"Item {itemCode}, Batch# {batchMaster.DistNumber} exist in warehouse {fromWhsCode}, " +
                       $"but batch qty falled into zero, " +
                       $"Please try other batch#. Thanks");
                    return;
                }

                /// Contnue when batch qty okay 

                var limit = (selectBatchObj.Quantity > remaining) ? remaining : selectBatchObj.Quantity;

                decimal qty = await CaptureQuantity(
                    $"Input Item {itemCode} @Batch # {selectBatchObj.DistNumber}",
                    $"Info -> Cur. Batch Qty: {selectBatchObj.Quantity:N}, Remaining Qty : {remaining:N}", limit);

                if (qty == -1) return;
                if (qty > selectBatchObj.Quantity)
                {
                    DisplayMessage($"Item {itemCode} @Batch # {selectBatchObj.DistNumber}, " +
                        $"Qty input -> {qty} must be less than the batch qty -> {selectBatchObj.Quantity:N}");
                    return;
                }

                if (qty > remaining)
                {
                    DisplayMessage($"Item {itemCode} @Batch # {selectBatchObj.DistNumber}, " +
                        $"Qty input -> {qty:N} must be less than the remain qty => {remaining:N}");
                    return;
                }

                var batch = new TransferItemDetailBinM
                {
                    ItemCode = itemCode,
                    Qty = qty,
                    Serial = string.Empty,
                    Batch = selectBatchObj.DistNumber,
                    InternalSerialNumber = $"{selectBatchObj.SysNumber}",
                    ManufacturerSerialNumber = selectBatchObj.MnfSerial,
                    BinAbs = -1,
                    SnBMDAbs = -1,
                    WhsCode = fromWhsCode,
                    Direction = direction,
                    BinCode = string.Empty
                };

                if (itemsSource == null) itemsSource = new List<TransferItemDetailBinM>();
                if (IsItemDuplicated(batch, itemManagedBy))
                {
                    DisplayMessage($"Item {itemCode} @Batch# {batch.CheckDistNum} found duplicated, Please try others batch#. Thanks.");
                    return;
                }

                AddItemToList(batch);
                selected += qty; // increase the selected Qty 

                RefreshListview();
                CalculateRemainingQty();

                // reset local bin 
                selectSerialObj = null;
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Handle detect input code is serial
        /// </summary>
        void HandlerSerialCapture()
        {
            try
            {
                // prompt scan bin code first
                if (binActivated.Equals("Yes"))
                {
                    PromptCodeScanner("Scan Bin#");
                    return;
                }

                // else add in the serial to list 
                // then add into the list
                var serial = new TransferItemDetailBinM
                {
                    ItemCode = itemCode,
                    Qty = 1,
                    Serial = selectSerialObj.DistNumber,
                    Batch = string.Empty,
                    InternalSerialNumber = $"{selectSerialObj.SysNumber}",
                    ManufacturerSerialNumber = selectSerialObj.MnfSerial,
                    BinAbs = -1,
                    SnBMDAbs = -1,
                    WhsCode = fromWhsCode,
                    Direction = direction,
                    BinCode = string.Empty
                };

                if (itemsSource == null) itemsSource = new List<TransferItemDetailBinM>();
                if (IsItemDuplicated(serial, itemManagedBy))
                {
                    DisplayMessage($"Item {itemCode} @Serial# {serial} found duplicated, Please try others serial#. Thanks.");
                    return;
                }

                itemsSource.Add(serial);
                selected++; // increase the selected Qty 
                RefreshListview();
                CalculateRemainingQty();

                // reset local bin 
                selectSerialObj = null;
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Leading user go to bin code
        /// </summary>
        void HandlerBinCodeCapture()
        {
            switch (itemManagedBy)
            {
                case "Serial":
                    {
                        CaptureBinSerialNum();
                        break;
                    }
                case "Batch":
                    {
                        CaptureBinBatchNum();
                        break;
                    }
                case "None":
                    {
                        CaptureBinQty();
                        break;
                    }
            }
        }

        /// <summary>
        ///  Prompt qty and 
        /// </summary>
        async void CaptureBinQty()
        {
            try
            {
                if (selectedBinObj == null)
                {
                    DisplayToastShort("Internal error reading bin information, please try again later. Thanks");
                    return;
                }

                // query server to get latest qty
                UserDialogs.Instance.ShowLoading("A moment ...");
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "QueryItemBinAccumulator",
                    TransferItemCode = itemCode,
                    TransferFoundBin = selectedBinObj
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
                    DisplayMessage($"Error reading bin {selectedBinObj.BinCode} information from server, please try again later. " +
                        $"Thanks [FA]\n\n{content}");
                    return;
                }

                // success
                var repliedCio = JsonConvert.DeserializeObject<Cio>(content);
                if (repliedCio == null)
                {
                    DisplayMessage($"Error reading bin {selectedBinObj.BinCode} information from server, please try again later. Thanks [CI]");
                    return;
                }

                OIBQ binAccumulator = repliedCio.TransferBinAccumulator;
                if (binAccumulator == null)
                {
                    DisplayMessage($"Error reading bin {selectedBinObj.BinCode} information from server, please try again later. Thanks [BQ]");
                    return;
                }

                if (binAccumulator.OnHandQty == 0)
                {
                    DisplayMessage($"Item {ItemCode} @Bin {selectedBinObj.BinCode} on hand qty was zero [0], " +
                        $"please try other bin and try again later. [ZR] Thanks");
                    return;
                }

                decimal limit = (remaining > binAccumulator.OnHandQty) ? binAccumulator.OnHandQty : remaining;
                decimal captureBinQty = await CaptureQuantity($"Input transfer qty",
                        $"Item {ItemCode} @Bin {selectedBinObj.BinCode}, " +
                        $"Needed Qty: {remaining:N}", limit);

                if (captureBinQty != -1) return;
                if (captureBinQty > remaining)
                {
                    DisplayMessage($"Item {ItemCode} @Bin {selectedBinObj.BinCode} " +
                        $"capture qty {captureBinQty:N} was over the remaining needed qty {remaining:N}, " +
                        $"please try other qty and try again. [OV] Thanks");
                    return;
                }

                // then add into the list
                var noneBin = new TransferItemDetailBinM
                {
                    ItemCode = itemCode,
                    Qty = captureBinQty,
                    Serial = string.Empty,
                    Batch = string.Empty,
                    InternalSerialNumber = string.Empty,
                    ManufacturerSerialNumber = string.Empty,
                    BinAbs = selectedBinObj.AbsEntry,
                    SnBMDAbs = -1,
                    WhsCode = fromWhsCode,
                    Direction = direction,
                    BinCode = selectedBinObj.BinCode
                };

                if (itemsSource == null) itemsSource = new List<TransferItemDetailBinM>();

                if (IsItemDuplicated(noneBin, this.itemManagedBy))
                {
                    int locId = itemsSource.IndexOf(itemsSource.Where(x => x.BinCode.Equals(noneBin.BinCode)).FirstOrDefault()); // overwrite the qty for existing the item
                    if (locId >= 0)
                    {
                        selected -= itemsSource[locId].Qty;
                        itemsSource[locId].Qty = captureBinQty;

                        selected += captureBinQty; // increase the selected Qty 
                        RefreshListview();
                        CalculateRemainingQty();

                        // reset local bin 
                        selectedBinObj = null;
                    }
                    return;
                }

                // Add new
                AddItemToList(noneBin);
                selected += captureBinQty; // increase the selected Qty 
                RefreshListview();
                CalculateRemainingQty();

                // reset local bin 
                selectedBinObj = null;
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
        /// Camera capture serial num
        /// </summary>
        void CaptureBinSerialNum()
        {
            string returnAddress = "20201011T1445_CaptureSerialNum";
            MessagingCenter.Subscribe(this, returnAddress, (string serialNum) =>
            {
                MessagingCenter.Unsubscribe<string>(this, returnAddress);

                if (string.IsNullOrWhiteSpace(serialNum)) return;
                if (serialNum.ToLower().Equals("cancel")) return;
                ProcessBinSerialNum(serialNum);
            });
            Navigation.PushAsync(new CameraScanView(returnAddress, "Scan a serial#"));
        }

        /// <summary>
        /// Hander serial number
        /// Query server
        /// </summary>
        /// <param name="serialNum"></param>
        async void ProcessBinSerialNum(string serialNum)
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
                    request = "QuerySerialNum",
                    TransferQueryCode = serialNum,
                    TransferItemCode = itemCode,
                    TransferFoundBin = selectedBinObj
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
                    DisplayMessage($"{content}");
                    return;
                }

                var repliedCio = JsonConvert.DeserializeObject<Cio>(content);
                if (repliedCio == null)
                {
                    await new Dialog().DisplayAlert($"Serial# {serialNum} no found, in bin# {selectedBinObj.BinCode}",
                        $"Please try another serial# for item code {itemCode} [x0]. Thanks", "OK");
                    return;
                }

                if (repliedCio.TransferFoundItem == null)
                {
                    await new Dialog().DisplayAlert($"Serial# {serialNum} no found, in bin# {selectedBinObj.BinCode}",
                        $"Please try another serial# for item code {itemCode} [x1]. Thanks", "OK");
                    return;
                }

                if (repliedCio.TransferFoundSerial == null)
                {
                    await new Dialog().DisplayAlert($"Serial# {serialNum} no found, in bin# {selectedBinObj.BinCode}",
                        $"Please try another serial# for item code {itemCode} [x2]. Thanks", "OK");
                    return;
                }

                string serialStatus = $"{repliedCio.TransferFoundSerial.Status}";
                if (!string.IsNullOrWhiteSpace(serialStatus) && !serialStatus.Equals("0"))
                {
                    await new Dialog().DisplayAlert($"Serial# {serialNum} is not available for transfer from bin# {selectedBinObj.BinCode}",
                    $"Please try another serial# for item code {itemCode}. Thanks", "OK");
                    return;
                }

                OSBQ accumulator = repliedCio.TransferBinSerialAccumulator;
                if (accumulator == null)
                {
                    await new Dialog().DisplayAlert($"Serial# {serialNum} no found in bin# {selectedBinObj.BinCode}",
                        $"Please try another serial# for item code {itemCode} [x3]. Thanks", "OK");
                    return;
                }

                if (accumulator.OnHandQty <= 0)
                {
                    await new Dialog().DisplayAlert($"Serial# {serialNum} on hand qty is zero, in bin# {selectedBinObj.BinCode}, ",
                       $"Please try another serial# for item code {itemCode} [x3]. Thanks", "OK");
                    return;
                }

                // then add into the list
                var serialBin = new TransferItemDetailBinM
                {
                    ItemCode = itemCode,
                    Qty = 1,
                    Serial = serialNum,
                    Batch = string.Empty,
                    InternalSerialNumber = $"{repliedCio.TransferFoundSerial.SysNumber}",
                    ManufacturerSerialNumber = repliedCio.TransferFoundSerial.MnfSerial,
                    BinAbs = selectedBinObj.AbsEntry,
                    SnBMDAbs = repliedCio.TransferBinSerialAccumulator.SnBMDAbs,
                    WhsCode = fromWhsCode,
                    Direction = direction,
                    BinCode = selectedBinObj.BinCode
                };

                if (itemsSource == null) itemsSource = new List<TransferItemDetailBinM>();
                if (IsItemDuplicated(serialBin, this.itemManagedBy))
                {
                    DisplayMessage($"Item {itemCode} @Serial# {serialNum} found duplicated, Please try others serial#. Thanks.");
                    return;
                }

                //var isDuplicate = itemsSource.Where(x => x.CheckDistNum.Equals(serialBin.CheckDistNum)).FirstOrDefault();
                //if (isDuplicate != null)
                //{
                //    DisplayMessage($"Item {itemCode} @Serial# {serialNum} found duplicated, Please try others serial#. Thanks.");
                //    return;
                //}

                //itemsSource.Add(serialBin);
                AddItemToList(serialBin);

                selected++; // increase the selected Qty 
                RefreshListview();
                CalculateRemainingQty();

                // reset local bin 
                selectedBinObj = null;
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
        /// Camera capture batch num
        /// </summary>
        void CaptureBinBatchNum()
        {
            string returnAddress = "20201011T1553_CaptureBatchNum";
            MessagingCenter.Subscribe(this, returnAddress, (string batchNum) =>
            {
                MessagingCenter.Unsubscribe<string>(this, returnAddress);

                if (string.IsNullOrWhiteSpace(batchNum)) return;
                if (batchNum.ToLower().Equals("cancel")) return;
                ProcessBinBatchNum(batchNum);
            });
            Navigation.PushAsync(new CameraScanView(returnAddress, "Scan a batch#"));
        }

        /// <summary>
        /// Query batch bin detail and add list
        /// </summary>
        /// <param name="batchNum"></param>
        async void ProcessBinBatchNum(string batchNum)
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
                    request = "QueryBatchNum",
                    TransferQueryCode = batchNum,
                    TransferItemCode = itemCode,
                    TransferFoundBin = selectedBinObj
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
                    await new Dialog().DisplayAlert($"Batch# {batchNum} no found",
                        $"Please try another batch# for item code {itemCode} [x0]. Thanks", "OK");
                    return;
                }

                if (repliedCio.TransferFoundItem == null)
                {
                    await new Dialog().DisplayAlert($"Batch# {batchNum} no found",
                        $"Please try another batch# for item code {itemCode} [x1]. Thanks", "OK");
                    return;
                }

                if (repliedCio.TransferFoundBatch == null)
                {
                    await new Dialog().DisplayAlert($"Batch# {batchNum} no found",
                        $"Please try another batch# for item code {itemCode} [x2]. Thanks", "OK");
                    return;
                }

                if (repliedCio.TransferBinBatchAccumulator == null)
                {
                    await new Dialog().DisplayAlert($"Batch# {batchNum} no found",
                        $"Please try another batch# for item code {itemCode} [x3]. Thanks", "OK");
                    return;
                }

                if (repliedCio.TransferBinBatchAccumulator.OnHandQty == 0)
                {
                    await new Dialog().DisplayAlert($"Batch# {batchNum}, " +
                        $"On hand quantity = {repliedCio.TransferBinBatchAccumulator.OnHandQty:N} " +
                        $"was insufficient",
                        $"Please try another batch# for item code {itemCode} [x4]. Thanks", "OK");
                    return;
                }

                if (repliedCio.TransferBinBatchAccumulator.OnHandQty < remaining)
                {
                    await new Dialog().DisplayAlert($"Batch# {batchNum}, " +
                        $"On hand quantity = {repliedCio.TransferBinBatchAccumulator.OnHandQty:N} " +
                        $"was insufficient, to remaining quantity {remaining} ",
                        $"Please try another batch# for item code {itemCode} [x5]. Thanks", "OK");
                    return;
                }

                decimal captureBatchQty = await CaptureQuantity($"Input batch# {batchNum} transfer Qty",
                    $"System Qty: {repliedCio.TransferBinBatchAccumulator.OnHandQty:N}, " +
                    $"Needed Qty: {remaining:N}", remaining);

                if (captureBatchQty != -1)
                {
                    return;
                }

                // then add into the list
                var batchBin = new TransferItemDetailBinM
                {
                    ItemCode = itemCode,
                    Qty = captureBatchQty,
                    Serial = string.Empty,
                    Batch = batchNum,
                    InternalSerialNumber = $"{repliedCio.TransferFoundBatch.SysNumber}",
                    ManufacturerSerialNumber = repliedCio.TransferFoundBatch.MnfSerial,
                    BinAbs = selectedBinObj.AbsEntry,
                    SnBMDAbs = repliedCio.TransferBinBatchAccumulator.SnBMDAbs,
                    WhsCode = fromWhsCode,
                    Direction = direction,
                    BinCode = selectedBinObj.BinCode
                };


                if (itemsSource == null) itemsSource = new List<TransferItemDetailBinM>();
                if (IsItemDuplicated(batchBin, this.itemManagedBy))
                {
                    DisplayMessage($"Item {itemCode} @Batch# {batchNum} found duplicated, Please try others batch#. Thanks.");
                    return;
                }

                AddItemToList(batchBin);
                RefreshListview();
                selected += captureBatchQty;
                CalculateRemainingQty();

                // reset local bin 
                selectedBinObj = null;
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
        /// Return true when duplicated in list
        /// </summary>
        /// <param name="checkItem"></param>
        /// <returns></returns>
        bool IsItemDuplicated(TransferItemDetailBinM checkItem, string itemManaged)
        {
            try
            {
                if (itemsSource == null) return false;
                if (itemsSource.Count == 0) return false;

                switch (itemManaged)
                {
                    case "Batch":
                    case "Serial":
                        {
                            if (binActivated.Equals("Yes"))
                            {
                                var isDup1 = itemsSource.Where(x =>
                                    x.CheckDistNum.Equals(checkItem.CheckDistNum) &&
                                    x.BinCode.Equals(checkItem.BinCode)).FirstOrDefault();

                                if (isDup1 == null) return false;
                                return true;
                            }

                            var isDup2 = itemsSource.Where(x => x.CheckDistNum.Equals(checkItem.CheckDistNum)).FirstOrDefault();
                            if (isDup2 == null) return false;
                            return true;
                        }
                    case "None":
                        {
                            if (binActivated.Equals("Yes"))
                            {
                                var isDup3 = itemsSource.Where(
                                    x => x.ItemCode.Equals(checkItem.ItemCode) &&
                                    x.BinCode.Equals(checkItem.BinCode)).FirstOrDefault();

                                if (isDup3 == null) return false;
                            }

                            var isDup4 = itemsSource.Where(x =>
                                x.ItemCode.Equals(checkItem.ItemCode)).FirstOrDefault();

                            if (isDup4 == null) return false;
                            return true;
                        }
                }
                return false;
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
                return true;
            }
        }

        /// <summary>
        /// Add new item into the list
        /// </summary>
        /// <param name="newItem"></param>
        void AddItemToList(TransferItemDetailBinM newItem)
        {
            if (itemsSource == null) itemsSource = new List<TransferItemDetailBinM>();
            itemsSource.Insert(0, newItem);
            ItemsSource = new ObservableCollection<TransferItemDetailBinM>(itemsSource);
        }

        /// <summary>
        /// 
        /// </summary>
        async Task<decimal> CaptureQuantity(string title, string message, decimal limit)
        {
            string qty = await new Dialog().DisplayPromptAsync(title, message, "Confirm", "Cancel", null, -1, Keyboard.Numeric, $"{limit:N}");
            if (string.IsNullOrWhiteSpace(qty))
            {
                DisplayToastShort("Invalid valued input");
                return -1;
            }

            if (qty.ToLower().Equals("cancel")) return -1;
            var isNumeric = decimal.TryParse(qty, out decimal resultVal);
            if (!isNumeric)
            {
                DisplayToastShort("Invalid input value, please input numeric value, Thanks.");
                return -1;
            }

            if (resultVal > limit)
            {
                DisplayToastShort($"Input value -> {resultVal:N} over limit -> {limit:N}, please input numeric value, Thanks.");
                return -1;
            }

            if (resultVal <= limit) return resultVal;

            return -1;
        }

        /// <summary>
        /// Refresh the listview
        /// </summary>
        void RefreshListview()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<TransferItemDetailBinM>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// Display Alert
        /// </summary>
        /// <param name="message"></param>
        async void DisplayMessage(string message) => await new Dialog().DisplayAlert("Alert", message, "OK");
        void DisplayToastShort(string message) => DependencyService.Get<IToastMessage>()?.ShortAlert(message);
        void DisplayToastLong(string message) => DependencyService.Get<IToastMessage>()?.LongAlert(message);
    }
}
