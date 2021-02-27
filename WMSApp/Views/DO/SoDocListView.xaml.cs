using WMSApp.ViewModels.SalesOrder;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.DeliveryOrder
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SoDocListView : ContentPage
    {
        public SoDocListView(string pageType)
        {
            InitializeComponent();
            BindingContext = new SoDocListVM(Navigation, pageType);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ((SoDocListVM)BindingContext).LoadSOList();
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopAsync();
            return base.OnBackButtonPressed();
        }
    }
}