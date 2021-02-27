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
using WMSApp.Class;
using WMSApp.Class.Helper;
using WMSApp.Dtos;
using WMSApp.Interface;
using WMSApp.Models.SAP;
using WMSApp.Models.Share;
using WMSApp.Views.Return;
using WMSApp.Views.Share;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace WMSApp.ViewModels.ReturnRequest
{
    public class ReturnRequestItemVM : INotifyPropertyChanged
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
        //public string SelectedReturnCardCode { get; set; }
        //public string SelectedReturnCardName { get; set; }

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
        string currentSourceType { get; set; } = string.Empty;
        NNM1[] DocSeries { get; set; } = null;

        readonly string _SelectDuplicateItemLines = "_SelectDuplicateItemLines_RReq";
        readonly string _SelectecChangeWarehouse = "_SelectecChangeWarehouse20201113T1113_RReq";
        readonly string _PickReturnLine = "_PickReturnLine20201117T2053_RReq";
        readonly string _ARINVOICE = "AR Invoice";
        readonly string _PopScreenAddr = "_PopScreenAddr_201207_RetReqItems";

        /// <summary
        /// Constructor, starting point
        /// </summary>
        public ReturnRequestItemVM(INavigation navigation, List<ReturnCommonHead_Ex> selected, string sourceType)
        {
            Navigation = navigation;
            IsRetDetailsExpand = false;
            currentSourceType = sourceType;
            itemsSourceRet = selected;
            InitCmd();
            RefreshListReturnListView();            
            if (currentSourceType.Equals(_ARINVOICE))
            {
                LoadArInvLines();
                return;
            }
            LoadDoLines();
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
                        return;
                    }

                    ShowToast("There is error to access the item, please try again later, Thanks.");
                    return;
                });

                Navigation.PushPopupAsync(
                    new PickWarehousePopUpView(_SelectecChangeWarehouse, "Pick a return warehouse", Color.Gray));
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
        async void LoadDoLines()
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
                    request = "GetOpenDoLines",
                    PoDocEntries = docEntries
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "ReturnRequest");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    ShowAlert($"{content}");
                    return;
                }

                var dtoOrrr = JsonConvert.DeserializeObject<DTO_ORRR>(content);
                if (dtoOrrr == null)
                {
                    ShowToast("Error reading information from server, please try again later. Thanks. [CN]");
                    return;
                }

                if (dtoOrrr.DLN1_Exs == null)
                {
                    ShowToast("Error reading information from server, please try again later. Thanks [SN]");
                    return;
                }

                itemsSource = new List<ReturnCommonLine_Ex>();
                var lineList = new List<DLN1_Ex>(dtoOrrr.DLN1_Exs);

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
                        Direction = "In",
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
                if (dtoOrrr.DocSeries == null)
                {
                    docSeriesItemsSource = new List<string>();
                    docSeriesItemsSource.Add("Primary");
                }
                else
                {
                    DocSeries = dtoOrrr.DocSeries;
                    docSeriesItemsSource = new List<string>();
                    dtoOrrr.DocSeries.ForEach(x => docSeriesItemsSource.Add(x.SeriesName));
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
        async void LoadArInvLines()
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
                    request = "GetArInvLines",
                    PoDocEntries = docEntries
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "ReturnRequest");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    ShowAlert($"{content}");
                    return;
                }

                var DtoOrrr = JsonConvert.DeserializeObject<DTO_ORRR>(content);
                if (DtoOrrr == null)
                {
                    ShowToast("Error reading information from server, please try again later. Thanks. [CN]");
                    return;
                }

                if (DtoOrrr.INV1_Exs == null)
                {
                    ShowToast("Error reading information from server, please try again later. Thanks [SN]");
                    return;
                }

                itemsSource = new List<ReturnCommonLine_Ex>();
                var lineList = new List<INV1_Ex>(DtoOrrr.INV1_Exs);

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
                        Head = itemsSourceRet.Where(apinv => apinv.DocEntry.Equals(x.DocEntry)).FirstOrDefault(),
                        ItemWhsCode = x.WhsCode,

                        Navigation = Navigation,
                        Direction = "In",
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
                if (DtoOrrr.DocSeries == null)
                {
                    docSeriesItemsSource = new List<string>();
                    docSeriesItemsSource.Add("Primary");
                }
                else
                {
                    DocSeries = DtoOrrr.DocSeries;
                    docSeriesItemsSource = new List<string>();
                    DtoOrrr.DocSeries.ForEach(x => docSeriesItemsSource.Add(x.SeriesName));
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
        void HandlerItemCodeSecStep(ReturnCommonLine_Ex line)
        {
            try
            {
                if (App.Warehouses == null)
                {
                    ShowAlert("Please restart the App and try again, the initial config data not load properly. Thanks");
                    return;
                }

                if (App.Warehouses.Length == 0)
                {
                    ShowAlert("Please restart the App and try again, the initial config data not load properly. Thanks");
                    return;
                }

                var locatedWhs = App.Warehouses.Where(x => x != null && x.WhsCode.Equals(line.ItemWhsCode)).FirstOrDefault();
                if (locatedWhs == null)
                {
                    ShowToast("Sorry the line warehouse can not locate in the setup. Please try again, Thansk.");
                    return;
                }

                CaptureItemEntryQty(line, locatedWhs);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }
        }

        /// <summary>
        /// Prompt dialog to capture the Qty
        /// </summary>
        /// <param name="selectedPOLine"></param>
        async void CaptureItemEntryQty(ReturnCommonLine_Ex line, OWHS targetWhs)
        {
            try
            {
                int locId = itemsSource.IndexOf(itemsSource
                .Where(x => x.Head.DocEntry.Equals(line.Head.DocEntry) && x.LineNum.Equals(line.LineNum)).FirstOrDefault());

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
                    $"Input quantity for {line.ItemCodeDisplay}",
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
                    ShowAlert($"The quantity ({m_exitQty:N}) must be numeric and positve," +
                        $" negative are no allowance.\nPlease try again later. Thanks [x2]");
                    return;
                }

                //if (itemsSource[locId].POLine.OpenQty < m_exitQty) // check open qty
                //{
                //    ShowAlert($"The receive quantity ({m_exitQty:N}) must be equal or " +
                //        $"smaller than the <= Open qty ({itemsSource[locId].POLine.OpenQty:N}).\nPlease try again later. Thanks [x3]");
                //    return;
                //}

                // update into the list in memory    
                // non manage item
                itemsSource[locId].EntryQuantity = m_exitQty;
                itemsSource[locId].ItemWhsCode = targetWhs.WhsCode;
                itemsSource[locId].ShowList = $"To Whs {targetWhs.WhsCode} -> {m_exitQty:N}";
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }
        }

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

                UserDialogs.Instance.ShowLoading("Awating Goods Return Request Creation ...");
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

                    List<zwaItemBin> binList = line.GetList(groupingGuid, lineGuid);
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
                    request = "Create Return Request",
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
                    request = "CreateReturnRequest",
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
                    content = await httpClient.RequestSvrAsync_mgt(requestBag, "ReturnRequest");
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
        //    var callerAddress = "CreateReturnReq_DocCheck";
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
            const int primarySeries = 67;
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
