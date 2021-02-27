using WMSApp.ViewModels.ReturnRequest;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.ReturnRequest
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReturnRequestListView : ContentPage
    {
        public ReturnRequestListView(string pageType)
        {
            InitializeComponent();
            BindingContext = new ReturnRequestListVM(Navigation, pageType);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ((ReturnRequestListVM)BindingContext).LoadDocument();
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopAsync();
            return base.OnBackButtonPressed();
        }

    }
}