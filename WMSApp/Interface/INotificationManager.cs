using System;
namespace WMSApp.Interface
{
    public interface INotificationManager
    {
        event EventHandler NotificationReceived;
        void Initialize();
        int ScheduleNotification(string title, string message);
        void ReceiveNotification(string title, string message, string nextView);
        void SendNotification(string title, string message, string nextView, string data, DateTime? notifyTime = null);
    }
}
