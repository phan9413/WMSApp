using System.Collections.Generic;
using WMSApp.Models.Share;
using WMSApp.ViewModels.Share;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


/// <summary>
///  Show received serial list
///  When user select the item, prompt lis of bin in to wharehouse selection
/// </summary>
namespace WMSApp.Views.Share
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SerialToBinSelectionView : ContentPage
    {
        public SerialToBinSelectionView(List<CommonStockInfo> selectedList, string toWhs, string returnAddress)
        {
            InitializeComponent();
            BindingContext = new SerialToBinSelectionVM(Navigation, selectedList, toWhs, returnAddress);
        }
    }
}