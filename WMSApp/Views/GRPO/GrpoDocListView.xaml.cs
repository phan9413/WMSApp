using WMSApp.Models.GRPO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.GRPO
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GrpoDocListView : ContentPage
    {
        public GrpoDocListView(string pageType)
        {
            InitializeComponent();
            BindingContext = new GrpoDocListVM(Navigation, pageType);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ((GrpoDocListVM)BindingContext).LoadPOList();
        }
        protected override bool OnBackButtonPressed()
        {
            Navigation.PopAsync();
            return base.OnBackButtonPressed();            
        }
    }
}
