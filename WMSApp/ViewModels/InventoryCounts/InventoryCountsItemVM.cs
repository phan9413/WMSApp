using Acr.UserDialogs;
using DbClass;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using WMSApp.Class;
using WMSApp.Class.Helper;
using WMSApp.Dtos;
using WMSApp.Interface;
using WMSApp.Models.GRPO;
using WMSApp.Models.SAP;
using WMSApp.Models.Share;
using WMSApp.Views.Share;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace WMSApp.ViewModels.InventoryCounts
{
    public class InventoryCountsItemVM : INotifyPropertyChanged
    {
        #region Screen binding
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string pName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pName));
        public Command CmdSaveUpdate { get; set; }
        public Command CmdClose { get; set; }
        public Command CmdStartScan { get; set; }
        public Command CmdManualInput { get; set; }

        string docDetails;
        public string DocDetails
        {
            get => docDetails;
            set
            {
                if (docDetails == value) return;
                docDetails = value;
                OnPropertyChanged(nameof(DocDetails));
            }
        }

        string selectedText;
        public string SelectedText
        {
            get => selectedText;
            set
            {
                if (selectedText == value) return;
                selectedText = value;
                OnPropertyChanged(nameof(SelectedText));
            }
        }

        string selectedDetails;
        public string SelectedDetails
        {
            get => selectedDetails;
            set
            {
                if (selectedDetails == value) return;
                selectedDetails = value;
                OnPropertyChanged(nameof(SelectedDetails));
            }
        }

        INC1_Ex selectedItem;
        public INC1_Ex SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;

                ProcessItem(selectedItem.ItemCode);
                
                //CaptureCounterQty(selectedItem);
                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

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

        //NNM1[] GrpoSeries; // kept orginal from server

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

        List<string> docSeriesItemsSource;
        public ObservableCollection<string> DocSeriesItemsSource { get; set; }
        List<INC1_Ex> itemsSource;
        public ObservableCollection<INC1_Ex> ItemsSource { get; set; }
        #endregion

        OINC_Ex selectedDoc { get; set; } = null;
        INavigation Navigation { get; set; } = null;
        zwaRequest currentRequest { get; set; } = null;
       
        readonly string _StartCameraScan = "_StartCameraScan_20201115T1043";
        readonly string _BatchCounting = "_BatchCounting_20201116T1128";
        readonly string _SerialCounting = "_SerialCounting_20201116T1225";
        readonly string _PopScreenAddr = "_PopScreenAddrInventoryCountItems_201207";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="navigation"></param>
        /// <param name="selected"></param>
        public InventoryCountsItemVM(INavigation navigation, OINC_Ex selected)
        {
            Navigation = navigation;
            DocDetails = selected.Text;
            SelectedText = selected.Details;
            selectedDoc = selected;

            InitCmd();
            LoadInvtCountLine();
            LoadDocSeries();
        }

        /// <summary>
        /// initial the command link method
        /// </summary>
        void InitCmd()
        {
            CmdSaveUpdate = new Command(SaveAndUpdate);
            CmdClose = new Command(() => { Navigation.PopAsync(); });
            CmdStartScan = new Command(HandlerStartScannerPage);
            CmdManualInput = new Command(LaunchSearchItemUpdate);
        }

        /// <summary>
        /// Handler the start scanning page
        /// </summary>
        async void HandlerStartScannerPage()
        {
            MessagingCenter.Subscribe(this, _StartCameraScan, (string code) =>
            {
                MessagingCenter.Unsubscribe<string>(this, _StartCameraScan);
                if (string.IsNullOrWhiteSpace(code)) return;
                if (code.ToLower().Equals("cancel")) return;
                ProcessItem(code);
            });

            await Navigation.PushAsync(new CameraScanView(_StartCameraScan));
        }

        /// <summary>
        /// Load the inventory count from server
        /// </summary>
        async void LoadInvtCountLine()
        {
            try
            {
                UserDialogs.Instance.ShowLoading("A moment please...");
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "GetInvtryCountDocLine",
                    OINCDocEntry = selectedDoc.DocEntry
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "InvntryCnt");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    ShowAlert(content);
                    return;
                }

                var retBag = JsonConvert.DeserializeObject<Cio>(content);
                if (retBag.InvenCountDocsLines == null) return;

                itemsSource = new List<INC1_Ex>();
                itemsSource.AddRange(retBag.InvenCountDocsLines);

                //retBag.InvenCountDocsLines.ForEach(x => { itemsSource.Add(new INC1_Ex { Line = x }); });

                ResetListView();
            }
            catch (Exception excep)
            {
                ShowAlert($"{excep.Message}");
                Console.WriteLine(excep.ToString());
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
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
                    request = "LoadDocSeries"
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "InvntryCnt");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    ShowAlert($"{content}");
                    return;
                }

                var dto_OINC = JsonConvert.DeserializeObject<DTO_OINC>(content);
                if (dto_OINC == null)
                {
                    ShowToast("Issue to read inventory counting doc series infor from server, Please try again later, Thanks.");
                    return;
                }

                // load the doc series 
                if (dto_OINC.DocSeries == null)
                {
                    docSeriesItemsSource = new List<string>();
                    dto_OINC.DocSeries.ForEach(x => docSeriesItemsSource.Add(x.SeriesName));
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
                ShowAlert(excep.Message);
            }
        }

        /// <summary>
        /// capture qty when user tap the listview
        /// </summary>
        /// <param name="selected"></param>
        async void CaptureCounterQty(INC1_Ex selectedItem)
        {
            try
            {
                int locId = itemsSource.IndexOf(selectedItem);
                if (locId < 0)
                {
                    ShowAlert("Unable to local the item in list, please try again. Thanks [x1]");
                    return;
                }

                if (selectedItem.CountedQty > 0)
                {
                    itemsSource[locId].CountedQty = 0;
                    return;
                }

                string receiptQty = await new Dialog().DisplayPromptAsync(
                    $"Please input {selectedItem.ItemCodeDisplay}",
                    $"{selectedItem.ItemDescDisplay}",
                    "OK",
                    "Cancel",
                    "",
                    -1,
                    Keyboard.Numeric,
                    "");

                if (receiptQty.ToLower().Equals("cancel")) return;

                if (receiptQty.Equals("")) return;

                var isNumeric = Decimal.TryParse(receiptQty, out decimal m_receiptQty);
                if (!isNumeric) return; // check numeric

                if (m_receiptQty.Equals(0)) // check zero, if zero assume as reset receipt qty
                {
                    // prompt select whs                   
                    itemsSource[locId].CountedQty = m_receiptQty; // assume as reset to the receipt
                    return;
                }

                // else           
                if (m_receiptQty < 0) // check negative
                {
                    ShowAlert($"The receive quantity ({m_receiptQty:N}) must be numeric and positve, " +
                        $"negative are no allowance.\nPlease try again later. Thanks [x2]");
                    return;
                }

                // update into the list in memory                
                itemsSource[locId].CountedQty = m_receiptQty;
                itemsSource[locId].ShowList = string.Empty;
            }
            catch (Exception excep)
            {
                ShowAlert(excep.Message);
                Console.WriteLine(excep.ToString());
            }
        }

        /// <summary>
        ///  Use by scan, manual input and 
        /// </summary>
        /// <param name="itemCode"></param>
        async void ProcessItem(string itemCode)
        {
            try
            {
                // check the item existing in line 
                if (itemsSource == null)
                {
                    ShowToast("Sorry the document line(s) is empty, please try again later, Thanks.");
                    return;
                }

                if (itemsSource.Count == 0)
                {
                    ShowToast("Sorry the document line(s) is empty, please try again later, Thanks.");
                    return;
                }

                var lowerCase = itemCode.ToLower();
                int indexOf = itemsSource.IndexOf(x => x.ItemCodeDisplay.ToLower().Equals(lowerCase));
                if (indexOf < 0)
                {
                    ShowToast($"The item code {itemCode.ToUpper()} can not be found in the counting document line(s), please try again, Thanks.");
                    return;
                }

                /// query the server to get the item property
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "GetItem",
                    QueryItemCode = itemCode
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
                    ShowAlert($"{content}");
                    return;
                }

                var repliedCio = JsonConvert.DeserializeObject<Cio>(content);
                if (repliedCio == null)
                {
                    ShowToast($"There is issue reading the item code {itemCode} from server, please try again later, Thanks.[CN]");
                    return;
                }

                if (repliedCio.Item == null)
                {
                    ShowToast($"There is issue reading the item code {itemCode} from server, please try again later, Thanks. [IN]");
                    return;
                }

                HandlerItemCode(repliedCio.Item, itemsSource[indexOf]);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Handler the line process
        /// check warehouse bin or not bin 
        /// check manage method of the item
        /// </summary>
        void HandlerItemCode(OITM item, INC1_Ex line)
        {
            try
            {
                if (item.ManBtchNum.Equals("Y"))
                {
                    // hander normal warehouse
                    // collect the batch information
                    HandlerBatchCounting(line);
                    return;
                }

                if (item.ManSerNum.Equals("Y"))
                {
                    HandlerSerialCounting( line);
                    return;
                }

                CaptureCounterQty(line); // manage by non 
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }
        }

        #region Serial counting 
        async void HandlerSerialCounting(INC1_Ex line)
        {
            try
            {
                MessagingCenter.Subscribe(this, _SerialCounting, (List<string> serials) =>
                {
                    MessagingCenter.Unsubscribe<List<string>>(this, _SerialCounting);
                    if (serials == null)
                    {
                        ShowToast("Serial counting operation cancel, please try again, Thanks.");
                        return;
                    }

                    if (serials.Count == 0)
                    {
                        ShowToast("Serial counting operation cancel, please try again, Thanks.");
                        return;
                    }

                    var showlist = string.Join("\n", serials);
                    int locIndex = itemsSource.IndexOf(line);
                    if (locIndex >= 0)
                    {
                        itemsSource[locIndex].Serials = serials;
                        itemsSource[locIndex].ShowList = showlist;
                        itemsSource[locIndex].CountedQty = serials.Count();
                        ResetListView();
                    }
                });

                await Navigation.PushAsync(new CollectSerialsView(_SerialCounting));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }
        }

        #endregion

        #region batch coounting 
        async void HandlerBatchCounting(INC1_Ex line)
        {
            try
            {
                MessagingCenter.Subscribe(this, _BatchCounting, (List<Batch> batches) => 
                {
                    MessagingCenter.Unsubscribe<List<Batch>>(this, _BatchCounting);
                    if (batches == null)
                    {
                        ShowToast("Batch counting operation cancel, please try again, Thanks.");
                        return;
                    }

                    if (batches.Count == 0)
                    {
                        ShowToast("Batch counting operation cancel, please try again, Thanks.");
                        return;
                    }

                    var showlist = string.Join("\n", batches.Select(x => $"{x.DistNumber} = {x.Qty:N}"));
                    int locIndex = itemsSource.IndexOf(line);
                    if (locIndex >=0)
                    {                        
                        itemsSource[locIndex].Batches = batches;
                        itemsSource[locIndex].ShowList = showlist;
                        itemsSource[locIndex].CountedQty = batches.Sum(x => x.Qty);
                        ResetListView();
                    }
                });

                await Navigation.PushAsync(new CollectBatchesView(_BatchCounting, false));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }
        }
        #endregion

        /// <summary>
        /// Launch Search ItemUpdate
        /// Prompt user to enter the item code
        /// Search the counts linst item 
        /// if found prompt user to enter the qty counted
        /// if not show alrt message
        /// </summary>
        async void LaunchSearchItemUpdate()
        {
            try
            {
                if (itemsSource == null) return;
                string searchItemCode = await new Dialog().DisplayPromptAsync(
                    $"Please input item code to search from the list",
                    "",
                    "OK",
                    "Cancel",
                    "",
                    -1,
                    Keyboard.Plain,
                    "");

                if (searchItemCode.ToLower().Equals("")) return;
                if (searchItemCode.ToLower().Equals("cancel")) return;

                searchItemCode = searchItemCode.ToLower();

                // start the search
                var foundItemIndex = itemsSource
                    .Where(x => x.ItemCodeDisplay.ToLower().Equals(searchItemCode)).FirstOrDefault();

                if (foundItemIndex == null)
                {
                    ShowAlert($"The search item code {searchItemCode} can not be found in the list, please try again later.");
                    return;
                }

                int indexLoc = itemsSource.IndexOf(foundItemIndex);
                if (indexLoc > 0)
                {
                    ProcessItem(searchItemCode);
                }
            }
            catch (Exception excep)
            {
                ShowAlert(excep.Message);
                Console.WriteLine(excep.ToString());
            }
        }

        /// <summary>
        /// Send request to server to update the item count
        /// </summary>
        async void SaveAndUpdate()
        {
            try
            {
                if (itemsSource == null)
                {
                    ShowAlert("The item list if empty, please try again later. Thanks");
                    return;
                }

                var transList = itemsSource.Where(x => x.CountedQty > 0).ToList();
                if (transList == null)
                {
                    ShowAlert($"Please ensure the all item list having counted quantity, please try again later. Thanks");
                    return;
                }

                if (transList.Count == 0)
                {
                    ShowAlert($"Please ensure the all item list having counted quantity, please try again later. Thanks");
                    return;
                }

                UserDialogs.Instance.ShowLoading($"Awating Inventry Count updating ...");

                // prepre the grpo object list 
                var groupingGuid = Guid.NewGuid();
                var lineList = new List<zwaGRPO>();
                var itemBinList = new List<zwaItemBin>();

                foreach (var line in transList)
                {
                    lineList.Add(new zwaGRPO
                    {
                        Guid = groupingGuid,
                        ItemCode = line.ItemCode,
                        Qty = line.CountedQty,
                        //SourceCardCode = "NA",
                        SourceDocNum = selectedDoc.DocNum,
                        SourceDocEntry = selectedDoc.DocNum,
                        SourceDocBaseType = -1, // from the stock count doc
                        SourceBaseEntry = line.DocEntry,
                        SourceBaseLine = line.LineNum,
                        Warehouse = line.WhsCode
                    });

                    var itemBin = line.GetList(groupingGuid);
                    if (itemBin == null) continue;
                    if (itemBin.Count == 0) continue;

                    itemBinList.AddRange(itemBin);
                }

                var headerDetails = new zmwDocHeaderField
                {
                    Guid = groupingGuid,
                    DocSeries = docSeriesSelectedItem,
                    Ref2 = ref2,
                    Comments = comments,
                    JrnlMemo = jrnlMemo,
                    NumAtCard = numberAtCard
                };


                // prepare the request dto                
                currentRequest = new zwaRequest
                {
                    sapUser = App.waUser,
                    request = $"Update Inventry Count",
                    guid = groupingGuid,
                    status = "ONHOLD",
                    phoneRegID = App.waToken,
                    tried = 0,
                    popScreenAddress = _PopScreenAddr
                };

                App.RequestList.Add(currentRequest);

                var requestBag = new Cio
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "UpdateInventoryCount",

                    dtoRequest = currentRequest,
                    dtoGRPO = lineList.ToArray(), 
                    dtoItemBins = itemBinList?.ToArray(), 
                    dtozmwDocHeaderField = headerDetails
                };

                // send over server to create request
                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(requestBag, "InvntryCnt");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    ShowAlert(content);
                    return;
                }

                if (!App.IsContinueRequestChecking)
                {
                    App.IsContinueRequestChecking = true;
                    App.StartDocStatusCheckTimer();
                }

                ShowToast("Request sent.");
                RegisterPopScreen();
            }
            catch (Exception excep)
            {
                ShowAlert($"{excep}");
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
        //    var callerAddress = "InventoryCounting_DocCheck";
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
        /// Show the message by toast
        /// </summary>
        /// <param name="message"></param>
        void ShowToast(string message) => DependencyService.Get<IToastMessage>()?.LongAlert(message);

        /// <summary>
        /// Refresh the list view
        /// </summary>
        void ResetListView()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<INC1_Ex>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// Show alert
        /// </summary>
        /// <param name="message"></param>
        async void ShowAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "Ok");

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose() { }// => GC.Collect();
    }
}
