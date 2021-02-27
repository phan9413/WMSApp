using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMSApp.ViewModels.Share;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Share
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TransferBoItemSrBatBinView : ContentPage
    {
        public TransferBoItemSrBatBinView()
        {
            InitializeComponent();

            //BindingContext = new TransferBoItemSrBatBinVM();
        }
    }
}