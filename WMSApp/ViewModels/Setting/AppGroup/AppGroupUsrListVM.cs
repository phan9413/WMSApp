using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMSApp.Class;
using WMSApp.Models.Login;
using WMSApp.Models.Setup.Group;
using WMSApp.Views.Setting.AppGroup;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Setting.AppGroup
{
    public class AppGroupUsrListVM : IDisposable, INotifyPropertyChanged
    {
        INavigation Navigation;
        public string companyId;
        public int groupId; // represent the group and company object

        #region Page property binding declaration
        public List<zwaUser> list;
        public ObservableCollection<zwaUser> obcList { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public Command cmdAddUser { get; private set; }
        public Command cmdSearchUser { get; private set; }
        public Command cmdSelectUser { get; private set; }
        public Command cmdUndoSelectUser { get; private set; }
        public Command cmdRemoveUser { get; private set; }
        public Command cmdSwipLeftViewEdit { get; private set; }
        public Command cmdSwipRightRemove { get; private set; }

        public Command CmdSaveAndReturn { get; set; }

        bool _isSearchBarVisible;
        public bool isSearchBarVisible
        {
            get
            {
                return _isSearchBarVisible;
            }
            set
            {
                if (_isSearchBarVisible != value)
                {
                    _isSearchBarVisible = value;
                    OnPropertyChanged(nameof(isSearchBarVisible));
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
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    if (_selectedItem == null) return;

                    // handle the selected item 
                    TickedSelectedItem(_selectedItem);

                    selectedItem = null;
                    OnPropertyChanged(nameof(selectedItem));
                }
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
                if (_searchText != value)
                {
                    _searchText = value;
                    SearchText(_searchText);
                }
            }
        }

        string _curCompanyName;
        public string curCompanyName
        {
            get
            {
                return _curCompanyName;
            }
            set
            {
                if (_curCompanyName != value)
                {
                    _curCompanyName = value;
                    OnPropertyChanged(nameof(curCompanyName));
                }
            }
        }

        string _curGroupName;
        public string curGroupName
        {
            get
            {
                return _curGroupName;
            }
            set
            {
                if (_curGroupName != value)
                {
                    _curGroupName = value;
                    OnPropertyChanged(nameof(curGroupName));
                }
            }
        }
        #endregion

        /// <summary>
        /// Comstrutor, starting point
        /// </summary>
        /// <param name="navigation"></param>
        public AppGroupUsrListVM(INavigation navigation, zwaUserGroup curGroup, List<zwaUser> userList)
        {
            curCompanyName = curGroup.companyId;
            curGroupName = curGroup.groupName;

            companyId = curCompanyName;
            groupId = curGroup.groupId;

            Navigation = navigation;
            list = userList;

            HandlerList();
            InitCmd();
        }

        /// <summary>
        /// Handler the list
        /// </summary>
        void HandlerList()
        {
            if (list == null) return;

            // is default group then disable the check box
            foreach (var user in list) user.isVisible = !(groupId == 1);

            obcList = new ObservableCollection<zwaUser>(list);
            OnPropertyChanged(nameof(obcList));
        }

        /// <summary>
        /// Use by external view to refresh the list on screen
        /// </summary>
        public void UpdateList(List<zwaUser> tickedList)
        {
            try
            {
                if (tickedList == null) return;

                if (list != null)
                {
                    var temp = tickedList.Distinct().ToList();
                    list.AddRange(temp);

                    // summary the reselection object by group, and distinct the list
                    var distinctItems = list.GroupBy(x => x.userIdName).Select(y => y.First());
                    list = new List<zwaUser>(distinctItems);

                    list.ForEach(x => x.isSelected = true);
                }
                else
                {
                    list = new List<zwaUser>();
                    list.AddRange(tickedList);
                }

                // pass the list back to the AddView for register the user
                MessagingCenter.Send<List<zwaUser>>(list, "SetGroupUserList"); //App._SNUpdateGroupUserList);

                obcList = new ObservableCollection<zwaUser>(list);
                OnPropertyChanged(nameof(obcList));
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// Init the command setup to thier action / handling
        /// </summary>
        void InitCmd()
        {
            cmdAddUser = new Command(LaunchAddUser);
            cmdSearchUser = new Command<SearchBar>((SearchBar searchbar) => ToggleSearchBar(searchbar));

            cmdSelectUser = new Command(TickAllUsers);
            cmdUndoSelectUser = new Command(UnTickAllUsers);
            cmdRemoveUser = new Command(RemoveTickedUser);

            CmdSaveAndReturn = new Command(() => 
            {
                Navigation.PopAsync();
            });
        }

        /// <summary>
        ///  handle the selected item on list view
        ///  check or uncheck the listview
        /// </summary>
        /// <param name="selected"></param>
        void TickedSelectedItem(zwaUser selected)
        {
            if (selected == null) return;
            if (this.groupId == 1)
            {
                selected.isSelected = false;
                selected.OnPropertyChanged("isSelected");
                return;
            }
            // else 
            selected.isSelected = (selected.isSelected) ? false : true;
            selected.OnPropertyChanged("isSelected");
        }

        /// <summary>
        /// perfor the test entry search to the list
        /// </summary>
        /// <param name="query"></param>
        void SearchText(string query)
        {
            try
            {
                if (list == null) return;
                if (String.IsNullOrWhiteSpace(query))
                {
                    obcList = new ObservableCollection<zwaUser>(list);
                }
                else
                {
                    string query_lower = query.ToLower();
                    var filterList = list
                        .Where(x => x.TextDisplay.ToLower().Contains(query_lower) ||
                                x.DetailsDisplay.ToLower().Contains(query_lower))
                        .ToList();

                    obcList = new ObservableCollection<zwaUser>(filterList);
                }
                OnPropertyChanged(nameof(obcList));
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// Show a user list who not inside the group
        /// </summary>
        void LaunchAddUser()
        {
            if (this.groupId == 1) return;
            Navigation.PushAsync(new AppGroupUserSelectionListView(this));
        }

        /// <summary>
        /// Toggle the visible and invisible of the search bar
        /// </summary>
        /// <param name="obj"></param>
        void ToggleSearchBar(SearchBar searchbar)
        {
            try
            {
                isSearchBarVisible = !isSearchBarVisible;
                if (isSearchBarVisible) searchbar?.Focus();
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// Share able to tick all true , or tick all false
        /// </summary>
        /// <param name="boolVal"></param>
        void TickAllUsers()
        {
            if (this.groupId == 1) return;
            ProcesssTick(true);
        }

        /// <summary>
        /// Share able to tick all true , or tick all false
        /// </summary>
        /// <param name="boolVal"></param>
        void UnTickAllUsers()
        {
            if (this.groupId == 1) return;
            ProcesssTick(false);
        }

        /// <summary>
        /// Share code
        /// </summary>
        /// <param name="val"></param>
        void ProcesssTick(bool val)
        {
            try
            {
                if (list == null) return;

                foreach (var item in list)
                {
                    item.isSelected = val;
                    //item.OnPropertyChanged(nameof(item.isSelected));
                }
                obcList = new ObservableCollection<zwaUser>(list);
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// reset the list into null, include obcloist and raise update to the view
        /// </summary>
        public void ResetList()
        {
            list = null;
            obcList = null;
            OnPropertyChanged(nameof(obcList));
        }

        /// <summary>
        /// Remove single user from the svr and reply a new list
        /// </summary>
        /// <param name="userIdName"></param>
        //void RemoveSingleleUser(string userIdName)
        //{
        //    try
        //    {
        //        int locId = list.IndexOf(list.Where(x => x.userIdName.ToLower().Equals(userIdName.ToLower())).FirstOrDefault());
        //        if (locId < 0)
        //        {
        //            DisplayAlert("Alert", "The selected user name : " + userIdName + " no found from the list, " +
        //                "Please contact system support for help. Thanks", "OK");
        //            return;
        //        }

        //        zwaUser[] array = { list[locId] }; //<--- single user array
        //        RequestServerRemoveGroupUser(companyId, groupId, array);
        //    }
        //    catch (Exception excep)
        //    {
        //        DisplayAlert("Alert", $"{excep}", "OK");
        //    }
        //}

        /// <summary>
        /// Remove the ticked user from the group
        /// Update the server and scren 
        /// </summary>
        async void RemoveTickedUser()
        {
            try
            {
                if (this.groupId == 1) return;

                if (list == null) return;
                var checkList = list.Where(x => x.isSelected == true).ToList();

                if (checkList == null) return;
                if (checkList.Count == 0)
                {
                    DisplayAlert("No group user ticked", "Please tick at least an user to remove. Thanks", "OK");
                    return;
                }

                bool confirm = await DisplayAlert("Confirm?",
                    "Removed user from the group are not recoverable.", "Confirm", "Cancel");

                if (confirm)
                    RequestServerRemoveGroupUser(companyId, groupId, checkList?.ToArray());
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// Send checked list to sever
        /// with company id and group id
        /// remove the user from the svr
        /// and replied a fresh list of the user
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="groupId"></param>
        /// <param name="checkedList"></param>
        async void RequestServerRemoveGroupUser(string companyId, int groupId, zwaUser[] checkedList)
        {
            try
            {
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    zwAppUsers = checkedList,
                    request = "ResetUserGroupToDefault"
                };

                var isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    await httpClient.RequestSvrAsync_mgt(cioRequest, "AppUserGroup");
                    isSuccess = httpClient.isSuccessStatusCode;
                }

                if (isSuccess)
                {
                    // connect to server and delete the list 
                    // reply list of exist user in the group
                    foreach (var user in checkedList)
                    {
                        list.Remove(user);
                    }

                    obcList = new ObservableCollection<zwaUser>(list);
                    OnPropertyChanged(nameof(obcList));
                    MessagingCenter.Send<object>(list, "SetGroupUserList");// App._SNUpdateGroupUserList);
                }
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// pass message to the app screen for alert inform
        /// </summary>
        /// <param name="message"></param>
        //void ShowToastMessage(string message)
        //{
        //    if (String.IsNullOrWhiteSpace(message)) return;

        //    var toast = DependencyService.Get<IToastMessage>();
        //    toast?.ShortAlert(message);
        //}

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
        async Task<bool> DisplayAlert(string title, string message, string accept, string cancel) =>
            await new Dialog().DisplayAlert(title, message, accept, cancel);

        /// <summary>
        /// Handle the on property changed, value update to screen
        /// </summary>
        /// <param name="propertyname"></param>
        public void OnPropertyChanged(string propertyname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));

        /// <summary>
        /// Dipose code
        /// </summary>
        public void Dispose() { }// => GC.Collect();
    }
}
