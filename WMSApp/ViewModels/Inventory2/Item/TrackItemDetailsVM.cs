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
using WMSApp.Interface;
using Xamarin.Forms;

namespace WMSApp.ViewModel.Item
{
    public class TrackItemDetailsVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Command CmdMenuSelect { get; set; }
        public Command CmdVisibleSearchbar { get; set; }        
        public OITMObj selectedItem { get; set; } = null;
        List<ItemObj> itemsSource { get; set; } = null;
        public ObservableCollection<ItemObj> ItemsSource { get; set; }
        string listTitle { get; set; } = string.Empty;
        public string ListTitle
        {
            get => listTitle;
            set
            {
                if (listTitle == value) return;
                listTitle = value;
                OnPropertyChanged(nameof(ListTitle));
            }
        }



        bool isVisible { get; set; } = false;
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (isVisible == value) return;
                isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        bool isVisibleData { get; set; } = false;
        public bool IsVisibleData
        {
            get => isVisibleData;
            set
            {
                if (isVisibleData == value) return;
                isVisibleData = value;
                OnPropertyChanged(nameof(IsVisibleData));
            }
        }

        bool isVisibleNoData { get; set; } = false;
        public bool IsVisibleNoData
        {
            get => isVisibleNoData;
            set
            {
                if (isVisibleNoData == value) return;
                isVisibleNoData = value;
                OnPropertyChanged(nameof(IsVisibleNoData));
            }
        }

        bool isExpanded { get; set; } = true;
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (isExpanded == value) return;
                isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
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

        public string SearchText
        {
            set => HandleSearch(value);
        }
        
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="SelectedItem"></param>
        public TrackItemDetailsVM(OITMObj SelectedItem)
        {
            
            selectedItem = SelectedItem;
            CmdMenuSelect = new Command<string>(MenuSelect);
            CmdVisibleSearchbar = new Command<SearchBar>((SearchBar searchbar) =>
            {
                
                searchbar.IsVisible = !searchbar.IsVisible;
                if (searchbar.IsVisible) searchbar.Focus();
                IsSearchBarVisible = searchbar.IsVisible;
                IsExpanded = !searchbar.IsVisible;
            });

            MenuSelect("1");
            IsExpanded = true;
        }

        void MenuSelect(string param)
        {            
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

        async void LoadInventoryData(string request)
        {           
            dynamic _request = new ExpandoObject();
            _request.request = request;
            _request.SelectedItem = selectedItem.ItemCode;

            string content, lastErrMessage;
            bool isSuccess = false;
            using (var httpClient = new HttpClientWapi())
            {
                content = await httpClient.RequestSvrAsync_NoAuthn(_request, "ItemsRequest");
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
                ShowToast($"No Data {ListTitle}");
                return;
            }
            
            IsVisible = true;
            IsVisibleNoData = false;
            IsVisibleData = true;
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

        async void ShowAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "ok");
        void ShowToast(string message) => DependencyService.Get<IToastMessage>().ShortAlert(message);
    }
}
