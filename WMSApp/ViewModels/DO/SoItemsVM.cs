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
using WMSApp.Dtos;
using WMSApp.Interface;
using WMSApp.Models.GRPO;
using WMSApp.Models.SAP;
using WMSApp.Models.Share;
using WMSApp.Models.Transfer1;
using WMSApp.ViewModels.Transfer1;
using WMSApp.Views.DO;
using WMSApp.Views.Share;
using WMSApp.Views.Transfer1;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace WMSApp.ViewModels.SalesOrder
{
    public class SoItemsVM : INotifyPropertyChanged
    {
        #region View property
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Command CmdClose { get; set; }
        public Command CmdSaveUpdate { get; set; }
        public Command CmdStartScan { get; set; }
        public Command CmdSearchBarVisible { get; set; }

        string soDetails;
        public string SoDetails
        {
            get => soDetails;
            set
            {
                if (soDetails == value) return;
                soDetails = value;
                OnPropertyChanged(nameof(SoDetails));
            }
        }
        public string SelectedSoCardCode { get; set; }
        public string SelectedSoCardName { get; set; }

        ORDR_Ex selectedItemSelectedSO;
        public ORDR_Ex SelectedItemSelectedSO
        {
            get => selectedItemSelectedSO;
            set
            {
                HandleSelectedSODoc(value);
                selectedItemSelectedSO = null;
                OnPropertyChanged(nameof(SelectedItemSelectedSO));
            }
        }

        List<ORDR_Ex> itemsSourceSelectedSOs { get; set; } // purchase order
        public ObservableCollection<ORDR_Ex> ItemsSourceSelectedSOs { get; set; }

        RDR1_Ex selectedItemTemp { get; set; }
        RDR1_Ex selectedItem_chgWhs { get; set; }
        RDR1_Ex selectedItem { get; set; }
        public RDR1_Ex SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;

                // handler the selected item
                // prompt qty entry and update the item
                //HandlerItemCode(selectedItem.ItemCode);
                HandlerItemCodeSecStep(selectedItem);

                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        List<RDR1_Ex> itemsSource;
        public ObservableCollection<RDR1_Ex> ItemsSource { get; set; }

        public string SearchItemQuery
        {
            set => FilterItemSearch(value);
        }

        string selectedMajorWhs;
        public string SelectedMajorWhs
        {
            get => $"Exit from warehouse:\n{selectedMajorWhs}";
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

        bool isSODetailsExpand;
        public bool IsSODetailsExpand
        {
            get => isSODetailsExpand;
            set
            {
                if (isSODetailsExpand == value) return;
                isSODetailsExpand = value;
                OnPropertyChanged(nameof(IsSODetailsExpand));
            }
        }

        NNM1 [] doSeries;
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

        readonly string _SelectDuplicateItemLines = "_SelectDuplicateItemLines_SO";
        readonly string _SelectecChangeWarehouse = "_SelectecChangeWarehouse20201113T1113_SO";
        readonly string _PickFromSerialBin = "_PickFromSerialBin_20201116T2135";
        readonly string _PickSerialListView = "_PickSerialListView_20201116T0952";
        readonly string _PickBatchesFromBin = "_PickBatchesFromBin_20201116T2225";
        readonly string _PickBatchFromWhs = "_PickBatchFromWhs_20201116T2234";
        readonly string _PickItemQtyFromBin = "_PickItemQtyFromBin_20201116T2258";
        readonly string _PickSOLine = "_PickSOLine20201117T2053";
        readonly string _PopScreenAddr = "_PopScreenAddr_SOITEM";

        /// <summary
        /// Constructor, starting point
        /// </summary>
        public SoItemsVM(INavigation navigation, List<ORDR_Ex> selected)
        {
            Navigation = navigation;
            itemsSourceSelectedSOs = selected;
            IsSODetailsExpand = false;

            RefreshListSoListView();
            LoadSoLines();
            InitCmd();
        }

        /// <summary>
        /// show list of the po lines
        /// </summary>
        /// <param name="selectedDoc"></param>
        void HandleSelectedSODoc(ORDR_Ex selectedDoc)
        {
            try
            {
                if (itemsSource == null) return;
                if (itemsSource.Count == 0) return;

                var soLines = itemsSource.Where(x => x.DocEntry.Equals(selectedDoc.DocEntry)).ToList();
                if (soLines == null) return;
                if (soLines.Count == 0) return;

                MessagingCenter.Subscribe(this, _PickSOLine, (RDR1_Ex selectedLine) =>
                {
                    MessagingCenter.Unsubscribe<RDR1_Ex>(this, _PickSOLine);
                    if (selectedLine == null) return;
                    if (selectedLine.SO == null) return;
                    if (selectedLine == null) return;

                    HandlerItemCodeSecStep(selectedLine);
                    return;
                });

                Navigation.PushPopupAsync(new DoSOLinePopUpView(soLines, selectedDoc, _PickSOLine));

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
            if (itemsSourceSelectedSOs == null)
            {
                SoDetails = $"Sales order details";
                return;
            }
            if (itemsSourceSelectedSOs.Count == 0)
            {
                SoDetails = $"Sales order details";
                return;
            }

            if (itemsSource == null) return;
            if (itemsSource.Count == 0) return;

            SoDetails = $"SO details, {itemsSourceSelectedSOs?.Count} selected, {itemsSource.Count} item(s)";
        }

        /// <summary>
        /// refresh the listview of PO 
        /// </summary>
        void RefreshListSoListView()
        {
            if (itemsSourceSelectedSOs == null) return;
            if (itemsSourceSelectedSOs.Count == 0) return;
            ItemsSourceSelectedSOs = new ObservableCollection<ORDR_Ex>(itemsSourceSelectedSOs);
            OnPropertyChanged(nameof(ItemsSourceSelectedSOs));
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
                    IsSODetailsExpand = false;
                    searchBar.Focus();
                    return;
                }
            });
        }

        /// <summary>
        /// Handle the wahrehouse change
        /// </summary>
        /// <param name="line"></param>
        public void HandlerLineWhsChanged(RDR1_Ex line)
        {
            try
            {
                selectedItem_chgWhs = line;
                if (line.ExitQuantity > 0)
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
                       // HandlerItemCode(itemsSource[locid].ItemCodeDisplay);
                    }
                    else
                    {
                        ShowToast("There is error to access the item, please try again later, Thanks.");
                    }
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

                ItemsSource = new ObservableCollection<RDR1_Ex>(filter);
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
        async void LoadSoLines()
        {
            try
            {
                UserDialogs.Instance.ShowLoading("A moment please...");

                if (itemsSourceSelectedSOs == null) return;
                if (itemsSourceSelectedSOs.Count == 0) return;

                var docEntries = itemsSourceSelectedSOs.Where(x => x.IsChecked = true).Select(p => p.DocEntry).ToArray();

                if (docEntries == null) return;
                if (docEntries.Length == 0) return;

                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "GetOpenSoLines",
                    PoDocEntries = docEntries
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Do");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    ShowAlert($"{content}");
                    return;
                }

                var dtoODRD = JsonConvert.DeserializeObject<DTO_ORDR>(content);

                if (dtoODRD == null)
                {
                    ShowToast("Error reading information from server, please try again later. Thanks. [CN]");
                    return;
                }

                if (dtoODRD.RDR1_Exs == null)
                {
                    ShowToast("Error reading information from server, please try again later. Thanks [SN]");
                    return;
                }

                itemsSource = new List<RDR1_Ex>(dtoODRD.RDR1_Exs);
                itemsSource.ForEach(x =>
                {
                    x.SO = itemsSourceSelectedSOs.Where(so => so.DocEntry.Equals(x.DocEntry)).FirstOrDefault();
                    x.ItemWhsCode = string.Empty;
                    x.Navigation = Navigation;
                    x.Direction = "out";
                    x.BaseWarehouse = x.WhsCode;
                    x.ItemWhsCode = x.WhsCode;
                });

                // update the po line count 
                itemsSourceSelectedSOs.ForEach(x =>
                {
                    var lineCount = itemsSource.Count(l => l.DocEntry.Equals(x.DocEntry));
                    x.LineCount = lineCount;
                });

                ResetListView();
                GetDocCounts();

                //load the doc series
                if (dtoODRD.DocSeries == null)
                {
                    docSeriesItemsSource = new List<string>();
                    docSeriesItemsSource.Add("Primary");
                }
                else
                {
                    doSeries = dtoODRD.DocSeries;
                    docSeriesItemsSource = new List<string>();
                    dtoODRD.DocSeries.ForEach(x => docSeriesItemsSource.Add(x.SeriesName));
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

                var foundSOItems = itemsSource
                    .Where(x => !string.IsNullOrWhiteSpace(x.ItemCode) &&
                    x.ItemCode.ToLower().Equals(lowercase)).ToList();

                // locate the correct item to do receipt
                if (foundSOItems == null)
                {
                    ShowAlert($"Sorry the item code {itemCode} no found in system setup. Please try other code, Thanks [L]");
                    return;
                }

                if (foundSOItems.Count == 0)
                {
                    ShowAlert($"Sorry the item code {itemCode} no found in system setup. Please try other code, Thanks [LZ]");
                    return;
                }

                if (foundSOItems.Count == 1)
                {
                    HandlerItemCodeSecStep(foundSOItems[0]);
                    return;
                }

                // there is more than 1 same item scan
                MessagingCenter.Subscribe(this, _SelectDuplicateItemLines, (RDR1_Ex selected) =>
                {
                    MessagingCenter.Unsubscribe<RDR1_Ex>(this, _SelectDuplicateItemLines);
                    if (selected == null) return;
                    HandlerItemCodeSecStep(selected);
                    return;
                });

                await Navigation.PushPopupAsync(new DoSoLinesPopUpSelectView(foundSOItems, _SelectDuplicateItemLines));
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
        async void HandlerItemCodeSecStep(RDR1_Ex line)
        {
            try
            {
                currentItem = await GetItem(line.ItemCode);
                if (currentItem == null)
                {
                    ShowAlert($"Sorry the item code {line.ItemCode} no found in system setup. Please try other code, Thanks [I]");
                    return;
                }

                if (currentItem.ManSerNum.Equals("Y"))
                {
                    PrepareSerialDo(line);
                    return;
                }

                if (currentItem.ManBtchNum.Equals("Y"))
                {
                    PrepareBatchDo(line);
                    return;
                }

                // ELSE handle no manage item
                PrepareNonManageGRN(line);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }
        }

        #region PrepareBatchGRN 
        void PrepareBatchDo(RDR1_Ex line)
        {
            try
            {
                if (line.exitQuantity > 0)
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
        void HandlerBatchsItem(List<Batch> batches, RDR1_Ex line)
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

                        itemsSource[lineLocId1].ExitQuantity = pickedBatchInBins.Sum(x => x.TransferBatchQty);
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

                    itemsSource[lineLocId].ExitQuantity = batchInWhs.Sum(x => x.TransferBatchQty); ;
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

        #region PrepareNonManageGRN
        void PrepareNonManageGRN(RDR1_Ex line)
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
        async void HandlerNonManageItemBin(RDR1_Ex line, OWHS targetWhs) // Handler bin warehouse
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
            if (line.exitQuantity > 0)
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

                    itemsSource[lineLocId1].ExitQuantity = qtyBin.Sum(x => x.TransferQty);
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
        async void CaptureItemExitQty(RDR1_Ex line, OWHS targetWhs)
        {
            try
            {
                int locId = itemsSource.IndexOf(itemsSource
                .Where(x => x.SO.DocEntry.Equals(line.SO.DocEntry) && x.LineNum.Equals(line.LineNum)).FirstOrDefault());

                if (locId < 0)
                {
                    ShowAlert("Unable to local the item in list, please try again. Thanks [x1]");
                    return;
                }

                if (line.exitQuantity > 0)
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
                    itemsSource[locId].ExitQuantity = m_exitQty; // assume as reset to the receipt
                    return;
                }

                // else           
                if (m_exitQty < 0) // check negative
                {
                    ShowAlert($"The receive quantity ({m_exitQty:N}) must be numeric and positve," +
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
                itemsSource[locId].ExitQuantity = m_exitQty;
                itemsSource[locId].ItemWhsCode = targetWhs.WhsCode;
                itemsSource[locId].ShowList = $"{m_exitQty:N} -> {targetWhs.WhsCode}";
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }            
        }

        #endregion

        #region PrepareSerialGRN => serial manage handle step
        /// <summary>
        ///  Prepare the serial to bin allocation screen
        /// </summary>
        void PrepareSerialDo(RDR1_Ex line)
        {
            if (line.exitQuantity > 0)
            {
                ResetLine(line);
                return;
            }

            HandlerSerialItem(null, line);
        }

        /// <summary>
        /// Handler the serial Item
        /// </summary>
        async void HandlerSerialItem(List<string> serials, RDR1_Ex line) // handler bin and no bin warehouse
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
                        itemsSource[lineLocId1].ExitQuantity = serialBins.Count;
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
                    itemsSource[lineLocId].ExitQuantity = picked_serials.Count;
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

        /// <summary>
        /// Reset the line cell
        /// </summary>
        /// <param name="line"></param>
        void ResetLine(RDR1_Ex line)
        {
            if (line.exitQuantity > 0)
            {
                int locid = itemsSource.IndexOf(line);
                if (locid < 0) return;
                itemsSource[locid].ExitQuantity = 0;

                ShowToast($"{itemsSource[locid].SO.DocNum}#, " +
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

                var receivedList = itemsSource.Where(x => x.ExitQuantity > 0).ToList();
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

                UserDialogs.Instance.ShowLoading("Awating DO Creation ...");

                // prepre the grpo object list 
                var groupingGuid = Guid.NewGuid();
                var lineList = new List<zwaGRPO>();
                var lineBinList = new List<zwaItemBin>();

                foreach (var line in receivedList)
                {
                    var lindGuid = Guid.NewGuid();
                    lineList.Add(new zwaGRPO
                    {
                        Guid = groupingGuid,
                        ItemCode = line.ItemCode,
                        Qty = line.exitQuantity,
                        SourceCardCode = line.SO.CardCode,
                        SourceDocNum = line.SO.DocNum,
                        SourceDocEntry = line.DocEntry,
                        SourceDocBaseType = Convert.ToInt32(line.ObjType), // from purchase order
                        SourceBaseEntry = line.DocEntry,
                        SourceBaseLine = line.LineNum,
                        Warehouse = (string.IsNullOrWhiteSpace(line.ItemWhsCode)) ? line.BaseWarehouse : line.ItemWhsCode,
                        LineGuid = lindGuid
                    }) ; 

                    List<zwaItemBin> binList = line.GetList(groupingGuid, lindGuid);
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
                    request = "Create DO",
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
                    request = "CreateDoRequest",
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
                    content = await httpClient.RequestSvrAsync_mgt(requestBag, "Do");
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
        //    var callerAddress = "CreateDO_DocCheck";
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
            const int primarySeries = 5;
            if (string.IsNullOrWhiteSpace(selectedSeriesName)) return primarySeries;
            if (doSeries == null) return primarySeries;
            if (doSeries.Length == 0) return primarySeries;

            var series =
                doSeries.Where(x => x.SeriesName.ToLower().Equals(selectedSeriesName.ToLower())).FirstOrDefault();

            if (series == null) return primarySeries;
            else return series.Series;
        }

        /// <summary>
        /// Reset the data source of the item in the user screen
        /// </summary>
        void ResetListView()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<RDR1_Ex>(itemsSource);
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
