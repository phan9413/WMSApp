using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMSApp.Class.AppVision;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Test
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestVisionView : ContentPage
    {
        public TestVisionView()
        {
            InitializeComponent();
        }

        async void Button_Clicked(object sender, EventArgs e)
        {
            try
            {
                var addrs = "ExeOCRText";
                MessagingCenter.Subscribe(this, addrs, (string textCapture) =>
                {
                    MessagingCenter.Unsubscribe<string>(this, addrs);
                    if (string.IsNullOrWhiteSpace(textCapture)) return;
                    if (textCapture.Length == 0) return;
                    if (textCapture.Equals("cancel")) return;
                    if (textCapture.Equals("empty")) return;

                    DisplayAlert("Catured Text", $"{textCapture}", "OK");
                });

                await new AppVision(addrs).ExecuteOCR();
            }
            catch (Exception exp)
            {
                await DisplayAlert("Alert", exp.ToString(), "OK");
            }
        }
    }
}