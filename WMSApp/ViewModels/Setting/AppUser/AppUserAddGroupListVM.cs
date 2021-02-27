using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Models.Setup.Group;
using WMSApp.ViewModels.Setting.AppUser;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Setting.AppUserGroup
{
    public class AppUserAddGroupListVM  : IDisposable, INotifyPropertyChanged
    {
        #region Page declaration binding property
        public event PropertyChangedEventHandler PropertyChanged;
        public Command cmdSearchBarVisible { get; private set; }
        bool _searchBarIsVisible;
        public bool searchBarIsVisible
        {
            get
            {
                return _searchBarIsVisible;
            }
            set
            {
                _searchBarIsVisible = value;
            }
        }

        zwaUserGroup _selecteditem;
        public zwaUserGroup selectedItem
        {
            get
            {
                return _selecteditem;
            }
            set
            {
                _selecteditem = value;
                if (_selecteditem == null)
                    return;

                HandlerSelectedItem(_selecteditem);

                selectedItem = null;
                OnPropertyChanged(nameof(selectedItem));
            }
        }

        string _curCompany;
        public string curCompany
        {
            get
            {
                return _curCompany;
            }
            set
            {
                _curCompany = value;
                OnPropertyChanged(nameof(curCompany));
            }
        }

        string _curGroup;
        public string curGroup
        {
            get
            {
                return _curGroup;
            }
            set
            {
                _curGroup = value;
                OnPropertyChanged(nameof(curGroup));
            }
        }

        List<zwaUserGroup> list;
        public ObservableCollection<zwaUserGroup> obcList { get; set; }
        #endregion

        INavigation Navigation;
        AppUserAddVM caller;
        string curCompanyName;

        /// <summary>
        /// The construtor
        /// </summary>
        public AppUserAddGroupListVM(INavigation navigation, AppUserAddVM source,
           string userName, string curGrpName, string currentCoy)
        {
            Navigation = navigation;
            caller = source;
            curGroup = curGrpName;
            curCompany = "User " + userName + " @ " + currentCoy + " in " + curGroup;
            curCompanyName = currentCoy;

            InitCmd();
            Init();
        }

        /// <summary>
        /// Load the company group from list 
        /// excep the current group
        /// </summary>
        async void Init()
        {
            try
            {
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    companyName = curCompanyName,
                    request = "QueryAppGroupList"
                };

                var content = string.Empty;
                var lastErrorMessage = string.Empty;
                var isSuccess = false;

                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "AppUserGroup");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrorMessage = httpClient.lastErrorDesc;
                }

                // reset the properties first
                list = null;
                obcList = null;
                OnPropertyChanged(nameof(obcList));

                if (isSuccess)
                {
                    App.cio = JsonConvert.DeserializeObject<Cio>(content);  // < -- set the global variable     
                    var app_group = App.cio.zwaGroupList;

                    if (app_group == null) return;
                    if (app_group.Length == 0) return;
                    if (app_group.Length == 1)
                    {
                        HandlerSelectedItem(app_group[0]); // if only 1 group then auto select, and log off the screen
                        return;
                    }

                    list = new List<zwaUserGroup>();
                    list.AddRange(app_group);

                    // remove the current group
                    if (!String.IsNullOrWhiteSpace(_curGroup))
                    {
                        int locId = list.IndexOf(list.Where(x => x.groupName.Equals(_curGroup)).FirstOrDefault());
                        if (locId >= 0)
                            list.RemoveAt(locId);
                    }

                    obcList = new ObservableCollection<zwaUserGroup>(list);

                    OnPropertyChanged(nameof(obcList));
                    return;
                }

                BRequest br = JsonConvert.DeserializeObject<BRequest>(content);  // < -- set the global variable                    
                DisplayAlert("Alert", $"{br?.Message}\n{lastErrorMessage}", "OK");
            }

            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// Init the command
        /// </summary>
        void InitCmd() => cmdSearchBarVisible = new Command<SearchBar>((SearchBar searchBar) => HandlerSearchBarVisible(searchBar));

        /// <summary>
        /// Handle the search bar visibilities
        /// </summary>
        /// <param name="obj"></param>
        void HandlerSearchBarVisible(SearchBar searchBar)
        {
            _searchBarIsVisible = !_searchBarIsVisible;
            OnPropertyChanged(nameof(searchBarIsVisible));
            if (_searchBarIsVisible) searchBar?.Focus();
        }

        /// <summary>
        /// Handler user selected group to add
        /// </summary>
        /// <param name="selected"></param>
        void HandlerSelectedItem(zwaUserGroup selected)
        {
            caller.SetUserGroup(selected); // pass the selected group back
            Navigation.PopAsync();
        }

        /// <summary>
        /// Simulate the display alert util from viewmodel
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="btnStr"></param>
        async void DisplayAlert(string title, string message, string accept) =>
            await new Dialog().DisplayAlert(title, message, accept);


        /// <summary>
        /// Handle the on property changed, value update to screen
        /// </summary>
        /// <param name="propertyName"></param>
        void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// dispose code
        /// </summary>
        public void Dispose() { }// => GC.Collect();
    }
}
