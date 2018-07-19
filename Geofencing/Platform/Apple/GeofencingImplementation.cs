using CoreLocation;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UIKit;

namespace Plugin.Geofencing
{
    public class GeofencingImplementation : IGeofencing
    {
        readonly CLLocationManager locationManager;

        public GeofencingImplementation()
        {
            locationManager = new CLLocationManager();
            locationManager.RegionEntered += (sender, args) => TryFireEvent(args, GeofenceStatus.Entered);
            locationManager.RegionLeft += (sender, args) => TryFireEvent(args, GeofenceStatus.Exited);
        }

        public GeofenceManagerStatus Status
        {
            get
            {
                if (!CLLocationManager.LocationServicesEnabled)
                    return GeofenceManagerStatus.Disabled;

                if (!CLLocationManager.IsMonitoringAvailable(typeof(CLCircularRegion)))
                    return GeofenceManagerStatus.Disabled;

                if (CLLocationManager.Status != CLAuthorizationStatus.AuthorizedAlways)
                    return GeofenceManagerStatus.PermissionDenied;

                return GeofenceManagerStatus.Ready;
            }
        }

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
        
        public async Task<GeofenceStatus> RequestState(GeofenceRegion region, CancellationToken? cancelToken)
        {
            var tcs = new TaskCompletionSource<GeofenceStatus>();
            cancelToken?.Register(() => tcs.TrySetCanceled());

            var handler = new EventHandler<CLRegionStateDeterminedEventArgs>((sender, args) =>
            {
                var clregion = args.Region as CLCircularRegion;
                if (clregion?.Identifier.Equals(region.Identifier) ?? false)
                {
                    var state = FromNative(args.State);
                    tcs.TrySetResult(state);
                }
            });

            try
            {
                locationManager.DidDetermineState += handler;
                var native = ToNative(region);
                locationManager.RequestState(native);
                return await tcs.Task;
            }
            finally
            {
                locationManager.DidDetermineState -= handler;
            }
        }

        public event EventHandler<GeofenceStatusChangedEventArgs> RegionStatusChanged;
        
        public IReadOnlyList<GeofenceRegion> MonitoredRegions
        {
            get
            {
                var list = locationManager
                    .MonitoredRegions
                    .Select(x => x as CLCircularRegion)
                    .Where(x => x != null)
                    .Select(FromNative)
                    .ToList();

                return new ReadOnlyCollection<GeofenceRegion>(list);
            }
        }

        public void StartMonitoring(GeofenceRegion region)
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                var native = this.ToNative(region);
                locationManager.StartMonitoring(native);
            });
        }

        public void StopMonitoring(GeofenceRegion region)
        {
            var native = ToNative(region);
            locationManager.StopMonitoring(native);
        }

        public void StopAllMonitoring()
        {
            var natives = locationManager
                .MonitoredRegions
                .Select(x => x as CLCircularRegion)
                .Where(x => x != null)
                .ToList();

            foreach (var native in natives)
                locationManager.StopMonitoring(native);
        }

        protected virtual void TryFireEvent(CLRegionEventArgs args, GeofenceStatus status)
        {
            if (!(args.Region is CLCircularRegion native))
                return;

            var region = FromNative(native);
            RegionStatusChanged?.Invoke(this, new GeofenceStatusChangedEventArgs(region, status));
        }
        
        protected virtual GeofenceRegion FromNative(CLCircularRegion native)
        {
            var radius = Distance.FromMeters(native.Radius);
            var center = FromNative(native.Center);
            return new GeofenceRegion(native.Identifier, center, radius);
        }
        
        protected virtual GeofenceStatus FromNative(CLRegionState state)
        {
            switch (state)
            {
                case CLRegionState.Inside:
                    return GeofenceStatus.Entered;

                case CLRegionState.Outside:
                    return GeofenceStatus.Exited;

                case CLRegionState.Unknown:
                default:
                    return GeofenceStatus.Unknown;
            }
        }

        protected virtual Position FromNative(CLLocationCoordinate2D native)
            => new Position(native.Latitude, native.Longitude);
        
        protected virtual CLLocationCoordinate2D ToNative(Position position)
            => new CLLocationCoordinate2D(position.Latitude, position.Longitude);
        
        protected virtual CLCircularRegion ToNative(GeofenceRegion region)
        {
            return new CLCircularRegion(
                ToNative(region.Center),
                region.Radius.TotalMeters,
                region.Identifier
            )
            {
                NotifyOnExit = true,
                NotifyOnEntry = true
            };
        }
    }
}
