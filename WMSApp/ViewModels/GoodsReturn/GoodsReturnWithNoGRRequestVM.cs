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
using WMSApp.Models.SAP;
using WMSApp.Models.Share;
using WMSApp.Models.Transfer1;
using WMSApp.ViewModels.Transfer1;
using WMSApp.Views.Share;
using WMSApp.Views.Transfer1;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace WMSApp.ViewModels.GoodsReturn
{
    public class GoodsReturnWithNoGRRequestVM : INotifyPropertyChanged
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

        string exitWhs;
        public string ExitWhs
        {
            get => exitWhs;
            set
            {
                if (exitWhs == value) return;
                exitWhs = value;
                OnPropertyChanged(nameof(ExitWhs));
            }
        }

        PRR1_Ex selectedItem;
        public PRR1_Ex SelectedItem
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

        List<PRR1_Ex> itemsSource;
        public ObservableCollection<PRR1_Ex> ItemsSource { get; set; }
        #endregion

        NNM1[] DocSeries { get; set; } = null;
        zwaRequest currentRequest { get; set; } = null;
        OWHS CurrentExitWhs { get; set; } = null;
        OITM CurrentItem { get; set; } = null;
        INavigation Navigation { get; set; } = null;

        readonly string _PromptWhs = "_PromptWhs20201110T1021_MGR";
        readonly string _PromptBp = "_PromptBp20201110T1021_MGR";
        readonly string _PromptScanCode = "_PromptScanCode20201110T1021_MGR";
        readonly string _PickSerialFromBin = "_PickSerialFromBin_20201118T1438_MGR";
        readonly string _PickSerialListView = "_PickSerialListView_20201118T1448_MGR";
        readonly string _PickBatchesFromBin = "_PickBatchesFromBin_20201118T1455_MGR";
        readonly string _PickBatchFromWhs = "_PickBatchFromWhs_20201118T1457_MGR";
        readonly string _PickItemQtyFromBin = "_PickItemQtyFromBin_20201118T1506_MGR";

        readonly string _PopScreenAddr = "_PopScreenAddr_201207GoodsReturnwNoSource";

        // the constructor
        public GoodsReturnWithNoGRRequestVM(INavigation navigation)
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
        public void RemoveLine(PRR1_Ex removeLine)
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
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "GoodsReturnRequest");
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
                                x.PRR.CardCode = selectedBp.CardCode;
                                x.PRR.CardName = selectedBp.CardName;
                                x.OnPropertyChanged("CardName");
                                x.OnPropertyChanged("CardCode");
                            });

                            CurrentBp = selectedBp;
                            CardName = CurrentBp.CardName;
                            CardCode = CurrentBp.CardCode;
                            RefreshListview();
                        }
                        return;
                    }

                    // else if same vender
                    CurrentBp = selectedBp;
                    CardName = CurrentBp.CardName;
                    CardCode = CurrentBp.CardCode;

                });

                await Navigation.PushPopupAsync(new SelectVendorPopUpView(_PromptBp, "Select a Bp", Color.LightSkyBlue));
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

                    CurrentExitWhs = selectedWhs;
                    ExitWhs = CurrentExitWhs.WhsCode;
                });

                Navigation.PushPopupAsync(new PickWarehousePopUpView(_PromptWhs,
                    "Select an exit warehouse", Color.Yellow));
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
                if (itemsSource == null) itemsSource = new List<PRR1_Ex>();

                SelectedItem = CreateNewLine();
                if (selectedItem == null)
                {
                    DisplayAlert($"There is issue to create the item code {itemCode}. Please try again later, Thanks [I]");
                    return;
                }

                if (CurrentItem.ManSerNum.Equals("Y"))
                {
                    PrepareSerialPrr(selectedItem);
                    return;
                }

                if (CurrentItem.ManBtchNum.Equals("Y"))
                {
                    PrepareBatchPrr(selectedItem);
                    return;
                }

                // ELSE handle no manage item
                PrepareNonManagePrr(selectedItem);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }

        #region PrepareNonManageDo
        void PrepareNonManagePrr(PRR1_Ex line)
        {

            if (line.exitQuantity > 0)
            {
                ResetLine(line);
                return;
            }

            if (CurrentExitWhs == null)
            {
                DisplayToast("Please select an exit warehouse, and try again, Thanks");
                PromptSelectWarehouse();
                return;
            }

            if (CurrentExitWhs.BinActivat.Equals("Y"))
            {
                HandlerNonManageItemBin(line); // handler bin warehouse
                return;
            }

            // ELSE
            CaptureItemExitQty(line); // handle normal qty capture
        }

        /// <summary>
        /// Handler non manage item
        /// </summary>
        async void HandlerNonManageItemBin(PRR1_Ex line) // Handler bin warehouse
        {
            //reset the qty
            if (line.exitQuantity > 0)
            {
                ResetLine(line);
                return;
            }

            if (CurrentExitWhs == null)
            {
                DisplayToast("Unable to process the next step, without specify the exit warehouse " +
                    "please select available warehouse for this transaction, Thanks. ");
                PromptSelectWarehouse();
                return;
            }


            if (!CurrentExitWhs.BinActivat.Equals("Y"))
            {
                // program flow out 
                return;
            }

            // show serial to bin allocation 
            MessagingCenter.Subscribe(this, _PickItemQtyFromBin, (List<OIBQ_Ex> qtyBin) =>
            {
                MessagingCenter.Unsubscribe<List<OIBQ_Ex>>(this, _PickItemQtyFromBin);
                if (qtyBin == null)
                {
                    DisplayToast("Pick item qty from bin operation cancel");
                    return;
                }

                if (qtyBin.Count == 0)
                {
                    DisplayToast("Pick item qty from bin operation cancel");
                    return;
                }

                line.ExitQuantity = qtyBin.Sum(x => x.TransferQty);
                line.ItemQtyInBin = qtyBin;
                line.ItemWhsCode = CurrentExitWhs.WhsCode;

                var showList = $"Frm Whs {CurrentExitWhs.WhsCode} ->\n" +
                                String.Join("\n", qtyBin.Select(x => $"{x.BinCode:N} -> {x.TransferQty:N}"));

                line.ShowList = showList;
                AddToList(line);
            });

            // launch the item qyt from bin arragement
            await Navigation.PushAsync(
                new BinItemListView(
                    _PickItemQtyFromBin, line.ItemCodeDisplay,
                    line.ItemNameDisplay, CurrentExitWhs.WhsCode, 0));
        }

        /// <summary>
        /// Prompt dialog to capture the Qty
        /// </summary>
        /// <param name="selectedPOLine"></param>
        async void CaptureItemExitQty(PRR1_Ex line)
        {
            try
            {
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
                    $"1");

                var isNumeric = Decimal.TryParse(exitQty, out decimal m_exitQty);
                if (!isNumeric) return; // check numeric
                if (m_exitQty.Equals(0)) return; // check zero, if zero assume as reset receipt qty                

                // else           
                if (m_exitQty < 0) // check negative
                {
                    DisplayToast($"The exit quantity ({m_exitQty:N}) must be numeric and positve," +
                        $" negative are no allowance.\nPlease try again later. Thanks [x2]");
                    return;
                }

                #region reserved
                //if (itemsSource[locId].POLine.OpenQty < m_exitQty) // check open qty
                //{
                //    ShowAlert($"The receive quantity ({m_exitQty:N}) must be equal or " +
                //        $"smaller than the <= Open qty ({itemsSource[locId].POLine.OpenQty:N}).\nPlease try again later. Thanks [x3]");
                //    return;
                //}
                #endregion

                // update into the list in memory    
                // non manage item
                line.ExitQuantity = m_exitQty;
                line.ItemWhsCode = CurrentExitWhs.WhsCode;
                line.ShowList = $"{CurrentExitWhs.WhsCode} -> {m_exitQty:N}";
                AddToList(line);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }

        #endregion

        #region PrepareBatchPrr 
        void PrepareBatchPrr(PRR1_Ex line)
        {
            try
            {
                if (line.exitQuantity > 0)
                {
                    ResetLine(line);
                    return;
                }

                // checking

                if (CurrentExitWhs == null)
                {
                    DisplayToast("Unable to process the next step, without specify the exit warehouse " +
                        "please select available warehouse for this transaction, Thanks. ");
                    PromptSelectWarehouse();
                    return;
                }

                #region batch picking from warehouse
                if (!CurrentExitWhs.BinActivat.Equals("Y"))
                {

                    MessagingCenter.Subscribe(this, _PickBatchFromWhs, (List<OBTQ_Ex> batchInWhs) =>
                    {
                        MessagingCenter.Unsubscribe<List<OBTQ_Ex>>(this, _PickBatchFromWhs);
                        if (batchInWhs == null)
                        {
                            DisplayToast("Pick batch from warehouse operation cancel");
                            return;
                        }
                        if (batchInWhs.Count == 0)
                        {
                            DisplayToast("Pick batch from warehouse operation cancel");
                            return;
                        }

                        line.ExitQuantity = batchInWhs.Sum(x => x.TransferBatchQty); ;
                        line.BatchesInWhs = batchInWhs;
                        line.ItemWhsCode = CurrentExitWhs.WhsCode;
                        line.ShowList =
                                 $"Frm Whs {CurrentExitWhs.WhsCode} ->\n" +
                                 string.Join("\n", batchInWhs.Select(x => $"{x.DistNumber} -> {x.TransferBatchQty:N}").ToList());

                        AddToList(line);
                    });

                    Navigation.PushAsync(
                        new BatchListView(_PickBatchFromWhs,
                                        line.ItemCodeDisplay, line.ItemNameDisplay, CurrentExitWhs.WhsCode, 0));
                    return;
                }
                #endregion

                #region batch picking from bin warehouse                
                MessagingCenter.Subscribe(this, _PickBatchesFromBin, (List<OBBQ_Ex> pickedBatchInBins) =>
                {
                    MessagingCenter.Unsubscribe<List<OBBQ_Ex>>(this, _PickBatchesFromBin);
                    if (pickedBatchInBins == null)
                    {
                        DisplayToast("Pick batch from bin operation cancel");
                        return;
                    }

                    if (pickedBatchInBins.Count == 0)
                    {
                        DisplayToast("Pick batch from bin operation cancel");
                        return;
                    }

                    string showList = $"Frm Whs {CurrentExitWhs.WhsCode} ->\n";
                    showList += string.Join("\n",
                        pickedBatchInBins.Select(x => $"{x.BinCode} -> {x.DistNumber} -> {x.TransferBatchQty:N}"));

                    line.ExitQuantity = pickedBatchInBins.Sum(x => x.TransferBatchQty);
                    line.BatchesInBin = pickedBatchInBins;
                    line.ItemWhsCode = CurrentExitWhs.WhsCode;
                    line.ShowList = showList;
                    AddToList(line);
                });

                // launch the serial to bin arragement
                Navigation.PushAsync(new BinBatchListView(_PickBatchesFromBin,
                    line.ItemCodeDisplay, line.ItemNameDisplay, CurrentExitWhs.WhsCode, 0));

                #endregion
            }
            catch (Exception excep)
            {
                Console.Write(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }
        #endregion

        #region PrepareSerialPrr => serial manage handle step
        /// <summary>
        ///  Prepare the serial to bin allocation screen
        /// </summary>
        async void PrepareSerialPrr(PRR1_Ex line)
        {
            try
            {
                if (line.exitQuantity > 0)
                {
                    ResetLine(line);
                    return;
                }

                if (CurrentExitWhs == null)
                {
                    DisplayToast("Unable to process the next step, without specify the exit warehouse " +
                        "please select available warehouse for this transaction, Thanks. ");
                    PromptSelectWarehouse();
                    return;
                }

                #region pick from warehouse serial
                if (!CurrentExitWhs.BinActivat.Equals("Y"))
                {
                    MessagingCenter.Subscribe(this, _PickSerialListView, (List<OSRQ_Ex> picked_serials) =>
                    {
                        MessagingCenter.Unsubscribe<List<OSRQ_Ex>>(this, _PickSerialListView);
                        if (picked_serials == null)
                        {
                            DisplayToast("Pick serial from warehouse operation cancel");
                            return;
                        }

                        if (picked_serials.Count == 0)
                        {
                            DisplayToast("Pick serial from warehouse operation cancel");
                            return;
                        }

                        var showlist = $"Frm Whs {CurrentExitWhs.WhsCode} ->\n" + String.Join("\n", picked_serials.Select(x => x.DistNumber));
                        line.ExitQuantity = picked_serials.Count;
                        line.SerialsWhs = picked_serials;
                        line.ItemWhsCode = CurrentExitWhs.WhsCode;
                        line.ShowList = showlist;

                        AddToList(line);
                    });

                    await Navigation.PushAsync(new SerialListView(_PickSerialListView, line.ItemCodeDisplay, line.ItemNameDisplay,
                        CurrentExitWhs.WhsCode, 0, null));
                    return;
                }
                #endregion

                #region pick serial from bin warehouse                
                MessagingCenter.Subscribe(this, _PickSerialFromBin, (List<OSBQ_Ex> serialBins) =>
                {
                    MessagingCenter.Unsubscribe<List<OSBQ_Ex>>(this, _PickSerialFromBin);
                    if (serialBins == null)
                    {
                        DisplayToast("Pick serial from bin operation cancel");
                        return;
                    }

                    if (serialBins.Count == 0)
                    {
                        DisplayToast("Pick serial from bin operation cancel");
                        return;
                    }

                    line.ExitQuantity = serialBins.Count;
                    line.SerialInBin = serialBins;
                    line.ItemWhsCode = CurrentExitWhs.WhsCode;

                    var showList = $"Frm Whs {CurrentExitWhs.WhsCode} ->\n" +
                                String.Join("\n", serialBins.Select(x => $"{x.BinCode} -> {x.DistNumber}"));

                    line.ShowList = showList;
                    AddToList(line);
                });

                await Navigation.PushAsync(new BinSerialListView(_PickSerialFromBin,
                    line.ItemCodeDisplay, line.ItemNameDisplay, CurrentExitWhs.WhsCode, 0, null));
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
        void ResetLine(PRR1_Ex line)
        {
            if (line.exitQuantity > 0)
            {
                int locid = itemsSource.IndexOf(line);
                if (locid < 0) return;
                itemsSource[locid].ExitQuantity = 0;

                DisplayToast($"{itemsSource[locid].PRR.DocNum}#, " +
                    $"Item line {itemsSource[locid].ItemCodeDisplay}\n{itemsSource[locid].ItemNameDisplay} reset.");
                return;
            }
        }

        /// <summary>
        /// Create a new line and return
        /// </summary>
        /// <returns></returns>
        PRR1_Ex CreateNewLine()
        {
            try
            {
                var newline = new PRR1_Ex();
                newline.ItemCode = CurrentItem.ItemCode;
                newline.Dscription = CurrentItem.ItemName;
                newline.PRR = new OPRR_Ex();
                newline.PRR.DocEntry = -1;
                newline.PRR.DocNum = -1;
                newline.PRR.CardCode = this.cardCode;
                newline.PRR.CardName = this.cardName;
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
        void AddToList(PRR1_Ex newLine)
        {
            if (itemsSource == null)
            {
                itemsSource = new List<PRR1_Ex>();
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
            ItemsSource = new ObservableCollection<PRR1_Ex>(itemsSource);
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

                var receivedList = itemsSource.Where(x => x.ExitQuantity > 0).ToList();
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

                UserDialogs.Instance.ShowLoading("Awating Create Goods Return Creation ...");

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
                        Qty = line.exitQuantity,
                        SourceCardCode = line.PRR.CardCode,
                        SourceDocNum = line.PRR.DocNum,
                        SourceDocEntry = line.DocEntry,
                        SourceDocBaseType = Convert.ToInt32(line.ObjType), // from purchase order
                        SourceBaseEntry = line.DocEntry,
                        SourceBaseLine = line.LineNum,
                        Warehouse = (string.IsNullOrWhiteSpace(line.ItemWhsCode)) ? line.BaseWarehouse : line.ItemWhsCode, 
                        LineGuid = lineGuid
                    }); ;

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
        //    var callerAddress = "CreateGoodsReturn_Manual_DocCheck";
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
