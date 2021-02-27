using WMSApp.ViewModels.ReturnRequest;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.ReturnRequest
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReturnListView : ContentPage
    {
        public ReturnListView(string pageType)
        {
            InitializeComponent();
            BindingContext = new ReturnListVM(Navigation,pageType);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ((ReturnListVM)BindingContext).LoadTransferRequest();
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopAsync();
            return base.OnBackButtonPressed();
        }
    }
}