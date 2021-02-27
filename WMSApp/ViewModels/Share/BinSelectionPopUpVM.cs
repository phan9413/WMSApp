using Acr.UserDialogs;
using DbClass;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using WMSApp.Class;
using WMSApp.Interface;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Share
{
    public class BinSelectionPopUpVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string pName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pName));

        string currentWhsCode;
        public string CurrentWhsCode 
        {
            get => currentWhsCode;
            set
            {
                if (currentWhsCode == value) return;
                currentWhsCode = value;
                OnPropertyChanged(nameof(CurrentWhsCode));
            }
        }

        string selectBinForDistNumber;
        public string SelectBinForDistNumber
        {
            get => selectBinForDistNumber;
            set
            {
                if (selectBinForDistNumber == value) return;
                selectBinForDistNumber = value;
                OnPropertyChanged(nameof(selectBinForDistNumber));
            }
        }

        OBIN selectedItem;
        public OBIN  SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;
                HandlerSelectedObin(selectedItem);

                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        List<OBIN> itemsSource; 
        public ObservableCollection<OBIN> ItemsSource { get; set; }
        INavigation Navigation;
        string ReturnAddrs;
        /// <summary>
        /// The constructor
        /// </summary>
        public BinSelectionPopUpVM (INavigation navigation, string selectedDistNumber, string whsCode, string returnAddr)
        {
            Navigation = navigation;
            CurrentWhsCode = whsCode;
            SelectBinForDistNumber = selectedDistNumber;
            ReturnAddrs = returnAddr;
            LoadBinDetailsFromSvr();
        }

        /// <summary>
        /// Send the selected bin back to caller screen
        /// </summary>
        /// <param name="seletedBin"></param>
        void HandlerSelectedObin (OBIN seletedBin)
        {
            MessagingCenter.Send(seletedBin, ReturnAddrs);
            Navigation.PopAsync();
        }

        /// <summary>
        /// Load bin detail from Whs
        /// </summary>
        async void LoadBinDetailsFromSvr ()
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
                    request = "GetWarehouseBins",
                    QueryWhs = currentWhsCode
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Transfer");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (isSuccess)
                {
                    var repliedCio = JsonConvert.DeserializeObject<Cio>(content);
                    if (repliedCio == null) return;
                    if (repliedCio.CommonStockInfos == null) return;

                    itemsSource = new List<OBIN>(repliedCio.DtoBins);
                    RefreshListView();
                    return;
                }

                //ELSE
                var bRequest = JsonConvert.DeserializeObject<BRequest>(content);
                if (bRequest == null)
                {
                    DisplayAlert(lastErrMessage + "\n" + content);
                    return;
                }

                lastErrMessage = $"{lastErrMessage}\n{bRequest?.Message}";
                DisplayAlert($"{lastErrMessage}");
            }
            catch (Exception excep)
            {
                Console.WriteLine($"{excep}");
                DisplayAlert(excep.Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Refresh the listview binding
        /// </summary>
        void RefreshListView ()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<OBIN>(ItemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        void DisplayToast(string message) => DependencyService.Get<IToastMessage>()?.ShortAlert(message);

        /// <summary>
        /// Display alert
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        async void DisplayAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "OK");

    }
}
