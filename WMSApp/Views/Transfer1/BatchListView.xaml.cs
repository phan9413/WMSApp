using System.Collections.Generic;
using WMSApp.Models.GRPO;
using WMSApp.ViewModels.Transfer1;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Transfer1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BatchListView : ContentPage
    {
        public BatchListView(string returnAddr, string itemCod, string itemNam, string whsCod, decimal ctrlLmt)
        {
            InitializeComponent();
            BindingContext = new BatchListVM(Navigation, returnAddr, itemCod, itemNam, whsCod, ctrlLmt);
        }

        //public BatchListView(string returnAddr, string itemCod, string itemNam, string whsCod, decimal ctrlLmt, List<Batch> batches)
        //{
        //    InitializeComponent();
        //    BindingContext = new BatchListVM(Navigation, returnAddr, itemCod, itemNam, whsCod, ctrlLmt);
        //}
    }
}