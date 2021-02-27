using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using WMSApp.Models.SAP;
using Xamarin.Forms;

namespace WMSApp.ViewModels.DO
{
    public class DoSOLinePopUpVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        List<RDR1_Ex> itemsSource;
        public ObservableCollection<RDR1_Ex> ItemsSource { get; set; }
        public string CardName { get; set; }
        public string CardCode { get; set; }
        public int DocNum { get; set; }
        public DateTime DocDate { get; set; }

        RDR1_Ex selectedItem { get; set; }
        public RDR1_Ex SelectedItem
        {
            get => selectedItem;
            set
            {
                //if (selectedItem == value) return;
                //selectedItem = value;

                //selectedItem = null;
                //OnPropertyChanged(nameof(SelectedItem));

                //MessagingCenter.Send(selectedItem, CallerAddress);
                //Navigation.PopPopupAsync();

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
        ORDR_Ex selectedDoc { get; set; } = null;

        public DoSOLinePopUpVM(INavigation navigation, ORDR_Ex doc, List<RDR1_Ex> soLines, string callerAddrs)
        {
            Navigation = navigation;
            CallerAddress = callerAddrs;
            selectedDoc = doc;
            itemsSource = soLines;

            CardName = selectedDoc.CardName;
            CardCode = selectedDoc.CardCode;
            DocNum = selectedDoc.DocNum;
            DocDate = selectedDoc.DocDate;
            RefreshListView();
        }

        void RefreshListView()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<RDR1_Ex>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// proper handler the close screen
        /// </summary>
        public void Close ()
        {
            MessagingCenter.Send(new ORDR_Ex(), CallerAddress);
            Navigation.PopPopupAsync();
        }
    }
}
