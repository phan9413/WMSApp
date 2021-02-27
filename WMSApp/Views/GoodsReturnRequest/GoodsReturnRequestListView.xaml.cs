
using WMSApp.ViewModels.GoodsReturnRequest;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.GoodsReturnRequest
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GoodsReturnRequestListView : ContentPage
    {
        public GoodsReturnRequestListView(string pageType)
        {
            InitializeComponent();
            BindingContext = new GoodsReturnRequestListVM(Navigation, pageType);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ((GoodsReturnRequestListVM)BindingContext).LoadDocuments();
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopAsync();
            return base.OnBackButtonPressed();
        }
    }
}