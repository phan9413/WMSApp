using Android.App;
using Android.Widget;
using WMSApp.Droid;
using WMSApp.Interface;

[assembly: Xamarin.Forms.Dependency(typeof(ToastMessageDroid))]
namespace WMSApp.Droid
{    
    class ToastMessageDroid : IToastMessage
    {
        public void LongAlert(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Long).Show();
        }

        public void ShortAlert(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Short).Show();
        }
    }
}