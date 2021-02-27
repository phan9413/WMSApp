//using System;
//using Android.App;
//using Android.Content;
//using Android.Graphics;
//using Android.OS;
//using Android.Support.V4.App;
//using WMSApp.Interface;
//using AndroidApp = Android.App.Application;

using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using AndroidApp = Android.App.Application;
using WMSApp.Interface;

[assembly: Xamarin.Forms.Dependency(typeof(WMSApp.Droid.NotificationManagerDroid))]
namespace WMSApp.Droid
{
    public class NotificationManagerDroid : INotificationManager
    {

        const string channelId = "default";
        const string channelName = "Default";
        const string channelDescription = "The default channel for notifications.";
        public const string TitleKey = "title";
        public const string MessageKey = "message";
        public const string NextViewKey = "NextViewKey"; // show to next view link, null retain the same screen
        public const string DataKey = "DataKey"; // show to next view link, null retain the same screen

        bool channelInitialized = false;
        int messageId = 0;
        int pendingIntentId = 0;

        NotificationManager manager;

        public event EventHandler NotificationReceived;

        public static NotificationManagerDroid Instance { get; private set; }

        public void Initialize()
        {
            CreateNotificationChannel();
            Instance = this;
        }

        public void SendNotification(string title, string message, string nextView, string data, DateTime? notifyTime = null)
        {
            if (!channelInitialized)
            {
                CreateNotificationChannel();
            }

            if (notifyTime != null)
            {
                Intent intent = new Intent(AndroidApp.Context, typeof(AlarmHandler));
                intent.PutExtra(TitleKey, title);
                intent.PutExtra(MessageKey, message);
                intent.PutExtra(NextViewKey, nextView);
                intent.PutExtra(DataKey, data);

                PendingIntent pendingIntent = PendingIntent.GetBroadcast(AndroidApp.Context, pendingIntentId++, intent, PendingIntentFlags.OneShot);
                long triggerTime = GetNotifyTime(notifyTime.Value);
                AlarmManager alarmManager = AndroidApp.Context.GetSystemService(Context.AlarmService) as AlarmManager;
                alarmManager.Set(AlarmType.RtcWakeup, triggerTime, pendingIntent);
                return;
            }

            Show(title, message, nextView, data);
        }

        public void ReceiveNotification(string title, string message)
        {
            var args = new NotificationEventArgs()
            {
                Title = title,
                Message = message,
            };
            NotificationReceived?.Invoke(null, args);
        }

        public void Show(string title, string message, string nextView, string data)
        {
            Intent intent = new Intent(AndroidApp.Context, typeof(MainActivity));
            intent.PutExtra(TitleKey, title);
            intent.PutExtra(MessageKey, message);
            intent.PutExtra(NextViewKey, nextView);
            intent.PutExtra(DataKey, data);

            PendingIntent pendingIntent = PendingIntent.GetActivity(AndroidApp.Context, pendingIntentId++, intent, PendingIntentFlags.UpdateCurrent);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(AndroidApp.Context, channelId)
                .SetContentIntent(pendingIntent)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetLargeIcon(BitmapFactory.DecodeResource(AndroidApp.Context.Resources, Resource.Drawable.whs512))
                .SetSmallIcon(Resource.Drawable.whs512)
                .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate);

            Notification notification = builder.Build();
            manager.Notify(messageId++, notification);
        }

        void CreateNotificationChannel()
        {
            manager = (NotificationManager)AndroidApp.Context.GetSystemService(AndroidApp.NotificationService);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelNameJava = new Java.Lang.String(channelName);
                var channel = new NotificationChannel(channelId, channelNameJava, NotificationImportance.Default)
                {
                    Description = channelDescription
                };
                manager.CreateNotificationChannel(channel);
            }

            channelInitialized = true;
        }

        long GetNotifyTime(DateTime notifyTime)
        {
            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(notifyTime);
            double epochDiff = (new DateTime(1970, 1, 1) - DateTime.MinValue).TotalSeconds;
            long utcAlarmTime = utcTime.AddSeconds(-epochDiff).Ticks / 10000;
            return utcAlarmTime; // milliseconds
        }

        public int ScheduleNotification(string title, string message)
        {
            //throw new NotImplementedException();
            return -1;
        }

        public void ReceiveNotification(string title, string message, string nextView)
        {

        }
    }
}

//const string channelId = "default";
//const string channelName = "Default";
//const string channelDescription = "The default channel for notifications.";
//const int pendingIntentId = 0;

//public const string TitleKey = "title";
//public const string MessageKey = "message";

//bool channelInitialized = false;
//int messageId = -1;
//NotificationManager manager;

//public event EventHandler NotificationReceived;

//public void Initialize() => CreateNotificationChannel();


//public int ScheduleNotification(string title, string message)
//{
//    if (!channelInitialized)
//    {
//        CreateNotificationChannel();
//    }

//    messageId++;

//    Intent intent = new Intent(AndroidApp.Context, typeof(MainActivity));
//    intent.PutExtra(TitleKey, title);
//    intent.PutExtra(MessageKey, message);

//    PendingIntent pendingIntent = PendingIntent.GetActivity(AndroidApp.Context, pendingIntentId, intent, PendingIntentFlags.OneShot);

//    NotificationCompat.Builder builder = new NotificationCompat.Builder(AndroidApp.Context, channelId)
//        .SetContentIntent(pendingIntent)
//        .SetContentTitle(title)
//        .SetContentText(message)
//        .SetLargeIcon(BitmapFactory.DecodeResource(AndroidApp.Context.Resources, Resource.Drawable.whs512))
//        .SetSmallIcon(Resource.Drawable.file)
//        .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate);

//    var notification = builder.Build();
//    manager.Notify(messageId, notification);

//    return messageId;
//}

//public void ReceiveNotification(string title, string message)
//{
//    var args = new NotificationEventArgs()
//    {
//        Title = title,
//        Message = message,
//    };


//    NotificationReceived?.Invoke(null, args);
//}

//void CreateNotificationChannel()
//{
//    //manager = (NotificationManager)Context.GetSystemService(AndroidApp.NotificationService);
//    manager = (NotificationManager)AndroidApp.Context.GetSystemService(AndroidApp.NotificationService);

//    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
//    {
//        var channelNameJava = new Java.Lang.String(channelName);
//        var channel = new NotificationChannel(channelId, channelNameJava, NotificationImportance.Default)
//        {
//            Description = channelDescription
//        };
//        manager.CreateNotificationChannel(channel);
//    }

//    channelInitialized = true;
//}