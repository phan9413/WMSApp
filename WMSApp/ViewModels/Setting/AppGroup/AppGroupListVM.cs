using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.SAP;
using WMSApp.Models.Setup.Group;
using WMSApp.Views.Setting;
using WMSApp.Views.Setting.AppGroup;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Setting.AppUserGroup
{
    public class AppGroupListVM : IDisposable, INotifyPropertyChanged
    {
        #region public / bindable decalartion

        public event PropertyChangedEventHandler PropertyChanged;
        List<zwaUserGroup> _list;
        public ObservableCollection<zwaUserGroup> list { get; set; }
        public Command cmdCompanyTapped { get; set; }
        public Command cmdAddNewGroup { get; set; }
        public Command cmdVisibleSearchBar { get; set; }

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
                LoadGroupListFromServer();
                OnPropertyChanged(nameof(curOADMCompany));
            }
        }

        bool _searchBarVisible;
        public bool searchBarVisible
        {
            get
            {
                return _searchBarVisible;
            }
            set
            {
                _searchBarVisible = value;
                OnPropertyChanged(nameof(searchBarVisible));
            }
        }
        string _searchText;
        public string searchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                _searchText = value;
                HandlerSearchText(_searchText);
            }
        }

        zwaUserGroup _selectedItem;
        public zwaUserGroup selectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;

                if (_selectedItem == null) return;

                HandlerSelectedGroup(_selectedItem);

                selectedItem = null;
                OnPropertyChanged(nameof(selectedItem));
            }
        }

        #endregion

        INavigation Navigation;

        /// <summary>
        /// Construtor, starting point
        /// </summary>
        /// <param name="navigation"></param>
        /// <param name="vListview"></param>
        public AppGroupListVM(INavigation navigation)
        {
            Navigation = navigation;
            InitCmd();
            StartTimer_LoadCompany();
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
                    return false; // return true to continue
                });
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "Ok");
            }
        }

        /// <summary>
        /// Handle the text search query
        /// </summary>
        /// <param name="query"></param>
        void HandlerSearchText(string query)
        {
            try
            {
                if (String.IsNullOrEmpty(query))
                {
                    list = new ObservableCollection<zwaUserGroup>(list);
                }
                else
                {
                    query = query.ToLower();
                    var filteredList = list
                        .Where(x =>
                                x.TextDisplay.ToLower().Contains(query) ||
                                x.DetailsDisplay.ToLower().Contains(query))
                        .ToList();

                    list = new ObservableCollection<zwaUserGroup>(filteredList);
                }
                OnPropertyChanged(nameof(list));

            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// Init the cmd object 
        /// </summary>
        void InitCmd()
        {
            cmdCompanyTapped = new Command(HandlerShowCompanyForChoose);
            cmdAddNewGroup = new Command(LaunchAddGroupView);

            cmdVisibleSearchBar = new Command<object>((object obj) =>
            {
                ToggleVisibleSearchBar(obj);
            });
        }

        /// <summary>
        /// Handler search bar menuitem click
        /// hide and show the search bar
        /// </summary>
        /// <param name="obj"></param>
        void ToggleVisibleSearchBar(object obj)
        {
            try
            {
                _searchBarVisible = !_searchBarVisible;
                OnPropertyChanged(nameof(searchBarVisible));

                SearchBar sb = (SearchBar)obj;
                if (sb != null && _searchBarVisible)
                {
                    sb.Focus();
                }
            }
            catch (Exception excep)
            {
                ShowToastMessage($"{excep}");
            }
        }

        /// <summary>
        /// Show list of company for choose
        /// Resue the user choose company view
        /// </summary>
        void HandlerShowCompanyForChoose()
        {
            Navigation.PushAsync(new AppChseCmpnyListView(this));
        }
        /// <summary>
        /// Show group edit screen - EDIT
        /// </summary>
        /// <param name="selected"></param>
        void HandlerSelectedGroup(zwaUserGroup selected)
        {
            if (selected.groupId == 1)
            {
                DisplayAlert("Alert", "System default group are not editable, Thanks", "OK");
            }

            Navigation.PushAsync(new AppGroupAddEditView(this, selected));
        }

        /// <summary>
        /// Launch the add group view - ADD New
        /// </summary>
        void LaunchAddGroupView()
        {
            Navigation.PushAsync(new AppGroupAddEditView(this));
        }

        /// <summary>
        /// Perform loading of the group list from svr
        /// based n selected company
        /// </summary>
        /// <summary>
        /// Perform loading of the record from the server
        /// use be triger by the user add screen when add screen finish the add or update operation
        /// </summary>
        public async void LoadGroupListFromServer()
        {
            try
            {
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    companyName = this._curOADMCompany.CompnyName,
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

                // reset the list 
                _list = null; // reset the properties first
                list = null;
                OnPropertyChanged(nameof(list));

                if (isSuccess)
                {
                    App.cio = JsonConvert.DeserializeObject<Cio>(content);  // < -- set the global variable     
                    zwaUserGroup[] app_group = App.cio.zwaGroupList;

                    if (app_group == null)
                    {
                        return;
                    }

                    if (app_group.Length == 0)
                    {
                        return;
                    }

                    _list = new List<zwaUserGroup>();
                    _list.AddRange(app_group);
                    list = new ObservableCollection<zwaUserGroup>(_list);

                    OnPropertyChanged(nameof(list));
                    return;
                }

                BRequest br = JsonConvert.DeserializeObject<BRequest>(content);  // < -- set the global variable                    
                DisplayAlert("Alert", $"{br?.Message}\n{lastErrorMessage}", "OK");

            }
            catch (Exception excep)
            {
                ShowToastMessage(excep.Message);
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
        async void DisplayAlert(string title, string message, string accept) => await new Dialog().DisplayAlert(title, message, accept);

        /// <summary>
        /// Handle the on property changed, value update to screen
        /// </summary>
        /// <param name="propertyname"></param>
        void OnPropertyChanged(string propertyname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));

        /// <summary>
        ///  Dispose code
        /// </summary>
        public void Dispose() { }// => GC.Collect();

    }
}
