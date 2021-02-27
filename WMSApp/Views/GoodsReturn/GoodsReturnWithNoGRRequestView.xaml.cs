using System;
using WMSApp.Models.SAP;
using WMSApp.ViewModels.GoodsReturn;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.GoodsReturn
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GoodsReturnWithNoGRRequestView : ContentPage
    {
        public GoodsReturnWithNoGRRequestView()
        {
            InitializeComponent();
            BindingContext = new GoodsReturnWithNoGRRequestVM(Navigation);
        }

        /// <summary>
        /// handler selected line
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void removeSwipe_Invoked(object sender, System.EventArgs e)
        {
            var swipeItem = sender as SwipeItem;
            PRR1_Ex selectedLine = (PRR1_Ex)swipeItem.CommandParameter;
            if (selectedLine == null) return;

            bool comfirmedRemove = await DisplayAlert($"Confirm remove {selectedLine.ItemCodeDisplay}\n " +
                $"{selectedLine.ItemNameDisplay}?", "All saved data will not be recovered after remove.", "OK", "Cancel");

            if (comfirmedRemove)
            {
                ((GoodsReturnWithNoGRRequestVM)BindingContext).RemoveLine(selectedLine);
            }
        }
    }
}