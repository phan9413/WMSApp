﻿using Acr.UserDialogs;
using DbClass;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.SAP;
using WMSApp.Views.Share;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Transfer1
{
    public class ItemToBinSelectVM : INotifyPropertyChanged
    {
        #region Page property
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        string whsCode;
        public string WhsCode
        {
            get => whsCode;
            set
            {
                if (whsCode == value) return;
                whsCode = value;
                OnPropertyChanged(nameof(WhsCode));
            }
        }

        public Command CmdSave { get; set; }
        public Command CmdCancel { get; set; }
        public Command CmdScanBinCode { get; set; }
        public Command CmdSearchVisible { get; set; }
        public string SearchText
        {
            set => HandlerSearch(value);
        }

        OBIN_Ex selectedItemBin;
        public OBIN_Ex SelectedItemBin /// support 1 selected item
        {
            get => selectedItemBin;
            set
            {
                if (selectedItemBin == value) return;
                selectedItemBin = value;

                if (selectedItemBin.BinQty > 0)
                {
                    ResetToZero(selectedItemBin);

                    selectedItemBin = null;
                    OnPropertyChanged(nameof(SelectedItemBin));
                    return;
                }

                HandlerSelectedBin(selectedItemBin);
                selectedItemBin = null;
                OnPropertyChanged(nameof(SelectedItemBin));
            }
        }

        List<OBIN_Ex> itemsSourceBin;
        public ObservableCollection<OBIN_Ex> ItemsSourceBin { get; set; }

        string itemCodeAndQty;
        public string ItemCodeAndQty
        {
            get => itemCodeAndQty;
            set
            {
                if (itemCodeAndQty == value) return;
                itemCodeAndQty = value;
                OnPropertyChanged(nameof(ItemCodeAndQty));
            }
        }

        string itemCode;
        public string ItemCode
        {
            get => itemCode;
            set
            {
                if (itemCode == value) return;
                itemCode = value;
                OnPropertyChanged(nameof(ItemCode));
            }
        }

        string itemName;
        public string ItemName
        {
            get => itemName;
            set
            {
                if (itemName == value) return;
                itemName = value;
                OnPropertyChanged(nameof(ItemName));
            }
        }

        string allocatedDisplay;
        public string AllocatedDisplay
        {
            get => allocatedDisplay;
            set
            {
                if (allocatedDisplay == value) return;
                allocatedDisplay = value;
                OnPropertyChanged(nameof(AllocatedDisplay));
            }
        }

        string remainDisplay;
        public string RemainDisplay
        {
            get => remainDisplay;
            set
            {
                if (remainDisplay == value) return;
                remainDisplay = value;
                OnPropertyChanged(nameof(RemainDisplay));
            }
        }

        decimal neededQty;
        public decimal NeededQty
        {
            get => neededQty;
            set
            {
                if (neededQty == value) return;
                neededQty = value;
                OnPropertyChanged(nameof(NeededQty));
            }
        }

        #endregion

        readonly string returnScannerAddr = "20201022T0913_StartBinCodScanner";
        string returnCallerAddress { get; set; } = string.Empty;
        string currentItem { get; set; } = string.Empty;
        decimal remain { get; set; } = 0;
        decimal allocated { get; set; } = 0;
        List<string> fromBinList { get; set; } = null;
        INavigation Navigation { get; set; } = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public ItemToBinSelectVM(INavigation navigation, string whsCode, string retAddrs,
            string itemCode, string itemName, decimal itemQty, List<string> _fromBinList)
        {
            Navigation = navigation;
            returnCallerAddress = retAddrs;
            WhsCode = whsCode;
            currentItem = itemCode;
            ItemCodeAndQty = $"{itemCode}# ,{itemQty:N}";
            NeededQty = itemQty;
            ItemCode = itemCode;
            ItemName = itemName;

            fromBinList = _fromBinList;

            CalcualteRemainAllocatedQty();
            LoadWarehouseBin();
            InitCmd();
        }

        // calculate the refresh the screen
        void CalcualteRemainAllocatedQty()
        {
            remain = neededQty - allocated;
            //AllocatedRemain = $"Allocated {allocated:N} / {remain:N} remaining";

            AllocatedDisplay = $"Allocated {allocated:N}";
            RemainDisplay = $"{remain:N} remain";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        void ResetToZero(OBIN_Ex selectedBin)
        {
            try
            {
                if (itemsSourceBin == null) return;
                if (itemsSourceBin.Count == 0) return;

                int locId = itemsSourceBin.IndexOf(selectedBin);
                if (locId < 0) return;

                allocated -= itemsSourceBin[locId].BinQty;
                CalcualteRemainAllocatedQty();

                itemsSourceBin[locId].BinQty = 0;
                itemsSourceBin[locId].CellColor = Color.White;
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        async void HandlerSelectedBin(OBIN_Ex selectedBin)
        {
            try
            {
                if (remain == 0)
                {
                    DisplayToast($"The Item # {this.currentItem} Qty fulfilled, " +
                        $"please try release some bin allocation or " +
                        $"Press Save to leave the screen, and try again, Thanks");
                    return;
                }

                decimal initialValue = remain;

                string qtyIn = await new Dialog().DisplayPromptAsync(
                    $"Input Qty for bin {selectedBin.oBIN.BinCode}", $"for item# {currentItem}",
                    "OK", "Cancel", null, -1, Keyboard.Numeric, $"{remain:N}");

                if (string.IsNullOrWhiteSpace(qtyIn)) return;
                if (qtyIn.ToLower().Equals("cancel")) return;

                bool isNumeric = decimal.TryParse(qtyIn, out decimal result);
                if (!isNumeric)
                {
                    DisplayToast($"Input value {qtyIn} is not a numeric value, please try again please, Thanks");
                    return;
                }

                if (result < 0)
                {
                    DisplayToast($"Input value {qtyIn} is not a positive numeric value, please try again please, Thanks");
                    return;
                }

                if (result > remain)
                {
                    DisplayToast($"Input value {qtyIn} is over the needed / remaining value {remain:N}, please try again please, Thanks");
                    return;
                }

                int locId = itemsSourceBin.IndexOf(selectedBin);
                if (locId < 0)
                {
                    DisplayToast($"Error locating the object in list, please try again later. Thanks");
                    return;
                }

                itemsSourceBin[locId].BinQty = result;
                itemsSourceBin[locId].CellColor = Color.LightGreen;

                allocated += result;
                CalcualteRemainAllocatedQty();
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Init the command
        /// </summary>
        void InitCmd()
        {
            CmdScanBinCode = new Command(StartScanner);
            CmdSearchVisible = new Command<SearchBar>((SearchBar sb) =>
            {
                sb.IsVisible = !sb.IsVisible;
                if (sb.IsVisible)
                {
                    sb.Focus();
                    sb.Placeholder = "Input bin code to search";
                }
            });

            CmdSave = new Command(Save);
            CmdCancel = new Command(Cancel);
        }

        /// <summary>
        /// Handler save 
        /// </summary>
        void Save()
        {
            try
            {
                if (itemsSourceBin == null) return;
                if (itemsSourceBin.Count == 0) return;

                var returnList = itemsSourceBin.Where(x => x.BinQty > 0).ToList();
                if (returnList == null)
                {
                    DisplayMessage("There is no bin being allocation for item(s), please try to allocate and save again. Thanks");
                    return;
                }

                if (returnList.Count == 0)
                {
                    DisplayMessage("There is no bin being allocation for item(s), please try to allocate and save again. Thanks");
                    return;
                }

                MessagingCenter.Send(returnList, returnCallerAddress);
                Close();
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        ///  handler cancel
        /// </summary>
        void Cancel()
        {
            try
            {
                var returnList = new List<OBIN_Ex>();
                MessagingCenter.Send(returnList, returnCallerAddress);
                Close();
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Close the user screen
        /// </summary>
        void Close() => Navigation.PopAsync();

        /// <summary>
        /// Start the scanner screen
        /// </summary>
        void StartScanner()
        {
            try
            {
                MessagingCenter.Subscribe<string>(this, returnScannerAddr, (string binCode) =>
                {
                    MessagingCenter.Unsubscribe<string>(this, returnScannerAddr);

                    if (string.IsNullOrWhiteSpace(binCode)) return;
                    if (binCode.ToLower().Equals("cancel")) return;

                    if (itemsSourceBin == null) return;

                    var foundBin = itemsSourceBin.Where(x => x.oBIN != null && x.oBIN.BinCode.ToLower().Equals(binCode.ToLower())).FirstOrDefault();
                    if (foundBin == null)
                    {
                        DisplayToast($"Input {binCode} no found. Please try again. Thanks.");
                        return;
                    }

                    HandlerSelectedBin(foundBin);
                });

                Navigation.PushAsync(new CameraScanView(returnScannerAddr, "Scan Bin#"));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Handle the search 
        /// </summary>
        /// <param name="query"></param>
        void HandlerSearch(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    RefreshBinListView();
                    return;
                }

                if (itemsSourceBin == null)
                {
                    RefreshBinListView();
                    return;
                }

                if (itemsSourceBin.Count == 0)
                {
                    RefreshBinListView();
                    return;
                }

                var lowerCaseQuery = query.ToLower();
                var filter = itemsSourceBin.Where(x => x.oBIN != null && x.oBIN.BinCode.ToLower().Contains(lowerCaseQuery)).ToList();
                if (filter == null)
                {
                    RefreshBinListView();
                    return;
                }
                if (filter.Count == 0)
                {
                    RefreshBinListView();
                    return;
                }

                // bind the filter list on the screen
                ItemsSourceBin = new ObservableCollection<OBIN_Ex>(filter);
                OnPropertyChanged(nameof(ItemsSourceBin));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Load the warehouse content bin
        /// </summary>
        async void LoadWarehouseBin()
        {
            try
            {
                // checking with server on the item number exsting in the warehouse
                UserDialogs.Instance.ShowLoading("A moment ...");
                var cioRequest = new Cio // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "GetWhsBins",
                    QueryWhs = whsCode
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Transfer1");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    DisplayMessage(content);
                    return;
                }

                Cio replied = JsonConvert.DeserializeObject<Cio>(content);
                if (replied == null)
                {
                    DisplayToast("Error getting information from server, please try again later. Thanks");
                    return;
                }

                if (replied.WarehouseBin == null)
                {
                    DisplayToast("Error getting information from server, please try again later [x1]. Thanks");
                    return;
                }

                if (replied.WarehouseBin.Length == 0)
                {
                    DisplayToast("Error getting information from server, please try again later [x2]. Thanks");
                    return;
                }

                var binList = new List<OBIN>(replied.WarehouseBin);
                if (itemsSourceBin == null) itemsSourceBin = new List<OBIN_Ex>();

                // to remove the 
                if (fromBinList == null)
                {
                    binList.ForEach(x => { itemsSourceBin.Add(new OBIN_Ex { oBIN = x }); });
                    RefreshBinListView();
                    return;
                }

                if (fromBinList.Count == 0)
                {
                    binList.ForEach(x => { itemsSourceBin.Add(new OBIN_Ex { oBIN = x }); });
                    RefreshBinListView();
                    return;
                }

                // remove all the from bin selection
                // to prevent the from bin being selected
                var filterList = replied.WarehouseBin.Where(x => !fromBinList.Contains(x.BinCode)).ToList();
                if (filterList == null) return;
                if (filterList.Count == 0) return;

                filterList.ForEach(x => { itemsSourceBin.Add(new OBIN_Ex { oBIN = x }); });
                RefreshBinListView();
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Refresh the list view bin
        /// </summary>
        void RefreshBinListView()
        {
            if (itemsSourceBin == null) return;
            if (itemsSourceBin.Count == 0) return;
            ItemsSourceBin = new ObservableCollection<OBIN_Ex>(itemsSourceBin);
            OnPropertyChanged(nameof(ItemsSourceBin));
        }

        /// <summary>
        /// Display Alert message
        /// </summary>
        /// <param name="message"></param>
        async void DisplayMessage(string message) =>
            await new Dialog().DisplayAlert("Alert", message, "OK");

        /// <summary>
        /// Display the toast message
        /// </summary>
        /// <param name="message"></param>
        void DisplayToast(string message) =>
            DependencyService.Get<IToastMessage>()?.ShortAlert(message);
    }
}

