using System.Collections.Generic;
using WMSApp.Models.SAP;
using WMSApp.ViewModels.GoodsReturnRequest;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.GoodsReturnRequest
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GoodsReturnRequestItemsView : ContentPage
    {
        public GoodsReturnRequestItemsView(List<ReturnCommonHead_Ex> selectedDocs, string sourceType)
        {
            InitializeComponent();
            BindingContext = new GoodsReturnRequestItemsVM(Navigation, selectedDocs, sourceType);
        }

        void DocPropExpander_Tapped(object sender, System.EventArgs e) => RetDetailsExpander.IsExpanded = false;

        void RetDetailsExpander_Tapped(object sender, System.EventArgs e) => DocPropExpander.IsExpanded = false;

        void LineChangeWhs_Invoked(object sender, System.EventArgs e)
        {
            SwipeItem poLine = (SwipeItem)sender;
            if (poLine == null) return;
            ReturnCommonLine_Ex selectedLine = (ReturnCommonLine_Ex)poLine.CommandParameter;
            if (selectedLine == null) return;

            ((GoodsReturnRequestItemsVM)BindingContext).HandlerLineWhsChanged(selectedLine);
        }
    }
}