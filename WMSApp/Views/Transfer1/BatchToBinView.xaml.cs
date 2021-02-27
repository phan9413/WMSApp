using System;
using System.Collections.Generic;
using WMSApp.Models.GRPO;
using WMSApp.ViewModels.Transfer1;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Transfer1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BatchToBinView : ContentPage
    {
        /// <summary>
        /// TransferLine
        /// </summary>
        /// <param name="docEntry"></param>
        /// <param name="whsCod"></param>
        /// <param name="returnAddrs"></param>
        public BatchToBinView(Guid head, Guid line, string whsCod, string returnAddrs, string itemCode, string itemName)
        {
            InitializeComponent();
            BindingContext = new BatchToBinVM(Navigation, head, line, itemCode, itemName, whsCod, returnAddrs);
        }

        /// <summary>
        /// Handler the GRPO batch entry to bin
        /// </summary>
        /// <param name="batches"></param>
        /// <param name="whsCod"></param>
        /// <param name="returnAddrs"></param>
        /// <param name="itemCode"></param>
        /// <param name="itemName"></param>
        public BatchToBinView(List<Batch> batches, string whsCod, string returnAddrs, string itemCode, string itemName)
        {
            InitializeComponent();
            BindingContext = new BatchToBinVM(Navigation, batches, itemCode, itemName, whsCod, returnAddrs);
        }

    }
}