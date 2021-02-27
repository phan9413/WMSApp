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
    public class ShowItemAvailableVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Command CmdVisibleSearchBar { get; set; }
        string whsCode;
        public string WhsCode
        {
            get => whsCode;
            set
            {
                if (whsCode != value)
                {
                    whsCode = value;
                    OnPropertyChanged(nameof(WhsCode));
                }
            }
        }

        string whsName;
        public string WhsName
        {
            get => whsName;
            set
            {
                if (whsName != value)
                {
                    whsName = value;
                    OnPropertyChanged(nameof(WhsName));
                }
            }
        }

        OWHSObj SelectedWhs { get; set; }
        INavigation Navigation { get; set; }

        List<AvailableItemObj> _ItemsSource;
        public ObservableCollection<AvailableItemObj> ItemsSource { get; set; }

        AvailableItemObj selectedItem;
        public AvailableItemObj SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem != value)
                {
                    selectedItem = value;
                    HandleSelected(selectedItem);
                    selectedItem = null;
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }
        public string SearchText
        {
            set => HandleSearch(value);
        }

        bool _IsSearchBarVisible { get; set; } = false;
        public bool IsSearchBarVisible
        {
            get => _IsSearchBarVisible;
            set
            {
                if (_IsSearchBarVisible == value) return;
                _IsSearchBarVisible = value;
                OnPropertyChanged(nameof(IsSearchBarVisible));
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="navigation"></param>
        /// <param name="selectedWarehouse"></param>
        public ShowItemAvailableVM(INavigation navigation, OWHSObj selectedWarehouse)
        {
            Navigation = navigation;
            SelectedWhs = selectedWarehouse;
            WhsCode = selectedWarehouse.WhsCode;
            WhsName = selectedWarehouse.WhsName;

            CmdVisibleSearchBar = new Command<SearchBar>((SearchBar sbar) =>
            {
                sbar.IsVisible = !sbar.IsVisible;
                if (sbar.IsVisible) sbar.Focus();
                IsSearchBarVisible = sbar.IsVisible;
            });

            LoadAvailableItems();
        }

        async void LoadAvailableItems()
        {
            dynamic _request = new ExpandoObject();
            _request.request = "QueryGetAvailableItem";
            _request.SelectedWarehouse = WhsCode;

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
            if (dtoWhs == null) { ShowAlert("The Item Data is empty."); return; }
            _ItemsSource = new List<AvailableItemObj>(dtoWhs.AvailableItemObjs);
            ItemsSource = new ObservableCollection<AvailableItemObj>(_ItemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        void RefreshListview()
        {
            if (_ItemsSource == null) return;
            ItemsSource = new ObservableCollection<AvailableItemObj>(_ItemsSource);
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
                             x.ItemCode != null && x.ItemCode.ToLower().Contains(lowerCapv) ||
                             x.ItemName != null && x.ItemName.ToLower().Contains(lowerCapv)).ToList();

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

                ItemsSource = new ObservableCollection<AvailableItemObj>(filter);
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

        void HandleSelected(AvailableItemObj SelectedItem ) => Navigation.PushAsync(new TrackWarehouseV(selectedItem, SelectedWhs));
        async void ShowAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "ok");
    }
}
