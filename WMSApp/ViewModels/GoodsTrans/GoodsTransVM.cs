using Acr.UserDialogs;
using DbClass;
using Newtonsoft.Json;
using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WMSApp.Class;
using WMSApp.Class.Helper;
using WMSApp.Interface;
using WMSApp.Models.GIGR;
using WMSApp.Models.GRPO;
using WMSApp.Models.SAP;
using WMSApp.Models.Share;
using WMSApp.Models.Transfer1;
using WMSApp.ViewModels.Transfer1;
using WMSApp.Views.Share;
using WMSApp.Views.Transfer1;
using WMSWebAPI.Dtos;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace WMSApp.ViewModels.GIGR
{
    public class GoodsTransVM : INotifyPropertyChanged
    {
        #region Screen property
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string PropertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        public Command CmdSaveUpdate { get; set; }
        public Command CmdClose { get; set; }
        public Command CmdChangeDefaultWhs { get; set; }
        public Command CmdStartScan { get; set; }
        public Command CmdStartSearchAdd { get; set; }
        public Command CmdSelectWhs { get; set; }
        public Command CmdCaptureSignature { get; set; }

        string pageTitle;
        public string PageTitle
        {
            get => pageTitle;
            set
            {
                if (pageTitle == value) return;
                pageTitle = value;
                OnPropertyChanged(nameof(PageTitle));
            }
        }

        string warehouseTitle;
        public string WarehouseTitle
        {
            get => warehouseTitle;
            set
            {
                if (warehouseTitle == value) return;
                warehouseTitle = value;
                OnPropertyChanged(nameof(WarehouseTitle));
            }
        }

        OITM_Ex selectedItem;
        public OITM_Ex SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;
                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        List<string> docSeriesItemsSource { get; set; } = null;
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

        bool signatureEnable;
        public bool SignatureEnable
        {
            get => signatureEnable;
            set
            {
                if (signatureEnable == value) return;
                signatureEnable = value;
                OnPropertyChanged(nameof(SignatureEnable));
            }
        }

        List<OITM_Ex> itemsSource { get; set; } = null;
        public ObservableCollection<OITM_Ex> ItemsSource { get; set; }
        #endregion

        #region Private declaration
        OITM CurrentOItm { get; set; } = null;
        zwaRequest CurrentRequest { get; set; } = null;
        INavigation Navigation { get; set; } = null;
        OWHS SelectedWhs { get; set; } = null; // for bin detection and selection
        string TransType { get; set; } = string.Empty;
        string SignaturePicturePath { get; set; } = string.Empty;

        #endregion

        #region Static declartion
        readonly string _GISerialBinListView = "_SerialBinListView_20201115111";
        readonly string _GISerialListView = "_GISerialListView_20201115T1158";
        readonly string _selectWhsAddrs = "_SelectWhsAddrs20201114T1834";
        readonly string _GIBatchBinListView = "_GIBatchBinListView_20201115T1215";
        readonly string _GIBatchListView = "_GIBatchListView_20201115T1238";
        readonly string _GIItemQtyBin = "_GIItemQtyBin_202011T1247";

        readonly string _GRSerialBinOpr = "_GRSerialBinOpr_202011151324";
        readonly string _CaptureSerials = "_CaptureSerials_20201115T1330";
        readonly string _GRCollectionBatches = "_GRCollectionBatches_20201115T1409";
        readonly string _GRBatchBinAllocation = "_GRBatchBinAllocation_20201115T1412";
        readonly string _ItemToBinSelect = "_ItemToBinSelect_20201115T1656";

        readonly string _CaptureSignature = "_CaptureSignature";
        readonly string _GI = "GI";
        readonly string _PopScreenAddr = "_PopScreenAddr_GoodsTrans201207";

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="transType"></param>
        /// <param name="navigation"></param>
        public GoodsTransVM(string transType, INavigation navigation)
        {
            TransType = transType;
            Navigation = navigation;
            PageTitle = (TransType.Equals(_GI)) ? "Goods Issue" : "Goods Receipt";

            InitCmd();
            PromptWarehouseSelectionAsync();
            LoadDocSeries();

            SignatureEnable = true;
        }

        /// <summary>
        /// start a task to capture the general entries warehouse
        /// </summary>
        /// <returns></returns>
        async void PromptWarehouseSelectionAsync()
        {
            if (App.Warehouses == null) return;
            MessagingCenter.Subscribe<OWHS>(this, _selectWhsAddrs, (OWHS selectedWhs) =>
            {
                if (selectedWhs == null)
                {
                    ShowToast("Selection of warehouse cancel, and not saved. ");
                    return;
                }

                SelectedWhs = selectedWhs;
                WarehouseTitle = (TransType.Equals(_GI)) ?
                        $"Exit Warehouse {selectedWhs.WhsCode}" :
                        $"Entry Warehouse {selectedWhs.WhsCode}";
            });

            await Navigation.PushPopupAsync(
                new PickWarehousePopUpView(_selectWhsAddrs, $"Select {TransType} warehouse", Color.DarkOrange));
        }

        /// <summary>
        /// Init the view command
        /// </summary>
        void InitCmd()
        {
            CmdSaveUpdate = new Command(SaveAndUpdate);
            CmdChangeDefaultWhs = new Command(PromptWarehouseSelectionAsync);
            CmdClose = new Command(() => Navigation.PopAsync());
            CmdStartSearchAdd = new Command(HandlerManualItemCodeEntry);
            CmdStartScan = new Command(HandlerStartScan);
            CmdSelectWhs = new Command(PromptWarehouseSelectionAsync);
            CmdCaptureSignature = new Command(LaunchSignaturePad);
        }

        /// <summary>
        /// Launch the signature pad
        /// </summary>
        async void LaunchSignaturePad()
        {
            try
            {
                MessagingCenter.Subscribe(this, _CaptureSignature, (Stream stream) =>
                {

                    MessagingCenter.Unsubscribe<Stream>(this, _CaptureSignature);
                    string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "sig.png");
                    using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fileStream);
                    }
                    ShowToast($"Signature saved as {fileName}");
                    SignaturePicturePath = fileName;
                });
                await Navigation.PushAsync(new CaptureSignatureView(_CaptureSignature));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowMessage(excep.Message);
            }
        }

        /// <summary>
        /// Prompt user to enter the item code
        /// </summary>
        async void HandlerManualItemCodeEntry()
        {
            string itemCode = await new Dialog().DisplayPromptAsync("Item Code",
                  "Enter item code to search",
                  "Search",
                  "Cancel",
                  "",
                  -1,
                  Keyboard.Text, "");
            HandlerAddItemCode(itemCode);
        }

        /// <summary>
        ///  Start a scanner view and return a string
        /// </summary>
        void HandlerStartScan()
        {
            const string returnAddress = "HandlerStartScan";
            MessagingCenter.Subscribe(this, returnAddress, (string itemCode) =>
            {
                MessagingCenter.Unsubscribe<string>(this, returnAddress);
                if (itemCode.Equals("cancel")) return;
                if (string.IsNullOrWhiteSpace(itemCode)) return;

                HandlerAddItemCode(itemCode);
            });

            // start a general scan view
            Navigation.PushAsync(new CameraScanView(returnAddress));
        }

        /// <summary>
        /// handler the pass in code
        /// </summary>
        /// <param name="itemCode"></param>
        async void HandlerAddItemCode(string itemCode)
        {
            try
            {
                // avoid duplicated line add in
                if (itemsSource != null && itemsSource.Count > 0)
                {
                    var foundDup = itemsSource.Where(x => x.Item.ItemCode.Equals(itemCode)).FirstOrDefault();
                    if (foundDup != null)
                    {
                        ShowToast($"The scan in code {itemCode} " +
                            $"is found duplicated, please remove the exsiting line, and add again, Thanks");
                        return;
                    }
                }

                UserDialogs.Instance.ShowLoading("A moment ...");
                // query serve tp check valid of item                 
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
                    ShowToast($"The code {itemCode}\nServer replied: {content}\nPlease try other again, Thanks.");
                    return;
                }

                var repliedCio = JsonConvert.DeserializeObject<Cio>(content);
                if (repliedCio == null)
                {
                    ShowToast($"There is issue reading infomation from server, please try again later, Thanks [C0]");
                    return;
                }

                if (repliedCio.Item == null)
                {
                    ShowToast($"There is issue reading infomation from server, please try again later, Thanks [I0]");
                    return;
                }

                if (SelectedWhs == null)
                {
                    ShowToast($"Please select a warehouse for {TransType}, please try again. Thanks[N-WHS]");
                    return;
                }

                // capture the needed qty
                CurrentOItm = repliedCio.Item;

                //  decimal transQty = await CaptureItemQuantity(repliedCio.Item);
                var pickedIemInfo = new TransferDataM
                {
                    ItemCode = CurrentOItm.ItemCode,
                    ItemName = CurrentOItm.ItemName,
                    ItemManagedBy = (CurrentOItm.ManBtchNum.Equals("Y")) ? "Batch" :
                                    (CurrentOItm.ManSerNum.Equals("Y")) ? "Serial" : "None",
                    FromWhsCode = SelectedWhs.WhsCode,
                    BinActivated = SelectedWhs.BinActivat.Equals("Y") ? "Yes" : "No",
                    RequestedQty = -1
                };

                if (TransType.Equals(_GI)) // handle exit warehouse
                {
                    HandlerGILine(pickedIemInfo);
                    return;
                }

                HandlerGRLine(pickedIemInfo);
                return;
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowMessage(excep.Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        #region GI operation
        /// <summary>
        /// Handler the item for GI
        /// </summary>
        /// <param name="item"></param>
        void HandlerGILine(TransferDataM pickedIemInfo)
        {
            try
            {
                // base on warehouse bin or non bin
                // based on transaction type - gi or gr 
                // based on item manage type 
                //pickedIemInfo.RequestedQty = await CaptureItemQuantity(CurrentOItm);

                switch (pickedIemInfo.ItemManagedBy)
                {
                    case "Serial":
                        {
                            if (pickedIemInfo.BinActivated.Equals("Yes"))
                            {
                                HandlerGISerialBinOpr(pickedIemInfo);
                                return;
                            }
                            HandlerGISerialOpr(pickedIemInfo);
                            break;
                        }
                    case "Batch":
                        {
                            if (pickedIemInfo.BinActivated.Equals("Yes"))
                            {
                                HanlderGIBatchBinOpr(pickedIemInfo);
                                return;
                            }
                            // handler non bin Batch 
                            HanlderGIBatchOpr(pickedIemInfo);
                            break;
                        }
                    case "None":
                        {
                            if (pickedIemInfo.BinActivated.Equals("Yes"))
                            {
                                HanlderGIQtyBinOpr(pickedIemInfo);
                                return;
                            }
                            // handler non bin serial 
                            HanlderGIQtyOpr(pickedIemInfo);
                            break;
                        }
                }
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowMessage(excep.Message);
            }
        }

        /// <summary>
        /// Handler GI Serial Bin Opr
        /// Show the warehouse serial in bin 
        /// for user to pick
        /// retrieve and save as line
        /// </summary>
        async void HandlerGISerialBinOpr(TransferDataM pickedIemInfo)
        {
            try
            {
                MessagingCenter.Subscribe(this, _GISerialBinListView, (List<OSBQ_Ex> binSerials) =>
                {
                    MessagingCenter.Unsubscribe<List<OSBQ_Ex>>(this, _GISerialBinListView);
                    if (binSerials == null)
                    {
                        ShowToast("Operation cancel, no serial in bin selected, Please again later. Thanks");
                        return;
                    }

                    if (binSerials.Count == 0)
                    {
                        ShowToast("Operation cancel, no serial in bin selected, Please again later. Thanks");
                        return;
                    }

                    var showList = string.Join("\n", binSerials.Select(x => $"{x.BinCode}->{x.DistNumber}").ToList());

                    AddItem(
                        new OITM_Ex
                        {
                            Item = CurrentOItm,
                            SerialsInBin = binSerials,
                            ItemWhsCode = SelectedWhs.WhsCode,
                            TransQty = binSerials.Count,
                            TransType = TransType,
                            ShowList = showList,
                        });
                });

                await Navigation.PushAsync(new BinSerialListView(_GISerialBinListView,
                    pickedIemInfo.ItemCode, pickedIemInfo.ItemName,
                    pickedIemInfo.FromWhsCode,
                    0, null));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowMessage(excep.Message);
            }
        }

        /// <summary>
        /// Handler GI Serial Opr
        /// </summary>
        async void HandlerGISerialOpr(TransferDataM pickedIemInfo)
        {
            try
            {
                MessagingCenter.Subscribe(this, _GISerialListView, (List<OSRQ_Ex> serials) =>
                {
                    MessagingCenter.Unsubscribe<List<OSRQ_Ex>>(this, _GISerialListView);
                    if (serials == null)
                    {
                        ShowToast("Operation cancel, no serial in bin selected, Please again later. Thanks");
                        return;
                    }

                    if (serials.Count == 0)
                    {
                        ShowToast("Operation cancel, no serial in bin selected, Please again later. Thanks");
                        return;
                    }

                    var showList = string.Join("\n", serials.Select(x => $"{x.DistNumber}").ToList());

                    AddItem(
                        new OITM_Ex
                        {
                            Item = CurrentOItm,
                            Serials = serials,
                            ItemWhsCode = SelectedWhs.WhsCode,
                            TransQty = serials.Count,
                            TransType = TransType,
                            ShowList = showList,
                        });
                });

                await Navigation.PushAsync(new SerialListView(_GISerialListView,
                    pickedIemInfo.ItemCode, pickedIemInfo.ItemName,
                    pickedIemInfo.FromWhsCode,
                    0, null));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowMessage(excep.Message);
            }
        }

        /// <summary>
        /// Handler GI batch operation
        /// </summary>
        /// <param name="pickedIemInfo"></param>
        async void HanlderGIBatchBinOpr(TransferDataM pickedIemInfo)
        {
            try
            {
                MessagingCenter.Subscribe(this, _GIBatchBinListView, (List<OBBQ_Ex> batchInBin) =>
                {
                    MessagingCenter.Unsubscribe<List<OBBQ_Ex>>(this, _GIBatchBinListView);
                    if (batchInBin == null)
                    {
                        ShowToast("Operation cancel, no batch in bin selected, Please again later. Thanks");
                        return;
                    }

                    if (batchInBin.Count == 0)
                    {
                        ShowToast("Operation cancel, no batch in bin selected, Please again later. Thanks");
                        return;
                    }

                    var showList = string.Join("\n", batchInBin.Select(x => $"{x.BinCode} -> {x.DistNumber} -> {x.TransferBatchQty}").ToList());

                    AddItem(
                        new OITM_Ex
                        {
                            Item = CurrentOItm,
                            BatchInBin = batchInBin,
                            ItemWhsCode = SelectedWhs.WhsCode,
                            TransQty = batchInBin.Sum(x => x.TransferBatchQty),
                            TransType = TransType,
                            ShowList = showList,
                        });
                });

                await Navigation.PushAsync(
                    new BinBatchListView(_GIBatchBinListView, pickedIemInfo.ItemCode, pickedIemInfo.ItemName,
                    pickedIemInfo.FromWhsCode,
                    0));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowMessage(excep.Message);
            }
        }

        /// <summary>
        /// _GRBatchListView
        /// </summary>
        /// <param name="pickedIemInfo"></param>
        async void HanlderGIBatchOpr(TransferDataM pickedIemInfo)
        {
            try
            {
                MessagingCenter.Subscribe(this, _GIBatchListView, (List<OBTQ_Ex> batches) =>
                {
                    MessagingCenter.Unsubscribe<List<OBTQ_Ex>>(this, _GIBatchListView);
                    if (batches == null)
                    {
                        ShowToast("Operation cancel, no batch selected, Please again later. Thanks");
                        return;
                    }

                    if (batches.Count == 0)
                    {
                        ShowToast("Operation cancel, no batch selected, Please again later. Thanks");
                        return;
                    }

                    var showList = string.Join("\n", batches.Select(x => $"{x.DistNumber} -> {x.TransferBatchQty}").ToList());

                    AddItem(
                        new OITM_Ex
                        {
                            Item = CurrentOItm,
                            Batches = batches,
                            ItemWhsCode = SelectedWhs.WhsCode,
                            TransQty = batches.Sum(x => x.TransferBatchQty),
                            TransType = TransType,
                            ShowList = showList,
                        });
                });

                await Navigation.PushAsync(
                    new BatchListView(_GIBatchListView, pickedIemInfo.ItemCode, pickedIemInfo.ItemName,
                    pickedIemInfo.FromWhsCode,
                    0));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowMessage(excep.Message);
            }
        }

        /// <summary>
        /// Hanlder GI Qty Bin Opr
        /// </summary>
        async void HanlderGIQtyBinOpr(TransferDataM pickedIemInfo)
        {
            try
            {
                MessagingCenter.Subscribe(this, _GIItemQtyBin, (List<OIBQ_Ex> itemQtyInBin) =>
                {
                    MessagingCenter.Unsubscribe<List<OIBQ_Ex>>(this, _GIItemQtyBin);
                    if (itemQtyInBin == null)
                    {
                        ShowToast("Operation cancel, no bin selected, Please again later. Thanks");
                        return;
                    }

                    if (itemQtyInBin.Count == 0)
                    {
                        ShowToast("Operation cancel, no bin selected, Please again later. Thanks");
                        return;
                    }

                    var showList = string.Join("\n", itemQtyInBin.Select(x => $"{x.BinCode} -> {x.TransferQty:N}").ToList());

                    AddItem(
                        new OITM_Ex
                        {
                            Item = CurrentOItm,
                            ItemQtyBins = itemQtyInBin,
                            ItemWhsCode = SelectedWhs.WhsCode,
                            TransQty = itemQtyInBin.Sum(x => x.TransferQty),
                            TransType = TransType,
                            ShowList = showList,
                        });
                });

                await Navigation.PushAsync(
                    new BinItemListView(_GIItemQtyBin, pickedIemInfo.ItemCode, pickedIemInfo.ItemName,
                    pickedIemInfo.FromWhsCode,
                    0));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowMessage(excep.Message);
            }
        }

        /// <summary>
        ///  capture quantity to issue from this warehouse
        /// </summary>
        /// <param name="pickedIemInfo"></param>
        /// <returns></returns>
        async void HanlderGIQtyOpr(TransferDataM pickedIemInfo)
        {
            pickedIemInfo.RequestedQty = await CaptureItemQuantity(CurrentOItm);
            AddItem(
                   new OITM_Ex
                   {
                       Item = CurrentOItm,
                       ItemWhsCode = SelectedWhs.WhsCode,
                       TransQty = pickedIemInfo.RequestedQty,
                       TransType = TransType,
                       ShowList = string.Empty,
                   });
        }
        #endregion

        #region GR process
        /// <summary>
        /// Handler the GR line
        /// </summary>
        /// <param name="item"></param>
        void HandlerGRLine(TransferDataM pickedIemInfo)
        {
            try
            {
                // base on warehouse bin or non bin
                // based on transaction type - gi or gr 
                // based on item manage type 

                switch (pickedIemInfo.ItemManagedBy)
                {
                    case "Serial":
                        {
                            if (pickedIemInfo.BinActivated.Equals("Yes"))
                            {
                                HandlerGRSerialBinOpr(pickedIemInfo);
                                return;
                            }
                            // handler non bin serial
                            HandlerGRSerialOpr();
                            break;
                        }
                    case "Batch":
                        {
                            if (pickedIemInfo.BinActivated.Equals("Yes"))
                            {
                                HanlderGRBatchBinOpr(pickedIemInfo);
                                return;
                            }
                            // handler non bin Batch 
                            HanlderGRBatchOpr();
                            break;
                        }
                    case "None":
                        {
                            if (pickedIemInfo.BinActivated.Equals("Yes"))
                            {
                                HanlderGRQtyBinOpr(pickedIemInfo);
                                return;
                            }
                            // handler non bin serial 
                            HanlderGRQtyOpr(pickedIemInfo);
                            break;
                        }
                }
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowMessage(excep.Message);
            }
        }

        /// <summary>
        /// Handler GR SerialBin Opr
        /// </summary>
        async void HandlerGRSerialBinOpr(TransferDataM pickedIemInfo)
        {
            try
            {
                // then launch the allocation screen (step 2)
                MessagingCenter.Subscribe(this, _GRSerialBinOpr, (List<zwaTransferDocDetailsBin> serialBins) =>
                {
                    MessagingCenter.Unsubscribe<List<zwaTransferDocDetailsBin>>(this, _GRSerialBinOpr);
                    if (serialBins == null)
                    {
                        ShowToast("Operation cancel, no serial in bin selected, Please again later. Thanks");
                        return;
                    }

                    if (serialBins.Count == 0)
                    {
                        ShowToast("Operation cancel, no serial in bin selected, Please again later. Thanks");
                        return;
                    }

                    var showlist = string.Join("\n", serialBins.Select(x => $"{x.Serial} -> {x.BinCode}"));
                    AddItem(new OITM_Ex
                    {
                        Item = CurrentOItm,
                        ItemWhsCode = SelectedWhs.WhsCode,
                        GRSerialToBins = serialBins,
                        TransQty = serialBins.Count,
                        TransType = TransType,
                        ShowList = showlist
                    });
                });

                // launch the serial collection screen (step 1)
                // launch the collect serial view to collect the serial for GR
                // capture the serial
                MessagingCenter.Subscribe(this, _CaptureSerials, (List<string> serials) =>
                {
                    MessagingCenter.Unsubscribe<List<string>>(this, _CaptureSerials);

                    if (serials == null) return;
                    if (serials.Count == 0) return;

                    Navigation.PushAsync(new SerialToBinView(serials,
                        pickedIemInfo.ItemCode, pickedIemInfo.ItemName, pickedIemInfo.FromWhsCode,
                        _GRSerialBinOpr));
                });

                await Navigation.PushAsync(new CollectSerialsView(_CaptureSerials));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowMessage(excep.Message);
            }
        }

        /// <summary>
        /// Handler serial with out bin allocation
        /// </summary>
        /// <param name=""></param>
        async void HandlerGRSerialOpr()
        {
            try
            {
                MessagingCenter.Subscribe(this, _CaptureSerials, (List<string> serials) =>
                {
                    MessagingCenter.Unsubscribe<List<string>>(this, _CaptureSerials);
                    if (serials == null) return;
                    if (serials.Count == 0) return;

                    var showlist = string.Join("\n", serials);

                    AddItem(new OITM_Ex
                    {
                        Item = CurrentOItm,
                        ItemWhsCode = SelectedWhs.WhsCode,
                        GRSerialList = serials,
                        TransQty = serials.Count,
                        TransType = TransType,
                        ShowList = showlist
                    });
                });

                await Navigation.PushAsync(new CollectSerialsView(_CaptureSerials));

            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowMessage(excep.Message);
            }
        }

        /// <summary>
        /// HanlderGR Batc hBin Opr
        /// </summary>
        /// <param name="pickedIemInfo"></param>
        async void HanlderGRBatchBinOpr(TransferDataM pickedIemInfo)
        {
            try
            {
                // launch the batch collection view to collect the batch
                MessagingCenter.Subscribe(this, _GRCollectionBatches, async (List<Batch> batches) =>
                {
                    MessagingCenter.Unsubscribe<List<Batch>>(this, _GRCollectionBatches);
                    if (batches == null)
                    {
                        ShowToast("Operation cancel, no new batch add in, Please again later. Thanks");
                        return;
                    }

                    if (batches.Count == 0)
                    {
                        ShowToast("Operation cancel, no new batch add in, Please again later. Thanks");
                        return;
                    }

                    MessagingCenter.Subscribe(this, _GRBatchBinAllocation,
                        (List<zwaTransferDocDetailsBin> batchInBins) =>
                    {

                        if (batchInBins == null)
                        {
                            ShowToast("Operation cancel, batched no allocated into any bin, Please again later. Thanks");
                            return;
                        }

                        if (batchInBins.Count == 0)
                        {
                            ShowToast("Operation cancel, batched no allocated into any bin, Please again later. Thanks");
                            return;
                        }

                        string showlist = string.Empty;
                        decimal receiptQty1 = 0;
                        batchInBins.ForEach(bb =>
                           bb.Bins
                               .ForEach(bin =>
                               {
                                   receiptQty1 += bin.BatchTransQty;
                                   showlist += $"{bin.BatchTransQty:N} -> {bin.BatchNumber} -> {bin.oBIN.BinCode}\n";
                               }));

                        AddItem(new OITM_Ex
                        {
                            Item = CurrentOItm,
                            ItemWhsCode = SelectedWhs.WhsCode,
                            GRBatchInBins = batchInBins,
                            TransQty = receiptQty1,
                            TransType = TransType,
                            ShowList = showlist
                        });
                    });

                    /// launch the batch allocation to bin screen
                    await Navigation.PushAsync(
                        new BatchToBinView(
                            batches,
                            pickedIemInfo.FromWhsCode,
                            _GRBatchBinAllocation,
                            pickedIemInfo.ItemCode,
                            pickedIemInfo.ItemName));
                });

                // launch to collect the batch first 
                await Navigation.PushAsync(new CollectBatchesView(_GRCollectionBatches));

            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowMessage(excep.Message);
            }
        }

        /// <summary>
        /// Hanlder GR batch without bin
        /// </summary>
        /// <param name="pickedIemInfo"></param>
        async void HanlderGRBatchOpr()
        {
            // launch the batch collection view to collect the batch
            try
            {
                MessagingCenter.Subscribe(this, _GRCollectionBatches, async (List<Batch> batches) =>
                {
                    MessagingCenter.Unsubscribe<List<Batch>>(this, _GRCollectionBatches);
                    if (batches == null)
                    {
                        ShowToast("Operation cancel, no new batch add in, Please again later. Thanks");
                        return;
                    }

                    if (batches.Count == 0)
                    {
                        ShowToast("Operation cancel, no new batch add in, Please again later. Thanks");
                        return;
                    }

                    decimal batchQtySum = batches.Sum(x => x.Qty);
                    string showList = string.Join("\n", batches.Select(x => $"{x.Qty:N} -> {x.DistNumber}"));

                    AddItem(new OITM_Ex
                    {
                        Item = CurrentOItm,
                        ItemWhsCode = SelectedWhs.WhsCode,
                        GRBatch = batches,
                        TransQty = batchQtySum,
                        TransType = TransType,
                        ShowList = showList
                    });
                });

                // launch to collect the batch first 
                await Navigation.PushAsync(new CollectBatchesView(_GRCollectionBatches));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowMessage(excep.Message);
            }
        }

        /// <summary>
        /// Handler qty to bin 
        /// </summary>
        /// <param name="pickedIemInfo"></param>
        async void HanlderGRQtyBinOpr(TransferDataM pickedIemInfo)
        {
            try
            {
                decimal transQty = await CaptureItemQuantity(CurrentOItm);
                MessagingCenter.Subscribe(this, _ItemToBinSelect, (List<OBIN_Ex> bins) =>
                {
                    MessagingCenter.Unsubscribe<List<OBIN_Ex>>(this, _ItemToBinSelect);
                    if (bins == null)
                    {
                        ShowToast("Operation cancel, no bin allocated, Please again later. Thanks");
                        return;
                    }

                    if (bins.Count == 0)
                    {
                        ShowToast("Operation cancel, no bin allocated, Please again later. Thanks");
                        return;
                    }

                    // add into the list
                    var showList = string.Join("\n", bins.Select(x => $"{x.BinQty:N} -> {x.oBIN.BinCode}"));
                    AddItem(new OITM_Ex
                    {
                        Item = CurrentOItm,
                        ItemWhsCode = SelectedWhs.WhsCode,
                        GRQtyBin = bins,
                        TransQty = bins.Sum(x => x.BinQty),
                        TransType = TransType,
                        ShowList = showList
                    });
                });

                await Navigation.PushAsync(
                    new ItemToBinSelectView(pickedIemInfo.FromWhsCode,
                        _ItemToBinSelect,
                        pickedIemInfo.ItemCode,
                        pickedIemInfo.ItemName,
                        transQty, null));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowMessage(excep.Message);
            }
        }

        /// <summary>
        /// get the qty and add into the listview
        /// </summary>
        async void HanlderGRQtyOpr(TransferDataM pickedIemInfo)
        {
            pickedIemInfo.RequestedQty = await CaptureItemQuantity(CurrentOItm);
            AddItem(
                   new OITM_Ex
                   {
                       Item = CurrentOItm,
                       ItemWhsCode = SelectedWhs.WhsCode,
                       TransQty = pickedIemInfo.RequestedQty,
                       TransType = TransType,
                       ShowList = string.Empty,
                   });
        }
        #endregion

        ///// <summary>
        ///// remove the item from the list
        ///// </summary>
        ///// <param name="removeItem"></param>
        public void RemoveListItem(OITM_Ex removeItem)
        {
            try
            {
                if (itemsSource == null) return;
                if (removeItem == null) return;
                itemsSource.Remove(removeItem);
                ResetListView();
            }
            catch (Exception excep)
            {
                ShowMessage(excep.Message);
                Console.WriteLine(excep.ToString());
            }
        }

        /// <summary>
        /// Capture the qty and process the next
        /// </summary>
        /// <param name="itemCode"></param>
        /// <returns></returns>
        async Task<decimal> CaptureItemQuantity(OITM itemCode)
        {
            string transQty = await new Dialog().DisplayPromptAsync(
               $"Please input item code: {itemCode.ItemCode} {TransType} Qty",
               $"{itemCode.ItemName}\n",
               "OK",
               "Cancel",
               null,
               -1,
               Keyboard.Numeric,
               "1");

            bool isNumeric = decimal.TryParse(transQty, out decimal result);  // check numeric
            if (result <= 0) return -1; // check negative
            return result;
        }

        /// <summary>
        /// to add in the item into the list
        /// </summary>
        /// <param name="newItem"></param>
        void AddItem(OITM_Ex newItem)
        {
            try
            {
                if (itemsSource == null)
                {
                    itemsSource = new List<OITM_Ex>();
                    itemsSource.Add(newItem);
                    ResetListView();
                    return;
                }

                itemsSource.Insert(0, newItem); // always insert the item into the first
                ResetListView();
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowMessage(excep.Message);
            }
        }

        /// <summary>
        /// Reset the list view for user screen
        /// </summary>
        void ResetListView()
        {
            if (itemsSource == null)
            {
                ItemsSource = null;
                OnPropertyChanged(nameof(ItemsSource));
                return;
            }

            // reset the item and update to screen
            ItemsSource = new ObservableCollection<OITM_Ex>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        ///  Perfor then save and update
        /// </summary>
        async void SaveAndUpdate()
        {
            try
            {
                if (itemsSource == null)
                {
                    ShowMessage("The item list if empty, please try again later. Thanks");
                    return;
                }

                if (itemsSource.Count == 0)
                {
                    ShowMessage("The item list if empty, please try again later. Thanks");
                    return;
                }

                var transList = itemsSource.Where(x => x.TransQty > 0).ToList();
                if (transList == null)
                {
                    ShowMessage($"Please ensure the {TransType} item list having quantity, please try again later. Thanks");
                    return;
                }

                UserDialogs.Instance.ShowLoading($"Awaiting {TransType} Creation ...");

                var groupingGuid = Guid.NewGuid();

                // prepre the grpo object list 
                // perform file upload if any
                if (!string.IsNullOrWhiteSpace(SignaturePicturePath))
                {
                    bool fileUpload = await PerformPictureUploadAsync($"{groupingGuid}");
                    if (!fileUpload)
                    {
                        SignaturePicturePath = string.Empty;
                        ShowToast("Error upload signature, and signature picture cancelled.");
                    }
                }

                var lineList = new List<zwaGRPO>();
                var lineBinList = new List<zwaItemBin>();
                foreach (var line in transList)
                {
                    var lineGuid = Guid.NewGuid();
                    lineList.Add(new zwaGRPO
                    {
                        Guid = groupingGuid,
                        ItemCode = line.Item.ItemCode,
                        Qty = line.TransQty,
                        SourceCardCode = null,
                        SourceDocNum = -1,
                        SourceDocEntry = -1,
                        SourceDocBaseType = -1, // from purchase order
                        SourceBaseEntry = -1,
                        SourceBaseLine = -1,
                        Warehouse = line.itemWhsCode,  //(string.IsNullOrWhiteSpace(line.ItemWhsCode)) ? line.ItemWhsCode : SelectedWhs.WhsCode
                        LineGuid = lineGuid
                    });

                    // combine loading of the line into the object 
                    var list = line.GetList(groupingGuid, lineGuid);
                    if (list != null && list.Count > 0)
                    {
                        lineBinList.AddRange(list);
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

                // prepare the request dto                
                CurrentRequest = new zwaRequest
                {
                    sapUser = App.waUser,
                    request = $"Create {TransType}",
                    guid = groupingGuid,
                    status = "ONHOLD",
                    phoneRegID = App.waToken,
                    tried = 0,
                    popScreenAddress = _PopScreenAddr
                };

                App.RequestList.Add(CurrentRequest);

                var requestModule = (TransType.Equals("GI")) ? "CreateGoodsIssueRequest" : "CreateGoodsReceiveRequest";
                var requestBag = new Cio
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = requestModule,
                    dtoRequest = CurrentRequest,
                    dtoGRPO = lineList.ToArray(),
                    dtoItemBins = lineBinList.ToArray(),
                    dtozmwDocHeaderField = headerDetails
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
                    ShowMessage(content);
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
                ShowMessage($"{excep}");
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

        async Task<bool> PerformPictureUploadAsync(string guid)
        {
            try
            {
                var uploadHelper = new PictureUploadHelper();
                return await uploadHelper.ManageFileUpload(SignaturePicturePath, guid);
            }
            catch (Exception e)
            {
                ShowMessage($"{e.Message}");
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        //void CheckDocStatus()
        //{
        //    var callerAddress = $"{TransType}_DocCheck";
        //    MessagingCenter.Subscribe(this, callerAddress, (string result) =>
        //    {
        //        MessagingCenter.Unsubscribe<string>(this, callerAddress);
        //        UserDialogs.Instance.HideLoading();

        //        if (result.Equals("fail")) return;
        //        if (result.Equals("success")) Navigation.PopAsync();

        //        App.RequestList.Remove(CurrentRequest);
        //    });

        //    var checker = new RequestCheckHelper { Requests = CurrentRequest, ReturnAddress = callerAddress };
        //    checker.StartChecking();
        //}

        /// <summary>
        /// serach the selected series name 
        /// </summary>
        /// <returns></returns>
        int GetCurrentSeries(string selectedSeriesName)
        {
            var defaultSeries = (TransType.Equals(_GI)) ? 17 : 16;

            if (string.IsNullOrWhiteSpace(selectedSeriesName)) return defaultSeries;
            if (DocSeries == null) return defaultSeries;
            if (DocSeries.Length == 0) return defaultSeries;

            var series =
                DocSeries.Where(x => x.SeriesName.ToLower().Equals(selectedSeriesName.ToLower())).FirstOrDefault();

            if (series == null) return defaultSeries;
            else return series.Series;
        }

        /// <summary>
        /// Load the doc series 
        /// </summary>
        async void LoadDocSeries()
        {
            try
            {
                var requestBag = new Cio
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = (TransType.Equals(_GI)) ? "GIDocSeries" : "GRDocSeries",
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
                    ShowMessage(content);
                    return;
                }

                // Handler GI
                if (TransType.Equals(_GI))
                {
                    DTO_OIGE GiSeries = JsonConvert.DeserializeObject<DTO_OIGE>(content);
                    if (GiSeries != null && GiSeries.DocSeries != null)
                    {
                        DocSeries = GiSeries.DocSeries;
                        docSeriesItemsSource = new List<string>();
                        GiSeries.DocSeries.ForEach(x => docSeriesItemsSource.Add(x.SeriesName));

                        RefreshDocSeries();
                        return;
                    }
                    goto AddPrimary;
                }

                // handle GR
                DTO_OIGN GRSeries = JsonConvert.DeserializeObject<DTO_OIGN>(content);
                if (GRSeries != null && GRSeries.DocSeries != null)
                {
                    DocSeries = GRSeries.DocSeries;
                    docSeriesItemsSource = new List<string>();
                    GRSeries.DocSeries.ForEach(x => docSeriesItemsSource.Add(x.SeriesName));
                    RefreshDocSeries();
                    return;
                }

            AddPrimary:
                docSeriesItemsSource = new List<string>();
                docSeriesItemsSource.Add("Primary");
                RefreshDocSeries();
            }
            catch (Exception excep)
            {
                ShowMessage($"{excep.Message}");
                Console.WriteLine(excep.ToString());
            }
        }

        /// <summary>
        /// RefreshDocSeries
        /// </summary>
        void RefreshDocSeries()
        {
            if (docSeriesItemsSource == null) return;
            DocSeriesItemsSource = new ObservableCollection<string>(docSeriesItemsSource);
            OnPropertyChanged(nameof(DocSeriesItemsSource));

            if (docSeriesItemsSource.Count > 0)
            {
                DocSeriesSelectedItem = docSeriesItemsSource[0];
            }
        }

        /// <summary>
        /// Show message onto user screen
        /// </summary>
        /// <param name="message"></param>
        async void ShowMessage(string message) => await new Dialog().DisplayAlert("Alert", message, "OK");

        /// <summary>
        ///  show toast
        /// </summary>
        /// <param name="message"></param>
        void ShowToast(string message) => DependencyService.Get<IToastMessage>()?.LongAlert(message);
    }
}
