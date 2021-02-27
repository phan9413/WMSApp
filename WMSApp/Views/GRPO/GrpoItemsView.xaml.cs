using System.Collections.Generic;
using WMSApp.Models.SAP;
using WMSApp.ViewModels.GRPO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.GRPO
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GrpoItemsView : ContentPage
    {
        public GrpoItemsView(List<OPOR_Ex> selecteds)
        {
            InitializeComponent();
            BindingContext = new GrpoItemsVM(Navigation, selecteds);
        }

        void DocPropExpander_Tapped(object sender, System.EventArgs e) => PODetailsExpander.IsExpanded = false;        

        void PODetailsExpander_Tapped(object sender, System.EventArgs e) => DocPropExpander.IsExpanded = false;
        
        void LineChangeWhs_Invoked(object sender, System.EventArgs e)
        {
            SwipeItem poLine = (SwipeItem)sender;
            if (poLine == null) return;
            POR1_Ex selectedLine = (POR1_Ex)poLine.CommandParameter;
            if (selectedLine == null) return;

            ((GrpoItemsVM)BindingContext).HandlerLineWhsChanged(selectedLine);
        }
    }
}