using System.Collections.Generic;
using WMSApp.Models.SAP;
using WMSApp.ViewModels.ReturnRequest;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.ReturnRequest
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReturnRequestItemView : ContentPage
    {
        public ReturnRequestItemView(List<ReturnCommonHead_Ex> selectedDoc, string sourceDocType )
        {
            InitializeComponent();
            BindingContext = new ReturnRequestItemVM(Navigation, selectedDoc, sourceDocType);
        }

        void DocPropExpander_Tapped(object sender, System.EventArgs e) => RetDetailsExpander.IsExpanded = false;

        void RetDetailsExpander_Tapped(object sender, System.EventArgs e) => DocPropExpander.IsExpanded = false;

        void LineChangeWhs_Invoked(object sender, System.EventArgs e)
        {
            SwipeItem line = (SwipeItem)sender;
            if (line == null) return;
            ReturnCommonLine_Ex selectedLine = (ReturnCommonLine_Ex)line.CommandParameter;
            if (selectedLine == null) return;

            ((ReturnRequestItemVM)BindingContext).HandlerLineWhsChanged(selectedLine);
        }
    }
}