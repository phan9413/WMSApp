using System.Collections.Generic;
using WMSApp.Models.SAP;
using WMSApp.ViewModels.Return;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Return
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReturnItemsView : ContentPage
    {
        public ReturnItemsView(List<ReturnCommonHead_Ex> selectedDocs, string sourceType)
        {
            InitializeComponent();
            BindingContext = new ReturnItemsVM(Navigation, selectedDocs, sourceType);
        }

        void DocPropExpander_Tapped(object sender, System.EventArgs e) => RetDetailsExpander.IsExpanded = false;

        void RetDetailsExpander_Tapped(object sender, System.EventArgs e) => DocPropExpander.IsExpanded = false;

        void LineChangeWhs_Invoked(object sender, System.EventArgs e)
        {
            SwipeItem poLine = (SwipeItem)sender;
            if (poLine == null) return;
            ReturnCommonLine_Ex  selectedLine = (ReturnCommonLine_Ex)poLine.CommandParameter;
            if (selectedLine == null) return;

            ((ReturnItemsVM)BindingContext).HandlerLineWhsChanged(selectedLine);
        }
    }
}