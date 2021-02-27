using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Acr.UserDialogs;
using Android.Content;
using Xamarin.Forms;
using WMSApp.Interface;
//using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;

namespace WMSApp.Droid
{
    [Activity(Label = "IMApp",
        Icon = "@mipmap/whs",
        Theme = "@style/MainTheme",
        LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        ScreenOrientation = ScreenOrientation.Portrait)]

    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {

        static readonly int NOTIFICATION_ID = 1000;
        static readonly string CHANNEL_ID = "location_notification";
        internal static readonly string COUNT_KEY = "count";

        static Xamarin.Forms.Application _App;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            // for Rg plugin pop up
            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);

            // for zxing .net 
            ZXing.Net.Mobile.Forms.Android.Platform.Init();
            UserDialogs.Init(this);

            CreateNotificationFromIntent(Intent);
            _App = new App();
            LoadApplication(_App);
        }

        // for rg plug in 
        // handl on back pressed
        public override void OnBackPressed()
        {
            if (Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed))
            {
                // Do something if there are some pages in the `PopupStack`
            }
            else
            {
                // Do something if there are not any pages in the `PopupStack`
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        // for local notification
        protected override void OnNewIntent(Intent intent)
        {
            CreateNotificationFromIntent(intent);
        }

        // for local notification
        void CreateNotificationFromIntent(Intent intent)
        {
            //if (intent?.Extras != null)
            //{
            //    //string title = intent.Extras.GetString(NotificationManagerDroid.TitleKey);
            //    //string message = intent.Extras.GetString(NotificationManagerDroid.MessageKey);
            //    //string nextView = intent.Extras.GetString(NotificationManagerDroid.NextViewKey);

            //    //DependencyService.Get<INotificationManager>().ReceiveNotification(title, message, nextView);

            //    if (intent.Extras == null) return;

            //    string nextView = intent.Extras.GetString(NotificationManagerDroid.NextViewKey);
            //    string data = intent.Extras.GetString(NotificationManagerDroid.DataKey);

            //    if (!string.IsNullOrWhiteSpace(nextView))
            //    {
            //        MessagingCenter.Send<string>(data, nextView);
            //        //LoadApplication(new App(nextView, data));                    
            //        return;
            //    }
            //}
        }


        //void CreateNotificationChannel()
        //{
        //    if (Build.VERSION.SdkInt < BuildVersionCodes.O)
        //    {
        //        // Notification channels are new in API 26 (and not a part of the
        //        // support library). There is no need to create a notification
        //        // channel on older versions of Android.
        //        return;
        //    }

        //    var name = Resources.GetString(Resource.String.channel_name);
        //    var description = GetString(Resource.String.channel_description);
        //    var channel = new NotificationChannel(CHANNEL_ID, name, NotificationImportance.Default)
        //    {
        //        Description = description
        //    };

        //    var notificationManager = (NotificationManager)GetSystemService(NotificationService);
        //    notificationManager.CreateNotificationChannel(channel);
        //}
    }
}