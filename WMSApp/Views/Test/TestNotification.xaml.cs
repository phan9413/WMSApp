using System;
using WMSApp.Interface;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace WMSApp.Views.Test
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestNotification : ContentPage
    {
        INotificationManager notificationManager;
        int notificationNumber = 0;

        public TestNotification()
        {
            InitializeComponent();
            notificationManager = DependencyService.Get<INotificationManager>();
            notificationManager.NotificationReceived += (sender, eventArgs) =>
            {
                var evtData = (NotificationEventArgs)eventArgs;
                ShowNotification(evtData.Title, evtData.Message);
            };

          
        }

        void OnScheduleClick(object sender, EventArgs e)
        {
            notificationNumber++;
            string title = $"Local Notification #{notificationNumber}";
            string message = $"You have now received {notificationNumber} notifications!";
            notificationManager?.ScheduleNotification(title, message);
        }

        void ShowNotification(string title, string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var msg = new Label()
                {
                    Text = $"Notification Received:\nTitle: {title}\nMessage: {message}"
                };
                stackLayout.Children.Add(msg);
            });
        }

        void SendNotification (string title, string message, string nextView)
        {
            //notificationManager?.SendNotification(title, message, nextView);
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            SendNotification("Johnny", "u had a message", "LoginView");
        }
    }
}