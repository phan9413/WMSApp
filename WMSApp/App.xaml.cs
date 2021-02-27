using DbClass;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using WMSApp.Class;
using WMSApp.Class.Helper;
using WMSApp.Models.Setup;
using WMSApp.Models.Setup.Group;
using WMSApp.Views.Login;
using WMSApp.Views.RequestSummary;
using WMSApp.Views.SimplifiedMain;
using WMSApp.Views.Test;
using Xamarin.Forms;

namespace WMSApp
{
    public partial class App : Application
    {
        public static ShareUltilities su = new ShareUltilities();
        // ---------------- Global Messaging Center string 
        public static readonly string _snChngWebApiConnViewSendTbAddres = "ChngWebApiConnViewSendTbAddres";
        public static readonly string _SNRegisterWebAddrViaIPAddr = "RegisterWebAddrViaIPAddr";
        public static readonly string _SNSendScannerCtrl = "SendScannerCtrl";
        public static readonly string _SNNavigation = "SendNavigationCtrl";

        public static readonly string _RequestSummaryView = "RequestSummaryView";
        //<---- for the login data transmission 
        public static readonly string userKey = "O0man6ipa1Dme8Hong"; // "3s6s9sf@str@ck";

        // sqlite database file name
        public static readonly string sap_db_name = "sapdb.db3";
        public static readonly string appName = "IMApp";

        // ---------------- SAP server company list 
        public static diSAPCompanyModel[] sapCompanyList;
        public static diSAPCompanyModel currentSapCompany;

        // ---------------- Global beerer token, server authorized token key 
        public static BearerToken bearerToken;

        // un-use, reserved    
        public static string registerPhoneKey = string.Empty;

        // ----------------- centralized the connection
        public static HttpClient client = new HttpClient(); //new HttpClient(new HttpClientHandler());

        // used is indicate current internet access
        public static bool iwWorkOffline = false;

        public static Cio cio;

        public static bool IsSuperAdmin = false;
        public static bool IsAdminRoles = false;

        /// <summary>
        /// Web api replied property        
        /// </summary>

        // -------------------- current app culture, default setup the culture to UI culture
        public static CultureInfo currentCultureInfo { get; set; } = CultureInfo.CurrentUICulture;

        // ------------------- current app connected user authority
        public static string waUser { get; set; } = string.Empty;
        public static string waPw { get; set; } = string.Empty;
        public static string waToken { get; set; } = string.Empty;
        public static string companyName { get; set; } = string.Empty;
        public static string waCurrentUser { get; set; } = string.Empty; // replacement of App.waSalesPersonName = cio.salesPersonName; 
        public static string waCurrentUserRoles { get; set; } = string.Empty;
        // ----------------- current user permission reference, infor from server 
        public static zwaUserGroup1[] currentPermissions { get; set; } = null;
        public static zwaUserGroup currentGroup { get; set; } = null;

        // 20200617T1030
        public static OWHS[] Warehouses { get; set; }
        public static OBIN[] Bins; // 20200718T1339

        //// 20200619T OBIN location 
        //public static OBIN[] BinLocations { get; set; }
        // 20200617 for keeing track the send request from phone to server
        // also add in the notifcation to the phone to preview and further action
        public static List<zwaRequest> RequestList { get; set; } = new List<zwaRequest>();        
        public static RequestCheckHelper RequestChecking { get; set; } = new RequestCheckHelper();
        public static bool IsContinueRequestChecking { get; set; } = false;

        /// <summary>
        /// get the current culture setup of the 
        /// read the phone calcture
        /// </summary>

        public App(string nextView = "", string data ="")
        {
            InitializeComponent();
            Device.SetFlags(new string[] { "Expander_Experimental", "SwipeView_Experimental" });

            currentCultureInfo = new CultureInfo("en"); //new CultureInfo("zh-Hans");
                                                        //currentCultureInfo = new CultureInfo("zh-Hans"); //new CultureInfo("zh-Hans");

            if (string.IsNullOrEmpty(nextView))
            {
                MainPage = new NavigationPage(new LoginView());                
                return;
            }
            HandlerNextView(nextView, data);
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        public static void HandlerNextView(string nextView, string data)
        {
            switch (nextView)
            {
                case "RequestSummaryView":
                    {
                        zwaRequest request = JsonConvert.DeserializeObject<zwaRequest>(data);
                        if (request == null)
                        {
                            return;
                        }

                        MessagingCenter.Send(request, _RequestSummaryView);
                        break;
                    }
                case "LoginView": // login view
                    {
                        Current.MainPage = new NavigationPage(new LoginView());
                        break;
                    }
                case "SimplifyMainMenuView": // no login
                    {
                        if (Current.MainPage == null)
                        {
                            Current.MainPage = new NavigationPage(new LoginView());
                            return;
                        }

                        Current.MainPage = new NavigationPage(new SimplifyMainMenuView());
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        /// <summary>
        /// Set the main page after the login
        /// </summary>
        public static void SetMainPage() // Sawpt the root page from login page into mainpage (master details page)
        {
            Current.MainPage = new NavigationPage(new SimplifyMainMenuView()); //new MainView();
        }

        public static void StartDocStatusCheckTimer()
        {
            Device.StartTimer(TimeSpan.FromSeconds(6), () =>
            {
                RequestChecking.StartCheckStatus();
                if (IsContinueRequestChecking)
                {                    
                    return true; // return true to continue,
                }
                return false;   //false to stop
            });
        }
    }
}
