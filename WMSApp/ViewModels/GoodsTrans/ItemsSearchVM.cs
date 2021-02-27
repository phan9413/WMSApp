using Acr.UserDialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Models.GIGR;
using Xamarin.Forms;

namespace WMSApp.ViewModels.GIGR
{
    public class ItemsSearchVM : IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string pName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pName));
        public void Dispose() { }// => GC.Collect();

        string queryText;
        public string QueryText
        {
            get => queryText;
            set
            {
                if (queryText != value)
                {
                    queryText = value;
                    HandlerSearchQuery(queryText);

                }
            }
        }

        OITM_Ex selectedItem;
        public OITM_Ex SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem != value)
                {
                    selectedItem = value;
                    HandlerSelectecItem(selectedItem);

                    selectedItem = null;
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        List<OITM_Ex> itemsSource;
        public ObservableCollection<OITM_Ex> ItemsSource { get; set; }

        INavigation Navigation;

        /// <summary>
        /// Constructor
        /// </summary>
        public ItemsSearchVM(INavigation navigation)
        {
            Navigation = navigation;
            LoadItemList();            
        }

        // Sent the selected item back to the 
        void HandlerSelectecItem(OITM_Ex selected)
        {
            // perform qty capture 
            // if warehouse 
            // sent back the object to add in

            //MessagingCenter.Send<OITM_Ex>(selected, "AddItemBySearch");
            //Navigation.PopAsync(); // close the screen
        }

        /// <summary>
        /// Load the item from the server
        /// </summary>
        async void LoadItemList()
        {
            try
            {
                UserDialogs.Instance.ShowLoading("A moment please...");
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "GetItems"
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Items");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (isSuccess)
                {
                    var retBag = JsonConvert.DeserializeObject<Cio>(content);
                    if (retBag.Items == null) return;

                    itemsSource = new List<OITM_Ex>();
                    foreach (var item in retBag.Items)
                    {
                        itemsSource.Add(
                            new OITM_Ex
                            {
                                Item = item
                            });
                    }

                    ResetListView();
                    UserDialogs.Instance.HideLoading();
                    return;
                }

                //ELSE
                var bRequest = JsonConvert.DeserializeObject<BRequest>(content);
                lastErrMessage = $"{lastErrMessage}\n{bRequest?.Message}";
                ShowAlert($"{lastErrMessage}");
            }
            catch (Exception excep)
            {
                ShowAlert($"{excep}");
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Reset the list view for user screen
        /// </summary>
        void ResetListView()
        {
            if (itemsSource == null)
            {
                ItemsSource = null;
                OnPropertyChanged(nameof(ItemsSource));
                return;
            }

            // reset the item and update to screen
            ItemsSource = new ObservableCollection<OITM_Ex>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// Handler search list
        /// </summary>
        /// <param name="query"></param>
        void HandlerSearchQuery(string query)
        {
            if (itemsSource == null) return;
            if (string.IsNullOrWhiteSpace(query))
            {
                ResetListView();
                return;
            }

            var lowerCaseQuery = query.ToLower();
            var filterList = itemsSource
                .Where(x => x.ItemCodeDisplay.ToLower().Contains(lowerCaseQuery) ||
                       x.ItemNameDisplay.ToLower().Contains(lowerCaseQuery)).ToList();

            if (filterList == null)
            {
                ResetListView();
                return;
            }

            if (filterList.Count == 0)
            {
                ResetListView();
                return;
            }

            ItemsSource = new ObservableCollection<OITM_Ex>(filterList);
            OnPropertyChanged(nameof(ItemsSource));
        }

        void ShowAlert(string message)
        {
            _ = new Dialog().DisplayAlert("Alert", message, "OK");
        }

    }

}
