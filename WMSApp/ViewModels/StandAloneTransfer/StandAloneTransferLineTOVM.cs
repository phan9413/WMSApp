using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using WMSApp.Class;
using System.Collections.ObjectModel;
using WMSApp.Models.SAP;
using Acr.UserDialogs;
using Newtonsoft.Json;
using System.Linq;
using WMSApp.Views.Share;
using System.Threading;
using WMSApp.Interface;
using DbClass;
using WMSApp.Models.Transfer1;
using WMSApp.Views.Transfer1;
using WMSApp.ViewModels.Transfer1;
using WMSApp.Class.Helper;

namespace WMSApp.ViewModels.StandAloneTransfer
{
    public class StandAloneTransferLineTOVM : INotifyPropertyChanged
    {
        #region Property
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string pName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pName));
        public Command CmdScanItemCode { get; set; }
        public Command CmdInputItemCode { get; set; }
        public Command CmdSave { get; set; }
        public Command CmdCancel { get; set; }

        zwaTransferDocDetails currentDocLine;
        zwaTransferDocDetails selectedItem;
        public zwaTransferDocDetails SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;
                currentDocLine = value;

                if (selectedItem.Qty > 0) ResetCellToZero(value);

                ProcessItemCode(selectedItem.ItemCode);

                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        string viewTitle;
        public string ViewTitle
        {
            get => viewTitle;
            set
            {
                if (viewTitle == value) return;
                viewTitle = value;
                OnPropertyChanged(nameof(ViewTitle));
            }
        }

        List<zwaTransferDocDetails> itemsSource;
        public ObservableCollection<zwaTransferDocDetails> ItemsSource { get; set; }

        string direction;
        public string Direction
        {
            get => direction;
            set
            {
                if (direction == value) return;
                direction = value;
                OnPropertyChanged(nameof(Direction));
            }
        }
        #endregion

        #region Private declation     
        INavigation Navigation { get; set; } = null;
        zwaHoldRequest CurrentDoc { get; set; } = null;
        zwaRequest currentRequest { get; set; } = null;        
        string WhsDirection { get; set; } = string.Empty;

        //  cater select to warehouse        
        readonly string _SerialToBinAddrs = "20201021_SerialToBin_STA";
        readonly string _SerialConfirmAddrs = "20201021_SerialConfirm_STA";
        readonly string _BatchToBinAddrs = "20201021_BatchToBinAddrs_STA";
        readonly string _BatchConfirmAddrs = "20201021_BatchConfirm_STA";
        readonly string _ItemToBinAddrs = "20201021_ItemToBin_STA";
        readonly string _TO = "TO";
        readonly string _PopScreenAddr = "_PopScreenAddr_201207_StandAloneTransLine";
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="navigation"></param>
        /// <param name="selectedDoc"></param>
        public StandAloneTransferLineTOVM(INavigation navigation, zwaHoldRequest selectedDoc, string direction)
        {
            Navigation = navigation;
            CurrentDoc = selectedDoc;
            ViewTitle = $"Request #{selectedDoc.Id}";
            WhsDirection = direction;
            Direction = (direction.Equals("FROM")) ? $"Pick item {direction} Operations" : $"Put item {direction} Operations";
            InitCmd();
            LoadDocLine();
        }

        /// <summary>
        /// Load rqeust line from web server
        /// </summary>
        async void LoadDocLine()
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
                    request = "LoadSTADocLines",  // STA - Standalone = SAT - Stand Alone
                    TransferDocRequestGuid = CurrentDoc.HeadGuid
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
                    DisplayAlert($"{content}\n{lastErrMessage}");
                    return;
                }

                var retBag = JsonConvert.DeserializeObject<Cio>(content);
                if (retBag.dtozmwTransferDocDetails == null)
                {
                    DisplayToast("There is error reading the stand alone transfer lines, " +
                        "please try again later or contact system administrator for help, Thanks [N].");
                    return;
                }

                if (retBag.dtozmwTransferDocDetails.Length == 0)
                {
                    DisplayToast("There is error reading the stand alone transfer lines, " +
                        "please try again later or contact system administrator for help, Thanks [Z].");
                    return;
                }

                itemsSource = new List<zwaTransferDocDetails>();
                itemsSource.AddRange(retBag.dtozmwTransferDocDetails);

                ResetListView();
                return;
            }
            catch (Exception excep)
            {
                DisplayAlert(excep.Message);
                Console.WriteLine(excep.ToString());
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Init command property binding
        /// </summary>
        void InitCmd()
        {
            CmdScanItemCode = new Command(HandlerScanItem);
            CmdInputItemCode = new Command(HandlerInputItem);
            CmdSave = new Command(Save);
            CmdCancel = new Command(Cancel);
        }

        /// <summary>
        /// Perform save into the svr for create transfer document
        /// </summary>
        void Save()
        {
            try
            {
                if (itemsSource == null)
                {
                    DisplayAlert("There is request lines not yet fulfilled, please try again later. [0]");
                    return;
                }

                var itemList = itemsSource.Where(x => x.Qty > 0).ToList();
                if (itemList == null)
                {
                    DisplayAlert("There is request lines not yet fulfilled, please try again later. [1]");
                    return;
                }
                if (itemList.Count == 0)
                {
                    DisplayAlert("There is request lines not yet fulfilled, please try again later. [2]");
                    return;
                }

                if (WhsDirection.Equals(_TO)) // handle the to warehouse
                {
                    HandlerCreateTransferDoc();
                    return;
                }
            }
            catch (Exception excep)
            {
                DisplayAlert($"{excep.Message}");
                Console.WriteLine(excep.ToString());
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Handle save the create transfer doc
        /// </summary>
        async void HandlerCreateTransferDoc()
        {
            try
            {                
                
                UserDialogs.Instance.ShowLoading("Awaiting Create Inventory Transfer ..");

                var processList = itemsSource.Where(x => x.Picked != null).ToList();

                // create request object 
                // create the to bin line
                // save the line                
                // save line from table
                var TransferDocDetailsBins = new List<zwaTransferDocDetailsBin>();
                var TransferDocDetails = new List<zwaTransferDocDetails>();

                foreach (var line in processList)
                {
                    currentDocLine = line;                    
                    var lines = line.GetList();
                    var newLine = new zwaTransferDocDetails
                    {
                        Guid = line.Guid,
                        LineGuid = line.LineGuid,
                        ItemCode = line.ItemCode,
                        ItemName = line.ItemName,
                        Qty = line.Qty,
                        ActualReceiptQty = lines.Where(x => x.Direction.Equals("TO")).Sum(x => x.Qty),
                        FromWhsCode = line.FromWhsCode,
                        ToWhsCode = line.ToWhsCode,
                        Serial = line.Serial,
                        Batch = line.Batch,
                        SourceDocBaseType = line.SourceDocBaseType,
                        SourceBaseEntry = line.SourceBaseEntry,
                        SourceBaseLine = line.SourceBaseLine, 
                    };

                    TransferDocDetails.Add(newLine);
                    TransferDocDetailsBins.AddRange(lines);
                }

                currentRequest = new zwaRequest
                {
                    sapUser = App.waUser,
                    request = "Create StdAloneTransfer",
                    guid = CurrentDoc.HeadGuid,
                    status = "ONHOLD",
                    phoneRegID = App.waToken,
                    tried = 0,
                    popScreenAddress = _PopScreenAddr
                };

                App.RequestList.Add(currentRequest);

                var cioRequest1 = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "CreateTransfer",
                    TransferDocDetailsBins = TransferDocDetailsBins?.ToArray(),
                    dtoRequest = currentRequest
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest1, "Transfer1");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    DisplayAlert($"{content}\n{lastErrMessage}");
                    return;
                }


                if (!App.IsContinueRequestChecking)
                {
                    App.IsContinueRequestChecking = true;
                    App.StartDocStatusCheckTimer();
                }

                DisplayToast("Request sent.");
                RegisterPopScreen();
            }
            catch (Exception excep)
            {
                DisplayAlert($"{excep}");
                Console.WriteLine(excep.Message);
            }
        }

        void RegisterPopScreen()
        {
            MessagingCenter.Subscribe(this, _PopScreenAddr, (string message) =>
            {
                MessagingCenter.Unsubscribe<string>(this, _PopScreenAddr);
                Navigation.PopAsync();
            });
        }

        //void CheckDocStatus()
        //{
        //    var callerAddress = "CreateSTAInvTrans_DocCheck";
        //    MessagingCenter.Subscribe(this, callerAddress, (string result) =>
        //    {
        //        MessagingCenter.Unsubscribe<string>(this, callerAddress);
        //        UserDialogs.Instance.HideLoading();

        //        if (result.Equals("fail")) return;
        //        if (result.Equals("success")) Navigation.PopAsync();

        //        App.RequestList.Remove(currentRequest);
        //    });

        //    var checker = new RequestCheckHelper { Requests = currentRequest, ReturnAddress = callerAddress };
        //    checker.StartChecking();
        //}

        /// <summary>
        /// Handler the cancel button tap
        /// </summary>
        async void Cancel()
        {
            if (itemsSource == null) await Navigation.PopAsync();
            if (itemsSource.Count == 0) await Navigation.PopAsync();
            var confirmLeave = await new Dialog().DisplayAlert("Confirm leaving?"
                , $"There are {itemsSource.Count} in list, leaving as this will lost the data save in?", "Confirm Leave", "Cancel");

            if (confirmLeave) await Navigation.PopAsync();
        }
       
        /// <summary>
        /// Handler start the scanner event and return the item code
        /// </summary>
        async void HandlerScanItem()
        {
            try
            {
                string returnAddress = "TransferListLineHandlerScanItem_SAT";
                MessagingCenter.Subscribe<string>(this, returnAddress, (string itemCode) =>
                {
                    MessagingCenter.Unsubscribe<string>(this, returnAddress);
                    if (string.IsNullOrWhiteSpace(itemCode)) return;
                    if (itemCode.Equals("cancel")) return;
                    ProcessItemCode(itemCode); // itemCode return from camara scanner
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
        /// Handle the manual input 
        /// </summary>
        async void HandlerInputItem()
        {
            try
            {
                var capturedItemCode = await new Dialog().DisplayPromptAsync("Input Item code", "", "OK", "Cancel");
                if (string.IsNullOrWhiteSpace(capturedItemCode)) return;
                if (capturedItemCode.ToLower().Equals("cancel")) return;
                ProcessItemCode(capturedItemCode);
            }
            catch (Exception excep)
            {
                Console.WriteLine($"{excep}");
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// When user click on the cell, reset the cell line to zero
        /// </summary>
        void ResetCellToZero(zwaTransferDocDetails selected)
        {
            int locId = itemsSource.IndexOf(selected);
            if (locId < 0) return;
            itemsSource[locId].CellCompleteColor = Color.Red;
            itemsSource[locId].Qty = 0;
            itemsSource[locId].ToBins = null;
            itemsSource[locId].Picked = null;
        }

        /// <summary>
        /// Process the item code
        /// </summary>
        /// <param name="itemCode"></param>
        async void ProcessItemCode(string itemCode)
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
                    request = "CheckItemCodeExistance",
                    QueryItemCode = itemCode
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Transfer");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    DisplayAlert($"{content}\n{lastErrMessage}");
                    return;
                }

                var retBag = JsonConvert.DeserializeObject<Cio>(content);
                if (retBag.Item == null) return;
                OITM item = retBag.Item;

                var foundItem = itemsSource.Where(x => x.ItemCode.Equals(item.ItemCode)).FirstOrDefault();
                if (foundItem == null)
                {
                    DisplayAlert($"The input code {item.ItemCode} was not found in SAT list, Please try again later, Thanks.[N]");
                    return;
                }

                var toWhs = App.Warehouses.Where(x => x.WhsCode.Equals(currentDocLine.ToWhsCode)).FirstOrDefault();
                if (toWhs == null) return;

                var itemInfo1 = new TransferDataM
                {
                    ItemCode = currentDocLine.ItemCode,
                    ItemName = item.ItemName,
                    ItemManagedBy = (item.ManBtchNum.Equals("Y")) ? "Batch" :
                                    (item.ManSerNum.Equals("Y")) ? "Serial" : "None",
                    FromWhsCode = toWhs.WhsCode,
                    BinActivated = toWhs.BinActivat.Equals("Y") ? "Yes" : "No",
                    RequestedQty = currentDocLine.Qty,
                };
                LaunchAllocationScreenToWhs(itemInfo1);
                return;
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
        /// Use to handle the to whs operation step
        /// </summary>
        /// <param name="line"></param>
        void LaunchAllocationScreenToWhs(TransferDataM line)
        {
            try
            {
                switch (line.ItemManagedBy)
                {
                    case "Serial":
                        {
                            if (line.BinActivated.Equals("Yes"))
                            {
                                LaunchSerialToBinOpr(line); // show list of bin to site in the serial
                                return;
                            }
                            // else                                                        
                            LaunchSerialConfirmView(line); // show a list of serial to check and received
                            break;
                        }
                    case "Batch":
                        {
                            if (line.BinActivated.Equals("Yes"))
                            {
                                LaunchBatchToBinOpr(line);
                                return;
                            }
                            LaunchBatchConfirmView(line); // BatchListView
                            break;
                        }
                    case "None":
                        {
                            if (line.BinActivated.Equals("Yes"))
                            {
                                LaunchBinItemOpr(line); // show list of the warehouse bin qty for this item                                
                                return;
                            }
                            // else 
                            CaptureQtyInput(line);
                            break;
                        }
                }
            }
            catch (Exception excep)
            {
                Console.WriteLine($"{excep}");
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Show list bin for qty allocation (non manage item)
        /// </summary>
        /// <param name="line"></param>
        void LaunchBinItemOpr(TransferDataM line)
        {
            try
            {
                MessagingCenter.Subscribe<List<OBIN_Ex>>(this, _ItemToBinAddrs,
                    (List<OBIN_Ex> itemToBins) =>
                    {
                        MessagingCenter.Unsubscribe<List<OBIN_Ex>>(this, _ItemToBinAddrs);
                        List<TransferItemDetailBinM> pickedList = new List<TransferItemDetailBinM>();
                        var showList = string.Empty;

                        foreach (var bin in itemToBins)
                        {
                            pickedList.Add(new TransferItemDetailBinM
                            {
                                LineGuid = Guid.NewGuid(), // need to replace by the head and line guid
                                ItemCode = line.ItemCode,
                                ItemName = line.ItemName,
                                Qty = bin.BinQty,
                                Serial = string.Empty,
                                Batch = string.Empty,
                                InternalSerialNumber = string.Empty,
                                ManufacturerSerialNumber = string.Empty,
                                BinCode = bin.oBIN.BinCode,
                                BinAbs = bin.oBIN.AbsEntry,
                                SnBMDAbs = -1,
                                WhsCode = currentDocLine.ToWhsCode,
                                Direction = _TO
                            });
                            showList += $"{bin.BinQty:N}->{bin.oBIN.BinCode}\n";
                        }

                        currentDocLine.ShowList = showList;
                        currentDocLine.ItemName = line.ItemName;

                        UpdateItem(currentDocLine, pickedList);
                    });

                //Navigation.PushAsync(
                //    new ItemToBinSelectView(
                //        currentDocLine.FromWhsCode, _ItemToBinAddrs, line.ItemCode, line.ItemName, line.RequestedQty));

                Navigation.PushAsync(
                 new ItemToBinView(currentDocLine.Guid, 
                     currentDocLine.LineGuid, 
                     currentDocLine.ItemCode, 
                     currentDocLine.ItemName, 
                     currentDocLine.ToWhsCode, 
                     _ItemToBinAddrs));
            }
            catch (Exception excep)
            {
                Console.WriteLine($"{excep}");
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Show list of from batch and and bin operation allocation
        /// </summary>
        /// <param name="line"></param>
        void LaunchBatchToBinOpr(TransferDataM line)
        {
            try
            {
                MessagingCenter.Subscribe<List<zwaTransferDocDetailsBin>>(this, _BatchToBinAddrs,
                    (List<zwaTransferDocDetailsBin> batchtoBins) =>
                    {
                        MessagingCenter.Unsubscribe<List<zwaTransferDocDetailsBin>>(this, _BatchToBinAddrs);
                        List<TransferItemDetailBinM> pickedList = new List<TransferItemDetailBinM>();
                        var showList = string.Empty;
                        var batchlist = string.Empty;

                        foreach (var batch in batchtoBins)
                        {
                            if (batch.Bins == null) continue;
                            batchlist = $"{batch.Batch}\n";

                            foreach (var batchBin in batch.Bins)
                            {
                                pickedList.Add(new TransferItemDetailBinM
                                {
                                    LineGuid = Guid.NewGuid(),
                                    ItemCode = line.ItemCode,
                                    ItemName = line.ItemName,
                                    Qty = batchBin.BatchTransQty,
                                    Serial = string.Empty,
                                    Batch = batch.Batch,
                                    InternalSerialNumber = string.Empty,
                                    ManufacturerSerialNumber = string.Empty,
                                    BinAbs = batchBin.oBIN.AbsEntry,
                                    BinCode = batchBin.oBIN.BinCode,
                                    SnBMDAbs = batch.SnBMDAbs,
                                    WhsCode = currentDocLine.ToWhsCode,
                                    Direction = _TO
                                });
                                batchlist += $"   {batchBin.BatchTransQty:N} -> {batchBin.oBIN.BinCode}\n";
                            }
                            showList += batchlist;
                        }

                        currentDocLine.ShowList = showList;
                        currentDocLine.ItemName = line.ItemName;
                        UpdateItem(currentDocLine, pickedList);
                    });

                /// need to settle it 
                Navigation.PushAsync(
                    new BatchToBinView(
                        currentDocLine.Guid, currentDocLine.LineGuid, currentDocLine.ToWhsCode, 
                        _BatchToBinAddrs, currentDocLine.ItemCode, currentDocLine.ItemName));
            }
            catch (Exception excep)
            {
                Console.WriteLine($"{excep}");
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Handle no bin batch
        /// </summary>
        /// <param name="line"></param>
        void LaunchBatchConfirmView(TransferDataM line)
        {
            // launch view for user to receipt 
            try
            {
                MessagingCenter.Subscribe<List<zwaTransferDocDetailsBin>>(this, _BatchConfirmAddrs,
                    (List<zwaTransferDocDetailsBin> confirmBatches) =>
                    {
                        MessagingCenter.Unsubscribe<List<zwaTransferDocDetailsBin>>(this, _BatchConfirmAddrs);
                        List<TransferItemDetailBinM> pickedList = new List<TransferItemDetailBinM>();
                        var showList = string.Empty;

                        foreach (var batch in confirmBatches)
                        {
                            pickedList.Add(new TransferItemDetailBinM
                            {
                                LineGuid = Guid.NewGuid(), // to be replace @ server site to match the from warehouse
                                ItemCode = line.ItemCode,
                                ItemName = line.ItemName,
                                Qty = batch.Qty,
                                Serial = string.Empty,
                                Batch = batch.Batch,
                                InternalSerialNumber = string.Empty,
                                ManufacturerSerialNumber = string.Empty,
                                BinCode = string.Empty,                                
                                BinAbs = -1,
                                SnBMDAbs = -1,
                                WhsCode = currentDocLine.ToWhsCode,
                                Direction = _TO
                            });
                            showList += $"{batch.Batch} -> {batch.Qty:N}\n";
                        }
                        
                        currentDocLine.ShowList = showList;
                        currentDocLine.ItemName = line.ItemName;
                        UpdateItem(currentDocLine, pickedList);
                    });

                // need to query the by the STA detail bin
                // get the 

                Navigation.PushAsync(new BatchesConfirmView(currentDocLine.Guid, currentDocLine.LineGuid,
                    currentDocLine.ToWhsCode, _BatchConfirmAddrs));
            }
            catch (Exception excep)
            {
                Console.WriteLine($"{excep}");
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Handler serial to warehouse confirmation screen
        /// </summary>
        /// <param name="line"></param>
        void LaunchSerialConfirmView(TransferDataM line)
        {
            try
            {
                MessagingCenter.Subscribe<List<zwaTransferDocDetailsBin>>(this, _SerialConfirmAddrs,
                    (List<zwaTransferDocDetailsBin> serialtoBins) =>
                    {
                        MessagingCenter.Unsubscribe<List<zwaTransferDocDetailsBin>>(this, _SerialConfirmAddrs);

                        List<TransferItemDetailBinM> pickedList = new List<TransferItemDetailBinM>();
                        var showList = string.Empty;
                        foreach (var serial in serialtoBins)
                        {
                            pickedList.Add(new TransferItemDetailBinM
                            {
                                LineGuid = Guid.NewGuid(), // to be replace @ server site to match the from warehouse
                                ItemCode = line.ItemCode,
                                ItemName = line.ItemName,
                                Qty = 1,
                                Serial = serial.Serial,
                                Batch = string.Empty,
                                InternalSerialNumber = string.Empty,
                                ManufacturerSerialNumber = string.Empty,
                                BinAbs = -1,
                                BinCode = string.Empty,
                                SnBMDAbs = -1,
                                WhsCode = currentDocLine.ToWhsCode,
                                Direction = _TO
                            });
                            showList += $"{serial.Serial}\n";
                        }

                        currentDocLine.ShowList = showList;
                        currentDocLine.ItemName = line.ItemName;
                        UpdateItem(currentDocLine, pickedList);
                    });

                Navigation.PushAsync(new SerialsConfirmView(currentDocLine.Guid, 
                    currentDocLine.LineGuid,
                    currentDocLine.ItemCode,
                    currentDocLine.ItemName,
                    currentDocLine.ToWhsCode, 
                    _SerialConfirmAddrs));
            }
            catch (Exception excep)
            {
                Console.WriteLine($"{excep}");
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Show page to handler serial bin list
        /// </summary>
        void LaunchSerialToBinOpr(TransferDataM line)
        {
            try
            {
                MessagingCenter.Subscribe<List<zwaTransferDocDetailsBin>>(this, _SerialToBinAddrs,
                    (List<zwaTransferDocDetailsBin> serialtoBins) =>
                    {
                        MessagingCenter.Unsubscribe<List<zwaTransferDocDetailsBin>>(this, _SerialToBinAddrs);
                        List<TransferItemDetailBinM> pickedList = new List<TransferItemDetailBinM>();
                        var showList = string.Empty;

                        foreach (var serial in serialtoBins)
                        {
                            pickedList.Add(new TransferItemDetailBinM
                            {
                                LineGuid = Guid.NewGuid(),
                                ItemCode = line.ItemCode,
                                ItemName = line.ItemName,
                                Qty = 1,
                                Serial = serial.Serial,
                                Batch = string.Empty,
                                InternalSerialNumber = string.Empty,
                                ManufacturerSerialNumber = string.Empty,
                                BinCode = serial.BinCode,
                                BinAbs = serial.BinAbs,
                                SnBMDAbs = serial.SnBMDAbs,
                                WhsCode = currentDocLine.ToWhsCode,
                                Direction = _TO
                            });
                            showList += $"{serial.BinCode} -> {serial.Serial}\n";
                        }

                        currentDocLine.ShowList = showList;
                        currentDocLine.ItemName = line.ItemName; 
                        UpdateItem(currentDocLine, pickedList);
                    });
                
                Navigation.PushAsync(new SerialToBinView(currentDocLine.Guid, 
                    currentDocLine.LineGuid, 
                    currentDocLine.ItemCode, 
                    currentDocLine.ItemName,
                    currentDocLine.ToWhsCode, 
                    _SerialToBinAddrs));
            }
            catch (Exception excep)
            {
                Console.WriteLine($"{excep}");
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Show prompt and capture the item qty
        /// </summary>
        async void CaptureQtyInput(TransferDataM line)
        {
            try
            {
                string input = await new Dialog().DisplayPromptAsync(
                    $"Input item {line.ItemCode} transfer qty",
                    $"needed qty -> {line.RequestedQty:N}",
                    "OK",
                    "Cancel",
                    null,
                    -1,
                    Keyboard.Numeric,
                    $"{line.RequestedQty:N}");

                if (string.IsNullOrWhiteSpace(input)) return;
                if (input.ToLower().Equals("cancel")) return;

                bool isNumeric = decimal.TryParse(input, out decimal result);
                if (!isNumeric)
                {
                    DisplayAlert("Inputted values must be numeric, please try again. Thanks");
                    return;
                }

                if (result <= 0)
                {
                    DisplayAlert($"Inputted values {result:N} must positive and greater than zero, please try again. Thanks");
                    return;
                }

                if (line.RequestedQty < result)
                {
                    DisplayAlert($"Inputted values {result:N} must smaller or equal to {line.RequestedQty:N}, please try again. Thanks");
                    return;
                }

                // save into the list
                var pick = new List<TransferItemDetailBinM>();
                pick.Add(new TransferItemDetailBinM
                {
                    LineGuid = Guid.NewGuid(),
                    ItemCode = line.ItemCode,
                    ItemName =line.ItemName,
                    Qty = result,
                    Serial = string.Empty,
                    Batch = string.Empty,
                    InternalSerialNumber = string.Empty,
                    ManufacturerSerialNumber = string.Empty,
                    BinCode = string.Empty,
                    BinAbs = -1,
                    SnBMDAbs = -1,
                    WhsCode = currentDocLine.ToWhsCode,
                    Direction = _TO
                });

                currentDocLine.ItemName = line.ItemName;
                currentDocLine.ShowList = $"{result:N} -> {currentDocLine.ToWhsCode}";
                UpdateItem(currentDocLine, pick);
                DisplayToast("Saved");
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Update / refresh the list line
        /// </summary>
        /// <param name="docLine"></param>
        void UpdateItem(zwaTransferDocDetails docLine, List<TransferItemDetailBinM> pickedList)
        {
            if (itemsSource == null) return;
            if (itemsSource.Count == 0) return;

            int locId = itemsSource.IndexOf(docLine);
            if (locId < 0) return;
            decimal sum = pickedList.Sum(x => x.Qty);

            itemsSource[locId].CellCompleteColor = Color.Green;
            itemsSource[locId].Qty = (pickedList == null) ? 0 : pickedList.Sum(x => x.Qty);
            itemsSource[locId].Picked = pickedList;            
        }

        //void Close() => Navigation.PopAsync(); // <-- close the screen
                                               // re binding the item source to screen
        void ResetListView()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<zwaTransferDocDetails>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// Display Aleart
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
