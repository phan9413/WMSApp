using Acr.UserDialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Models.SAP;
using WMSApp.Views.GRPO;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace WMSApp.Models.GRPO
{
    public class GrpoDocListVM : INotifyPropertyChanged
    {
        #region Property binding
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public Command CmdSearchBarVisible { get; set; } = null;
        public Command CmdRefreshList { get; set; } = null;
        public Command CmdRefreshListView { get; set; } = null;
        public Command CmdSelectPOs { get; set; } = null;
        public Command CmdManualGrpo { get; set; } = null;

        bool searchBarVisible;
        public bool SearchBarVisible
        {
            get => searchBarVisible;
            set
            {
                if (searchBarVisible != value) return;
                searchBarVisible = value;
                OnPropertyChanged(nameof(searchBarVisible));
            }
        }

        public string SearchText
        {
            set => HandlerSearchQuery(value);
        }

        OPOR_Ex selectedItem;
        public OPOR_Ex SelectedItem
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

        bool isListViewRefreshing;
        public bool IsListViewRefreshing
        {
            get => isListViewRefreshing;
            set
            {
                if (isListViewRefreshing != value) return;
                isListViewRefreshing = value;
                OnPropertyChanged(nameof(IsListViewRefreshing));
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
                RefreshDateRange();
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

        List<OPOR_Ex> itemsSource;
        public ObservableCollection<OPOR_Ex> ItemsSource { get; set; }
        #endregion

        /// <summary>
        /// Private declaration
        /// </summary>
        INavigation Navigation { get; set; } = null;
        string PageType { get; set; } = string.Empty;
        string CurrentListStatus { get; set; } = string.Empty;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="navigation"></param>
        /// <param name="sbar"></param>
        public GrpoDocListVM(INavigation navigation, string pageType)
        {
            Navigation = navigation;
            PageType = pageType;

#if DEBUG
            _startDate = new DateTime(2014, 1, 1); //DateTime.Now.AddDays(-7);
#else   
            _startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
#endif
            _endDate = DateTime.Now;
            IsExpanded = true;

            InitCmd();
            RefreshDateRange();
        }

        /// <summary>
        /// Refresh the page date time 
        /// </summary>
        void RefreshDateRange()
        {
            DateRange = $"From {_startDate:dd/MM/yyyy} to {_endDate:dd/MM/yyyy} [Open]";
            LoadPOList();
        }

        /// <summary>
        /// Init command
        /// </summary>
        void InitCmd()
        {
            CmdSearchBarVisible = new Command<SearchBar>((SearchBar searchBar) =>
            {
                searchBar.IsVisible = !searchBar.IsVisible;
                if (searchBar.IsVisible) searchBar.Focus();
            });

            CmdRefreshListView = new Command(() =>
            {
                IsListViewRefreshing = true;
                LoadPOList();
                IsListViewRefreshing = false;
            });

            CmdRefreshList = new Command<string>((string listType) =>
            {
                if (listType.Equals("a"))
                {
                    PageType = "ALL";
                    CurrentListStatus = "a";
                    LoadPOList();
                    RefreshDateRange();
                    return;
                }

                if (listType.Equals("o"))
                {
                    PageType = "OPEN";
                    CurrentListStatus = "o";
                    LoadPOList();
                    RefreshDateRange();
                    return;
                }
                if (listType.Equals("c"))
                {
                    PageType = "CLOSED";
                    CurrentListStatus = "c";
                    LoadPOList();
                    RefreshDateRange();
                    return;
                }
            });

            CmdSelectPOs = new Command(HandlerSelectedPOs);
            CmdManualGrpo = new Command(() =>
            {
               Navigation.PushAsync(new GrpoWithNoPoView());
            });
        }

        /// <summary>
        /// HandlerSelectedPOs
        /// generate the checked PO list
        /// </summary>
        void HandlerSelectedPOs()
        {
            try
            {
                if (itemsSource == null) return;
                if (itemsSource.Count == 0) return;

                var selectedPOs = itemsSource.Where(x => x.IsChecked).ToList();
                if (selectedPOs == null) return;
                if (selectedPOs.Count == 0) return;

                // detect same card code
                var distinct = selectedPOs.Select(x => x.PO.CardCode).Distinct().ToList();
                if (distinct != null && distinct.Count > 1)
                {
                    ShowAlert("Please select same vendor PO to process mutiple GRPO");
                    itemsSource.ForEach(x => x.IsChecked = false);
                    return;
                }

                Navigation.PushAsync(new GrpoItemsView(selectedPOs));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }
        }

        /// <summary>
        /// prompt to select warehouse before enter the tem list
        /// </summary>
        /// <param name="selected"></param>
        void HandlerSelectedItem(OPOR_Ex selected)
        {
            try
            {
                if (itemsSource == null) return;
                if (itemsSource.Count == 0) return;
                int locId = itemsSource.IndexOf(selected);
                if (locId < 0) return;

                itemsSource[locId].IsChecked = !itemsSource[locId].IsChecked;
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }
        }

        /// <summary>
        /// Handler the sear PO
        /// </summary>
        /// <param name="query"></param>
        void HandlerSearchQuery(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    ResetListView();
                    return;
                }

                if (itemsSource == null) return;
                if (itemsSource.Count == 0) return;

                var lowerCaseQuery = query.ToLower();
                var filterList = itemsSource
                                .Where(x => x.Text.ToLower().Contains(lowerCaseQuery) ||
                                        x.Details.ToLower().Contains(lowerCaseQuery)).ToList();

                if (filterList == null)
                {
                    ResetListView();
                    return;
                }

                ItemsSource = new ObservableCollection<OPOR_Ex>(filterList);
                OnPropertyChanged(nameof(ItemsSource));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                ShowAlert(excep.Message);
            }
        }

        /// <summary>
        /// Load the PO list from the server
        /// </summary>
        public async void LoadPOList()
        {
            try
            {
                UserDialogs.Instance.ShowLoading("A moment please...");
                var cioRequest = new Cio() // loathe data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "GetOpenPo",
                    getPoType = GetPoStatus(),
                    QueryStartDate = _startDate,
                    QueryEndDate = _endDate
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
                    ShowAlert(content);
                    return;
                }

                // when success
                var POs = JsonConvert.DeserializeObject<DtoGrpo>(content);
                if (POs == null) return;

                if (POs.OPORs == null) return;
                if (POs.OPORs.Length == 0) return;

                itemsSource = new List<OPOR_Ex>();
                POs.OPORs.ForEach(x => itemsSource.Add(new OPOR_Ex { PO = x }));

                ResetListView();
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
        /// Return the PO Status
        /// </summary>
        /// <returns></returns>
        string GetPoStatus()
        {
            switch (PageType)
            {
                case "ALL": return "";
                case "OPEN": return "O";
                case "CLOSED": return "C";
            }
            return "";
        }

        /// <summary>
        /// Refresh the list view binding
        /// </summary>
        void ResetListView()
        {
            if (itemsSource == null) return;
            if (itemsSource.Count == 0) return;

            ItemsSource = new ObservableCollection<OPOR_Ex>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        ///  display the message on screen
        /// </summary>
        /// <param name="message"></param>
        async void ShowAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "Ok");

    }
}
