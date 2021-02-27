using Acr.UserDialogs;
using DbClass;
using Newtonsoft.Json;
using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WMSApp.Class;
using WMSApp.Class.Helper;
using WMSApp.Dtos;
using WMSApp.Interface;
using WMSApp.Models.GRPO;
using WMSApp.Models.SAP;
using WMSApp.Models.Share;
using WMSApp.Models.Transfer1;
using WMSApp.ViewModels.Transfer1;
using WMSApp.Views.Return;
using WMSApp.Views.Share;
using WMSApp.Views.Transfer1;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace WMSApp.ViewModels.GoodsReturn
{
    public class GoodsReturnItemsVM : INotifyPropertyChanged
    {
        #region View property
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Command CmdClose { get; set; }
        public Command CmdSaveUpdate { get; set; }
        public Command CmdStartScan { get; set; }
        public Command CmdSearchBarVisible { get; set; }

        string retDetails;
        public string RetDetails
        {
            get => retDetails;
            set
            {
                if (retDetails == value) return;
                retDetails = value;
                OnPropertyChanged(nameof(RetDetails));
            }
        }
        public string SelectedReturnCardCode { get; set; }
        public string SelectedReturnCardName { get; set; }

        ReturnCommonHead_Ex selectedDocRet;
        public ReturnCommonHead_Ex SelectedDocRet
        {
            get => selectedDocRet;
            set
            {
                HandleSelectedRetDoc(value);
                selectedDocRet = null;
                OnPropertyChanged(nameof(SelectedDocRet));
            }
        }

        List<ReturnCommonHead_Ex> itemsSourceRet { get; set; } // purchase order
        public ObservableCollection<ReturnCommonHead_Ex> ItemsSourceRet { get; set; }        
        ReturnCommonLine_Ex selectedItem_chgWhs { get; set; }
        ReturnCommonLine_Ex selectedItem { get; set; }
        public ReturnCommonLine_Ex SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;

                // handler the selected item
                // prompt qty entry and update the item                
                HandlerItemCodeSecStep(selectedItem);

                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        List<ReturnCommonLine_Ex> itemsSource;
        public ObservableCollection<ReturnCommonLine_Ex> ItemsSource { get; set; }

        public string SearchItemQuery
        {
            set => FilterItemSearch(value);
        }

        string selectedMajorWhs;
        public string SelectedMajorWhs
        {
            get => $"Entry to warehouse:\n{selectedMajorWhs}";
            set
            {
                if (selectedMajorWhs == value) return;
                selectedMajorWhs = value;
                OnPropertyChanged(nameof(SelectedMajorWhs));
            }
        }

        bool isFollowMajorWarehouse;
        public bool IsFollowMajorWarehouse
        {
            get => isFollowMajorWarehouse;
            set
            {
                if (isFollowMajorWarehouse == value) return;
                isFollowMajorWarehouse = value;
                OnPropertyChanged(nameof(IsFollowMajorWarehouse));
            }
        }

        bool isRetDetailsExpand;
        public bool IsRetDetailsExpand
        {
            get => isRetDetailsExpand;
            set
            {
                if (isRetDetailsExpand == value) return;
                isRetDetailsExpand = value;
                OnPropertyChanged(nameof(IsRetDetailsExpand));
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

        #endregion
        /// <summary>
        /// Inner declaration
        /// </summary>        
        INavigation Navigation { get; set; } = null;
        zwaRequest currentRequest { get; set; } = null;
        OITM currentItem { get; set; } = null;
        bool IsStopTimmer { get; set; } = false;
        string currentSourceType { get; set; } = string.Empty;
        NNM1[] DocSeries { get; set; }

        readonly string _SelectDuplicateItemLines = "_SelectDuplicateItemLines_GR";
        readonly string _SelectecChangeWarehouse = "_SelectecChangeWarehouse20201113T1113_GR";
        readonly string _PickReturnLine = "_PickReturnLine20201117T2053_GR";               
        readonly string _PickFromSerialBin = "_PickFromSerialBin20201125T1557_GR";
        readonly string _PickSerialListView = "_PickSerialListView20201125T1559_GR";
        readonly string _PickBatchesFromBin = "_PickBatchesFromBin20201125T1602_GR";
        readonly string _PickBatchFromWhs = "_PickBatchFromWhs20201125T1602_GR";
        readonly string _PickItemQtyFromBin = "_PickItemQtyFromBin20201125T1605_GR";
        readonly string _GRPO = "GRPO";

        readonly string _PopScreenAddr = "_PopScreenAddr_GoodReturn201207";

        /// <summary
        /// Constructor, starting point
        /// </summary>
        public GoodsReturnItemsVM(INavigation navigation, List<ReturnCommonHead_Ex> selected, string sourceType)
        {
            Navigation = navigation;
            IsRetDetailsExpand = false;
            currentSourceType = sourceType;

            itemsSourceRet = selected;
            RefreshListReturnListView();

            InitCmd();
            if (currentSourceType.Contains(_GRPO))
            {
                LoadGRPOLines();
                return;
            }

            LoadGRRLines();
        }

        /// <summary>
        /// show list of the po lines
        /// </summary>
        /// <param name="selectedDoc"></param>
        void HandleSelectedRetDoc(ReturnCommonHead_Ex selectedDoc)
        {
            try
            {
                if (itemsSource == null) return;
                if (itemsSource.Count == 0) return;

                var lines = itemsSource.Where(x => x.DocEntry.Equals(selectedDoc.DocEntry)).ToList();
                if (lines == null) return;
                if (lines.Count == 0) return;

                MessagingCenter.Subscribe(this, _PickReturnLine, (ReturnCommonLine_Ex selectedLine) =>
                {
                    MessagingCenter.Unsubscribe<ReturnCommonLine_Ex>(this, _PickReturnLine);
                    if (selectedLine == null) return;
                    if (selectedLine.Head == null) return;
                    if (selectedLine == null) return;

                    HandlerItemCodeSecStep(selectedLine);
                    return;
                });

                Navigation.PushPopupAsync(new ReturnItemLinePopUpView(lines, selectedDoc, _PickReturnLine));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                ShowAlert(e.Message);
            }
        }

        /// <summary>
        /// Formating the doc so lines number
        /// </summary>
        void GetDocCounts()
        {
            if (itemsSourceRet == null)
            {
                RetDetails = $"{currentSourceType} details";
                return;
            }
            if (itemsSourceRet.Count == 0)
            {
                RetDetails = $"{currentSourceType} details";
                return;
            }

            if (itemsSource == null) return;
            if (itemsSource.Count == 0) return;

            RetDetails = $"{currentSourceType} details, {itemsSourceRet?.Count} selected, {itemsSource.Count} item(s)";
        }

        /// <summary>
        /// refresh the listview 
        /// </summary>
        void RefreshListReturnListView()
        {
            if (itemsSourceRet == null) return;
            if (itemsSourceRet.Count == 0) return;
            ItemsSourceRet = new ObservableCollection<ReturnCommonHead_Ex>(itemsSourceRet);
            OnPropertyChanged(nameof(ItemsSourceRet));
        }

        /// <summary>
        /// Init the command
        /// </summary>
        void InitCmd()
        {
            /// close this, and return to main
            CmdClose = new Command(() => { Navigation?.PopAsync(); });

            // perfor save to server
            CmdSaveUpdate = new Command(SaveAndUpdate);

            // navigate to scanner view
            CmdStartScan = new Command(() =>
            {
                if (itemsSource == null) return;
                HandlerScanAddItem();
            });

            CmdSearchBarVisible = new Command<SearchBar>((SearchBar searchBar) =>
            {
                searchBar.IsVisible = !searchBar.IsVisible;
                if (searchBar.IsVisible)
                {
                    IsRetDetailsExpand = false;
                    searchBar.Focus();
                    return;
                }
            });
        }

        /// <summary>
        /// Handle the wahrehouse change
        /// </summary>
        /// <param name="line"></param>
        public void HandlerLineWhsChanged(ReturnCommonLine_Ex line)
        {
            try
            {
                selectedItem_chgWhs = line;
                if (line.EntryQuantity > 0)
                {
                    ResetLine(line);
                }

                MessagingCenter.Subscribe(this, _SelectecChangeWarehouse, (OWHS selectWhs) =>
                {
                    MessagingCenter.Unsubscribe<OWHS>(this, _SelectecChangeWarehouse);

                    if (selectWhs == null) return;
                    int locid = itemsSource.IndexOf(selectedItem_chgWhs);

                    if (locid >= 0)
                    {
                        itemsSource[locid].ItemWhsCode = selectWhs.WhsCode;
                        //HandlerItemCodeSecStep(itemsSource[locid]);
                        return;
                    }

                    ShowToast("There is error to access the item, please try again later, Thanks.");
                    return;
                });

                Navigation.PushPopupAsync(
                    new PickWarehousePopUpView(_SelectecChangeWarehouse, "Pick a exit warehouse", Color.Gray));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }
        }

        /// <summary>
        /// Handler when user select scan item code 
        /// </summary>
        async void HandlerScanAddItem()
        {
            string returnAddress = "HandlerScanAddItem";
            MessagingCenter.Subscribe(this, returnAddress, async (string scanItemCode) =>
            {
                MessagingCenter.Unsubscribe<string>(this, returnAddress);
                if (string.IsNullOrWhiteSpace(scanItemCode)) return;
                if (scanItemCode.Equals("cancel")) return;

                // process to add in the item code   
                HandlerItemCode(scanItemCode);
            });

            await Navigation.PushAsync(new CameraScanView(returnAddress));
        }

        /// <summary>
        /// Query the server on the item code
        /// </summary>
        /// <param name="itemCode"></param>
        /// <returns></returns>
        async Task<OITM> GetItem(string itemCode)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("A moment ...");
                // query the item from server
                var requestBag = new Cio
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "GetItem",
                    QueryItemCode = itemCode
                };

                // send over server to create request
                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(requestBag, "Items");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    ShowAlert(content);
                    return null;
                }

                Cio replied = JsonConvert.DeserializeObject<Cio>(content);
                if (replied == null)
                {
                    ShowAlert("There is error reading information from server, please try again later. Thanks");
                    return null;
                }

                if (replied.Item == null)
                {
                    ShowAlert($"The item code {itemCode} no found in system setup, " +
                        $"please confirm the code scan / input, and try again. Thanks");
                    return null;
                }

                return replied.Item;
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
                return null;
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Get list of batch from the delivery order
        /// </summary>
        /// <param name="DoDocEtry"></param>
        /// <returns></returns>
        async Task<List<Batch>> GetBatchesInDo(int docEntry, int lineNum)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("A moment ...");
                //int[] docEntries = itemsSourceRet.Select(x => x.DocEntry).Distinct().ToArray();

                // query the item from server
                var requestBag = new Cio
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "GetBatchesInDo",
                    QueryDocEntry = docEntry,
                    QueryDocLineNum = lineNum
                };

                // send over server to create request
                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(requestBag, "Return");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    //ShowAlert(content);
                    return null;
                }

                var batches = JsonConvert.DeserializeObject<Batch[]>(content);
                if (batches == null)
                {
                    ShowAlert("There is error reading information from server, please try again later. Thanks");
                    return null;
                }

                if (batches.Length == 0)
                {
                    ShowAlert("There is error reading information from server, please try again later. Thanks");
                    return null;
                }

                return new List<Batch>(batches);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
                return null;
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Get list of batch from the delivery order
        /// </summary>
        /// <param name="DoDocEtry"></param>
        /// <returns></returns>
        async Task<List<string>> GetSerialInDo(int docEntry, int lineNum)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("A moment ...");
                int[] docEntries = itemsSourceRet.Select(x => x.DocEntry).Distinct().ToArray();

                // query the item from server
                var requestBag = new Cio
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "GetSerialInDo",
                    QueryDocEntry = docEntry,
                    QueryDocLineNum = lineNum
                };

                // send over server to create request
                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(requestBag, "Return");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    //ShowAlert(content);
                    return null;
                }

                var serials = JsonConvert.DeserializeObject<List<string>>(content);
                if (serials == null)
                {
                    ShowAlert("There is error reading information from server, please try again later. Thanks");
                    return null;
                }

                if (serials.Count == 0)
                {
                    ShowAlert("There is error reading information from server, please try again later. Thanks");
                    return null;
                }
                return serials;
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
                return null;
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Filtering the search
        /// </summary>
        /// <param name="query"></param>
        void FilterItemSearch(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    ResetListView();
                    return;
                }

                var lowerCaseQuery = query.ToLower();
                var filter = itemsSource.Where(x => x.ItemCode != null && x.ItemCode.ToLower().Contains(lowerCaseQuery) ||
                                                x.Dscription.ToLower().Contains(lowerCaseQuery)).ToList();

                if (filter == null)
                {
                    ResetListView();
                    return;
                }

                ItemsSource = new ObservableCollection<ReturnCommonLine_Ex>(filter);
                OnPropertyChanged(nameof(ItemsSource));
            }
            catch (Exception excep)
            {
                ShowAlert($"{excep}");
            }
        }

        /// <summary>
        /// load the do lins as return common line iin this view
        /// </summary>
        /// <returns></returns>
        async void LoadGRRLines()
        {
            try
            {
                UserDialogs.Instance.ShowLoading("A moment please...");

                if (itemsSourceRet == null) return;
                if (itemsSourceRet.Count == 0) return;

                var docEntries = itemsSourceRet.Where(x => x.IsChecked = true).Select(p => p.DocEntry).ToArray();

                if (docEntries == null) return;
                if (docEntries.Length == 0) return;

                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "GetOpenGrrLines",
                    PoDocEntries = docEntries
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "GoodsReturn");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    ShowAlert($"{content}");
                    return;
                }

                var dtoOprr = JsonConvert.DeserializeObject<DTO_OPRR>(content);
                if (dtoOprr == null)
                {
                    ShowToast("Error reading information from server, please try again later. Thanks. [CN]");
                    return;
                }

                if (dtoOprr.PRR1_Exs == null)
                {
                    ShowToast("Error reading information from server, please try again later. Thanks [SN]");
                    return;
                }

                itemsSource = new List<ReturnCommonLine_Ex>();
                var lineList = new List<PRR1_Ex>(dtoOprr.PRR1_Exs);

                lineList.ForEach(x =>
                {
                    itemsSource.Add(new ReturnCommonLine_Ex
                    {
                        ItemCode = x.ItemCode,
                        Dscription = x.Dscription,
                        Quantity = x.Quantity,
                        OpenQty = x.OpenQty,

                        WhsCode = x.WhsCode,
                        FromWhsCod = x.FromWhsCod,
                        Head = itemsSourceRet.Where(prr => prr.DocEntry.Equals(x.DocEntry)).FirstOrDefault(),
                        ItemWhsCode = x.WhsCode,
                        Navigation = Navigation,
                        Direction = "Out",
                        BaseWarehouse = x.WhsCode,
                        DocEntry = x.DocEntry,

                        LineNum = x.LineNum,
                        ObjType = x.ObjType
                    });
                });

                // update the po line count 
                itemsSourceRet.ForEach(x =>
                {
                    var lineCount = itemsSource.Count(l => l.DocEntry.Equals(x.DocEntry));
                    x.LineCount = lineCount;
                });

                ResetListView();
                GetDocCounts();

                //load the doc series
                if (dtoOprr.DocSeries == null)
                {
                    docSeriesItemsSource = new List<string>();
                    docSeriesItemsSource.Add("Primary");
                }
                else
                {
                    DocSeries = dtoOprr.DocSeries;
                    docSeriesItemsSource = new List<string>();
                    dtoOprr.DocSeries.ForEach(x => docSeriesItemsSource.Add(x.SeriesName));
                }

                DocSeriesItemsSource = new ObservableCollection<string>(docSeriesItemsSource);
                OnPropertyChanged(nameof(DocSeriesItemsSource));
                DocSeriesSelectedItem = docSeriesItemsSource[0];
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert($"{excep.Message}");
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Connect to server to load selected PO lines
        /// </summary>
        async void LoadGRPOLines()
        {
            try
            {
                UserDialogs.Instance.ShowLoading("A moment please...");

                if (itemsSourceRet == null) return;
                if (itemsSourceRet.Count == 0) return;

                var docEntries = itemsSourceRet.Where(x => x.IsChecked = true).Select(p => p.DocEntry).ToArray();

                if (docEntries == null) return;
                if (docEntries.Length == 0) return;

                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "GetGrpoLines",
                    PoDocEntries = docEntries
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "GoodsReturn");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    ShowAlert($"{content}");
                    return;
                }

                var DtoOPRR = JsonConvert.DeserializeObject<DTO_OPRR>(content);
                if (DtoOPRR == null)
                {
                    ShowToast("Error reading information from server, please try again later. Thanks. [CN]");
                    return;
                }

                if (DtoOPRR.PDN1s == null)
                {
                    ShowToast("Error reading information from server, please try again later. Thanks [SN]");
                    return;
                }

                itemsSource = new List<ReturnCommonLine_Ex>();
                var lineList = new List<PDN1_Ex>(DtoOPRR.PDN1s);

                lineList.ForEach(x =>
                {
                    itemsSource.Add(new ReturnCommonLine_Ex
                    {
                        ItemCode = x.ItemCode,
                        Dscription = x.Dscription,
                        Quantity = x.Quantity,
                        OpenQty = x.OpenQty,

                        WhsCode = x.WhsCode,
                        FromWhsCod = x.FromWhsCod,
                        Head = itemsSourceRet.Where(prr => prr.DocEntry.Equals(x.DocEntry)).FirstOrDefault(),
                        ItemWhsCode = x.WhsCode,

                        Navigation = Navigation,
                        Direction = "Out",
                        BaseWarehouse = x.WhsCode,
                        DocEntry = x.DocEntry,

                        LineNum = x.LineNum,
                        ObjType = x.ObjType
                    });
                });

                // update the po line count 
                itemsSourceRet.ForEach(x =>
                {
                    var lineCount = itemsSource.Count(l => l.DocEntry.Equals(x.DocEntry));
                    x.LineCount = lineCount;
                });

                ResetListView();
                GetDocCounts();

                //load the doc series
                if (DtoOPRR.DocSeries == null)
                {
                    docSeriesItemsSource = new List<string>();
                    docSeriesItemsSource.Add("Primary");
                }
                else
                {
                    DocSeries = DtoOPRR.DocSeries;
                    docSeriesItemsSource = new List<string>();
                    DtoOPRR.DocSeries.ForEach(x => docSeriesItemsSource.Add(x.SeriesName));
                }

                DocSeriesItemsSource = new ObservableCollection<string>(docSeriesItemsSource);
                OnPropertyChanged(nameof(DocSeriesItemsSource));
                DocSeriesSelectedItem = docSeriesItemsSource[0];
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert($"{excep.Message}");
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Main procedure to handle the code std way
        /// </summary>
        /// <param name="otemCode"></param>
        async void HandlerItemCode(string itemCode)
        {
            try
            {
                // check item source contain the item 
                var lowercase = itemCode.ToLower();

                var foundRetItems = itemsSource
                    .Where(x => !string.IsNullOrWhiteSpace(x.ItemCode) &&
                    x.ItemCode.ToLower().Equals(lowercase)).ToList();

                // locate the correct item to do receipt
                if (foundRetItems == null)
                {
                    ShowAlert($"Sorry the item code {itemCode} no found in system setup. Please try other code, Thanks [L]");
                    return;
                }

                if (foundRetItems.Count == 0)
                {
                    ShowAlert($"Sorry the item code {itemCode} no found in system setup. Please try other code, Thanks [LZ]");
                    return;
                }

                if (foundRetItems.Count == 1)
                {
                    HandlerItemCodeSecStep(foundRetItems[0]);
                    return;
                }

                // there is more than 1 same item scan
                MessagingCenter.Subscribe(this, _SelectDuplicateItemLines, (ReturnCommonLine_Ex selected) =>
                {
                    MessagingCenter.Unsubscribe<ReturnCommonLine_Ex>(this, _SelectDuplicateItemLines);
                    if (selected == null) return;
                    HandlerItemCodeSecStep(selected);
                    return;
                });

                // resued the screen
                await Navigation.PushPopupAsync(new ReturnItemLinesPopUpSelectView(foundRetItems, _SelectDuplicateItemLines));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }
        }

        /// <summary>
        /// Handler next step
        /// </summary>
        async void HandlerItemCodeSecStep(ReturnCommonLine_Ex line)
        {
            try
            {
                currentItem = await GetItem(line.ItemCode);
                if (currentItem == null)
                {
                    ShowAlert($"Sorry the item code {line.ItemCode} " +
                        $"no found in system setup. Please try other code, Thanks [I]");
                    return;
                }

                if (currentItem.ManSerNum.Equals("Y"))
                {
                    PrepareSerialGr(line);
                    return;
                }

                if (currentItem.ManBtchNum.Equals("Y"))
                {
                    PrepareBatchGr(line);
                    return;
                }

                // ELSE handle no manage item
                PrepareNonManageGR(line);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }
        }

        #region PrepareNonManage Prr
        void PrepareNonManageGR(ReturnCommonLine_Ex line)
        {
            try
            {
                OWHS lineWhs;
                if (!string.IsNullOrWhiteSpace(line.ItemWhsCode))
                {
                    lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.ItemWhsCode)).FirstOrDefault();
                }
                else
                {
                    lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.WhsCode)).FirstOrDefault();
                }

                if (lineWhs.BinActivat.Equals("Y"))
                {
                    HandlerNonManageItemBin(line, lineWhs); // handler bin warehouse
                    return;
                }

                // handle normal qty capture
                CaptureItemExitQty(line, lineWhs);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }
        }

        /// <summary>
        /// Handler non manage item
        /// </summary>
        async void HandlerNonManageItemBin(ReturnCommonLine_Ex line, OWHS targetWhs) // Handler bin warehouse
        {
            // checking
            if (App.Warehouses == null)
            {
                ShowToast("Unable to process the next step, " +
                    "as the warehouse configuration is no load in correctly, please close the app, and try again later, Thanks. ");
                return;
            }

            if (itemsSource == null)
            {
                ShowToast("Unable to process the next step, " +
                    "as the warehouse configuration is no load in correctly, please close the app, and try again later, Thanks. ");
                return;
            }

            //reset the qty
            if (line.entryQuantity > 0)
            {
                ResetLine(line);
                return;
            }

            //string inputQty = await new Dialog().DisplayPromptAsync(
            //    $"Input item code {line.ItemCodeDisplay}", "receipt qty", "Ok", "cancel", null, -1, Keyboard.Numeric, "");

            //if (string.IsNullOrWhiteSpace(inputQty))
            //{
            //    return;
            //}

            //if (inputQty.ToLower().Equals("cancel"))
            //{
            //    return;
            //}

            //bool IsNumeric = decimal.TryParse(inputQty, out decimal result);

            //if (!IsNumeric)
            //{
            //    ShowToast("Invalid receipt qty, please enter numeric and positive value, please try again later. Thanks");
            //    return;
            //}

            //if (result <= 0)
            //{
            //    ShowToast("Invalid receipt qty, please enter numeric and positive value, please try again later. Thanks");
            //    return;
            //}

            //OWHS lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.POLine.WhsCode)).FirstOrDefault();
            if (targetWhs.BinActivat.Equals("Y"))
            {
                // show serial to bin allocation 
                MessagingCenter.Subscribe(this, _PickItemQtyFromBin, (List<OIBQ_Ex> qtyBin) =>
                {
                    MessagingCenter.Unsubscribe<List<OIBQ_Ex>>(this, _PickItemQtyFromBin);

                    if (qtyBin == null)
                    {
                        ShowToast("Pick item qty from bin operation cancel");
                        return;
                    }

                    if (qtyBin.Count == 0)
                    {
                        ShowToast("Pick item qty from bin operation cancel");
                        return;
                    }

                    int lineLocId1 = itemsSource.IndexOf(line);
                    if (lineLocId1 < 0)
                    {
                        ShowToast("There is issue to locate the item in the list, please try again later, Thanks.");
                        return;
                    }

                    itemsSource[lineLocId1].EntryQuantity = qtyBin.Sum(x => x.TransferQty);
                    itemsSource[lineLocId1].ItemQtyInBin = qtyBin;
                    itemsSource[lineLocId1].ItemWhsCode = targetWhs.WhsCode;

                    var showList = $"Frm Whs {targetWhs.WhsCode} ->\n" +
                                    String.Join("\n", qtyBin.Select(x => $"{x.BinCode:N} -> {x.TransferQty:N}"));

                    itemsSource[lineLocId1].ShowList = showList;
                });

                // launch the item qyt from bin arragement
                await Navigation.PushAsync(new BinItemListView(
                    _PickItemQtyFromBin, line.ItemCodeDisplay,
                    line.ItemNameDisplay, targetWhs.WhsCode, line.OpenQty));
            }
        }

        /// <summary>
        /// Prompt dialog to capture the Qty
        /// </summary>
        /// <param name="selectedPOLine"></param>
        async void CaptureItemExitQty(ReturnCommonLine_Ex line, OWHS targetWhs)
        {
            try
            {
                int locId = itemsSource.IndexOf(itemsSource
                .Where(x => x.Head.DocEntry.Equals(line.DocEntry) && x.LineNum.Equals(line.LineNum)).FirstOrDefault());

                if (locId < 0)
                {
                    ShowAlert("Unable to local the item in list, please try again. Thanks [x1]");
                    return;
                }

                if (line.entryQuantity > 0)
                {
                    ResetLine(line);
                    return;
                }

                string exitQty = await new Dialog().DisplayPromptAsync(
                    $"Input exit qty for {line.ItemCodeDisplay}",
                    $"{line.ItemNameDisplay}\nOpen Qty: {line.OpenQty:N}",
                    "OK",
                    "Cancel",
                    "",
                    -1,
                    Keyboard.Numeric,
                    $"{line.OpenQty:N}");

                var isNumeric = Decimal.TryParse(exitQty, out decimal m_exitQty);
                if (!isNumeric) // check numeric
                {
                    return;
                }

                if (m_exitQty.Equals(0)) // check zero, if zero assume as reset receipt qty
                {
                    // prompt select whs             
                    itemsSource[locId].EntryQuantity = m_exitQty; // assume as reset to the receipt
                    return;
                }

                // else           
                if (m_exitQty < 0) // check negative
                {
                    ShowAlert($"The quantity ({m_exitQty:N}) input must be numeric and positve," +
                        $" negative are no allowance.\nPlease try again later. Thanks [x2]");
                    return;
                }

                #region reserved control
                //if (itemsSource[locId].POLine.OpenQty < m_exitQty) // check open qty
                //{
                //    ShowAlert($"The receive quantity ({m_exitQty:N}) must be equal or " +
                //        $"smaller than the <= Open qty ({itemsSource[locId].POLine.OpenQty:N}).\nPlease try again later. Thanks [x3]");
                //    return;
                //}
                #endregion 

                // update into the list in memory    
                // non manage item
                itemsSource[locId].EntryQuantity = m_exitQty;
                itemsSource[locId].ItemWhsCode = targetWhs.WhsCode;
                itemsSource[locId].ShowList = $"{targetWhs.WhsCode} -> {m_exitQty:N}";
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }
        }
        #endregion

        #region PrepareBatch GR
        void PrepareBatchGr(ReturnCommonLine_Ex line)
        {
            try
            {
                if (line.entryQuantity > 0)
                {
                    ResetLine(line);
                    return;
                }
                HandlerBatchsItem(null, line);
            }
            catch (Exception excep)
            {
                Console.Write(excep.ToString());
                ShowAlert(excep.Message);
            }
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="batches"></param>
        /// <param name="line"></param>
        void HandlerBatchsItem(List<Batch> batches, ReturnCommonLine_Ex line)
        {
            try
            {
                // checking
                if (App.Warehouses == null)
                {
                    ShowToast("Unable to process the next step, " +
                        "as the warehouse configuration is no load in correctly, " +
                        "please close the app, and try again later, Thanks. ");
                    return;
                }

                if (itemsSource == null)
                {
                    ShowToast("Unable to process the next step, " +
                        "as the warehouse configuration is no load in correctly, " +
                        "please close the app, and try again later, Thanks. ");
                    return;
                }

                OWHS lineWhs = null;
                if (!string.IsNullOrWhiteSpace(line.ItemWhsCode))
                {
                    lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.ItemWhsCode)).FirstOrDefault();
                }
                else
                {
                    lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.WhsCode)).FirstOrDefault();
                }

                if (lineWhs == null)
                {
                    ShowToast("Error reading warehouse info, please try again later, Thanks.");
                    return;
                }

                #region batch picking from bin warehouse
                if (lineWhs.BinActivat.Equals("Y"))
                {
                    // show serial to bin allocation 
                    MessagingCenter.Subscribe(this, _PickBatchesFromBin, (List<OBBQ_Ex> pickedBatchInBins) =>
                    {
                        MessagingCenter.Unsubscribe<List<OBBQ_Ex>>(this, _PickBatchesFromBin);

                        if (pickedBatchInBins == null)
                        {
                            ShowToast("Pick batch from bin operation cancel");
                            return;
                        }

                        if (pickedBatchInBins.Count == 0)
                        {
                            ShowToast("Pick batch from bin operation cancel");
                            return;
                        }

                        string showList = $"Frm Whs {lineWhs.WhsCode} ->\n";
                        showList += string.Join("\n",
                            pickedBatchInBins.Select(x => $"{x.BinCode} -> {x.DistNumber} -> {x.TransferBatchQty:N}"));

                        int lineLocId1 = itemsSource.IndexOf(line);
                        if (lineLocId1 < 0)
                        {
                            ShowToast("There is issue to locate the item in the list, please try again later, Thanks.");
                            return;
                        }

                        itemsSource[lineLocId1].EntryQuantity = pickedBatchInBins.Sum(x => x.TransferBatchQty);
                        itemsSource[lineLocId1].BatchesInBin = pickedBatchInBins;
                        itemsSource[lineLocId1].ItemWhsCode = lineWhs.WhsCode;
                        itemsSource[lineLocId1].ShowList = showList;
                    });

                    // launch the serial to bin arragement
                    Navigation.PushAsync(new BinBatchListView(_PickBatchesFromBin,
                        line.ItemCodeDisplay, line.ItemNameDisplay, lineWhs.WhsCode, line.OpenQty));
                    return;
                }
                #endregion

                #region batch picking from warehouse
                MessagingCenter.Subscribe(this, _PickBatchFromWhs, (List<OBTQ_Ex> batchInWhs) =>
                {
                    MessagingCenter.Unsubscribe<List<OBTQ_Ex>>(this, _PickBatchFromWhs);
                    if (batchInWhs == null)
                    {
                        ShowToast("Pick batch from warehouse operation cancel");
                        return;
                    }
                    if (batchInWhs.Count == 0)
                    {
                        ShowToast("Pick batch from warehouse operation cancel");
                        return;
                    }

                    // add into the line with serial list capture
                    // no to bin warehouse operation                    
                    int lineLocId = itemsSource.IndexOf(line);
                    if (lineLocId < 0)
                    {
                        ShowToast("There is issue to locate the item in the list, please try again later, Thanks.");
                        return;
                    }

                    itemsSource[lineLocId].EntryQuantity = batchInWhs.Sum(x => x.TransferBatchQty); ;
                    itemsSource[lineLocId].BatchesInWhs = batchInWhs;
                    itemsSource[lineLocId].ItemWhsCode = lineWhs.WhsCode;
                    itemsSource[lineLocId].ShowList =
                            $"Frm Whs {lineWhs.WhsCode} ->\n" +
                            string.Join("\n", batchInWhs.Select(x => $"{x.DistNumber} -> {x.TransferBatchQty:N}").ToList());
                });

                Navigation.PushAsync(new BatchListView(_PickBatchFromWhs,
                    line.ItemCodeDisplay, line.ItemNameDisplay, lineWhs.WhsCode, line.OpenQty));

                #endregion 
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }
        }
        #endregion

        #region PrepareSerial GR => serial manage handle step
        /// <summary>
        ///  Prepare the serial to bin allocation screen
        /// </summary>
        void PrepareSerialGr(ReturnCommonLine_Ex line)
        {
            if (line.entryQuantity > 0)
            {
                ResetLine(line);
                return;
            }
            HandlerSerialItem(null, line);
        }

        /// <summary>
        /// Handler the serial Item
        /// </summary>
        async void HandlerSerialItem(List<string> serials, ReturnCommonLine_Ex line) // handler bin and no bin warehouse
        {
            try
            {
                // checking
                if (App.Warehouses == null)
                {
                    ShowToast("Unable to process the next step, " +
                        "as the warehouse configuration is no load in correctly, " +
                        "please close the app, and try again later, Thanks. ");
                    return;
                }

                if (itemsSource == null)
                {
                    ShowToast("Unable to process the next step, " +
                        "as the warehouse configuration is no load in correctly, " +
                        "please close the app, and try again later, Thanks. ");
                    return;
                }

                OWHS lineWhs = null;
                if (!string.IsNullOrWhiteSpace(line.ItemWhsCode))
                {
                    lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.ItemWhsCode)).FirstOrDefault();
                }
                else
                {
                    lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.WhsCode)).FirstOrDefault();
                }

                if (lineWhs == null)
                {
                    ShowToast("Error reading warehouse info, please try again later, Thanks.");
                    return;
                }

                #region pick serial from bin warehouse
                if (lineWhs.BinActivat.Equals("Y"))
                {
                    // show serial and bin and 
                    MessagingCenter.Subscribe(this, _PickFromSerialBin, (List<OSBQ_Ex> serialBins) =>
                    {
                        if (serialBins == null)
                        {
                            ShowToast("Pick serial from bin operation cancel");
                            return;
                        }

                        if (serialBins.Count == 0)
                        {
                            ShowToast("Pick serial from bin operation cancel");
                            return;
                        }

                        var showlist = string.Join("\n", serialBins.Select(x => $"{x.BinCode} ->  {x.DistNumber}"));
                        int lineLocId1 = itemsSource.IndexOf(line);
                        if (lineLocId1 < 0)
                        {
                            ShowToast("There is issue to locate the item in the list, please try again later, Thanks.");
                            return;
                        }
                        itemsSource[lineLocId1].EntryQuantity = serialBins.Count;
                        itemsSource[lineLocId1].SerialInBin = serialBins;
                        itemsSource[lineLocId1].ItemWhsCode = lineWhs.WhsCode;

                        var showList = $"Frm Whs {lineWhs.WhsCode} ->\n" +
                                    String.Join("\n", serialBins.Select(x => $"{x.BinCode} -> {x.DistNumber}"));

                        itemsSource[lineLocId1].ShowList = showList;
                    });

                    await Navigation.PushAsync(new BinSerialListView(_PickFromSerialBin,
                        line.ItemCodeDisplay, line.ItemNameDisplay, lineWhs.WhsCode, (int)line.OpenQty, null));
                    return;
                }
                #endregion

                #region pick from warehouse serial
                MessagingCenter.Subscribe(this, _PickSerialListView, (List<OSRQ_Ex> picked_serials) =>
                {
                    MessagingCenter.Unsubscribe<List<OSRQ_Ex>>(this, _PickFromSerialBin);

                    if (picked_serials == null)
                    {
                        ShowToast("Pick serial from warehouse operation cancel");
                        return;
                    }

                    if (picked_serials.Count == 0)
                    {
                        ShowToast("Pick serial from warehouse operation cancel");
                        return;
                    }

                    // add into the line with serial list capture

                    int lineLocId = itemsSource.IndexOf(line);
                    if (lineLocId < 0)
                    {
                        ShowToast("There is issue to locate the item in the list, please try again later, Thanks.");
                        return;
                    }

                    var showlist = $"Frm Whs {lineWhs.WhsCode} ->\n" + String.Join("\n", picked_serials.Select(x => x.DistNumber));
                    itemsSource[lineLocId].EntryQuantity = picked_serials.Count;
                    itemsSource[lineLocId].SerialsWhs = picked_serials;
                    itemsSource[lineLocId].ItemWhsCode = lineWhs.WhsCode;
                    itemsSource[lineLocId].ShowList = showlist;
                });

                await Navigation.PushAsync(new SerialListView(_PickSerialListView, line.ItemCodeDisplay, line.ItemNameDisplay,
                    line.WhsCode, (int)line.OpenQty, null));
                #endregion
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }
        }
        #endregion
        
        #region PrepareBatch PRR
        //async void PrepareBatchReturn(ReturnCommonLine_Ex line)
        //{
        //    try
        //    {
        //        if (line.entryQuantity > 0)
        //        {
        //            ResetLine(line);
        //            return;
        //        }

        //        if (currentSourceType.Contains(_GRPO))
        //        {
        //            List<Batch> batches = await GetBatchesInDo(line.DocEntry, line.LineNum);
        //            if (batches == null)
        //            {
        //                LaunchBatchesCollection(line);
        //                return;
        //            }

        //            if (batches.Count == 0)
        //            {
        //                LaunchBatchesCollection(line);
        //                return;
        //            }

        //            HandlerBatchsItem(batches, line);
        //            return;
        //        }

        //        /// handle the doc from request
        //        LaunchBatchesCollection(line);
        //    }
        //    catch (Exception excep)
        //    {
        //        Console.Write(excep.ToString());
        //        ShowAlert(excep.Message);
        //    }
        //}

        ///// <summary>
        ///// Launch the screen to collect the batch
        ///// </summary>
        ///// <param name="Line"></param>
        //void LaunchBatchesCollection(ReturnCommonLine_Ex line)
        //{
        //    // launch to collect the batch screen
        //    MessagingCenter.Subscribe(this, _CollectReturnBatch, (List<Batch> collectReturnBatch) =>
        //    {
        //        if (collectReturnBatch == null)
        //        {
        //            ShowToast("Collect batch opr cancel.");
        //            return;
        //        }

        //        if (collectReturnBatch.Count == 0)
        //        {
        //            ShowToast("Collect batch opr cancel.");
        //            return;
        //        }

        //        HandlerBatchsItem(collectReturnBatch, line);
        //    });
        //    Navigation.PushAsync(new CollectBatchesView(_CollectReturnBatch));
        //}


        ///// <summary>
        /////  
        ///// </summary>
        ///// <param name="batches"></param>
        ///// <param name="line"></param>
        //void HandlerBatchsItem(List<Batch> batches, ReturnCommonLine_Ex line)
        //{
        //    try
        //    {
        //        // checking                
        //        if (itemsSource == null)
        //        {
        //            ShowToast("Unable to process the next step, " +
        //                "as the warehouse configuration is no load in correctly, " +
        //                "please close the app, and try again later, Thanks. ");
        //            return;
        //        }

        //        OWHS lineWhs = null;
        //        if (!string.IsNullOrWhiteSpace(line.ItemWhsCode))
        //        {
        //            lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.ItemWhsCode)).FirstOrDefault();
        //        }
        //        else
        //        {
        //            lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.WhsCode)).FirstOrDefault();
        //        }

        //        if (lineWhs == null)
        //        {
        //            ShowToast("Error reading warehouse info, please try again later, Thanks.");
        //            return;
        //        }

        //        #region batch picking from bin warehouse
        //        if (lineWhs.BinActivat.Equals("Y"))
        //        {
        //            // show serial to bin allocation 
        //            MessagingCenter.Subscribe(this, _AllocateBatchToBin, (List<zwaTransferDocDetailsBin> batchToBin) =>
        //            {
        //                MessagingCenter.Unsubscribe<List<zwaTransferDocDetailsBin>>(this, _AllocateBatchToBin);
        //                if (batchToBin == null)
        //                {
        //                    ShowToast("Allocate batch to bin operation cancel");
        //                    return;
        //                }

        //                if (batchToBin.Count == 0)
        //                {
        //                    ShowToast("Allocate batch to bin operation cancel");
        //                    return;
        //                }

        //                decimal receiptQty1 = 0;
        //                string showList = $"To Whs {lineWhs.WhsCode} ->\n";

        //                batchToBin.ForEach(bb =>
        //                   bb.Bins
        //                       .ForEach(bin =>
        //                       {
        //                           receiptQty1 += bin.BatchTransQty;
        //                           showList += $"{bin.BatchTransQty:N} -> {bin.BatchNumber} -> {bin.oBIN.BinCode}\n";
        //                       }));

        //                int lineLocId1 = itemsSource.IndexOf(line);
        //                if (lineLocId1 < 0)
        //                {
        //                    ShowToast("There is issue to locate the item in the list, please try again later, Thanks.");
        //                    return;
        //                }

        //                itemsSource[lineLocId1].EntryQuantity = receiptQty1;
        //                itemsSource[lineLocId1].BatchWithBins = batchToBin;
        //                itemsSource[lineLocId1].ItemWhsCode = lineWhs.WhsCode;
        //                itemsSource[lineLocId1].ShowList = showList;
        //            });

        //            // launch the serial to bin arragement
        //            Navigation.PushAsync(
        //                new BatchToBinView(batches, lineWhs.WhsCode, _AllocateBatchToBin, line.ItemCodeDisplay, line.ItemNameDisplay));
        //            return;
        //        }
        //        #endregion

        //        #region batch assign to line warehoue
        //        MessagingCenter.Subscribe(this, _BatchQtyWhsAllocation, (List<zwaTransferDocDetailsBin> batchWhsAllocation) =>
        //        {
        //            MessagingCenter.Unsubscribe<List<zwaTransferDocDetailsBin>>(this, _BatchQtyWhsAllocation);
        //            if (batchWhsAllocation == null)
        //            {
        //                ShowToast("Batch qty allocation to warehouse operation cancel");
        //                return;
        //            }

        //            if (batchWhsAllocation.Count == 0)
        //            {
        //                ShowToast("Batch qty allocation to warehouse operation cancel");
        //                return;
        //            }

        //            var batch = new List<Batch>();
        //            batchWhsAllocation.ForEach(x => batch.Add(new Batch
        //            {
        //                DistNumber = x.Batch,
        //                Qty = x.Qty
        //            }));

        //            int lineLocId2 = itemsSource.IndexOf(line);
        //            if (lineLocId2 < 0)
        //            {
        //                ShowToast("There is issue to locate the item in the list, please try again later, Thanks.");
        //                return;
        //            }
        //            itemsSource[lineLocId2].EntryQuantity = batch.Sum(x => x.Qty); ;
        //            itemsSource[lineLocId2].Batches = batch;
        //            itemsSource[lineLocId2].ItemWhsCode = lineWhs.WhsCode;
        //            itemsSource[lineLocId2].ShowList =
        //                    $"To Whs {lineWhs.WhsCode} ->\n" +
        //                    string.Join("\n", batch.Select(x => $"{x.DistNumber} -> {x.Qty:N}").ToList());
        //        });

        //        Navigation.PushAsync(new BatchesConfirmView(lineWhs.WhsCode,
        //            _BatchQtyWhsAllocation, batches, line.ItemCodeDisplay, line.ItemNameDisplay));
        //        #endregion
        //    }
        //    catch (Exception excep)
        //    {
        //        Console.WriteLine(excep.ToString());
        //        ShowAlert(excep.Message);
        //    }
        //}

        #endregion

        #region PrepareNonManage return
        //void PrepareNonManageReturn(ReturnCommonLine_Ex line)
        //{
        //    try
        //    {
        //        OWHS lineWhs;
        //        if (!string.IsNullOrWhiteSpace(line.ItemWhsCode))
        //        {
        //            lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.ItemWhsCode)).FirstOrDefault();
        //        }
        //        else
        //        {
        //            lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.WhsCode)).FirstOrDefault();
        //        }

        //        if (lineWhs.BinActivat.Equals("Y"))
        //        {
        //            HandlerNonManageItemBin(line, lineWhs); // handler bin warehouse
        //            return;
        //        }

        //        // handle normal qty capture
        //        CaptureItemExitQty(line, lineWhs);
        //    }
        //    catch (Exception excep)
        //    {
        //        Console.WriteLine(excep.ToString());
        //        ShowAlert(excep.Message);
        //    }
        //}

        ///// <summary>
        ///// Handler non manage item
        ///// </summary>
        //async void HandlerNonManageItemBin(ReturnCommonLine_Ex line, OWHS targetWhs) // Handler bin warehouse
        //{
        //    // checking

        //    if (itemsSource == null)
        //    {
        //        ShowToast("Unable to process the next step, " +
        //            "as the warehouse configuration is no load in correctly, please close the app, and try again later, Thanks. ");
        //        return;
        //    }

        //    //reset the qty
        //    if (line.EntryQuantity > 0)
        //    {
        //        ResetLine(line);
        //        return;
        //    }

        //    //string inputQty = await new Dialog().DisplayPromptAsync(
        //    //    $"Input item code {line.ItemCodeDisplay}", "receipt qty", "Ok", "cancel", null, -1, Keyboard.Numeric, "");

        //    //if (string.IsNullOrWhiteSpace(inputQty))
        //    //{
        //    //    return;
        //    //}

        //    //if (inputQty.ToLower().Equals("cancel"))
        //    //{
        //    //    return;
        //    //}

        //    //bool IsNumeric = decimal.TryParse(inputQty, out decimal result);

        //    //if (!IsNumeric)
        //    //{
        //    //    ShowToast("Invalid receipt qty, please enter numeric and positive value, please try again later. Thanks");
        //    //    return;
        //    //}

        //    //if (result <= 0)
        //    //{
        //    //    ShowToast("Invalid receipt qty, please enter numeric and positive value, please try again later. Thanks");
        //    //    return;
        //    //}

        //    //OWHS lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.POLine.WhsCode)).FirstOrDefault();
        //    if (targetWhs.BinActivat.Equals("Y"))
        //    {
        //        // show serial to bin allocation 
        //        MessagingCenter.Subscribe(this, _AllocateItemQtyToBin, (List<OBIN_Ex> itemQtyBin) =>
        //        {
        //            MessagingCenter.Unsubscribe<List<OBIN_Ex>>(this, _AllocateItemQtyToBin);

        //            if (itemQtyBin == null)
        //            {
        //                ShowToast("Allocate item qty to bin operation cancel");
        //                return;
        //            }

        //            if (itemQtyBin.Count == 0)
        //            {
        //                ShowToast("Allocate item qty to bin operation cancel");
        //                return;
        //            }

        //            int lineLocId1 = itemsSource.IndexOf(line);
        //            if (lineLocId1 < 0)
        //            {
        //                ShowToast("There is issue to locate the item in the list, please try again later, Thanks.");
        //                return;
        //            }

        //            itemsSource[lineLocId1].EntryQuantity = itemQtyBin.Sum(x => x.BinQty);
        //            itemsSource[lineLocId1].Bins = itemQtyBin;
        //            itemsSource[lineLocId1].ItemWhsCode = targetWhs.WhsCode;
        //            itemsSource[lineLocId1].ShowList = $"To Whs {targetWhs.WhsCode} ->\n" +
        //                            String.Join("\n", itemQtyBin.Select(x => $"{x.BinQty:N} -> {x.oBIN.BinCode}")); ;
        //        });

        //        // launch the item qyt from bin arragement
        //        await Navigation.PushAsync(
        //            new ItemToBinSelectView(targetWhs.WhsCode, _AllocateItemQtyToBin,
        //            line.ItemCodeDisplay, line.ItemNameDisplay, line.OpenQty, null));
        //    }
        //}

        ///// <summary>
        ///// Prompt dialog to capture the Qty
        ///// </summary>
        ///// <param name="selectedPOLine"></param>
        //async void CaptureItemExitQty(ReturnCommonLine_Ex line, OWHS targetWhs)
        //{
        //    try
        //    {
        //        int locId = itemsSource.IndexOf(itemsSource
        //        .Where(x => x.Head.DocEntry.Equals(line.Head.DocEntry) && x.LineNum.Equals(line.LineNum)).FirstOrDefault());

        //        if (locId < 0)
        //        {
        //            ShowAlert("Unable to local the item in list, please try again. Thanks [x1]");
        //            return;
        //        }

        //        if (line.entryQuantity > 0)
        //        {
        //            ResetLine(line);
        //            return;
        //        }

        //        string exitQty = await new Dialog().DisplayPromptAsync(
        //            $"Input entry qty for {line.ItemCodeDisplay}",
        //            $"{line.ItemNameDisplay}\nOpen Qty: {line.OpenQty:N}",
        //            "OK",
        //            "Cancel",
        //            "",
        //            -1,
        //            Keyboard.Numeric,
        //            $"{line.OpenQty:N}");

        //        var isNumeric = Decimal.TryParse(exitQty, out decimal m_exitQty);
        //        if (!isNumeric) // check numeric
        //        {
        //            return;
        //        }

        //        if (m_exitQty.Equals(0)) // check zero, if zero assume as reset receipt qty
        //        {
        //            // prompt select whs             
        //            itemsSource[locId].EntryQuantity = m_exitQty; // assume as reset to the receipt
        //            return;
        //        }

        //        // else           
        //        if (m_exitQty < 0) // check negative
        //        {
        //            ShowAlert($"The receive quantity ({m_exitQty:N}) must be numeric and positve," +
        //                $" negative are no allowance.\nPlease try again later. Thanks [x2]");
        //            return;
        //        }

        //        //if (itemsSource[locId].POLine.OpenQty < m_exitQty) // check open qty
        //        //{
        //        //    ShowAlert($"The receive quantity ({m_exitQty:N}) must be equal or " +
        //        //        $"smaller than the <= Open qty ({itemsSource[locId].POLine.OpenQty:N}).\nPlease try again later. Thanks [x3]");
        //        //    return;
        //        //}

        //        // update into the list in memory    
        //        // non manage item
        //        itemsSource[locId].EntryQuantity = m_exitQty;
        //        itemsSource[locId].ItemWhsCode = targetWhs.WhsCode;
        //        itemsSource[locId].ShowList = $"To Whs {targetWhs.WhsCode} -> {m_exitQty:N}";
        //    }
        //    catch (Exception excep)
        //    {
        //        Console.WriteLine(excep.ToString());
        //        ShowAlert(excep.Message);
        //    }
        //}

        #endregion

        #region PrepareSerial Return => serial manage handle step
        /// <summary>
        ///  Prepare the serial to bin allocation screen
        /// </summary>
        //async void PrepareSerialReturn(ReturnCommonLine_Ex line)
        //{
        //    if (line.entryQuantity > 0)
        //    {
        //        ResetLine(line);
        //        return;
        //    }

        //    if (currentSourceType.Contains(_GRPO))
        //    {
        //        var serialList = await GetSerialInDo(line.DocEntry, line.LineNum);
        //        if (serialList == null)
        //        {
        //            LaunchCollectSerialView(line);
        //            return;
        //        }

        //        if (serialList.Count == 0)
        //        {
        //            LaunchCollectSerialView(line);
        //            return;
        //        }

        //        HandlerSerialItem(serialList, line);
        //        return;
        //    }

        //    // for doc from return request
        //    LaunchCollectSerialView(line);
        //}

        ///// <summary>
        ///// Luanch the serial collection screen
        ///// </summary>
        ///// <param name="line"></param>
        //void LaunchCollectSerialView(ReturnCommonLine_Ex line)
        //{
        //    // launch the screen to collect the serial number, and pass to entry the warehouse
        //    MessagingCenter.Subscribe(this, _CollectReturnSerial, (List<string> collectedSerials) =>
        //    {
        //        MessagingCenter.Unsubscribe<List<string>>(this, _CollectReturnSerial);
        //        if (collectedSerials == null)
        //        {
        //            ShowToast("Collect serial opr cancel.");
        //            return;
        //        }

        //        if (collectedSerials.Count == 0)
        //        {
        //            ShowToast("Collect serial opr cancel.");
        //            return;
        //        }

        //        HandlerSerialItem(collectedSerials, line);
        //    });

        //    Navigation.PushAsync(new CollectSerialsView(_CollectReturnSerial));
        //}

        ///// <summary>
        ///// Handler the serial Item
        ///// </summary>
        //async void HandlerSerialItem(List<string> serials, ReturnCommonLine_Ex line) // handler bin and no bin warehouse
        //{
        //    try
        //    {
        //        if (itemsSource == null)
        //        {
        //            ShowToast("Unable to process the next step, " +
        //                "as the warehouse configuration is no load in correctly, " +
        //                "please close the app, and try again later, Thanks. ");
        //            return;
        //        }

        //        #region warehouse checking
        //        OWHS lineWhs = null;
        //        if (!string.IsNullOrWhiteSpace(line.ItemWhsCode))
        //        {
        //            lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.ItemWhsCode)).FirstOrDefault();
        //        }
        //        else
        //        {
        //            lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.WhsCode)).FirstOrDefault();
        //        }

        //        if (lineWhs == null)
        //        {
        //            ShowToast("Error reading warehouse info, please try again later, Thanks.");
        //            return;
        //        }
        //        #endregion

        //        #region pick to serial base on request line whs
        //        if (!lineWhs.BinActivat.Equals("Y"))
        //        {
        //            MessagingCenter.Subscribe(this, _AllocationSerialToWhs, (List<zwaTransferDocDetailsBin> selectedReturnSerial) =>
        //            {
        //                MessagingCenter.Unsubscribe<List<zwaTransferDocDetailsBin>>(this, _AllocationSerialToWhs);
        //                if (selectedReturnSerial == null)
        //                {
        //                    ShowToast("Serial allocation to whs operation cancel.");
        //                    return;
        //                }
        //                if (selectedReturnSerial.Count == 0)
        //                {
        //                    ShowToast("Serial allocation to whs operation cancel.");
        //                    return;
        //                }

        //                var selectedSerialStr = selectedReturnSerial.Select(x => x.Serial).ToList();
        //                int lineLocId1 = itemsSource.IndexOf(line);
        //                if (lineLocId1 < 0)
        //                {
        //                    ShowToast("There is issue to locate the item in the list, please try again later, Thanks.");
        //                    return;
        //                }

        //                var showlist = $"To Whs {lineWhs.WhsCode} ->\n{string.Join("\n", selectedSerialStr)}";
        //                itemsSource[lineLocId1].EntryQuantity = selectedSerialStr.Count;
        //                itemsSource[lineLocId1].Serials = selectedSerialStr;
        //                itemsSource[lineLocId1].ItemWhsCode = lineWhs.WhsCode;
        //                itemsSource[lineLocId1].ShowList = showlist;
        //            });

        //            await Navigation.PushAsync(
        //                new SerialsConfirmView(serials, line.ItemCodeDisplay, line.ItemNameDisplay, lineWhs.WhsCode, _AllocationSerialToWhs));

        //            return;
        //        }
        //        #endregion
        //        #region pick serial from bin warehouse                
        //        MessagingCenter.Subscribe(this, _AllowcatingSerialToBin, (List<zwaTransferDocDetailsBin> serialToBin) =>
        //        {
        //            if (serialToBin == null)
        //            {
        //                ShowToast("Allocate serial to bin operation cancel");
        //                return;
        //            }

        //            if (serialToBin.Count == 0)
        //            {
        //                ShowToast("Allocate serial to bin operation cancel");
        //                return;
        //            }

        //            int lineLocId = itemsSource.IndexOf(line);
        //            if (lineLocId < 0)
        //            {
        //                ShowToast("There is issue to locate the item in the list, please try again later, Thanks.");
        //                return;
        //            }
        //            itemsSource[lineLocId].EntryQuantity = serialToBin.Count;
        //            itemsSource[lineLocId].SerialsWithBins = serialToBin;
        //            itemsSource[lineLocId].ItemWhsCode = lineWhs.WhsCode;

        //            itemsSource[lineLocId].ShowList = $"To Whs {lineWhs.WhsCode} ->\n" +
        //                        String.Join("\n", serialToBin.Select(x => $"{x.Serial} -> {x.BinCode}"));
        //        });

        //        await Navigation.PushAsync(
        //            new SerialToBinView(serials, line.ItemCodeDisplay, line.ItemNameDisplay
        //                                , lineWhs.WhsCode, _AllowcatingSerialToBin));
        //        #endregion
        //    }
        //    catch (Exception excep)
        //    {
        //        Console.WriteLine(excep.ToString());
        //        ShowAlert(excep.Message);
        //    }
        //}
        #endregion

        /// <summary>
        /// Reset the line cell
        /// </summary>
        /// <param name="line"></param>
        void ResetLine(ReturnCommonLine_Ex line)
        {
            if (line.entryQuantity > 0)
            {
                int locid = itemsSource.IndexOf(line);
                if (locid < 0) return;
                itemsSource[locid].EntryQuantity = 0;

                ShowToast($"{itemsSource[locid].Head.DocNum}#, " +
                    $"Item line {itemsSource[locid].ItemCodeDisplay}\n{itemsSource[locid].ItemNameDisplay} reset.");
                return;
            }
        }

        /// <summary>
        /// Perform save and update 
        /// to request server to create the GRPO doc request
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

                var receivedList = itemsSource.Where(x => x.EntryQuantity > 0).ToList();
                if (receivedList == null)
                {
                    ShowAlert("There is no items exit entered, please try again later. Thanks");
                    return;
                }

                if (receivedList.Count == 0)
                {
                    ShowAlert("There is no items exit entered, please try again later. Thanks");
                    return;
                }

                UserDialogs.Instance.ShowLoading("Awating Goods Return Creation ...");
                // prepre the grpo object list 
                var groupingGuid = Guid.NewGuid();
                var lineList = new List<zwaGRPO>();
                var lineBinList = new List<zwaItemBin>();

                foreach (var line in receivedList)
                {
                    var lineGuid = Guid.NewGuid();
                    lineList.Add(new zwaGRPO
                    {
                        Guid = groupingGuid,
                        ItemCode = line.ItemCode,
                        Qty = line.entryQuantity,
                        SourceCardCode = line.Head.CardCode,
                        SourceDocNum = line.Head.DocNum,
                        SourceDocEntry = line.DocEntry,
                        SourceDocBaseType = Convert.ToInt32(line.ObjType),
                        SourceBaseEntry = line.DocEntry,
                        SourceBaseLine = line.LineNum,
                        Warehouse = (string.IsNullOrWhiteSpace(line.ItemWhsCode)) ? line.BaseWarehouse : line.ItemWhsCode,
                        SourceDocType = line.Head.DocType,
                        LineGuid = lineGuid
                    });

                    List<zwaItemBin> binList = line.GetList_GR(groupingGuid, lineGuid);
                    if (binList.Count > 0)
                    {
                        lineBinList.AddRange(binList);
                    }
                }

                var headerDetails = new zmwDocHeaderField
                {
                    Guid = groupingGuid,
                    DocSeries = docSeriesSelectedItem,
                    Ref2 = ref2,
                    Comments = comments,
                    JrnlMemo = jrnlMemo,
                    NumAtCard = numberAtCard,
                    Series = GetCurrentSeries(docSeriesSelectedItem)
                };

                // 20200719T1030
                // prepare the request dto
                currentRequest = new zwaRequest
                {
                    sapUser = App.waUser,
                    request = "Create Goods Return",
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
                    request = "CreateGoodsReturn",
                    dtoRequest = currentRequest,
                    dtoGRPO = lineList.ToArray(),
                    dtoItemBins = lineBinList?.ToArray(), // 20200719T1042 add in the bin line for doc creation
                    dtozmwDocHeaderField = headerDetails
                };

                // send over server to create request
                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(requestBag, "GoodsReturn");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    ShowAlert($"{content}\n{lastErrMessage}");
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
        //    var callerAddress = "CreateGoodsReturn_DocCheck";
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
        /// serach the selected series name 
        /// </summary>
        /// <returns></returns>
        int GetCurrentSeries(string selectedSeriesName)
        {
            const int primarySeries = 9;
            if (string.IsNullOrWhiteSpace(selectedSeriesName)) return primarySeries;
            if (DocSeries == null) return primarySeries;
            if (DocSeries.Length == 0) return primarySeries;

            var series =
                DocSeries.Where(x => x.SeriesName.ToLower().Equals(selectedSeriesName.ToLower())).FirstOrDefault();

            if (series == null) return primarySeries;
            else return series.Series;
        }

        /// <summary>
        /// Reset the data source of the item in the user screen
        /// </summary>
        void ResetListView()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<ReturnCommonLine_Ex>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// Show the alert message on to the user screen
        /// </summary>
        /// <param name="message"></param>
        async void ShowAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "Ok");

        /// <summary>
        /// Show silent message on user screen
        /// </summary>
        /// <param name="message"></param>
        void ShowToast(string message) => DependencyService.Get<IToastMessage>()?.LongAlert(message);
    }
}

