using System.Collections.Generic;
using WMSApp.ViewModels.Transfer1;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Transfer1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BinSerialListView : ContentPage
    {
        public BinSerialListView(
            string returnAddress, 
            string itemCod, 
            string itemNam, 
            string WhsCod, 
            int neededQty, 
            List<string> selectedSerials)
        {
            InitializeComponent();
            BindingContext = new BinSerialListVM(
                Navigation, 
                returnAddress, 
                itemCod, 
                itemNam, 
                WhsCod, 
                neededQty, 
                selectedSerials);
        }
    }
}