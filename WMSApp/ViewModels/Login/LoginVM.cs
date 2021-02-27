using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using WMSApp.Views.Login;
using SQLite;
using ZXing.Net.Mobile.Forms;
using System.Linq;
using Xamarin.Essentials;
using System.Net.Http;
using WMSApp.Class;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using WMSApp.Models.Setup.User;
using System.IO;
using System.Net;
using ZXing.Mobile;
using WMSApp.Models.Setup;
using WMSApp.Interface;
using WMSApp.Models.Login;
using WMSApp.Dtos;

namespace WMSApp.ViewModels.Login
{
    public class LoginVM : IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// View binding related properties
        /// </summary>
        #region 
        public Command cmdRefreshConnection { get; private set; }
        public Command cmdSetupWebLink { get; private set; }
        public Command cmdSetupQrCodeLink { get; private set; }
        public Command cmdQuit { get; private set; }
        public Command cmdLogin { get; private set; }
        public Command cmdCancelScan { get; private set; }

        string _ScannerMiddleText;
        public string ScannerMiddleText
        {
            get => translateExt.GetLabelValue(resourceId, _ScannerMiddleText, App.currentCultureInfo);
            set
            {
                if (_ScannerMiddleText == value) return;
                _ScannerMiddleText = value;
                OnPropertyChanged(nameof(ScannerMiddleText));
            }
        }

        string _TextConnectStatus;
        public string TextConnectStatus
        {
            get => translateExt.GetLabelValue(resourceId, _TextConnectStatus, App.currentCultureInfo);
            set
            {
                if (_TextConnectStatus == value) return;
                _TextConnectStatus = value;
                OnPropertyChanged(nameof(TextConnectStatus));
            }
        }

        string _TextSetupQrCodeLink;
        public string TextSetupQrCodeLink
        {
            get => translateExt.GetLabelValue(resourceId, _TextSetupQrCodeLink, App.currentCultureInfo);
            set
            {
                if (_TextSetupQrCodeLink == value) return;
                _TextSetupQrCodeLink = value;
                OnPropertyChanged(nameof(TextSetupQrCodeLink));
            }
        }

        string _TextSetupAddressLink;
        public string TextSetupAddressLink
        {
            get => translateExt.GetLabelValue(resourceId, _TextSetupAddressLink, App.currentCultureInfo);
            set
            {
                if (_TextSetupAddressLink == value) return;
                _TextSetupAddressLink = value;
                OnPropertyChanged(nameof(TextSetupAddressLink));
            }
        }

        string _TextQuit;
        public string TextQuit
        {
            get => translateExt.GetLabelValue(resourceId, _TextQuit, App.currentCultureInfo);
            set
            {
                if (_TextQuit == value) return;
                _TextQuit = value;
                OnPropertyChanged(nameof(TextQuit));
            }
        }


        string _Title;
        public string Title
        {
            get => translateExt.GetLabelValue(resourceId, _Title, App.currentCultureInfo);  //_Title;            
            set
            {
                if (_Title == value) return;
                _Title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        bool _stckScannerVisible;
        public bool stckScannerVisible
        {
            get => _stckScannerVisible;
            set
            {
                if (_stckScannerVisible == value) return;
                _stckScannerVisible = value;
                OnPropertyChanged(nameof(stckScannerVisible));
            }
        }

        bool _stckLoginVisible;
        public bool stckLoginVisible
        {
            get => _stckLoginVisible;
            set
            {
                if (_stckLoginVisible == value) return;
                _stckLoginVisible = value;
                OnPropertyChanged(nameof(stckLoginVisible));
            }
        }

        bool _actIndVisibility;
        public bool actIndVisibility
        {
            get => _actIndVisibility;
            set
            {
                if (_actIndVisibility == value) return;
                _actIndVisibility = value;
                OnPropertyChanged(nameof(actIndVisibility));
            }
        }

        string[] _pkerCompany;
        public string[] pkerCompany
        {
            get => _pkerCompany;
            set
            {
                if (_pkerCompany == value) return;
                _pkerCompany = value;
                OnPropertyChanged(nameof(pkerCompany));
            }
        }
        string _pkerSelectedItem;
        public string pkerSelectedItem
        {
            get => _pkerSelectedItem;
            set
            {
                if (_pkerSelectedItem == value) return;
                _pkerSelectedItem = value;
                OnPropertyChanged(nameof(pkerSelectedItem));
            }
        }

        string _tbName;
        public string tbName
        {
            get
            {
                if (Preferences.Get(nameof(RememberMe), false))
                {
                    return Preferences.Get(nameof(tbName), _tbName);
                }
                return _tbName;
            }
            set
            {
                if (Preferences.Get(nameof(RememberMe), false))
                {
                    Preferences.Set(nameof(tbName), value);
                }

                if (_tbName == value) return;
                _tbName = value;
                OnPropertyChanged(nameof(tbName));
            }
        }

        string _tbPw;
        public string tbPw
        {
            get
            {
                if (Preferences.Get(nameof(RememberMe), false))
                {
                    return Preferences.Get(nameof(tbPw), _tbPw);
                }
                return _tbPw;
            }
            set
            {
                if (Preferences.Get(nameof(RememberMe), false))
                {
                    Preferences.Set(nameof(tbPw), value);
                }

                if (_tbPw == value) return;
                _tbPw = value;
                OnPropertyChanged(nameof(tbPw));
            }
        }

        bool _isViewPw;
        public bool isViewPw
        {
            get => _isViewPw;
            set
            {
                if (_isViewPw == value) return;
                _isViewPw = value;
                OnPropertyChanged(nameof(isViewPw));
                OnPropertyChanged(nameof(isPwVisible));
            }
        }

        public bool isPwVisible
        {
            get => !_isViewPw;
        }

        bool _isMenuEnable;
        public bool isMenuEnable
        {
            get => _isMenuEnable;
            set
            {
                if (_isMenuEnable == value) return;
                _isMenuEnable = value;
                OnPropertyChanged(nameof(isMenuEnable));
            }
        }

        string _ViewScanCancel;
        public string ViewScanCancel
        {
            get => translateExt.GetLabelValue(resourceId, _ViewScanCancel, App.currentCultureInfo);  //_Title; }
            set
            {
                if (_ViewScanCancel == value) return;
                _ViewScanCancel = value;
                OnPropertyChanged(nameof(ViewScanCancel));
            }
        }

        string _LabelCompany;
        public string LabelCompany
        {
            get => translateExt.GetLabelValue(resourceId, _LabelCompany, App.currentCultureInfo);  //_Title; }
            set
            {
                if (_LabelCompany == value) return;
                _LabelCompany = value;
                OnPropertyChanged(nameof(LabelCompany));
            }
        }

        string _LabelUserId;
        public string LabelUserId
        {
            get => translateExt.GetLabelValue(resourceId, _LabelUserId, App.currentCultureInfo);   //_Title; }
            set
            {
                if (_LabelUserId == value) return;
                _LabelUserId = value;
                OnPropertyChanged(nameof(LabelUserId));
            }
        }

        string _LabelPassword;
        public string LabelPassword
        {
            get => translateExt.GetLabelValue(resourceId, _LabelPassword, App.currentCultureInfo);  //_Title; }
            set
            {
                if (_LabelPassword == value) return;
                _LabelPassword = value;
                OnPropertyChanged(nameof(LabelPassword));
            }
        }

        string _BtnLabelLogon;
        public string BtnLabelLogon
        {
            get => translateExt.GetLabelValue(resourceId, _BtnLabelLogon, App.currentCultureInfo);   //_Title; }
            set
            {
                if (_BtnLabelLogon == value) return;
                _BtnLabelLogon = value;
                OnPropertyChanged(nameof(BtnLabelLogon));
            }
        }

        string _LabelSwicthPasswordVisible;
        public string LabelSwicthPasswordVisible
        {
            get => translateExt.GetLabelValue(resourceId, _LabelSwicthPasswordVisible, App.currentCultureInfo);   //_Title; }
            set
            {
                if (_LabelSwicthPasswordVisible == value) return;
                _LabelSwicthPasswordVisible = value;
                OnPropertyChanged(nameof(LabelSwicthPasswordVisible));
            }
        }

        public bool RememberMe
        {
            get => Preferences.Get(nameof(RememberMe), false);
            set => Preferences.Set(nameof(RememberMe), value);
        }
        
        public string Version
        {
            get
            {
                var version =   DependencyService.Get<IAppVersion>()?.Version;
                if (string.IsNullOrWhiteSpace(version)) return string.Empty;              
                return $"version {version}";
            }
        }

        #endregion

        // Private declaration        
        readonly string resourceId = "WMSApp.Resources.Login.LoginView.LoginView";
        TranslateExtension translateExt = new TranslateExtension(); // 20200503T1359

        INavigation Navigation;
        SQLiteConnection sqliteConnection;
        ZXingScannerView scannerView;

        //readonly string titleLogin = "APP Login";
        //readonly string titleScanning = "Scan Server Address in QR Code";
        //readonly string tableName = "mwUser";
        string lastErrMessage;
        bool isDbTableExist;
        bool tempRejectScan = false;

        /// <summary>
        /// Constructor, class entry point
        /// </summary>
        /// <param name="navigation"></param>
        public LoginVM(INavigation navigation)
        {
            SetupLanguageResource();
            Navigation = navigation;
            isMenuEnable = true;
            isViewPw = false;
            stckScannerVisible = false;
            stckLoginVisible = true;

            LoadCompanyNameFromWebApi();
            InitCmd();
            SubscribeMC();
            SetupActIndicator(false);
        }

        /// <summary>
        /// to init the resource language 
        /// </summary>
        void SetupLanguageResource()
        {
            Title = "ViewTiltleLogin";
            TextConnectStatus = "ToolBarItem_ConnectStatus_Connected";
            TextSetupQrCodeLink = "ToolbarItem_SetupLinkWeb_Qr";
            TextSetupAddressLink = "ToolbarItem_SetupLinkWeb_Addr";
            TextQuit = "ToolbarItem_Quit";
            ScannerMiddleText = "ScannerMiddleText";
            ViewScanCancel = "ViewScanCancel";
            LabelCompany = "LabelCompany";
            LabelUserId = "LabelUserId";
            LabelPassword = "LabelPassword";
            BtnLabelLogon = "BtnLabelLogon";
            LabelSwicthPasswordVisible = "LabelSwicthPasswordVisible";
        }

        /// <summary>
        /// Init public command action link
        /// </summary>
        void InitCmd()
        {
            cmdRefreshConnection = new Command(LoadCompanyNameFromWebApi);
            cmdSetupWebLink = new Command(() =>
            {
                Navigation.PushAsync(new ChngWebApiConnView());
            });

            cmdSetupQrCodeLink = new Command(StartScanner);
            cmdQuit = new Command(ConfirmQuit);
            cmdLogin = new Command(HandlerLogin);
            cmdCancelScan = new Command(HandleCancel);
        }

        /// <summary>
        /// Subscribe needed messaging center
        /// </summary>
        void SubscribeMC()
        {
            // subscribe / receive message from web ip address
            MessagingCenter.Subscribe<object>(this, App._SNRegisterWebAddrViaIPAddr, (object obj) =>
            {
                // activate this function to reload the company list
                LoadCompanyNameFromWebApi();
            });

            MessagingCenter.Subscribe<object>(this, App._SNSendScannerCtrl, (object obj) =>
            {
                scannerView = (ZXingScannerView)obj;
                scannerView.OnScanResult += ScannerView_OnScanResult;
                MessagingCenter.Unsubscribe<object>(this, App._SNSendScannerCtrl);
            });
        }

        /// <summary>
        /// Unsubscribe the messaging center
        /// </summary>
        void UnSubscribeMC()
        {
            MessagingCenter.Unsubscribe<object>(this, App._SNRegisterWebAddrViaIPAddr);
            MessagingCenter.Unsubscribe<object>(this, App._SNSendScannerCtrl);
            MessagingCenter.Unsubscribe<object>(this, App._SNNavigation);
        }

        /// <summary>
        /// Set the App current selectec company name
        /// </summary>
        /// <param name="selectedCompanyName"></param>
        void UpdateSelectedCurrentSAPCompany(string selectedCompanyName)
        {
            try
            {
                var selectedCIF = App.sapCompanyList.Where(x => x.cmpName.Equals(selectedCompanyName)).FirstOrDefault();
                App.currentSapCompany = selectedCIF;
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", excep.ToString(), "OK");
            }
        }

        /// <summary>
        /// Procedure to handle login
        /// </summary>
        void HandlerLogin()
        {
            bool isvalid = IsValid();
            if (isvalid && _tbName != null && _tbPw != null)
            {
                SetupActIndicator(true);
                // check internet connection 
                if (!(Connectivity.NetworkAccess == NetworkAccess.Internet))
                {
                    VerifyLocalAuthentication(); // check internal database
                    return;
                }

                // request login token 
                GetBearerToken(_tbName, _tbPw);
            }
        }

        /// <summary>
        /// request bearer token from the web server
        /// </summary>
        /// <param name="userIdName"></param>
        /// <param name="password"></param>
        async void GetBearerToken(string userIdName, string password)
        {
            try
            {
                #region Prepare for request token bearer
                string webSvrAddr = App.su.WEBAPI_HOST + "users/token";

                var authUser = new zwaUserModel
                {
                    userIdName = userIdName,
                    password = new MD5EnDecrytor().Encrypt(password, true, App.userKey),
                    companyId = String.IsNullOrWhiteSpace(_pkerSelectedItem) ? "" : _pkerSelectedItem
                };

                //// code reference from https://briancaos.wordpress.com/2019/08/28/httpclient-post-or-put-json-with-content-type-application-json/
                HttpResponseMessage postReplied = null;
                string jsonSerial = JsonConvert.SerializeObject(authUser);
                var stringContent = new StringContent(jsonSerial, System.Text.Encoding.UTF8, App.su.APP_JSON);
                postReplied = await App.client.PostAsync(webSvrAddr, stringContent);

                //handle the replied from server
                if (postReplied != null && !postReplied.IsSuccessStatusCode)
                {
                    string resultNok = await postReplied.Content.ReadAsStringAsync();
                    string langitepty90Header = translateExt.GetLabelValue(resourceId, nameof(langitepty90Header), App.currentCultureInfo);
                    string langitepty90Message = translateExt.GetLabelValue(resourceId, nameof(langitepty90Message), App.currentCultureInfo);
                    string langitepty90Ok = translateExt.GetLabelValue(resourceId, nameof(langitepty90Ok), App.currentCultureInfo);

                    DisplayAlert(langitepty90Header, $"{resultNok}\n{langitepty90Message}", langitepty90Ok);
                    SetupActIndicator(false);
                    return;
                }

                // upon success request and replied
                string resultOk = await postReplied.Content.ReadAsStringAsync();
                var replied = JsonConvert.DeserializeObject<DtoAuthen>(resultOk);

                //App.cio = JsonConvert.DeserializeObject<Cio>(resultOk);
                App.bearerToken = replied.bearerToken;
                if (App.bearerToken == null)
                {
                    string langitepty93Header = translateExt.GetLabelValue(resourceId, nameof(langitepty93Header), App.currentCultureInfo);
                    string langitepty93Message = translateExt.GetLabelValue(resourceId, nameof(langitepty93Message), App.currentCultureInfo);
                    string langitepty93Ok = translateExt.GetLabelValue(resourceId, nameof(langitepty93Ok), App.currentCultureInfo);
                    DisplayAlert(langitepty93Header, langitepty93Message, langitepty93Ok);
                    SetupActIndicator(false);
                    return;
                }

                #endregion

                #region Once got token bearer, prepare login
                // process main menu loading
                // if there is internet then verified with web api
                string selectedCoyName = String.IsNullOrWhiteSpace(_pkerSelectedItem) ? "" : _pkerSelectedItem;
                string usrIdName = (String.IsNullOrWhiteSpace(_tbName)) ? "" : _tbName.Trim();
                string passw = (String.IsNullOrWhiteSpace(_tbPw)) ? "" : _tbPw.Trim();

                var cio_request = new Cio()
                {
                    sap_logon_name = usrIdName,
                    sap_logon_pw = new MD5EnDecrytor().Encrypt(passw, true, App.userKey),
                    request = "Login",
                    companyName = selectedCoyName,
                    companyId = -1,
                    phoneRegID = App.registerPhoneKey
                };

                bool isSuccess = false;
                string lastHttpErroMessage = "", contentStr = "";
                using (var _client = new HttpClientWapi())
                {
                    contentStr = await _client.RequestSvrAsync_mgt(cio_request, "LOGIN");
                    isSuccess = _client.isSuccessStatusCode;
                    lastHttpErroMessage = _client.lastErrorDesc;
                    Thread.Sleep(900);
                }

                if (!isSuccess)
                {
                    string offLineHeader = translateExt.GetLabelValue(resourceId, nameof(offLineHeader), App.currentCultureInfo);
                    string offLineMessage = translateExt.GetLabelValue(resourceId, nameof(offLineMessage), App.currentCultureInfo);
                    string offLineMessageYes = translateExt.GetLabelValue(resourceId, nameof(offLineMessageYes), App.currentCultureInfo);
                    string offLineMessageNo = translateExt.GetLabelValue(resourceId, nameof(offLineMessageNo), App.currentCultureInfo);
                    bool usedLocalAuthen = await DisplayAlert(offLineHeader, $"{lastHttpErroMessage}\n{offLineMessage}", offLineMessageYes, offLineMessageNo);

                    if (usedLocalAuthen) VerifyLocalAuthentication();
                    SetupActIndicator(false);
                    return;
                }
                #endregion

                #region Finally handler the login success with bearer

                // when everything ok, process next                                                                        
                //HandlerResponseSuccess(contentStr);
                App.cio = JsonConvert.DeserializeObject<Cio>(contentStr);
                HandlerResponseSuccess();
                App.SetMainPage();
                SetupActIndicator(false);

                #endregion
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", excep.Message, "OK");
            }
        }

        /// <summary>
        /// Handle when program received OK from web APIs
        /// </summary>
        /// <param name="success_r"></param>
        void HandlerResponseSuccess()//string successResponse)
        {
            // var cio = JsonConvert.DeserializeObject<Cio>(successResponse);  // < -- set the global variable   
            if (App.cio != null)
            {
                App.waToken = App.cio.token; //<--- guid
                App.waUser = App.cio.sap_logon_name;

                App.waPw = _tbPw;
                App.waCurrentUser = App.cio.currentUser;
                App.waCurrentUserRoles = App.cio.currentUserRole;

                App.IsSuperAdmin = (App.cio.sap_logon_name.ToLower().Equals("superadmin"));
                if (!string.IsNullOrWhiteSpace(App.cio.currentUserRole))
                {
                    App.IsAdminRoles = (App.cio.currentUserRole.ToLower().Equals("admin")); // 20200626T1104
                }

                App.currentPermissions = App.cio.currentPermissions;
                App.currentGroup = App.cio.currentGroup;
                App.companyName = App.currentGroup.companyId;
                UpdateSelectedCurrentSAPCompany(_pkerSelectedItem);
            }
        }

        /// <summary>
        /// Handle the bad request replied from web api
        /// </summary>
        /// <param name="failure_r"></param>
        /// <param name="lastClientMsg"></param>
        /// <param name="isSuppressMsg"></param>
        //void HandlerReponseBadReqest(string failResponse, string message, bool isSuppressMsg = false)
        //{
        //    if (!isSuppressMsg && failResponse.Length > 0)
        //    {
        //        BRequest badRequest = JsonConvert.DeserializeObject<BRequest>(failResponse);

        //        string translateAlertLCx00 = translateExt.GetLabelValue(resourceId, "translateAlertLCx00", App.currentCultureInfo);
        //        DisplayAlert("Alert", $"{badRequest?.Message}\n{translateAlertLCx00}", "OK");
        //        return;
        //    }

        //    if (!isSuppressMsg && message.Length > 0)
        //    {
        //        string translateAlertLCx01 = translateExt.GetLabelValue(resourceId, "translateAlertLCx01", App.currentCultureInfo);
        //        DisplayAlert("Alert", $"{message}\n{translateAlertLCx01}", "OK");
        //        return;
        //    }

        //    if (!isSuppressMsg)
        //    {
        //        string translateAlertLCx02 = translateExt.GetLabelValue(resourceId, "translateAlertLCx02", App.currentCultureInfo);
        //        DisplayAlert("Alert", translateAlertLCx02, "OK");
        //    }
        //}

        /// <summary>
        /// When offline login, code verify previuos login to get local authenticated
        /// if verified, user will see the main menu
        /// </summary>
        async void VerifyLocalAuthentication()
        {
            // check internal database            
            LoadDatabase();

            if (CheckUserLogin(_tbName, _tbPw))
            {
                SetupActIndicator(false);
                // 20200217T2029
                // setup a offline flag to control all
                App.iwWorkOffline = true;
                App.waUser = _tbName;
                App.waPw = new MD5EnDecrytor().Decrypt(_tbPw, true, App.userKey);
                App.waToken = "";
                App.SetMainPage();
                await Navigation.PopModalAsync();
                return;
            }

            DisplayAlert("Alert", lastErrMessage, "OK");
            SetupActIndicator(false);
            return;
        }

        /// <summary>
        /// Setup loading in hide or show
        /// </summary>
        /// <param name="isOn"></param>
        void SetupActIndicator(bool isVisible) => actIndVisibility = isVisible;

        /// <summary>
        /// When login button click, verify the user name and password were entered
        /// </summary>
        /// <returns></returns>
        bool IsValid()
        {
            if (String.IsNullOrWhiteSpace(_tbName))
            {
                string InvalidUserId = translateExt.GetLabelValue(resourceId, nameof(InvalidUserId), App.currentCultureInfo);
                DisplayAlert("Alert", InvalidUserId, "Ok");
                return false;
            }

            if (String.IsNullOrWhiteSpace(_tbPw))
            {
                string InvalidPassword = translateExt.GetLabelValue(resourceId, nameof(InvalidPassword), App.currentCultureInfo);
                DisplayAlert("Alert", InvalidPassword, "Ok");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Handle the command quit 
        /// </summary>
        async void ConfirmQuit()
        {
            string langQuit = translateExt.GetLabelValue(resourceId, "AppQuit", App.currentCultureInfo);
            string langYes = translateExt.GetLabelValue(resourceId, "AppYes", App.currentCultureInfo);
            string langNo = translateExt.GetLabelValue(resourceId, "AppNo", App.currentCultureInfo);
            bool confirm_quit = await DisplayAlert(langQuit, "", langYes, langNo);
            if (confirm_quit)
            {
                var closer = DependencyService.Get<ICloseApplication>();
                closer?.closeApplication();
            }
        }

        /// <summary>
        /// Handle to load list of sap company name from the web api 
        /// </summary>
        async void LoadCompanyNameFromWebApi()
        {
            try
            {
                var webServerAddress = App.su.WEBAPI_HOST + "users/sapcompany/";
                using (var client = new HttpClient())
                {
                    var content = await client.PostAsync(webServerAddress, null);
                    if (content.IsSuccessStatusCode)
                    {
                        string result = content.Content.ReadAsStringAsync().Result;
                        App.sapCompanyList = JsonConvert.DeserializeObject<diSAPCompanyModel[]>(result);
                        var companyNameList = App.sapCompanyList.Select(x => x.cmpName).Distinct().ToList();

                        // Show OEC only for demo purpose
                        // 20201003
                        if (companyNameList == null) return;
                        if (companyNameList.Count == 0) return;

                        //pkerCompany = companyNameList?.ToArray();
                        pkerCompany = new string[] { "DEMO OEC Computers" };

                        pkerSelectedItem = (pkerCompany != null && pkerCompany.Length > 0) ? pkerCompany[0] : "";
                        TextConnectStatus = "ToolBarItem_ConnectStatus_Connected";//"Connected";
                    }
                    else
                    {
                        TextConnectStatus = "ToolBarItem_ConnectStatus_Reconnect"; //"Reconnect";
                        pkerCompany = new string[] { "Default" };
                        pkerSelectedItem = pkerCompany[0];
                    }
                }
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", excep.Message, "OK");
            }
        }

        /// <summary>
        ///  start scanner
        /// </summary>
        void StartScanner()
        {
            try
            {
                stckScannerVisible = true;
                stckLoginVisible = false;
                isMenuEnable = false;
                Title = "ViewTitlteScanner";

                var options = new MobileBarcodeScanningOptions();
                options.PossibleFormats = new List<ZXing.BarcodeFormat>() {
                    ZXing.BarcodeFormat.QR_CODE,
                    ZXing.BarcodeFormat.All_1D};

                scannerView.Options = options;
                scannerView.IsEnabled = true;
                scannerView.IsScanning = true;
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", excep.ToString(), "OK");
            }
        }

        /// <summary>
        /// Simulator for thie scan
        /// </summary>
        /// <param name="result"></param>
        void ScannerView_OnScanResult(ZXing.Result result)
        {
            try
            {
                if (result == null) return;
                Device.BeginInvokeOnMainThread(() =>
                {
                    HandlerScannerResult(result.ToString());
                });
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", excep.Message, "OK");
            }
        }

        /// <summary>
        /// Handle the scanner result and test
        /// </summary>
        /// <param name="obj"></param>
        async void HandlerScannerResult(string scan_result)
        {
            try
            {
                if (tempRejectScan) return;
                tempRejectScan = true;

                //string scan_result = (string)obj;
                string scanResultHeader = translateExt.GetLabelValue(resourceId, nameof(scanResultHeader), App.currentCultureInfo);
                string scanTestConnectMsg = translateExt.GetLabelValue(resourceId, nameof(scanTestConnectMsg), App.currentCultureInfo);
                string scanTestAccept = translateExt.GetLabelValue(resourceId, nameof(scanTestAccept), App.currentCultureInfo);
                string scanTestCancel = translateExt.GetLabelValue(resourceId, nameof(scanTestCancel), App.currentCultureInfo);

                bool isToTest = await DisplayAlert($"{scanResultHeader}\n{scan_result}", scanTestConnectMsg, scanTestAccept, scanTestCancel);
                if (!isToTest)
                {
                    HandleCancel();
                    tempRejectScan = false;
                    return;
                }

                string web_address = scan_result.Trim();
                bool result = Uri.TryCreate(web_address, UriKind.Absolute, out Uri uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                if (!result)
                {
                    string invalidServerAddrs = translateExt.GetLabelValue(resourceId, nameof(invalidServerAddrs), App.currentCultureInfo);
                    DisplayAlert("Alert", invalidServerAddrs, "Ok");
                    tempRejectScan = false;
                    return;
                }

                TestConnectionAsync(web_address);
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", excep.Message, "OK");
            }
        }

        /// <summary>
        /// Use to test the Web api address , or test the commucation between app and server
        /// </summary>
        /// <param name = "web_address" ></ param >
        async void TestConnectionAsync(string webAdress)
        {
            try
            {
                string tempAddress = webAdress + "api/values/9";
                Uri uri = new Uri(tempAddress);

                var client = new WebClient();
                string test_result = client.DownloadString(webAdress);

                if (!test_result.Contains("9"))
                {
                    string misMatchReplied = translateExt.GetLabelValue(resourceId, nameof(misMatchReplied), App.currentCultureInfo);
                    DisplayAlert("Alert", misMatchReplied, "Ok");
                }

                SetupActIndicator(false);

                // Getting the translated message from resource
                string testConnectSuccessHeader = translateExt.GetLabelValue(resourceId, nameof(testConnectSuccessHeader), App.currentCultureInfo);
                string testConnectSuccessMsg = translateExt.GetLabelValue(resourceId, nameof(testConnectSuccessMsg), App.currentCultureInfo);
                string testConnectSave = translateExt.GetLabelValue(resourceId, nameof(testConnectSave), App.currentCultureInfo);
                string testConnectCancel = translateExt.GetLabelValue(resourceId, nameof(testConnectCancel), App.currentCultureInfo);

                bool confirmSaveFile = await DisplayAlert(
                    $"{testConnectSuccessHeader}\n{tempAddress}",
                    testConnectSuccessMsg,
                    testConnectSave,
                    testConnectCancel);

                if (confirmSaveFile) SaveToFile(tempAddress); // perform the save operation to file                          

                HandleCancel();
                tempRejectScan = false;
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// Handle the event when user wish to cancel the login
        /// </summary>
        void HandleCancel()
        {
            stckScannerVisible = false;
            stckLoginVisible = true;
            Title = "ViewTiltleLogin";
            isMenuEnable = true;
        }

        /// <summary>
        /// Perform the web api address saved into the local mobile phone text file
        /// </summary>
        /// <param name="test_result"></param>
        void SaveToFile(string result)
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string filename = Path.Combine(path, "spserver.txt");

                if (File.Exists(filename)) File.Delete(filename); //<--- if file exist then delete the file     

                using (var streamWriter = new StreamWriter(filename, true))
                {
                    streamWriter.WriteLine(result.Trim());
                }

                App.su.WEBAPI_HOST = result.Trim(); // update the current address in memory 
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "Ok");
            }
        }

        /// <summary>
        /// Load db usr info from the sqlite database
        /// </summary>
        void LoadDatabase()
        {
            // intial the component
            try
            {
                ReconnectDb();
                if (!IsDbTableExist(nameof(mwUser))) // if not exist then create the table        
                {
                    sqliteConnection.CreateTable<mwUser>();
                    isDbTableExist = true;
                }
            }
            catch (Exception exception)
            {
                lastErrMessage = exception.ToString();
                DisplayAlert("Alert", exception.Message, "OK");
            }
        }

        /// <summary>
        /// Reconnect db, ensure the db is always available
        /// </summary>
        void ReconnectDb()
        {
            string applicationPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string databasePath = Path.Combine(applicationPath, App.sap_db_name);

            if (sqliteConnection == null)
            {
                sqliteConnection = new SQLiteConnection(databasePath); // open the sqlite database      
            }
        }

        /// <summary>
        /// Check User Login
        /// </summary>
        /// <param name="salesPersonCode"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        bool CheckUserLogin(string userCode, string password)
        {
            try
            {
                ReconnectDb();
                if (sqliteConnection.Table<mwUser>().Count() == 0)
                {
                    InsertUser(userCode, password);
                    return true;
                }

                mwUser checkUser = sqliteConnection.Table<mwUser>()
                    .Where(x => x.salesCode == userCode).FirstOrDefault();

                if (checkUser == null)
                {
                    lastErrMessage = translateExt.GetLabelValue(resourceId, "translateErrECx00", App.currentCultureInfo);
                    return false;
                }

                string dbPwVal = new MD5EnDecrytor().Decrypt(checkUser.password, true, App.userKey);
                if (dbPwVal.Equals(password))
                {
                    sqliteConnection.Execute($"DELETE FROM {nameof(mwUser)}"); // always had one record                            
                    InsertUser(userCode, password);
                    return true;
                }

                lastErrMessage = translateExt.GetLabelValue(resourceId, "translateErrECx01", App.currentCultureInfo);
                DisplayAlert("Alert", lastErrMessage, "OK");
                return false;
            }
            catch (Exception excep)
            {
                lastErrMessage = excep.ToString();
                DisplayAlert("Alert", excep.Message, "OK");
                return false;
            }
        }

        /// <summary>
        /// Insert user record into local db
        /// </summary>
        /// <param name="salesCode"></param>
        /// <param name="pw"></param>
        void InsertUser(string userId, string password)
        {
            //App.salesPersonCode = salesCode; // set global variable for the sale person code // for doc id             
            var newUsr = new mwUser()
            {
                salesCode = userId,
                password = new MD5EnDecrytor().Encrypt(password, true, App.userKey)
            };

            sqliteConnection.Insert(newUsr);
        }

        /// <summary>
        /// Check table exist
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="sqliteConnection"></param>
        /// <returns></returns>
        bool IsDbTableExist(string tableName)
        {
            try
            {
                var tableExist = sqliteConnection.GetTableInfo(tableName);
                return (tableExist != null && tableExist.Count > 0);
            }
            catch (Exception exp)
            {
                DisplayAlert($"{exp}");
                return false;
            }
        }

        /// <summary>
        /// Handler the display alert with confirmation
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="btnStr"></param>
        /// <param name="cancelBtn"></param>
        /// <returns></returns>
        async Task<bool> DisplayAlert(string title, string message, string accept, string cancel) =>
            await new Dialog().DisplayAlert(title, message, accept, cancel);

        /// <summary>
        /// Display message from VM to client screen 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="btnStr"></param>
        async void DisplayAlert(string title, string message, string accept) =>
            await new Dialog().DisplayAlert(title, message, accept);

        /// <summary>
        /// Short and convi method
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="btnStr"></param>
        async void DisplayAlert(string message) =>
            await new Dialog().DisplayAlert("Alert", message, "OK");

        /// <summary>
        /// Handle the on property changed, value update to screen
        /// </summary>
        /// <param name="propertyname"></param>
        public void OnPropertyChanged(string propertyname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));

        /// <summary>
        /// Dipose code
        /// </summary>
        public void Dispose()
        {
            sqliteConnection = null;
            UnSubscribeMC();
            //GC.Collect();
        }
    }
}
