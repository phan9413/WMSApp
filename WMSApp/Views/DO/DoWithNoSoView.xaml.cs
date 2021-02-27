using WMSApp.Models.SAP;
using WMSApp.ViewModels.DO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.DO
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DoWithNoSoView : ContentPage
    {
        public DoWithNoSoView()
        {
            InitializeComponent();
            BindingContext = new DoWithNoSoVM(Navigation);
        }

        /// <summary>
        /// handler selected line
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void removeSwipe_Invoked(object sender, System.EventArgs e)
        {
            var swipeItem = sender as SwipeItem;
            RDR1_Ex selectedLine = (RDR1_Ex)swipeItem.CommandParameter;
            if (selectedLine == null) return;

            bool comfirmedRemove = await DisplayAlert($"Confirm remove {selectedLine.ItemCodeDisplay}\n " +
                $"{selectedLine.ItemNameDisplay}?", "All saved data will not be recovered after remove.", "OK", "Cancel");

            if (comfirmedRemove)
            {
                ((DoWithNoSoVM)BindingContext).RemoveLine(selectedLine);
            }
        }

    }
}