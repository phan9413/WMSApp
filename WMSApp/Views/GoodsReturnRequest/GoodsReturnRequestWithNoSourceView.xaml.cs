
using WMSApp.Models.SAP;
using WMSApp.ViewModels.GoodsReturnRequest;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.GoodsReturnRequest
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GoodsReturnRequestWithNoSourceView : ContentPage
    {
        public GoodsReturnRequestWithNoSourceView()
        {
            InitializeComponent();
            BindingContext = new GoodsReturnRequestWithNoSourceVM(Navigation);
        }

        /// <summary>
        /// handler selected line
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void removeSwipe_Invoked(object sender, System.EventArgs e)
        {
            var swipeItem = sender as SwipeItem;
            PDN1_Ex selectedLine = (PDN1_Ex)swipeItem.CommandParameter;
            if (selectedLine == null) return;

            bool comfirmedRemove = await DisplayAlert($"Confirm remove {selectedLine.ItemCodeDisplay}\n " +
                $"{selectedLine.ItemNameDisplay}?", "All saved data will not be recovered after remove.", "OK", "Cancel");

            if (comfirmedRemove)
            {
                ((GoodsReturnRequestWithNoSourceVM)BindingContext).RemoveLine(selectedLine);
            }
        }
    }
}