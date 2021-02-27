using WMSApp.Models.SAP;
using WMSApp.ViewModels.Transfer;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Transfer
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectBinListView : ContentPage
    {
        /// <summary>
        /// no reference and to be remove later
        /// </summary>
        /// <param name="selectedLine"></param>
        /// <param name="returnAddr"></param>
        /// <param name="title"></param>
        public SelectBinListView(WTQ1_Ex selectedLine, string returnAddr,string title)
        {
            InitializeComponent();
            BindingContext = new SelectBinListVM(Navigation, selectedLine, returnAddr, title);
        }
    }
}