using WMSApp.Models.GIGR;
using WMSApp.ViewModels.GIGR;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace WMSApp.Views.GIGR
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GoodsTransView : ContentPage
    {
        public GoodsTransView(string transType)
        {
            InitializeComponent();            
            BindingContext = new GoodsTransVM(transType, Navigation);
        }

        /// <summary>
        /// Prompt for remove the line from list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void RemoteItem_Invoked(object sender, System.EventArgs e)
        {
            SwipeItem selectedLine = (SwipeItem)sender;
            if (selectedLine == null) return;

            var line = (OITM_Ex)selectedLine.CommandParameter;

            if (line == null) return;
            bool confirmRemove = await DisplayAlert(
                $"Comfirm remove item {line.Item.ItemCode}?",
                $"{line.Item.ItemName}\nRemove this item will reset the line, " +
                $"and all pre-setted warehouse entry / exit will be clear?", "Remove", "Cancel");

            if (confirmRemove) ((GoodsTransVM)BindingContext).RemoveListItem(line);
        }

        // close for reserved and discussion later
        //async void ChangeWhs_Invoked(object sender, System.EventArgs e)
        //{
        //    SwipeItem selectedLine = (SwipeItem)sender;
        //    if (selectedLine == null) return;

        //    var line = (OITM_Ex)selectedLine.CommandParameter;

        //    if (line == null) return;
        //    bool confirmRemove = await DisplayAlert(
        //        $"Comfirm change item {line.Item.ItemCode} warehouse?",
        //        $"{line.Item.ItemName}\nChange this item line warehouse will reset the line, " +
        //        $"and all pre-setted warehouse entry / exit will be clear?", "Cancel", "Confirm");

        //    if (confirmRemove) ((GoodsTransVM)BindingContext).ChangeLineWhs(line);
        //}
    }
}