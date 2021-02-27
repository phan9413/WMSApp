using System.Collections.Generic;
using System.ComponentModel;
using WMSApp.Class;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Acr.UserDialogs;
using System;
using WMSApp.Interface;
using Newtonsoft.Json;
using System.Linq;
using WMSApp.Views.StandAloneTransfer;

namespace WMSApp.ViewModels.StandAloneTransfer
{
    public class StandAloneTransferListVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Command CmdSetSearchBarVisible { get; set; }

        zwaHoldRequest selectedItem;
        public zwaHoldRequest SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;
                HandlerSelectedDoc(selectedItem);

                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public string SearchQuery
        {
            set
            {
                HandlerQuerySearch(value);
            }
        }

        List<zwaHoldRequest> itemsSource;
        public ObservableCollection<zwaHoldRequest> ItemsSource { get; set; }

        INavigation Navigation;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="navigation"></param>
        public StandAloneTransferListVM (INavigation navigation)
        {
            Navigation = navigation;
            LoadDoc();
            InitCmds();
        }

        /// <summary>
        /// Init the command method link
        /// </summary>
        void InitCmds()
        {
            CmdSetSearchBarVisible = new Command<SearchBar>((SearchBar searchbar) => 
            {
                searchbar.IsVisible = !searchbar.IsVisible;
                if (searchbar.IsVisible)
                {
                    searchbar.Focus();
                }
            });
        }

        /// <summary>
        /// Handler the query search text
        /// </summary>
        /// <param name="query"></param>
        void HandlerQuerySearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                RefreshListView();
                return;
            }

            if (itemsSource == null) return;

            if (itemsSource.Count == 0) return;

            var lowerCase = query.ToLower();
            var filterList = itemsSource.Where(x => x.Id.ToString().Contains(lowerCase)).ToList();      
            if (filterList == null)
            {
                DisplayToast("No item found, please try again");
                return;
            }

            ItemsSource = new ObservableCollection<zwaHoldRequest>(filterList);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// Handle selected item from the list view tapped
        /// </summary>
        /// <param name="selected"></param>
        async void HandlerSelectedDoc(zwaHoldRequest selected) => 
            await Navigation.PushAsync(new StandAloneTransferLineTOView(selected, "TO"));        

        /// <summary>
        /// Load open STA doc from server
        /// </summary>
        async void LoadDoc()
        {
            try
            {
                UserDialogs.Instance.ShowLoading("A moment ...");
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "LoadSTADocList",                    
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Transfer1");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    DisplayMessage($"{content}\n{lastErrMessage}\nOr not exist in any selected warehouse. Please try again later");
                    return;
                }

                Cio repliedCio = JsonConvert.DeserializeObject<Cio>(content);
                if (repliedCio == null)
                {
                    DisplayMessage("Error reading server information to procee the list view, please try again later. Thanks");
                    return;
                }

                if (repliedCio.dtozwaHoldRequests == null)
                {
                    DisplayMessage("There is no stand alone transfer found in server, please try again later [null]. Thanks");
                    PopScreen();
                    return;
                }

                if (repliedCio.dtozwaHoldRequests.Length == 0)
                {
                    DisplayMessage("There is no stand alone transfer found in server, please try again later [zero]. Thanks");
                    PopScreen();
                    return;
                }

                if (itemsSource == null) itemsSource = new List<zwaHoldRequest>();
                itemsSource.AddRange(repliedCio.dtozwaHoldRequests);
                RefreshListView();
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Quit the screen
        /// </summary>
        void PopScreen() => Navigation.PopAsync();
        

        /// <summary>
        /// Refresh the list view onto the screen
        /// </summary>
        void RefreshListView()
        {
            if (itemsSource == null) return;
            if (itemsSource.Count == 0) return;
            ItemsSource = new ObservableCollection<zwaHoldRequest>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// Display message
        /// </summary>
        /// <param name="message"></param>
        async void DisplayMessage(string message) => await new Dialog().DisplayAlert("Alert", message, "OK");

        void DisplayToast(string message) => DependencyService.Get<IToastMessage>()?.LongAlert(message);

    }
}
