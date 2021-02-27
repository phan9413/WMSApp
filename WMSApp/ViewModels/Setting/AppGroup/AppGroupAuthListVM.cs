using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Models.SAP;
using WMSApp.Models.Setting;
using WMSApp.Models.Setup.Group;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Setting.AppGroup
{
    public class AppGroupAuthListVM : IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region bindable property declaration
        public Command cmdSave { get; private set; }
        public Command cmdShowSearchBar { get; private set; }

        string _searchQuery;
        public string searchQuery
        {
            get
            {
                return _searchQuery;
            }
            set
            {
                _searchQuery = value;
                HandlerSearchQuery(_searchQuery);
            }
        }

        bool _seachBarVisible;
        public bool seachBarVisible
        {
            get
            {
                return _seachBarVisible;
            }
            set
            {
                _seachBarVisible = value;
                OnPropertyChanged(nameof(seachBarVisible));
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
                _curCompanyName = value;
                OnPropertyChanged(nameof(curCompanyName));
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
                _curGroupName = value;
                OnPropertyChanged(nameof(curGroupName));
            }
        }

        Item _selectedItem;
        public Item selectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                if (_selectedItem == null) return;

                // handle the click item from item listview
                HandlerSelectedItemToggleSwitch(_selectedItem);

                selectedItem = null;
                OnPropertyChanged(nameof(selectedItem));
            }
        }

        ObservableCollection<ItemGroup> _list;
        public ObservableCollection<ItemGroup> obcList { get; set; }

        #endregion end of bindable declaration

        INavigation Navigation;
        zwaUserGroup curGroup;
        OADM_Ex curCompany;

        /// <summary>
        /// Handle the on property changed, value update to screen
        /// </summary>
        /// <param name="propertyname"></param>
        public void OnPropertyChanged(string propertyname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));

        /// <summary>
        /// Dipose code
        /// </summary>
        public void Dispose() { }// => GC.Collect();

        /// <summary>
        /// View model constructor, starting point
        /// </summary>
        public AppGroupAuthListVM(INavigation navigation, OADM_Ex company, zwaUserGroup group)
        {
            Navigation = navigation;
            curCompany = company;
            curGroup = group;
            curCompanyName = curCompany?.CompnyName;
            curGroupName = curGroup?.groupName;
            InitCmd();
        }

        /// <summary>
        /// Init the command
        /// </summary>
        void InitCmd()
        {
            cmdShowSearchBar = new Command<SearchBar>((SearchBar searchBar) => HandlerSetSeacrhBarVisible(searchBar));
            cmdSave = new Command(SaveGroupPermission);

            MessagingCenter.Subscribe(this, "svrUseGrpPermissionList",
                (object obj) => ProcessGroupPermission(((List<zwaUserGroup1>)obj).ToArray()));
        }

        /// <summary>
        /// Save the permission list back to group add / edit page
        /// then close this screen
        /// </summary>
        void SaveGroupPermission()
        {
            MessagingCenter.Send<ObservableCollection<ItemGroup>>(_list, "SetGroupPermissionList"); // App._SNUpdateGroupUserPermissionList);
            Navigation.PopAsync();
        }

        /// <summary>
        /// Set Seacrh Bar Visible
        /// </summary>
        /// <param name="obj"></param>
        void HandlerSetSeacrhBarVisible(SearchBar obj)
        {
            try
            {
                _seachBarVisible = !_seachBarVisible;
                OnPropertyChanged(nameof(seachBarVisible));

                // set search bar focus                
                if (_seachBarVisible)
                {
                    obj?.Focus();
                }
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// Handler the search query
        /// </summary>
        /// <param name="query"></param>
        void HandlerSearchQuery(string query)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(query))
                {
                    var filterList = new List<ItemGroup>();
                    foreach (var itemGroup in _list)
                    {
                        var foundItems = itemGroup
                            .Where(x =>
                                    x.Title.ToLower().Contains(query) ||
                                    x.Desc.ToLower().Contains(query)).ToList();

                        if (foundItems == null) continue;
                        if (foundItems.Count == 0) continue;

                        var newGrp = new ItemGroup(itemGroup.Title, itemGroup.ShortName, itemGroup.headerId, false);
                        foreach (var item in foundItems)
                        {
                            newGrp.Add(item);
                        }
                        filterList.Add(newGrp);
                    }
                    obcList = new ObservableCollection<ItemGroup>(filterList);
                    OnPropertyChanged(nameof(obcList));
                    return;
                }
                // ELSE
                RefreshList();
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// toggle the switch of the item
        /// </summary>
        /// <param name="selectedItem"></param>
        void HandlerSelectedItemToggleSwitch(Item selectedItem)
        {
            try
            {
                _selectedItem.isSwitchOn = !_selectedItem.isSwitchOn;
                _selectedItem.OnPropertyChanged("isSwitchOn");
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// Process group permission list from server
        /// </summary>
        /// <param name="list"></param>
        void ProcessGroupPermission(zwaUserGroup1[] list)
        {
            try
            {
                MessagingCenter.Unsubscribe<object>(this, "svrUseGrpPermissionList");

                if (list == null) return;
                if (list.Length == 0) return;
                var itemGroups = list.Where(x => x.parentId == -1).ToList();
                var group = new ObservableCollection<ItemGroup>();

                foreach (var iGrp in itemGroups) // for menu group
                {
                    var itemGroupList = list.Where(x => x.parentId == iGrp.screenId).ToList();
                    var newItemGroup = new ItemGroup(iGrp.title, "", iGrp.id, false);
                    newItemGroup.Tag = iGrp;

                    if (itemGroupList == null) continue;

                    foreach (var item in itemGroupList) // for menu group item
                    {
                        string itemDesc = (item.ctrlLimit > 0) ? $"{item.dscrptn}, Limited : {item.ctrlLimit}" : item.dscrptn;
                        var newItem = new Item
                        {
                            Title = item.title,
                            Desc = itemDesc,
                            Id = (MenuItemType)item.id,
                            Tag = item
                        };
                        newItemGroup.Add(newItem); // add item into the group
                    }
                    group.Add(newItemGroup); // add group into screen list
                }

                _list = group; /// assign the group list into screen list
                RefreshList();
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// Refresh the list
        /// </summary>
        void RefreshList()
        {
            if (_list == null) return;
            obcList = new ObservableCollection<ItemGroup>(_list);
            OnPropertyChanged(nameof(obcList));
        }

        /// <summary>
        /// Display the dialog onto the screen
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="btnStr"></param>
        async void DisplayAlert(string title, string message, string accept) => await App.Current.MainPage.DisplayAlert(title, message, accept);

        #region reserved ref code
        /// <summary>
        /// Show andriod toast message onto client client
        /// </summary>
        /// <param name="message"></param>
        //void DisplayToastMessage(string message)
        //{
        //    var toast = DependencyService.Get<IToastMessage>();
        //    toast?.ShortAlert(message);
        //}

        /// <summary>
        /// Built to share among page
        /// </summary>
        /// <param name="title"></param>
        /// <param name="promptMessage"></param>
        /// <param name="acceptStr"></param>
        /// <param name="cancelStr"></param>
        /// <param name="placeHolder"></param>
        /// <param name="maxLen"></param>
        /// <param name="keyboard"></param>
        /// <param name="initialValue"></param>
        /// <returns></returns>
        //async Task<string> DisplayPromptAsync(string title, string promptMessage, string lastValue,
        //   string acceptStr = "OK", string cancelStr = "Cancel", string placeHolder = null, int maxLen = -1,
        //   Keyboard keyboard = null)
        //{
        //    try
        //    {
        //        string result = await App.Current.MainPage.DisplayPromptAsync(
        //            title, promptMessage, acceptStr, cancelStr, placeHolder, maxLen, keyboard);

        //        if (result == null)
        //        {
        //            return lastValue + "";
        //        }

        //        if (!result.ToLower().Equals("cancel"))
        //        {
        //            return result + "";
        //        }

        //        return lastValue + "";
        //    }
        //    catch (Exception excep)
        //    {
        //        DisplayAlert("Alert", excep.ToString(), "OK");
        //        return lastValue + "";
        //    }
        //}

        /// <summary>
        /// Use to capture the control limit of the selected Item
        /// 202004T0527
        /// </summary>
        /// <param name="_SelectedItem"></param>
        //async void CapturePermittedControlLimit(Models.GroupMenus.Item _SelectedItem, zwaUserGroup1 selectField)
        //{
        //    try
        //    {
        //        string updatedValue = await DisplayPromptAsync($"Input {_SelectedItem.Title} limit"
        //            , $"Numeric Only [any values from 0-9], Input zero (0) as no limit, Last limit : {selectField.ctrlLimit}"
        //            , ""
        //            , "Confirm"
        //            , "Cancel"
        //            , ""
        //            , -1
        //            , Keyboard.Numeric);

        //        #region Validate input
        //        if (updatedValue == null)
        //        {
        //            goto ResetSelectedItem;
        //        }

        //        if (updatedValue.Equals(""))
        //        {
        //            goto ResetSelectedItem;
        //        }

        //        if (!IsNumeric(updatedValue))
        //        {
        //            DisplayAlert("Alert", "String value not allowed, Please try again. Thanks", "OK");
        //            goto ResetSelectedItem;
        //        }

        //        decimal value = Convert.ToDecimal(updatedValue);
        //        if (selectField.ctrlLimit == value)
        //        {
        //            goto ResetSelectedItem;
        //        }

        //        if (value < 0)
        //        {
        //            DisplayAlert("Alert", "Negative value not allowed, Please try again. Thanks", "OK");
        //            goto ResetSelectedItem;
        //        } 
        //        #endregion

        //        ((zwaUserGroup1)_selectedItem.Tag).ctrlLimit = value;

        //        _selectedItem.Desc = (selectField.ctrlLimit >= 0) ? $"{selectField.dscrptn}, Limited : {selectField.ctrlLimit}" : selectField.dscrptn;
        //        _selectedItem.OnPropertyChanged("Desc");

        //    // deselect the selected item
        //    ResetSelectedItem:

        //        selectedItem = null;
        //        OnPropertyChanged(nameof(selectedItem));

        //    }
        //    catch (Exception excep)
        //    {
        //        DisplayAlert("Alert", $"{excep}", "OK");
        //    }
        //}

        /// <summary>
        ///  check input is numeric
        /// </summary>
        /// <param name="numbericString"></param>
        ///// <returns></returns>
        //bool IsNumeric(string numbericString)
        //{
        //    return decimal.TryParse(numbericString, out decimal decimalVal);
        //}

        /// <summary>
        /// Load User Group Permission
        /// </summary>
        //async void LoadUserGroupPermission()
        //{
        //    try
        //    {
        //        Cio cioRequest = new Cio() // load the data from web server 
        //        {
        //            sap_logon_name = App.waUser,
        //            sap_logon_pw = App.waPw,
        //            token = App.waToken,
        //            companyName = curCompany.CompnyName,
        //            groupId = curGroup.groupId,
        //            request = "QueryAppUserGroupPermissionList"
        //        };

        //        HttpClientWapi httpClient = new HttpClientWapi();
        //        string content = await httpClient.RequestSvrAsync_mgt(cioRequest, App.su.QUERY_APPSETUP_GRP);

        //        if (httpClient.isSuccessStatusCode)
        //        {
        //            App.cio = JsonConvert.DeserializeObject<Cio>(content);  // < -- set the global variable  
        //            zwaUserGroup1[] groupPermission = App.cio.zwaUserGroupsPermission;

        //            ProcessGroupPermission(groupPermission);
        //            return;
        //        }

        //        BRequest br = JsonConvert.DeserializeObject<BRequest>(content);  // < -- set the global variable    
        //        lastErrMsg = br?.Message;
        //        DisplayAlert("Alert", lastErrMsg + "\n" + httpClient.lastErrorDesc, "OK");
        //    }
        //    catch (Exception excep)
        //    {
        //        lastErrMsg = excep.ToString();
        //        DisplayToastMessage(excep.Message);
        //    }
        //} 
        #endregion
    }
}

