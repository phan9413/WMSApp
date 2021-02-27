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
using WMSApp.ViewModels.Return;
using WMSApp.Views.Return;
using Xamarin.Forms;

namespace WMSApp.ViewModels.ReturnRequest
{
    public class ReturnListVM : INotifyPropertyChanged
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
                if (isReturnReqListVisible)
                {
                    HandlerSearchTextReq(value); // search request list
                    return;
                }
                // handler the request search list
                HandlerSearchTextDo(value); // search do list
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
                LoadTransferRequest("o");
            }
        }

        bool isDoListVisible;
        public bool IsDoListVisible
        {
            get => isDoListVisible;
            set
            {
                if (isDoListVisible == value) return;
                isDoListVisible = value;
                OnPropertyChanged(nameof(IsDoListVisible));
            }
        }

        bool isReturnReqListVisible;
        public bool IsReturnReqListVisible
        {
            get => isReturnReqListVisible;
            set
            {
                if (isReturnReqListVisible == value) return;
                isReturnReqListVisible = value;
                OnPropertyChanged(nameof(IsReturnReqListVisible));
            }
        }

        ODLN_Ex selectedItemDo;
        public ODLN_Ex SelectedItemDo
        {
            get => selectedItemDo;
            set
            {
                if (selectedItemDo == value) return;
                selectedItemDo = value;
                HandlerSelectedDoDoc(selectedItemDo);

                selectedItemDo = null;
                OnPropertyChanged(nameof(SelectedItemDo));
            }
        }

        List<ODLN_Ex> itemsSourceDo;
        public ObservableCollection<ODLN_Ex> ItemsSourceDo { get; set; } // for display the picked list from diff table

        ORRR_Ex selectedItemRReq;
        public ORRR_Ex SelectedItemRReq
        {
            get => selectedItemRReq;
            set
            {
                if (selectedItemRReq == value) return;
                selectedItemRReq = value;
                HandlerSelectedRequestDoc(selectedItemRReq);

                selectedItemRReq = null;
                OnPropertyChanged(nameof(SelectedItemRReq));
            }
        }

        List<ORRR_Ex> itemsSourceRReq; // for display the picked list from diff table        
        public ObservableCollection<ORRR_Ex> ItemsSourceRReq { get; set; }

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
        public ReturnListVM(INavigation navigation, string pageType)
        {
            Navigation = navigation;
#if DEBUG
            _startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
#else
            _startDate = DateTime.Now.AddDays(-7);
#endif
            _endDate = DateTime.Now;

            _isExpanded = true;           

            RefreshDateRange();
            InitCmd();
            LoadTransferRequest("o"); // init load, set for load all (open and closed)
            SetButtonColor();
        }

        void SetButtonColor()
        {
            if (isDoListVisible)
            {
                DetailsText = "Return (Base on Delivery Order)";
                DoTextColor = Color.Black;
                ReqTextColor = Color.White;
            }
            else
            {
                DetailsText = "Return (Base on Return Request)";
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
            CmdDirectRequestList = new Command(LaunchManualReturn);

            CmdCancelTransfer = new Command(() => Navigation.PopAsync());
            CmdShowRequestList = new Command<string>((string actionName) =>
            {
                if (actionName.Equals("do"))
                {
                    IsDoListVisible = true;
                    IsReturnReqListVisible = false;
                    SetButtonColor();
                    return;
                }

                // handle the request
                IsDoListVisible = false;
                IsReturnReqListVisible = true;
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
            if (isDoListVisible) // check the do list
            {
                HandlerDoDocSelection();
                return;
            }

            // check the request list
            HandlerReqDocSelection();
        }

        /// <summary>
        /// Handler the do do selection
        /// </summary>
        void HandlerDoDocSelection()
        {
            if (itemsSourceDo == null) return;
            if (itemsSourceDo.Count == 0) return;

            var checkedList = itemsSourceDo.Where(x => x.IsChecked).ToList();
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
                DisplayAlert("Please select same customer to process mutiple Return");
                itemsSourceDo.ForEach(x => x.IsChecked = false);
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
                    DocType = "ODLN"                    
                });
            });

            // launch the ReturnItemScreen
            Navigation.PushAsync(new ReturnItemsView(list, "Delivery"));
        }

        /// <summary>
        /// Handler the do do selection
        /// </summary>
        void HandlerReqDocSelection()
        {
            if (itemsSourceRReq == null) return;
            if (itemsSourceRReq.Count == 0) return;

            var checkedList = itemsSourceRReq.Where(x => x.IsChecked).ToList();
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
                DisplayAlert("Please select same customer to process mutiple Return");
                itemsSourceDo.ForEach(x => x.IsChecked = false);
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
                    DocType = "ORRR"
                });
            });

            // launch the ReturnItemScreen
            Navigation.PushAsync(new ReturnItemsView(list, "Return Request"));
        }

        /// <summary>
        /// Launch Stand Alone Transfer
        /// </summary>
        async void LaunchManualReturn() => await Navigation.PushAsync(new ReturnWithNoDocSourceView());        

        /// <summary>
        /// Search by doc #
        /// request doc
        /// </summary>
        /// <param name="query"></param>
        void HandlerSearchTextDo(string query)
        {
            try
            {
                if (itemsSourceDo == null) return;
                if (string.IsNullOrWhiteSpace(query))
                {
                    ResetListViewDo();
                    return;
                }

                var lowercase = query.ToLower();
                var filter = itemsSourceDo.Where(x => x.DocNum.ToString().Contains(query)).ToList();
                if (filter == null)
                {
                    ResetListViewDo();
                    return;
                }
                if (filter.Count == 0)
                {
                    ResetListViewDo();
                    return;
                }

                ItemsSourceDo = new ObservableCollection<ODLN_Ex>(filter);
                OnPropertyChanged(nameof(ItemsSourceDo));
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
        void HandlerSearchTextReq(string query)
        {
            try
            {
                if (itemsSourceRReq == null) return;
                if (string.IsNullOrWhiteSpace(query))
                {
                    ResetListViewRReq();
                    return;
                }

                var lowercase = query.ToLower();
                var filter = itemsSourceRReq.Where(x => x.DocNum.ToString().Contains(query)).ToList();
                if (filter == null)
                {
                    ResetListViewRReq();
                    return;
                }
                if (filter.Count == 0)
                {
                    ResetListViewRReq();
                    return;
                }

                ItemsSourceRReq = new ObservableCollection<ORRR_Ex>(filter);
                OnPropertyChanged(nameof(ItemsSourceRReq));
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
        void HandlerSelectedDoDoc (ODLN_Ex selectedDoc)
        {
            if (itemsSourceDo == null) return;
            if (itemsSourceDo.Count == 0) return;

            int locid = itemsSourceDo.IndexOf(selectedDoc);
            if (locid < 0) return;

            itemsSourceDo[locid].IsChecked = !itemsSourceDo[locid].IsChecked;
            ResetListViewDo();
        }

        /// <summary>
        /// Handle the selected request documentation
        /// </summary>
        void HandlerSelectedRequestDoc(ORRR_Ex selectedDoc)
        {
            if (itemsSourceRReq == null) return;
            if (itemsSourceRReq.Count == 0) return;

            int locid = itemsSourceRReq.IndexOf(selectedDoc);
            if (locid < 0) return;

            itemsSourceRReq[locid].IsChecked = !itemsSourceRReq[locid].IsChecked;
            ResetListViewRReq();
        }
        
        /// <summary>
        /// Load inventory transfer request from web server
        /// </summary>
        async public void LoadTransferRequest(string filter = "")
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
                    request = "GetDORreqList",
                    getSoType = "O",
                    QueryStartDate = _startDate,
                    QueryEndDate = _endDate
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Return");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                // reet the list view 
                itemsSourceDo = new List<ODLN_Ex>();
                ResetListViewDo();

                itemsSourceRReq = new List<ORRR_Ex>();
                ResetListViewRReq();

                if (!isSuccess)
                {
                    DisplayAlert(content + "\n" + lastErrMessage);
                    return;
                }

                var dtoOrrr = JsonConvert.DeserializeObject<DTO_ORRR>(content);
                if (dtoOrrr == null)
                {
                    DisplayToast("There is error reading server info, please try again later, Thanks.");
                    return;
                }

                if (dtoOrrr.ODLN_Exs != null && dtoOrrr.ODLN_Exs.Length > 0)
                {
                    itemsSourceDo.AddRange(dtoOrrr.ODLN_Exs);
                    ResetListViewDo();
                }

                if (dtoOrrr.ORRR_Exs != null && dtoOrrr.ORRR_Exs.Length > 0)
                {
                    itemsSourceRReq.AddRange(dtoOrrr.ORRR_Exs);
                    ResetListViewRReq();
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
        void ResetListViewDo()
        {
            if (itemsSourceDo == null) return;

            IsReturnReqListVisible = false;
            IsDoListVisible = true;

            ItemsSourceDo = new ObservableCollection<ODLN_Ex>(itemsSourceDo);
            OnPropertyChanged(nameof(ItemsSourceDo));
        }

        /// <summary>
        /// Refresh the list view of open list
        /// </summary>
        void ResetListViewRReq()
        {
            if (itemsSourceRReq == null) return;

            IsReturnReqListVisible = true;
            IsDoListVisible = false;

            ItemsSourceRReq = new ObservableCollection<ORRR_Ex>(itemsSourceRReq);
            OnPropertyChanged(nameof(ItemsSourceRReq));
        }

        /// <summary>
        /// Display Alert
        /// </summary>
        /// <param name="message"></param>
        async void DisplayAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "Ok");

        void DisplayToast(string message) => DependencyService.Get<IToastMessage>()?.ShortAlert(message);
    }
}