using WMSApp.Models.SAP;
using WMSApp.ViewModels.ReturnRequest;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.ReturnRequest
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReturnRequestWithNoSourceView : ContentPage
    {
        public ReturnRequestWithNoSourceView()
        {
            InitializeComponent();
            BindingContext = new ReturnRequestWithNoSourceVM(Navigation);
        }

        /// <summary>
        /// handler selected line
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void removeSwipe_Invoked(object sender, System.EventArgs e)
        {
            var swipeItem = sender as SwipeItem;
            DLN1_Ex selectedLine = (DLN1_Ex)swipeItem.CommandParameter;
            if (selectedLine == null) return;

            bool comfirmedRemove = await DisplayAlert($"Confirm remove {selectedLine.ItemCodeDisplay}\n " +
                $"{selectedLine.ItemNameDisplay}?", "All saved data will not be recovered after remove.", "OK", "Cancel");

            if (comfirmedRemove)
            {
                ((ReturnRequestWithNoSourceVM)BindingContext).RemoveLine(selectedLine);
            }
        }
    }
}