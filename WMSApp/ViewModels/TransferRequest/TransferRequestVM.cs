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
using WMSApp.Views.Share;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace WMSApp.ViewModels.TransferRequest
{
    public class TransferRequestVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        public Command CmdSaveUpdate { get; set; }
        public Command CmdClose { get; set; }
        public Command CmdSelectFromWhs { get; set; }
        public Command CmdSelectToWhs { get; set; }
        public Command CmdScanAddItem { get; set; }
        public Command CmdManualAddItem { get; set; }

        string fromWarehouse;

        public string FromWarehouse
        {
            get => fromWarehouse;
            set
            {
                if (fromWarehouse == value) return;
                fromWarehouse = value;
                OnPropertyChanged(nameof(FromWarehouse));
            }
        }

        string toWarehouse;
        public string ToWarehouse
        {
            get => toWarehouse;
            set
            {
                if (toWarehouse == value) return;
                toWarehouse = value;
                OnPropertyChanged(nameof(ToWarehouse));
            }
        }

        bool isExpanded;
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (isExpanded == value) return;
                isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        public ObservableCollection<WTQ1_Ex> ItemsSource { get; set; }
        List<WTQ1_Ex> itemsSource { get; set; }

        WTQ1_Ex currentLine;
        WTQ1_Ex selectedItem;
        public WTQ1_Ex SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;

                HandlerSelectedItem(selectedItem);

                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }


        // added in the doc series and doc property
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

        NNM1[] OwtqSeries { get; set; } // kept orginal from server

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

        OWHS FromOWHS { get; set; }
        OWHS ToOWHS { get; set; }
        INavigation Navigation { get; set; } = null;
        zwaRequest currentRequest { get; set; } = null;
        string lastErrorMessage { get; set; } = string.Empty;
       
        readonly string _frmWarehouseReturnAddr = "_frmWarehouseReturnAddr_20201113";
        readonly string _toWarehouseReturnAddr = "_toWarehouseReturnAddr_20201113";
        readonly string _FROM = "FROM";
        readonly string _TO = "TO";

        readonly string _PopScreenAddr = "_PopScreenAddr_201207_TransReq";

        /// <summary>
        /// Constructor
        /// </summary>
        public TransferRequestVM(INavigation navigation)
        {
            Navigation = navigation;
            IsExpanded = true;

            InitCmd();
            LoadDocSeries();
        }

        /// <summary>
        /// Remove the item from page instruction
        /// </summary>
        /// <param name="removedItem"></param>
        public void RemoveItemFromList(WTQ1_Ex removedItem)
        {
            IsExpanded = false;
            if (itemsSource == null) return;
            itemsSource.Remove(removedItem);
            RefreshList();
        }

        /// <summary>
        /// change the warehouse for the line item
        /// </summary>
        /// <param name="editItem"></param>
        public void EditFromWarehouse(WTQ1_Ex editItem)
        {
            if (itemsSource == null) return;
            currentLine = editItem;
            IsExpanded = true;

            ///// handler the to warehouse
            MessagingCenter.Subscribe<OWHS>(this, _frmWarehouseReturnAddr, (OWHS fromWhs) =>
            {
                MessagingCenter.Unsubscribe<OWHS>(this, _frmWarehouseReturnAddr);
                currentLine.RequestFromWarehouse = fromWhs.WhsCode;
                int locId = itemsSource.IndexOf(currentLine);

                if (locId >= 0)
                {
                    itemsSource[locId].RequestFromWarehouse = fromWhs.WhsCode;
                    DisplayShortToast("From warehouse changed.");
                    RefreshList();
                    return;
                }
                DisplayShortToast("Warehouse changed fail, please try again later, Thanks");
            });

            Navigation.PushPopupAsync(new PickWarehousePopUpView(
                _frmWarehouseReturnAddr,
                "Select[FROM] warehouse",
                Color.Yellow));
            return;
        }

        /// <summary>
        /// change the warehouse for the line item
        /// </summary>
        /// <param name="editItem"></param>
        public async void EditToWarehouse(WTQ1_Ex editItem)
        {

            if (itemsSource == null) return;
            currentLine = editItem;
            IsExpanded = true;

            ///// handler the to warehouse
            MessagingCenter.Subscribe<OWHS>(this, _toWarehouseReturnAddr, (OWHS toWhs) =>
            {
                MessagingCenter.Unsubscribe<OWHS>(this, _toWarehouseReturnAddr);

                // If the from warehouse and to warehouse are not same
                // direct add the warehouse in
                if (!currentLine.RequestFromWarehouse.Equals(toWhs.WhsCode))
                {
                    int locId = itemsSource.IndexOf(currentLine);
                    if (locId >= 0)
                    {
                        itemsSource[locId].RequestToWarehouse = toWhs.WhsCode;
                        DisplayShortToast("TO warehouse changed.");
                        RefreshList();
                    }
                    return;
                }

                // if from and to warehouse are the same
                // check warehouse is bin activated or not
                if (currentLine.RequestFromWarehouse.Equals(toWhs.WhsCode) && toWhs.BinActivat.Equals("Y"))
                {
                    int locId = itemsSource.IndexOf(currentLine);
                    if (locId >= 0)
                    {
                        itemsSource[locId].RequestToWarehouse = toWhs.WhsCode;
                        DisplayShortToast("TO warehouse changed.");
                        RefreshList();
                        return;
                    }
                }

                DisplayLongToast("Receipt [TO] warehouse cannot be identical to the release warehouse. " +
                    "Please select different FROM and TO warehouse");
                return;
            });

            await Navigation.PushPopupAsync(new PickWarehousePopUpView(
                 _toWarehouseReturnAddr,
                 "Select[TO] warehouse",
                 Color.Yellow));
        }

        /// <summary>
        ///  Promtp selection of the warehouse
        /// </summary>
        /// <param name="direction"></param>
        void SelectWarehouse(string direction)
        {
            if (direction.Equals(_FROM))
            {
                MessagingCenter.Subscribe<OWHS>(this, _frmWarehouseReturnAddr, (OWHS fromWhs) =>
                {
                    MessagingCenter.Unsubscribe<OWHS>(this, _frmWarehouseReturnAddr);
                    FromOWHS = fromWhs;
                    FromWarehouse = fromWhs.WhsCode;
                });
                Navigation.PushPopupAsync(new PickWarehousePopUpView(_frmWarehouseReturnAddr,
                    "Select[FROM] warehouse", Color.Green));
                return;
            }

            if (direction.Equals(_TO))
            {
                ///// handler the to warehouse
                MessagingCenter.Subscribe<OWHS>(this, _toWarehouseReturnAddr, (OWHS toWhs) =>
                {
                    MessagingCenter.Unsubscribe<OWHS>(this, _toWarehouseReturnAddr);
                    if (!fromWarehouse.Equals(toWhs.WhsCode))
                    {
                        ToOWHS = toWhs;
                        ToWarehouse = toWhs.WhsCode;
                        IsExpanded = true;
                        return;
                    }

                    if (fromWarehouse.Equals(toWhs.WhsCode) && FromOWHS.BinActivat.Equals("Y"))
                    {
                        ToOWHS = toWhs;
                        ToWarehouse = toWhs.WhsCode;
                        IsExpanded = true;
                        return;
                    }

                    DisplayLongToast("Receipt [TO] warehouse cannot be identical to the release warehouse. " +
                        "Please select different FROM and TO warehouse");
                    return;

                });
                Navigation.PushPopupAsync(new PickWarehousePopUpView(_toWarehouseReturnAddr, "Select[TO] warehouse", Color.Yellow));
                return;
            }
        }

        /// <summary>
        ///  Init commands
        /// </summary>
        void InitCmd()
        {
            CmdSelectFromWhs = new Command(async () =>
            {
                //FromWarehouse = await PromtpSelectWarehouse("Choose a request from warehouse", "Cancel");
                SelectWarehouse(_FROM);
            });

            CmdSelectToWhs = new Command(async () =>
            {
                //ToWarehouse = await PromtpSelectWarehouse("Choose a request to warehouse", "Cancel", fromWarehouse);
                SelectWarehouse(_TO);
            });

            CmdSaveUpdate = new Command(SaveAndUpdate);
            CmdClose = new Command(Close);

            CmdScanAddItem = new Command(async () => HandlerScanAddItem());
            CmdManualAddItem = new Command(async () => HandlerManualAddItem());
        }

        /// <summary>
        /// Handler the selected item on screen
        /// When the item qty is zero then prompt enter new qty
        /// When item qty > zero, then reset the item to zero
        /// </summary>
        /// <param name="selectedItem"></param>
        void HandlerSelectedItem(WTQ1_Ex selectedItem)
        {
            if (selectedItem == null) return;
            if (selectedItem.RequestQuantity == 0)
            {
                CaptureQuantity(selectedItem.SelectedOITM);
                return;
            }

            if (selectedItem.RequestQuantity > 0)
            {
                int locId = itemsSource.IndexOf(selectedItem);
                if (locId < 0) return;

                selectedItem.RequestQuantity = 0;
            }
        }

        /// <summary>
        /// Start a scan page and return a scan item
        /// </summary>
        async void HandlerScanAddItem()
        {
            string returnAddress = "HandlerScanAddItem_Transfer";
            MessagingCenter.Subscribe(this, returnAddress, (string newItemCode) =>
            {
                MessagingCenter.Unsubscribe<string>(this, returnAddress);
                if (string.IsNullOrWhiteSpace(newItemCode))
                {
                    return;
                }
                if (newItemCode.Equals("cancel"))
                {
                    return;
                }
                IsExpanded = false;
                // process to add in the item code   
                CheckItemCode(newItemCode);
            });

            await Navigation.PushAsync(new CameraScanView(returnAddress));
        }

        /// <summary>
        /// Handler manual entry the item code
        /// </summary>
        async void HandlerManualAddItem()
        {
            if (string.IsNullOrWhiteSpace(fromWarehouse))
            {
                DisplayAlert("Please select a request from warehouse, Thanks");
                return;
            }

            if (string.IsNullOrWhiteSpace(toWarehouse))
            {
                DisplayAlert("Please select a request to warehouse, Thanks");
                return;
            }

            // capture the item code
            string enteredItemCode = await new Dialog().DisplayPromptAsync("Input Item Code", "", "OK", "Cancel");
            if (string.IsNullOrWhiteSpace(enteredItemCode))
            {
                return;
            }

            IsExpanded = false;
            CheckItemCode(enteredItemCode);
        }

        /// <summary>
        /// Query web server to get the item code and item name 
        /// </summary>
        /// <param name="itemCode"></param>
        /// <returns></returns>
        async void CheckItemCode(string itemCode)
        {
            // check from webapi from the item exist
            try
            {
                UserDialogs.Instance.ShowLoading("A moment ...");
                var requestBag = new Cio
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "CheckItemCodeExistance",
                    QueryItemCode = itemCode
                };

                // send over server to create request
                string content, lastErrMessage;
                var isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(requestBag, "TransferRequest");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    DisplayShortToast($"Error contacting server for query item code {itemCode}, Please again later.[x0]");
                    return;
                }

                // else get the object and return
                var repliedBag = JsonConvert.DeserializeObject<Cio>(content);
                if (repliedBag == null)
                {
                    DisplayShortToast($"Error contacting server for query item code {itemCode}, Please again later. [x1]");
                    return;
                }

                if (repliedBag.Item == null)
                {
                    DisplayShortToast($"Error contacting server for query item code {itemCode}, Please again later. [x2]");
                    return;
                }

                // capture the quantity
                CaptureQuantity(repliedBag.Item);
            }
            catch (Exception excep)
            {
                lastErrorMessage = excep.ToString();
                Console.WriteLine(lastErrorMessage);
                DisplayAlert(excep.Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Capture the Item Quantity
        /// </summary>
        async void CaptureQuantity(OITM item)
        {
            try
            {
                string captureQty = await new Dialog()
                        .DisplayPromptAsync("Input request quantity", "", "Confirm", "Cancel", null, -1, Keyboard.Numeric, "1");

                UserDialogs.Instance.ShowLoading("A moment ...");
                if (string.IsNullOrWhiteSpace(captureQty))
                {
                    DisplayAlert("Please enter valid numeric data, Thanks");
                    return;
                }

                var isNumeric = int.TryParse(captureQty, out int resultQty);
                if (!isNumeric)
                {
                    DisplayAlert("Please enter valid / positive numeric data, Thanks");
                    return;
                }

                // check duplivated item in list
                if (itemsSource != null)
                {
                    var isExist = itemsSource.Where(x => x.ItemCode == item.ItemCode).FirstOrDefault();
                    if (isExist == null)
                    {
                        goto AddToList;
                    }

                    int locIdx = itemsSource.IndexOf(isExist);
                    if (isExist.RequestQuantity == 0)
                    {
                        ItemsSource[locIdx].RequestQuantity = resultQty;
                        RefreshList();
                        return;
                    }

                    #region handle duplicated item in list
                    // prompt over write, add up, cancel
                    string[] duplicatedItemOpts = { "Over write", "Add up" };

                    var selectedOpt = await new Dialog()
                        .DisplayActionSheet($"Duplicated Input Item {item.ItemCode}\n" +
                        $"Select one from below"
                        , "Cancel", null, duplicatedItemOpts);

                    if (string.IsNullOrWhiteSpace(selectedOpt)) return;

                    if (selectedOpt.ToLower().Equals("cancel")) return;


                    if (locIdx < 0)
                    {
                        DisplayAlert($"Sorry, unable to update the item quantity {item.ItemCode}");
                        return;
                    }

                    if (selectedOpt.Contains("Add up"))
                    {
                        ItemsSource[locIdx].RequestQuantity += resultQty;
                        RefreshList();
                        return;
                    }

                    if (selectedOpt.Contains("Over write"))
                    {
                        ItemsSource[locIdx].RequestQuantity = resultQty;
                        RefreshList();
                        return;
                    }
                    #endregion
                }

            AddToList:// Add into the list
                AddList(new WTQ1_Ex()
                {
                    SelectedOITM = item,
                    ItemCode = item.ItemCode,
                    Dscription = item.ItemName,
                    FromWhsCod = fromWarehouse,
                    WhsCode = toWarehouse,
                    Quantity = resultQty
                });

                // refresh the list 
                RefreshList();
            }
            catch (Exception excep)
            {
                lastErrorMessage = excep.ToString();
                Console.WriteLine(lastErrorMessage);
                DisplayAlert(excep.Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Re binding the list to the screen
        /// </summary>
        void RefreshList()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<WTQ1_Ex>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// Add into the list
        /// </summary>
        void AddList(WTQ1_Ex newRequestLine)
        {
            if (itemsSource == null) itemsSource = new List<WTQ1_Ex>();
            itemsSource.Insert(0, newRequestLine);
        }

        /// <summary>
        /// Handler the close screen
        /// </summary>
        async void Close()
        {
            if (itemsSource == null)
            {
                await Navigation.PopAsync();
                return;
            }

            var confirmLeaveScreen =
                await new Dialog().DisplayAlert("Confirm leave ?",
                $"There are {itemsSource.Count} in the list, leaving as this will clear the list and will not recovered."
                , "Leave now", "Cancel");

            if (confirmLeaveScreen)
            {
                await Navigation.PopAsync();
                return;
            }
        }

        /// <summary>
        /// Perform save and update into he middle ware and waiting replied
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

                var requestList = itemsSource.Where(x => x.RequestQuantity > 0).ToList();
                if (requestList == null)
                {
                    DisplayAlert("There is no items request entered, please try again later. Thanks [x0]");
                    return;
                }

                if (requestList.Count == 0)
                {
                    DisplayAlert("There is no items request entered, please try again later. Thanks [x1]");
                    return;
                }

                UserDialogs.Instance.ShowLoading("Awaiting Inventory Request Creation ...");

                var groupingGuid = Guid.NewGuid();
                var header = new zwaInventoryRequestHead
                {
                    Id = -1,
                    FromWarehouse = fromWarehouse,
                    ToWarehouse = toWarehouse,
                    Guid = groupingGuid,
                    TransDate = DateTime.Now
                };

                // prepare the doc series / property
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

                // prepre the grpo object list 

                var lineList = new List<zwaInventoryRequest>();
                foreach (var line in requestList)
                {
                    lineList.Add(new zwaInventoryRequest
                    {
                        Id = -1,
                        Guid = groupingGuid,
                        ItemCode = line.RequestedItemCode,
                        Quantity = line.RequestQuantity,
                        FromWarehouse = line.RequestFromWarehouse,
                        ToWarehouse = line.RequestToWarehouse,
                        AppUser = App.waUser,
                        TransTime = DateTime.Now
                    });
                }

                // 20200719T1030
                // prepare the request dto
                currentRequest = new zwaRequest
                {
                    sapUser = App.waUser,
                    request = "Create Inventory Request",
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
                    request = "CreateInventoryRequest",
                    dtoRequest = currentRequest,
                    dtoInventoryRequest = lineList.ToArray(),
                    dtoInventoryRequestHead = header,
                    dtozmwDocHeaderField = headerDetails
                };

                // send over server to create request
                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(requestBag, "TransferRequest");
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

                DisplayShortToast("Request sent.");
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
        //    var callerAddress = "CreateTransferReq_DocCheck";
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
            const int primarySeries = 52;
            if (string.IsNullOrWhiteSpace(selectedSeriesName)) return primarySeries;
            if (OwtqSeries == null) return primarySeries;
            if (OwtqSeries.Length == 0) return primarySeries;

            var series =
                OwtqSeries.Where(x => x.SeriesName.ToLower().Equals(selectedSeriesName.ToLower())).FirstOrDefault();

            if (series == null) return primarySeries;
            else return series.Series;
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
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "TransferRequest");
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
                    docSeriesItemsSource = new List<string>();
                    docSeriesItemsSource.Add("Primary");
                }
                else
                {
                    OwtqSeries = dtoOwtq.DocSeries;
                    docSeriesItemsSource = new List<string>();
                    OwtqSeries.ForEach(x => docSeriesItemsSource.Add(x.SeriesName));
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
        /// Display alert message onto the user screen
        /// </summary>
        /// <param name="message"></param>
        async void DisplayAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "OK");

        /// <summary>
        /// Display Short Toast
        /// </summary>
        /// <param name="message"></param>
        void DisplayShortToast(string message) => DependencyService.Get<IToastMessage>()?.ShortAlert(message);

        /// <summary>
        /// Display Long Toast
        /// </summary>
        /// <param name="message"></param>
        void DisplayLongToast(string message) => DependencyService.Get<IToastMessage>()?.LongAlert(message);
    }
}
