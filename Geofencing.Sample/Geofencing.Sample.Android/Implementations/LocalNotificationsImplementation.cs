using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Geofencing.Sample.Droid.Implementations;
using Geofencing.Sample.Interfaces;
using System;
using System.IO;
using System.Xml.Serialization;
using Xamarin.Forms;

[assembly: Dependency(typeof(LocalNotificationsImplementation))]
namespace Geofencing.Sample.Droid.Implementations
{
    public class LocalNotificationsImplementation : ILocalNotifications
    {
        /// <summary>
        /// Get or Set Resource Icon to display
        /// </summary>
        public static int NotificationIconId { get; set; }
        public static long DefaultVibration { get; set; } = 300L;
        public static string NotificationChannelId { get; set; } = "Geofencing.Sample.Android.Implementations.Channel.Id";
        public static string NotificationChannelName { get; set; } = "Geofencing Notifications";
        static bool _channelCreated;

        /// <summary>
        /// Show a local notification
        /// </summary>
        /// <param name="title">Title of the notification</param>
        /// <param name="body">Body or description of the notification</param>
        /// <param name="id">Id of the notification</param>
        public void Show(string title, string body, int id)
        {
            var resultIntent = GetLauncherActivity();
            PendingIntent pendingIntent = null;
            if (resultIntent != null)
            {
                resultIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                pendingIntent = PendingIntent.GetActivity(Android.App.Application.Context, 0, resultIntent, 0);
            }

            var notificationSound = Android.Media.RingtoneManager.GetDefaultUri(Android.Media.RingtoneType.Notification);
            var style = new NotificationCompat.BigTextStyle();
            style.BigText(body);

            var builder = new NotificationCompat.Builder(Android.App.Application.Context, NotificationChannelId)
                .SetContentTitle(title)
                .SetAutoCancel(true)
                .SetContentText(body)
                .SetStyle(style)
                .SetWhen(Java.Lang.JavaSystem.CurrentTimeMillis())
                .SetGroup("push_messages")
                .SetSmallIcon(Resource.Mipmap.ic_launcher);

            if (pendingIntent != null)
                builder.SetContentIntent(pendingIntent);

            builder.SetSound(notificationSound);

            var notification = builder.Build();
            try
            {
                notification.LedARGB = Android.Graphics.Color.Argb(1, 29, 113, 184);
                notification.Flags |= NotificationFlags.ShowLights;
                notification.Flags |= NotificationFlags.HighPriority;
                notification.LedOnMS = 1000;
                notification.LedOffMS = 2000;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }

            NotificationManager notificationManager = Android.App.Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O && !_channelCreated)
            {
                var channel = new NotificationChannel(NotificationChannelId, NotificationChannelName, NotificationImportance.High);
                channel.EnableLights(true);
                channel.EnableVibration(false);
                notificationManager.CreateNotificationChannel(channel);
                _channelCreated = true;
            }
            notificationManager.Notify(id, notification);
        }


        public static Intent GetLauncherActivity()
        {
            try
            {
                var packageName = Android.App.Application.Context.PackageName;
                return Android.App.Application.Context.PackageManager.GetLaunchIntentForPackage(packageName);
            }
            catch { return null; }
        }

        /// <summary>
        /// Show a local notification at a specified time
        /// </summary>
        /// <param name="title">Title of the notification</param>
        /// <param name="body">Body or description of the notification</param>
        /// <param name="id">Id of the notification</param>
        /// <param name="notifyTime">Time to show notification</param>
        public void Show(string title, string body, DateTime notifyTime, int id = 0)
        {
            var intent = CreateIntent(id);

            var localNotification = new LocalNotification
            {
                Title = title,
                Body = body,
                Id = id,
                NotifyTime = notifyTime
            };

            if (NotificationIconId != 0)
            {
                localNotification.IconId = NotificationIconId;
            }
            else
            {
                localNotification.IconId = Resource.Mipmap.ic_launcher;
            }

            var serializedNotification = SerializeNotification(localNotification);
            intent.PutExtra(ScheduledAlarmHandler.LocalNotificationKey, serializedNotification);

            var pendingIntent = PendingIntent.GetBroadcast(Android.App.Application.Context, 0, intent, PendingIntentFlags.CancelCurrent);
            var triggerTime = NotifyTimeInMilliseconds(localNotification.NotifyTime);
            var alarmManager = GetAlarmManager();

            alarmManager.Set(AlarmType.RtcWakeup, triggerTime, pendingIntent);
        }

        /// <summary>
        /// Cancel a local notification
        /// </summary>
        /// <param name="id">Id of the notification to cancel</param>
        public void Cancel(int id)
        {
            var intent = CreateIntent(id);
            var pendingIntent = PendingIntent.GetBroadcast(Android.App.Application.Context, 0, intent, PendingIntentFlags.CancelCurrent);

            var alarmManager = GetAlarmManager();
            alarmManager.Cancel(pendingIntent);

            var notificationManager = NotificationManagerCompat.From(Android.App.Application.Context);
            notificationManager.Cancel(id);
        }

        private Intent CreateIntent(int id)
        {
            return new Intent(Android.App.Application.Context, typeof(ScheduledAlarmHandler))
                .SetAction("LocalNotifierIntent" + id);
        }


        private AlarmManager GetAlarmManager()
        {
            var alarmManager = Android.App.Application.Context.GetSystemService(Context.AlarmService) as AlarmManager;
            return alarmManager;
        }

        private string SerializeNotification(LocalNotification notification)
        {
            var xmlSerializer = new XmlSerializer(notification.GetType());
            using (var stringWriter = new StringWriter())
            {
                xmlSerializer.Serialize(stringWriter, notification);
                return stringWriter.ToString();
            }
        }

        private long NotifyTimeInMilliseconds(DateTime notifyTime)
        {
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(notifyTime);
            var epochDifference = (new DateTime(1970, 1, 1) - DateTime.MinValue).TotalSeconds;

            var utcAlarmTimeInMillis = utcTime.AddSeconds(-epochDifference).Ticks / 10000;
            return utcAlarmTimeInMillis;
        }
    }

    public class LocalNotification
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public int Id { get; set; }
        public int IconId { get; set; }
        public DateTime NotifyTime { get; set; }
    }

    /// <summary>
    /// Broadcast receiver
    /// </summary>
    [BroadcastReceiver(Enabled = true, Label = "Local Geofencing Notifications Broadcast Receiver")]
    public class ScheduledAlarmHandler : BroadcastReceiver
    {
        public const string LocalNotificationKey = "LocalNotification";
        public override void OnReceive(Context context, Intent intent)
        {
            var extra = intent.GetStringExtra(LocalNotificationKey);
            var notification = DeserializeNotification(extra);

            var builder = new NotificationCompat.Builder(Android.App.Application.Context)
                .SetContentTitle(notification.Title)
                .SetContentText(notification.Body)
                .SetSmallIcon(notification.IconId)
                .SetAutoCancel(true);

            var resultIntent = LocalNotificationsImplementation.GetLauncherActivity();
            resultIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
            var stackBuilder = Android.Support.V4.App.TaskStackBuilder.Create(Android.App.Application.Context);
            stackBuilder.AddNextIntent(resultIntent);
            var resultPendingIntent =
                stackBuilder.GetPendingIntent(0, (int)PendingIntentFlags.UpdateCurrent);
            builder.SetContentIntent(resultPendingIntent);

            var notificationManager = NotificationManagerCompat.From(Android.App.Application.Context);
            notificationManager.Notify(notification.Id, builder.Build());
        }

        private LocalNotification DeserializeNotification(string notificationString)
        {
            var xmlSerializer = new XmlSerializer(typeof(LocalNotification));
            using (var stringReader = new StringReader(notificationString))
            {
                var notification = (LocalNotification)xmlSerializer.Deserialize(stringReader);
                return notification;
            }
        }
    }
}
