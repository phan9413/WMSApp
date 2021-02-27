using DbClass;
using Newtonsoft.Json;
using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Dtos;
using WMSApp.Interface;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Share
{
    public class SelectVendorPopUpVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public Command CmdSearchBarVisible { get; set; }

        OCRD selecteditem;
        public OCRD SelectedItem
        {
            get => selecteditem; 
            set
            {
                selecteditem = null;
                OnPropertyChanged(nameof(SelectedItem));

                MessagingCenter.Send(value, returnAddr);
                Navigation.PopPopupAsync(); // select and pop up the screen
            }
        }

        List<OCRD> itemsSource;
        public ObservableCollection<OCRD> ItemsSource { get; set; }

        public string SearchText
        {
            set => HandlerSearchText(value);
        }

        string title;
        public string Title
        {
            get => title;
            set
            {
                if (title == value) return;
                title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        string returnAddr;
        INavigation Navigation;

        Color headerColor;
        public Color HeaderColor
        {
            get => headerColor;
            set
            {
                if (headerColor == value) return;
                headerColor = value;
                OnPropertyChanged(nameof(HeaderColor));
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="callerAddress"></param>
        public SelectVendorPopUpVM(INavigation navigation, string callerAddress, 
            string title, Color headerColor, bool isLoadBp = false)
        {
            Navigation = navigation;
            returnAddr = callerAddress;
            Title = title;
            HeaderColor = headerColor;
            InitCmd();

            if (isLoadBp) LoadBpFromServer();
            else  LoadVendorFromServer();            
        }

        /// <summary>
        /// Initial the command
        /// </summary>
        void InitCmd()
        {
            CmdSearchBarVisible = new Command<SearchBar>((SearchBar sbar) =>
            {
                sbar.IsVisible = !sbar.IsVisible;
                if (sbar.IsVisible)
                {
                    sbar.Focus();
                    sbar.Placeholder = "Enter vendor code / name to search";
                }
            });
        }

        /// <summary>
        /// Load vendor from the server
        /// </summary>
        async void LoadVendorFromServer()
        {
            try
            {
                // you are here
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "QueryVendor",
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Grpo");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    DisplayMessage(content);
                    return;
                }

                var replied = JsonConvert.DeserializeObject<DtoGrpo>(content);
                if (replied == null)
                {
                    DisplayToast("There is error reading the data from the server, please try again later, Thanks. [N0]");
                    return;
                }

                if (replied.Vendors == null)
                {
                    DisplayToast("There is error reading the data from the server, please try again later, Thanks.[N1]");
                    return;
                }

                itemsSource = new List<OCRD>(replied.Vendors);                
                RefreshListView();
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Load vendor from the server
        /// </summary>
        async void LoadBpFromServer()
        {
            try
            {
                // you are here
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "GetBp",
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Do");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    DisplayMessage(content);
                    return;
                }

                var replied = JsonConvert.DeserializeObject<DTO_ORDR>(content);
                if (replied == null)
                {
                    DisplayToast("There is error reading the data from the server, please try again later, Thanks. [N0]");
                    return;
                }

                if (replied.Bp == null)
                {
                    DisplayToast("There is error reading the data from the server, please try again later, Thanks.[N1]");
                    return;
                }

                itemsSource = new List<OCRD>(replied.Bp);
                RefreshListView();
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Refresh the list view
        /// </summary>
        void RefreshListView()
        {
            if (itemsSource == null) return;
            if (itemsSource.Count == 0) return;
            ItemsSource = new ObservableCollection<OCRD>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// Handle the search Text
        /// </summary>
        /// <param name="query"></param>
        void HandlerSearchText(string query)
        {
            try
            {
                if (itemsSource == null) return;

                if (string.IsNullOrWhiteSpace(query))
                {
                    RefreshListView();
                    return;
                }

                var filter = itemsSource.Where(
                            x => x.CardCode.Contains(query) ||
                            x.CardName.Contains(query)).ToList();

                if (filter == null)
                {
                    RefreshListView();
                    return;
                }

                if (filter.Count == 0)
                {
                    RefreshListView();
                    return;
                }

                ItemsSource = new ObservableCollection<OCRD>(filter);
                OnPropertyChanged(nameof(ItemsSource));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        /// <summary>
        /// Display message onto the screen
        /// </summary>
        /// <param name="message"></param>
        async void DisplayMessage(string message) => await new Dialog().DisplayAlert("Alert", message, "Ok");

        /// <summary>
        /// Display toast
        /// </summary>
        /// <param name="message"></param>
        void DisplayToast(string message) => DependencyService.Get<IToastMessage>()?.ShortAlert(message);

    }
}
