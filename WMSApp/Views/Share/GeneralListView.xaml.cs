using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Share
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GeneralListView : ContentPage
    {
        public string PageTitle { get; set; }
        string selectedItem;
        public string SelectedItem
        {
            get => selectedItem; 
            set
            {
                if(selectedItem != value)
                {
                    selectedItem = value;
                    HandlerSelectedItem(selectedItem);
                    selectedItem = null;
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        string[] itemsSource;
        public ObservableCollection<string> ItemsSource { get; set; }

        string searchText;
        public string SearchText
        {
            get => searchText; 
            set
            {
                if(searchText != value)
                {
                    searchText = value;
                    QuerySearch(searchText);
                    OnPropertyChanged(nameof(SearchText));
                }
            }
        }

        string ReturnAddress; // use to hold the retuurn address of the caller

        /// <summary>
        /// The page construtor
        /// </summary>
        /// <param name="returnAddress"></param>
        /// <param name="source"></param>
        public GeneralListView( string pageTitle , string returnAddress, string [] source)
        {
            InitializeComponent();
            PageTitle = pageTitle; 
            ReturnAddress = returnAddress;
            itemsSource = source;

            if (itemsSource != null && itemsSource.Length >0)
            {
                ItemsSource = new ObservableCollection<string>(itemsSource);
            }

            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Thread.Sleep(360);
            QuerySearchBar.Focus();
        }

        /// <summary>
        /// Handler the selected item
        /// When the item is not empty string send in the messaging center and close the screen
        /// </summary>
        /// <param name="selected"></param>
        void HandlerSelectedItem (string selected)
        {
            if (string.IsNullOrWhiteSpace(selected))
            {
                return;
            }

            MessagingCenter.Send(selected, ReturnAddress);
            Navigation.PopAsync();
        }

        /// <summary>
        /// Perform a text search to faster the search and UI
        /// </summary>
        /// <param name="query"></param>
        void QuerySearch (string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                RefreshList();
                return;
            }

            var serachText = query.ToUpper();
            var filterList = itemsSource.Where(x => x.Contains(serachText)).ToArray();
            if (filterList == null)
            {
                RefreshList();
                return;
            }

            if (filterList.Length == 0)
            {
                RefreshList();
                return;
            }

            // refresh the filter list
            ItemsSource = new ObservableCollection<string>(filterList);
            OnPropertyChanged(nameof(ItemsSource));
        }
    

        /// <summary>
        /// Refresh the list on user ui 
        /// </summary>
        void RefreshList ()
        {
            if (itemsSource == null) return;
            if (itemsSource.Length == 0) return;

            ItemsSource = new ObservableCollection<string>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }
    }
}