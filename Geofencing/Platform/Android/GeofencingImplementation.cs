using Android.App;
using Android.Content;
using Android.Gms.Location;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

[assembly: UsesPermission(Android.Manifest.Permission.AccessFineLocation)]
[assembly: UsesPermission(Android.Manifest.Permission.WakeLock)]
[assembly: UsesFeature("android.hardware.location.gps")]
[assembly: UsesFeature("android.hardware.location.network")]
namespace Plugin.Geofencing
{
    /// <summary>
    /// Interface for $safeprojectgroupname$
    /// </summary>
    public class GeofencingImplementation : IGeofencing
    {
        readonly GeofencingClient client;
        readonly IList<GeofenceRegion> regions;
        readonly object syncLock;
        PendingIntent geofencePendingIntent;

        public GeofencingImplementation()
        {
            syncLock = new object();
            client = LocationServices.GetGeofencingClient(Application.Context);
            regions = MarcelloDatabase.Current.GetAll<GeofenceRegion>().ToList();
        }
        public GeofenceManagerStatus Status => GeofenceManagerStatus.Ready;
        
        public async Task<PermissionStatus> RequestPermission()
        {
            var result = await CrossPermissions
                .Current
                .RequestPermissionsAsync(Permission.LocationAlways)
                .ConfigureAwait(false);

            if (!result.ContainsKey(Permission.LocationAlways))
                return PermissionStatus.Unknown;

            return result[Permission.LocationAlways];
        }

        public IReadOnlyList<GeofenceRegion> MonitoredRegions
        {
            get
            {
                lock (syncLock)
                    return regions.ToList();
            }
        }

        public async void StartMonitoring(GeofenceRegion region)
        {
            var geofence = new GeofenceBuilder()
                .SetRequestId(region.Identifier)
                .SetExpirationDuration(Geofence.NeverExpire)
                .SetCircularRegion(
                    region.Center.Latitude,
                    region.Center.Longitude,
                    Convert.ToSingle(region.Radius.TotalMeters)
                )
                .SetTransitionTypes(
                    Geofence.GeofenceTransitionEnter |
                    Geofence.GeofenceTransitionExit
                )
                .Build();

            var request = new GeofencingRequest.Builder()
                .AddGeofence(geofence)
                .SetInitialTrigger(GeofencingRequest.InitialTriggerEnter | GeofencingRequest.InitialTriggerExit)
                .Build();

            await client.AddGeofencesAsync(request, GetPendingIntent());

            lock (syncLock)
            {
                regions.Add(region);
                MarcelloDatabase.Current.Save(region);

                //if (regions.Count == 1)
                //    Application.Context.StartForegroundService(new Intent(Application.Context, typeof(GeofenceBroadcastReceiver)));
            }
        }

        public void StopMonitoring(GeofenceRegion region)
        {
            lock (syncLock)
            {
                client.RemoveGeofences(new List<string> { region.Identifier });
                if (regions.Remove(region))
                    MarcelloDatabase.Current.Delete(region);

                //if (regions.Count == 0)
                //    Application.Context.StopService(new Intent(Application.Context, typeof(GeofenceBroadcastReceiver)));
            }
        }

        public void StopAllMonitoring()
        {
            if (regions.Count == 0)
                return;

            lock (syncLock)
            {
                var ids = regions.Select(x => x.Identifier).ToList();
                client.RemoveGeofences(ids);
                regions.Clear();

                //if (regions.Count == 0)
                //    Application.Context.StopService(new Intent(Application.Context, typeof(GeofenceBroadcastReceiver)));
            }
        }

        public Task<GeofenceStatus> RequestState(GeofenceRegion region, CancellationToken? cancelToken = null)
        {
            var foundRegion = regions.FirstOrDefault(x => x.Identifier.Equals(region.Identifier));
            if (foundRegion == null)
            {
                System.Diagnostics.Debug.WriteLine($"Triggered geofence does not exist in the list of watching geofences");
                return Task.FromResult(GeofenceStatus.Unknown);
            }
            return Task.FromResult(foundRegion.LastKnownGeofenceStatus);
        }

        public event EventHandler<GeofenceStatusChangedEventArgs> RegionStatusChanged;
        
        protected virtual void OnRegionStatusChanged(GeofenceRegion region, GeofenceStatus status)
            => RegionStatusChanged?.Invoke(this, new GeofenceStatusChangedEventArgs(region, status));


        protected virtual PendingIntent GetPendingIntent()
        {
            if (geofencePendingIntent != null)
                return geofencePendingIntent;

            var intent = new Intent(Application.Context, typeof(GeofenceBroadcastReceiver));
            geofencePendingIntent = PendingIntent.GetBroadcast(Application.Context, 0, intent, PendingIntentFlags.UpdateCurrent);

            return geofencePendingIntent;
        }

        internal void TryFireEvent(GeofencingEvent e)
        {
            lock (syncLock)
            {
                try
                {
                    var status = e.GeofenceTransition == Geofence.GeofenceTransitionEnter
                        ? GeofenceStatus.Entered
                        : GeofenceStatus.Exited;

                    if (e.TriggeringGeofences == null)
                        return;

                    foreach (var native in e.TriggeringGeofences)
                    {
                        var region = regions.FirstOrDefault(x => x.Identifier.Equals(native.RequestId));
                        if (region == null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Triggered geofence does not exist in the list of watching geofences");
                            return;
                        }
                        region.LastKnownGeofenceStatus = status;
                        MarcelloDatabase.Current.Save(region);
                        OnRegionStatusChanged(region, status);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in TryFireEvent: {ex.Message}");
                }
            }
        }
    }
}
