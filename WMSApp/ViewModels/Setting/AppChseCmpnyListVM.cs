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
using WMSApp.ViewModels.Setting.AppUser;
using WMSApp.ViewModels.Setting.AppUserGroup;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Setting
{
    public class AppChseCmpnyListVM : IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        List<OADM_Ex> _list;
        public ObservableCollection<OADM_Ex> list { get; set; }

        string _searchText;
        public string searchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    HandlerSearchText(_searchText);
                }
            }
        }

        OADM_Ex _selectedItem;
        public OADM_Ex selectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                if (_selectedItem == null) return;

                SetSelectedCompany(_selectedItem);
                selectedItem = null;
                OnPropertyChanged(nameof(selectedItem));
            }
        }

        public Command cmdSetVisibleSearchBar { get; private set; }
        bool _searchBarVisibility = false;
        public bool searchBarVisibility
        {
            get
            {
                return _searchBarVisibility;
            }
            set
            {
                _searchBarVisibility = value;
                OnPropertyChanged(nameof(searchBarVisibility));
            }
        }

        // inner declaration 
        enum ModeObject
        {
            user,
            group
        }

        ModeObject current_mode;
        INavigation Navigation;        
        AppUserListVM parent_user;
        AppGroupListVM parent_group;

        /// <summary>
        /// Construtor for user screen
        /// </summary>
        public AppChseCmpnyListVM(INavigation navigation, AppUserListVM caller)
        {
            Navigation = navigation;
            parent_user = caller;
            LoadCompanyListFromWepApi();
            current_mode = ModeObject.user;
            InitCmd();
        }

        ///// <summary>
        ///// Construtor for group screen
        ///// </summary>
        public AppChseCmpnyListVM(INavigation navigation, AppGroupListVM caller)
        {
            Navigation = navigation;
            parent_group = caller;
            LoadCompanyListFromWepApi();
            current_mode = ModeObject.group;
            InitCmd();
        }

        /// <summary>
        /// Init the command
        /// </summary>
        void InitCmd() => cmdSetVisibleSearchBar = new Command<SearchBar>((SearchBar searchBar) => HandlerSearchBarVisible(searchBar));

        /// <summary>
        /// Handler searchbar visibilities
        /// </summary>
        void HandlerSearchBarVisible(SearchBar searchbar)
        {
            _searchBarVisibility = !_searchBarVisibility;
            OnPropertyChanged(nameof(searchBarVisibility));
            if (_searchBarVisibility) searchbar?.Focus();
        }

        /// <summary>
        /// Set selected company and pop out the screen
        /// </summary>
        /// <param name="selected"></param>
        void SetSelectedCompany(OADM_Ex selected)
        {
            /// can be replace by the messaging center 

            if (current_mode.Equals(ModeObject.user) && parent_user != null)
            {
                parent_user.curOADMCompany = selected;
            }

            /// can be replace by the messaging center 
            if (current_mode.Equals(ModeObject.group) && parent_group != null)
            {
                parent_group.curOADMCompany = selected;
            }

            UserDialogs.Instance.HideLoading();
            Navigation.PopAsync();
        }

        /// <summary>
        /// Handle the listview search
        /// </summary>
        /// <param name="query"></param>
        void HandlerSearchText(string query)
        {

            if (_list == null) return;
            if (string.IsNullOrWhiteSpace(query))
            {
                list = new ObservableCollection<OADM_Ex>(_list);
            }
            else
            {
                var filteredList = _list
                    .Where(x =>
                       x.TextDisplay.ToLower().Contains(query.ToLower()) ||
                       x.DetailsDisplay.ToLower().Contains(query.ToLower())).ToList();

                if (filteredList != null)
                    list = new ObservableCollection<OADM_Ex>(filteredList);
            }
            OnPropertyChanged(nameof(list));
        }

        /// <summary>
        /// Display the message to client screen
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="btnStr"></param>
        async void DisplayAlert(string title, string message, string accept) => await new Dialog().DisplayAlert(title, message, accept);

        /// <summary>
        /// Connect to web server and get list of configured company
        /// </summary>
        async void LoadCompanyListFromWepApi()
        {
            UserDialogs.Instance.ShowLoading("A Moment Please ...");
            try
            {
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    request = "QueryAppCompany"
                };

                // reset the list 
                _list = null;
                list = null;
                OnPropertyChanged(nameof(list));

                var lastErroMessage = string.Empty;
                var content = string.Empty;
                var isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "AppCompanySetup");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErroMessage = httpClient.lastErrorDesc;
                }

                if (isSuccess)
                {
                    App.cio = JsonConvert.DeserializeObject<Cio>(content);  // < -- set the global variable                           
                    if (App.cio.oADM_CompanyInfoList != null)
                    {
                        // if only one company then, auto select the company
                        if (App.cio.oADM_CompanyInfoList.Length == 1)
                        {
                            SetSelectedCompany(App.cio.oADM_CompanyInfoList[0]);
                            return;
                        }

                        // else
                        _list = new List<OADM_Ex>();
                        _list.AddRange(App.cio.oADM_CompanyInfoList);
                        list = new ObservableCollection<OADM_Ex>(_list);

                        OnPropertyChanged(nameof(list));
                    }
                    UserDialogs.Instance.HideLoading();
                    return;
                }

                await new Dialog().DisplayAlert("Alert", content, "OK");               
                UserDialogs.Instance.HideLoading();
            }
            catch (Exception excep)
            {
                ShowToast($"{excep.Message}");
            }
            finally
            {
                UserDialogs.Instance.HideLoading();                    
            }
        }

        /// <summary>
        /// Show toast on the user screen
        /// </summary>
        /// <param name="v"></param>
        void ShowToast(string v)
        {
            var itoast = DependencyService.Get<IToastMessage>();
            itoast?.ShortAlert(v);
        }

        /// <summary>
        /// Handle the on property changed, value update to screen
        /// </summary>
        /// <param name="propertyname"></param>
        public void OnPropertyChanged(string propertyname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));

        /// <summary>
        /// Class disposal 
        /// </summary>
        public void Dispose() { }// => GC.Collect();
    }
}
