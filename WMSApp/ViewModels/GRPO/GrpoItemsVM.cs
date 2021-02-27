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
using WMSApp.Models.GRPO;
using WMSApp.Models.SAP;
using WMSApp.Models.Share;
using WMSApp.Views.GRPO;
using WMSApp.Views.Share;
using WMSApp.Views.Transfer1;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace WMSApp.ViewModels.GRPO
{
    public class GrpoItemsVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Command CmdClose { get; set; }
        public Command CmdSaveUpdate { get; set; }
        public Command CmdStartScan { get; set; }
        public Command CmdSearchBarVisible { get; set; }

        string poDetails;
        public string PoDetails
        {
            get => poDetails;
            set
            {
                if (poDetails == value) return;
                poDetails = value;
                OnPropertyChanged(nameof(PoDetails));
            }
        }
        public string SelectedPoCardCode { get; set; }
        public string SelectedPoCardName { get; set; }

        OPOR_Ex selectedItemSelectedPO;
        public OPOR_Ex SelectedItemSelectedPO
        {
            get => selectedItemSelectedPO;
            set
            {
                HandleSelectedPODoc(value);
                selectedItemSelectedPO = null;
                OnPropertyChanged(nameof(SelectedItemSelectedPO));
            }
        }

        List<OPOR_Ex> itemsSourceSelectedPOs { get; set; } // purchase order
        public ObservableCollection<OPOR_Ex> ItemsSourceSelectedPOs { get; set; }

        POR1_Ex selectedItem_chgWhs;
        POR1_Ex selectedItem;
        public POR1_Ex SelectedItem
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

        List<POR1_Ex> itemsSource;
        public ObservableCollection<POR1_Ex> ItemsSource { get; set; }

        string searchItemQuery;
        public string SearchItemQuery
        {
            get => searchItemQuery;
            set
            {
                if (searchItemQuery == value) return;
                searchItemQuery = value; // perform item search
                FilterItemSearch(searchItemQuery);
            }
        }

        string selectedMajorWhs;
        public string SelectedMajorWhs
        {
            get => $"Entrance to warehouse:\n{selectedMajorWhs}";
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

        bool isPODetailsExpand;
        public bool IsPODetailsExpand
        {
            get => isPODetailsExpand;
            set
            {
                if (isPODetailsExpand == value) return;
                isPODetailsExpand = value;
                OnPropertyChanged(nameof(IsPODetailsExpand));
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

        NNM1[] GrpoSeries; // kept orginal from server

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

        /// <summary>
        /// Inner declaration
        /// </summary>        
        INavigation Navigation { get; set; } = null;
        zwaRequest currentRequest { get; set; } = null;
        OITM currentItem { get; set; } = null;

        readonly string _SelectDuplicateItemLines = "_SelectDuplicateItemLines";
        readonly string _CaptureSerials = "_CaptureSerials";
        readonly string _SerialsToBin = "_SerialsToBin_20201109";
        readonly string _ItemQtyToBin = "_ItemQtyToBin_20201109";
        readonly string _CaptureBatches = "_CaptureBatches";
        readonly string _BatchesToBin = "_BatchesToBin_20201109";
        readonly string _SelectecChangeWarehouse = "_SelectecChangeWarehouse20201113T1113";
        readonly string _ShowPORLines = "_ShowPORLines20201123T1637";
        readonly string _PopScreenAddr = "_PopScreenAddr_201207";

        /// <summary
        /// Constructor, starting point
        /// </summary>
        public GrpoItemsVM(INavigation navigation, List<OPOR_Ex> selected)
        {
            Navigation = navigation;
            itemsSourceSelectedPOs = selected;
            IsPODetailsExpand = false;

            RefreshListPoListView();
            LoadPoLines();
            InitCmd();
        }

        /// <summary>
        /// Handle the wahrehouse change
        /// </summary>
        /// <param name="line"></param>
        public void HandlerLineWhsChanged(POR1_Ex line)
        {
            try
            {
                selectedItem_chgWhs = line;
                if (line.ReceiptQuantity > 0)
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
                        //Thread.Sleep(300);
                        //HandlerItemCode(itemsSource[locid].ItemCodeDisplay);                    
                    }
                    else
                    {
                        ShowToast("There is error to access the item, please try again later, Thanks.");
                    }
                    return;
                });

                Navigation.PushPopupAsync(
                    new PickWarehousePopUpView(_SelectecChangeWarehouse, "Pick a entry warehouse", Color.Gray));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }
        }

        /// <summary>
        /// show list of the po lines
        /// </summary>
        /// <param name="selectedDoc"></param>
        void HandleSelectedPODoc(OPOR_Ex selectedDoc)
        {
            if (itemsSource == null) return;
            if (itemsSource.Count == 0) return;

            var poLines = itemsSource.Where(x => x.POLine.DocEntry.Equals(selectedDoc.PO.DocEntry)).ToList();
            if (poLines == null) return;
            if (poLines.Count == 0) return;

            MessagingCenter.Subscribe<POR1_Ex>(this, _ShowPORLines, (POR1_Ex selectedLine) =>
            {
                MessagingCenter.Unsubscribe<POR1_Ex>(this, _ShowPORLines);
                if (selectedLine == null) return;

                HandlerItemCodeSecStep(selectedLine);
                return;
            });

            Navigation.PushPopupAsync(new GrpoPOLinePopUpView(poLines, selectedDoc, _ShowPORLines));
        }

        /// <summary>
        /// Formating the doc po lines number
        /// </summary>
        void GetDocCounts()
        {
            if (itemsSourceSelectedPOs == null)
            {
                PoDetails = $"Purchase order details";
                return;
            }
            if (itemsSourceSelectedPOs.Count == 0)
            {
                PoDetails = $"Purchase order details";
                return;
            }

            if (itemsSource == null) return;
            if (itemsSource.Count == 0) return;

            PoDetails = $"PO details, {itemsSourceSelectedPOs?.Count} selected, {itemsSource.Count} item(s)";
        }

        /// <summary>
        /// refresh the listview of PO 
        /// </summary>
        void RefreshListPoListView()
        {
            if (itemsSourceSelectedPOs == null) return;
            if (itemsSourceSelectedPOs.Count == 0) return;
            ItemsSourceSelectedPOs = new ObservableCollection<OPOR_Ex>(itemsSourceSelectedPOs);
            OnPropertyChanged(nameof(ItemsSourceSelectedPOs));
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
                    IsPODetailsExpand = false;
                    searchBar.Focus();
                    return;
                }
            });
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
                var filter = itemsSource.Where(x => x.POLine.ItemCode.ToLower().Contains(lowerCaseQuery) ||
                                                x.POLine.Dscription.ToLower().Contains(lowerCaseQuery)).ToList();

                if (filter == null)
                {
                    ResetListView();
                    return;
                }

                ItemsSource = new ObservableCollection<POR1_Ex>(filter);
                OnPropertyChanged(nameof(ItemsSource));
            }
            catch (Exception excep)
            {
                ShowAlert($"{excep}");
            }
        }

        /// <summary>
        /// Connect to server to load selected PO lines
        /// </summary>
        async void LoadPoLines()
        {
            try
            {
                UserDialogs.Instance.ShowLoading("A moment please...");

                if (itemsSourceSelectedPOs == null) return;
                if (itemsSourceSelectedPOs.Count == 0) return;

                var docEntries = itemsSourceSelectedPOs.Where(x => x.IsChecked = true).Select(p => p.PO.DocEntry).ToArray();

                if (docEntries == null) return;
                if (docEntries.Length == 0) return;

                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "GetOpenPoLines",
                    PoDocEntries = docEntries
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Grpo");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    ShowAlert($"{content}");
                    return;
                }

                var dtpGrpo = JsonConvert.DeserializeObject<DtoGrpo>(content);
                if (dtpGrpo.POR1s == null) return;
                if (dtpGrpo.POR1s.Length == 0) return;

                itemsSource = new List<POR1_Ex>();
                dtpGrpo.POR1s.ForEach(x => itemsSource.Add(
                    new POR1_Ex
                    {
                        PO = itemsSourceSelectedPOs.Where(po => po.PO.DocEntry.Equals(x.DocEntry)).FirstOrDefault(),
                        POLine = x,
                        ItemWhsCode = x.WhsCode,
                        BaseWarehouse = x.WhsCode,
                        Navigation = Navigation,
                        Direction = "in"
                    }));

                // update the po line count 
                itemsSourceSelectedPOs.ForEach(x =>
                {
                    var lineCount = itemsSource.Count(l => l.POLine.DocEntry.Equals(x.PO.DocEntry));
                    x.LineCount = lineCount;
                });

                ResetListView();
                GetDocCounts();

                // load the doc series 
                if (dtpGrpo.GrpoDocSeries == null)
                {
                    docSeriesItemsSource = new List<string>();
                    docSeriesItemsSource.Add("Primary");
                }
                else
                {
                    GrpoSeries = dtpGrpo.GrpoDocSeries;
                    docSeriesItemsSource = new List<string>();
                    GrpoSeries.ForEach(x => docSeriesItemsSource.Add(x.SeriesName));
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
                var foundPOItems = itemsSource.Where(x => x.POLine.ItemCode.ToLower().Equals(itemCode.ToLower())).ToList();

                // locate the correct item to do receipt
                if (foundPOItems == null)
                {
                    ShowAlert($"Sorry the item code {itemCode} no found in system setup. Please try other code, Thanks [L]");
                    return;
                }

                if (foundPOItems.Count == 0)
                {
                    ShowAlert($"Sorry the item code {itemCode} no found in system setup. Please try other code, Thanks [LZ]");
                    return;
                }

                if (foundPOItems.Count == 1)
                {
                    HandlerItemCodeSecStep(foundPOItems[0]);
                    return;
                }

                // there is more than 1 same item scan
                MessagingCenter.Subscribe<POR1_Ex>(this, _SelectDuplicateItemLines, (POR1_Ex selected) =>
                {
                    MessagingCenter.Unsubscribe<POR1_Ex>(this, _SelectDuplicateItemLines);
                    if (selected == null) return;
                    HandlerItemCodeSecStep(selected);
                    return;
                });

                await Navigation.PushPopupAsync(new GrpoPoLinesPopUpSelectView(foundPOItems, _SelectDuplicateItemLines));
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
        async void HandlerItemCodeSecStep(POR1_Ex line)
        {
            currentItem = await GetItem(line.POLine.ItemCode);
            if (currentItem == null)
            {
                ShowToast($"There is issue reading the {line.POLine.ItemCode} info from server. Please try again later, Thanks.");
                return;
            }

            if (currentItem.ManSerNum.Equals("Y"))
            {
                PrepareSerialGRN(line);
                return;
            }

            if (currentItem.ManBtchNum.Equals("Y"))
            {
                PrepareBatchGRN(line);
                return;
            }

            // ELSE handle no manage item
            PrepareNonManageGRN(line);
        }

        #region PrepareBatchGRN 
        void PrepareBatchGRN(POR1_Ex line)
        {
            try
            {
                if (line.ReceiptQuantity > 0)
                {
                    ResetLine(line);
                    return;
                }

                MessagingCenter.Subscribe(this, _CaptureBatches, (List<Batch> batches) =>
                {
                    MessagingCenter.Unsubscribe<List<Batch>>(this, _CaptureBatches);
                    if (batches == null)
                    {
                        ShowToast("Captured batches list is empty, please try again later, Thanks[n]");
                        return;
                    }

                    if (batches.Count == 0)
                    {
                        ShowToast("Captured batches list is empty, please try again later, Thanks. [z]");
                        return;
                    }

                    HandlerBatchsItem(batches, line);
                    return;
                });
                Navigation.PushAsync(new CollectBatchesView(_CaptureBatches));
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
        void HandlerBatchsItem(List<Batch> batches, POR1_Ex line)
        {
            try
            {
                if (line.ReceiptQuantity > 0)
                {
                    int locid = itemsSource.IndexOf(line);
                    if (locid < 0) return;
                    itemsSource[locid].ReceiptQuantity = 0;
                    return;
                }

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

                OWHS lineWhs;
                if (!string.IsNullOrWhiteSpace(line.ItemWhsCode))
                {
                    lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.ItemWhsCode)).FirstOrDefault();
                }
                else
                {
                    lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.POLine.WhsCode)).FirstOrDefault();
                }

                if (lineWhs.BinActivat.Equals("Y"))
                {
                    // show serial to bin allocation 
                    MessagingCenter.Subscribe(this, _BatchesToBin, (List<zwaTransferDocDetailsBin> batchBin) =>
                    {
                        MessagingCenter.Unsubscribe<zwaTransferDocDetailsBin>(this, _BatchesToBin);

                        if (batchBin == null) return;
                        if (batchBin.Count == 0) return;

                        int lineLocId1 = itemsSource.IndexOf(line);
                        if (lineLocId1 < 0)
                        {
                            ShowToast("There is issue to locate the item in the list, please try again later, Thanks.");
                            return;
                        }

                        string showList = $"{lineWhs.WhsCode} ->\n";
                        decimal receiptQty1 = 0;
                        batchBin.ForEach(bb =>
                            bb.Bins
                                .ForEach(bin =>
                                {
                                    receiptQty1 += bin.BatchTransQty;
                                    showList += $"{bin.BatchTransQty:N}->{bin.BatchNumber}->{bin.oBIN.BinCode}\n";
                                }));

                        int lineLocId2 = itemsSource.IndexOf(line);
                        if (lineLocId2 < 0)
                        {
                            ShowToast("There is issue to locate the item in the list, please try again later, Thanks.");
                            return;
                        }

                        itemsSource[lineLocId2].ReceiptQuantity = batchBin.Sum(x => x.ReceiptQty);
                        itemsSource[lineLocId2].BatchWithBins = batchBin;
                        itemsSource[lineLocId2].ReceiptQuantity = receiptQty1;
                        itemsSource[lineLocId2].ShowList = showList;
                    });

                    // launch the serial to bin arragement
                    Navigation.PushAsync(new BatchToBinView(batches, lineWhs.WhsCode, _BatchesToBin,
                        line.ItemCodeDisplay, line.ItemNameDisplay));
                    return;
                }

                // add into the line with serial list capture
                // no to bin warehouse operation
                decimal receiptQty = batches.Sum(x => x.Qty);
                int lineLocId = itemsSource.IndexOf(line);
                if (lineLocId < 0)
                {
                    ShowToast("There is issue to locate the item in the list, please try again later, Thanks.");
                    return;
                }

                itemsSource[lineLocId].ReceiptQuantity = receiptQty;
                itemsSource[lineLocId].Batches = batches;
                itemsSource[lineLocId].ShowList = $"{lineWhs.WhsCode} ->\n" +
                                                  string.Join("\n", batches.Select(x => $"{x.Qty:N}->{x.DistNumber}").ToList());
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }
        }

        #endregion

        #region PrepareNonManageGRN
        void PrepareNonManageGRN(POR1_Ex line)
        {
            OWHS lineWhs;
            if (!string.IsNullOrWhiteSpace(line.ItemWhsCode))
            {
                lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.ItemWhsCode)).FirstOrDefault();
            }
            else
            {
                lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.POLine.WhsCode)).FirstOrDefault();
            }

            if (lineWhs.BinActivat.Equals("Y"))
            {
                HandlerNonManageItemBin(line, lineWhs); // handler bin warehouse
                return;
            }
            // handle normal qty capture
            CaptureItemReceiptQty(line, lineWhs);
        }

        /// <summary>
        /// Handler non manage item
        /// </summary>
        async void HandlerNonManageItemBin(POR1_Ex line, OWHS targetWhs) // Handler bin warehouse
        {
            // checking
            try
            {
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
                if (line.ReceiptQuantity > 0)
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
                    MessagingCenter.Subscribe(this, _ItemQtyToBin, (List<OBIN_Ex> qtyBin) =>
                    {
                        if (qtyBin == null) return;
                        if (qtyBin.Count == 0) return;

                        int lineLocId1 = itemsSource.IndexOf(line);
                        if (lineLocId1 < 0)
                        {
                            ShowToast("There is issue to locate the item in the list, please try again later, Thanks.");
                            return;
                        }

                        itemsSource[lineLocId1].ReceiptQuantity = qtyBin.Sum(x => x.BinQty);
                        itemsSource[lineLocId1].Bins = qtyBin;

                        var showList = $"{targetWhs.WhsCode} ->\n" +
                                        String.Join("\n", qtyBin.Select(x => $"{x.BinQty:N} -> {x.oBIN.BinCode}"));

                        itemsSource[lineLocId1].ShowList = showList;


                        MessagingCenter.Unsubscribe<List<OBIN_Ex>>(this, _ItemQtyToBin);
                    });

                    // launch the serial to bin arragement
                    await Navigation.PushAsync(new ItemToBinSelectView(
                        targetWhs.WhsCode, _ItemQtyToBin, line.ItemCodeDisplay, line.ItemNameDisplay, line.POLine.OpenQty, null));
                }
            }
            catch (Exception e)
            {
                ShowAlert($"{e.Message}\n{e.StackTrace}");
                Console.WriteLine($"{e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// Prompt dialog to capture the Qty
        /// </summary>
        /// <param name="selectedPOLine"></param>
        async void CaptureItemReceiptQty(POR1_Ex line, OWHS targetWhs)
        {
            int locId = itemsSource.IndexOf(itemsSource
                .Where(
                x => x.PO.PO.DocEntry.Equals(line.PO.PO.DocEntry) &&
                x.POLine.LineNum.Equals(line.POLine.LineNum)).FirstOrDefault());

            if (locId < 0)
            {
                ShowAlert("Unable to local the item in list, please try again. Thanks [x1]");
                return;
            }

            if (line.ReceiptQuantity > 0)
            {
                ResetLine(line);
                return;
            }

            string receiptQty = await new Dialog().DisplayPromptAsync(
                $"Please input {line.ItemCodeDisplay}",
                $"{line.ItemNameDisplay}\nOpen Qty: {line.POLine.OpenQty:N}",
                "OK",
                "Cancel",
                "",
                -1,
                Keyboard.Numeric,
                $"{line.POLine.OpenQty:N}");

            var isNumeric = Decimal.TryParse(receiptQty, out decimal m_receiptQty);
            if (!isNumeric) // check numeric
            {
                return;
            }

            if (m_receiptQty.Equals(0)) // check zero, if zero assume as reset receipt qty
            {
                // prompt select whs             
                itemsSource[locId].ReceiptQuantity = m_receiptQty; // assume as reset to the receipt
                return;
            }

            // else           
            if (m_receiptQty < 0) // check negative
            {
                ShowAlert($"The receive quantity ({m_receiptQty:N}) must be numeric and positve, negative are no allowance.\nPlease try again later. Thanks [x2]");
                return;
            }

            //if (itemsSource[locId].POLine.OpenQty < m_receiptQty) // check open qty
            //{
            //    ShowAlert($"The receive quantity ({m_receiptQty:N}) must be equal or " +
            //        $"smaller than the <= Open qty ({itemsSource[locId].POLine.OpenQty:N}).\nPlease try again later. Thanks [x3]");
            //    return;
            //}

            // update into the list in memory    
            // non manage item
            itemsSource[locId].ReceiptQuantity = m_receiptQty;
            itemsSource[locId].ShowList = $"{m_receiptQty:N} -> {targetWhs.WhsCode}";
        }

        #endregion

        #region PrepareSerialGRN => serial manage handle step
        /// <summary>
        ///  Prepare the serial to bin allocation screen
        /// </summary>
        void PrepareSerialGRN(POR1_Ex line)
        {
            if (line.ReceiptQuantity > 0)
            {
                ResetLine(line);
                return;
            }

            MessagingCenter.Subscribe(this, _CaptureSerials, (List<string> serials) =>
            {
                MessagingCenter.Unsubscribe<List<string>>(this, _CaptureSerials);
                if (serials == null)
                {
                    ShowToast("Captured serials list is empty, please try again later, Thanks[n]");
                    return;
                }

                if (serials.Count == 0)
                {
                    ShowToast("Captured serials list is empty, please try again later, Thanks. [z]");
                    return;
                }

                HandlerSerialItem(serials, line);
                return;
            });
            Navigation.PushAsync(new CollectSerialsView(_CaptureSerials));
        }

        /// <summary>
        /// Handler the serial Item
        /// </summary>
        void HandlerSerialItem(List<string> serials, POR1_Ex line) // handler bin and no bin warehouse
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


                OWHS lineWhs;
                if (!string.IsNullOrWhiteSpace(line.ItemWhsCode))
                {
                    lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.ItemWhsCode)).FirstOrDefault();
                }
                else
                {
                    lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.POLine.WhsCode)).FirstOrDefault();
                }

                if (lineWhs.BinActivat.Equals("Y"))
                {
                    // show serial to bin allocation 
                    MessagingCenter.Subscribe(this, _SerialsToBin, (List<zwaTransferDocDetailsBin> serialBin) =>
                    {
                        if (serialBin == null) return;
                        if (serialBin.Count == 0) return;

                        int lineLocId1 = itemsSource.IndexOf(line);
                        if (lineLocId1 < 0)
                        {
                            ShowToast("There is issue to locate the item in the list, please try again later, Thanks.");
                            return;
                        }
                        itemsSource[lineLocId1].ReceiptQuantity = serialBin.Count;
                        itemsSource[lineLocId1].SerialsWithBins = serialBin;

                        var showList = $"{lineWhs.WhsCode} ->\n" +
                                    String.Join("\n", serialBin.Select(x => x.Serial + "->" + x.BinCode));

                        itemsSource[lineLocId1].ShowList = showList;
                    });

                    // launch the serial to bin arragement
                    Navigation.PushAsync(new SerialToBinView(serials,
                        line.ItemCodeDisplay, line.ItemNameDisplay, lineWhs.WhsCode, _SerialsToBin));

                    return;
                }

                // add into the line with serial list capture
                int receiptQty = serials.Count;
                int lineLocId = itemsSource.IndexOf(line);
                if (lineLocId < 0)
                {
                    ShowToast("There is issue to locate the item in the list, please try again later, Thanks.");
                    return;
                }

                itemsSource[lineLocId].ReceiptQuantity = receiptQty;
                itemsSource[lineLocId].Serials = serials;
                itemsSource[lineLocId].ShowList = $"{lineWhs.WhsCode} ->\n" + String.Join("\n", serials);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }
        }
        #endregion

        /// <summary>
        /// Reset the line cell
        /// </summary>
        /// <param name="line"></param>
        void ResetLine(POR1_Ex line)
        {
            if (line.ReceiptQuantity > 0)
            {
                int locid = itemsSource.IndexOf(line);
                if (locid < 0) return;
                itemsSource[locid].ReceiptQuantity = 0;
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

                var receivedList = itemsSource.Where(x => x.ReceiptQuantity > 0).ToList();
                if (receivedList == null)
                {
                    ShowAlert("There is no items receipt entered, please try again later. Thanks");
                    return;
                }

                if (receivedList.Count == 0)
                {
                    ShowAlert("There is no items receipt entered, please try again later. Thanks");
                    return;
                }

                UserDialogs.Instance.ShowLoading("Awating GRPO Creation ...");

                // prepre the grpo object list 
                var groupingGuid = Guid.NewGuid();
                var lineList = new List<zwaGRPO>();
                var lineBinList = new List<zwaItemBin>();

                foreach (var line in receivedList)
                {
                    Guid lineGuid = Guid.NewGuid();
                    lineList.Add(new zwaGRPO
                    {
                        Guid = groupingGuid,
                        ItemCode = line.POLine.ItemCode,
                        Qty = line.ReceiptQuantity,
                        SourceCardCode = line.PO.PO.CardCode,
                        SourceDocNum = line.PO.PO.DocNum,
                        SourceDocEntry = line.POLine.DocEntry,
                        SourceDocBaseType = Convert.ToInt32(line.POLine.ObjType), // from purchase order
                        SourceBaseEntry = line.POLine.DocEntry,
                        SourceBaseLine = line.POLine.LineNum,
                        Warehouse = line.ItemWhsCode,
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
                    request = "Create GRPO",
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
                    request = "CreateGRPORequest",
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
                    content = await httpClient.RequestSvrAsync_mgt(requestBag, "Grpo");
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
        //    var callerAddress = "CreateGRPO_DocCheck";
        //    MessagingCenter.Subscribe(this, callerAddress, (string result) =>
        //        {
        //            MessagingCenter.Unsubscribe<string>(this, callerAddress);
        //            UserDialogs.Instance.HideLoading();

        //            if (result.Equals("fail")) return;
        //            if (result.Equals("success")) Navigation.PopAsync();

        //            App.RequestList.Remove(currentRequest);                 
        //        });

        //    var checker = new RequestCheckHelper { Requests = currentRequest, ReturnAddress = callerAddress };
        //    checker.StartChecking();
        //}

        /// <summary>
        /// serach the selected series name 
        /// </summary>
        /// <returns></returns>
        int GetCurrentSeries(string selectedSeriesName)
        {
            const int grpoDefaultSeries = 8;
            if (string.IsNullOrWhiteSpace(selectedSeriesName)) return grpoDefaultSeries;
            if (GrpoSeries == null) return grpoDefaultSeries;
            if (GrpoSeries.Length == 0) return grpoDefaultSeries;

            var series =
                GrpoSeries.Where(x => x.SeriesName.ToLower().Equals(selectedSeriesName.ToLower())).FirstOrDefault();

            if (series == null) return grpoDefaultSeries;
            else return series.Series;
        }

        /// <summary>
        /// Reset the data source of the item in the user screen
        /// </summary>
        void ResetListView()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<POR1_Ex>(itemsSource);
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