using Acr.UserDialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.Login;
using WMSApp.Models.Setting;
using WMSApp.Models.Setup.Group;
using WMSApp.Views.Setting.AppUserGroup;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Setting.AppUser
{
    public class AppUserAddVM : IDisposable, INotifyPropertyChanged
    {   static class CreateSAPUser
        {
            public const bool Create = true;
            public const bool Ignore = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        List<zsCommDocList> _list; // list of group 
        public ObservableCollection<zsCommDocList> list { get; private set; }
        public Command cmdAdd { get; private set; }
        public Command cmdCancel { get; private set; }

        string _title { get; set; }
        public string title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnPropertyChanged(nameof(title));
            }
        }

        string _saveBntText;
        public string saveBntText
        {
            get
            {
                return _saveBntText;
            }
            set
            {
                _saveBntText = value;
                OnPropertyChanged(nameof(saveBntText));
            }
        }

        string _sapId;
        public string sapId
        {
            get
            {
                return _sapId;
            }
            set
            {
                if (_sapId != value)
                {
                    _sapId = value;
                    OnPropertyChanged(nameof(sapId));
                }
            }
        }

        bool isCreateSapUserCheckBoxEnable;
        public bool IsCreateSapUserCheckBoxEnable
        {
            get
            {
                return isCreateSapUserCheckBoxEnable;
            }
            set
            {
                if (isCreateSapUserCheckBoxEnable != value)
                {
                    isCreateSapUserCheckBoxEnable = value;
                    OnPropertyChanged(nameof(IsCreateSapUserCheckBoxEnable));
                }
            }
        }

        /// <summary>
        /// Handler the listview selected item
        /// </summary>
        zsCommDocFields _selectedItem;
        public zsCommDocFields selectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                if (_selectedItem == null) return;

                CaptureData(_selectedItem);
                selectedItem = null;
                OnPropertyChanged(nameof(selectedItem));
            }
        }

        bool _IsCreateSapUser;
        public bool IsCreateSapUser
        {
            get
            {
                return _IsCreateSapUser;
            }
            set
            {
                if (_IsCreateSapUser != value)
                {
                    _IsCreateSapUser = value;
                    OnPropertyChanged(nameof(IsCreateSapUser));
                }
            }
        }

        enum fieldIndex // <--- inner used
        {
            userIdName = 0,
            password = 1,
            displayName = 2,
            locked = 3,
            roles = 4,
            contactNumber = 5,
            email = 6,
            group = 7
        }
        static readonly string[] rolesOpt = { "Admin", "User", "Guest", "Anonymous" };
        static readonly string[] lockedOpt = { "Locked", "Active" };

        INavigation Navigation;
        zwaUser curUser;
        zsCommDocList scrList; //<--- group        
        AppUserListVM parent;

        /// <summary>
        /// The constructor for new
        /// </summary>
        public AppUserAddVM(INavigation navigation, AppUserListVM caller)
        {
            Navigation = navigation;
            parent = caller;
            _title = "Add New User";
            _saveBntText = "Add";
            curUser = new zwaUser();

            Init();
            InitCommand();
        }

        /// <summary>
        /// The constructor for edit
        /// </summary>
        public AppUserAddVM(INavigation navigation, AppUserListVM caller, zwaUser selected)
        {
            Navigation = navigation;
            parent = caller;
            _title = "Edit User";
            _saveBntText = "Update";
            curUser = selected;

            Init();
            InitCommand();
        }

        /// <summary>
        /// for external page to set the group info
        /// </summary>
        /// <param name="selected"></param>
        public void SetUserGroup(zwaUserGroup selected)
        {
            curUser.groupId = selected.groupId;
            curUser.groupName = selected.groupName;
            scrList[(int)fieldIndex.group].value = curUser.groupName;
            scrList[(int)fieldIndex.group].OnPropertyChanged("value");
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
                    case (int)fieldIndex.displayName:
                    case (int)fieldIndex.userIdName:
                    case (int)fieldIndex.contactNumber:
                    case (int)fieldIndex.email:
                    case (int)fieldIndex.password:
                        {
                            HandlerFieldInput(selected);
                            break;
                        }
                    case (int)fieldIndex.locked:
                        {
                            HandlerLockedInput(selected);
                            break;
                        }
                    case (int)fieldIndex.roles:
                        {
                            HandlerRolesInput(selected);
                            break;
                        }
                    case (int)fieldIndex.group:
                        {
                            HandlerGroupSelection(selected);
                            break;
                        }
                }
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// Show list of group in this company
        /// and ask user to select
        /// </summary>
        void HandlerGroupSelection(zsCommDocFields selected)
        {
            Navigation.PushAsync(new AppUserAddGroupListView(this,
                $"{curUser.displayName}",
                $"{selected.value}",
                $"{parent.curOADMCompany.CompnyName}"));
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

                // determin the input type
                Keyboard selectedKeyboard;
                switch (selectField.id)
                {
                    case (int)fieldIndex.contactNumber:
                        {
                            selectedKeyboard = Keyboard.Telephone;
                            break;
                        }
                    case (int)fieldIndex.email:
                        {
                            selectedKeyboard = Keyboard.Email;
                            break;
                        }
                    case (int)fieldIndex.userIdName:
                    case (int)fieldIndex.displayName:
                        {
                            selectedKeyboard = Keyboard.Text;
                            break;
                        }
                    case (int)fieldIndex.password:
                        {
                            selectedKeyboard = Keyboard.Default;
                            break;
                        }
                    default:
                        {
                            selectedKeyboard = Keyboard.Plain;
                            break;
                        }
                }

                string updatedValue = await new Dialog()
                    .DisplayPromptAsync(
                        $"Input {selectField.title}",
                        $"",
                        $"Save",
                        $"Cancel",
                        $"",
                        -1,
                        selectedKeyboard,
                        $"{selectField.value}");

                if (scrList[selectField.id].value == null)
                    scrList[selectField.id].value = string.Empty;

                // check / validation
                if (scrList[selectField.id].value.Equals(updatedValue + "")) return;

                if (String.IsNullOrWhiteSpace(updatedValue)) return;

                // update the field
                scrList[selectField.id].value = updatedValue.ToUpper();
                scrList[selectField.id].OnPropertyChanged("value");
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// Modular the code from the switch selector 
        /// </summary>
        /// <param name="selecteField"></param>
        void HandlerRolesInput(zsCommDocFields selecteField)
        {
            try
            {
                // use of the action sheet

                var config = new ActionSheetConfig
                {
                    Cancel = new ActionSheetOption("Cancel"),
                    Title = "Input " + selecteField.title
                };

                foreach (var option in rolesOpt)
                {
                    config.Add(option, new Action(async () => HandlerRolesData(option, selecteField.id)));
                }

                UserDialogs.Instance.ActionSheet(config); // show the action sheet on screen
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// Modular the code from the switch selector 
        /// </summary>
        /// <param name="selecteField"></param>
        void HandlerLockedInput(zsCommDocFields selecteField)
        {
            try
            {
                // use of the action sheet
                var config = new ActionSheetConfig
                {
                    Cancel = new ActionSheetOption("Cancel"),
                    Title = "Input " + selecteField.title
                };

                foreach (var option in lockedOpt)
                {
                    config.Add(option, new Action(async () => HandlerLockedData(option, selecteField.id)));
                }

                UserDialogs.Instance.ActionSheet(config); // show the action sheet on screen
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// use by the acr user dialog to capture and update the 
        /// list 
        /// </summary>
        /// <param name="updatedValue"></param>
        /// <param name="id"></param>
        void HandlerLockedData(string updatedValue, int id)
        {
            string uValue = (updatedValue.Equals("Locked")) ? "Y" : "N";
            scrList[id].value = uValue;
            scrList[id].OnPropertyChanged("value");
        }

        /// <summary>
        /// use by the acr user dialog to capture and update the 
        /// list 
        /// </summary>
        /// <param name="updatedValue"></param>
        /// <param name="id"></param>
        void HandlerRolesData(string updatedValue, int id)
        {
            if (id < 0) return;
            if (string.IsNullOrWhiteSpace(updatedValue)) updatedValue = string.Empty; // to ensure the data is at least empty string

            scrList[id].value = updatedValue?.ToUpper();
            scrList[id].OnPropertyChanged("value");
        }

        /// <summary>
        /// Perform the save action
        /// </summary>
        void Save()
        {
            if (!isValidated()) return;
            if (_saveBntText.ToLower().Equals("add"))
            {
                CreateNewUserToSvr();
                return;
            }

            if (_saveBntText.ToLower().Equals("update"))
            {
                UpdateNewUserToSvr();
            }
        }

        /// <summary>
        /// Send the user object to svr to create new user
        /// svr return a new list of the usr under this company
        /// </summary>
        async void CreateNewUserToSvr()
        {
            try
            {
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    newzwaUser = curUser,
                    request = "AddUserRequest"
                };

                var content = string.Empty;
                var lastErrorMessage = string.Empty;
                var isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "AppUsersSetup");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrorMessage = httpClient.lastErrorDesc;
                }

                if (isSuccess)
                {
                    ShowToastMsg($"User {curUser.userIdName} created");
                    parent?.LoadUsrListFromServer();
                    await Navigation.PopAsync();
                    return;
                }

                var badRequest = JsonConvert.DeserializeObject<BRequest>(content);  // < -- set the global variable                    
                DisplayAlert("Alert", $"{badRequest?.Message}\n{lastErrorMessage}", "OK");
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// Update existing user back to svr
        /// </summary>
        async void UpdateNewUserToSvr()
        {
            try
            {
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    newzwaUser = curUser,
                    request = "UpdateUserRequest"
                };

                var content = string.Empty;
                var lastErrorMessage = string.Empty;
                var isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "AppUsersSetup");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrorMessage = httpClient.lastErrorDesc;
                }

                if (isSuccess)
                {
                    ShowToastMsg($"User {curUser.userIdName} Updated");
                    parent?.LoadUsrListFromServer();
                    await Navigation.PopAsync();
                    return;
                }

                var badRequest = JsonConvert.DeserializeObject<BRequest>(content);  // < -- set the global variable                    
                DisplayAlert("Alert", $"{badRequest?.Message}\n{lastErrorMessage}", "OK");

            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
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
                /// Query the list to detect mandatory fields and value == null or empty
                var mandatoryFields = scrList.Where(x => x.isMandatoryField == true && String.IsNullOrWhiteSpace(x.value)).ToArray();

                // check each field here                
                for (int x = 0; x < mandatoryFields?.Length; x++)
                {
                    CaptureData(mandatoryFields[x]);
                }

                if (mandatoryFields?.Length > 0) return false;

                // ESLE
                // copy into the object for further action
                // for update and new                
                curUser.userIdName = $"{scrList[(int)fieldIndex.userIdName].value}";
                curUser.password = $"{scrList[(int)fieldIndex.password].value}";
                curUser.displayName = $"{scrList[(int)fieldIndex.displayName].value}";
                curUser.locked = $"{scrList[(int)fieldIndex.locked].value}";
                curUser.Roles = GetRolesId(scrList[(int)fieldIndex.roles].value);
                curUser.RoleDesc = $"{scrList[(int)fieldIndex.roles].value}";
                curUser.phoneNumber = $"{scrList[(int)fieldIndex.contactNumber].value}";
                curUser.email = $"{scrList[(int)fieldIndex.email].value}";
                curUser.lastModiUser = App.waUser;
                curUser.lastModiDate = DateTime.Now;
                curUser.companyId = parent.curOADMCompany.CompnyName;
                curUser.createERPSalesEmp = Convert.ToInt32(_IsCreateSapUser);

                if (_saveBntText.ToLower().Equals("add"))
                {
                    curUser.sysId = -1; // for new user    
                    bool checkDuplicated = CheckUserIdDuplication();
                    if (checkDuplicated)
                    {
                        DisplayAlert("Alert", "Duplication user Id Name, Please input different name, Thanks", "OK");
                        return false;
                    }
                }
                return true; /// everything for update 
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
                return false;
            }
        }

        /// <summary>
        /// check user duplication in the parent list
        /// </summary>
        /// <returns></returns>
        bool CheckUserIdDuplication()
        {
            try
            {
                if (_saveBntText.ToLower().Equals("add")) // new object
                {
                    // if list is empty mean new user
                    if (parent == null) return false;
                    if (parent.list == null) return false;

                    var dupObj = parent?.list.Where(x => x.userIdName.Equals(curUser.userIdName)).FirstOrDefault();
                    return (dupObj != null);
                }
                return true;
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
                return true;
            }
        }

        /// <summary>
        /// Verify the email address in valid format
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        //bool IsValidEmail(string email)
        //{
        //    try
        //    {
        //        var addr = new System.Net.Mail.MailAddress(email);
        //        return (addr.Address == email);
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        /// <summary>
        /// Get the roles id convert from text 
        /// </summary>
        /// <param name="rolesDesc"></param>
        /// <returns></returns>
        int GetRolesId(string rolesDesc)
        {
            switch (rolesDesc.ToLower())
            {
                case "superadmin":
                    {
                        return 4;
                    }
                case "admin":
                    {
                        return 3;
                    }
                case "user":
                    {
                        return 2;
                    }
                case "guest":
                    {
                        return 1;
                    }
                case "anonymous":
                default:
                    {
                        return 0;
                    }
            }
        }

        /// <summary>
        /// Init the command for add and cancel
        /// </summary>
        void InitCommand()
        {
            cmdAdd = new Command(Save);
            cmdCancel = new Command(async () =>
            {
                await Navigation.PopAsync();
            });
        }

        /// <summary>
        ///  Initial the screen
        /// </summary>
        void Init()
        {
            try
            {
                #region screen field setup
                scrList = new zsCommDocList();
                scrList.Heading = "";
                scrList.isVisible = false;

                IsCreateSapUser = (curUser.createERPSalesEmp > 0) ? true : false;
                IsCreateSapUserCheckBoxEnable = !_IsCreateSapUser;

                sapId = (String.IsNullOrWhiteSpace(curUser.sapId)) ?
                    "" : (curUser.sapId == "-1") ?
                    "" : (curUser.sapId == "1") ?
                    "" : $"SAP EmpID: {curUser.sapId}";

                scrList.Add(new zsCommDocFields
                {
                    title = "User Id Name",
                    value = curUser?.userIdName,
                    id = scrList.Count,
                    isRightArrowVisible = true,
                    isVisible = true,
                    isMandatoryField = true
                });


                scrList.Add(new zsCommDocFields
                {
                    title = "Password",
                    value = curUser?.password,
                    id = scrList.Count,
                    isRightArrowVisible = true,
                    isVisible = true,
                    isMandatoryField = true
                });

                // sapId skip

                scrList.Add(new zsCommDocFields
                {
                    title = "Display Name",
                    value = curUser?.displayName,
                    id = scrList.Count,
                    isRightArrowVisible = true,
                    isVisible = true,
                    isMandatoryField = true
                });


                scrList.Add(new zsCommDocFields
                {
                    title = "Locked",
                    value = curUser?.locked,
                    id = scrList.Count,
                    isRightArrowVisible = true,
                    isVisible = true,
                    isMandatoryField = true
                });


                scrList.Add(new zsCommDocFields
                {
                    title = "Roles",
                    value = curUser?.RoleDesc,
                    id = scrList.Count,
                    isRightArrowVisible = true,
                    isVisible = true,
                    isMandatoryField = true
                });


                scrList.Add(new zsCommDocFields
                {
                    title = "Contact Number",
                    value = curUser?.phoneNumber,
                    id = scrList.Count,
                    isRightArrowVisible = true,
                    isVisible = true,
                });


                scrList.Add(new zsCommDocFields
                {
                    title = "Email",
                    value = curUser?.email,
                    id = scrList.Count,
                    isRightArrowVisible = true,
                    isVisible = true
                });

                scrList.Add(new zsCommDocFields
                {
                    title = "Group",
                    value = curUser?.groupName,
                    id = scrList.Count,
                    isRightArrowVisible = true,
                    isVisible = true,
                    isMandatoryField = true
                });

                scrList.isVisible = true;
                scrList.Heading = "Account Info";
                ResetList();
                #endregion
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// used when list data changed 
        /// </summary>
        void ResetList()
        {
            scrList.isVisible = true;
            scrList.Heading = "Account Info";

            _list = new List<zsCommDocList>();
            _list.Add(scrList); // Add list of field into the group
            list = new ObservableCollection<zsCommDocList>(_list); // <-- convert the group into the obc list            
            OnPropertyChanged(nameof(list));
        }

        /// <summary>
        /// send message to user screen
        /// </summary>
        /// <param name="message"></param>
        void ShowToastMsg(string message)
        {
            var toast = DependencyService.Get<IToastMessage>();
            toast?.ShortAlert(message);
        }


        /// <summary>
        /// Display message from VM to client screen 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="btnStr"></param>
        async void DisplayAlert(string title, string message, string accept) => await new Dialog().DisplayAlert(title, message, accept);

        /// <summary>
        /// Handle the on property changed, value update to screen
        /// </summary>
        /// <param name="propertyname"></param>
        public void OnPropertyChanged(string propertyname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));

        /// <summary>
        /// Dispose the code
        /// </summary>
        public void Dispose() { }// => GC.Collect();
    }
}
