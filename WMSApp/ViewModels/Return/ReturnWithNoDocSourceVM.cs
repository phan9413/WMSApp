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
using WMSApp.Views.Share;
using WMSApp.Views.Transfer1;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace WMSApp.ViewModels.Return
{
    public class ReturnWithNoDocSourceVM : INotifyPropertyChanged
    {
        #region View binding property
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Command CmdSave { get; set; }
        public Command CmdCancel { get; set; }
        public Command CmdSearchBarVisible { get; set; }
        public Command CmdChangeWhs { get; set; }
        public Command CmdStartScan { get; set; }
        public Command CmdSelectBp { get; set; }
        public Command CmdManualInput { get; set; }
        OCRD CurrentBp;
        string cardCode;
        public string CardCode
        {
            get => cardCode;
            set
            {
                if (cardCode == value) return;
                cardCode = value;
                OnPropertyChanged(nameof(CardCode));
            }
        }

        string cardName;
        public string CardName
        {
            get => cardName;
            set
            {
                if (cardName == value) return;
                cardName = value;
                OnPropertyChanged(nameof(CardName));
            }
        }

        string entryWhs;
        public string EntryWhs
        {
            get => entryWhs;
            set
            {
                if (entryWhs == value) return;
                entryWhs = value;
                OnPropertyChanged(nameof(EntryWhs));
            }
        }

        ReturnCommonLine_Ex selectedItem;
        public ReturnCommonLine_Ex SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
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

        List<ReturnCommonLine_Ex> itemsSource;
        public ObservableCollection<ReturnCommonLine_Ex> ItemsSource { get; set; }
        #endregion

        zwaRequest currentRequest { get; set; } = null;
        OWHS CurrentEntryWhs { get; set; } = null;
        OITM CurrentItem { get; set; } = null;
        INavigation Navigation { get; set; } = null;

        readonly string _PromptWhs = "_PromptWhs20201110T1021_MGR";
        readonly string _PromptBp = "_PromptBp20201110T1021_MGR";
        readonly string _PromptScanCode = "_PromptScanCode20201110T1021_MGR";

        readonly string _AllocateBatchToBin = "_AllocateBatchToBin_20201122T1459";
        readonly string _CollectReturnSerial = "_CollectReturnSerial20201122T1650";
        readonly string _AllowcatingSerialToBin = "_AllowcatingSerialToBin_20201122T1652";
        readonly string _CollectReturnBatch = "_CollectReturnBatch_20201121T1458";
        readonly string _AllocateItemQtyToBin = "_AllocateItemQtyToBin_20201122T1607";
        readonly string _PopScreenAddr = "_PopScreenAddr_201207_ReturnWNoSource";

        // the constructor
        public ReturnWithNoDocSourceVM(INavigation navigation)
        {
            Navigation = navigation;
            PromptSelectWarehouse();
            PromptSelectBp();
            LoadDocSeries();
            InitCmd();
        }

        /// <summary>
        ///  Initial the command liking 
        /// </summary>
        void InitCmd()
        {
            CmdSave = new Command(SaveAndUpdate);
            CmdCancel = new Command(Cancel);
            CmdSearchBarVisible = new Command<SearchBar>((SearchBar searchbar) =>
            {
                searchbar.IsVisible = !searchbar.IsVisible;
                if (searchbar.IsVisible)
                {
                    searchbar.Focus();
                    return;
                }
            });

            CmdChangeWhs = new Command(PromptSelectWarehouse);
            CmdStartScan = new Command(StartScanner);
            CmdSelectBp = new Command(PromptSelectBp);
            CmdManualInput = new Command(PromptInputItemCode);
        }

        /// <summary>
        /// use to remove the selected line
        /// </summary>
        /// <param name="removeLine"></param>
        public void RemoveLine(ReturnCommonLine_Ex removeLine)
        {
            try
            {
                if (itemsSource == null) return;
                if (itemsSource.Count == 0) return;

                itemsSource.Remove(removeLine);
                RefreshListview();
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Prompt input item code
        /// </summary>
        async void PromptInputItemCode()
        {
            try
            {
                string captureItemCode = await new Dialog().DisplayPromptAsync(
                    $"Input Item Code", "", "OK", "Cancel", null, -1, Keyboard.Default, "");

                if (string.IsNullOrWhiteSpace(captureItemCode)) return;
                if (captureItemCode.ToLower().Equals("cancel")) return;

                ProcessItemCde(captureItemCode);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
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
                    request = "GetDocSeries"
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Return");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                var dtoOprr = JsonConvert.DeserializeObject<DTO_OPRR>(content);
                if (dtoOprr == null)
                {
                    DisplayToast("There is error reading server info, please try again later,Thanks.");
                    return;
                }

                //load the doc series
                if (dtoOprr.DocSeries == null)
                {
                    docSeriesItemsSource = new List<string>();
                    docSeriesItemsSource.Add("Primary");
                }
                else
                {
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
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Prompt select the vendor
        /// </summary>
        async void PromptSelectBp()
        {
            try
            {
                MessagingCenter.Subscribe<OCRD>(this, _PromptBp, async (OCRD selectedBp) =>
                {
                    MessagingCenter.Unsubscribe<OCRD>(this, _PromptBp);
                    if (selectedBp == null) return;

                    /// check vendor
                    if (itemsSource != null && !CurrentBp.Equals(selectedBp))
                    {
                        bool confirmChangedBp = await new Dialog()
                            .DisplayAlert($"Confirm change Bp?",
                            $"Update document rows according to new BP's data?", "OK", "Cancel");

                        if (confirmChangedBp)
                        {
                            // change all itemssource line
                            // line.PO.PO.CardCode
                            itemsSource.ForEach(x =>
                            {
                                x.Head.CardCode = selectedBp.CardCode;
                                x.Head.CardName = selectedBp.CardName;
                                x.OnPropertyChanged("CardName");
                                x.OnPropertyChanged("CardCode");
                            });

                            CurrentBp = selectedBp;
                            CardName = CurrentBp.CardName;
                            CardCode = CurrentBp.CardCode;
                            RefreshListview();
                        }
                    }
                    else // if same vender
                    {
                        CurrentBp = selectedBp;
                        CardName = CurrentBp.CardName;
                        CardCode = CurrentBp.CardCode;
                    }
                });

                await Navigation.PushPopupAsync(new SelectVendorPopUpView(_PromptBp, "Select a Bp", Color.LightSkyBlue, true));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Start a scanner to capture Item code
        /// </summary>
        void StartScanner()
        {
            MessagingCenter.Subscribe(this, _PromptScanCode, (string code) =>
            {
                MessagingCenter.Unsubscribe<string>(this, _PromptScanCode);

                if (string.IsNullOrWhiteSpace(code)) return;
                if (code.ToLower().Equals("cancel")) return;

                // process the code with program logic flow
                ProcessItemCde(code);
            });

            Navigation.PushAsync(new CameraScanView(_PromptScanCode, "Scan / Input Item Code#"));
        }

        /// <summary>
        /// Show a warehouse slection screen
        /// </summary>
        void PromptSelectWarehouse()
        {
            try
            {
                MessagingCenter.Subscribe(this, _PromptWhs, (OWHS selectedWhs) =>
                {
                    MessagingCenter.Unsubscribe<OWHS>(this, _PromptWhs);

                    CurrentEntryWhs = selectedWhs;
                    EntryWhs = CurrentEntryWhs.WhsCode;
                });

                Navigation.PushPopupAsync(new PickWarehousePopUpView(_PromptWhs,
                    "Select an entry warehouse", Color.Yellow));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// process item code
        /// </summary>
        /// <param name="code"></param>
        async void ProcessItemCde(string itemCode)
        {
            try
            {
                CurrentItem = await GetItem(itemCode);
                if (CurrentItem == null)
                {
                    DisplayAlert($"Sorry the item code {itemCode} no found in system setup. Please try other code, Thanks [I]");
                    return;
                }

                if (itemsSource == null) itemsSource = new List<ReturnCommonLine_Ex>();
                SelectedItem = CreateNewLine(); //<-- create new line
                if (selectedItem == null)
                {
                    DisplayAlert($"There is issue to create the item code {itemCode}. Please try again later, Thanks [I]");
                    return;
                }

                if (CurrentItem.ManSerNum.Equals("Y"))
                {
                    PrepareSerialRet(selectedItem);
                    return;
                }

                if (CurrentItem.ManBtchNum.Equals("Y"))
                {
                    PrepareBatchRet(selectedItem);
                    return;
                }

                // ELSE handle no manage item
                PrepareNonManageReturn(selectedItem);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }

        #region PrepareNonManage return
        void PrepareNonManageReturn(ReturnCommonLine_Ex line)
        {
            try
            {
                if (CurrentEntryWhs.BinActivat.Equals("Y"))
                {
                    HandlerNonManageItemBin(line, CurrentEntryWhs); // handler bin warehouse
                    return;
                }

                // handle normal qty capture
                CaptureItemEntryQty(line, CurrentEntryWhs);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Handler non manage item
        /// </summary>
        async void HandlerNonManageItemBin(ReturnCommonLine_Ex line, OWHS targetWhs) // Handler bin warehouse
        {
            // checking
            if (itemsSource == null)
            {
                DisplayToast("Unable to process the next step, " +
                    "as the warehouse configuration is no load in correctly, please close the app, and try again later, Thanks. ");
                return;
            }

            //reset the qty
            if (line.EntryQuantity > 0)
            {
                ResetLine(line);
                return;
            }

            #region reserved additional control
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
            #endregion

            if (!targetWhs.BinActivat.Equals("Y")) return;

            string entrytQty = await new Dialog().DisplayPromptAsync(
                  $"Input entry qty for {line.ItemCodeDisplay}",
                  $"{line.ItemNameDisplay}\nOpen Qty: {line.OpenQty:N}",
                  "OK",
                  "Cancel",
                  "",
                  -1,
                  Keyboard.Numeric,
                  "1");

            var isNumeric = Decimal.TryParse(entrytQty, out decimal m_entrytQty);
            if (!isNumeric) return;// check numeric

            if (m_entrytQty.Equals(0))    // check zero, if zero assume as reset receipt qty
            {
                // prompt select whs             
                DisplayAlert($"The receive quantity ({m_entrytQty:N}) must be numeric and positve," +
                  $" negative are no allowance.\nPlease try again later. Thanks [1]");
                return;
            }

            // else           
            if (m_entrytQty < 0) // check negative
            {
                DisplayAlert($"The receive quantity ({m_entrytQty:N}) must be numeric and positve," +
                    $" negative are no allowance.\nPlease try again later. Thanks [2]");
                return;
            }


            // show serial to bin allocation 
            MessagingCenter.Subscribe(this, _AllocateItemQtyToBin, (List<OBIN_Ex> itemQtyBin) =>
            {
                MessagingCenter.Unsubscribe<List<OBIN_Ex>>(this, _AllocateItemQtyToBin);
                if (itemQtyBin == null)
                {
                    DisplayToast("Allocate item qty to bin operation cancel");
                    return;
                }

                if (itemQtyBin.Count == 0)
                {
                    DisplayToast("Allocate item qty to bin operation cancel");
                    return;
                }

                //int lineLocId1 = itemsSource.IndexOf(line);
                //if (lineLocId1 < 0)
                //{
                //    DisplayToast("There is issue to locate the item in the list, please try again later, Thanks.");
                //    return;
                //}

                line.EntryQuantity = itemQtyBin.Sum(x => x.BinQty);
                line.Bins = itemQtyBin;
                line.ItemWhsCode = targetWhs.WhsCode;
                line.ShowList = $"To Whs {targetWhs.WhsCode} ->\n" +
                                String.Join("\n", itemQtyBin.Select(x => $"{x.BinQty:N} -> {x.oBIN.BinCode}"));

                AddToList(line);
            });

            // launch the item qyt from bin arragement
            await Navigation.PushAsync(
                new ItemToBinSelectView(targetWhs.WhsCode, _AllocateItemQtyToBin,
                line.ItemCodeDisplay, line.ItemNameDisplay, m_entrytQty, null));
        }

        /// <summary>
        /// Prompt dialog to capture the Qty
        /// </summary>
        /// <param name="selectedPOLine"></param>
        async void CaptureItemEntryQty(ReturnCommonLine_Ex line, OWHS targetWhs)
        {
            try
            {
                if (line.entryQuantity > 0)
                {
                    ResetLine(line);
                    return;
                }

                string entrytQty = await new Dialog().DisplayPromptAsync(
                    $"Input entry qty for {line.ItemCodeDisplay}",
                    $"{line.ItemNameDisplay}\nOpen Qty: {line.OpenQty:N}",
                    "OK",
                    "Cancel",
                    "",
                    -1,
                    Keyboard.Numeric,
                    "1");

                var isNumeric = Decimal.TryParse(entrytQty, out decimal m_entrytQty);
                if (!isNumeric) return;// check numeric

                if (m_entrytQty.Equals(0))    // check zero, if zero assume as reset receipt qty
                {
                    // prompt select whs             
                    DisplayAlert($"The receive quantity ({m_entrytQty:N}) must be numeric and positve," +
                      $" negative are no allowance.\nPlease try again later. Thanks [1]");
                    return;
                }

                // else           
                if (m_entrytQty < 0) // check negative
                {
                    DisplayAlert($"The receive quantity ({m_entrytQty:N}) must be numeric and positve," +
                        $" negative are no allowance.\nPlease try again later. Thanks [2]");
                    return;
                }

                #region reserved additional control
                //if (itemsSource[locId].POLine.OpenQty < m_exitQty) // check open qty
                //{
                //    ShowAlert($"The receive quantity ({m_exitQty:N}) must be equal or " +
                //        $"smaller than the <= Open qty ({itemsSource[locId].POLine.OpenQty:N}).\nPlease try again later. Thanks [x3]");
                //    return;
                //}

                // update into the list in memory    
                // non manage item
                //int locId = itemsSource.IndexOf(itemsSource
                //    .Where(x => x.Head.DocEntry.Equals(line.Head.DocEntry) && x.LineNum.Equals(line.LineNum)).FirstOrDefault());

                //if (locId < 0)
                //{
                //    DisplayAlert("Unable to local the item in list, please try again. Thanks [x1]");
                //    return;
                //}
                #endregion

                line.EntryQuantity = m_entrytQty;
                line.ItemWhsCode = targetWhs.WhsCode;
                line.ShowList = $"To Whs {targetWhs.WhsCode} -> {m_entrytQty:N}";

                AddToList(line);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }

        #endregion

        #region PrepareBatch Return
        void PrepareBatchRet(ReturnCommonLine_Ex line)
        {
            try
            {
                if (line.entryQuantity > 0)
                {
                    ResetLine(line);
                    return;
                }
                LaunchBatchesCollection(line);
                #region resered control code
                //if (currentSourceType.Contains(_DELIVERY))
                //{
                //    List<Batch> batches = await GetBatchesInDo(line.DocEntry, line.LineNum);
                //    if (batches == null)
                //    {
                //        LaunchBatchesCollection(line);
                //        return;
                //    }

                //    if (batches.Count == 0)
                //    {
                //        LaunchBatchesCollection(line);
                //        return;
                //    }

                //    HandlerBatchsItem(batches, line);
                //    return;
                //}

                ///// handle the doc from request
                //LaunchBatchesCollection(line);
                #endregion
            }
            catch (Exception excep)
            {
                Console.Write(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Launch the screen to collect the batch
        /// </summary>
        /// <param name="Line"></param>
        void LaunchBatchesCollection(ReturnCommonLine_Ex line)
        {
            // launch to collect the batch screen
            MessagingCenter.Subscribe(this, _CollectReturnBatch, (List<Batch> collectReturnBatch) =>
            {
                MessagingCenter.Unsubscribe<List<Batch>>(this, _CollectReturnBatch);

                if (collectReturnBatch == null)
                {
                    DisplayToast("Collect batch opr cancel.");
                    return;
                }

                if (collectReturnBatch.Count == 0)
                {
                    DisplayToast("Collect batch opr cancel.");
                    return;
                }

                HandlerBatchsItem(collectReturnBatch, line);
            });
            Navigation.PushAsync(new CollectBatchesView(_CollectReturnBatch));
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
                if (itemsSource == null)
                {
                    DisplayToast("Unable to process the next step, " +
                        "as the warehouse configuration is no load in correctly, " +
                        "please close the app, and try again later, Thanks. ");
                    return;
                }

                OWHS lineWhs = CurrentEntryWhs;

                #region Allocating batch to whs
                if (!lineWhs.BinActivat.Equals("Y")) // no bin whs
                {
                    line.EntryQuantity = batches.Sum(x => x.Qty); ;
                    line.Batches = batches;
                    line.ItemWhsCode = lineWhs.WhsCode;
                    line.ShowList =
                            $"To Whs {lineWhs.WhsCode} ->\n" +
                            string.Join("\n", batches.Select(x => $"{x.DistNumber} -> {x.Qty:N}").ToList());

                    AddToList(line);
                    return;
                    #region reserved addition control
                    //MessagingCenter.Subscribe(this, _BatchQtyWhsAllocation, (List<zwaTransferDocDetailsBin> batchWhsAllocation) =>
                    //{
                    //    MessagingCenter.Unsubscribe<List<zwaTransferDocDetailsBin>>(this, _BatchQtyWhsAllocation);
                    //    if (batchWhsAllocation == null)
                    //    {
                    //        DisplayToast("Batch qty allocation to warehouse operation cancel");
                    //        return;
                    //    }

                    //    if (batchWhsAllocation.Count == 0)
                    //    {
                    //        DisplayToast("Batch qty allocation to warehouse operation cancel");
                    //        return;
                    //    }

                    //    var batch = new List<Batch>();
                    //    batchWhsAllocation.ForEach(x => batch.Add(new Batch
                    //    {
                    //        DistNumber = x.Batch,
                    //        Qty = x.Qty
                    //    }));

                    //    //int lineLocId2 = itemsSource.IndexOf(line);
                    //    //if (lineLocId2 < 0)
                    //    //{
                    //    //    DisplayToast("There is issue to locate the item in the list, please try again later, Thanks.");
                    //    //    return;
                    //    //}

                    //    line.EntryQuantity = batch.Sum(x => x.Qty); ;
                    //    line.Batches = batch;
                    //    line.ItemWhsCode = lineWhs.WhsCode;
                    //    line.ShowList =
                    //            $"To Whs {lineWhs.WhsCode} ->\n" +
                    //            string.Join("\n", batch.Select(x => $"{x.DistNumber} -> {x.Qty:N}").ToList());

                    //    AddToList(line);
                    //});

                    //Navigation.PushAsync(new BatchesConfirmView(lineWhs.WhsCode,
                    //    _BatchQtyWhsAllocation, batches, line.ItemCodeDisplay, line.ItemNameDisplay));
                    #endregion
                }
                #endregion

                #region allocating batch to bin                 
                MessagingCenter.Subscribe(this, _AllocateBatchToBin, (List<zwaTransferDocDetailsBin> batchToBin) =>
                {
                    MessagingCenter.Unsubscribe<List<zwaTransferDocDetailsBin>>(this, _AllocateBatchToBin);
                    if (batchToBin == null)
                    {
                        DisplayToast("Allocate batch to bin operation cancel");
                        return;
                    }

                    if (batchToBin.Count == 0)
                    {
                        DisplayToast("Allocate batch to bin operation cancel");
                        return;
                    }

                    decimal receiptQty1 = 0;
                    string showList = $"To Whs {lineWhs.WhsCode} ->\n";

                    batchToBin.ForEach(bb =>
                       bb.Bins
                           .ForEach(bin =>
                           {
                               receiptQty1 += bin.BatchTransQty;
                               showList += $"{bin.BatchTransQty:N} -> {bin.BatchNumber} -> {bin.oBIN.BinCode}\n";
                           }));

                    //int lineLocId1 = itemsSource.IndexOf(line);
                    //if (lineLocId1 < 0)
                    //{
                    //    DisplayToast("There is issue to locate the item in the list, please try again later, Thanks.");
                    //    return;
                    //}

                    line.EntryQuantity = receiptQty1;
                    line.BatchWithBins = batchToBin;
                    line.ItemWhsCode = lineWhs.WhsCode;
                    line.ShowList = showList;
                    AddToList(line);
                });

                // launch the serial to bin arragement
                Navigation.PushAsync(
                    new BatchToBinView(batches, lineWhs.WhsCode, _AllocateBatchToBin, line.ItemCodeDisplay, line.ItemNameDisplay));
                #endregion
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }

        #endregion

        #region PrepareSerial Return => serial manage handle step
        /// <summary>
        ///  Prepare the serial to bin allocation screen
        /// </summary>
        void PrepareSerialRet(ReturnCommonLine_Ex line)
        {
            if (line.entryQuantity > 0)
            {
                ResetLine(line);
                return;
            }

            LaunchCollectSerialView(line);
            #region reserved control code
            //if (currentSourceType.Contains(_DELIVERY))
            //{
            //    var serialList = await GetSerialInDo(line.DocEntry, line.LineNum);
            //    if (serialList == null)
            //    {
            //        LaunchCollectSerialView(line);
            //        return;
            //    }

            //    if (serialList.Count == 0)
            //    {
            //        LaunchCollectSerialView(line);
            //        return;
            //    }

            //    HandlerSerialItem(serialList, line);
            //    return;
            //}

            //// for doc from return request
            //LaunchCollectSerialView(line);
            #endregion
        }

        /// <summary>
        /// Luanch the serial collection screen
        /// </summary>
        /// <param name="line"></param>
        void LaunchCollectSerialView(ReturnCommonLine_Ex line)
        {
            // launch the screen to collect the serial number, and pass to entry the warehouse
            MessagingCenter.Subscribe(this, _CollectReturnSerial, (List<string> collectedSerials) =>
            {
                MessagingCenter.Unsubscribe<List<string>>(this, _CollectReturnSerial);
                if (collectedSerials == null)
                {
                    DisplayToast("Collect serial opr cancel.");
                    return;
                }

                if (collectedSerials.Count == 0)
                {
                    DisplayToast("Collect serial opr cancel.");
                    return;
                }

                HandlerSerialItem(collectedSerials, line);
            });

            // collecting serial from camera
            Navigation.PushAsync(new CollectSerialsView(_CollectReturnSerial));
        }

        /// <summary>
        /// Handler the serial Item
        /// </summary>
        async void HandlerSerialItem(List<string> serials, ReturnCommonLine_Ex line) // handler bin and no bin warehouse
        {
            try
            {
                if (itemsSource == null)
                {
                    DisplayToast("Unable to process the next step, " +
                        "as the warehouse configuration is no load in correctly, " +
                        "please close the app, and try again later, Thanks. ");
                    return;
                }

                OWHS lineWhs = CurrentEntryWhs;
                #region warehouse checking
                // OWHS lineWhs = CurrentEntryWhs;
                //if (!string.IsNullOrWhiteSpace(line.ItemWhsCode))
                //{
                //    lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.ItemWhsCode)).FirstOrDefault();
                //}
                //else
                //{
                //    lineWhs = App.Warehouses.Where(x => x.WhsCode.Equals(line.WhsCode)).FirstOrDefault();
                //}

                //if (lineWhs == null)
                //{
                //    DisplayToast("Error reading warehouse info, please try again later, Thanks.");
                //    return;
                //}
                #endregion

                #region Allocating serial to bin
                if (!lineWhs.BinActivat.Equals("Y"))
                {
                    var showlist = $"To Whs {lineWhs.WhsCode} ->\n{string.Join("\n", serials)}";
                    line.EntryQuantity = serials.Count;
                    line.Serials = serials;
                    line.ItemWhsCode = lineWhs.WhsCode;
                    line.ShowList = showlist;

                    AddToList(line);
                    return;

                    #region reserved additional code
                    //MessagingCenter.Subscribe(this, _AllocationSerialToWhs, (List<zwaTransferDocDetailsBin> selectedReturnSerial) =>
                    //{
                    //    MessagingCenter.Unsubscribe<List<zwaTransferDocDetailsBin>>(this, _AllocationSerialToWhs);
                    //    if (selectedReturnSerial == null)
                    //    {
                    //        DisplayToast("Serial allocation to whs operation cancel.");
                    //        return;
                    //    }
                    //    if (selectedReturnSerial.Count == 0)
                    //    {
                    //        DisplayToast("Serial allocation to whs operation cancel.");
                    //        return;
                    //    }

                    //    var selectedSerialStr = selectedReturnSerial.Select(x => x.Serial).ToList();
                    //    //int lineLocId1 = itemsSource.IndexOf(line);
                    //    //if (lineLocId1 < 0)
                    //    //{
                    //    //    DisplayToast("There is issue to locate the item in the list, please try again later, Thanks.");
                    //    //    return;
                    //    //}

                    //    var showlist = $"To Whs {lineWhs.WhsCode} ->\n{string.Join("\n", selectedSerialStr)}";
                    //    line.EntryQuantity = selectedSerialStr.Count;
                    //    line.Serials = selectedSerialStr;
                    //    line.ItemWhsCode = lineWhs.WhsCode;
                    //    line.ShowList = showlist;

                    //    AddToList(line);
                    //});

                    //await Navigation.PushAsync(
                    //    new SerialsConfirmView(serials, line.ItemCodeDisplay, line.ItemNameDisplay, lineWhs.WhsCode, _AllocationSerialToWhs));

                    // return;
                    #endregion
                }
                #endregion

                #region Allocating serial to warehouse           
                MessagingCenter.Subscribe(this, _AllowcatingSerialToBin, (List<zwaTransferDocDetailsBin> serialToBin) =>
                {
                    if (serialToBin == null)
                    {
                        DisplayToast("Allocate serial to bin operation cancel");
                        return;
                    }

                    if (serialToBin.Count == 0)
                    {
                        DisplayToast("Allocate serial to bin operation cancel");
                        return;
                    }

                    //int lineLocId = itemsSource.IndexOf(line);
                    //if (lineLocId < 0)
                    //{
                    //    DisplayToast("There is issue to locate the item in the list, please try again later, Thanks.");
                    //    return;
                    //}

                    line.EntryQuantity = serialToBin.Count;
                    line.SerialsWithBins = serialToBin;
                    line.ItemWhsCode = lineWhs.WhsCode;
                    line.ShowList = $"To Whs {lineWhs.WhsCode} ->\n" +
                                String.Join("\n", serialToBin.Select(x => $"{x.Serial} -> {x.BinCode}"));

                    AddToList(line);
                });

                await Navigation.PushAsync(
                    new SerialToBinView(serials, line.ItemCodeDisplay, line.ItemNameDisplay
                                        , lineWhs.WhsCode, _AllowcatingSerialToBin));
                #endregion
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }
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

                DisplayToast($"{itemsSource[locid].Head.DocNum}#, " +
                    $"Item line {itemsSource[locid].ItemCodeDisplay}\n{itemsSource[locid].ItemNameDisplay} reset.");
                return;
            }
        }

        /// <summary>
        /// Create a new line and return
        /// </summary>
        /// <returns></returns>
        ReturnCommonLine_Ex CreateNewLine()
        {
            try
            {
                var newline = new ReturnCommonLine_Ex();
                newline.ItemCode = CurrentItem.ItemCode;
                newline.Dscription = CurrentItem.ItemName;
                newline.Head = new ReturnCommonHead_Ex();
                newline.Head.DocEntry = -1;
                newline.Head.DocNum = -1;
                newline.Head.CardCode = this.cardCode;
                newline.Head.CardName = this.cardName;
                return newline;
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
                return null;
            }
        }

        /// <summary>
        /// Add into the new line 
        /// </summary>
        /// <param name="newLine"></param>
        void AddToList(ReturnCommonLine_Ex newLine)
        {
            if (itemsSource == null)
            {
                itemsSource = new List<ReturnCommonLine_Ex>();
                itemsSource.Add(newLine);
                return;
            }

            var duplicatedItem = itemsSource
                .Where(x => x.ItemCodeDisplay.ToLower().Equals(newLine.ItemCodeDisplay.ToLower())).FirstOrDefault();

            if (duplicatedItem == null)
            {
                itemsSource.Add(selectedItem); // add into the itemsource       
                RefreshListview();
                return;
            }

            DisplayAlert($"The {newLine.ItemCodeDisplay}\n{newLine.ItemNameDisplay} found duplicated in list, please reset the " +
                $"existing and try to add again, Thanks.");

        }

        /// <summary>
        /// redbing the list view
        /// </summary>
        void RefreshListview()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<ReturnCommonLine_Ex>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
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
                    DisplayAlert(content);
                    return null;
                }

                Cio replied = JsonConvert.DeserializeObject<Cio>(content);
                if (replied == null)
                {
                    DisplayAlert("There is error reading information from server, please try again later. Thanks");
                    return null;
                }

                if (replied.Item == null)
                {
                    DisplayAlert($"The item code {itemCode} no found in system setup, " +
                        $"please confirm the code scan / input, and try again. Thanks");
                    return null;
                }

                return replied.Item;
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
                return null;
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
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
                    DisplayAlert("The item list if empty, please try again later. Thanks");
                    return;
                }

                var receivedList = itemsSource.Where(x => x.EntryQuantity > 0).ToList();
                if (receivedList == null)
                {
                    DisplayAlert("There is no items exit entered, please try again later. Thanks");
                    return;
                }

                if (receivedList.Count == 0)
                {
                    DisplayAlert("There is no items exit entered, please try again later. Thanks");
                    return;
                }

                UserDialogs.Instance.ShowLoading("Awating Return Creation ...");

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
                    NumAtCard = numberAtCard
                };

                // 20200719T1030
                // prepare the request dto
                currentRequest = new zwaRequest
                {
                    sapUser = App.waUser,
                    request = "Create Return",
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
                    request = "CreateReturn",
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
                    content = await httpClient.RequestSvrAsync_mgt(requestBag, "Return");
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
        //    var callerAddress = "CreateReturn_Manual_DocCheck";
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
        /// leave the screen
        /// </summary>
        void Cancel() => Navigation.PopAsync();

        /// <summary>
        /// Display message dialog
        /// </summary>
        /// <param name="message"></param>
        async void DisplayAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "OK");

        /// <summary>
        /// Display the toast message
        /// </summary>
        /// <param name="message"></param>
        void DisplayToast(string message) => DependencyService.Get<IToastMessage>()?.ShortAlert(message);
    }
}