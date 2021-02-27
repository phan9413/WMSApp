using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.ViewModels.Transfer1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemToBinView : ContentPage
    {
        /// <summary>
        /// For item from bin selection 
        /// </summary>
        public ItemToBinView(
            Guid headGuid,
            Guid linGuid,
            string itemCod,
            string itemNam,
            string whsCode,
            string returnAddr)
        {
            InitializeComponent();
            BindingContext = new ItemToBinVM(Navigation, headGuid,
            linGuid,
            itemCod,
            itemNam,
            whsCode,
            returnAddr);
        }

        /// <summary>
        /// Handler item to bin steps
        /// </summary>
        public ItemToBinView(
            string itemCod,
            string itemNam,
            string whsCode,
            string returnAddr)
        {
            InitializeComponent();
            BindingContext = new ItemToBinVM(Navigation,
               itemCod,
               itemNam,
               whsCode,
               returnAddr);
        }

    }
}