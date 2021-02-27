using Rg.Plugins.Popup.Pages;
using System.Collections.Generic;
using WMSApp.Models.Share;
using WMSApp.ViewModels.Share;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Share
{
    /// <summary>
    /// A general view to handle bin 
    /// for batch and serial
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TransferBoItemSrBatView : ContentPage
    {
        public TransferBoItemSrBatView(List<CommonStockInfo> details, 
            string returnAddr, string itemCode, string warehouse, decimal neededQty, string pageTitle, string request)
        {
            InitializeComponent();
            BindingContext = new TransferBoBinSelectionVM(Navigation, details, 
                returnAddr, itemCode, warehouse, neededQty, pageTitle, request);
        }
    }
}