using Acr.UserDialogs;
using DbClass;
using Newtonsoft.Json;
using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.StandAloneTransfer;
using WMSApp.Models.Transfer1;
using WMSApp.ViewModels.Transfer1;
using WMSApp.Views.Share;
using WMSApp.Views.Transfer1;
using Xamarin.Forms;

namespace WMSApp.ViewModels.StandAloneTransfer
{
    public class StandAloneTransferLineFROMVM : INotifyPropertyChanged
    {
        #region View binding property
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Command CmdScan { get; set; }
        public Command CmdInput { get; set; }
        public Command CmdPromptSelectWarehouse { get; set; }
        public Command CmdSave { get; set; }
        public Command CmdCancel { get; set; }

        TransferLine selectedItem;
        public TransferLine SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;
                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public OWHS FromWhs { get; set; }
        string fromWarehouse;
        public string FromWarehouse
        {
            get => fromWarehouse;
            set
            {
                if (fromWarehouse == value) return;
                fromWarehouse = value;
                OnPropertyChanged(nameof(FromWarehouse));
            }
        }

        public OWHS ToWhs { get; set; }
        string toWarehouse;
        public string ToWarehouse
        {
            get => toWarehouse;
            set
            {
                if (toWarehouse == value) return;
                toWarehouse = value;
                OnPropertyChanged(nameof(ToWarehouse));
            }
        }

        List<TransferLine> itemsSource;
        public ObservableCollection<TransferLine> ItemsSource { get; set; }
        #endregion

        #region messaging center return addresses
        readonly string cameraReturnAddr = "20201024T2335_StartCameraScan";
        readonly string frmWarehouseReturnAddr = "20201024T2335_fromPickWarehouse";
        readonly string toWarehouseReturnAddr = "20201024T2335_toPickWarehouse";
        readonly string _BinSerialListAddrs = "_BinSerialListAddrs_20201028";
        readonly string _SerialListAddrs = "_SerialListAddrs_20201028T";
        readonly string _BinBatchListAddrs = "_BinBatchListAddrs_202028";
        readonly string _BatchListAddrs = "_BatchListAddrs_202028T1200";
        readonly string _BinItemAddrs = "_BinItemAddrs_20201028T1436";
        #endregion

        OITM currentOITM { get; set; } = null;
        INavigation Navigation { get; set; } = null;
        readonly string _FROM = "FROM";

        /// <summary>T
        /// The constructor
        /// </summary>
        /// <param name="navigation"></param>
        public StandAloneTransferLineFROMVM(INavigation navigation)
        {
            Navigation = navigation;
            InitCmds();
            SelectWarehouse("to");
            SelectWarehouse("from");
            InitCmds();
        }

        /// <summary>
        /// link to screen to remove the item
        /// </summary>
        /// <param name="removeItem"></param>
        public void RemoveItem(TransferLine removeItem)
        {
            if (itemsSource == null) return;
            int locId = itemsSource.IndexOf(removeItem);

            if (locId < 0) return;
            itemsSource.RemoveAt(locId);
            RefreshListView();
        }

        /// <summary>
        /// Start Select Warehouse
        /// </summary>
        void SelectWarehouse(string direction)
        {
            const string titleFrom = "Select [FROM] warehouse";
            const string titleTo = "Select [TO] warehouse";

            if (direction.Equals("from"))
            {
                MessagingCenter.Subscribe<OWHS>(this, frmWarehouseReturnAddr, (OWHS fromWhs) =>
                {
                    MessagingCenter.Unsubscribe<OWHS>(this, frmWarehouseReturnAddr);
                    FromWhs = fromWhs;
                    FromWarehouse = fromWhs.WhsCode;
                });
                Navigation.PushPopupAsync(new PickWarehousePopUpView(frmWarehouseReturnAddr, titleFrom, Color.Green));
                return;
            }

            //if (direction.Equals("to"))
            MessagingCenter.Subscribe<OWHS>(this, toWarehouseReturnAddr, (OWHS toWhs) =>
            {
                MessagingCenter.Unsubscribe<OWHS>(this, toWarehouseReturnAddr);
                ToWhs = toWhs;
                ToWarehouse = toWhs.WhsCode;
            });
            Navigation.PushPopupAsync(new PickWarehousePopUpView(toWarehouseReturnAddr, titleTo, Color.Yellow));
        }

        /// <summary>
        /// Init the command setup
        /// </summary>
        void InitCmds()
        {
            CmdScan = new Command(StartCameraScan);
            CmdInput = new Command(StartManualInput);
            CmdPromptSelectWarehouse = new Command<string>((string whsDir) => SelectWarehouse(whsDir));
            CmdSave = new Command(Save);
            CmdCancel = new Command(Cancel);
        }

        /// <summary>
        /// Perform to save to the server
        /// </summary>
        async void Save()
        {
            try
            {
                if (itemsSource == null)
                {
                    DisplayMessage("There is no transfer line being add into the document, " +
                        "please add some lines, and try save again. Thanks");
                    return;
                }

                if (itemsSource.Count == 0)
                {
                    DisplayMessage("There is no transfer line being add into the document, " +
                        "please add some lines, and try save again. Thanks");
                    return;
                }

                if (itemsSource == null)
                {
                    DisplayMessage("There is request lines not yet fulfilled, please try again later. [0]");
                    return;
                }

                var itemList = itemsSource.Where(x => x.FromTransQty > 0).ToList();
                if (itemList == null)
                {
                    DisplayMessage("There is request lines not yet fulfilled, please try again later. [1]");
                    return;
                }
                if (itemList.Count == 0)
                {
                    DisplayMessage("There is request lines not yet fulfilled, please try again later. [2]");
                    return;
                }

                // create onhold document 

                // save and create a onhold entry     
                Guid groupingGuid = Guid.NewGuid();
                var OnHoldRequest = new zwaHoldRequest
                {
                    #region hide ref code
                    //public int DocEntry { get; set; }
                    //public int DocNum { get; set; }
                    //public string Picker { get; set; }
                    //public DateTime TransDate { get; set; }
                    //public Guid HeadGuid { get; set; }
                    //public string Status { get; set; }
                    #endregion
                    DocEntry = -1,
                    DocNum = -1,
                    Picker = App.waUser,
                    TransDate = DateTime.Now,
                    HeadGuid = groupingGuid,
                    Status = "O"
                };

                // save the doc header
                var TransferDocHeader = new zwaTransferDocHeader
                {
                    #region reference code 
                    /*  public int Id { get; set; }
                    public DateTime DocDate { get; set; }
                    public DateTime TaxDate { get; set; }
                    public string FromWhsCode { get; set; }
                    public string ToWhsCode { get; set; }
                    public string JrnlMemo { get; set; }
                    public string Comments { get; set; }
                    public Guid Guid { get; set; }
                    public string DocNumber { get; set; }
                    public string DocStatus { get; set; }
                    public string LastErrorMessage { get; set; } */
                    #endregion

                    DocDate = DateTime.Now,
                    TaxDate = DateTime.Now,
                    FromWhsCode = FromWhs.WhsCode,
                    ToWhsCode = ToWhs.WhsCode,
                    JrnlMemo = string.Empty, //CurrentDoc.RequestDocument.JrnlMemo,
                    Comments = string.Empty, //CurrentDoc.RequestDocument.Comments,
                    Guid = groupingGuid,
                    DocNumber = string.Empty,
                    DocStatus = "O",
                    LastErrorMessage = string.Empty
                };

                // save the line 
                var TransferDocDetails = new List<zwaTransferDocDetails>();
                var TransferDocDetailsBins = new List<zwaTransferDocDetailsBin>();
                // save line from table
                foreach (var line in itemsSource)
                {
                    #region refer hide codes
                    //public int Id { get; set; }
                    //public Guid Guid { get; set; }
                    //public Guid LineGuid { get; set; }
                    //public string ItemCode { get; set; }
                    //public decimal Qty { get; set; }
                    //public string FromWhsCode { get; set; }
                    //public string ToWhsCode { get; set; }
                    //public string Serial { get; set; }
                    //public string Batch { get; set; }
                    //public string SourceDocBaseType { get; set; }
                    //public int SourceBaseEntry { get; set; }
                    //public int SourceBaseLine { get; set; }
                    #endregion

                    var lineGuid = Guid.NewGuid();
                    TransferDocDetails.Add(new zwaTransferDocDetails
                    {
                        Guid = groupingGuid,
                        LineGuid = lineGuid,
                        ItemCode = line.ItemCode,
                        ItemName = line.ItemName,
                        Qty = line.FromTransQty,
                        FromWhsCode = line.FromWhsCode,
                        ToWhsCode = line.ToWhsCode,
                        Serial = string.Empty,
                        Batch = string.Empty,
                        SourceDocBaseType = "",
                        SourceBaseEntry = -1,
                        SourceBaseLine = -1
                    });

                    #region hide codes
                    // bin 
                    //public int Id { get; set; }
                    //public Guid Guid { get; set; }
                    //public Guid LineGuid { get; set; }
                    //public string ItemCode { get; set; }
                    //public decimal Qty { get; set; }
                    //public string Serial { get; set; }
                    //public string Batch { get; set; }
                    //public string InternalSerialNumber { get; set; }
                    //public string ManufacturerSerialNumber { get; set; }
                    //public int BinAbs { get; set; }
                    //public int SnBMDAbs { get; set; }
                    //public string WhsCode { get; set; }
                    //public string Direction { get; set; }
                    #endregion
                    foreach (var bin in line.Lines)
                    {
                        TransferDocDetailsBins.Add(new zwaTransferDocDetailsBin
                        {
                            Guid = groupingGuid,
                            LineGuid = lineGuid,
                            ItemCode = line.ItemCode,
                            ItemName = line.ItemName,
                            Qty = bin.Qty,
                            Serial = bin.Serial,
                            Batch = bin.Batch,
                            InternalSerialNumber = bin.InternalSerialNumber,
                            ManufacturerSerialNumber = bin.ManufacturerSerialNumber,
                            BinAbs = bin.BinAbs,
                            BinCode = bin.BinCode,
                            SnBMDAbs = bin.SnBMDAbs,
                            WhsCode = bin.WhsCode,
                            Direction = bin.Direction
                        });
                    }
                }

                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "SaveStandAloneTransferOnHold",
                    TransferHoldRequest = OnHoldRequest,
                    TransferDocHeader = TransferDocHeader,
                    TransferDocDetails = TransferDocDetails?.ToArray(),
                    TransferDocDetailsBins = TransferDocDetailsBins?.ToArray()
                };

                // sent to server for other site to pick
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
                    DisplayMessage($"{content}\n{lastErrMessage}");
                    return;
                }

                var repliedCio = JsonConvert.DeserializeObject<Cio>(content);
                if (repliedCio == null)
                {
                    DisplayMessage("There is error reading the message from server replied, please try again later. Thanks");
                    return;
                }

                if (repliedCio.STAHoldRequestId < 0)
                {
                    DisplayMessage("There is fail to update the STA request, " +
                        "please try again later, or contact system administrator for help. Thanks");
                    return;
                }

                // build the waiting mechanism to check created doc and stop the looing
                DisplayMessage($"Inventory stand alone transfer # {repliedCio.STAHoldRequestId} sent and await for pickup.");
                Close();
            }
            catch (Exception excep)
            {
                DisplayMessage(excep.Message);
                Console.WriteLine(excep.ToString());
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// cancel and leave the leave 
        /// </summary>
        async void Cancel()
        {
            if (itemsSource == null) Close();
            if (itemsSource.Count == 0) Close();

            bool confirmLeave = await new Dialog().DisplayAlert(
                "Confirm leave without save ?",
                $"Leave without save will lost the data.", "Confirm", "Leave");

            if (confirmLeave) Close();            
            return;            
        }

        /// <summary>
        /// Close the 
        /// </summary>
        void Close() => Navigation.PopAsync();

        /// <summary>
        /// Start a scanner to capture the value for search
        /// </summary>
        void StartCameraScan()
        {
            MessagingCenter.Subscribe<string>(this, cameraReturnAddr, (string code) =>
            {
                if (string.IsNullOrWhiteSpace(code)) return;
                if (code.ToLower().Equals("cancel")) return;
                ProcessCode(code);
            });
            Navigation.PushAsync(new CameraScanView(cameraReturnAddr));
        }

        /// <summary>
        /// Starting a manual input and check with the server to detect the input
        /// </summary>
        async void StartManualInput()
        {
            string code = await new Dialog().DisplayPromptAsync("Input code"
                , "Input Item code, serial# or batch#"
                , "Ok"
                , "Cancel"
                , null
                , -1
                , Keyboard.Default
                , "");

            if (string.IsNullOrWhiteSpace(code)) return;
            if (code.ToLower().Equals("cancel")) return;
            ProcessCode(code);
        }

        /// <summary>
        /// Process the code from multiple source 
        /// send code to server to identified the code is item code, serial, batch or bin
        /// </summary>
        /// <param name="code"></param>
        async void ProcessCode(string code)
        {
            try
            {
                // to prevent fron and to warehouse
                if (FromWhs.BinActivat.Equals("N") && ToWhs.BinActivat.Equals("N") && FromWhs.WhsCode.Equals(ToWhs.WhsCode))
                {
                    DisplayMessage("Receipt (TO) warehouse cannot be identical to the release warehouse. " +
                        "Please pick a different warehouse. Thanks");
                    return;
                }

                UserDialogs.Instance.ShowLoading("A moment ...");
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "QueryCode",
                    TransferQueryCode = code
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
                    DisplayMessage($"{content}\n{lastErrMessage}\nOr not exist in any selected warehouse. Please try again later");
                    return;
                }

                // if success                
                var repliedCio = JsonConvert.DeserializeObject<Cio>(content);
                if (repliedCio == null)
                {
                    DisplayMessage("Error contacting server replied, please try again later");
                    return;
                }

                #region handler item code scan in
                if (repliedCio.TransferFoundItem != null)
                {
                    currentOITM = repliedCio.TransferFoundItem;
                    decimal NeededQty = await GetNeededQty(currentOITM);
                    if (NeededQty == -1) return;

                    var itemInfo = new TransferDataM
                    {
                        ItemCode = currentOITM.ItemCode,
                        ItemName = currentOITM.ItemName,
                        ItemManagedBy = (currentOITM.ManBtchNum.Equals("Y")) ? "Batch" :
                                    (currentOITM.ManSerNum.Equals("Y")) ? "Serial" : "None",
                        FromWhsCode = FromWhs.WhsCode,
                        BinActivated = FromWhs.BinActivat.Equals("Y") ? "Yes" : "No",
                        RequestedQty = NeededQty,
                        Item = currentOITM
                    };

                    LaunchAllocationScreen(itemInfo);
                    return;
                }
                #endregion
                DisplayMessage($"Scan in {code} does not match any record in server, please try other again, Thanks.");
            }
            catch (Exception excep)
            {
                DisplayMessage(excep.Message);
                Console.WriteLine(excep.ToString());
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Capture qty
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        async Task<decimal> GetNeededQty(OITM item)
        {
            try
            {
                string input = await new Dialog().DisplayPromptAsync(
                    $"Input needed qty for {item.ItemCode}",
                    $"{item.ItemName}",
                    "OK", "Cancel", null, -1, Keyboard.Numeric, "1.00");

                if (string.IsNullOrWhiteSpace(input)) return -1;
                if (input.ToLower().Equals("cancel")) return -1;

                bool isNumeric = decimal.TryParse(input, out decimal result);
                if (!isNumeric)
                {
                    DisplayMessage($"Please input numeric value for item {item.ItemCode}, " +
                        $"Please try again, Thanks");
                    return -1;
                }

                if (result <= 0)
                {
                    DisplayMessage($"Please input positive value for item {item.ItemCode}, " +
                       $"Please try again, Thanks");
                    return -1;
                }
                return result;
            }
            catch (Exception excep)
            {
                DisplayMessage(excep.Message);
                Console.WriteLine(excep.ToString());
                return -1;
            }
        }

        /// <summary>
        /// Based on the selected line 
        /// and launch different list for selection
        /// </summary>
        void LaunchAllocationScreen(TransferDataM line)
        {
            switch (line.ItemManagedBy)
            {
                case "Serial":
                    {
                        if (line.BinActivated.Equals("Yes"))
                        {
                            LaunchBinSerialList(line); // show list of serial bin
                            return;
                        }
                        LaunchSerialList(line); // show list of the serial #
                        break;
                    }
                case "Batch":
                    {
                        if (line.BinActivated.Equals("Yes"))
                        {
                            LaunchBinBatchList(line);
                            return;
                        }
                        LaunchBatchList(line); // BatchListView
                        break;
                    }
                case "None":
                    {
                        if (line.BinActivated.Equals("Yes"))
                        {
                            LaunchBinItemList(line); // show list of the warehouse bin qty for this item
                            return;
                        }
                        HandlerFromToWhs(line);
                        break;
                    }
            }
        }

        /// <summary>
        /// Show prompt and capture the item qty
        /// </summary>
        async void HandlerFromToWhs(TransferDataM line)
        {
            try
            {
                // check item qty enough for this transfer 
                decimal availableQty = await CheckItemWhsQty(line.ItemCode, line.FromWhsCode, line.RequestedQty);
                if (availableQty <= 0)
                {
                    DisplayMessage($"Item {line.ItemCode}\nInsufficient needed quantity {line.RequestedQty:N} " +
                        $"from warehouse {line.FromWhsCode}, available qty {availableQty:N}, please select other warehouse and try again, Thanks");
                    return;
                }

                if (availableQty < line.RequestedQty)
                {
                    DisplayMessage($"Item {line.ItemCode}\nInsufficient needed quantity {line.RequestedQty:N} " +
                      $"from warehouse {line.FromWhsCode}, available qty {availableQty:N}, please select other warehouse and try again, Thanks");
                    return;
                }

                // save into the list
                // handle the from warehouse
                var pick = new List<TransferItemDetailBinM>();
                Guid lineGuid = Guid.NewGuid();
                pick.Add(new TransferItemDetailBinM
                {
                    LineGuid = lineGuid,
                    ItemCode = line.ItemCode,
                    Qty = line.RequestedQty,
                    Serial = string.Empty,
                    Batch = string.Empty,
                    InternalSerialNumber = string.Empty,
                    ManufacturerSerialNumber = string.Empty,
                    BinAbs = -1,
                    SnBMDAbs = -1,
                    WhsCode = FromWhs.WhsCode,
                    Direction = _FROM
                });

                // create line 
                var newLine = new TransferLine
                {
                    ItemCode = line.ItemCode,
                    ItemName = line.ItemName,
                    DistNumber = string.Empty,
                    Lines = pick,
                    FromWhsCode = FromWhs.WhsCode,
                    FromTransQty = line.RequestedQty,
                    ToWhsCode = ToWhs.WhsCode,
                    ToTransQty = line.RequestedQty,
                    Item = line.Item
                };

                UpdateItem(newLine);
                DisplayToast("Saved");
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Check and replied the wahrehouse have stock request
        /// </summary>
        /// <param name="itemCode"></param>
        /// <param name="Whs"></param>
        /// <param name="neededQty"></param>
        /// <returns></returns>
        async Task<decimal> CheckItemWhsQty(string itemCode, string Whs, decimal neededQty)
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
                    request = "GetWarehouseItemQty",
                    QueryItemCode = itemCode,
                    QueryItemWhsCode = Whs
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
                    DisplayMessage($"{content}\n{lastErrMessage}\nOr not exist in any selected warehouse. Please try again later");
                    return -1;
                }

                // if success                
                var repliedCio = JsonConvert.DeserializeObject<Cio>(content);
                if (repliedCio == null)
                {
                    DisplayMessage("Error contacting server replied, please try again later");
                    return -1;
                }

                if (repliedCio.oITW == null)
                {
                    DisplayMessage("Error retrieve item warehouse information, please try again later");
                    return -1;
                }

                return repliedCio.oITW.OnHand;
            }
            catch (Exception excep)
            {
                DisplayMessage(excep.Message);
                Console.WriteLine(excep.ToString());
                return -1;
            }
        }

        /// <summary>
        /// Show list of the item qty in each bin of the warehouse
        /// </summary>
        void LaunchBinItemList(TransferDataM line)
        {
            try
            {
                MessagingCenter.Subscribe<List<OIBQ_Ex>>(this, _BinItemAddrs, (List<OIBQ_Ex> binItemList) =>
                {
                    MessagingCenter.Unsubscribe<List<OIBQ_Ex>>(this, _BinItemAddrs);
                    var pickedList = new List<TransferItemDetailBinM>();
                    var showList = string.Empty;
                    Guid lineGuid = Guid.NewGuid();
                    foreach (var item in binItemList)
                    {
                        pickedList.Add(new TransferItemDetailBinM
                        {
                            LineGuid = lineGuid,
                            ItemCode = line.ItemCode,
                            ItemName = line.ItemName,
                            Qty = item.TransferQty,
                            Serial = string.Empty,
                            Batch = string.Empty,
                            InternalSerialNumber = string.Empty,
                            ManufacturerSerialNumber = string.Empty,
                            BinCode = item.BinCode,
                            BinAbs = item.BinAbs,
                            SnBMDAbs = -1,
                            WhsCode = FromWhs.WhsCode,
                            Direction = _FROM
                        });
                        showList += $"{item.BinCode}->{item.TransferQty:N}\n";
                    }

                    // create line 
                    var newLine = new TransferLine
                    {
                        ItemCode = line.ItemCode,
                        ItemName = line.ItemName,
                        DistNumber = showList,
                        Lines = pickedList,
                        FromWhsCode = FromWhs.WhsCode,
                        FromTransQty = binItemList.Sum(x => x.TransferQty),
                        ToWhsCode = ToWhs.WhsCode,
                        ToTransQty = binItemList.Sum(x => x.TransferQty),
                        Item = line.Item
                    };

                    UpdateItem(newLine);
                    DisplayToast("Saved");
                });

                Navigation.PushAsync(
                    new BinItemListView(
                        _BinItemAddrs, line.ItemCode, line.ItemName, 
                        line.FromWhsCode, (int)line.RequestedQty));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Show list of batch reside in the warehouse
        /// </summary>
        /// <param name="line"></param>
        void LaunchBatchList(TransferDataM line)
        {
            try
            {
                MessagingCenter.Subscribe<List<OBTQ_Ex>>(this, _BatchListAddrs, (List<OBTQ_Ex> batchList) =>
                {
                    MessagingCenter.Unsubscribe<List<OBTQ_Ex>>(this, _BatchListAddrs);
                    var pickedList = new List<TransferItemDetailBinM>();
                    var showList = string.Empty;

                    #region prepare the from warehouse line 
                    Guid lineGuid = Guid.NewGuid();
                    foreach (var batch in batchList)
                    {
                        pickedList.Add(new TransferItemDetailBinM
                        {
                            LineGuid = lineGuid,
                            ItemCode = line.ItemCode,
                            Qty = batch.TransferBatchQty,
                            Serial = string.Empty,
                            Batch = batch.DistNumber,
                            InternalSerialNumber = string.Empty,
                            ManufacturerSerialNumber = string.Empty,
                            BinAbs = -1,
                            SnBMDAbs = -1,
                            WhsCode = FromWhs.WhsCode,
                            Direction = _FROM
                        });
                        showList += $"{batch.DistNumber}->{batch.TransferBatchQty}\n";
                    }
                    #endregion

                    #region create line and save with from and to warehouse
                    // create line 
                    var newLine = new TransferLine
                    {
                        ItemCode = line.ItemCode,
                        ItemName = line.ItemName,
                        DistNumber = showList,
                        Lines = pickedList,
                        FromWhsCode = FromWhs.WhsCode,
                        FromTransQty = batchList.Sum(x => x.TransferBatchQty),
                        ToWhsCode = ToWhs.WhsCode,
                        ToTransQty = batchList.Sum(x => x.TransferBatchQty),
                        Item = line.Item
                    };

                    UpdateItem(newLine); // create the tansfer line and process to to warehouse
                    DisplayToast("Saved");
                    #endregion
                });

                Navigation.PushAsync(
                    new BatchListView(
                        _BatchListAddrs, line.ItemCode, line.ItemName,
                        line.FromWhsCode, (int)line.RequestedQty));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Show list of the bin batch for user selection
        /// </summary>
        /// <param name="line"></param>
        void LaunchBinBatchList(TransferDataM line)
        {
            try
            {
                MessagingCenter.Subscribe<List<OBBQ_Ex>>(this, _BinBatchListAddrs, (List<OBBQ_Ex> binBatchList) =>
                {
                    MessagingCenter.Unsubscribe<List<OBBQ_Ex>>(this, _BinBatchListAddrs);
                    List<TransferItemDetailBinM> pickedList = new List<TransferItemDetailBinM>();
                    var showList = string.Empty;
                    Guid lineGuid = Guid.NewGuid();
                    foreach (var batch in binBatchList)
                    {
                        pickedList.Add(new TransferItemDetailBinM
                        {
                            LineGuid = lineGuid,
                            ItemCode = line.ItemCode,
                            ItemName = line.ItemName,
                            Qty = batch.TransferBatchQty,
                            Serial = string.Empty,
                            Batch = batch.DistNumber,
                            InternalSerialNumber = string.Empty,
                            ManufacturerSerialNumber = string.Empty,
                            BinCode = batch.BinCode,
                            BinAbs = batch.BinAbs,
                            SnBMDAbs = batch.SnBMDAbs,
                            WhsCode = FromWhs.WhsCode,
                            Direction = _FROM
                        });
                        showList += $"{batch.BinCode}->{batch.DistNumber}->{batch.TransferBatchQty:N}\n";
                    }

                    // create line 
                    var newLine = new TransferLine
                    {
                        ItemCode = line.ItemCode,
                        ItemName = line.ItemName,
                        DistNumber = showList,
                        Lines = pickedList,
                        FromWhsCode = FromWhs.WhsCode,
                        FromTransQty = binBatchList.Sum(x => x.TransferBatchQty),
                        ToWhsCode = ToWhs.WhsCode,
                        ToTransQty = binBatchList.Sum(x => x.TransferBatchQty),
                        Item = line.Item
                    };

                    UpdateItem(newLine); // create the tansfer line and process to to warehouse
                    DisplayToast("Saved");
                });

                Navigation.PushAsync(
                    new BinBatchListView(
                        _BinBatchListAddrs,line.ItemCode, line.ItemName, 
                        line.FromWhsCode, (int)line.RequestedQty));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Launch list of serial 
        /// </summary>
        void LaunchSerialList(TransferDataM line)
        {
            try
            {
                MessagingCenter.Subscribe<List<OSRQ_Ex>>(this, _SerialListAddrs, (List<OSRQ_Ex> serialList) =>
                {
                    MessagingCenter.Unsubscribe<List<OSRQ_Ex>>(this, _SerialListAddrs);
                    List<TransferItemDetailBinM> pickedList = new List<TransferItemDetailBinM>();
                    var showList = string.Empty;

                    #region create the record for from warehouse
                    Guid lineGuid = Guid.NewGuid();
                    foreach (var serial in serialList)
                    {
                        pickedList.Add(new TransferItemDetailBinM
                        {
                            LineGuid = lineGuid,
                            ItemCode = line.ItemCode,
                            Qty = 1,
                            Serial = serial.DistNumber,
                            Batch = string.Empty,
                            InternalSerialNumber = string.Empty,
                            ManufacturerSerialNumber = string.Empty,
                            BinAbs = -1,
                            SnBMDAbs = serial.SnBMDAbs,
                            WhsCode = FromWhs.WhsCode,
                            Direction = _FROM
                        });
                        showList += $"{serial.DistNumber}\n";
                    }

                    #endregion

                    // create line 
                    var newLine = new TransferLine
                    {
                        ItemCode = line.ItemCode,
                        ItemName = line.ItemName,
                        DistNumber = showList,
                        Lines = pickedList,
                        FromWhsCode = FromWhs.WhsCode,
                        FromTransQty = serialList.Count,
                        ToWhsCode = ToWhs.WhsCode,
                        ToTransQty = serialList.Count,
                        Item = line.Item
                    };

                    UpdateItem(newLine); // create the tansfer line and process to to warehouse
                    DisplayToast("Saved.");
                });

                // get list of selectedSerial to prevent same serial being selected
                // get list of selected serial 
                // prevent to select on the next screen
                List<string> selectedSerial = GetSelectedSerials();

                Navigation.PushAsync(new SerialListView(_SerialListAddrs,
                    line.ItemCode, line.ItemName, line.FromWhsCode, (int)line.RequestedQty, selectedSerial));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Show bin + serial list in that warehouse
        /// </summary
        void LaunchBinSerialList(TransferDataM line)
        {
            try
            {
                MessagingCenter.Subscribe<List<OSBQ_Ex>>(this, _BinSerialListAddrs, (List<OSBQ_Ex> binSerialList) =>
                {
                    MessagingCenter.Unsubscribe<List<OSBQ_Ex>>(this, _BinSerialListAddrs);
                    List<TransferItemDetailBinM> pickedList = new List<TransferItemDetailBinM>();
                    var showList = string.Empty;
                    Guid lineGuid = Guid.NewGuid();
                    foreach (var serial in binSerialList)
                    {
                        pickedList.Add(new TransferItemDetailBinM
                        {
                            LineGuid = lineGuid,
                            ItemCode = line.ItemCode,
                            ItemName = line.ItemName,
                            Qty = 1,
                            Serial = serial.DistNumber,
                            Batch = string.Empty,
                            InternalSerialNumber = string.Empty,
                            ManufacturerSerialNumber = string.Empty,
                            BinCode = serial.BinCode,
                            BinAbs = serial.BinAbs,
                            SnBMDAbs = serial.SnBMDAbs,
                            WhsCode = FromWhs.WhsCode,
                            Direction = _FROM
                        });
                        showList += $"{serial.BinCode}->{serial.DistNumber}\n";
                    }

                    // create line 
                    var newLine = new TransferLine
                    {
                        ItemCode = line.ItemCode,
                        ItemName = line.ItemName,
                        DistNumber = showList,
                        Lines = pickedList,
                        FromWhsCode = FromWhs.WhsCode,
                        FromTransQty = binSerialList.Count,
                        ToWhsCode = ToWhs.WhsCode,
                        ToTransQty = binSerialList.Count,
                        Item = line.Item
                    };

                    UpdateItem(newLine); // create the tansfer line and process to to warehouse
                    DisplayToast("Saved");
                });

                // get list of selected serial 
                // prevent to select on the next screen
                List<string> selectedSerial = GetSelectedSerials();

                Navigation.PushAsync(new BinSerialListView(_BinSerialListAddrs,
                    line.ItemCode, line.ItemName, line.FromWhsCode, (int)line.RequestedQty, selectedSerial));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }
        /// <summary>
        /// Return of list of selected serial 
        /// to avoid serial being selected again
        /// </summary>
        List<string> GetSelectedSerials()
        {
            try
            {
                if (itemsSource == null) return null;
                if (itemsSource.Count == 0) return null;

                var selectedSerial = new List<string>();
                for (int x = 0; x < itemsSource.Count; x++)
                {
                    if (itemsSource[x].Lines == null) continue;
                    if (itemsSource[x].Lines.Count == 0) continue;

                    for (int y = 0; y < itemsSource[x].Lines.Count; y++)
                    {
                        if (string.IsNullOrWhiteSpace(itemsSource[x].Lines[y].Serial)) continue;
                        selectedSerial.Add(itemsSource[x].Lines[y].Serial);
                    }
                }
                return selectedSerial;
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
                return null;
            }
        }

        /// <summary>
        /// Update / refresh the list line
        /// </summary>
        /// <param name="docLine"></param>
        void UpdateItem(TransferLine newLine)
        {
            // no duplication check 
            if (itemsSource == null) itemsSource = new List<TransferLine>();
            itemsSource.Add(newLine);
            RefreshListView();
        }

        /// <summary>
        /// for refresh the listview
        /// </summary>
        void RefreshListView()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<TransferLine>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// Display message onto the screen
        /// </summary>
        /// <param name="message"></param>
        async void DisplayMessage(string message) => await new Dialog().DisplayAlert("Alert", message, "OK");

        void DisplayToast(string message) => DependencyService.Get<IToastMessage>()?.ShortAlert(message);
    }
}
