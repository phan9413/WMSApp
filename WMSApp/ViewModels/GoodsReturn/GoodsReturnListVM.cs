using Acr.UserDialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Dtos;
using WMSApp.Interface;
using WMSApp.Models.SAP;
using WMSApp.Views.GoodsReturn;
using Xamarin.Forms;

namespace WMSApp.ViewModels.GoodsReturn
{
    public class GoodsReturnListVM : INotifyPropertyChanged
    {
        #region Page view property binding        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string pName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pName));
        public Command CmdCancelTransfer { get; set; }
        public Command CmdShowRequestList { get; set; }
        public Command CmdDirectRequestList { get; set; }
        public Command CmdSearchBarVisible { get; set; }
        public Command CmdSelectDocs { get; set; }

        Color _viewBackgroundColor;
        public Color ViewBackgroundColor
        {
            get => _viewBackgroundColor;
            set
            {
                if (_viewBackgroundColor == value) return;
                _viewBackgroundColor = value;
                OnPropertyChanged(nameof(ViewBackgroundColor));
            }
        }

        DateTime _startDate;
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate == value) return;
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
                RefreshDateRange();
            }
        }

        DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate == value) return;
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
                RefreshDateRange();
            }
        }

        bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value) return;
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        bool isSearchBarVisible;
        public bool IsSearchBarVisible
        {
            get => isSearchBarVisible;
            set
            {
                if (isSearchBarVisible == value) return;
                isSearchBarVisible = value;
                OnPropertyChanged(nameof(IsSearchBarVisible));
            }
        }
        public string SearchText
        {
            set
            {
                if (isGrrListVisible)
                {
                    HandlerSearchTextGrr(value); // search request list
                    return;
                }
                // handler the request search list
                HandlerSearchTextGRN(value); // search do list
            }
        }

        string dateRange;
        public string DateRange
        {
            get => dateRange;
            set
            {
                if (dateRange == value) return;
                dateRange = value;
                OnPropertyChanged(nameof(DateRange));
                LoadDocuments("o");
            }
        }

        bool isGrpoListVisible;
        public bool IsGrpoListVisible
        {
            get => isGrpoListVisible;
            set
            {
                if (isGrpoListVisible == value) return;
                isGrpoListVisible = value;
                OnPropertyChanged(nameof(IsGrpoListVisible));
            }
        }

        bool isGrrListVisible;
        public bool IsGrrListVisible
        {
            get => isGrrListVisible;
            set
            {
                if (isGrrListVisible == value) return;
                isGrrListVisible = value;
                OnPropertyChanged(nameof(IsGrrListVisible));
            }
        }

        OPDN_Ex selectedItemGrn;
        public OPDN_Ex SelectedItemGrn
        {
            get => selectedItemGrn;
            set
            {
                if (selectedItemGrn == value) return;
                selectedItemGrn = value;
                HandlerSelectedGRNDoc(selectedItemGrn);

                selectedItemGrn = null;
                OnPropertyChanged(nameof(SelectedItemGrn));
            }
        }

        List<OPDN_Ex> itemsSourceGRN;
        public ObservableCollection<OPDN_Ex> ItemsSourceGRN { get; set; } // for display the picked list from diff table

        OPRR_Ex selectedItemGrr;
        public OPRR_Ex SelectedItemGrr
        {
            get => selectedItemGrr;
            set
            {
                if (selectedItemGrr == value) return;
                selectedItemGrr = value;
                HandlerSelectedGrr(selectedItemGrr);

                selectedItemGrr = null;
                OnPropertyChanged(nameof(SelectedItemGrr));
            }
        }

        List<OPRR_Ex> itemsSourceGrr; // for display the picked list from diff table        
        public ObservableCollection<OPRR_Ex> ItemsSourceGrr { get; set; }

        string detailsText;
        public string DetailsText
        {
            get => detailsText;
            set
            {
                if (detailsText == value) return;
                detailsText = value;
                OnPropertyChanged(nameof(DetailsText));
            }
        }

        Color doTextColor;
        public Color DoTextColor
        {
            get => doTextColor;
            set
            {
                if (doTextColor == value) return;
                doTextColor = value;
                OnPropertyChanged(nameof(DoTextColor));
            }
        }

        Color reqTextColor;
        public Color ReqTextColor
        {
            get => reqTextColor;
            set
            {
                if (reqTextColor == value) return;
                reqTextColor = value;
                OnPropertyChanged(nameof(ReqTextColor));
            }
        }
        #endregion

        #region private 
        /// <summary>
        ///  Private declaration
        /// </summary>
        INavigation Navigation { get; set; } = null;
        string currentListStatus { get; set; } = "o";
        #endregion

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="navigation"></param>
        public GoodsReturnListVM(INavigation navigation, string pageType)
        {
            Navigation = navigation;
#if DEBUG
            _startDate = new DateTime(2016, 1, 1);
#else
            _startDate = DateTime.Now.AddDays(-7);
#endif
            _endDate = DateTime.Now;

            _isExpanded = true;

            RefreshDateRange();
            InitCmd();
            LoadDocuments("o"); // init load, set for load all (open and closed)
            SetButtonColor();
        }

        void SetButtonColor()
        {
            if (isGrrListVisible)
            {
                DetailsText = "Goods Return (Base on Goods Return Request)";
                DoTextColor = Color.Black;
                ReqTextColor = Color.White;
            }
            else
            {
                DetailsText = "Goods Return (Base on Goods Receipt PO)";
                DoTextColor = Color.White;
                ReqTextColor = Color.Black;
            }
        }

        /// <summary>
        /// refresh the date range
        /// </summary>
        void RefreshDateRange() => DateRange = $"From {_startDate:dd/MM/yyyy} to {_endDate:dd/MM/yyyy} [{GetListName()}]";

        /// <summary>
        /// Get list name
        /// </summary>
        /// <returns></returns>
        string GetListName()
        {
            switch (currentListStatus)
            {
                case "a": return "All";
                case "o": return "Open";
                case "c": return "Closed";
                case "h": return "Picked";
                default: return "All";
            }
        }

        /// <summary>
        /// Init the command
        /// </summary>
        void InitCmd()
        {
            CmdDirectRequestList = new Command(LaunchManualReturnRquest);

            CmdCancelTransfer = new Command(() => Navigation.PopAsync());
            CmdShowRequestList = new Command<string>((string actionName) =>
            {
                if (actionName.Equals("APINV"))
                {
                    IsGrrListVisible = true;
                    IsGrpoListVisible = false;
                    SetButtonColor();
                    return;
                }

                // handle the request
                IsGrrListVisible = false;
                IsGrpoListVisible = true;
                SetButtonColor();
            });

            CmdSearchBarVisible = new Command<SearchBar>((SearchBar sb) =>
            {
                sb.IsVisible = !sb.IsVisible;
                if (sb.IsVisible) sb.Focus();
            });
            CmdSelectDocs = new Command(HandlerSelectionOfDoc);
        }

        /// <summary>
        /// Handler selected doc
        /// </summary>
        void HandlerSelectionOfDoc()
        {
            if (isGrrListVisible) // check the do list
            {
                HandlerGrrDocSelection();
                return;
            }

            // check the request list
            HandlerGRNDocSelection();
        }

        /// <summary>
        /// Handler the do do selection
        /// </summary>
        void HandlerGrrDocSelection()
        {
            if (itemsSourceGrr == null) return;
            if (itemsSourceGrr.Count == 0) return;

            var checkedList = itemsSourceGrr.Where(x => x.IsChecked).ToList();
            if (checkedList == null)
            {
                DisplayToast("Please tick one or more document from the list, Thanks.");
                return;
            }

            if (checkedList.Count == 0)
            {
                DisplayToast("Please tick one or more document from the list, Thanks.");
                return;
            }

            var distinct = checkedList.Select(x => x.CardCode).Distinct().ToList();
            if (distinct != null && distinct.Count > 1)
            {
                DisplayAlert("Please select same vendor to process mutiple Goods Return");
                itemsSourceGrr.ForEach(x => x.IsChecked = false);
                return;
            }

            var list = new List<ReturnCommonHead_Ex>();
            checkedList.ForEach(x =>
            {
                list.Add(new ReturnCommonHead_Ex
                {
                    CardCode = x.CardCode,
                    CardName = x.CardName,
                    DocEntry = x.DocEntry,
                    DocDate = x.DocDate,
                    DocStatus = x.DocStatus,
                    DocNum = x.DocNum,
                    DocType = "OPRR" // ap invoice
                });
            });

            // launch the ReturnItemScreen
            Navigation.PushAsync(new GoodsReturnItemsView(list, "GRet. Request"));
        }

        /// <summary>
        /// Handler the do do selection
        /// </summary>
        void HandlerGRNDocSelection()
        {
            if (itemsSourceGRN == null) return;
            if (itemsSourceGRN.Count == 0) return;

            var checkedList = itemsSourceGRN.Where(x => x.IsChecked).ToList();
            if (checkedList == null)
            {
                DisplayToast("Please tick one or more document from the list, Thanks.");
                return;
            }

            if (checkedList.Count == 0)
            {
                DisplayToast("Please tick one or more document from the list, Thanks.");
                return;
            }

            var distinct = checkedList.Select(x => x.CardCode).Distinct().ToList();
            if (distinct != null && distinct.Count > 1)
            {
                DisplayAlert("Please select same vendor to process mutiple Goods Return");
                itemsSourceGRN.ForEach(x => x.IsChecked = false);
                return;
            }

            var list = new List<ReturnCommonHead_Ex>();
            checkedList.ForEach(x =>
            {
                list.Add(new ReturnCommonHead_Ex
                {
                    CardCode = x.CardCode,
                    CardName = x.CardName,
                    DocEntry = x.DocEntry,
                    DocDate = x.DocDate,
                    DocStatus = x.DocStatus,
                    DocNum = x.DocNum,
                    DocType = "OPDN" // grn
                });
            });

            // launch the ReturnItemScreen
            Navigation.PushAsync(new GoodsReturnItemsView(list, "GRPO"));
        }

        /// <summary>
        /// Launch Stand Alone Transfer
        /// </summary>
        async void LaunchManualReturnRquest()
            => await Navigation.PushAsync(new GoodsReturnWithNoGRRequestView());

        /// <summary>
        /// Search by doc #
        /// request doc
        /// </summary>
        /// <param name="query"></param>
        void HandlerSearchTextGRN(string query)
        {
            try
            {
                if (itemsSourceGRN == null) return;
                if (string.IsNullOrWhiteSpace(query))
                {
                    ResetListViewGRN();
                    return;
                }

                var lowercase = query.ToLower();
                var filter = itemsSourceGRN.Where(x => x.DocNum.ToString().Contains(query)).ToList();
                if (filter == null)
                {
                    ResetListViewGRN();
                    return;
                }
                if (filter.Count == 0)
                {
                    ResetListViewGRN();
                    return;
                }

                ItemsSourceGRN = new ObservableCollection<OPDN_Ex>(filter);
                OnPropertyChanged(nameof(ItemsSourceGRN));
            }
            catch (Exception excep)
            {
                DisplayAlert(excep.Message);
                Console.WriteLine($"{excep}");
            }
        }

        /// <summary>
        /// for search the return request
        /// </summary>
        /// <param name="query"></param>
        void HandlerSearchTextGrr(string query)
        {
            try
            {
                if (itemsSourceGrr == null) return;
                if (string.IsNullOrWhiteSpace(query))
                {
                    ResetListViewGrr();
                    return;
                }

                var lowercase = query.ToLower();
                var filter = itemsSourceGrr.Where(x => x.DocNum.ToString().Contains(query)).ToList();
                if (filter == null)
                {
                    ResetListViewGrr();
                    return;
                }
                if (filter.Count == 0)
                {
                    ResetListViewGrr();
                    return;
                }

                ItemsSourceGrr = new ObservableCollection<OPRR_Ex>(filter);
                OnPropertyChanged(nameof(ItemsSourceGrr));
            }
            catch (Exception excep)
            {
                DisplayAlert(excep.Message);
                Console.WriteLine($"{excep}");
            }
        }

        /// <summary>
        /// Handler when Do doc selected
        /// </summary>
        /// <param name="selectedDoc"></param>
        void HandlerSelectedGRNDoc(OPDN_Ex selectedDoc)
        {
            if (itemsSourceGRN == null) return;
            if (itemsSourceGRN.Count == 0) return;

            int locid = itemsSourceGRN.IndexOf(selectedDoc);
            if (locid < 0) return;

            itemsSourceGRN[locid].IsChecked = !itemsSourceGRN[locid].IsChecked;
            ResetListViewGRN();
        }

        /// <summary>
        /// Handle the selected request documentation
        /// </summary>
        void HandlerSelectedGrr(OPRR_Ex selectedDoc)
        {
            if (itemsSourceGrr == null) return;
            if (itemsSourceGrr.Count == 0) return;

            int locid = itemsSourceGrr.IndexOf(selectedDoc);
            if (locid < 0) return;

            itemsSourceGrr[locid].IsChecked = !itemsSourceGrr[locid].IsChecked;
            ResetListViewGrr();
        }

        /// <summary>
        /// Load inventory transfer request from web server
        /// </summary>
        async public void LoadDocuments(string filter = "")
        {
            try
            {
                UserDialogs.Instance.ShowLoading("A moment please...");
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    RequestTransferDocFilter = (string.IsNullOrWhiteSpace(filter)) ? currentListStatus : filter,
                    request = "GetGrnGrrReqList",
                    getSoType = "O",
                    QueryStartDate = _startDate,
                    QueryEndDate = _endDate
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "GoodsReturn");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                // reet the list view 
                itemsSourceGrr = new List<OPRR_Ex>();
                ResetListViewGrr();

                itemsSourceGRN = new List<OPDN_Ex>();
                ResetListViewGRN();

                if (!isSuccess)
                {
                    DisplayAlert(content + "\n" + lastErrMessage);
                    return;
                }

                var dtoOPrr = JsonConvert.DeserializeObject<DTO_OPRR>(content);
                if (dtoOPrr == null)
                {
                    DisplayToast("There is error reading server info, please try again later, Thanks.");
                    return;
                }

                if (dtoOPrr.OPDNs != null && dtoOPrr.OPDNs.Length > 0)
                {
                    itemsSourceGRN.AddRange(dtoOPrr.OPDNs);
                    ResetListViewGRN();
                }

                if (dtoOPrr.OPRR_Exs != null && dtoOPrr.OPRR_Exs.Length > 0)
                {
                    itemsSourceGrr.AddRange(dtoOPrr.OPRR_Exs);
                    ResetListViewGrr();
                }

                SetButtonColor();
            }
            catch (Exception excep)
            {
                DisplayAlert(excep.Message);
                Console.WriteLine($"{excep}");
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Refresh the list view of picked
        /// </summary>
        void ResetListViewGRN()
        {
            if (itemsSourceGRN == null) return;

            IsGrpoListVisible = true;
            IsGrrListVisible = false;

            ItemsSourceGRN = new ObservableCollection<OPDN_Ex>(itemsSourceGRN);
            OnPropertyChanged(nameof(ItemsSourceGRN));
        }

        /// <summary>
        /// Refresh the list view of open list
        /// </summary>
        void ResetListViewGrr()
        {
            if (itemsSourceGrr == null) return;

            IsGrpoListVisible = false;
            IsGrrListVisible = true;

            ItemsSourceGrr = new ObservableCollection<OPRR_Ex>(itemsSourceGrr);
            OnPropertyChanged(nameof(ItemsSourceGrr));
        }

        /// <summary>
        /// Display Alert
        /// </summary>
        /// <param name="message"></param>
        async void DisplayAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "Ok");

        void DisplayToast(string message) => DependencyService.Get<IToastMessage>()?.ShortAlert(message);
    }
}