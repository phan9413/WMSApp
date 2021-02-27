using Acr.UserDialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Models.SAP;
using WMSApp.Views.StandAloneTransfer;
using WMSApp.Views.Transfer;
using Xamarin.Forms;
namespace WMSApp.ViewModels.Transfer
{
    public class TransferListVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string pName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pName));

        #region Page view property binding        
        public Command CmdCancelTransfer { get; set; }
        public Command CmdShowRequestList { get; set; }
        public Command CmdDirectRequestList { get; set; }
        public Command CmdSearchBarVisible { get; set; }

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

        OWTQ_Ex selectedItem;
        public OWTQ_Ex SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;
                HandlerSelectedRequestDoc(selectedItem);

                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        zwaHoldRequest selectedItemPicked;
        public zwaHoldRequest SelectedItemPicked
        {
            get => selectedItemPicked;
            set
            {
                if (selectedItemPicked == value) return;
                selectedItemPicked = value;
                HandlerSelectedRequestDocPicked(selectedItemPicked);

                selectedItemPicked = null;
                OnPropertyChanged(nameof(SelectedItemPicked));
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

        public string SearchText { set => HandlerSearchText(value); }

        string dateRange;
        public string DateRange
        {
            get => dateRange;
            set
            {
                if (dateRange == value) return;
                dateRange = value;
                OnPropertyChanged(nameof(DateRange));
            }
        }

        bool openListVisible;
        public bool OpenListVisible
        {
            get => openListVisible;
            set
            {
                if (openListVisible == value) return;
                openListVisible = value;
                OnPropertyChanged(nameof(OpenListVisible));

            }
        }

        bool pickedListVisible;
        public bool PickedListVisible
        {
            get => pickedListVisible;
            set
            {
                if (pickedListVisible == value) return;
                pickedListVisible = value;
                OnPropertyChanged(nameof(PickedListVisible));

            }
        }

        List<OWTQ_Ex> itemsSource;
        List<zwaHoldRequest> itemsSourcePicked; // for display the picked list from diff table
        public ObservableCollection<zwaHoldRequest> ItemsSourcePicked { get; set; } // for display the picked list from diff table

        public ObservableCollection<OWTQ_Ex> ItemsSource { get; set; }

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
        public TransferListVM(INavigation navigation)
        {
            Navigation = navigation;
            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDate = DateTime.Now;
            IsExpanded = true;
            RefreshDateRange();
            InitCmd();
            LoadTransferRequest("o"); // init load, set for load all (open and closed)
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
            CmdDirectRequestList = new Command(LaunchStandAloneTransfer);

            CmdCancelTransfer = new Command(() => Navigation.PopAsync());
            CmdShowRequestList = new Command<string>((string actionName) =>
            {
                LoadTransferRequest(actionName);
                currentListStatus = actionName;
                RefreshDateRange();
            });

            CmdSearchBarVisible = new Command<SearchBar>((SearchBar sb) =>
            {
                sb.IsVisible = !sb.IsVisible;
                if (sb.IsVisible) sb.Focus();
            });
        }

        /// <summary>
        /// Launch Stand Alone Transfer
        /// </summary>
        async void LaunchStandAloneTransfer() => await Navigation.PushAsync(new StandAloneTransferLineFROMView());

        /// <summary>
        /// Search by doc #
        /// request doc
        /// </summary>
        /// <param name="query"></param>
        void HandlerSearchText(string query)
        {
            try
            {
                if (itemsSource == null) return;
                if (string.IsNullOrWhiteSpace(query))
                {
                    ResetListView();
                    return;
                }

                var lowercase = query.ToLower();
                var filter = itemsSource.Where(x => x.RequestDocument.DocNum.ToString().Contains(query)).ToList();
                if (filter == null)
                {
                    ResetListView();
                    return;
                }
                if (filter.Count == 0)
                {
                    ResetListView();
                    return;
                }

                ItemsSource = new ObservableCollection<OWTQ_Ex>(filter);
                OnPropertyChanged(nameof(ItemsSource));
            }
            catch (Exception excep)
            {
                DisplayAlert(excep.Message);
                Console.WriteLine($"{excep}");
            }
        }

        /// <summary>
        /// Handle the selected request documentation
        /// </summary>
        void HandlerSelectedRequestDoc(OWTQ_Ex selectedDoc) => 
            Navigation.PushAsync(new TransferListLineView(selectedDoc, "FROM"));

        /// <summary>
        /// Handler Selected Request Doc Picked 
        /// picked doc 
        /// </summary>
        /// <param name="selected"></param>
        void HandlerSelectedRequestDocPicked(zwaHoldRequest selected) => 
            Navigation.PushAsync(new TransferListLineView(selected, "TO"));
        
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
                    RequestTransferDocFilter = (string.IsNullOrWhiteSpace(filter)) ? currentListStatus: filter,
                    request = "GetTransferRequestList",
                    RequestTransferStartDt = _startDate,
                    RequestTransferEndDt = _endDate
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Transfer");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    DisplayAlert(content + "\n" + lastErrMessage);
                    return;
                }

                var retBag = JsonConvert.DeserializeObject<Cio>(content);
                // add in the logic to bind different listview
                if (currentListStatus.Equals("o"))
                {
                    if (retBag.TransferRequestList == null) return;

                    itemsSource = new List<OWTQ_Ex>();
                    foreach (var requestDoc in retBag.TransferRequestList)
                    {
                        itemsSource.Add(new OWTQ_Ex { RequestDocument = requestDoc });
                    }

                    ResetListView();                   
                    return;
                }

                // activate the picke list
                // if (currentListStatus.Equals("h"))
                if (retBag.dtozwaHoldRequests == null) return;
                itemsSourcePicked = new List<zwaHoldRequest>();
                itemsSourcePicked.AddRange(retBag.dtozwaHoldRequests);
                ResetListViewPicked();                
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
        void ResetListViewPicked()
        {
            if (itemsSourcePicked == null) return;
            OpenListVisible = false;
            PickedListVisible = true;            
            ItemsSourcePicked = new ObservableCollection<zwaHoldRequest>(itemsSourcePicked);
            OnPropertyChanged(nameof(ItemsSourcePicked));
        }

        /// <summary>
        /// Refresh the list view of open list
        /// </summary>
        void ResetListView()
        {
            if (itemsSource == null) return;
            OpenListVisible = true;
            PickedListVisible = false;            
            ItemsSource = new ObservableCollection<OWTQ_Ex>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// Display Alert
        /// </summary>
        /// <param name="message"></param>
        async void DisplayAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "Ok");
    }
}
