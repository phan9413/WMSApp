using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.Login;
using WMSApp.Models.Setting;
using WMSApp.Models.Setup.Group;
using WMSApp.ViewModels.Setting.AppUserGroup;
using WMSApp.Views.Setting.AppGroup;
using Xamarin.Forms;
namespace WMSApp.ViewModels.Setting.AppGroup
{
    public class AppGroupAddEditVM : IDisposable, INotifyPropertyChanged
    {
        #region inner declaration
        enum fieldIndex // <--- inner used
        {
            groupName = 0,
            groupDesc = 1,
            mgtUser = 2,
            mgtPermission = 3
        }

        INavigation Navigation;
        zwaUserGroup curGroup;
        zsCommDocList scrList;
        AppGroupListVM caller;
        List<zsCommDocList> list;
        List<zwaUser> userList;  //<-- send to svr
        List<zwaUserGroup1> userGroupPermissionList; //<-- send to svr
        ObservableCollection<ItemGroup> groupPermissionList;
        //string lastErrMsg;
        #endregion

        #region property / binding declaration
        public event PropertyChangedEventHandler PropertyChanged;
        public Command cmdSave { get; private set; }
        public Command cmdCancel { get; private set; }
        public Command cmdManageUser { get; private set; }
        public Command cmdManagePermission { get; private set; }
        AppGroupListVM _callerVM;
        public AppGroupListVM callerVM
        {
            get => _callerVM;
            set
            {
                if (_callerVM != value)
                {
                    _callerVM = value;
                    OnPropertyChanged(nameof(callerVM));
                }
            }
        }

        string _title;
        public string title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(title));
                }
            }
        }

        string _tbiSavedText;
        public string tbiSavedText
        {
            get => _tbiSavedText;
            set
            {
                if (_tbiSavedText != value)
                {
                    _tbiSavedText = value;
                    OnPropertyChanged(nameof(tbiSavedText));
                }
            }
        }


        zsCommDocFields _selectedItem;
        public zsCommDocFields selectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    if (_selectedItem == null) return;

                    // handle the listview selected item
                    if (curGroup.groupId != 1)
                        CaptureData(_selectedItem);

                    selectedItem = null;
                    OnPropertyChanged(nameof(selectedItem));
                }
            }
        }

        bool _tbItemEnable;
        public bool tbItemEnable
        {
            get => _tbItemEnable;
            set
            {
                if (_tbItemEnable != value)
                {
                    _tbItemEnable = value;
                    OnPropertyChanged(nameof(tbItemEnable));
                }
            }
        }

        public ObservableCollection<zsCommDocList> obcList { get; private set; }

        string _curCompanyName;
        public string curCompanyName
        {
            get => _curCompanyName;
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
            get => _curGroupName;
            set
            {
                if (_curGroupName != value)
                {
                    _curGroupName = value;
                    OnPropertyChanged(nameof(curGroupName));
                }
            }
        }

        string _groupUserTitleCount;
        public string groupUserTitleCount
        {
            get => _groupUserTitleCount;
            set
            {
                if (_groupUserTitleCount != value)
                {
                    _groupUserTitleCount = value;
                    OnPropertyChanged(nameof(groupUserTitleCount));
                }
            }
        }
        #endregion

        /// <summary>
        /// Constructor for new group
        /// Starting point
        /// </summary>
        public AppGroupAddEditVM(INavigation navigation, AppGroupListVM source)
        {
            Navigation = navigation;
            caller = source;
            curGroup = new zwaUserGroup();
            curGroup.companyId = caller.curOADMCompany.CompnyName;
            curCompanyName = caller.curOADMCompany.CompnyName;
            curGroupName = "New group";
            title = "Add Group";
            tbiSavedText = "Save";
            groupUserTitleCount = "Manage User";
            InitGroupId();
            Init();
            InitCmd();
            QueryAppGroupUserPermissionAsync();
        }

        /// <summary>
        /// The constructor for edit
        /// starting point
        /// </summary>
        public AppGroupAddEditVM(INavigation navigation, AppGroupListVM source, zwaUserGroup selected)
        {
            Navigation = navigation;
            caller = source;
            title = "Edit Group";
            tbiSavedText = "Update";
            curGroup = selected;
            curCompanyName = caller.curOADMCompany.CompnyName;
            curGroupName = curGroup.groupName;
            QueryAppGroupUserListAsync();
            QueryAppGroupUserPermissionAsync();
            Init();
            InitCmd();
        }

        /// <summary>
        /// determine the group id
        /// </summary>
        void InitGroupId()
        {
            if (caller.list == null)
            {
                GetTempGroupIdFromSvr();
                return;
            }

            curGroup.groupId = caller.list.Max(x => x.groupId) + 1;
        }

        /// <summary>
        /// Init the command of the add view page
        /// </summary>
        void InitCmd()
        {
            cmdSave = new Command(Save);
            cmdCancel = new Command(() => { Navigation.PopAsync(); });
            cmdManageUser = new Command(() =>
            {
                Navigation.PushAsync(new AppGroupUsrListView(curGroup, userList));
                if (curGroup.groupId == 1) return;
                // subscribe the message to get added | edited user group list
                MessagingCenter.Subscribe<List<zwaUser>>(this, "SetGroupUserList", (List<zwaUser> list) => SetGroupUserList(list));
            });

            cmdManagePermission = new Command(() =>
            {
                if (curGroup.groupId == 1) return;
                Navigation.PushAsync(new AppGroupAuthListView(caller.curOADMCompany, curGroup));
                if (userGroupPermissionList != null)
                {
                    MessagingCenter.Send<object>(userGroupPermissionList, "svrUseGrpPermissionList");
                }
                //subscribe to set | get the permission setup
                MessagingCenter.Subscribe<ObservableCollection<ItemGroup>>(this, "SetGroupPermissionList",
                    (ObservableCollection<ItemGroup> obj) => SetGroupPermissionList(obj));
            });
        }

        /// <summary>
        /// Function to gotbthe user list get set
        /// </summary>
        /// <param name="list"></param>
        void SetGroupUserList(List<zwaUser> obj)
        {
            try
            {
                UnsubscribeMc("SetGroupUserList");
                userList = obj;
                groupUserTitleCount = (userList == null) ? "Manage User" : "Manager Users, " + userList.Count;
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// Set the user group permission list
        /// </summary>
        /// <param name="obj"></param>
        void SetGroupPermissionList(ObservableCollection<ItemGroup> obj)
        {
            try
            {
                UnsubscribeMc("SetGroupPermissionList");
                groupPermissionList = obj; //(ObservableCollection<ItemGroup>)obj;

                if (groupPermissionList == null) return;
                var list = new List<zwaUserGroup1>();
                foreach (var permitGrp in groupPermissionList)
                {
                    var group = (zwaUserGroup1)permitGrp.Tag;
                    group.companyId = this.caller.curOADMCompany.CompnyName;
                    group.groupId = this.curGroup.groupId;

                    list.Add(group);

                    foreach (var item in permitGrp)
                    {
                        var grpItem = (zwaUserGroup1)item.Tag;
                        grpItem.groupId = this.curGroup.groupId;
                        grpItem.companyId = this.caller.curOADMCompany.CompnyName;
                        list.Add(grpItem);
                    }
                }
                userGroupPermissionList = list;
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        ///  Initial the screen
        /// </summary>
        void Init()
        {
            try
            {
                scrList = new zsCommDocList();
                scrList.Heading = "";
                scrList.isVisible = false;

                scrList.Add(
                    new zsCommDocFields
                    {
                        title = "Group Name",
                        value = curGroup?.groupName,
                        id = scrList.Count,
                        isRightArrowVisible = true,
                        isVisible = true,
                    });

                scrList.Add(
                    new zsCommDocFields
                    {
                        title = "Description",
                        value = curGroup?.groupDesc,
                        id = scrList.Count,
                        isRightArrowVisible = true,
                        isVisible = true,
                    });

                ResetList();
            }
            catch (Exception excep)
            {
                DisplayToastMessage(excep.Message, 1);
            }
        }

        /// <summary>
        /// used when list data changed 
        /// </summary>
        void ResetList()
        {
            scrList.isVisible = false;
            scrList.Heading = "Group Info";
            list = new List<zsCommDocList>();
            list.Add(scrList);
            obcList = new ObservableCollection<zsCommDocList>(list);
        }

        /// <summary>
        /// Use to capture field data
        /// </summary>
        /// <param name="selected"></param>
        void CaptureData(zsCommDocFields selected)
        {
            try
            {
                switch (selected.id)
                {
                    case (int)fieldIndex.groupName:
                    case (int)fieldIndex.groupDesc:
                        {
                            HandlerFieldInput(selected);
                            break;
                        }
                }
            }
            catch (Exception excep)
            {
                DisplayToastMessage(excep.Message, 1);
            }
        }

        /// <summary>
        /// modular the function from the switch separator
        /// </summary>
        /// <param name="selectField"></param>
        // code to handle the input text
        async void HandlerFieldInput(zsCommDocFields selectField)
        {
            try
            {
                if (selectField.id > scrList.Count) return;
                string updatedValue = await new Dialog()
                                                    .DisplayPromptAsync($"Input {selectField.title}",
                                                    "",
                                                    "Save",
                                                    "Cancel",
                                                    "Input suitable value",
                                                    -1,
                                                    Keyboard.Default,
                                                    selectField.value);

                if (updatedValue == null) updatedValue = $"{selectField.value}";
                if (scrList[selectField.id].value == null) scrList[selectField.id].value = string.Empty;
                if (updatedValue.Equals("")) return;

                // check / validation
                if (scrList[selectField.id].value.Equals(updatedValue)) return;

                // update the field
                scrList[selectField.id].value = updatedValue?.ToUpper();
                scrList[selectField.id].OnPropertyChanged("value");
            }
            catch (Exception excep)
            {
                DisplayToastMessage(excep.Message, 1);
            }
        }

        /// <summary>
        /// Perform save of the group
        /// </summary>
        void Save()
        {
            try
            {
                if (curGroup.groupId == 1) return;
                if (!isValidated()) return;
                if (_tbiSavedText.ToLower().Equals("save"))
                {
                    CreateNewGroupToSvr();
                    return;
                }

                if (_tbiSavedText.ToLower().Equals("update"))
                {
                    UpdateGroupToSvr();
                }
            }
            catch (Exception excep)
            {
                DisplayToastMessage(excep.Message, 1);
            }
        }

        /// <summary>
        /// Get a temp group number from svr
        /// </summary>
        async void GetTempGroupIdFromSvr()
        {
            try
            {
                Cio cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    request = "GetTempGroupIdFromSvr"
                };

                using (var httpClient = new HttpClientWapi())
                {
                    string content = await httpClient.RequestSvrAsync_mgt(cioRequest, "AppUserGroup");
                    if (httpClient.isSuccessStatusCode)
                    {
                        App.cio = JsonConvert.DeserializeObject<Cio>(content);  // < -- set the global variable    
                        curGroup.groupId = App.cio.newUserGroupTempId;
                        return;
                    }

                    BRequest br = JsonConvert.DeserializeObject<BRequest>(content);  // < -- set the global variable                        
                    DisplayAlert("Alert", $"{br?.Message}\n{httpClient.lastErrorDesc}", "OK");
                }
            }
            catch (Exception excep)
            {
                DisplayToastMessage(excep.Message);
            }
        }

        /// <summary>
        /// Send the user object to svr to create new user
        /// svr return a new list of the usr under this company
        /// </summary>
        async void CreateNewGroupToSvr()
        {
            try
            {
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    newUserGroup = curGroup,
                    newUserGroupPermission = userGroupPermissionList?.ToArray(),
                    newUserGroupUsr = userList?.ToArray(),                    
                    request = "AddGroupRequest"
                };

                using (var httpClient = new HttpClientWapi())
                {
                    string content = await httpClient.RequestSvrAsync_mgt(cioRequest, "AppUserGroup");
                    if (httpClient.isSuccessStatusCode)
                    {
                        DisplayToastMessage("Group " + curGroup.groupName + " created");
                        caller?.LoadGroupListFromServer();
                        await Navigation.PopAsync();
                        return;
                    }

                    BRequest br = JsonConvert.DeserializeObject<BRequest>(content);  // < -- set the global variable                        
                    DisplayAlert("Alert", $"{br?.Message}\n{httpClient.lastErrorDesc}", "OK");
                }
            }
            catch (Exception excep)
            {
                DisplayToastMessage(excep.Message);
            }
        }

        /// <summary>
        /// Update existing user back to svr
        /// </summary>
        async void UpdateGroupToSvr()
        {
            try
            {
                Cio cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    newUserGroup = curGroup,
                    newUserGroupPermission = userGroupPermissionList?.ToArray(),
                    newUserGroupUsr = userList?.ToArray(),
                    request = "UpdateGroupRequest"
                };

                using (var httpClient = new HttpClientWapi())
                {
                    string content = await httpClient.RequestSvrAsync_mgt(cioRequest, "AppUserGroup");
                    if (httpClient.isSuccessStatusCode)
                    {
                        DisplayToastMessage("Group " + curGroup.groupName + " updated");
                        caller?.LoadGroupListFromServer();
                        await Navigation.PopAsync();
                        return;
                    }

                    BRequest br = JsonConvert.DeserializeObject<BRequest>(content);  // < -- set the global variable                        
                    DisplayAlert("Alert", $"{br?.Message}\n{httpClient.lastErrorDesc}", "OK");
                }
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", excep.Message, "OK");
            }
        }

        /// <summary>
        /// Validate all the entry and create the entry
        /// </summary>
        /// <returns></returns>
        bool isValidated()
        {
            try
            {
                // validate the user group
                curGroup.companyId = _curCompanyName;
                curGroup.groupDesc = scrList[(int)fieldIndex.groupDesc].value + "";
                curGroup.groupName = scrList[(int)fieldIndex.groupName].value + "";
                curGroup.lastModiDate = DateTime.Now;
                curGroup.lastModiUser = App.waUser;

                // check group name duplication
                // check when new group 
                // ignore when update group
                if (this._tbiSavedText.ToLower().Equals("Save"))
                {
                    curGroup.groupId = -1; // for new group

                    var duplicateGroupName =
                       caller.list
                           .Where(x => x.groupName.ToLower().Equals(curGroup.groupName.ToLower()))
                           .FirstOrDefault();

                    if (duplicateGroupName != null)
                    {
                        DisplayAlert("Alert", "The group name " + curGroup.groupName + " is duplicated, Thanks", "OK");
                        return false;
                    }
                }

                if (String.IsNullOrWhiteSpace(curGroup.companyId))
                {
                    DisplayAlert("Alert", "The group company name can not be empty, Thanks", "OK");
                    return false;
                }

                if (String.IsNullOrWhiteSpace(curGroup.groupDesc))
                {
                    DisplayAlert("Alert", "The group description can not be empty, Thanks", "OK");
                    return false;
                }

                if (String.IsNullOrWhiteSpace(curGroup.groupName))
                {
                    DisplayAlert("Alert", "The group name can not be empty, Thanks", "OK");
                    return false;
                }

                // check user list is not empty
                // allow add and edit group without user
                // 20200510T1909
                //if (userList == null)
                //{
                //    DisplayAlert("Alert", "No user(s) selected in the group, Please add in at least 1 user [u00], Thanks", "OK");
                //    return false;
                //}

                //if (userList.Count == 0)
                //{
                //    DisplayAlert("Alert", "No user(s) selected in the group, Please add in at least 1 user [u01], Thanks", "OK");
                //    return false;
                //}

                if (userGroupPermissionList == null)
                {
                    DisplayAlert("Alert", "The group permission is empty, Please configure the permission before save [p00], Thanks", "OK");
                    return false;
                }

                var permissionList = userGroupPermissionList.Where(x => x.authorised == "1").ToList();
                if (permissionList == null)
                {
                    // no permission set for any group
                    DisplayAlert("Alert", "The group permission is empty, Please configure the permission before save [p01], Thanks", "OK");
                    return false;
                }

                if (permissionList.Count == 0)
                {
                    // no permission set for any group
                    DisplayAlert("Alert", "The group permission is empty, Please configure the permission before save [p02], Thanks", "OK");
                    return false;
                }

                return true;
            }
            catch (Exception excep)
            {
                DisplayToastMessage(excep.Message, 1);
                return false;
            }
        }

        /// <summary>
        /// Load user from server based on group id and company id
        /// Initial in add and edit mode
        /// </summary>
        /// <summary>
        /// Load User Group Permission
        /// </summary>
        async void QueryAppGroupUserPermissionAsync()
        {
            try
            {
                Cio cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    companyName = curCompanyName,
                    groupId = curGroup.groupId,
                    request = "QueryAppUserGroupPermissionList"
                };

                string content, httpMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "AppUserGroup");
                    isSuccess = httpClient.isSuccessStatusCode;
                    httpMessage = httpClient.lastErrorDesc;
                }

                if (isSuccess)
                {
                    App.cio = JsonConvert.DeserializeObject<Cio>(content);  // < -- set the global variable  
                    var groupPermission = App.cio.zwaUserGroupsPermission;

                    if (groupPermission == null) return;
                    if (groupPermission.Length == 0) return;

                    if (userGroupPermissionList == null) userGroupPermissionList = new List<zwaUserGroup1>();
                    userGroupPermissionList.AddRange(groupPermission);
                    return;
                }

                //ELSE
                BRequest br = JsonConvert.DeserializeObject<BRequest>(content);  // < -- set the global variable                    
                DisplayAlert("Alert", $"{br?.Message}\n{httpMessage}", "OK");

            }
            catch (Exception excep)
            {
                DisplayAlert(excep.ToString());
            }
        }

        /// <summary>
        /// Load user from server based on group id and company id
        /// </summary>
        async void QueryAppGroupUserListAsync()
        {
            try
            {
                if (curGroup.groupId == -1) // since this is new group // asume not user in the group                
                    return;

                Cio cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    companyName = curGroup.companyId,
                    groupId = curGroup.groupId,
                    request = "QueryAppGroupUserList"
                };

                var httpClient = new HttpClientWapi();
                string content = await httpClient.RequestSvrAsync_mgt(cioRequest, "AppUserGroup");

                // incase no user in this group
                groupUserTitleCount = "Manage User";
                if (httpClient.isSuccessStatusCode)
                {
                    App.cio = JsonConvert.DeserializeObject<Cio>(content);  // < -- set the global variable     
                    zwaUser[] app_group = App.cio.zwAppUsers;

                    if (app_group == null) return;
                    if (app_group.Length == 0) return;

                    userList = new List<zwaUser>();
                    userList.AddRange(app_group);
                    groupUserTitleCount = (userList == null) ? "Manage User" : "Manager Users, " + userList.Count;
                    return;
                }

                groupUserTitleCount = "Manage User";

                BRequest br = JsonConvert.DeserializeObject<BRequest>(content);  // < -- set the global variable                    
                DisplayAlert("Alert", $"{br?.Message}\n{httpClient.lastErrorDesc}", "OK");
            }
            catch (Exception excep)
            {
                DisplayAlert(excep.ToString());
            }
        }

        /// <summary>
        /// Display message from VM to client screen 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="btnStr"></param>
        async void DisplayAlert(string title, string message, string accept) => await new Dialog().DisplayAlert(title, message, accept);

        /// <summary>
        /// Display the alert message
        /// </summary>
        /// <param name="message"></param>
        async void DisplayAlert(string message)
        {
            await new Dialog().DisplayAlert("Alert", message, "OK");
        }

        /// <summary>
        /// send message to user screen
        /// </summary>
        /// <param name="message"></param>
        void DisplayToastMessage(string message, int LongPeriod = 1)
        {
            var toast = DependencyService.Get<IToastMessage>();

            if (LongPeriod == 1)
            {
                toast?.LongAlert(message);
                return;
            }
            toast?.ShortAlert(message);
        }

        /// <summary>
        /// Unsubscribe the ser
        /// </summary>
        /// <param name="serviceName"></param>
        void UnsubscribeMc(string serviceName) => MessagingCenter.Unsubscribe<object>(this, serviceName);

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
