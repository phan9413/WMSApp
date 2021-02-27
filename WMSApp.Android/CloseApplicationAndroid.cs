using Android.App;
using Android.Content;
using Java.Lang;
using WMSApp.Interface;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using AndroidApp = Android.App.Application;

[assembly: Xamarin.Forms.Dependency(typeof(WMSApp.Droid.CloseApplicationAndroid))]
namespace WMSApp.Droid
{
    class CloseApplicationAndroid : ICloseApplication
    {        
        public void closeApplication()
        {
            //var activity = (Activity)Form.Context;
            //activity.FinishAffinity();

            JavaSystem.Exit(0);
        }
    }
}