using DbClass;
using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Share
{
    public class PickWarehousePopUpVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public Command CmdSearchBarVisible { get; set; }

        OWHS selectedItem; 
        public OWHS SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
                
                MessagingCenter.Send(value, returnAddr);
                Navigation.PopPopupAsync(); // select and pop up the screen
            }
        }

        List<OWHS> itemsSource;
        public ObservableCollection<OWHS> ItemsSource { get; set; }
        
        public string SearchText
        {
            set => HandlerSearchText(value);
        }

        string title;
        public string Title
        {
            get => title;
            set
            {
                if (title == value) return;
                title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        string returnAddr;
        INavigation Navigation;

        Color headerColor;
        public Color HeaderColor
        {
            get => headerColor;
            set
            {
                if (headerColor == value) return;
                headerColor = value;
                OnPropertyChanged(nameof(HeaderColor));
            }
        }

        string ExcepWhs;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="callerAddress"></param>
        public PickWarehousePopUpVM (INavigation navigation, 
            string callerAddress, string title, Color headerColor, string excepWhs ="")
        {
            Navigation = navigation;
            returnAddr = callerAddress;
            Title = title;
            HeaderColor = headerColor;
            ExcepWhs = excepWhs;
            InitWhs();
            InitCmd();
        }

        void InitCmd()
        {
            CmdSearchBarVisible = new Command<SearchBar>((SearchBar sbar) =>
            {
                sbar.IsVisible = !sbar.IsVisible;
                if (sbar.IsVisible)
                {
                    sbar.Focus();
                    sbar.Placeholder = "Enter warehouse code to search";
                }
            });
        }

        void InitWhs ()
        {
            if (App.Warehouses == null) return;            
            if (!string.IsNullOrWhiteSpace(ExcepWhs))
            {
                itemsSource = new List<OWHS>(App.Warehouses.Where(x=> !x.WhsCode.Equals(ExcepWhs)).ToList());
            }
            else // select all whs
            {
                itemsSource = new List<OWHS>(App.Warehouses);
            }

            RefreshListView();
        }

        void RefreshListView()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<OWHS>(itemsSource);
        }

        void HandlerSearchText (string query)
        {
            try
            {
                if (itemsSource == null) return;

                if (string.IsNullOrWhiteSpace(query))
                {
                    RefreshListView();
                    return;
                }

                var filter = itemsSource.Where(x => x.WhsCode.Contains(query)).ToList();

                if (filter == null)
                {
                    RefreshListView();
                    return;
                }

                if (filter.Count == 0)
                {
                    RefreshListView();
                    return;
                }

                ItemsSource = new ObservableCollection<OWHS>(filter);
                OnPropertyChanged(nameof(ItemsSource));
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayMessage(excep.Message);
            }
        }

        async void DisplayMessage(string message) => await new Dialog().DisplayAlert("Alert", message, "Ok");
    }
}
