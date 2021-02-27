using WMSApp.ClassObj.Item;
using WMSApp.ViewModel.Item;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.View.Item
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TrackItemDetailsV : ContentPage
    {
        public TrackItemDetailsV(OITMObj SelectedItem)
        {
            InitializeComponent();
            BindingContext = new TrackItemDetailsVM(SelectedItem);
        }
    }
}