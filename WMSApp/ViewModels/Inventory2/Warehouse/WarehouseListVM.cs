using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using WMSApp.Class;
using WMSApp.ClassObj;
using WMSApp.ClassObj.Warehouse;
using WMSApp.View.Warehouse;
using Xamarin.Forms;

namespace WMSApp.ViewModel.Warehouse
{
    public class WarehouseListVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        List<OWHSObj> _ItemsSource;

        public ObservableCollection<OWHSObj> ItemsSource { get; set; }

        OWHSObj _SelectedItem;
        public OWHSObj SelectedItem
        {
            get => _SelectedItem;
            set
            {
                if (_SelectedItem == value) return;
                _SelectedItem = value;
                HandleSelected(_SelectedItem);

                _SelectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));

            }
        }
        public string searchText;
        public string SearchText
        {
            get => searchText;
            set
            {
                if (searchText != value)
                    searchText = value;
                OnPropertyChanged(nameof(SearchText));
                HandleSearch(value);
            }
        }

        INavigation navigation { get; set; }

        public WarehouseListVM(INavigation Navigation)
        {
            navigation = Navigation;
            LoadItem();
        }

        async void LoadItem()
        {
            dynamic _request = new ExpandoObject();
            _request.request = "QueryAllWarehouse";


            string content, lastErrMessage;
            bool isSuccess = false;
            using (var httpClient = new HttpClientWapi())
            {
                content = await httpClient.RequestSvrAsync_NoAuthn(_request, "WarehouseRequest");
                isSuccess = httpClient.isSuccessStatusCode;
                lastErrMessage = httpClient.lastErrorDesc;
            }

            if (!isSuccess)
            {
                ShowAlert($"{content}\n{lastErrMessage}");
                return;
            }

            var dtoWhs = JsonConvert.DeserializeObject<WarehouseDTO>(content);
            if (dtoWhs == null) { ShowAlert("Please make sure your Content is not empty."); return; }

            _ItemsSource = new List<OWHSObj>(dtoWhs.oWHSs);
            ItemsSource = new ObservableCollection<OWHSObj>(_ItemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        void HandleSearch(string value)
        {
            try
            {
                if (_ItemsSource == null) return;
                if (_ItemsSource.Count == 0) return;

                if (string.IsNullOrWhiteSpace(value))
                {
                    RefreshListview();
                    return;
                }

                var lowerCapv = value.ToLower();
                var filter = _ItemsSource
                    .Where(
                        x =>
                             x.WhsCode.Contains(lowerCapv) ||
                             x.WhsName.ToLower().Contains(lowerCapv)
                    ).ToList();

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

                ItemsSource = new ObservableCollection<OWHSObj>(filter);
                OnPropertyChanged(nameof(ItemsSource));
            }
            catch (Exception e)
            {
                LogException(e);
            }
        }

        void RefreshListview()
        {
            if (_ItemsSource == null) return;
            ItemsSource = new ObservableCollection<OWHSObj>(_ItemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        void HandleSelected(OWHSObj selected) => navigation.PushAsync(new ShowItemAvailableV(selected));

        void LogException(Exception e)
        {
            Console.WriteLine(e.ToString());
            ShowAlert($"{e.Message}\n{e.StackTrace}");
        }

        async void ShowAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "ok");
    }
}
