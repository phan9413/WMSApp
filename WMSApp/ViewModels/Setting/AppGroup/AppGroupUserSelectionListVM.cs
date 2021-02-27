using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.Login;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Setting.AppGroup
{
    public class AppGroupUserSelectionListVM : IDisposable, INotifyPropertyChanged
    {
        INavigation Navigation;
        AppGroupUsrListVM caller;
        List<zwaUser> list;

        string companyId;
        int groupId;

        #region Page property binding
        public event PropertyChangedEventHandler PropertyChanged;
        public Command cmdChooseTick { get; private set; }
        public Command cmdSearhIconClicked { get; private set; }

        bool _searchBarVisible;
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

        zwaUser _selectedItem;
        public zwaUser selectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                if (_selectedItem == null) return;

                HandlerSelectItem(_selectedItem);

                selectedItem = null;
                OnPropertyChanged(nameof(selectedItem));
            }
        }
        #endregion

        public ObservableCollection<zwaUser> obcList { get; set; }

        /// <summary>
        /// The constructor, starting point
        /// </summary>
        /// <param name="navigation"></param>
        /// <param name="source"></param>
        public AppGroupUserSelectionListVM(INavigation navigation, AppGroupUsrListVM source)
        {
            caller = source;
            Navigation = navigation;
            companyId = source.companyId;
            groupId = source.groupId;

            InitCmd();
            QueryAppNotGroupUserList();
        }

        /// <summary>
        /// Init the command action
        /// </summary>
        void InitCmd()
        {
            cmdSearhIconClicked = new Command<SearchBar>((SearchBar searchBar) => HandlerSearchBarIconClicked(searchBar));
            cmdChooseTick = new Command(HandlerSelectedUser);
        }

        /// <summary>
        /// set the iselected property to true
        /// </summary>
        /// <param name="selected"></param>
        void HandlerSelectItem(zwaUser selected)
        {
            try
            {
                int locId = list.IndexOf(list.Where(x => x.userIdName == selected.userIdName).FirstOrDefault());
                if (locId >= 0)
                {
                    list[locId].isSelected = !list[locId].isSelected;                                        
                }
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// Handler selected list
        /// send the select list back to caller source
        /// </summary>
        void HandlerSelectedUser()
        {
            try
            {
                if (list == null)
                {
                    DisplayAlert("Alert", "There is not available user(s) for selection", "OK");
                    return;
                }

                var tickedList = list.Where(x => x.isSelected == true).ToList();

                caller.UpdateList(tickedList);
                Navigation.PopAsync();
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// Handler search bar icon clicked
        /// where searchbar will be turn on and off visible
        /// </summary>
        /// <param name="obj"></param>
        void HandlerSearchBarIconClicked(SearchBar searchBar)
        {
            try
            {
                _searchBarVisible = !_searchBarVisible;
                OnPropertyChanged(nameof(searchBarVisible));

                if (_searchBarVisible) searchBar?.Focus();
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// function to reset the list of this VM
        /// </summary>
        void ResetList()
        {
            list = null;
            obcList = null;
            OnPropertyChanged(nameof(obcList));
        }

        /// <summary>
        /// Connect to web api server, and do the query
        /// </summary>
        async void QueryAppNotGroupUserList()
        {
            try
            {
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    companyName = companyId,
                    groupId = groupId,
                    request = "QueryAppNotGroupUserList"
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
                ResetList();
                if (isSuccess)
                {
                    App.cio = JsonConvert.DeserializeObject<Cio>(content);  // < -- set the global variable     
                    zwaUser[] app_group = App.cio.zwAppUsers;

                    if (app_group == null) return;
                    if (app_group.Length == 0) return;

                    list = new List<zwaUser>();
                    list.AddRange(app_group);
                    obcList = new ObservableCollection<zwaUser>(list);
                    OnPropertyChanged(nameof(obcList));
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

        /// <summary>
        /// Simulate the display alert util from viewmodel
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="btnStr"></param>
        //async Task<bool> DisplayAlert(string title, string message, string accept, string cancel) =>
        //        await new Dialog().DisplayAlert(title, message, accept, cancel);

        /// <summary>
        /// Handle the on property changed, value update to screen
        /// </summary>
        /// <param name="propertyName"></param>
        void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Dipose code
        /// </summary>
        public void Dispose() { } // => GC.Collect();
    }
}
