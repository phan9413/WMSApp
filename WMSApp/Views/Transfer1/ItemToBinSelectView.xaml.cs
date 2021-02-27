using System.Collections.Generic;
using WMSApp.ViewModels.Transfer1;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Transfer1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemToBinSelectView : ContentPage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="whsCode"></param>
        /// <param name="retAddrs"></param>
        /// <param name="itemCode"></param>
        /// <param name="itemName"></param>
        /// <param name="itemQty"></param>
        /// <param name="fromBinList"></param>
        public ItemToBinSelectView(
            string whsCode, 
            string retAddrs, 
            string itemCode, 
            string itemName, 
            decimal itemQty, 
            List<string> fromBinList)
        {
            InitializeComponent();
            BindingContext = new ItemToBinSelectVM(Navigation, whsCode, retAddrs, itemCode, itemName, itemQty, fromBinList);
        }
    }
}