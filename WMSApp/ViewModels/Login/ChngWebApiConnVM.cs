using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using WMSApp.Class;
using Xamarin.Forms;
namespace WMSApp.ViewModels.Login
{
    public class ChngWebApiConnVM : IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        #region Bindable property to page
        public Command cmdTestAndSave { get; private set; }
        public Command cmdCancel { get; private set; }
        public Command cmdBtn1 { get; private set; }
        public Command cmdBtn2 { get; private set; }
        public Command cmdBtn3 { get; private set; }

        bool _isActIndiEnabled;
        public bool isActIndiEnabled
        {
            get
            {
                return _isActIndiEnabled;
            }
            set
            {
                if (_isActIndiEnabled != value)
                {
                    _isActIndiEnabled = value;
                    OnPropertyChanged(nameof(isActIndiEnabled));
                }
                
            }
        }

        string _tbAddr;
        public string tbAddr
        {
            get
            {
                return _tbAddr;
            }
            set
            {
                if (_tbAddr != value)
                {
                    _tbAddr = value;
                    OnPropertyChanged(nameof(tbAddr));
                }
            }
        }
        string _toolbarItemCancel;
        public string ToolbarItemCancel 
        {
            get
            {
                return _toolbarItemCancel;
            }
            set
            {
                if(_toolbarItemCancel != value)
                {
                    _toolbarItemCancel = value;
                    OnPropertyChanged(nameof(ToolbarItemCancel));
                }
            }
        }

        string _toolbarItemTestAndSave; 
        public string ToolbarItemTestAndSave
        {
            get
            {
                return _toolbarItemTestAndSave;
            }
            set
            {
                if(_toolbarItemTestAndSave != value)
                {
                    _toolbarItemTestAndSave = value;
                    OnPropertyChanged(nameof(ToolbarItemTestAndSave));
                }
            }
        }

        string _Title;
        public string Title
        {
            get { return _Title; }
            set 
            { 
                if (_Title != value)
                {
                    _Title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        string _LabelWebApiInstru; 
        public string LabelWebApiInstru
        {
            get
            {
                return _LabelWebApiInstru;
            }
            set
            {
                if(_LabelWebApiInstru != value)
                {
                    _LabelWebApiInstru = value;
                    OnPropertyChanged(nameof(LabelWebApiInstru));
                }
            }
        }

        string _LabelWebApiSvrAddrs; 
        public string LabelWebApiSvrAddrs
        {
            get
            {
                return _LabelWebApiSvrAddrs;
            }
            set
            {
                if (_LabelWebApiSvrAddrs!= value)
                {
                    _LabelWebApiSvrAddrs = value;
                    OnPropertyChanged(nameof(LabelWebApiSvrAddrs));
                }
            }
        }

        string _BtnLabelTestConnect; 
        public string BtnLabelTestConnect
        {
            get
            {
                return _BtnLabelTestConnect;
            }
            set
            {
                if (_BtnLabelTestConnect != value)
                {
                    _BtnLabelTestConnect = value;
                    OnPropertyChanged(nameof(BtnLabelTestConnect));
                }   
            }
        }

        #endregion

        // Private declaration
        readonly string resourceId = "WMSApp.Resources.Login.ChngWebApiConnView.ChngWebApiConnView";
        static readonly string _defaultAddr = "http://216.81.183.227:20719/"; //"http://219.92.2.141:82/"; //"http://ftsap.com:42330/";

        TranslateExtension translateExt { get; set; } = new TranslateExtension(); // 20200503T1359
        INavigation Navigation;
        Entry entryAddr;

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
        /// Constructor, class entry
        /// </summary>
        /// <param name="navigation"></param>
        /// <param name="entry_addr"></param>
        public ChngWebApiConnVM(INavigation navigation)
        {
            Navigation = navigation;
            tbAddr = _defaultAddr;

            InitLang(); //<- 20200505T1542
            InitCmd();
            SetupActIndicator(false);
            SubribeMC();
        }

        /// <summary>
        /// Init the language for screen label display
        /// </summary>
        void InitLang ()
        {
            ToolbarItemCancel = translateExt.GetLabelValue(resourceId, nameof(ToolbarItemCancel), App.currentCultureInfo);
            ToolbarItemTestAndSave = translateExt.GetLabelValue(resourceId, nameof(ToolbarItemTestAndSave), App.currentCultureInfo);
            Title = translateExt.GetLabelValue(resourceId, nameof(Title), App.currentCultureInfo);
            LabelWebApiSvrAddrs = translateExt.GetLabelValue(resourceId, nameof(LabelWebApiSvrAddrs), App.currentCultureInfo);
            LabelWebApiInstru = translateExt.GetLabelValue(resourceId, nameof(LabelWebApiInstru), App.currentCultureInfo);
            BtnLabelTestConnect = translateExt.GetLabelValue(resourceId, nameof(BtnLabelTestConnect), App.currentCultureInfo);
        }

        /// <summary>
        /// Init for each command on the screen
        /// </summary>
        void InitCmd()
        {
            cmdTestAndSave = new Command(TestAndSave);
            cmdCancel = new Command(() => { Navigation.PopAsync(); });
            cmdBtn1 = new Command<Button>((Button button) => HandlerBtnCliked(button));
            cmdBtn2 = new Command<Button>((Button button) => HandlerBtnCliked(button));
            cmdBtn3 = new Command<Button>((Button button) => HandlerBtnCliked(button));
        }

        /// <summary>
        /// Subsribe the mc 
        /// </summary>
        void SubribeMC()
        {
            MessagingCenter.Subscribe<object>(this, App._snChngWebApiConnViewSendTbAddres, (object obj) =>
            {
                entryAddr = (Entry)obj;
                InitialScreenSetup();
                MessagingCenter.Unsubscribe<object>(this, App._snChngWebApiConnViewSendTbAddres);
            });
        }

        /// <summary>
        /// Set entry on the view to focus
        /// </summary>
        void InitialScreenSetup() => entryAddr.Focus();        

        /// <summary>
        /// Based on each button text, pass the button text to view entry control
        /// </summary>
        /// <param name="obj"></param>
        void HandlerBtnCliked(Button button) => tbAddr = button?.Text;                    

        /// <summary>
        /// Set the status of the activity indicated to false / true
        /// </summary>
        /// <param name="isOn"></param>
        void SetupActIndicator(bool isEnable) =>isActIndiEnabled = isEnable;
        
        /// <summary>
        /// For test the user entry 
        /// and if user agree then save the web address into mobile storage
        /// </summary>
        async void TestAndSave()
        {
            try
            {
                if (String.IsNullOrWhiteSpace(_tbAddr))
                {
                    string addressEmptyTitle = translateExt.GetLabelValue(resourceId, nameof(addressEmptyTitle), App.currentCultureInfo);
                    string addressEmptyTitleMessage = translateExt.GetLabelValue(resourceId, nameof(addressEmptyTitleMessage), App.currentCultureInfo);
                    string addressEmptyTitleConfirm = translateExt.GetLabelValue(resourceId, nameof(addressEmptyTitleConfirm), App.currentCultureInfo);
                    DisplayAlert(addressEmptyTitle, addressEmptyTitleMessage, addressEmptyTitleConfirm);
                    SetupActIndicator(false);
                    entryAddr.Focus();
                    return;
                }

                string web_address = _tbAddr;
                bool result = Uri.TryCreate(
                    web_address, UriKind.Absolute, out Uri uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp ||
                    uriResult.Scheme == Uri.UriSchemeHttps);

                if (!result)
                {
                    string addressIncorrectTitle = translateExt.GetLabelValue(resourceId, nameof(addressIncorrectTitle), App.currentCultureInfo);
                    string addressIncorrectMessage = translateExt.GetLabelValue(resourceId, nameof(addressIncorrectMessage), App.currentCultureInfo);
                    string addressIncorrectConfirm = translateExt.GetLabelValue(resourceId, nameof(addressIncorrectConfirm), App.currentCultureInfo);
                    DisplayAlert(addressIncorrectTitle, addressIncorrectMessage, addressIncorrectConfirm);
                    return;
                }

                // conduct to test head
                //Creating the HttpWebRequest                
                web_address = $"{web_address}values?id=9";

                Uri uri = new Uri(web_address);
                string test_result;
                using (var client = new WebClient())
                {
                    test_result = client.DownloadString(web_address);
                }
                uri = null; // reset the variable to null for GC

                if (!test_result.Contains("9"))
                {
                    string addressMismacthTitle = translateExt.GetLabelValue(resourceId, nameof(addressMismacthTitle), App.currentCultureInfo);
                    string addressMismacthMessage = translateExt.GetLabelValue(resourceId, nameof(addressMismacthMessage), App.currentCultureInfo);
                    string addressMismacthConfirm = translateExt.GetLabelValue(resourceId, nameof(addressMismacthConfirm), App.currentCultureInfo);


                    DisplayAlert(addressMismacthTitle, addressMismacthMessage, addressMismacthConfirm);
                    return;
                }

                SetupActIndicator(false);

                string successTitle = translateExt.GetLabelValue(resourceId, nameof(successTitle), App.currentCultureInfo);
                string successMessage = translateExt.GetLabelValue(resourceId, nameof(successMessage), App.currentCultureInfo);
                string successConfirmSave = translateExt.GetLabelValue(resourceId, nameof(successConfirmSave), App.currentCultureInfo);
                string successConfirmCancel = translateExt.GetLabelValue(resourceId, nameof(successConfirmCancel), App.currentCultureInfo);

                bool dialogRest = await DisplayAlert($"{successTitle}\n{web_address}",
                        $"\n\n{successMessage}", successConfirmSave, successConfirmCancel);

                if (dialogRest)
                {
                    SaveToFile(); // perform the save operation to file   
                                  // send message to LoginVM
                                  // to excute the load company from new web serve address

                    MessagingCenter.Send<object>(this, App._SNRegisterWebAddrViaIPAddr);
                    await Navigation.PopAsync();
                }
            }
            catch (Exception excep)
            {
                //return excep.Message;
                DisplayAlert("Exception", excep.Message, "OK");
            }
        }

        /// <summary>
        /// Save the veliad web address into the text file
        /// </summary>
        void SaveToFile()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string filename = Path.Combine(path, "spserver.txt");

                if (File.Exists(filename)) //<--- if file exist then delete the file                
                {
                    File.Delete(filename);
                }

                using (var streamWriter = new StreamWriter(filename, true))
                {
                    streamWriter.WriteLine(_tbAddr.Trim());
                }

                App.su.WEBAPI_HOST = _tbAddr.Trim(); // update the current address in memory 
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", excep.ToString(), "Ok");
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
          
    }
}
