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
using WMSApp.Views.Share;
using WMSApp.Views.Transfer1;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace WMSApp.ViewModels.GRPO
{
    public class GrpoWithNoPoVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Command CmdSave { get; set; }
        public Command CmdCancel { get; set; }
        public Command CmdSearchBarVisible { get; set; }
        public Command CmdChangeWhs { get; set; }
        public Command CmdStartScan { get; set; }
        public Command CmdSelectVendor { get; set; }
        public Command CmdManualInput { get; set; }

        OCRD CurrentVendor;
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


        POR1_Ex selectedItem;
        public POR1_Ex SelectedItem
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

        List<POR1_Ex> itemsSource;
        public ObservableCollection<POR1_Ex> ItemsSource { get; set; }

        zwaRequest currentRequest { get; set; } = null;
        OWHS CurrentEntryWhs { get; set; } = null;
        OITM CurrentItem { get; set; } = null;
        INavigation Navigation { get; set; } = null;

        readonly string _PromptWhs = "_PromptWhs20201110T1021";
        readonly string _PromptVendor = "_PromptVendor20201110T1021";
        readonly string _PromptScanCode = "_PromptScanCode20201110T1021";
        readonly string _CaptureSerials = "_CaptureSerials20201112";
        readonly string _SerialsToBin = "_SerialsToBin20201112";
        readonly string _BatchesToBin = "_BatchesToBin20201112";
        readonly string _CaptureBatches = "_CaptureBatches20201112";
        readonly string _ItemQtyToBin = "_ItemQtyToBin20201112";
        readonly string _PopScreenAddr = "_PopScreenAddr_GRPOWNoSource_201207";

        // the constructor
        public GrpoWithNoPoVM(INavigation navigation)
        {
            Navigation = navigation;

            PromptSelectWarehouse();
            PromptSelectVendor();
            LoadDocSeries();
            InitCmd();
        }

        /// <summary>
        ///  Initial the command liking 
        /// </summary>
        void InitCmd()
        {
            CmdSave = new Command(Save);
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
            CmdSelectVendor = new Command(PromptSelectVendor);
            CmdManualInput = new Command(PromptInputItemCode);
        }

        /// <summary>
        /// use to remove the selected line
        /// </summary>
        /// <param name="removeLine"></param>
        public void RemoveLine(POR1_Ex removeLine)
        {
            try
            {
                if (itemsSource == null) return;
                if (itemsSource.Count == 0) return;

                int locId = itemsSource.IndexOf(removeLine);
                if (locId < 0) return;

                itemsSource.RemoveAt(locId);
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
                    request = "GetGRPODocSeries"
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
                    DisplayAlert($"{content}");
                    return;
                }

                var dtpGrpo = JsonConvert.DeserializeObject<DtoGrpo>(content);

                // load the doc series 
                if (dtpGrpo.GrpoDocSeries == null)
                {
                    GrpoSeries = dtpGrpo.GrpoDocSeries;
                    docSeriesItemsSource = new List<string>();
                    GrpoSeries.ForEach(x => docSeriesItemsSource.Add(x.SeriesName));
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
        /// Prompt select the vendor
        /// </summary>
        async void PromptSelectVendor()
        {
            try
            {
                MessagingCenter.Subscribe<OCRD>(this, _PromptVendor, async (OCRD selectedVendor) =>
                {
                    MessagingCenter.Unsubscribe<OCRD>(this, _PromptVendor);
                    if (selectedVendor == null) return;

                    /// check vendor
                    if (itemsSource != null && !CurrentVendor.Equals(selectedVendor))
                    {
                        bool confirmChangedVendor = await new Dialog()
                            .DisplayAlert($"Confirm change vendor?",
                            $"Update document rows according to new BP's data?", "OK", "Cancel");

                        if (confirmChangedVendor)
                        {
                            // change all itemssource line
                            // line.PO.PO.CardCode
                            itemsSource.ForEach(x =>
                                {
                                    x.PO.PO.CardCode = selectedVendor.CardCode;
                                    x.PO.PO.CardName = selectedVendor.CardName;
                                    x.OnPropertyChanged("CardName");
                                    x.OnPropertyChanged("CardCode");
                                });

                            CurrentVendor = selectedVendor;
                            CardName = CurrentVendor.CardName;
                            CardCode = CurrentVendor.CardCode;
                            RefreshListview();
                        }
                    }
                    else // if same vender
                    {
                        CurrentVendor = selectedVendor;
                        CardName = CurrentVendor.CardName;
                        CardCode = CurrentVendor.CardCode;
                    }
                });

                await Navigation.PushPopupAsync(new SelectVendorPopUpView(_PromptVendor, "Select a vendor", Color.Yellow));
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

                if (itemsSource == null) itemsSource = new List<POR1_Ex>();

                SelectedItem = CreateNewLine();
                if (selectedItem == null)
                {
                    DisplayAlert($"There is issue to create the item code {itemCode}. Please try again later, Thanks [I]");
                    return;
                }

                if (CurrentItem.ManSerNum.Equals("Y"))
                {
                    PrepareSerialGRN(selectedItem);
                    return;
                }

                if (CurrentItem.ManBtchNum.Equals("Y"))
                {
                    PrepareBatchGRN(selectedItem);
                    return;
                }

                // ELSE handle no manage item
                PrepareNonManageGRN(selectedItem);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }

        #region PrepareNonManageGRN
        void PrepareNonManageGRN(POR1_Ex line)
        {
            if (CurrentEntryWhs == null)
            {
                DisplayToast("Please select an entry warehouse, and try again, Thanks");
                PromptSelectWarehouse();
                return;
            }

            if (CurrentEntryWhs.BinActivat.Equals("Y"))
            {
                HandlerNonManageItemBin(line); // handler bin warehouse
                return;
            }
            // handle normal qty capture
            CaptureItemReceiptQty(line);
        }

        /// <summary>
        /// Handler non manage item
        /// </summary>
        async void HandlerNonManageItemBin(POR1_Ex line) // Handler bin warehouse
        {
            //// checking
            //if (App.Warehouses == null)
            //{
            //    DisplayToast("Unable to process the next step, " +
            //        "as the warehouse configuration is no load in correctly, please close the app, and try again later, Thanks. ");
            //    return;
            //}

            //if (itemsSource == null)
            //{
            //    ShowToast("Unable to process the next step, " +
            //        "as the warehouse configuration is no load in correctly, please close the app, and try again later, Thanks. ");
            //    return;
            //}

            string inputQty = await new Dialog().DisplayPromptAsync(
                $"Input item code {line.ItemCodeDisplay}", "receipt qty", "Ok", "cancel", null, -1, Keyboard.Numeric, "");

            if (string.IsNullOrWhiteSpace(inputQty))
            {
                return;
            }
            if (inputQty.ToLower().Equals("cancel"))
            {
                return;
            }

            bool IsNumeric = decimal.TryParse(inputQty, out decimal result);

            if (!IsNumeric)
            {
                DisplayToast("Invalid receipt qty, please enter numeric and positive value, please try again later. Thanks");
                return;
            }

            if (result <= 0)
            {
                DisplayToast("Invalid receipt qty, please enter numeric and positive value, please try again later. Thanks");
                return;
            }

            if (CurrentEntryWhs == null)
            {
                DisplayToast("Please select an entry warehouse, and try again, Thanks");
                PromptSelectWarehouse();
                return;
            }

            if (CurrentEntryWhs.BinActivat.Equals("Y"))
            {
                // show serial to bin allocation 
                MessagingCenter.Subscribe(this, _ItemQtyToBin, (List<OBIN_Ex> qtyBin) =>
                {
                    MessagingCenter.Unsubscribe<List<OBIN_Ex>>(this, _ItemQtyToBin);

                    if (qtyBin == null) return;
                    if (qtyBin.Count == 0) return;

                    selectedItem.ReceiptQuantity = qtyBin.Sum(x => x.BinQty);
                    selectedItem.Bins = qtyBin;

                    var showList = String.Join("\n", qtyBin.Select(x => $"{x.BinQty:N} -> {x.oBIN.BinCode}"));
                    selectedItem.ShowList = showList;
                    selectedItem.BaseWarehouse = CurrentEntryWhs.WhsCode;

                    AddToList(selectedItem);
                });

                // launch the serial to bin arragement
                await Navigation.PushAsync(new ItemToBinSelectView(
                   CurrentEntryWhs.WhsCode, _ItemQtyToBin, line.ItemCodeDisplay, line.ItemNameDisplay, result, null));
            }
        }

        /// <summary>
        /// Prompt dialog to capture the Qty
        /// </summary>
        /// <param name="selectedPOLine"></param>
        async void CaptureItemReceiptQty(POR1_Ex selectedPOLine)
        {
            try
            {
                //int locId = itemsSource.IndexOf(itemsSource
                //    .Where(x => x.POLine.LineNum.Equals(selectedPOLine.POLine.LineNum)).FirstOrDefault());

                //if (locId < 0)
                //{
                //    DisplayAlert("Unable to local the item in list, please try again. Thanks [x1]");
                //    return;
                //}

                //if (selectedPOLine.receiptQuantity > 0)
                //{
                //    itemsSource[locId].ReceiptQuantity = 0;
                //    itemsSource[locId].Bins = null;
                //    itemsSource[locId].ItemWhsCode = string.Empty;
                //    return;
                //}

                string receiptQty = await new Dialog().DisplayPromptAsync(
                    $"Please input {selectedPOLine.ItemCodeDisplay}",
                    $"{selectedPOLine.ItemNameDisplay}",
                    "OK",
                    "Cancel",
                    "",
                    -1,
                    Keyboard.Numeric,
                    $"1.00");

                var isNumeric = Decimal.TryParse(receiptQty, out decimal m_receiptQty);
                if (!isNumeric) return; // check numeric


                //if (m_receiptQty.Equals(0)) // check zero, if zero assume as reset receipt qty
                //{
                //    // prompt select whs             
                //    itemsSource[locId].ReceiptQuantity = m_receiptQty; // assume as reset to the receipt
                //    return;
                //}

                // else           
                if (m_receiptQty < 0) // check negative
                {
                    DisplayAlert($"The receive quantity ({m_receiptQty:N}) must be numeric and positve, negative are no allowance.\nPlease try again later. Thanks [x2]");
                    return;
                }

                //if (itemsSource[locId].POLine.OpenQty < m_receiptQty) // check open qty
                //{
                //    ShowAlert($"The receive quantity ({m_receiptQty:N}) must be equal or " +
                //        $"smaller than the <= Open qty ({itemsSource[locId].POLine.OpenQty:N}).\nPlease try again later. Thanks [x3]");
                //    return;
                //}


                if (CurrentEntryWhs == null)
                {
                    DisplayToast("Please select an entry warehouse, and try again, Thanks");
                    PromptSelectWarehouse();
                    return;
                }

                // update into the list in memory    
                // non manage item
                selectedItem.ReceiptQuantity = m_receiptQty;
                selectedItem.BaseWarehouse = CurrentEntryWhs.WhsCode;
                AddToList(selectedItem);
            }
            catch (Exception excep)
            {
                Console.Write(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }

        #endregion

        #region PrepareBatchGRN 
        void PrepareBatchGRN(POR1_Ex line)
        {
            try
            {
                if (line.ReceiptQuantity > 0) // reset the line
                {
                    int locid = itemsSource.IndexOf(line);
                    if (locid < 0) return;
                    itemsSource[locid].ReceiptQuantity = 0;
                    itemsSource[locid].Bins = null;
                    itemsSource[locid].ItemWhsCode = string.Empty;
                    itemsSource[locid].Serials = null;
                    itemsSource[locid].SerialsWithBins = null; // keept serial wit bin selection
                    itemsSource[locid].ShowList = string.Empty;
                    itemsSource[locid].BaseWarehouse = string.Empty;
                    return;
                }

                MessagingCenter.Subscribe(this, _CaptureBatches, (List<Batch> batches) =>
                {
                    MessagingCenter.Unsubscribe<List<string>>(this, _CaptureBatches);
                    if (batches == null)
                    {
                        DisplayToast("Captured batches list is empty, please try again later, Thanks[n]");
                        return;
                    }

                    if (batches.Count == 0)
                    {
                        DisplayToast("Captured batches list is empty, please try again later, Thanks. [z]");
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
                DisplayAlert(excep.Message);
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
                    itemsSource[locid].Bins = null;
                    itemsSource[locid].ItemWhsCode = string.Empty;
                    itemsSource[locid].Serials = null;
                    itemsSource[locid].Batches = null;
                    itemsSource[locid].SerialsWithBins = null; // keept serial wit bin selection
                    itemsSource[locid].BatchWithBins = null;
                    itemsSource[locid].ShowList = string.Empty;
                    itemsSource[locid].BaseWarehouse = string.Empty;
                    return;
                }

                // checking
                if (App.Warehouses == null)
                {
                    DisplayAlert("Unable to process the next step, " +
                        "as the warehouse configuration is no load in correctly, please close the app, and try again later, Thanks. ");
                    return;
                }

                if (CurrentEntryWhs == null)
                {
                    DisplayToast("Please select a entry warehouse");
                    PromptSelectWarehouse();
                    return;
                }

                if (CurrentEntryWhs.BinActivat.Equals("Y"))
                {
                    // show serial to bin allocation 
                    MessagingCenter.Subscribe(this, _BatchesToBin, (List<zwaTransferDocDetailsBin> batchBin) =>
                    {
                        MessagingCenter.Unsubscribe<zwaTransferDocDetailsBin>(this, _BatchesToBin);

                        if (batchBin == null) return;
                        if (batchBin.Count == 0) return;

                        string showList = string.Empty;
                        decimal receiptQty1 = 0;
                        batchBin.ForEach(bb =>
                            bb.Bins
                                .ForEach(bin =>
                                {
                                    receiptQty1 += bin.BatchTransQty;
                                    showList += $"{bin.BatchTransQty:N}->{bin.BatchNumber}->{bin.oBIN.BinCode}\n";
                                }));

                        selectedItem.ReceiptQuantity = batchBin.Sum(x => x.ReceiptQty);
                        selectedItem.BatchWithBins = batchBin;
                        selectedItem.ReceiptQuantity = receiptQty1;
                        selectedItem.ShowList = showList;
                        selectedItem.BaseWarehouse = CurrentEntryWhs.WhsCode;
                        AddToList(selectedItem);
                    });

                    // launch the serial to bin arragement
                    Navigation.PushAsync(new BatchToBinView(batches, CurrentEntryWhs.WhsCode, _BatchesToBin,
                        line.ItemCodeDisplay, line.ItemNameDisplay));
                    return;
                }

                // add into the line with serial list capture
                // no to bin warehouse operation
                selectedItem.ReceiptQuantity = batches.Sum(x => x.Qty);
                selectedItem.Batches = batches;
                selectedItem.ShowList = string.Join("\n", batches.Select(x => $"{x.Qty:N}->{x.DistNumber}").ToList());
                selectedItem.BaseWarehouse = CurrentEntryWhs.WhsCode;
                AddToList(selectedItem);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
            }
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
                int locid = itemsSource.IndexOf(line);
                if (locid < 0) return;
                itemsSource[locid].ReceiptQuantity = 0;
                itemsSource[locid].Bins = null;
                itemsSource[locid].ItemWhsCode = string.Empty;
                itemsSource[locid].Serials = null;
                itemsSource[locid].SerialsWithBins = null; // keept serial wit bin selection
                itemsSource[locid].ShowList = string.Empty;
                itemsSource[locid].BaseWarehouse = string.Empty;
                return;
            }

            MessagingCenter.Subscribe(this, _CaptureSerials, (List<string> serials) =>
            {
                MessagingCenter.Unsubscribe<List<string>>(this, _CaptureSerials);
                if (serials == null)
                {
                    DisplayToast("Captured serials list is empty, please try again later, Thanks[n]");
                    return;
                }

                if (serials.Count == 0)
                {
                    DisplayToast("Captured serials list is empty, please try again later, Thanks. [z]");
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
                    DisplayToast("Unable to process the next step, " +
                        "as the warehouse configuration is no load in correctly, " +
                        "please close the app, and try again later, Thanks. ");
                    return;
                }

                if (CurrentEntryWhs == null)
                {
                    DisplayToast("Please select an entry warehouse, and try again, Thanks");
                    PromptSelectWarehouse();
                    return;
                }

                if (CurrentEntryWhs.BinActivat.Equals("Y"))
                {
                    // show serial to bin allocation 
                    MessagingCenter.Subscribe(this, _SerialsToBin, (List<zwaTransferDocDetailsBin> serialBin) =>
                    {
                        if (serialBin == null) return;
                        if (serialBin.Count == 0) return;

                        selectedItem.ReceiptQuantity = serialBin.Count;
                        selectedItem.SerialsWithBins = serialBin;
                        var showList = String.Join("\n", serialBin.Select(x => x.Serial + "->" + x.BinCode));
                        selectedItem.ShowList = showList;
                        selectedItem.BaseWarehouse = CurrentEntryWhs.WhsCode;
                        AddToList(selectedItem);
                    });

                    // launch the serial to bin arragement
                    Navigation.PushAsync(new SerialToBinView(serials,
                        line.ItemCodeDisplay, line.ItemNameDisplay, CurrentEntryWhs.WhsCode, _SerialsToBin));
                    return;
                }

                // add into the line with serial list capture
                int receiptQty = serials.Count;
                int lineLocId = itemsSource.IndexOf(line);

                selectedItem.ReceiptQuantity = receiptQty;
                selectedItem.Serials = serials;
                selectedItem.ShowList = String.Join("\n", serials);
                selectedItem.BaseWarehouse = CurrentEntryWhs.WhsCode;

                AddToList(selectedItem);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }
        #endregion

        /// <summary>
        /// Create a new line and return
        /// </summary>
        /// <returns></returns>
        POR1_Ex CreateNewLine()
        {
            try
            {
                var newPoLine = new POR1_Ex { POLine = new POR1() };
                newPoLine.POLine.ItemCode = CurrentItem.ItemCode;
                newPoLine.POLine.Dscription = CurrentItem.ItemName;

                newPoLine.PO = new OPOR_Ex();
                newPoLine.PO.PO = new OPOR();

                newPoLine.PO.PO.DocEntry = -1;
                newPoLine.PO.PO.DocNum = -1;
                newPoLine.PO.PO.CardCode = this.cardCode;
                newPoLine.PO.PO.CardName = this.cardName;
                return newPoLine;
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
        void AddToList(POR1_Ex newLine)
        {
            if (itemsSource == null) itemsSource = new List<POR1_Ex>();
            itemsSource.Add(selectedItem); // add into the itemsource       
            RefreshListview();
        }

        /// <summary>
        /// redbing the list view
        /// </summary>
        void RefreshListview()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<POR1_Ex>(itemsSource);
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
        /// Save / cretae the GRPO without PO
        /// </summary>
        async void Save()
        {
            try
            {
                if (itemsSource == null)
                {
                    DisplayAlert("The item list if empty, please try again later. Thanks");
                    return;
                }

                var receivedList = itemsSource.Where(x => x.ReceiptQuantity > 0).ToList();
                if (receivedList == null)
                {
                    DisplayAlert("There is no items receipt entered, please try again later. Thanks");
                    return;
                }

                if (receivedList.Count == 0)
                {
                    DisplayAlert("There is no items receipt entered, please try again later. Thanks");
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
                        SourceDocNum = -1,
                        SourceDocEntry = -1,
                        SourceBaseEntry = -1,
                        SourceBaseLine = -1,
                        Warehouse = line.BaseWarehouse,
                        LineGuid = lineGuid
                    });

                    List<zwaItemBin> binList = line.GetList(groupingGuid, lineGuid);
                    if (binList.Count > 0)
                    {
                        lineBinList.AddRange(binList);
                    }
                }

                // create the header property
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
        //    var callerAddress = "CreateGRPO_DocCheck_ManualPO";
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
