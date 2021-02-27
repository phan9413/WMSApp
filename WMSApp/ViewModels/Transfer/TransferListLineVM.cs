using Acr.UserDialogs;
using DbClass;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Threading;
using WMSApp.Class;
using WMSApp.Class.Helper;
using WMSApp.Dtos;
using WMSApp.Interface;
using WMSApp.Models.SAP;
using WMSApp.Models.Share;
using WMSApp.Models.Transfer1;
using WMSApp.ViewModels.Transfer1;
using WMSApp.Views.Share;
using WMSApp.Views.Transfer1;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace WMSApp.ViewModels.Transfer
{
    public class TransferListLineVM : INotifyPropertyChanged
    {
        #region Property
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string pName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pName));

        public Command CmdScanItemCode { get; set; }
        public Command CmdInputItemCode { get; set; }
        public Command CmdSave { get; set; }
        public Command CmdCancel { get; set; }
        public WTQ1_Ex SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;
                currentDocLine = value;

                if (WhsDirection.Equals(_FROM))    // Handler from warehouse selection
                {
                    if (currentDocLine.TransferQty > 0)
                    {
                        ResetCellToZero();
                        selectedItem = null;
                        OnPropertyChanged(nameof(SelectedItem));
                        return;
                    }

                    ProcessItemCode(currentDocLine.ItemCode); // handle the selected item for scan
                    selectedItem = null;
                    OnPropertyChanged(nameof(SelectedItem));
                    return;
                }

                if (WhsDirection.Equals(_TO))  // handle to warehouse direction
                {
                    ProcessItemCode(currentDocLine.ItemCode);
                }

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

        List<WTQ1_Ex> itemsSource;
        public ObservableCollection<WTQ1_Ex> ItemsSource { get; set; }

        /// <summary>
        /// for handle the put / picking
        /// </summary>
        List<zwaTransferDocDetails> itemsSourcePicked;
        public ObservableCollection<zwaTransferDocDetails> ItemsSourcePicked { get; set; }

        List<zwaTransferDocDetailsBin> CurrentDocLineFromBins;
        zwaTransferDocDetails CurrentItemPicked;
        zwaTransferDocDetails selectedItemPicked;
        public zwaTransferDocDetails SelectedItemPicked
        {
            get => selectedItemPicked;
            set
            {
                if (selectedItemPicked == value) return;
                selectedItemPicked = value;
                CurrentItemPicked = value;

                WhsDirection = _TO;
                ProcessItemCodePicked(CurrentItemPicked.ItemCode);

                selectedItemPicked = null;
                OnPropertyChanged(nameof(SelectedItemPicked));
            }
        }

        bool fromListVisible;
        public bool FromListVisible
        {
            get => fromListVisible;
            set
            {
                if (fromListVisible == value) return;
                fromListVisible = value;
                OnPropertyChanged(nameof(FromListVisible));
            }
        }

        bool pickedListVisible;
        public bool PickedListVisible
        {
            get => pickedListVisible;
            set
            {
                if (pickedListVisible == value) return;
                pickedListVisible = value;
                OnPropertyChanged(nameof(PickedListVisible));
            }
        }

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

        List<string> docSeriesItemsSource;
        public ObservableCollection<string> DocSeriesItemsSource { get; set; }

        string docSeriesSelectedItem;
        public string DocSeriesSelectedItem
        {
            get => docSeriesSelectedItem;
            set
            {
                if (docSeriesSelectedItem == value) return;
                docSeriesSelectedItem = value;
                OnPropertyChanged(nameof(DocSeriesSelectedItem));
            }
        }

        NNM1[] DocSeries; // kept orginal from server

        string numberAtCard;
        public string NumberAtCard
        {
            get => numberAtCard;
            set
            {
                if (numberAtCard == value) return;
                numberAtCard = value;
                OnPropertyChanged(nameof(NumberAtCard));
            }
        }

        string ref2;
        public string Ref2
        {
            get => ref2;
            set
            {
                if (ref2 == value) return;
                ref2 = value;
                OnPropertyChanged(nameof(Ref2));
            }
        }

        string comments;
        public string Comments
        {
            get => comments;
            set
            {
                if (comments == value) return;
                comments = value;
                OnPropertyChanged(nameof(Comments));
            }
        }

        string jrnlMemo;
        public string JrnlMemo
        {
            get => jrnlMemo;
            set
            {
                if (jrnlMemo == value) return;
                jrnlMemo = value;
                OnPropertyChanged(nameof(JrnlMemo));
            }
        }

        public bool IsTransferDocVisible { get; set; }

        #endregion

        #region Private declation
        /// <summary>
        /// Private declaration
        /// </summary>
        /// 
        readonly string _BinSerialListAddrs = "20201019T2058_BinSerialList";
        readonly string _SerialListAddrs = "20201019T2058_SerialList";
        readonly string _BinBatchListAddrs = "20201019T2058_BinBatchList";
        readonly string _BatchListAddrs = "20201019T1758_BatchList";
        readonly string _BinItemAddrs = "20201019T1758_BinItemList";
        readonly string _FROM = "FROM";
        readonly string _TO = "TO";

        /// <summary>
        ///  cater select to warehouse 
        /// </summary>
        readonly string _SerialToBinAddrs = "20201021_SerialToBin";
        readonly string _SerialConfirmAddrs = "20201021_SerialConfirm";
        readonly string _BatchToBinAddrs = "20201021_BatchToBinAddrs";
        readonly string _BatchConfirmAddrs = "20201021_BatchConfirm";
        readonly string _ItemToBinAddrs = "20201021_ItemToBin_T1621";

        readonly string _PopScreenAddr = "_PopScreenAddr_201207_TransferListLine";

        INavigation Navigation { get; set; } = null;
        OWTQ_Ex CurrentDoc { get; set; } = null;
        WTQ1_Ex currentDocLine { get; set; } = null;
        WTQ1_Ex selectedItem { get; set; } = null;
        zwaRequest currentRequest { get; set; } = null;
        zwaHoldRequest CurrentDocPicked { get; set; } = null;
        string WhsDirection { get; set; } = string.Empty;

        public bool isBusy { get; private set; }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="navigation"></param>
        /// <param name="selectedDoc"></param>
        public TransferListLineVM(INavigation navigation, OWTQ_Ex selectedDoc, string direction)
        {
            Navigation = navigation;
            CurrentDoc = selectedDoc;
            ViewTitle = $"Request #{selectedDoc.RequestDocument.DocNum}";
            WhsDirection = direction;
            Direction = (direction.Equals("FROM")) ? $"Pick item {direction} Operations" : $"Put item {direction} Operations";

            InitCmd();
            LoadDocLine();

            isBusy = true;
            IsTransferDocVisible = false;
        }

        /// <summary>
        /// for handler the request line
        /// </summary>
        /// <param name="navigation"></param>
        /// <param name="selectedDoc"></param>
        /// <param name="direction"></param>
        public TransferListLineVM(INavigation navigation, zwaHoldRequest selectedDoc, string direction)
        {
            Navigation = navigation;
            CurrentDocPicked = selectedDoc;
            ViewTitle = (selectedDoc.DocEntry == -1) ? $"Picked #{selectedDoc.Id}" : $"Picked #{selectedDoc.DocEntry}";
            WhsDirection = direction;
            Direction = (direction.Equals("FROM")) ? $"Pick item {direction} Operations" : $"Put item {direction} Operations";

            InitCmd();
            LoadDocLinePicked();

            isBusy = true;
            IsTransferDocVisible = true;
            
            LoadDocSeries();
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
                    request = "GetTransferRequestLine",
                    TransferRequestDocEntry = CurrentDoc.RequestDocument.DocEntry
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
                if (retBag.TransferRequestLine == null) return;
                itemsSource = new List<WTQ1_Ex>();

                foreach (var lines in retBag.TransferRequestLine)
                {
                    itemsSource.Add(new WTQ1_Ex { Line = lines, CellCompleteColor = Color.Red });
                }
                ResetListView();
                isBusy = false;
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
        /// Load rqeust line from web server
        /// </summary>
        async void LoadDocLinePicked()
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
                    request = "GetPickedRequestLines",
                    TransferDocRequestGuid = CurrentDocPicked.HeadGuid
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
                itemsSourcePicked = new List<zwaTransferDocDetails>();
                itemsSourcePicked.AddRange(retBag.dtozmwTransferDocDetails);

                // kept the from bin line for reference later
                CurrentDocLineFromBins = new List<zwaTransferDocDetailsBin>();
                CurrentDocLineFromBins.AddRange(retBag.dtozwaTransferDocDetailsBin);

                ResetListViewPicked();
                isBusy = false;
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
            CmdCancel = new Command(Close);
        }

        /// <summary>
        /// Perform save into the svr for create transfer document
        /// </summary>
        async void Save()
        {
            try
            {
                if (WhsDirection.Equals(_TO)) // handle the to warehouse
                {
                    //Device.BeginInvokeOnMainThread(async () =>
                    //{
                    //    UserDialogs.Instance.ShowLoading("Test");
                    //});
                       
                    HandlerCreateTransferDoc();
                    return;
                }

                if (itemsSource == null)
                {
                    DisplayAlert("There is request lines not yet fulfilled, please try again later. [0]");
                    return;
                }

                var itemList = itemsSource.Where(x => x.TransferQty > 0).ToList();
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

                UserDialogs.Instance.ShowLoading("A moment ...");
                // 20200719T1030
                // prepare the request dto
                var groupingGuid = Guid.NewGuid();
                var transDate = DateTime.Now;

                // save and create a onhold entry                
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

                    DocEntry = CurrentDoc.RequestDocument.DocEntry,
                    DocNum = CurrentDoc.RequestDocument.DocNum,
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
                    FromWhsCode = CurrentDoc.RequestDocument.Filler,
                    ToWhsCode = CurrentDoc.RequestDocument.ToWhsCode,
                    JrnlMemo = CurrentDoc.RequestDocument.JrnlMemo,
                    Comments = CurrentDoc.RequestDocument.Comments,
                    Guid = groupingGuid,
                    DocNumber = string.Empty,
                    DocStatus = "O",
                    LastErrorMessage = string.Empty
                };

                // save the line 
                var TransferDocDetails = new List<zwaTransferDocDetails>();
                var TransferDocDetailsBins = new List<zwaTransferDocDetailsBin>();
                // save line from table
                foreach (var line in itemList)
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
                    var actualQty = line.TransferItemList.Sum(x => x.Qty);

                    TransferDocDetails.Add(new zwaTransferDocDetails
                    {
                        Guid = groupingGuid,
                        LineGuid = lineGuid,
                        ItemCode = line.ItemCode,                        
                        ItemName = line.Dscription,
                        Qty = actualQty,
                        FromWhsCode = line.FromWhsCod,
                        ToWhsCode = line.WhsCode,
                        Serial = string.Empty,
                        Batch = string.Empty,
                        SourceDocBaseType = line.me.ObjType,
                        SourceBaseEntry = line.me.DocEntry,
                        SourceBaseLine = line.me.LineNum
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
                    foreach (var bin in line.TransferItemList)
                    {
                        TransferDocDetailsBins.Add(new zwaTransferDocDetailsBin
                        {
                            Guid = groupingGuid,
                            LineGuid = lineGuid,
                            ItemCode = line.ItemCode,
                            ItemName = line.Dscription,
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
                    request = "SaveRequestOnHold",
                    TransferHoldRequest = OnHoldRequest,
                    TransferDocHeader = TransferDocHeader,
                    TransferDocDetails = TransferDocDetails?.ToArray(),
                    TransferDocDetailsBins = TransferDocDetailsBins?.ToArray()
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

                // build the waiting mechanism to check created doc and stop the looing
                DisplayToast("Inventory transfer sent !");
                Close();
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

                if (itemsSourcePicked == null)
                {
                    DisplayAlert("There is no transfer line setup, please try again later, Thanks. [N]");
                    return;
                }

                if (itemsSourcePicked.Count == 0)
                {
                    DisplayAlert("There is no transfer line setup, please try again later, Thanks.[Z]");
                    return;
                }

                var processList = itemsSourcePicked.Where(x => x.Picked != null).ToList();
                if (processList == null)
                {
                    DisplayAlert("There is no item being allocation the PUT location, " +
                        "Please PUT the item and try save again, Thanks.[N]");
                }

                if (processList.Count == 0)
                {
                    DisplayAlert("There is no item being allocation the PUT location, " +
                         "Please PUT the item and try save again, Thanks.[Z]");
                }

                // create request object 
                // create the to bin line
                // save the line                
                // save line from table

                Device.BeginInvokeOnMainThread(async () =>
                {
                    UserDialogs.Instance.ShowLoading("Awaiting Create Inventory Transfer...");
                });

                var TransferDocDetailsBins = new List<zwaTransferDocDetailsBin>();
                var TransferDocLine = new List<zwaTransferDocDetails>();
                foreach (var line in processList)
                {
                    var lines = line.GetList();
                    if (lines == null) continue;
                    if (lines.Count == 0) continue;

                    var newToLine = new zwaTransferDocDetails
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
                        SourceBaseLine = line.SourceBaseLine
                    };

                    TransferDocLine.Add(newToLine);
                    TransferDocDetailsBins.AddRange(lines);
                }

                // create the header property                
                var headerDetails = new zmwDocHeaderField
                {
                    Guid = processList[0].Guid,
                    DocSeries = docSeriesSelectedItem,
                    Ref2 = ref2,
                    Comments = comments,
                    JrnlMemo = jrnlMemo,
                    NumAtCard = numberAtCard,
                    Series = GetCurrentSeries(docSeriesSelectedItem)
                };

                currentRequest = new zwaRequest
                {
                    sapUser = App.waUser,
                    request = "Create Transfer1",
                    guid = CurrentDocPicked.HeadGuid,
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
                    TransferDocDetails = TransferDocLine.ToArray(),
                    dtozmwDocHeaderField = headerDetails,
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

                // set to server to reset the onhold request
                RemoveOnHold($"{currentRequest.guid}");
                Navigation.PopAsync();
            });
        }

        async void RemoveOnHold(string guid)
        {
            try
            {
                dynamic request_ = new ExpandoObject();
                request_.request = "RemoveOnHold";
                request_.checkDocGuid = guid;

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(request_, "Transfer1");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    DisplayAlert($"{content}\n{lastErrMessage}");
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                DisplayAlert($"{e.Message}\n{e.StackTrace}");
            }
        }

        //void CheckDocStatus()
        //{
        //    var callerAddress = "CreateInvTransfer_DocCheck";
        //    MessagingCenter.Subscribe(this, callerAddress, (string result) =>
        //    {
        //        MessagingCenter.Unsubscribe<string>(this, callerAddress);
        //        UserDialogs.Instance.HideLoading();

        //        if (result.Equals("fail")) return;
        //        if (result.Equals("success")) Navigation.PopAsync();

        //        App.RequestList.Remove(currentRequest);
        //    });

        //    currentRequest.request = "Transfer";
        //    var checker = new RequestCheckHelper { Requests = currentRequest, ReturnAddress = callerAddress };
        //    checker.StartChecking();
        //}

        /// <summary>
        /// serach the selected series name 
        /// </summary>
        /// <returns></returns>
        int GetCurrentSeries(string selectedSeriesName)
        {
            const int primarySeries = 18;
            if (string.IsNullOrWhiteSpace(selectedSeriesName)) return primarySeries;
            if (DocSeries == null) return primarySeries;
            if (DocSeries.Length == 0) return primarySeries;

            var series =
                DocSeries.Where(x => x.SeriesName.ToLower().Equals(selectedSeriesName.ToLower())).FirstOrDefault();

            if (series == null) return primarySeries;
            else return series.Series;
        }

        /// <summary>
        /// for from list display
        /// </summary>
        void ResetListView()
        {
            if (itemsSource == null) return;
            FromListVisible = true;
            PickedListVisible = false;
            ItemsSource = new ObservableCollection<WTQ1_Ex>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// for picked list display
        /// </summary>
        void ResetListViewPicked()
        {
            if (itemsSourcePicked == null) return;
            FromListVisible = false;
            PickedListVisible = true;
            ItemsSourcePicked = new ObservableCollection<zwaTransferDocDetails>(itemsSourcePicked);
            OnPropertyChanged(nameof(ItemsSourcePicked));
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

        /// <summary>
        /// Handler start the scanner event and return the item code
        /// </summary>
        async void HandlerScanItem()
        {
            try
            {
                string returnAddress = "TransferListLineHandlerScanItem";
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
        void ResetCellToZero()
        {
            int locId = itemsSource.IndexOf(currentDocLine);
            if (locId < 0) return;
            itemsSource[locId].CellCompleteColor = Color.Red;
            itemsSource[locId].TransferQty = 0;
            itemsSource[locId].ItemWhsBinQty = null;
            itemsSource[locId].TransferItemList = null; // 20201012T
            itemsSource[locId].ShowList = string.Empty;
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
                currentDocLine.SelectedOITM = retBag.Item;

                #region handle to warehouse
                if (WhsDirection.Equals(_TO))
                {
                    var toWhs = App.Warehouses.Where(x => x.WhsCode.Equals(CurrentDocPicked)).FirstOrDefault();
                    if (toWhs == null) return;

                    var itemInfo1 = new TransferDataM
                    {
                        ItemCode = currentDocLine.SelectedOITM.ItemCode,
                        ItemName = currentDocLine.SelectedOITM.ItemName,
                        ItemManagedBy = (currentDocLine.SelectedOITM.ManBtchNum.Equals("Y")) ? "Batch" :
                                        (currentDocLine.SelectedOITM.ManSerNum.Equals("Y")) ? "Serial" : "None",
                        FromWhsCode = toWhs.WhsCode,
                        BinActivated = toWhs.BinActivat.Equals("Y") ? "Yes" : "No",
                        RequestedQty = currentDocLine.RequestQuantity
                    };

                    LaunchAllocationScreenToWhs(itemInfo1);
                    return;
                }
                #endregion

                #region handle from warehouse
                var fromWhs = App.Warehouses.Where(x => x.WhsCode.Equals(currentDocLine.RequestFromWarehouse)).FirstOrDefault();
                if (fromWhs == null) return;

                var itemInfo = new TransferDataM
                {
                    ItemCode = currentDocLine.SelectedOITM.ItemCode,
                    ItemName = currentDocLine.SelectedOITM.ItemName,
                    ItemManagedBy = (currentDocLine.SelectedOITM.ManBtchNum.Equals("Y")) ? "Batch" :
                                    (currentDocLine.SelectedOITM.ManSerNum.Equals("Y")) ? "Serial" : "None",
                    FromWhsCode = fromWhs.WhsCode,
                    BinActivated = fromWhs.BinActivat.Equals("Y") ? "Yes" : "No",
                    RequestedQty = currentDocLine.RequestQuantity
                };

                LaunchAllocationScreen(itemInfo);
                #endregion
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
        /// Process item code for picking 
        /// </summary>
        /// <param name="itemCode"></param>
        async void ProcessItemCodePicked(string itemCode)
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
                CurrentItemPicked.SelectedOITM = retBag.Item;

                #region handle to warehouse
                if (WhsDirection.Equals(_TO))
                {
                    var toWhs = App.Warehouses.Where(x => x.WhsCode.Equals(CurrentItemPicked.ToWhsCode)).FirstOrDefault();
                    if (toWhs == null) return;
                    var pickedIemInfo = new TransferDataM
                    {
                        ItemCode = CurrentItemPicked.SelectedOITM.ItemCode,
                        ItemName = CurrentItemPicked.SelectedOITM.ItemName,
                        ItemManagedBy = (CurrentItemPicked.SelectedOITM.ManBtchNum.Equals("Y")) ? "Batch" :
                                        (CurrentItemPicked.SelectedOITM.ManSerNum.Equals("Y")) ? "Serial" : "None",
                        FromWhsCode = toWhs.WhsCode,
                        BinActivated = toWhs.BinActivat.Equals("Y") ? "Yes" : "No",
                        RequestedQty = CurrentItemPicked.Qty
                    };


                    LaunchAllocationScreenToWhs(pickedIemInfo);
                    return;
                }
                #endregion
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
                            CaptureQtyInputPicked(line);
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

                        if (CurrentDocLineFromBins == null) return;
                        if (CurrentDocLineFromBins.Count == 0) return;

                        var pickedList = new List<TransferItemDetailBinM>();
                        var showlist = string.Empty;

                        foreach (var bin in itemToBins)
                        {
                            // search for from line 
                            var foundFromLine = CurrentDocLineFromBins.Where(
                                x =>
                                x.LineGuid.Equals(CurrentItemPicked.LineGuid) &&
                                x.WhsCode.Equals(CurrentItemPicked.FromWhsCode) &&
                                x.ItemCode.Equals(line.ItemCode)).FirstOrDefault();

                            if (foundFromLine == null) continue;
                            pickedList.Add(new TransferItemDetailBinM
                            {
                                Guid = CurrentItemPicked.Guid,
                                LineGuid = CurrentItemPicked.LineGuid, // to be replace @ server site to match the from warehouse
                                ItemCode = CurrentItemPicked.ItemCode, 
                                ItemName = CurrentItemPicked.ItemName,
                                Qty = bin.BinQty,
                                Serial = foundFromLine.Serial,
                                Batch = foundFromLine.Batch,
                                InternalSerialNumber = foundFromLine.InternalSerialNumber,
                                ManufacturerSerialNumber = foundFromLine.ManufacturerSerialNumber,
                                BinAbs = foundFromLine.BinAbs,
                                BinCode = foundFromLine.BinCode,
                                SnBMDAbs = foundFromLine.SnBMDAbs,
                                WhsCode = foundFromLine.WhsCode,
                                Direction = _FROM
                            });


                            pickedList.Add(new TransferItemDetailBinM
                            {
                                Guid = CurrentItemPicked.Guid,
                                LineGuid = CurrentItemPicked.LineGuid, // need to replace by the head and line guid
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
                                WhsCode = CurrentItemPicked.ToWhsCode,
                                Direction = _TO
                            });
                            showlist += $"{bin.oBIN.BinCode} <- {bin.BinQty:N}\n";
                        }

                        CurrentItemPicked.ShowList = showlist;
                        UpdateItemPicked(CurrentItemPicked, pickedList);
                    });


                Navigation.PushAsync(
                    new ItemToBinView(CurrentItemPicked.Guid, 
                    CurrentItemPicked.LineGuid, 
                    CurrentItemPicked.ItemCode, 
                    CurrentItemPicked.ItemName, 
                    CurrentItemPicked.ToWhsCode ,
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
        async void LaunchBatchToBinOpr(TransferDataM line)
        {
            try
            {
                MessagingCenter.Subscribe<List<zwaTransferDocDetailsBin>>(this, _BatchToBinAddrs,
                    (List<zwaTransferDocDetailsBin> batchtoBins) =>
                    {
                        MessagingCenter.Unsubscribe<List<zwaTransferDocDetailsBin>>(this, _BatchToBinAddrs);
                        if (CurrentDocLineFromBins == null) return;
                        if (CurrentDocLineFromBins.Count == 0) return;

                        var pickedList = new List<TransferItemDetailBinM>();
                        var showlist = string.Empty;

                        foreach (var batch in batchtoBins)
                        {
                            if (batch.Bins == null) continue;
                            foreach (var batchBin in batch.Bins)
                            {
                                // search for from line 
                                var foundFromLine = CurrentDocLineFromBins.Where(
                                    x =>
                                    x.LineGuid.Equals(CurrentItemPicked.LineGuid) &&
                                    x.BinAbs.Equals(batch.BinAbs) &&
                                    x.Batch.Equals(batch.Batch)).FirstOrDefault();

                                if (foundFromLine == null) continue;
                                pickedList.Add(new TransferItemDetailBinM
                                {
                                    Guid = CurrentItemPicked.Guid,
                                    LineGuid = CurrentItemPicked.LineGuid, // to be replace @ server site to match the from warehouse
                                    ItemCode = foundFromLine.ItemCode, 
                                    ItemName = foundFromLine.ItemName,
                                    Qty = batchBin.BatchTransQty,
                                    Serial = foundFromLine.Serial,
                                    Batch = foundFromLine.Batch,
                                    InternalSerialNumber = foundFromLine.InternalSerialNumber,
                                    ManufacturerSerialNumber = foundFromLine.ManufacturerSerialNumber,
                                    BinCode = foundFromLine.BinCode,
                                    BinAbs = foundFromLine.BinAbs,
                                    SnBMDAbs = foundFromLine.SnBMDAbs,
                                    WhsCode = foundFromLine.WhsCode,
                                    Direction = _FROM
                                });

                                pickedList.Add(new TransferItemDetailBinM
                                {
                                    Guid = CurrentItemPicked.Guid,
                                    LineGuid = CurrentItemPicked.LineGuid,
                                    ItemCode = foundFromLine.ItemCode,
                                    ItemName = foundFromLine.ItemName,
                                    Qty = batchBin.BatchTransQty,
                                    Serial = string.Empty,
                                    Batch = batch.Batch,
                                    InternalSerialNumber = string.Empty,
                                    ManufacturerSerialNumber = string.Empty,
                                    BinAbs = batchBin.oBIN.AbsEntry,
                                    BinCode = batchBin.oBIN.BinCode,
                                    SnBMDAbs = batch.SnBMDAbs,
                                    WhsCode = CurrentItemPicked.ToWhsCode,
                                    Direction = _TO
                                });
                                showlist += $"{batchBin.oBIN.BinCode} <- {batch.Batch} <- {batchBin.BatchTransQty:N}\n";
                            }
                            CurrentItemPicked.ShowList = showlist;
                        }
                        UpdateItemPicked(CurrentItemPicked, pickedList);
                    });

                await Navigation.PushAsync(
                    new BatchToBinView(
                        CurrentItemPicked.Guid, CurrentItemPicked.LineGuid, CurrentItemPicked.ToWhsCode,
                        _BatchToBinAddrs, CurrentItemPicked.ItemCode, CurrentItemPicked.ItemName));
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
        async void LaunchBatchConfirmView(TransferDataM line)
        {
            // launch view for user to receipt 
            try
            {
                MessagingCenter.Subscribe<List<zwaTransferDocDetailsBin>>(this, _BatchConfirmAddrs,
                    (List<zwaTransferDocDetailsBin> confirmBatches) =>
                    {
                        MessagingCenter.Unsubscribe<List<zwaTransferDocDetailsBin>>(this, _BatchConfirmAddrs);

                        if (CurrentDocLineFromBins == null) return;
                        if (CurrentDocLineFromBins.Count == 0) return;

                        var pickedList = new List<TransferItemDetailBinM>();
                        var showList = string.Empty;

                        foreach (var batch in confirmBatches)
                        {
                            // search for from line 
                            var foundFromLine = CurrentDocLineFromBins.Where(
                                x =>
                                x.LineGuid.Equals(CurrentItemPicked.LineGuid) &&
                                x.Batch.Equals(batch.Batch)).FirstOrDefault();

                            if (foundFromLine == null) continue;

                            pickedList.Add(new TransferItemDetailBinM
                            {
                                Guid = CurrentItemPicked.Guid,
                                LineGuid = CurrentItemPicked.LineGuid, // to be replace @ server site to match the from warehouse
                                ItemCode = foundFromLine.ItemCode, 
                                ItemName = foundFromLine.ItemName,
                                Qty = batch.Qty,
                                Serial = foundFromLine.Serial,
                                Batch = foundFromLine.Batch,
                                InternalSerialNumber = foundFromLine.InternalSerialNumber,
                                ManufacturerSerialNumber = foundFromLine.ManufacturerSerialNumber,
                                BinAbs = foundFromLine.BinAbs,
                                BinCode = foundFromLine.BinCode,
                                SnBMDAbs = foundFromLine.SnBMDAbs,
                                WhsCode = foundFromLine.WhsCode,
                                Direction = _FROM
                            });

                            pickedList.Add(new TransferItemDetailBinM
                            {
                                Guid = CurrentItemPicked.Guid,
                                LineGuid = CurrentItemPicked.LineGuid, // to be replace @ server site to match the from warehouse
                                ItemCode = CurrentItemPicked.ItemCode, 
                                ItemName = CurrentItemPicked.ItemName,
                                Qty = batch.Qty,
                                Serial = string.Empty,
                                Batch = batch.Batch,
                                InternalSerialNumber = string.Empty,
                                ManufacturerSerialNumber = string.Empty,
                                BinCode = string.Empty,
                                BinAbs = -1,
                                SnBMDAbs = -1,
                                WhsCode = CurrentItemPicked.ToWhsCode,
                                Direction = _TO
                            });
                            showList += $"{batch.Batch} <- {batch.Qty:N}\n";
                        }

                        CurrentItemPicked.ShowList = showList;
                        UpdateItemPicked(CurrentItemPicked, pickedList);
                    });

                await Navigation.PushAsync(
                    new BatchesConfirmView(
                        CurrentItemPicked.Guid,
                        CurrentItemPicked.LineGuid,
                        CurrentItemPicked.ToWhsCode,
                        _BatchConfirmAddrs));
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
        async void LaunchSerialConfirmView(TransferDataM line)
        {
            try
            {
                MessagingCenter.Subscribe<List<zwaTransferDocDetailsBin>>(this, _SerialConfirmAddrs,
                    (List<zwaTransferDocDetailsBin> serialtoBins) =>
                    {
                        MessagingCenter.Unsubscribe<List<zwaTransferDocDetailsBin>>(this, _SerialConfirmAddrs);
                        if (CurrentDocLineFromBins == null) return;
                        if (CurrentDocLineFromBins.Count == 0) return;

                        var pickedList = new List<TransferItemDetailBinM>();
                        var showList = string.Empty;

                        foreach (var serial in serialtoBins)
                        {
                            // search for from line 
                            var foundFromLine = CurrentDocLineFromBins.Where(
                                x =>
                                x.LineGuid.Equals(CurrentItemPicked.LineGuid) &&
                                x.Serial.Equals(serial.Serial)).FirstOrDefault();

                            if (foundFromLine == null) continue;
                            pickedList.Add(new TransferItemDetailBinM
                            {
                                Guid = CurrentItemPicked.Guid,
                                LineGuid = CurrentItemPicked.LineGuid, // to be replace @ server site to match the from warehouse
                                ItemCode = foundFromLine.ItemCode, 
                                ItemName = foundFromLine.ItemName,
                                Qty = 1,
                                Serial = foundFromLine.Serial,
                                Batch = foundFromLine.Batch,
                                InternalSerialNumber = foundFromLine.InternalSerialNumber,
                                ManufacturerSerialNumber = foundFromLine.ManufacturerSerialNumber,
                                BinCode = foundFromLine.BinCode,
                                BinAbs = foundFromLine.BinAbs,
                                SnBMDAbs = foundFromLine.SnBMDAbs,
                                WhsCode = foundFromLine.WhsCode,
                                Direction = _FROM
                            });

                            pickedList.Add(new TransferItemDetailBinM
                            {
                                Guid = CurrentItemPicked.Guid,
                                LineGuid = CurrentItemPicked.LineGuid, // to be replace @ server site to match the from warehouse
                                ItemCode = serial.ItemCode,
                                ItemName = serial.ItemName,
                                Qty = 1,
                                Serial = serial.Serial,
                                Batch = string.Empty,
                                InternalSerialNumber = string.Empty,
                                ManufacturerSerialNumber = string.Empty,
                                BinAbs = -1,
                                BinCode = serial.BinCode,
                                SnBMDAbs = -1,
                                WhsCode = CurrentItemPicked.ToWhsCode,
                                Direction = _TO
                            });
                            showList += $"{serial.Serial}\n";
                        }
                        CurrentItemPicked.ShowList = showList;
                        UpdateItemPicked(CurrentItemPicked, pickedList);
                    });

                var docLine = CurrentItemPicked;
                if (docLine == null)
                {
                    DisplayAlert("Error reading info from server, please try again later, Thanks.");
                    return;
                }

                await Navigation.PushAsync(new SerialsConfirmView(
                    docLine.Guid,
                    docLine.LineGuid,
                    docLine.ItemCode,
                    docLine.ItemName,
                    docLine.ToWhsCode,
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
        async void LaunchSerialToBinOpr(TransferDataM line)
        {
            try
            {
                MessagingCenter.Subscribe<List<zwaTransferDocDetailsBin>>(this, _SerialToBinAddrs,
                    (List<zwaTransferDocDetailsBin> serialtoBins) =>
                {
                    MessagingCenter.Unsubscribe<List<zwaTransferDocDetailsBin>>(this, _SerialToBinAddrs);

                    if (CurrentDocLineFromBins == null) return;
                    if (CurrentDocLineFromBins.Count == 0) return;

                    var showList = string.Empty;
                    var pickedList = new List<TransferItemDetailBinM>();
                    foreach (var serial in serialtoBins)
                    {
                        // search for from line 
                        var foundFromLine = CurrentDocLineFromBins
                        .Where(x =>
                                x.LineGuid.Equals(CurrentItemPicked.LineGuid) &&
                                x.Serial.Equals(serial.Serial)).FirstOrDefault();

                        if (foundFromLine == null) continue;
                        pickedList.Add(new TransferItemDetailBinM
                        {
                            Guid = CurrentItemPicked.Guid,
                            LineGuid = CurrentItemPicked.LineGuid,
                            ItemCode = foundFromLine.ItemCode,
                            ItemName = foundFromLine.ItemName,
                            Qty = 1,
                            Serial = serial.Serial,
                            Batch = foundFromLine.Batch,
                            InternalSerialNumber = foundFromLine.InternalSerialNumber,
                            ManufacturerSerialNumber = foundFromLine.ManufacturerSerialNumber,
                            BinAbs = foundFromLine.BinAbs,
                            SnBMDAbs = foundFromLine.SnBMDAbs,
                            BinCode = foundFromLine.BinCode,
                            WhsCode = foundFromLine.WhsCode,
                            Direction = _FROM
                        });

                        // prepare to warehouse
                        pickedList.Add(new TransferItemDetailBinM
                        {
                            Guid = CurrentItemPicked.Guid,
                            LineGuid = CurrentItemPicked.LineGuid,
                            ItemCode = foundFromLine.ItemCode,
                            ItemName = foundFromLine.ItemName,
                            Qty = 1,
                            Serial = serial.Serial,
                            Batch = string.Empty,
                            InternalSerialNumber = string.Empty,
                            ManufacturerSerialNumber = string.Empty,
                            BinAbs = serial.BinAbs,
                            BinCode = serial.BinCode,
                            SnBMDAbs = serial.SnBMDAbs,
                            WhsCode = CurrentItemPicked.ToWhsCode,
                            Direction = _TO
                        });
                        showList += $"{serial.BinCode} <- {serial.Serial}\n";
                    }
                    CurrentItemPicked.ShowList = showList;
                    UpdateItemPicked(CurrentItemPicked, pickedList);
                });

                var docLine = CurrentItemPicked;
                if (docLine == null)
                {
                    DisplayAlert("There is error reading info from server, please try again later. Thanks");
                    return;
                }

                await Navigation.PushAsync(new SerialToBinView(docLine.Guid,
                    docLine.LineGuid,
                    docLine.ItemCode,
                    docLine.ItemName,
                    docLine.ToWhsCode,
                    _SerialToBinAddrs));
            }
            catch (Exception excep)
            {
                Console.WriteLine($"{excep}");
                DisplayAlert(excep.Message);
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
                        // else
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
                        // BatchListView
                        LaunchBatchList(line);
                        break;
                    }
                case "None":
                    {
                        if (line.BinActivated.Equals("Yes"))
                        {
                            LaunchBinItemList(line); // show list of the warehouse bin qty for this item
                            return;
                        }
                        // else 
                        CaptureQtyInput(line);
                        break;
                    }
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
                if (!isNumeric) // check is numeric
                {
                    DisplayAlert("Inputted values must be numeric, please try again. Thanks");
                    return;
                }

                if (result <= 0) // check negative and avoid zero
                {
                    DisplayAlert($"Inputted values {result:N} must positive and greater than zero, please try again. Thanks");
                    return;
                }

                // temporary commented based on user request
                // check do not over requested qty
                //if (line.RequestedQty < result)
                //{
                //    DisplayAlert($"Inputted values {result:N} must smaller or equal to {line.RequestedQty:N}, please try again. Thanks");
                //    return;
                //}

                // save into the list
                var pick = new List<TransferItemDetailBinM>();
                pick.Add(new TransferItemDetailBinM
                {
                    LineGuid = Guid.NewGuid(),
                    ItemCode = line.ItemCode,
                    Qty = result,
                    Serial = string.Empty,
                    Batch = string.Empty,
                    InternalSerialNumber = string.Empty,
                    ManufacturerSerialNumber = string.Empty,
                    BinAbs = -1,
                    SnBMDAbs = -1,
                    WhsCode = (WhsDirection.Equals(_FROM)) ?
                        currentDocLine.RequestFromWarehouse :
                        currentDocLine.RequestToWarehouse,
                    Direction = WhsDirection
                });

                currentDocLine.ShowList = string.Empty;

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
        /// Show prompt and capture the item qty
        /// </summary>
        async void CaptureQtyInputPicked(TransferDataM line)
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
                if (!isNumeric) // check is numeric
                {
                    DisplayAlert("Inputted values must be numeric, please try again. Thanks");
                    return;
                }

                if (result <= 0) // check negative and avoid zero
                {
                    DisplayAlert($"Inputted values {result:N} must positive and greater than zero, please try again. Thanks");
                    return;
                }

                // temporary commented based on user request
                // check do not over requested qty
                //if (line.RequestedQty < result)
                //{
                //    DisplayAlert($"Inputted values {result:N} must smaller or equal to {line.RequestedQty:N}, please try again. Thanks");
                //    return;
                //}

                if (CurrentDocLineFromBins == null) return;
                if (CurrentDocLineFromBins.Count == 0) return;

                var pick = new List<TransferItemDetailBinM>();
                // search for from line 
                var foundFromLine = CurrentDocLineFromBins.Where(
                    x =>
                    x.LineGuid.Equals(CurrentItemPicked.LineGuid) &&
                    x.ItemCode.Equals(CurrentItemPicked.ItemCode) &&
                    x.WhsCode.Equals(CurrentItemPicked.FromWhsCode)).FirstOrDefault();

                if (foundFromLine == null) return;

                pick.Add(new TransferItemDetailBinM
                {
                    Guid = CurrentItemPicked.Guid,
                    LineGuid = CurrentItemPicked.LineGuid, // to be replace @ server site to match the from warehouse
                    ItemCode = CurrentItemPicked.ItemCode,
                    ItemName = CurrentItemPicked.ItemName,
                    Qty = result,
                    Serial = foundFromLine.Serial,
                    Batch = foundFromLine.Batch,
                    InternalSerialNumber = foundFromLine.InternalSerialNumber,
                    ManufacturerSerialNumber = foundFromLine.ManufacturerSerialNumber,
                    BinCode = foundFromLine.BinCode,
                    BinAbs = foundFromLine.BinAbs,
                    SnBMDAbs = foundFromLine.SnBMDAbs,
                    WhsCode = foundFromLine.WhsCode,
                    Direction = _FROM
                });

                // save into the list

                pick.Add(new TransferItemDetailBinM
                {
                    Guid = CurrentItemPicked.Guid,
                    LineGuid = CurrentItemPicked.LineGuid,
                    ItemCode = line.ItemCode,
                    ItemName = line.ItemName,
                    Qty = result,
                    Serial = string.Empty,
                    Batch = string.Empty,
                    InternalSerialNumber = string.Empty,
                    ManufacturerSerialNumber = string.Empty,
                    BinCode = string.Empty,
                    BinAbs = -1,
                    SnBMDAbs = -1,
                    WhsCode = CurrentItemPicked.ToWhsCode,
                    Direction = _TO
                });

                CurrentItemPicked.ShowList = string.Empty;

                UpdateItemPicked(CurrentItemPicked, pick);
                DisplayToast("Saved");
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
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
                    var showlist = string.Empty;

                    foreach (var item in binItemList)
                    {
                        pickedList.Add(new TransferItemDetailBinM
                        {
                            LineGuid = Guid.NewGuid(),
                            ItemCode = line.ItemCode,
                            Qty = item.TransferQty,
                            Serial = string.Empty,
                            Batch = string.Empty,
                            InternalSerialNumber = string.Empty,
                            ManufacturerSerialNumber = string.Empty,
                            BinAbs = item.BinAbs,
                            BinCode = item.BinCode,
                            SnBMDAbs = -1,
                            WhsCode = (WhsDirection.Equals(_FROM)) ?
                                    currentDocLine.RequestFromWarehouse :
                                    currentDocLine.RequestToWarehouse,

                            Direction = WhsDirection
                        });
                        showlist += $"{item.BinCode} -> {item.TransferQty:N}\n";
                    }

                    currentDocLine.ShowList = showlist;
                    UpdateItem(currentDocLine, pickedList);
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
                DisplayAlert(excep.Message);
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
                    foreach (var batch in batchList)
                    {
                        pickedList.Add(new TransferItemDetailBinM
                        {
                            LineGuid = Guid.NewGuid(),
                            ItemCode = line.ItemCode,
                            Qty = batch.TransferBatchQty,
                            Serial = string.Empty,
                            Batch = batch.DistNumber,
                            InternalSerialNumber = string.Empty,
                            ManufacturerSerialNumber = string.Empty,
                            BinAbs = -1,
                            SnBMDAbs = -1,
                            WhsCode = (WhsDirection.Equals(_FROM)) ?
                                    currentDocLine.RequestFromWarehouse :
                                    currentDocLine.RequestToWarehouse,
                            Direction = WhsDirection
                        });
                        showList += $"{batch.DistNumber} -> {batch.TransferBatchQty:N}\n";
                    }
                    currentDocLine.ShowList = showList;

                    UpdateItem(currentDocLine, pickedList);
                    DisplayToast("Saved");
                });

                Navigation.PushAsync(
                    new BatchListView(
                        _BatchListAddrs, line.ItemCode, line.ItemName,
                        line.FromWhsCode, (int)line.RequestedQty));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
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
                    foreach (var batch in binBatchList)
                    {
                        pickedList.Add(new TransferItemDetailBinM
                        {
                            LineGuid = Guid.NewGuid(),
                            ItemCode = line.ItemCode,                            
                            Qty = batch.TransferBatchQty,
                            Serial = string.Empty,
                            Batch = batch.DistNumber,
                            InternalSerialNumber = string.Empty,
                            ManufacturerSerialNumber = string.Empty,
                            BinAbs = batch.BinAbs,
                            BinCode = batch.BinCode,
                            SnBMDAbs = batch.SnBMDAbs,
                            WhsCode = (WhsDirection.Equals(_FROM)) ?
                                    currentDocLine.RequestFromWarehouse :
                                    currentDocLine.RequestToWarehouse,
                            Direction = WhsDirection
                        });
                        showList += $"{batch.BinCode} -> {batch.DistNumber} -> {batch.TransferBatchQty:N}\n";
                    }
                    currentDocLine.ShowList = showList;

                    UpdateItem(currentDocLine, pickedList);
                    DisplayToast("Saved");
                });

                Navigation.PushAsync(
                    new BinBatchListView(
                        _BinBatchListAddrs, line.ItemCode, line.ItemName,
                        line.FromWhsCode, (int)line.RequestedQty));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Show bin + serial list in that warehouse
        /// </summary>
        void LaunchBinSerialList(TransferDataM line)
        {
            try
            {
                MessagingCenter.Subscribe<List<OSBQ_Ex>>(this, _BinSerialListAddrs, (List<OSBQ_Ex> binSerialList) =>
                {
                    MessagingCenter.Unsubscribe<List<OSBQ_Ex>>(this, _BinSerialListAddrs);
                    List<TransferItemDetailBinM> pickedList = new List<TransferItemDetailBinM>();
                    var showList = string.Empty;
                    foreach (var serial in binSerialList)
                    {
                        pickedList.Add(new TransferItemDetailBinM
                        {
                            LineGuid = Guid.NewGuid(),
                            ItemCode = line.ItemCode,                            
                            Qty = 1,
                            Serial = serial.DistNumber,
                            Batch = string.Empty,
                            InternalSerialNumber = string.Empty,
                            ManufacturerSerialNumber = string.Empty,
                            BinAbs = serial.BinAbs,
                            BinCode = serial.BinCode,
                            SnBMDAbs = serial.SnBMDAbs,
                            WhsCode = (WhsDirection.Equals(_FROM)) ?
                                    currentDocLine.RequestFromWarehouse :
                                    currentDocLine.RequestToWarehouse,
                            Direction = WhsDirection
                        });
                        showList += $"{serial.BinCode} -> {serial.DistNumber}\n";
                    }

                    currentDocLine.ShowList = showList;
                    UpdateItem(currentDocLine, pickedList);
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
                DisplayAlert(excep.Message);
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
                    if (itemsSource[x].TransferItemList == null) continue;
                    if (itemsSource[x].TransferItemList.Count == 0) continue;

                    for (int y = 0; y < itemsSource[x].TransferItemList.Count; y++)
                    {
                        if (string.IsNullOrWhiteSpace(itemsSource[x].TransferItemList[y].Serial)) continue;
                        selectedSerial.Add(itemsSource[x].TransferItemList[y].Serial);
                    }
                }
                return selectedSerial;
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
                return null;
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
                    foreach (var serial in serialList)
                    {
                        pickedList.Add(new TransferItemDetailBinM
                        {
                            LineGuid = Guid.NewGuid(),
                            ItemCode = line.ItemCode,
                            Qty = 1,
                            Serial = serial.DistNumber,
                            Batch = string.Empty,
                            InternalSerialNumber = string.Empty,
                            ManufacturerSerialNumber = string.Empty,
                            BinAbs = -1,
                            SnBMDAbs = serial.SnBMDAbs,
                            WhsCode = (WhsDirection.Equals(_FROM)) ?
                                    currentDocLine.RequestFromWarehouse :
                                    currentDocLine.RequestToWarehouse,
                            Direction = WhsDirection
                        });
                        showList += $"{serial.DistNumber}\n";
                    }

                    currentDocLine.ShowList = showList;

                    UpdateItem(currentDocLine, pickedList);
                    DisplayToast("Saved.");
                });

                // get the selected serial to prevent serial being selected duplicated
                var selectedSerial = GetSelectedSerials();
                Navigation.PushAsync(new SerialListView(_SerialListAddrs,
                    line.ItemCode, line.ItemName, line.FromWhsCode, (int)line.RequestedQty, selectedSerial));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Update to pick list
        /// </summary>
        /// <param name="docLine"></param>
        /// <param name="pickedList"></param>
        void UpdateItemPicked(zwaTransferDocDetails docLine, List<TransferItemDetailBinM> pickedList)
        {
            if (itemsSourcePicked == null) return;
            if (itemsSourcePicked.Count == 0) return;

            // checking other doc line with from direction with same bin abs
            var lines = itemsSourcePicked.Where(x => x.Picked != null).ToList();

            if (lines == null) // if nothing picked then just add 
            {
                int locId = itemsSourcePicked.IndexOf(docLine);
                if (locId < 0) return;
                decimal sum = pickedList.Where(x => x.Direction.Equals(_TO)).Sum(x => x.Qty);

                itemsSourcePicked[locId].CellCompleteColor = Color.Green;
                itemsSourcePicked[locId].TransferQty = sum;
                itemsSourcePicked[locId].Picked = pickedList;
            }

            if (lines.Count == 0)
            {
                int locId = itemsSourcePicked.IndexOf(docLine);
                if (locId < 0) return;
                decimal sum = pickedList.Where(x => x.Direction.Equals(_TO)).Sum(x => x.Qty);

                itemsSourcePicked[locId].CellCompleteColor = Color.Green;
                itemsSourcePicked[locId].TransferQty = sum;
                itemsSourcePicked[locId].Picked = pickedList;
            }

            // check overall line of the doc line
            bool isDuplicatedBathAbs = false;
            foreach (var line in lines)
            {
                foreach (var linePick in line.Picked)
                {
                    //if (linePick.Direction.Equals(_TO)) continue; // continue when direction to to
                    foreach (var picked in pickedList)
                    {
                        if (linePick.Direction.Equals(_FROM) &&
                            linePick.DistNum.Equals(picked.DistNum) &&
                            linePick.BinAbs.Equals(picked.BinAbs))
                        {
                            isDuplicatedBathAbs = true;
                            break;
                        }
                    }
                }
            }

            // if found dunplicate
            if (isDuplicatedBathAbs)
            {
                DisplayAlert("The FROM and TO Bin for batch # are not allowed, " +
                    "please try to allocation to different bin, Thanks.");
                return;
            }
            else
            {
                int locId = itemsSourcePicked.IndexOf(docLine);
                if (locId < 0) return;
                decimal sum = pickedList.Where(x => x.Direction.Equals(_TO)).Sum(x => x.Qty);

                itemsSourcePicked[locId].CellCompleteColor = Color.Green;
                itemsSourcePicked[locId].TransferQty = sum;
                itemsSourcePicked[locId].Picked = pickedList;
            }
        }

        /// <summary>
        /// Update / refresh the list line
        /// </summary>
        /// <param name="docLine"></param>
        void UpdateItem(WTQ1_Ex docLine, List<TransferItemDetailBinM> pickedList)
        {
            if (itemsSource == null) return;
            if (itemsSource.Count == 0) return;

            int locId = itemsSource.IndexOf(docLine);
            if (locId < 0) return;
            decimal sum = pickedList.Sum(x => x.Qty);

            itemsSource[locId].CellCompleteColor = (sum > 0) ? Color.Green : Color.Red;
            itemsSource[locId].TransferQty = (pickedList == null) ? 0 : pickedList.Sum(x => x.Qty);
            itemsSource[locId].TransferItemList = pickedList;
        }

        /// <summary>
        /// Query server to get document series
        /// </summary>
        async void LoadDocSeries()
        {
            try
            {
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "GetDocSeries"
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
                    DisplayAlert($"{content}");
                    return;
                }

                var dtoOwtq = JsonConvert.DeserializeObject<DtoOwtq>(content);

                // load the doc series 
                if (dtoOwtq.DocSeries == null)
                {
                    DocSeries = dtoOwtq.DocSeries;
                    docSeriesItemsSource = new List<string>();
                    DocSeries.ForEach(x => docSeriesItemsSource.Add(x.SeriesName));
                }
                else
                {
                    docSeriesItemsSource = new List<string>();
                    docSeriesItemsSource.Add("Primary");
                }

                DocSeriesItemsSource = new ObservableCollection<string>(docSeriesItemsSource);
                OnPropertyChanged(nameof(DocSeriesItemsSource));

                DocSeriesSelectedItem = docSeriesItemsSource[0];
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Close the user screen
        /// </summary>
        void Close() => Navigation.PopAsync(); // <-- close the screen
    }
}
