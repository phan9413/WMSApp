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
using WMSApp.ClassObj.Warehouse;
using WMSApp.Interface;
using Xamarin.Forms;

namespace WMSApp.ViewModel.Warehouse
{
    public class TrackWarehouseVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Command CmdMenuSelect { get; set; }
        public Command CmdVisibleSearchbar { get; set; }
        List<ItemObj> itemsSource { get; set; } = null;
        public ObservableCollection<ItemObj> ItemsSource { get; set; }
        public AvailableItemObj selectedItem { get; set; } = null;
        public OWHSObj selectedWarehouse { get; set; } = null;
        string listTitle { get; set; } = string.Empty;
        public string ListTitle
        {
            get => listTitle;
            set
            {
                if (listTitle != value)
                {
                    listTitle = value;
                    OnPropertyChanged(nameof(ListTitle));
                }
            }
        }

        bool isVisible;
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (isVisible != value)
                {
                    isVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
                }   
            }
        }

        bool isVisibleData;
        public bool IsVisibleData
        {
            get => isVisibleData;
            set
            {
                if (isVisibleData != value)
                {
                    isVisibleData = value;
                    OnPropertyChanged(nameof(IsVisibleData));
                }   
            }
        }

        bool isVisibleNoData;
        public bool IsVisibleNoData
        {
            get => isVisibleNoData;
            set
            {
                if (isVisibleNoData != value)
                {
                    isVisibleNoData = value;
                    OnPropertyChanged(nameof(IsVisibleNoData));
                }   
            }
        }

        bool isExpanded;
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }   
            }
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

        string searchText { get; set; } = string.Empty;
        public string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                HandleSearch(value);
            }
        }

        public TrackWarehouseVM(AvailableItemObj SelectedItem, OWHSObj SelectedWarehouse)
        {
            selectedItem = SelectedItem;
            selectedWarehouse = SelectedWarehouse;
            IsVisible = false;
            isExpanded = true;
            CmdMenuSelect = new Command<string>(MenuSelect);

            MenuSelect("1"); // intial load
            IsExpanded = true;
        }

        void MenuSelect(string param)
        {
            try
            {
                isExpanded = false;
                switch (param)
                {
                    case "1":
                        ListTitle = "Non Managed Item Quantity";
                        LoadInventoryData("QueryNonManageItem");
                        break;
                    case "2":
                        ListTitle = "Non Managed Item Quantity With Bin";
                        LoadInventoryData("QueryNonManageItemWithBin");
                        break;
                    case "3":
                        ListTitle = "Serial Item Quantity Without Bin";
                        LoadInventoryData("QuerySerialItemWithoutBin");
                        break;
                    case "4":
                        ListTitle = "Serial Item Quantity With Bin";
                        LoadInventoryData("QuerySerialItemWithBin");
                        break;
                    case "5":
                        ListTitle = "Batch Item Quantity Without Bin";
                        LoadInventoryData("QueryBatchItemWithoutBin");
                        break;
                    case "6":
                        ListTitle = "Batch Item Quantity With Bin";
                        LoadInventoryData("QueryBatchItemWithBin");
                        break;
                }
            }
            catch (Exception e)
            {
                LogException(e);
            }
        }

        async void LoadInventoryData(string request)
        {
            try
            {
                dynamic _request = new ExpandoObject();
                _request.request = request;
                _request.SelectedItem = selectedItem.ItemCode;
                _request.SelectedWarehouse = selectedWarehouse.WhsCode;

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

                var dtoWhs = JsonConvert.DeserializeObject<ItemDTO>(content);

                if (dtoWhs == null)
                {
                    ShowAlert("Please make sure your Content is not empty.");
                    return;
                }

                if (dtoWhs.ItemWhsResult == null)
                {
                    ShowAlert("Error reading info from server, Pls try again later, Thanks.");
                    return;
                }

                itemsSource = new List<ItemObj>(dtoWhs.ItemWhsResult);
                RefreshListview();

                if (itemsSource.Count == 0)
                {
                    IsVisibleData = false;
                    IsVisibleNoData = true;
                    ShowToast($"No Data for {ListTitle}");
                }
                else
                {
                    IsVisible = true;
                    IsVisibleNoData = false;
                    IsVisibleData = true;
                }
            }
            catch (Exception e)
            {
                LogException(e);
            }
        }

        void RefreshListview()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<ItemObj>(itemsSource);
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
                var filter = itemsSource
                    .Where(
                        x =>
                            x.WhsCode != null && x.WhsCode.Contains(value) ||
                            x.BinCode != null && x.BinCode.Contains(value) ||
                            x.DistNumber != null && x.DistNumber.Contains(value)
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

                ItemsSource = new ObservableCollection<ItemObj>(filter);
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

        void ShowToast(string message) => DependencyService.Get<IToastMessage>()?.ShortAlert(message);

        async void ShowAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "ok");
    }
}
