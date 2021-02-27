using System.Collections.Generic;
using WMSApp.ViewModels.Transfer1;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Transfer1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SerialListView : ContentPage
    {
        public SerialListView(string returnAddr, string itemCod, string itemNam, string whsCod, int ctrlLmt, List<string> selectedSerial)
        {
            InitializeComponent();
            BindingContext = new SerialListVM(Navigation, returnAddr, itemCod, itemNam, whsCod, ctrlLmt, selectedSerial);
        }
    }
}