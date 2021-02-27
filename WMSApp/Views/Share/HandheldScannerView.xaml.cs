using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Share
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HandheldScannerView : ContentPage
    {
        string repliedMcAddress { get; set; } = string.Empty;
        string resultData { get; set; } = string.Empty;

        public string PageTitle { get; set; } = string.Empty;

        public HandheldScannerView(string repliedMc, string pageTitle)
        {            
            InitializeComponent();
            repliedMcAddress = repliedMc;
            PageTitle = pageTitle;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await Task.Delay(1);
            entryData.Focus();                
        }

        /// <summary>
        /// Handle the scanner capture of the data and go off
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void entryData_TextChanged(object sender, TextChangedEventArgs e)
        {
            Navigation.PopAsync();
            resultData = entryData.Text;
            resultData = resultData.Trim(); // rmeove all blank key
            MessagingCenter.Send<string>(resultData, repliedMcAddress);            
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            MessagingCenter.Send<string>("cancel", repliedMcAddress);
            Navigation.PopAsync();
        }

        private void btnCancel_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send<string>("cancel", repliedMcAddress);
            Navigation.PopAsync();
        }
    }
}