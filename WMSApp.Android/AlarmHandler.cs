using Android.Content;

namespace WMSApp.Droid
{
    [BroadcastReceiver(Enabled = true, Label = "Local Notifications Broadcast Receiver")]
    public class AlarmHandler : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent?.Extras != null)
            {
                string title = intent.GetStringExtra(NotificationManagerDroid.TitleKey);
                string message = intent.GetStringExtra(NotificationManagerDroid.MessageKey);
                string nextView = intent.GetStringExtra(NotificationManagerDroid.NextViewKey);
                string data = intent.GetStringExtra(NotificationManagerDroid.DataKey);

                NotificationManagerDroid.Instance.Show(title, message, nextView, data);
            }
        }
    }
}