
using System;
using System.Collections.Generic;
using WMSApp.Models.GRPO;
using WMSApp.ViewModels.Transfer1;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Transfer1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BatchesConfirmView : ContentPage
    {
        // TransferLine
        public BatchesConfirmView(Guid head, Guid line, string whsCod, string returnAddr)
        {
            InitializeComponent();
            BindingContext = new BatchesConfirmVM(Navigation, head, line, whsCod, returnAddr);            
        }

        // grpo 
        public BatchesConfirmView(string whsCod, string returnAddr, List<Batch> batches, string itemCode, string itemName)
        {
            InitializeComponent();
            BindingContext = new BatchesConfirmVM(Navigation,whsCod, returnAddr, batches, itemCode, itemName);
        }
    }
}