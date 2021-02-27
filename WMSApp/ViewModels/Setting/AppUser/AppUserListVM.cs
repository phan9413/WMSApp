using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.Login;
using WMSApp.Models.SAP;
using WMSApp.Views.Setting;
using WMSApp.Views.Setting.AppUser;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Setting.AppUser
{
    public class AppUserListVM : IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string pName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pName));
        public void Dispose() { }// => GC.Collect();

        #region public / bindable decalration
        /// <summary>
        /// View bindable related declaration
        /// </summary>

        List<zwaUser> _list;
        public ObservableCollection<zwaUser> list { get; private set; }
        public Command cmdAddNewUser { get; private set; }

        /// Search bar related properties
        /// <summary>
        /// view search bar
        /// </summary>
        public Command cmdVisibleSearchBar { get; private set; }
        bool _searchBarVisible { get; set; }
        public bool searchBarVisible
        {
            get
            {
                return _searchBarVisible;
            }
            set
            {
                if (_searchBarVisible != value)
                {
                    _searchBarVisible = value;
                    OnPropertyChanged(nameof(searchBarVisible));
                }
            }
        }
        string _searchText { get; set; }
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
                    HandlerSearchBarTextChanged(_searchText);
                }
            }
        }
        public Command cmdListViewTapped { get; private set; }
        zwaUser _selectedItem { get; set; }
        public zwaUser selectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                if (_selectedItem == null)
                    return;

                HandlerLvSelectedItem(_selectedItem);
                selectedItem = null;
                OnPropertyChanged(nameof(selectedItem));
            }
        }

        public Command cmdCompanyTapped { get; private set; }

        OADM_Ex _curOADMCompany;
        public OADM_Ex curOADMCompany
        {
            get
            {
                return _curOADMCompany;
            }
            set
            {
                _curOADMCompany = value;
                LoadUsrListFromServer();
                OnPropertyChanged(nameof(curOADMCompany));
            }
        }

        #endregion
        INavigation Navigation;

        /// <summary>
        /// Constructor
        /// </summary>
        public AppUserListVM(INavigation navigation)
        {
            Navigation = navigation;
            initCmd();
            StartTimer_LoadCompany();
        }

        /// <summary>
        /// Handle the list tapped item selected
        /// </summary>
        /// <param name="selected"></param>
        void HandlerLvSelectedItem(zwaUser selected)
        {
            Navigation.PushAsync(new AppUserAddView(this, selected));
        }

        /// <summary>
        /// Initial the command
        /// </summary>
        void initCmd()
        {
            // handle the add command, where load the add user page
            cmdAddNewUser = new Command(HanlderAddNewUser);

            // handle the search bar visibilities toggler
            cmdVisibleSearchBar = new Command<SearchBar>((SearchBar searchbar) => HandlerToggleSearchBar(searchbar));

            // handler the listview header tapped
            cmdCompanyTapped = new Command(HanlderListViewHeaderTapped);
        }

        /// <summary>
        /// Show the choose company page for user to swap compnay
        /// </summary>
        void HanlderListViewHeaderTapped()
        {
            Navigation.PushAsync(new AppChseCmpnyListView(this));
        }

        /// <summary>
        /// Handle the command, in structire readable way
        /// </summary>
        /// <param name="searchBar"></param>
        void HandlerToggleSearchBar(SearchBar searchBar)
        {
            searchBarVisible = !searchBarVisible;
            if (searchBarVisible) searchBar?.Focus();
            else
            {
                if (_list == null) return;

                list = new ObservableCollection<zwaUser>(_list);
                _searchText = "";
                OnPropertyChanged(nameof(searchText));
                OnPropertyChanged(nameof(list));
            }

            OnPropertyChanged(nameof(searchBarVisible));
        }

        /// <summary>
        /// perform search to the user list and filter
        /// </summary>
        void HandlerSearchBarTextChanged(string query)
        {
            try
            {
                if (_list == null) return;
                if (String.IsNullOrWhiteSpace(query))
                {
                    list = new ObservableCollection<zwaUser>(_list);
                }
                else
                {
                    string lowerCase = query.ToLower();
                    var filtered_list = _list
                            .Where(x =>
                                    x.TextDisplay.ToLower().Contains(lowerCase) ||
                                    x.DetailsDisplay.ToLower().Contains(lowerCase))
                            .ToList();

                    list = new ObservableCollection<zwaUser>(filtered_list);
                }

                OnPropertyChanged(nameof(list));
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "Ok");
            }
        }

        /// <summary>
        /// Setup timer to load company from the screen
        /// </summary>
        void StartTimer_LoadCompany()
        {
            try
            {
                Device.StartTimer(TimeSpan.FromMilliseconds(600), () =>
                {
                    Navigation.PushAsync(new AppChseCmpnyListView(this));
                    return false; // return true;
                });
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "Ok");
            }
        }

        /// <summary>
        /// Push the add user view
        /// </summary>
        async void HanlderAddNewUser()
        {
            if (_curOADMCompany == null)
            {
                DisplayAlert("Alert", "Please choose a company before add new user, Thanks", "OK");
                return;
            }
            await Navigation.PushAsync(new AppUserAddView(this));
        }


        /// <summary>
        /// Perform loading of the record from the server
        /// use be triger by the user add screen when add screen finish the add or update operation
        /// </summary>
        public async void LoadUsrListFromServer()
        {
            try
            {
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    companyName = curOADMCompany.CompnyName,
                    request = "QueryAppUserList"
                };

                var content = string.Empty;
                var isSuccess = false;
                var lastErrorMessage = string.Empty;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "AppUsersSetup");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrorMessage = httpClient.lastErrorDesc;
                }

                // reset the properties first
                _list = null;
                list = null;
                OnPropertyChanged(nameof(list));
                OnPropertyChanged(nameof(curOADMCompany));

                if (isSuccess)
                {
                    App.cio = JsonConvert.DeserializeObject<Cio>(content);  // < -- set the global variable     
                    zwaUser[] app_users = App.cio.zwAppUsers;

                    if (app_users == null) return;
                    if (app_users.Length == 0) return;

                    _list = new List<zwaUser>();
                    _list.AddRange(app_users);
                    list = new ObservableCollection<zwaUser>(_list);
                    OnPropertyChanged(nameof(list));
                    return;
                }

                BRequest br = JsonConvert.DeserializeObject<BRequest>(content);  // < -- set the global variable                    
                DisplayAlert("Alert", $"{br?.Message}\n{lastErrorMessage}", "OK");
            }
            catch (Exception excep)
            {
                ShowToastMessage($"{excep}");
            }
        }

        /// <summary>
        /// pass message to the app screen for alert inform
        /// </summary>
        /// <param name="message"></param>
        void ShowToastMessage(string message)
        {
            if (String.IsNullOrWhiteSpace(message)) return;

            var toast = DependencyService.Get<IToastMessage>();
            toast?.ShortAlert(message);
        }

        /// <summary>
        /// Simulate the display alert util from viewmodel
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="btnStr"></param>
        async void DisplayAlert(string title, string message, string accept) =>
            await new Dialog().DisplayAlert(title, message, accept);

    }
}
