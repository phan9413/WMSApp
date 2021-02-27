using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMSApp.Models.SAP;
using WMSApp.ViewModels.Return;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Return
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReturnWithNoDocSourceView : ContentPage
    {
        public ReturnWithNoDocSourceView()
        {
            InitializeComponent();
            BindingContext = new ReturnWithNoDocSourceVM(Navigation);
        }

        /// <summary>
        /// handler selected line
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void removeSwipe_Invoked(object sender, System.EventArgs e)
        {
            var swipeItem = sender as SwipeItem;
            ReturnCommonLine_Ex selectedLine = (ReturnCommonLine_Ex)swipeItem.CommandParameter;
            if (selectedLine == null) return;

            bool comfirmedRemove = await DisplayAlert($"Confirm remove {selectedLine.ItemCodeDisplay}\n " +
                $"{selectedLine.ItemNameDisplay}?", "All saved data will not be recovered after remove.", "OK", "Cancel");

            if (comfirmedRemove)
            {
                ((ReturnWithNoDocSourceVM)BindingContext).RemoveLine(selectedLine);
            }
        }
    }
}