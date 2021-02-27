using WMSApp.Models.SAP;
using WMSApp.ViewModels.GRPO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.GRPO
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GrpoWithNoPoView : ContentPage
    {
        public GrpoWithNoPoView()
        {
            InitializeComponent();
            BindingContext = new GrpoWithNoPoVM(Navigation);
        }

        /// <summary>
        /// handler selected line
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void removeSwipe_Invoked(object sender, System.EventArgs e)
        {
            var swipeItem = sender as SwipeItem;
            POR1_Ex selectedLine = (POR1_Ex) swipeItem.CommandParameter;

            if (selectedLine == null) return;

            bool comfirmedRemove = await DisplayAlert($"Confirm remove {selectedLine.ItemCodeDisplay}\n " +
                $"{selectedLine.ItemNameDisplay}?", "All saved data will not be recovered after remove.", "OK", "Cancel");

            if (comfirmedRemove)
            {
                ((GrpoWithNoPoVM)BindingContext).RemoveLine(selectedLine);
            }
        }
    }
}