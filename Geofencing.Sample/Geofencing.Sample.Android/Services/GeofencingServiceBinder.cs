using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using System;

namespace Geofencing.Sample.Droid.Services
{
    public class GeofencingServiceBinder : Binder
    {
        public GeofencingServiceBinder(GeofencingService service)
        {
            Service = service;
        }

        public GeofencingService Service { get; }

        public bool IsBound { get; set; }
    }

    [Service]
    public class GeofencingService : Service
    {
        IBinder binder;
        public Plugin.Geofencing.IGeofencing GeofencingInstance { get; private set; }
        public GeofencingHelper GeofencingHelperInstance { get; private set; }
        public static string NotificationChannelId { get; set; } = "geofencing-notification-channel-id";
        public static string NotificationChannelName { get; set; } = "Geofencing";
        static bool _channelCreated;

        public override IBinder OnBind(Intent intent)
        {
            binder = new GeofencingServiceBinder(this);
            return binder;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var builder = new NotificationCompat.Builder(this, NotificationChannelId);

            var newIntent = new Intent(this, typeof(MainActivity));
            newIntent.PutExtra("tracking", true);
            newIntent.AddFlags(ActivityFlags.ClearTop);
            newIntent.AddFlags(ActivityFlags.SingleTop);

            var pendingIntent = PendingIntent.GetActivity(this, 0, newIntent, 0);
            var notification = builder.SetContentIntent(pendingIntent)
                .SetSmallIcon(Resource.Mipmap.ic_launcher)
                .SetAutoCancel(false)
                .SetTicker("Geofencing active")
                .SetContentTitle("Geofencing")
                .SetContentText("Geofencing active")
                .Build();

            NotificationManager notificationManager = GetSystemService(NotificationService) as NotificationManager;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O || !_channelCreated)
            {
                var channel = new NotificationChannel(NotificationChannelId, NotificationChannelName, NotificationImportance.None);
                channel.EnableLights(true);
                channel.EnableVibration(true);
                notificationManager.CreateNotificationChannel(channel);
                _channelCreated = true;
            }

            StartForeground((int)NotificationFlags.ForegroundService, notification);

            GeofencingInstance = Plugin.Geofencing.CrossGeofencing.Current;
            GeofencingHelperInstance = GeofencingHelper.Current;
            return StartCommandResult.Sticky;
        }

        public void StartGeofencingUpdates()
        {
            GeofencingHelper.StartGeofencingService();
        }

        public void StopLocationUpdates()
        {
            GeofencingHelper.StopGeofencingService();
        }
    }

    public class GeofencingServiceConnection : Java.Lang.Object, IServiceConnection
    {
        public GeofencingServiceConnection(GeofencingServiceBinder binder)
        {
            if (binder != null)
                Binder = binder;
        }

        public GeofencingServiceBinder Binder { get; set; }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            var serviceBinder = service as GeofencingServiceBinder;

            if (serviceBinder == null)
                return;


            Binder = serviceBinder;
            Binder.IsBound = true;

            // raise the service bound event
            ServiceConnected?.Invoke(this, new ServiceConnectedEventArgs { Binder = service });

            // begin updating the location in the Service
            serviceBinder.Service.StartGeofencingUpdates();
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            Binder.IsBound = false;
        }

        public event EventHandler<ServiceConnectedEventArgs> ServiceConnected;
    }

    public class ServiceConnectedEventArgs : EventArgs
    {
        public IBinder Binder { get; set; }
    }
}