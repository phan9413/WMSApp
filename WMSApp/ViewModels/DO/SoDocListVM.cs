using Acr.UserDialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Dtos;
using WMSApp.Models.SAP;
using WMSApp.Views.DeliveryOrder;
using WMSApp.Views.DO;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace WMSApp.ViewModels.SalesOrder
{
    public class SoDocListVM :  INotifyPropertyChanged
    {
        #region Property binding
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public Command CmdSearchBarVisible { get; set; } = null;
        public Command CmdRefreshList { get; set; } = null;
        public Command CmdRefreshListView { get; set; } = null;
        public Command CmdSelectSOs { get; set; } = null;
        public Command CmdManualDo { get; set; } = null;
        public Command CmdDoPicklist { get; set; } = null;

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

        ORDR_Ex selectedItem;
        public ORDR_Ex SelectedItem
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

        List<ORDR_Ex> itemsSource;
        public ObservableCollection<ORDR_Ex> ItemsSource { get; set; }
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
        public SoDocListVM(INavigation navigation, string pageType)
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
            LoadSOList();
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
                LoadSOList();
                IsListViewRefreshing = false;
            });

            CmdRefreshList = new Command<string>((string listType) =>
            {
                if (listType.Equals("a"))
                {
                    PageType = "ALL";
                    CurrentListStatus = "a";
                    LoadSOList();
                    RefreshDateRange();
                    return;
                }

                if (listType.Equals("o"))
                {
                    PageType = "OPEN";
                    CurrentListStatus = "o";
                    LoadSOList();
                    RefreshDateRange();
                    return;
                }
                if (listType.Equals("c"))
                {
                    PageType = "CLOSED";
                    CurrentListStatus = "c";
                    LoadSOList();
                    RefreshDateRange();
                    return;
                }
            });

            CmdSelectSOs = new Command(HandlerSelectedSOs);
            CmdManualDo = new Command( () => 
            {
                Navigation.PushAsync(new DoWithNoSoView());
            });

            CmdDoPicklist = new Command(() => { ShowAlert("Under construction"); });
    }

    /// <summary>
    /// HandlerSelectedSOs
    /// generate the checked SO list
    /// </summary>
    void HandlerSelectedSOs()
        {
            try
            {
                if (itemsSource == null) return;
                if (itemsSource.Count == 0) return;

                var selectedSOs = itemsSource.Where(x => x.IsChecked).ToList();
                if (selectedSOs == null) return;
                if (selectedSOs.Count == 0) return;

                // detect same card code
                var distinct = selectedSOs.Select(x => x.CardCode).Distinct().ToList();
                if (distinct != null && distinct.Count > 1)
                {
                    ShowAlert("Please select same client SO to process mutiple DO");
                    itemsSource.ForEach(x => x.IsChecked = false);
                    return;
                }

                Navigation.PushAsync(new SoItemsView(selectedSOs));
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
        void HandlerSelectedItem(ORDR_Ex selected)
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
        /// Handler the sear SO
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

                ItemsSource = new ObservableCollection<ORDR_Ex>(filterList);
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
        public async void LoadSOList()
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
                    request = "GetOpenSo",
                    getSoType = GetSoStatus(),
                    QueryStartDate = _startDate,
                    QueryEndDate = _endDate
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Do");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    ShowAlert(content);
                    return;
                }

                // when success
                var SOs = JsonConvert.DeserializeObject<DTO_ORDR>(content);
                if (SOs == null) return;
                    
                if (SOs.ORDR_Exs == null) return;
                if (SOs.ORDR_Exs.Length == 0) return;

                itemsSource = new List<ORDR_Ex>(SOs.ORDR_Exs);
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
        /// Return the SO Status
        /// </summary>
        /// <returns></returns>
        string GetSoStatus()
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

            ItemsSource = new ObservableCollection<ORDR_Ex>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        ///  display the message on screen
        /// </summary>
        /// <param name="message"></param>
        async void ShowAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "Ok");

        public void Dispose() { }// => GC.Collect();

        //public event PropertyChangedEventHandler PropertyChanged;
        //public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //public Command CmdSearchBarVisible { get; set; } = null;



        //bool searchBarVisible;
        //public bool SearchBarVisible
        //{
        //    get => searchBarVisible;
        //    set
        //    {
        //        if (searchBarVisible != value)
        //        {
        //            searchBarVisible = value;
        //            OnPropertyChanged(nameof(searchBarVisible));
        //        }
        //    }
        //}

        //string searchText;
        //public string SearchText
        //{
        //    get => searchText;
        //    set
        //    {
        //        if (searchText != value)
        //        {
        //            searchText = value;
        //            HandlerSearchQuery(searchText);
        //        }
        //    }
        //}

        //ORDR_Ex selectedItem;
        //public ORDR_Ex SelectedItem
        //{
        //    get => selectedItem;
        //    set
        //    {
        //        if (selectedItem != value)
        //        {
        //            selectedItem = value;

        //            // handle the search bar when user select an item
        //            SearchText = string.Empty;
        //            OnPropertyChanged(nameof(SearchText));
        //            ResetSearchBar(); // reset the search bar

        //            // handle the selected Item
        //            HandlerSelectedItem(selectedItem);

        //            selectedItem = null;
        //            OnPropertyChanged(nameof(SelectedItem));
        //        }
        //    }
        //}

        //List<ORDR_Ex> itemSource;
        //public ObservableCollection<ORDR_Ex> ItemsSource { get; set; }

        //SearchBar SearchBar;
        //INavigation Navigation;
        //string PageType;

        ///// <summary>
        ///// The constructor
        ///// </summary>
        ///// <param name="navigation"></param>
        ///// <param name="sbar"></param>
        //public SoDocListVM(INavigation navigation, SearchBar sbar, string pageType)
        //{
        //    Navigation = navigation;
        //    SearchBar = sbar;
        //    PageType = pageType;
        //    SearchBar.IsVisible = false;           
        //    InitCmd();
        //}

        //void InitCmd()
        //{
        //    CmdSearchBarVisible = new Command(() =>
        //    {
        //        ResetSearchBar();
        //    });
        //}

        //void ResetSearchBar()
        //{
        //    SearchBar.IsVisible = !SearchBar.IsVisible;
        //    if (SearchBar.IsVisible)
        //    {
        //        SearchBar.Focus();
        //    }
        //}

        //void HandlerSelectedItem(ORDR_Ex selected)
        //{
        //    // load the so item list and start on new page
        //    Navigation.PushAsync(new SoItemsView(selected)); 
        //}

        //void HandlerSearchQuery(string query)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(query))
        //        {
        //            ResetListView();
        //            return;
        //        }

        //        if (itemSource == null) return;

        //        var lowerCaseQuery = query.ToLower();
        //        var filterList = itemSource
        //                        .Where(x => x.Text.ToLower().Contains(lowerCaseQuery) ||
        //                                x.Details.ToLower().Contains(lowerCaseQuery)).ToList();

        //        if (filterList == null)
        //        {
        //            ResetListView();
        //            return;
        //        }

        //        ItemsSource = new ObservableCollection<ORDR_Ex>(filterList);
        //        OnPropertyChanged(nameof(ItemsSource));
        //    }
        //    catch (Exception excep)
        //    {
        //        ShowAlert($"{excep}");
        //    }
        //}

        ///// <summary>
        ///// Load the SO list from the server
        ///// </summary>
        //public async void LoadSOList()
        //{
        //    try
        //    {
        //        UserDialogs.Instance.ShowLoading("A moment please...");
        //        var cioRequest = new Cio() // load the data from web server 
        //        {
        //            sap_logon_name = App.waUser,
        //            sap_logon_pw = App.waPw,
        //            token = App.waToken,
        //            phoneRegID = App.waToken,
        //            request = "GetOpenSo",
        //            getSoType = GetSoStatus()
        //        };

        //        string content, lastErrMessage;
        //        bool isSuccess = false;
        //        using (var httpClient = new HttpClientWapi())
        //        {
        //            content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Do");
        //            isSuccess = httpClient.isSuccessStatusCode;
        //            lastErrMessage = httpClient.lastErrorDesc;
        //        }

        //        if (isSuccess)
        //        {
        //            var retBag = JsonConvert.DeserializeObject<Cio>(content);
        //            if (retBag.So == null) return;

        //           // if (retBag.dtoWhs != null) App.Warehouses = retBag.dtoWhs;

        //            itemSource = new List<ORDR_Ex>();
        //            foreach (var so in retBag.So)
        //            {
        //                var newOrder = new ORDR_Ex(so);
        //                itemSource.Add(newOrder);
        //            }

        //            ResetListView();
        //            UserDialogs.Instance.HideLoading();
        //            return;
        //        }

        //        //ELSE
        //        var bRequest = JsonConvert.DeserializeObject<BRequest>(content);
        //        lastErrMessage = $"{lastErrMessage}\n{bRequest?.Message}";
        //        ShowAlert($"{lastErrMessage}");
        //    }
        //    catch (Exception excep)
        //    {
        //        ShowAlert($"{excep}");
        //    }
        //    finally
        //    {
        //        UserDialogs.Instance.HideLoading();
        //    }
        //}

        //string GetSoStatus()
        //{
        //    switch (PageType)
        //    {
        //        case "ALL": return "";
        //        case "OPEN": return "O";
        //        case "CLOSED": return "C";
        //    }
        //    return "";
        //}

        //void ResetListView()
        //{
        //    if (itemSource != null)
        //    {
        //        ItemsSource = new ObservableCollection<ORDR_Ex>(itemSource);
        //        OnPropertyChanged(nameof(ItemsSource));
        //    }
        //}

        //void ShowAlert(string message)
        //{
        //    _ = new Dialog().DisplayAlert("Alert", message, "Ok");
        //}

    }
}
