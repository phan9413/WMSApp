using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using WMSApp.Models.SAP;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Return
{
    public class ReturnItemLinePopUpVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        List<ReturnCommonLine_Ex> itemsSource;
        public ObservableCollection<ReturnCommonLine_Ex> ItemsSource { get; set; }
        public string CardName { get; set; }
        public string CardCode { get; set; }
        public int DocNum { get; set; }
        public DateTime DocDate { get; set; }

        ReturnCommonLine_Ex selectedItem { get; set; }
        public ReturnCommonLine_Ex SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;
                MessagingCenter.Send(selectedItem, CallerAddress);
                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
                Navigation.PopPopupAsync(); //<-- cllose the screen
            }
        }

        INavigation Navigation { get; set; } = null;
        string CallerAddress { get; set; } = string.Empty;
        ReturnCommonHead_Ex selectedDoc { get; set; } = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="navigation"></param>
        /// <param name="doc"></param>
        /// <param name="lines"></param>
        /// <param name="callerAddrs"></param>
        public ReturnItemLinePopUpVM(INavigation navigation, 
            ReturnCommonHead_Ex doc, 
            List<ReturnCommonLine_Ex> lines, 
            string callerAddrs)
        {
            Navigation = navigation;
            CallerAddress = callerAddrs;
            selectedDoc = doc;
            itemsSource = lines;

            CardName = selectedDoc.CardName;
            CardCode = selectedDoc.CardCode;
            DocNum = selectedDoc.DocNum;
            DocDate = selectedDoc.DocDate;
            RefreshListView();
        }

        void RefreshListView()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<ReturnCommonLine_Ex>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// proper handler the close screen
        /// </summary>
        public void Close()
        {
            MessagingCenter.Send(new ReturnCommonLine_Ex(), CallerAddress);
            Navigation.PopPopupAsync();
        }
    }
}

