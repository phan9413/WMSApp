using SignaturePad.Forms;
using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Share
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CaptureSignatureView : ContentPage
    {
        string returnAddress { get; set; }
        public CaptureSignatureView(string callerAddress)
        {
            InitializeComponent();
            returnAddress = callerAddress;
        }

        /// <summary>
        /// return the strem from the signature and process by the caller
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async private void SignatureDone_Tapped(object sender, EventArgs e)
        {
            try
            {
                var bitmap = await signatureView.GetImageStreamAsync(SignatureImageFormat.Png);
                MessagingCenter.Send<Stream>(bitmap, returnAddress);
                await Navigation.PopAsync();
            }
            catch (Exception excep)
            {
                await DisplayAlert("Alert", excep.Message, "OK");
            }   
        }
    }
}