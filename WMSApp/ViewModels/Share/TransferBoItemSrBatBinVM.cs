using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using WMSApp.Models.Share;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Share
{
    public class TransferBoItemSrBatBinVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        string pageTitle;
        public string PageTitle
        {
            get => pageTitle;
            set
            {
                if (pageTitle == value) return;
                pageTitle = value;
                OnPropertyChanged(nameof(PageTitle));
            }
        }

        string itemName;
        public string ItemName
        {
            get => itemName;
            set
            {
                if (itemName == value) return;
                itemName = value;
                OnPropertyChanged(nameof(ItemName));
            }
        }

        string itemCode;
        public string ItemCode
        {
            get => itemCode;
            set
            {
                if (itemCode == value) return;
                itemCode = value;
                OnPropertyChanged(nameof(ItemCode));
            }
        }

        CommonStockInfo selectedItem;
        public CommonStockInfo SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        List<CommonStockInfo> itemsSource;
        public ObservableCollection<CommonStockInfo> ItemsSource {get; set;}

        INavigation Navigation;
        string returnAddress = string.Empty;

        /// <summary>
        /// The constructor
        /// </summary>
        public TransferBoItemSrBatBinVM (INavigation navigation, 
            List<CommonStockInfo> details, string returnAddrs, string pageTitle)
        {
            Navigation = navigation;
            returnAddress = returnAddrs;
            itemsSource = details;
            PageTitle = pageTitle;

            RefreshList();
            Init();
        }

        /// <summary>
        /// Initial the object
        /// </summary>
        void Init()
        {
            if (itemsSource == null) return;
            if (itemsSource.Count == 0) return;

            ItemCode = itemsSource[0].ItemCode;
            ItemName = itemsSource[0].itemName;
        }

        /// <summary>
        /// Refresh the list of the screen
        /// </summary>
        void RefreshList()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<CommonStockInfo>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }
    }
}
