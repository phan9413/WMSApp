using System;
using System.Collections.Generic;
using WMSApp.Models.SAP;
using WMSApp.ViewModels.GoodsReturn;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.GoodsReturn
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GoodsReturnItemsView : ContentPage
    {
        public GoodsReturnItemsView(List<ReturnCommonHead_Ex> requestDocs, string sourceType)
        {
            InitializeComponent();
            BindingContext = new GoodsReturnItemsVM(Navigation, requestDocs, sourceType);
        }

        void DocPropExpander_Tapped(object sender, System.EventArgs e) => RetDetailsExpander.IsExpanded = false;

        void RetDetailsExpander_Tapped(object sender, System.EventArgs e) => DocPropExpander.IsExpanded = false;

        void LineChangeWhs_Invoked(object sender, System.EventArgs e)
        {
            SwipeItem poLine = (SwipeItem)sender;
            if (poLine == null) return;
            ReturnCommonLine_Ex selectedLine = (ReturnCommonLine_Ex)poLine.CommandParameter;
            if (selectedLine == null) return;

            ((GoodsReturnItemsVM)BindingContext).HandlerLineWhsChanged(selectedLine);
        }
    }
}