using System.Collections.Generic;
using WMSApp.Models.SAP;
using WMSApp.ViewModels.SalesOrder;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.DeliveryOrder
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SoItemsView : ContentPage
    {
        public SoItemsView(List<ORDR_Ex> selected)
        {
            InitializeComponent();
            BindingContext = new SoItemsVM(Navigation, selected);
        }

        void DocPropExpander_Tapped(object sender, System.EventArgs e) => SODetailsExpander.IsExpanded = false;

        void SODetailsExpander_Tapped(object sender, System.EventArgs e) => DocPropExpander.IsExpanded = false;

        void LineChangeWhs_Invoked(object sender, System.EventArgs e)
        {
            SwipeItem poLine = (SwipeItem)sender;
            if (poLine == null) return;
            RDR1_Ex selectedLine = (RDR1_Ex)poLine.CommandParameter;
            if (selectedLine == null) return;

            ((SoItemsVM)BindingContext).HandlerLineWhsChanged(selectedLine);
        }
    }
}