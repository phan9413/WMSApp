using WMSApp.ViewModels.GoodsReturn;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.GoodsReturn
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GoodsReturnListView : ContentPage
    {
        public GoodsReturnListView(string pageType)
        {
            InitializeComponent();
            BindingContext = new GoodsReturnListVM(Navigation, pageType);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ((GoodsReturnListVM)BindingContext).LoadDocuments();
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopAsync();
            return base.OnBackButtonPressed();
        }
    }
}