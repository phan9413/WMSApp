using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using WMSApp.Class;
using WMSApp.ClassObj;
using WMSApp.ClassObj.Item;
using WMSApp.View.Item;
using WMSApp.View.Warehouse;
using Xamarin.Forms;

namespace WMSApp.ViewModel.Item
{
    public class ItemListVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Command CmdByWhs { get; set; }
        List<OITMObj> itemsSource { get; set; } = null;

        public ObservableCollection<OITMObj> ItemsSource { get; set; }

        public string SearchText
        {
            set => HandleSearch(value);
        }

        OITMObj selecteditem { get; set; } = null;
        public OITMObj SelectedItem
        {
            get => selecteditem;
            set
            {
                if (selecteditem == value) return;
                selecteditem = value;
                HandleSelected(selecteditem);

                selecteditem = null;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        INavigation navigation { get; set; } = null;

        /// <summary>
        /// Consructor
        /// </summary>
        /// <param name="Navigation"></param>
        public ItemListVM(INavigation Navigation)
        {
            navigation = Navigation;
            CmdByWhs = new Command(() =>
           {
               Navigation.PushAsync(new WarehouseListV());


           });
         
            LoadItem();
        }

        async void LoadItem()
        {
            dynamic request = new ExpandoObject();
            request.request = "QueryGetAllItem";

            string content, lastErrMessage;
            bool isSuccess = false;
            using (var httpClient = new HttpClientWapi())
            {
                content = await httpClient.RequestSvrAsync_NoAuthn(request, "ItemsRequest");
                isSuccess = httpClient.isSuccessStatusCode;
                lastErrMessage = httpClient.lastErrorDesc;
            }

            if (!isSuccess)
            {
                ShowAlert($"{content}\n{lastErrMessage}");
                return;
            }

            var dtoWhs = JsonConvert.DeserializeObject<ItemDTO>(content);
            if (dtoWhs == null)
            {
                ShowAlert("Please make sure your Content is not empty.");
                return;
            }

            if (dtoWhs.oITMs == null)
            {
                ShowAlert("The Item Data is empty.");
                return;
            }

            itemsSource = new List<OITMObj>(dtoWhs.oITMs);
            RefreshListview();
        }

        void RefreshListview()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<OITMObj>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        void HandleSearch(string value)
        {
            try
            {
                if (itemsSource == null) return;
                if (itemsSource.Count == 0) return;

                if (string.IsNullOrWhiteSpace(value))
                {
                    RefreshListview();
                    return;
                }

                var lowerCapv = value.ToLower();
                var filter = itemsSource.Where(x =>
                                        x.ItemCode != null &&
                                        x.ItemName != null &&
                                        x.ItemCode.ToLower().Contains(lowerCapv) ||
                                        x.ItemName.ToLower().Contains(lowerCapv)).ToList();

                if (filter == null)
                {
                    RefreshListview();
                    return;
                }

                if (filter.Count == 0)
                {
                    RefreshListview();
                    return;
                }

                ItemsSource = new ObservableCollection<OITMObj>(filter);
                OnPropertyChanged(nameof(ItemsSource));
            }
            catch (Exception e)
            {
                LogException(e);
            }
        }

        void LogException(Exception e)
        {
            Console.WriteLine(e.ToString());
            ShowAlert($"{e.Message}\n{e.StackTrace}");
        }

        void HandleSelected(OITMObj selectedItem) => navigation.PushAsync(new TrackItemDetailsV(selectedItem));


        async void ShowAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "ok");
    }
}
