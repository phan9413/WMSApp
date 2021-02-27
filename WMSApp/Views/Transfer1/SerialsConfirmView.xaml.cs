using System;
using System.Collections.Generic;
using WMSApp.ViewModels.Transfer1;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Transfer1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SerialsConfirmView : ContentPage
    {
        public SerialsConfirmView(Guid head, Guid line, string itemCod, string itemNam, string whsCod, string returnAddr)
        {
            InitializeComponent();
            BindingContext = new SerialsConfirmVM(Navigation, head, line, itemCod, itemNam, whsCod, returnAddr);

        }

        /// <summary>
        /// for selection of the serial to return to the selected whs
        /// </summary>
        /// <param name="serials"></param>
        /// <param name="itemCod"></param>
        /// <param name="itemNam"></param>
        /// <param name="whsCod"></param>
        /// <param name="returnAddr"></param>
        public SerialsConfirmView(List<string> serials, string itemCod, string itemNam, string whsCod, string returnAddr)
        {
            InitializeComponent();
            BindingContext = new SerialsConfirmVM(Navigation, serials, itemCod, itemNam, whsCod, returnAddr);
        }
    }
}