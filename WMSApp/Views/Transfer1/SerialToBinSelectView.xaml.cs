using System.Collections.Generic;
using WMSApp.Class;
using WMSApp.ViewModels.Transfer1;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Transfer1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SerialToBinSelectView : ContentPage
    {
        public SerialToBinSelectView(string whsCode, string returnAddrs, List<zwaTransferDocDetailsBin> serials )
        {
            InitializeComponent();
            BindingContext = new SerialToBinSelectVM(Navigation, whsCode, returnAddrs, serials);
        }
    }
}