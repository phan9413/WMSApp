using Acr.UserDialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.SAP;
using WMSApp.Views.InventoryCounts;
using Xamarin.Forms;

namespace WMSApp.ViewModels.InventoryCounts
{
    public class InventoryCountsDocListVM : IDisposable, INotifyPropertyChanged
    {
        #region View binding
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Command CmdSearchBarVisible { get; set; } = null;
        public Command CmdRefreshListView { get; set; }
        
        /// <summary>
        /// Set only
        /// </summary>
        public string SearchText
        {
            set => HandlerSearchQuery(value);
        }

        OINC_Ex selectedItem;
        public OINC_Ex SelectedItem
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
                if (isListViewRefreshing == value) return;
                isListViewRefreshing = value;
                OnPropertyChanged(nameof(IsListViewRefreshing));
            }
        }

        List<OINC_Ex> itemSource;
        public ObservableCollection<OINC_Ex> ItemsSource { get; set; }
        #endregion

        INavigation Navigation { get; set; } = null;
        string PageType { get; set; } = string.Empty;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="navigation"></param>
        /// <param name="sbar"></param>
        public InventoryCountsDocListVM(INavigation navigation, string pageType)
        {
            Navigation = navigation;
            PageType = pageType;
            InitCmd();            
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
                GetInvtryCountDoc();
                IsListViewRefreshing = false;
            });
        }

        /// <summary>
        /// prompt to select warehouse before enter the tem list
        /// </summary>
        /// <param name="selected"></param>
        void HandlerSelectedItem(OINC_Ex selected) => Navigation.PushAsync(new InventoryCountsItemView(selected));

        /// <summary>
        /// Search query
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

                if (itemSource == null) return;

                var lowerCaseQuery = query.ToLower();
                var filterList = itemSource
                                .Where(x => x.Text.ToLower().Contains(lowerCaseQuery) ||
                                        x.Details.ToLower().Contains(lowerCaseQuery)).ToList();

                if (filterList == null)
                {
                    ResetListView();
                    return;
                }

                ItemsSource = new ObservableCollection<OINC_Ex>(filterList);
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
        public async void GetInvtryCountDoc()
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
                    request = "GetInvtryCountDoc",
                    OINCStatus = GetStatus()
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "InvntryCnt");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    ShowAlert(content);
                    return;
                }

                var retBag = JsonConvert.DeserializeObject<Cio>(content);
                if (retBag.InvenCountDocs == null) return;

                itemSource = new List<OINC_Ex>();
                itemSource.AddRange(retBag.InvenCountDocs);

                ResetListView();

                // collect the bin location here
                //if (retBag.LocationBin == null) return;
                //App.BinLocations = retBag.LocationBin;
            }
            catch (Exception excep)
            {
                ShowAlert($"{excep}");
                Console.WriteLine(excep.Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        

        /// <summary>
        /// Get the doc status
        /// </summary>
        /// <returns></returns>
        string GetStatus()
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
        /// Refresh list view
        /// </summary>
        void ResetListView()
        {
            if (itemSource == null) return;
            ItemsSource = new ObservableCollection<OINC_Ex>(itemSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// Show alert on user screen
        /// </summary>
        /// <param name="message"></param>
        async void ShowAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "Ok");

        /// <summary>
        /// Show toast message to screen
        /// </summary>
        /// <param name="message"></param>
        void ShowToast(string message) => DependencyService.Get<IToastMessage>()?.LongAlert(message);

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose() { }// => GC.Collect();
    }
}
