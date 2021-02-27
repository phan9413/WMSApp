using DbClass;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using WMSApp.Models.Share;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Share
{
    public class SerialToBinSelectionVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string pName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pName));

        CommonStockInfo currentItem;
        CommonStockInfo selectedItem;
        public CommonStockInfo SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;
                currentItem = value;
                // Handler selected Item
                // show list on bin for item to entry in
                LaunchBinSelection();

                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        List<CommonStockInfo> itemsSource;
        public ObservableCollection<CommonStockInfo> ItemsSource { get; set; }

        INavigation Navigation;
        string ReturnAddr;
        string ToWhs;

        /// <summary>
        /// Constructor
        /// </summary>
        public SerialToBinSelectionVM (INavigation navigation, List<CommonStockInfo> selectedList, string toWhs, string returnAddress)
        {
            ReturnAddr = returnAddress;
            ToWhs = toWhs;
            Navigation = navigation;
            itemsSource = selectedList;
            RefreshListView();
        }

        /// <summary>
        /// Refresh the listview binding
        /// </summary>
        void RefreshListView ()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<CommonStockInfo>(itemsSource);
            OnPropertyChanged(nameof(itemsSource));
        }

        /// <summary>
        /// Show a pop up screen to select the bin
        /// </summary>
        void LaunchBinSelection()
        {
            var returnAddress = "20201004T1000_LaunchBinSelection";
            MessagingCenter.Subscribe<OBIN>(this, returnAddress, (OBIN selectedBin) =>
            { 
                int locid = itemsSource.IndexOf(currentItem);
                if (locid >= 0)
                {
                    itemsSource[locid].ToWhs = selectedBin.WhsCode;
                    itemsSource[locid].ToBinAbs = selectedBin.AbsEntry;
                    itemsSource[locid].ToBinCode = selectedBin.BinCode;
                }
                return;
            });

           // Navigation.PushAsync(new BinSelectionPopUpView(currentItem.DistNumber, ));

        }
    }
}
