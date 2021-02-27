using System;
using WMSApp.ViewModels.Transfer1;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Transfer1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BinItemListView : ContentPage
    {
        public BinItemListView(string returnAddr, string itemCod, string itemNam, string whsCode, decimal neededQty)
        {
            InitializeComponent();
            BindingContext = new BinItemListVM(Navigation, returnAddr, itemCod, itemNam, whsCode, neededQty);
        }
    }
}