using System;
using System.Collections.Generic;
using WMSApp.ViewModels.Transfer1;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Transfer1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SerialToBinView : ContentPage
    {
        public SerialToBinView(
            Guid headGuid,
            Guid linGuid,
            string itemCod,
            string itemNam,
            string whsCode,
            string returnAddr)
        {
            InitializeComponent();
            BindingContext = new SerialToBinVM(Navigation,
                headGuid,
                linGuid,
                itemCod,
                itemNam,
                whsCode,
                returnAddr);
        }

        /// <summary>
        /// Handler GRPO serial to bin steps
        /// </summary>
        public SerialToBinView(List<string> serials,
            string itemCod,
            string itemNam,
            string whsCode,
            string returnAddr)
        {
            InitializeComponent();
            BindingContext = new SerialToBinVM(Navigation,
                serials,
               itemCod,
               itemNam,
               whsCode,
               returnAddr);
        }
    }
}